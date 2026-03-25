taskkill /F /IM dotnet.exe 2>$null
taskkill /F /IM adb.exe 2>$null
taskkill /F /IM java.exe 2>$null

Write-Host "Cleaning..."

Remove-Item -Recurse -Force -ErrorAction SilentlyContinue bin
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue obj

Write-Host "Done clean!"
