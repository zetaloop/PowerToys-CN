
if ((Get-AppxPackage microsoft.DesktopAppInstaller).Version -ge [System.Version]"1.21")
{
  Write-Host "已安装 winget, 即将开始安装 PowerShell."
}
else
{
  Write-Host "未安装 WinGet, 即将开始安装它."
  # To speed up Invoke-WebRequest. Older versions are very slow when printing the progress.
  $ProgressPreference = 'SilentlyContinue'
  $cpuArchitecture="x64"
  $detectedArchitecture=""
  if ($env:PROCESSOR_ARCHITEW6432 -eq $null) {
    $detectedArchitecture=$env:PROCESSOR_ARCHITECTURE
  } else {
    $detectedArchitecture=$env:PROCESSOR_ARCHITEW6432
  }
  Write-Host "CPU 架构: $detectedArchitecture"
  if ($detectedArchitecture -ne "AMD64")
  {
    Write-Host "不属于 AMD64, 判定为 arm64, 因为这是本软件通常所处的环境."
    $cpuArchitecture="arm64"
  }
  if((Get-AppxPackage Microsoft.VCLibs.140.00).Version -ge [System.Version]"14.0.30704")
  {
    Write-Host "已安装 Microsoft.VCLibs.140.00."
  }
  else
  {
    Write-Host "未安装 Microsoft.VCLibs.140.00, 即将开始安装它."
    Invoke-WebRequest -Uri https://aka.ms/Microsoft.VCLibs.$cpuArchitecture.14.00.Desktop.appx -OutFile "$Env:TMP\Microsoft.VCLibs.14.00.appx"
    Add-AppxPackage -Path "$Env:TMP\Microsoft.VCLibs.14.00.appx"
    Remove-Item -Path "$Env:TMP\Microsoft.VCLibs.14.00.appx" -Force
  }
  if((Get-AppxPackage Microsoft.VCLibs.140.00.UWPDesktop).Version -ge [System.Version]"14.0.30704")
  {
    Write-Host "已安装 Microsoft.VCLibs.140.00.UWPDesktop"
  }
  else
  {
    Write-Host "未安装 Microsoft.VCLibs.140.00.UWPDesktop, 即将开始安装它."
    Invoke-WebRequest -Uri https://aka.ms/Microsoft.VCLibs.$cpuArchitecture.14.00.Desktop.appx -OutFile "$Env:TMP\Microsoft.VCLibs.14.00.Desktop.appx"
    Add-AppxPackage -Path "$Env:TMP\Microsoft.VCLibs.14.00.Desktop.appx"
    Remove-Item -Path "$Env:TMP\Microsoft.VCLibs.14.00.Desktop.appx" -Force
  }
  if (Get-AppxPackage Microsoft.UI.Xaml.2.7)
  {
    Write-Host "已安装 Microsoft.UI.Xaml.2.7"
  }
  else
  {
    Write-Host "未安装 Microsoft.UI.Xaml.2.7, 即将开始安装它."
    Write-Host "正在下载到 $Env:TMP\microsoft.ui.xaml.2.7.3.zip"
    Invoke-WebRequest -Uri https://www.nuget.org/api/v2/package/Microsoft.UI.Xaml/2.7.3 -OutFile "$Env:TMP\microsoft.ui.xaml.2.7.3.zip"
    Write-Host "正在解压 $Env:TMP\microsoft.ui.xaml.2.7.3.zip"
    Expand-Archive "$Env:TMP\microsoft.ui.xaml.2.7.3.zip" -DestinationPath "$Env:TMP\microsoft.ui.xaml.2.7.3"
    Write-Host "正在安装 $Env:TMP\microsoft.ui.xaml.2.7.3\tools\AppX\$cpuArchitecture\Release\Microsoft.UI.Xaml.2.7.appx"
    Add-AppxPackage "$Env:TMP\microsoft.ui.xaml.2.7.3\tools\AppX\$cpuArchitecture\Release\Microsoft.UI.Xaml.2.7.appx"
    Remove-Item -Path "$Env:TMP\microsoft.ui.xaml.2.7.3" -Recurse -Force
    Remove-Item -Path "$Env:TMP\microsoft.ui.xaml.2.7.3.zip" -Force
  }
  Write-Host "正在获取最新稳定版的 winget"
  Invoke-WebRequest -Uri https://aka.ms/getwinget -OutFile "$Env:TMP\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle"
  Add-AppxPackage -Path "$Env:TMP\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle"
  Remove-Item -Path "$Env:TMP\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle" -Force

  #winget is not visible right away, so reload the PATH variable.
  $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User") 
}

winget install Microsoft.PowerShell --source winget
if ($LASTEXITCODE -eq 0)
{
  Write-Host "成功安装 Powershell 7."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
else
{
  Write-Host "未能成功安装 Powershell 7."
}

