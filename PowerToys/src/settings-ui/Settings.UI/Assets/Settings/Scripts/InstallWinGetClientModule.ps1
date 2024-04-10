if (Get-Module -ListAvailable -Name Microsoft.WinGet.Client)
{
  Write-Host "已安装 WinGet 客户端模块."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
} 
else {
  Install-Module -Name Microsoft.WinGet.Client
  if (Get-Module -ListAvailable -Name Microsoft.WinGet.Client)
  {
    Write-Host "已安装 WinGet 客户端模块."
    # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
  } else {
    Write-Host "未安装 WinGet 客户端模块. 如需安装请访问 https://www.powershellgallery.com/packages/Microsoft.WinGet.Client `r`n"
    # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
  }
}
