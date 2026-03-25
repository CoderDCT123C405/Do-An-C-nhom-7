param(
    [switch]$IncludeOfflineDb
)

$ErrorActionPreference = "SilentlyContinue"

Write-Host "Cleaning temporary files..."

# Stop local run processes if still running
$ports = @(5000, 5256, 5257, 5258, 5259, 5260, 5261, 5262, 5263, 5264, 5265, 5266, 5267, 5268, 5269, 5270, 5271, 5272, 5273, 5274, 5275)
foreach ($p in $ports) {
    $lines = netstat -ano | Select-String ":$p\s+.*LISTENING\s+(\d+)$"
    foreach ($line in $lines) {
        $pid = $line.Matches[0].Groups[1].Value
        if ($pid) { taskkill /PID $pid /F *> $null }
    }
}

# Remove build artifacts
Get-ChildItem -Path . -Recurse -Directory -Filter bin | Remove-Item -Recurse -Force
Get-ChildItem -Path . -Recurse -Directory -Filter obj | Remove-Item -Recurse -Force

# Remove local dotnet temp homes
Remove-Item -Path ".\.dotnet",".\.dotnet-home" -Recurse -Force

# Remove run/build logs
Remove-Item -Path ".\api.build.log",".\cms.build.log",".\api.run.log",".\api.run.err.log",".\cms.run.log",".\cms.run.err.log" -Force

# Remove SQLite temp files
Get-ChildItem -Path . -Recurse -File -Include "*.db-shm","*.db-wal" | Remove-Item -Force

if ($IncludeOfflineDb) {
    Get-ChildItem -Path . -Recurse -File -Include "HeThongThuyetMinhDuLich.offline.db" | Remove-Item -Force
    Write-Host "Offline DB file removed."
}

Write-Host "CLEAN_OK"
