$ErrorActionPreference = "SilentlyContinue"

$ports = @(5000, 5256)
foreach ($p in $ports) {
    $lines = netstat -ano | Select-String ":$p\s+.*LISTENING\s+(\d+)$"
    foreach ($line in $lines) {
        $procId = $line.Matches[0].Groups[1].Value
        if ($procId) { taskkill /PID $procId /F | Out-Null }
    }
}

Write-Host "Starting API at http://localhost:5000 ..."
Start-Process -FilePath "dotnet" -ArgumentList "run --project `"HeThongThuyetMinhDuLich.Api\HeThongThuyetMinhDuLich.Api.csproj`" --urls `"http://localhost:5000`"" -WorkingDirectory (Get-Location) | Out-Null

Start-Sleep -Seconds 2

Write-Host "Starting CMS at http://localhost:5256 ..."
Start-Process -FilePath "dotnet" -ArgumentList "run --project `"HeThongThuyetMinhDuLich.Cms\HeThongThuyetMinhDuLich.Cms.csproj`" --urls `"http://localhost:5256`"" -WorkingDirectory (Get-Location) | Out-Null

Write-Host ""
Write-Host "Run complete:"
Write-Host " - API Swagger: http://localhost:5000/swagger/index.html"
Write-Host " - CMS: http://localhost:5256"
