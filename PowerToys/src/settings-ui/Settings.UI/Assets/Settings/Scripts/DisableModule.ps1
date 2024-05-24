$profileContent = Get-Content $PROFILE

$newContent = ""
$linesToDeleteFound = $False
$atLeastOneInstanceFound = $False

$profileContent | ForEach-Object {
  if (($_.Contains("34de4b3d-13a8-4540-b76d-b9e8d3851756") -or $_.Contains("f45873b3-b655-43a6-b217-97c00aa0db58")) -and !$linesToDeleteFound)
  {
    $linesToDeleteFound = $True
    $atLeastOneInstanceFound = $True
    return
  }

  if (($_.Contains("34de4b3d-13a8-4540-b76d-b9e8d3851756") -or $_.Contains("f45873b3-b655-43a6-b217-97c00aa0db58")) -and $linesToDeleteFound)
  {
    $linesToDeleteFound = $False
    return
  }

  if($linesToDeleteFound)
  {
    return
  }

  $newContent += $_ + "`r`n"
}

if($atLeastOneInstanceFound)
{
  Set-Content -Path $PROFILE -Value $newContent
  Write-Host "已从配置文件中删除本模块."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
} else {
  Write-Host "配置文件中没有发现本模块, 不需要删除."
  # This message will be compared against in Command Not Found Settings page code behind. Take care when changing it.
}
