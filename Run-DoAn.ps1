param(
    [ValidateSet("online", "offline")]
    [string]$Mode = "offline",
    [switch]$ResetOfflineDb,
    [switch]$SkipBuild
)

$ErrorActionPreference = "SilentlyContinue"

$ports = @(5000, 5256)
foreach ($p in $ports) {
    $lines = netstat -ano | Select-String ":$p\s+.*LISTENING\s+(\d+)$"
    foreach ($line in $lines) {
        $procId = $line.Matches[0].Groups[1].Value
        if ($procId -and (Get-Process -Id $procId -ErrorAction SilentlyContinue)) {
            taskkill /PID $procId /F *> $null
        }
    }
}

Remove-Item -Path ".\api.run.log",".\api.run.err.log",".\cms.run.log",".\cms.run.err.log",".\api.build.log",".\cms.build.log" -Force -ErrorAction SilentlyContinue

function Test-PortListening {
    param([int]$Port)
    $line = netstat -ano | Select-String ":$Port\s+.*LISTENING\s+(\d+)$" | Select-Object -First 1
    return $null -ne $line
}

function Get-ListeningPid {
    param([int]$Port)
    $line = netstat -ano | Select-String ":$Port\s+.*LISTENING\s+(\d+)$" | Select-Object -First 1
    if ($null -eq $line) { return $null }
    return [int]$line.Matches[0].Groups[1].Value
}

$cmsPort = 5256
if (Test-PortListening -Port $cmsPort) {
    for ($port = 5257; $port -le 5275; $port++) {
        if (-not (Test-PortListening -Port $port)) {
            $cmsPort = $port
            break
        }
    }
}
$cmsUrl = "http://localhost:$cmsPort"

# $dotnetHome = Join-Path (Get-Location) ".dotnet-home"
# New-Item -ItemType Directory -Path $dotnetHome -Force | Out-Null
# $env:DOTNET_CLI_HOME = $dotnetHome
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:MSBUILDDISABLENODEREUSE = "1"
$dbProvider = if ($Mode -eq "offline") { "Sqlite" } else { "SqlServer" }
$env:Database__Provider = $dbProvider

if ($Mode -eq "offline" -and $ResetOfflineDb) {
    $offlineDbPath = Join-Path (Get-Location) "HeThongThuyetMinhDuLich.Api\HeThongThuyetMinhDuLich.offline.db"
    if (Test-Path $offlineDbPath) {
        Remove-Item -Path $offlineDbPath -Force -ErrorAction SilentlyContinue
        Write-Host "Reset offline DB: removed $offlineDbPath"
    } else {
        Write-Host "Reset offline DB: file not found (skip)"
    }
}

function Invoke-BuildWithRetry {
    param(
        [string]$ProjectPath,
        [string]$BuildLogPath
    )

    $projectDir = Split-Path -Parent $ProjectPath
    for ($attempt = 1; $attempt -le 3; $attempt++) {
        dotnet build-server shutdown *> $null
        dotnet build $ProjectPath /nodeReuse:false *> $BuildLogPath
        if ($LASTEXITCODE -eq 0) {
            return $true
        }

        $log = Get-Content $BuildLogPath -Raw -ErrorAction SilentlyContinue
        $isLockedFile = $log -match "being used by another process" -or $log -match "rjsmrazor\.dswa\.cache\.json"
        if (-not $isLockedFile) {
            return $false
        }

        Remove-Item -Path (Join-Path $projectDir "obj"), (Join-Path $projectDir "bin") -Recurse -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }

    return $false
}

function Get-LatestDllPath {
    param(
        [string]$ProjectBinDebugDir,
        [string]$DllName
    )

    return Get-ChildItem -Path $ProjectBinDebugDir -Recurse -Filter $DllName -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}

function Test-RunnableDll {
    param([string]$DllPath)

    if (-not $DllPath) { return $false }
    if (-not (Test-Path $DllPath)) { return $false }

    $runtimeConfig = [System.IO.Path]::ChangeExtension($DllPath, ".runtimeconfig.json")
    return (Test-Path $runtimeConfig)
}

Write-Host "Mode: $Mode ($dbProvider)"
if (-not $SkipBuild) {
    Write-Host "Building API..."
    if (-not (Invoke-BuildWithRetry -ProjectPath "HeThongThuyetMinhDuLich.Api\HeThongThuyetMinhDuLich.Api.csproj" -BuildLogPath ".\api.build.log")) {
        Write-Host "API build failed. Last log lines:"
        Get-Content ".\api.build.log" -Tail 120
        exit 1
    }

    Write-Host "Building CMS..."
    if (-not (Invoke-BuildWithRetry -ProjectPath "HeThongThuyetMinhDuLich.Cms\HeThongThuyetMinhDuLich.Cms.csproj" -BuildLogPath ".\cms.build.log")) {
        Write-Host "CMS build failed. Last log lines:"
        Get-Content ".\cms.build.log" -Tail 120
        exit 1
    }
} else {
    Write-Host "Skip build: using existing binaries."
}

Write-Host "Starting API at http://localhost:5000 ..."
$api = $null
$apiProjectDir = Join-Path (Get-Location) "HeThongThuyetMinhDuLich.Api"
$apiBinDebug = Join-Path (Get-Location) "HeThongThuyetMinhDuLich.Api\bin\Debug"
$apiDll = Get-LatestDllPath -ProjectBinDebugDir $apiBinDebug -DllName "HeThongThuyetMinhDuLich.Api.dll"
$apiProjectPath = "HeThongThuyetMinhDuLich.Api\HeThongThuyetMinhDuLich.Api.csproj"
$existingApiPid = Get-ListeningPid -Port 5000
$apiReused = $false
if ($existingApiPid) {
    try {
        $apiStatus = (Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -UseBasicParsing -TimeoutSec 4).StatusCode
        if ($apiStatus -ge 200 -and $apiStatus -lt 400) {
            $apiReused = $true
            Write-Host "API already running on :5000 (PID: $existingApiPid). Reusing existing process."
        }
    } catch {}
}

if (-not $apiReused) {
    if (-not (Test-RunnableDll -DllPath $apiDll)) {
        Write-Host "API binaries are incomplete. Building API..."
        if (-not (Invoke-BuildWithRetry -ProjectPath $apiProjectPath -BuildLogPath ".\api.build.log")) {
            Write-Host "API build failed. Last log lines:"
            Get-Content ".\api.build.log" -Tail 120
            exit 1
        }
        $apiDll = Get-LatestDllPath -ProjectBinDebugDir $apiBinDebug -DllName "HeThongThuyetMinhDuLich.Api.dll"
        if (-not (Test-RunnableDll -DllPath $apiDll)) {
            Write-Host "API build completed but runnable binaries still missing."
            exit 1
        }
    }
    $api = Start-Process -FilePath "dotnet" `
        -ArgumentList "exec `"$apiDll`" --urls http://localhost:5000 --contentRoot `"$apiProjectDir`"" `
        -WorkingDirectory (Get-Location) `
        -RedirectStandardOutput ".\api.run.log" `
        -RedirectStandardError ".\api.run.err.log" `
        -PassThru
}

Start-Sleep -Seconds 3

Write-Host "Starting CMS at $cmsUrl ..."
$cmsProjectDir = Join-Path (Get-Location) "HeThongThuyetMinhDuLich.Cms"
$cmsBinDebug = Join-Path (Get-Location) "HeThongThuyetMinhDuLich.Cms\bin\Debug"
$cmsDll = Get-LatestDllPath -ProjectBinDebugDir $cmsBinDebug -DllName "HeThongThuyetMinhDuLich.Cms.dll"
$cmsProjectPath = "HeThongThuyetMinhDuLich.Cms\HeThongThuyetMinhDuLich.Cms.csproj"
if (-not (Test-RunnableDll -DllPath $cmsDll)) {
    Write-Host "CMS binaries are incomplete. Building CMS..."
    if (-not (Invoke-BuildWithRetry -ProjectPath $cmsProjectPath -BuildLogPath ".\cms.build.log")) {
        Write-Host "CMS build failed. Last log lines:"
        Get-Content ".\cms.build.log" -Tail 120
        exit 1
    }
    $cmsDll = Get-LatestDllPath -ProjectBinDebugDir $cmsBinDebug -DllName "HeThongThuyetMinhDuLich.Cms.dll"
    if (-not (Test-RunnableDll -DllPath $cmsDll)) {
        Write-Host "CMS build completed but runnable binaries still missing."
        exit 1
    }
}
$cms = Start-Process -FilePath "dotnet" `
    -ArgumentList "exec `"$cmsDll`" --urls $cmsUrl --contentRoot `"$cmsProjectDir`"" `
    -WorkingDirectory (Get-Location) `
    -RedirectStandardOutput ".\cms.run.log" `
    -RedirectStandardError ".\cms.run.err.log" `
    -PassThru

$apiOk = $apiReused
$cmsOk = $false
for ($i = 0; $i -lt 10 -and -not ($apiOk -and $cmsOk); $i++) {
    Start-Sleep -Seconds 2
    if (-not $apiOk) {
        try {
            $apiStatus = (Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -UseBasicParsing -TimeoutSec 5).StatusCode
            if ($apiStatus -ge 200 -and $apiStatus -lt 400) { $apiOk = $true }
        } catch {}
    }
    if (-not $cmsOk) {
        try {
            $cmsStatus = (Invoke-WebRequest -Uri $cmsUrl -UseBasicParsing -TimeoutSec 5).StatusCode
            if ($cmsStatus -ge 200 -and $cmsStatus -lt 400) { $cmsOk = $true }
        } catch {}
    }
}

Write-Host ""
Write-Host "Run complete."
Write-Host " - Database Mode: $Mode ($dbProvider)"
Write-Host " - API PID: $(if($apiReused){$existingApiPid}else{$api.Id})"
Write-Host " - CMS PID: $($cms.Id)"
Write-Host " - API: $(if($apiOk){'OK'}else{'FAIL'})  http://localhost:5000/swagger/index.html"
Write-Host " - CMS: $(if($cmsOk){'OK'}else{'FAIL'})  $cmsUrl"

if ($apiOk) {
    try {
        $poiCount = (Invoke-RestMethod -Uri "http://localhost:5000/api/diemthamquan" -Method Get -TimeoutSec 8).Count
        Write-Host " - API Data (DiemThamQuan): $poiCount"
        if ($poiCount -eq 0) {
            Write-Host "   WARNING: DB dang trong. Chay lai voi -ResetOfflineDb de seed lai du lieu mau."
        }
    } catch {
        Write-Host " - API Data check: FAIL"
    }
}

if (-not $apiOk -or -not $cmsOk) {
    Write-Host ""
    Write-Host "=== ERROR LOGS ==="
    if (-not $apiOk -and (Test-Path ".\api.run.err.log")) {
        Write-Host "--- api.run.err.log ---"
        Get-Content ".\api.run.err.log" -Tail 120
    }
    if (-not $apiOk -and (Test-Path ".\api.run.log")) {
        Write-Host "--- api.run.log ---"
        Get-Content ".\api.run.log" -Tail 120
    }
    if (Test-Path ".\cms.run.err.log") {
        Write-Host "--- cms.run.err.log ---"
        Get-Content ".\cms.run.err.log" -Tail 120
    }
    if (-not $cmsOk -and (Test-Path ".\cms.run.log")) {
        Write-Host "--- cms.run.log ---"
        Get-Content ".\cms.run.log" -Tail 120
    }
}
