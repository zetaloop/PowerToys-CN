Write-Host $PSVersionTable
if ($PSVersionTable.PSVersion -ge 7.4)
{
  Write-Host "已安装 PowerShell 7.4 或更高版本."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
else
{
  Write-Host "未安装 PowerShell 7.4 或更高版本. 如需安装请访问 https://learn.microsoft.com/powershell/scripting/install/installing-powershell-on-windows `r`n"
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}

if ($mods = Get-Module -ListAvailable -Name Microsoft.WinGet.Client)
{
  Write-Host "已安装 WinGet 客户端模块."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.

  $needsUpdate = $true;
  foreach ($mod in $mods)
  {
    if ($mod.Version -ge "1.8.1133")
    {
      $needsUpdate = $false;
      break;
    }
  }
  if ($needsUpdate)
  {
    Write-Host "WinGet 客户端模块需要更新."
    # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
  }
} 
else {
  Write-Host "未安装 WinGet 客户端模块. 如需安装请访问 https://www.powershellgallery.com/packages/Microsoft.WinGet.Client `r`n"
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
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
  Write-Host "已在配置文件中注册旧版模块."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
elseif ((-not [string]::IsNullOrEmpty($profileContent)) -and ($profileContent.Contains("f45873b3-b655-43a6-b217-97c00aa0db58")))
{
  Write-Host "已在配置文件中注册本模块."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
else
{
  Write-Host "未在配置文件中注册本模块."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
