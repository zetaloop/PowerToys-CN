[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True,Position=1)]
  [string]$scriptPath
)

Write-Host "Enabling experimental feature: PSFeedbackProvider"
Enable-ExperimentalFeature PSFeedbackProvider
Write-Host "Enabling experimental feature: PSCommandNotFoundSuggestion"
Enable-ExperimentalFeature PSCommandNotFoundSuggestion

$wingetModules = Get-Module -ListAvailable -Name Microsoft.WinGet.Client
if ($wingetModules) {

  $moduleUpToDate = $false;
  foreach ($mod in $wingetModules) {
    if ($mod.Version -ge "1.8.1133") {
      $moduleUpToDate = $true;
      break;
    }
  }

  if ($moduleUpToDate) {
    Write-Host "已安装 WinGet 客户端模块"
  } else {
    Write-Host "WinGet 客户端模块需要更新. 运行 `"Update-Module -Name Microsoft.WinGet.Client`" 来更新 `r`n"
  }
} 
else {
    Write-Host "未安装 WinGet 客户端模块. 如需安装请访问 https://www.powershellgallery.com/packages/Microsoft.WinGet.Client `r`n"
}

$CNFModule = Get-Module -ListAvailable -Name Microsoft.WinGet.CommandNotFound
if ($CNFModule) {
  Write-Host "已安装 Microsoft.WinGet.CommandNotFound 模块"
} else {
  Write-Host "未安装 Microsoft.WinGet.CommandNotFound 模块. 正在安装它...`r`n"
  Install-Module -Name Microsoft.WinGet.CommandNotFound -Force
  Write-Host "成功安装 Microsoft.WinGet.CommandNotFound 模块`r`n"
}

if (!(Test-Path $PROFILE))
{
  Write-Host "未发现配置文件 $PROFILE".
  New-Item -Path $PROFILE -ItemType File
  Write-Host "已创建配置文件 $PROFILE".
}

$profileContent = Get-Content -Path $PROFILE -Raw

if ((-not [string]::IsNullOrEmpty($profileContent)) -and ($profileContent.Contains("34de4b3d-13a8-4540-b76d-b9e8d3851756")))
{
  if ($profileContent.Contains("Import-Module `"$scriptPath\WinGetCommandNotFound.psd1`""))
  {
    $profileContent = $profileContent.Replace("Import-Module `"$scriptPath\WinGetCommandNotFound.psd1`"",
                                              "Import-Module -Name Microsoft.WinGet.CommandNotFound")
    $profileContent = $profileContent.Replace("34de4b3d-13a8-4540-b76d-b9e8d3851756",
                                              "f45873b3-b655-43a6-b217-97c00aa0db58")
    Set-Content -Path $PROFILE -Value $profileContent
    Write-Host "成功在配置文件中更新模块."
    # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
  }
}
elseif ((-not [string]::IsNullOrEmpty($profileContent)) -and ($profileContent.Contains("f45873b3-b655-43a6-b217-97c00aa0db58")))
{
  Write-Host "发现模块已经在配置文件中注册过了."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
else
{
  Add-Content -Path $PROFILE  -Value "`r`n#f45873b3-b655-43a6-b217-97c00aa0db58 PowerToys CommandNotFound module"
  Add-Content -Path $PROFILE  -Value "`r`nImport-Module -Name Microsoft.WinGet.CommandNotFound"
  Add-Content -Path $PROFILE  -Value "#f45873b3-b655-43a6-b217-97c00aa0db58"  
  Write-Host "成功在配置文件中注册模块."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
