param(
    [string]$AdbPath = "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe",
    [string]$DeviceId = "emulator-5554",
    [switch]$UseUnsignedApk
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $AdbPath)) {
    throw "Khong tim thay adb tai: $AdbPath"
}

$apkDir = Join-Path $PSScriptRoot "HeThongThuyetMinhDuLich.Mobile\bin\Debug\net10.0-android\android-x64"
if (-not (Test-Path $apkDir)) {
    throw "Khong tim thay thu muc APK: $apkDir"
}

$apkName = if ($UseUnsignedApk) {
    "com.companyname.hethongthuyetminhdulich.mobile.apk"
} else {
    "com.companyname.hethongthuyetminhdulich.mobile-Signed.apk"
}

$apkPath = Join-Path $apkDir $apkName
if (-not (Test-Path $apkPath)) {
    throw "Khong tim thay APK: $apkPath"
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