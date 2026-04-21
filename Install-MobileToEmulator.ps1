param(
    [string]$AdbPath = "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe",
    [string]$DeviceId = "emulator-5554",
    [switch]$UseUnsignedApk,
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $AdbPath)) {
    throw "Khong tim thay adb tai: $AdbPath"
}

$apkName = if ($UseUnsignedApk) {
    "com.companyname.hethongthuyetminhdulich.mobile.apk"
} else {
    "com.companyname.hethongthuyetminhdulich.mobile-Signed.apk"
}

if ($Configuration -eq "Release") {
    $candidateApkPaths = @(
        (Join-Path $PSScriptRoot "HeThongThuyetMinhDuLich.Mobile\bin\Release\net10.0-android\publish\$apkName"),
        (Join-Path $PSScriptRoot "HeThongThuyetMinhDuLich.Mobile\bin\Release\net10.0-android\$apkName"),
        (Join-Path $PSScriptRoot "HeThongThuyetMinhDuLich.Mobile\bin\Release\net10.0-android\publish\com.companyname.hethongthuyetminhdulich.mobile.apk"),
        (Join-Path $PSScriptRoot "HeThongThuyetMinhDuLich.Mobile\bin\Release\net10.0-android\com.companyname.hethongthuyetminhdulich.mobile.apk")
    )
} else {
    $candidateApkPaths = @(
        (Join-Path $PSScriptRoot "HeThongThuyetMinhDuLich.Mobile\bin\Debug\net10.0-android\android-x64\$apkName"),
        (Join-Path $PSScriptRoot "HeThongThuyetMinhDuLich.Mobile\bin\Debug\net10.0-android\android-x64\com.companyname.hethongthuyetminhdulich.mobile.apk"),
        (Join-Path $PSScriptRoot "HeThongThuyetMinhDuLich.Mobile\bin\Debug\net10.0-android\$apkName"),
        (Join-Path $PSScriptRoot "HeThongThuyetMinhDuLich.Mobile\bin\Debug\net10.0-android\com.companyname.hethongthuyetminhdulich.mobile.apk")
    )
}

$apkPath = $candidateApkPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $apkPath) {
    throw "Khong tim thay APK. Da tim trong: $($candidateApkPaths -join '; ')"
}

$deviceLines = & $AdbPath devices
$matchedDevice = $deviceLines |
    Where-Object { $_ -match '^\S+\s+device$' } |
    ForEach-Object { ($_ -split '\s+')[0] } |
    Where-Object { $_ -eq $DeviceId } |
    Select-Object -First 1

if (-not $matchedDevice) {
    throw "Khong tim thay device/emulator $DeviceId. Hay mo emulator truoc."
}

Write-Host "Installing APK to $DeviceId ..."
& $AdbPath -s $DeviceId install -r $apkPath

if ($LASTEXITCODE -ne 0) {
    throw "adb install that bai voi exit code $LASTEXITCODE"
}

Write-Host "Install thanh cong: $apkPath"
