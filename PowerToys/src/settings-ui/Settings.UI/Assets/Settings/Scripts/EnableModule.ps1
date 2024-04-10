[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True,Position=1)]
  [string]$scriptPath
)

Write-Host "Enabling experimental feature: PSFeedbackProvider"
Enable-ExperimentalFeature PSFeedbackProvider
Write-Host "Enabling experimental feature: PSCommandNotFoundSuggestion"
Enable-ExperimentalFeature PSCommandNotFoundSuggestion

if (Get-Module -ListAvailable -Name Microsoft.WinGet.Client) {
    Write-Host "已安装 WinGet 客户端模块"
} 
else {
    Write-Host "未安装 WinGet 客户端模块. 如需安装请访问 https://www.powershellgallery.com/packages/Microsoft.WinGet.Client `r`n"
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
  Write-Host "发现模块已经在配置文件中注册过了."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
else
{
  Add-Content -Path $PROFILE  -Value "`r`n#34de4b3d-13a8-4540-b76d-b9e8d3851756 PowerToys CommandNotFound module"
  Add-Content -Path $PROFILE  -Value "`r`nImport-Module `"$scriptPath\WinGetCommandNotFound.psd1`""
  Add-Content -Path $PROFILE  -Value "#34de4b3d-13a8-4540-b76d-b9e8d3851756"  
  Write-Host "成功在配置文件中注册模块."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
