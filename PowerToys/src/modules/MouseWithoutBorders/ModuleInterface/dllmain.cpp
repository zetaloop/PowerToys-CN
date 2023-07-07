#include "pch.h"
#include <interface/powertoy_module_interface.h>
#include <common/SettingsAPI/settings_objects.h>
#include <common/interop/shared_constants.h>
#include "trace.h"
#include "generateSecurityDescriptor.h"

#include <common/logger/logger.h>
#include <common/SettingsAPI/settings_helpers.h>

#include <common/utils/process_path.h>
#include <common/utils/resources.h>
#include <common/utils/winapi_error.h>
#include <common/utils/processApi.h>
#include <common/utils/elevation.h>

HINSTANCE g_hInst_MouseWithoutBorders = 0;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID /*lpReserved*/)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        g_hInst_MouseWithoutBorders = hModule;
        Trace::RegisterProvider();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        Trace::UnregisterProvider();
        break;
    }
    return TRUE;
}

bool GetUserSid(const wchar_t* username, PSID& sid)
{
    DWORD sidSize = 0;
    DWORD domainNameSize = 0;
    SID_NAME_USE sidNameUse;

    LookupAccountName(nullptr, username, nullptr, &sidSize, nullptr, &domainNameSize, &sidNameUse);
    if (GetLastError() != ERROR_INSUFFICIENT_BUFFER)
    {
        Logger::error("Failed to get buffer sizes");
        return false;
    }

    sid = LocalAlloc(LPTR, sidSize);
    LPWSTR domainName = static_cast<LPWSTR>(LocalAlloc(LPTR, domainNameSize * sizeof(wchar_t)));

    if (!LookupAccountNameW(nullptr, username, sid, &sidSize, domainName, &domainNameSize, &sidNameUse))
    {
        Logger::error("Failed to lookup account name");
        LocalFree(sid);
        LocalFree(domainName);
        return false;
    }

    LocalFree(domainName);
    return true;
}

std::wstring GetCurrentUserSid()
{
    wchar_t username[UNLEN + 1];
    DWORD usernameSize = UNLEN + 1;

    std::wstring result;
    if (!GetUserNameW(username, &usernameSize))
    {
        Logger::error("Failed to get the current user name");
        return result;
    }

    PSID sid;
    if (GetUserSid(username, sid))
    {
        LPWSTR sidString;
        if (ConvertSidToStringSid(sid, &sidString))
        {
            result = sidString;
            LocalFree(sidString);
        }
        LocalFree(sid);
    }
    else
    {
        Logger::error(L"Failed to get SID for user \"");
    }

    return result;
}

std::wstring escapeDoubleQuotes(const std::wstring& input)
{
    std::wstring output;
    output.reserve(input.size());

    for (const wchar_t& ch : input)
    {
        if (ch == L'"')
        {
            output += L'\\';
        }
        output += ch;
    }

    return output;
}

const static wchar_t* MODULE_NAME = L"MouseWithoutBorders";
const static wchar_t* MODULE_DESC = L"A module to move your mouse across computers.";
const static wchar_t* SERVICE_NAME = L"PowerToys.MWB.Service";
const static std::wstring_view USE_SERVICE_PROPERTY_NAME = L"UseService";

class MouseWithoutBorders : public PowertoyModuleIface
{
    std::wstring app_name;
    std::wstring app_key;

private:
    bool m_enabled = false;
    bool run_in_service_mode = false;
    HANDLE send_telemetry_event;
    HANDLE m_hInvokeEvent;
    PROCESS_INFORMATION p_info;

    bool is_enabled_by_default() const override
    {
        return false;
    }

    bool is_process_running()
    {
        return WaitForSingleObject(p_info.hProcess, 0) == WAIT_TIMEOUT;
    }

    void launch_process()
    {
        Logger::trace(L"Launching PowerToys MouseWithoutBorders process");
        const std::wstring application_path = L"modules\\MouseWithoutBorders\\PowerToys.MouseWithoutBorders.exe";
        STARTUPINFO info = { sizeof(info) };
        std::wstring full_command_path = application_path;
        if (run_in_service_mode)
        {
            full_command_path += L" ";
            full_command_path += USE_SERVICE_PROPERTY_NAME;
        }

        if (!CreateProcessW(application_path.c_str(), full_command_path.data(), nullptr, nullptr, true, {}, nullptr, nullptr, &info, &p_info))
        {
            DWORD error = GetLastError();
            std::wstring message = L"PowerToys MouseWithoutBorders failed to start with error: ";
            message += std::to_wstring(error);
            Logger::error(message);
        }

        Trace::MouseWithoutBorders::Activate();
    }

    void unregister_service()
    {
        SC_HANDLE schSCManager = OpenSCManagerW(nullptr, SERVICES_ACTIVE_DATABASE, SC_MANAGER_ALL_ACCESS);

        SC_HANDLE hService = OpenServiceW(schSCManager, SERVICE_NAME, SERVICE_STOP | DELETE);
        if (!hService)
        {
            Logger::error("Failed to open MWB service");
            return;
        }

        SERVICE_STATUS ss;
        if (ControlService(hService, SERVICE_CONTROL_STOP, &ss))
        {
            Sleep(1000);
            for (int i = 0; i < 5; ++i)
            {
                while (QueryServiceStatus(hService, &ss))
                {
                    if (ss.dwCurrentState == SERVICE_STOP_PENDING)
                    {
                        Sleep(1000);
                    }
                    else
                    {
                        goto outer;
                    }
                }
            }
        }

    outer:
        BOOL deleteResult = DeleteService(hService);
        CloseServiceHandle(hService);

        if (!deleteResult)
        {
            Logger::error("Failed to delete MWB service");
            return;
        }
    
        Trace::MouseWithoutBorders::ToggleServiceRegistration(false);
    }

    void
    register_service()
    {
        SC_HANDLE schSCManager = OpenSCManagerW(nullptr, SERVICES_ACTIVE_DATABASE, SC_MANAGER_ALL_ACCESS);
        if (schSCManager == nullptr)
        {
            Logger::error(L"Couldn't open sc manager");
            return;
        }

        const auto closeSCM = wil::scope_exit([&] {
            CloseServiceHandle(schSCManager);
        });

        SC_HANDLE schService = OpenServiceW(schSCManager, SERVICE_NAME, SERVICE_QUERY_STATUS | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

        const auto closeService = wil::scope_exit([&] {
            CloseServiceHandle(schService);
        });

        const auto servicePath = get_module_folderpath(g_hInst_MouseWithoutBorders) + L"/PowerToys.MouseWithoutBordersService.exe";

        // Check that the service doesn't exist already and is not disabled
        DWORD bytesNeeded;
        LPQUERY_SERVICE_CONFIGW pServiceConfig = nullptr;
        if (!QueryServiceConfigW(schService, nullptr, 0, &bytesNeeded))
        {
            if (GetLastError() == ERROR_INSUFFICIENT_BUFFER)
            {
                pServiceConfig = static_cast<LPQUERY_SERVICE_CONFIGW>(LocalAlloc(LMEM_FIXED, bytesNeeded));
                if (!QueryServiceConfigW(schService, pServiceConfig, bytesNeeded, &bytesNeeded))
                {
                    LocalFree(pServiceConfig);
                    pServiceConfig = nullptr;
                    CloseServiceHandle(schService);
                }
            }
        }

        bool alreadyRegistered = false;
        if (pServiceConfig)
        {
            std::wstring_view existingServicePath{ pServiceConfig->lpBinaryPathName };
            existingServicePath = existingServicePath.substr(1, existingServicePath.find(L'"', 1) - 1);
            alreadyRegistered = std::filesystem::path{ existingServicePath } == servicePath;
            LocalFree(pServiceConfig);

            if (alreadyRegistered && pServiceConfig->dwStartType == SERVICE_DISABLED)
            {
                if (!ChangeServiceConfigW(schService,
                                          SERVICE_NO_CHANGE,
                                          SERVICE_DEMAND_START,
                                          SERVICE_NO_CHANGE,
                                          nullptr,
                                          nullptr,
                                          nullptr,
                                          nullptr,
                                          nullptr,
                                          nullptr,
                                          nullptr))
                {
                    const bool markedForDelete = GetLastError() == ERROR_SERVICE_MARKED_FOR_DELETE;
                    // We cannot remove the mark for deletion from the service.
                    if (markedForDelete)
                    {
                        alreadyRegistered = false;
                        CloseServiceHandle(schService);
                    }
                }
            }

            if (alreadyRegistered)
            {
                SERVICE_DELAYED_AUTO_START_INFO delayedAutoStartInfo;
                if (!QueryServiceConfig2W(schService, SERVICE_CONFIG_DELAYED_AUTO_START_INFO, reinterpret_cast<LPBYTE>(&delayedAutoStartInfo), sizeof(delayedAutoStartInfo), &bytesNeeded) || delayedAutoStartInfo.fDelayedAutostart)
                {
                    alreadyRegistered = false;
                    // Wait until the service has been actually deleted so we can recreate it again
                    for (int i = 0; i < 10; ++i)
                    {
                        schService = OpenServiceW(schSCManager, SERVICE_NAME, SERVICE_QUERY_STATUS | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);
                        if (schService)
                        {
                            CloseServiceHandle(schService);
                            Sleep(1000);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        if (alreadyRegistered)
        {
            return;
        }

        // Pass local app data of the current user to the service
        PWSTR cLocalAppPath;
        winrt::check_hresult(SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, nullptr, &cLocalAppPath));
        CoTaskMemFree(cLocalAppPath);

        std::wstring localAppPath{ cLocalAppPath };
        std::wstring binaryWithArgsPath = L"\"";
        binaryWithArgsPath += servicePath;
        binaryWithArgsPath += L"\" ";
        binaryWithArgsPath += escapeDoubleQuotes(localAppPath);

        schService = CreateServiceW(
            schSCManager,
            SERVICE_NAME,
            SERVICE_NAME,
            SERVICE_ALL_ACCESS,
            SERVICE_WIN32_OWN_PROCESS,
            SERVICE_DEMAND_START,
            SERVICE_ERROR_NORMAL,
            binaryWithArgsPath.c_str(),
            nullptr,
            nullptr,
            nullptr,
            nullptr,
            nullptr);

        if (schService == nullptr)
        {
            Logger::error(L"Failed to create service");
            return;
        }

        // Set up the security descriptor to allow non-elevated users to start the service
        PSECURITY_DESCRIPTOR pSD = nullptr;
        ULONG szSD = 0;
        std::wstring securityDescriptor = generateSecurityDescriptor(GetCurrentUserSid());

        if (!ConvertStringSecurityDescriptorToSecurityDescriptorW(
                securityDescriptor.c_str(),
                SDDL_REVISION_1,
                &pSD,
                &szSD))
        {
            Logger::error(L"Failed to convert security descriptor string");
            CloseServiceHandle(schService);
            return;
        }

        if (!SetServiceObjectSecurity(schService, DACL_SECURITY_INFORMATION, pSD))
        {
            Logger::error("Failed to set service object security");
        }

        LocalFree(pSD);
        CloseServiceHandle(schService);
    }

    void update_state_from_settings(const PowerToysSettings::PowerToyValues& values)
    {
        const bool new_run_in_service_mode = values.get_bool_value(USE_SERVICE_PROPERTY_NAME).value_or(false);

        if (new_run_in_service_mode != run_in_service_mode)
        {
            run_in_service_mode = new_run_in_service_mode;

            shutdown_processes();

            if (new_run_in_service_mode)
            {
                register_service();
            }
            // Wait until Settings -> MWB IPC Shutdown() call is completed
            else
            {
                const auto ps = getProcessHandlesByName(L"PowerToys.MouseWithoutBorders.exe", PROCESS_QUERY_LIMITED_INFORMATION);
                for (const auto& p : ps)
                {
                    DWORD status = STILL_ACTIVE;
                    do
                    {
                        GetExitCodeProcess(p.get(), &status);
                    } while (status == STILL_ACTIVE);
                }

                Sleep(1000);
            }

            if (m_enabled)
            {
                launch_process();
            }

            Trace::MouseWithoutBorders::ToggleServiceRegistration(new_run_in_service_mode);
        }
    }

public:
    MouseWithoutBorders()
    {
        app_name = L"MouseWithoutBorders";
        app_key = app_name;
        std::filesystem::path logFilePath(PTSettingsHelper::get_module_save_folder_location(app_key));
        logFilePath.append(LogSettings::mouseWithoutBordersLogPath);
        Logger::init(LogSettings::mouseWithoutBordersLoggerName, logFilePath.wstring(), PTSettingsHelper::get_log_settings_file_location());

        try
        {
            PowerToysSettings::PowerToyValues values =
                PowerToysSettings::PowerToyValues::load_from_settings_file(MODULE_NAME);
            update_state_from_settings(values);
        }
        catch (std::exception&)
        {
            // Initial start/
        }
    };

    // Return the configured status for the gpo policy for the module
    virtual powertoys_gpo::gpo_rule_configured_t gpo_policy_enabled_configuration() override
    {
        return powertoys_gpo::getConfiguredMouseWithoutBordersEnabledValue();
    }

    void shutdown_process(HANDLE handle)
    {
        auto cb = [](HWND hwnd, LPARAM lParam) -> BOOL {
            DWORD processId;
            GetWindowThreadProcessId(hwnd, &processId);

            if (processId == lParam)
            {
                PostMessageW(hwnd, WM_CLOSE, 0, 0);
                return FALSE;
            }

            return TRUE;
        };

        DWORD processId = GetProcessId(handle);
        EnumWindows(cb, processId);

        DWORD waitResult = WaitForSingleObject(handle, 3000);
        if (waitResult != WAIT_OBJECT_0)
        {
            TerminateProcess(handle, 0);
        }
    }

    void shutdown_processes()
    {
        const auto services = getProcessHandlesByName(L"PowerToys.MouseWithoutBordersService.exe", PROCESS_TERMINATE);
        for (const auto& svc : services)
            TerminateProcess(svc.get(), 0);
        wil::unique_process_handle s;

        std::array<std::wstring_view, 2> processes_names = { L"PowerToys.MouseWithoutBorders.exe",
                                                             L"PowerToys.MouseWithoutBordersHelper.exe" };

        for (const auto process : processes_names)
        {
            const auto apps = getProcessHandlesByName(process, PROCESS_TERMINATE);
            for (const auto& app : apps)
                shutdown_process(app.get());
        }
    }

    virtual void destroy() override
    {
        shutdown_processes();
        TerminateProcess(p_info.hProcess, 1);
        delete this;
    }

    virtual const wchar_t* get_name() override
    {
        return MODULE_NAME;
    }

    virtual bool get_config(wchar_t* buffer, int* buffer_size) override
    {
        HINSTANCE hinstance = reinterpret_cast<HINSTANCE>(&__ImageBase);

        PowerToysSettings::Settings settings(hinstance, get_name());
        settings.set_description(MODULE_DESC);

        return settings.serialize_to_buffer(buffer, buffer_size);
    }

    virtual const wchar_t* get_key() override
    {
        return app_key.c_str();
    }

    virtual void set_config(const wchar_t* config) override
    {
        try
        {
            // Parse the input JSON string.
            PowerToysSettings::PowerToyValues values =
                PowerToysSettings::PowerToyValues::from_json_string(config, get_key());

            update_state_from_settings(values);
            // If you don't need to do any custom processing of the settings, proceed
            // to persists the values.
            values.save_to_settings_file();
        }
        catch (std::exception&)
        {
            // Improper JSON.
        }
    }

    virtual void enable()
    {
        Trace::MouseWithoutBorders::Enable(true);
        ResetEvent(send_telemetry_event);
        ResetEvent(m_hInvokeEvent);

        launch_process();

        m_enabled = true;
    };

    virtual void disable()
    {
        if (m_enabled)
        {
            Trace::MouseWithoutBorders::Enable(false);
            Logger::trace(L"Disabling MouseWithoutBorders...");
            ResetEvent(send_telemetry_event);
            ResetEvent(m_hInvokeEvent);

            Logger::trace(L"Signaled exit event for PowerToys MouseWithoutBorders.");
            TerminateProcess(p_info.hProcess, 1);

            shutdown_processes();

            CloseHandle(p_info.hProcess);
        }

        m_enabled = false;
    }

    virtual bool is_enabled() override
    {
        return m_enabled;
    }

    void launch_add_firewall_process()
    {
        Logger::trace(L"Starting Process to add firewall rule");

        std::wstring executable_path = get_module_folderpath();
        executable_path.append(L"\\modules\\MouseWithoutBorders\\PowerToys.MouseWithoutBorders.exe");

        std::wstring executable_args = L"";
        executable_args.append(L"/S /c \"");
        executable_args.append(L"echo 给 PowerToys.MouseWithoutBorders.exe 删除所有原有的入站规则");
        executable_args.append(L" & netsh advfirewall firewall delete rule dir=in name=all program=\"");
        executable_args.append(executable_path);
        executable_args.append(L"\" & echo 给 PowerToys.MouseWithoutBorders.exe 添加一条允许通过的入站规则");
        executable_args.append(L" & netsh advfirewall firewall add rule name=\"PowerToys.MouseWithoutBorders\" dir=in action=allow program=\"");
        executable_args.append(executable_path);
        executable_args.append(L"\" enable=yes remoteip=LocalSubnet profile=any protocol=tcp & echo 现在可以关掉这个窗口了 & pause\"");

        SHELLEXECUTEINFOW sei{ sizeof(sei) };
        sei.fMask = { SEE_MASK_NOCLOSEPROCESS | SEE_MASK_FLAG_NO_UI };
        sei.lpFile = L"cmd.exe";
        sei.nShow = SW_SHOWNORMAL;
        sei.lpParameters = executable_args.data();
        sei.lpVerb = L"runas";

        if (ShellExecuteExW(&sei))
        {
            Logger::trace("Successfully started the firewall rule adding process");
        }
        else
        {
            Logger::error(L"The firewall rule adding process failed to start. {}", get_last_error_or_default(GetLastError()));
        }
    }

    virtual void call_custom_action(const wchar_t* action) override
    {
        try
        {
            PowerToysSettings::CustomActionObject action_object =
                PowerToysSettings::CustomActionObject::from_json_string(action);

            if (action_object.get_name() == L"add_firewall")
            {
                launch_add_firewall_process();
                Trace::MouseWithoutBorders::AddFirewallRule();
            }
            else if (action_object.get_name() == L"uninstall_service")
            {
                unregister_service();
            }
        }
        catch (std::exception&)
        {
            Logger::error(L"Failed to parse action. {}", action);
        }
    }
};

extern "C" __declspec(dllexport) PowertoyModuleIface* __cdecl powertoy_create()
{
    return new MouseWithoutBorders();
}