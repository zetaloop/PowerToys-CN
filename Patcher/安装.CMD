@echo off
chcp 65001>nul
color f0
if not exist "%~dp0组策略文件\安装组策略.cmd" echo 检测到文件缺失，请解压后运行！&pause&exit
echo 请允许管理员权限以便安装，谢谢。
%1 mshta vbscript:CreateObject("Shell.Application").ShellExecute("cmd.exe","/c %~s0 ::","","runas",1)(window.close)&&exit
cd /d %~dp0
mode con cols=20 lines=10

for /f "tokens=2 delims= " %%i in ('dir /b^|findstr PowerToys') do set modver=%%i
set modfulver=%modver%
set softname=PowerToys

title 修改 %softname%
for /l %%i in (1,20,40) do mode con cols=20 lines=%%i
for /l %%i in (20,40,160) do mode con cols=%%i lines=40
:start
color f0
msg %username% /time:1 /v /w 欢迎使用 Zetaloop 的 %softname% 修改！>nul
color 07

for /f "skip=2 tokens=2,*" %%a in ('reg query "HKCR\powertoys\shell\open\command"') do set ptpath=%%b
if %errorlevel%==1 echo 警告：找不到注册表信息！你可能未安装 %softname%&set /p =请按任意键退出. . . &exit
set ptpath=%ptpath:~1,-6%
for /f "delims=" %%a in ('echo %ptpath%') do set installpath=%%~dpa
for /f "usebackq delims=" %%a in (`mshta vbscript:CreateObject("Scripting.FileSystemObject"^).GetStandardStream(1^).WriteLine(CStr(CreateObject("Scripting.FileSystemObject"^).GetFileVersion("%ptpath%"^)^)^)(window.close^)`) do set installver=%%a
for /f "tokens=1,2,3 delims=." %%a in ('echo %installver%') do set installver=%%a.%%b.%%c
echo.
echo 修改包版本 %modfulver%
echo %softname% 版本 %installver%
if not "%modfulver%"=="%installver%" echo ·警告：汉化包版本不对应！&msg %username% /v /w 警告：汉化包版本不对应，强行安装可能导致报错闪退！>nul
echo.
echo 安装目录 %installpath%
if not exist "%installpath%PowerToys.exe" echo 警告：安装目录检测失败！ %&set /p =请按任意键退出. . . &exi
echo.
echo 安装过程：
::
echo ·解除安装目录的占用
::
echo ·复制汉化文件到软件目录
echo.
echo 为解除文件占用，将会重启 Windows 资源管理器
echo ---------------------------------------------------
choice /c thequickbrownfxjmpsvlazydg /n /m "确认无误后，按下 Y 开始："
if %errorlevel%==24 goto install
::if %errorlevel%==22 goto installarm64
echo 取消安装。退出安装器...
choice /t 1 /d n >nul
exit

:installarm64
set modver=%modver% ARM64

:install
echo 开始安装！
if exist "%tmp%\PowerToys.NewPlus.ShellExtension.dll_*" del "%tmp%\PowerToys.NewPlus.ShellExtension.dll_*"
if exist "%tmp%\PowerToys.FileLocksmithExt.dll_*" del "%tmp%\PowerToys.FileLocksmithExt.dll_*"
if exist "%tmp%\PowerToys.FileLocksmithContextMenu.dll_*" del "%tmp%\PowerToys.FileLocksmithContextMenu.dll_*"
if exist "%tmp%\PowerToys.PowerRenameExt.dll_*" del "%tmp%\PowerToys.PowerRenameExt.dll_*"
if exist "%tmp%\PowerToys.PowerRenameContextMenu.dll_*" del "%tmp%\PowerToys.PowerRenameContextMenu.dll_*"
if exist "%tmp%\PowerToys.RegistryPreviewExt.dll_*" del "%tmp%\PowerToys.RegistryPreviewExt.dll_*"
if exist "%tmp%\PowerToys.ImageResizerExt.dll_*" del "%tmp%\PowerToys.ImageResizerExt.dll_*"
:chcp 936>nul
call :killpt
rmdir /s /q %installpath%WinUI3Apps\Assets\NewPlus\Templates
xcopy ".\%softname% %modver%\*" "%installpath%" /e /y
if %errorlevel%==0 goto success
echo 尝试解锁...
taskkill /f /im explorer.exe
rmdir /s /q %installpath%WinUI3Apps\Assets\NewPlus\Templates
xcopy ".\%softname% %modver%\*" "%installpath%" /e /y
if %errorlevel%==0 start explorer&goto success
start explorer
echo 尝试移走一些文件...
echo 这些文件放到了系统临时文件夹。
::
echo 你可能需要重启电脑来使汉化生效。
::
if exist "%tmp%\PowerToys.NewPlus.ShellExtension.dll_*" del "%tmp%\PowerToys.NewPlus.ShellExtension.dll_*"
if exist "%installpath%WinUI3Apps\PowerToys.NewPlus.ShellExtension.dll" move "%installpath%WinUI3Apps\PowerToys.NewPlus.ShellExtension.dll" "%tmp%\PowerToys.NewPlus.ShellExtension.dll_%random%.tmp"
if exist "%tmp%\PowerToys.FileLocksmithExt.dll_*" del "%tmp%\PowerToys.FileLocksmithExt.dll_*"
if exist "%installpath%WinUI3Apps\PowerToys.FileLocksmithExt.dll" move "%installpath%WinUI3Apps\PowerToys.FileLocksmithExt.dll" "%tmp%\PowerToys.FileLocksmithExt.dll_%random%.tmp"
if exist "%tmp%\PowerToys.FileLocksmithContextMenu.dll_*" del "%tmp%\PowerToys.FileLocksmithContextMenu.dll_*"
if exist "%installpath%WinUI3Apps\PowerToys.FileLocksmithContextMenu.dll" move "%installpath%WinUI3Apps\PowerToys.FileLocksmithContextMenu.dll" "%tmp%\PowerToys.FileLocksmithContextMenu.dll_%random%.tmp"
if exist "%tmp%\PowerToys.PowerRenameExt.dll_*" del "%tmp%\PowerToys.PowerRenameExt.dll_*"
if exist "%installpath%WinUI3Apps\PowerToys.PowerRenameExt.dll" move "%installpath%WinUI3Apps\PowerToys.PowerRenameExt.dll" "%tmp%\PowerToys.PowerRenameExt.dll_%random%.tmp"
if exist "%tmp%\PowerToys.PowerRenameContextMenu.dll_*" del "%tmp%\PowerToys.PowerRenameContextMenu.dll_*"
if exist "%installpath%WinUI3Apps\PowerToys.PowerRenameContextMenu.dll" move "%installpath%WinUI3Apps\PowerToys.PowerRenameContextMenu.dll" "%tmp%\PowerToys.PowerRenameContextMenu.dll_%random%.tmp"
if exist "%tmp%\PowerToys.RegistryPreviewExt.dll_*" del "%tmp%\PowerToys.RegistryPreviewExt.dll_*"
if exist "%installpath%WinUI3Apps\PowerToys.RegistryPreviewExt.dll" move "%installpath%WinUI3Apps\PowerToys.RegistryPreviewExt.dll" "%tmp%\PowerToys.RegistryPreviewExt.dll_%random%.tmp"
if exist "%tmp%\PowerToys.ImageResizerExt.dll_*" del "%tmp%\PowerToys.ImageResizerExt.dll_*"
if exist "%installpath%PowerToys.ImageResizerExt.dll" move "%installpath%PowerToys.ImageResizerExt.dll" "%tmp%\PowerToys.ImageResizerExt.dll_%random%.tmp"
echo 再试一次...
echo.
echo.
call :killpt
rmdir /s /q %installpath%WinUI3Apps\Assets\NewPlus\Templates
xcopy ".\%softname% %modver%\*" "%installpath%" /e /y
if %errorlevel%==0 goto success
echo 尝试解锁...
taskkill /f /im explorer.exe
rmdir /s /q %installpath%WinUI3Apps\Assets\NewPlus\Templates
xcopy ".\%softname% %modver%\*" "%installpath%" /e /y
if %errorlevel%==0 start explorer&goto success
start explorer
echo 安装失败！未能解除文件占用，请尝试手动复制，或重启后再试。
::
msg %username% /time:2 /v /w 安装失败！未能解除文件占用，请尝试手动复制，或重启后再试。>nul
set /p =请按任意键退出. . . &exit
goto :eof

:success
echo 重启文件资源管理器以加载汉化右键菜单
::
powershell exit
taskkill /f /im explorer.exe>nul 2>nul
taskkill /f /im explorer.exe>nul 2>nul
taskkill /f /im explorer.exe>nul 2>nul
powershell exit
start explorer
powershell exit
echo 启动 PowerToys
mshta vbscript:CreateObject("Shell.Application").ShellExecute("%installpath%PowerToys.exe","","","runas",1)(window.close)
powershell exit
echo 打开 PowerToys 设置
mshta vbscript:CreateObject("Shell.Application").ShellExecute("%installpath%PowerToys.exe","","","runas",1)(window.close)
powershell exit
echo 安装完成
:: Powershell 用于提供一个恰当的启动延迟
::
msg %username% /time:2 /v /w 安装完成！>nul
goto :eof

:killpt
taskkill /F /IM "PowerToys*"
taskkill /F /IM "PowerToys*"
goto :eof