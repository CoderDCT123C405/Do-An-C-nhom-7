param(
    [string]$ApiBaseUrl = "http://localhost:5000",
    [string]$TenDangNhap = "admin",
    [string]$MatKhau = "Admin@123",
    [string]$BearerToken,
    [switch]$SkipExisting,
    [int]$TimeoutSec = 1800
)

$ErrorActionPreference = "Stop"

function Normalize-BaseUrl {
    param([string]$Value)

    return $Value.Trim().TrimEnd('/')
}

function Get-AccessToken {
    param(
        [string]$BaseUrl,
        [string]$Username,
        [string]$Password,
        [int]$RequestTimeoutSec
    )

    $loginUri = "{0}/api/auth/admin/login" -f $BaseUrl
    $loginBody = @{
        TenDangNhap = $Username
        MatKhau     = $Password
    } | ConvertTo-Json

    $loginParams = @{
        Method      = "Post"
        Uri         = $loginUri
        ContentType = "application/json"
        Body        = $loginBody
        TimeoutSec  = $RequestTimeoutSec
    }

    $response = Invoke-RestMethod @loginParams

    if ([string]::IsNullOrWhiteSpace($response.Token)) {
        throw "Dang nhap API that bai, khong nhan duoc access token."
    }

    return $response.Token
}

$baseUrl = Normalize-BaseUrl -Value $ApiBaseUrl
$effectiveToken = $BearerToken

if ([string]::IsNullOrWhiteSpace($effectiveToken)) {
    $effectiveToken = Get-AccessToken -BaseUrl $baseUrl -Username $TenDangNhap -Password $MatKhau -RequestTimeoutSec ([Math]::Min($TimeoutSec, 60))
}

$overwriteValue = if ($SkipExisting) { "false" } else { "true" }
$modeLabel = if ($SkipExisting) { "skip-existing" } else { "overwrite" }
$headers = @{ Authorization = "Bearer $effectiveToken" }
$generateUri = "{0}/api/noidungthuyetminh/generate-audio?overwrite={1}" -f $baseUrl, $overwriteValue

Write-Host "API: $baseUrl"
Write-Host "Mode: $modeLabel"
Write-Host "Calling: $generateUri"

$generateParams = @{
    Method     = "Post"
    Uri        = $generateUri
    Headers    = $headers
    TimeoutSec = $TimeoutSec
}

$result = Invoke-RestMethod @generateParams

Write-Host "Tong noi dung: $($result.TongNoiDung)"
Write-Host "Da sinh audio: $($result.DaSinhAudio)"

if ($SkipExisting) {
    Write-Host "Da bo qua cac noi dung da co audio, khong tao trung file."
}
else {
    Write-Host "Da tao lai audio theo che do overwrite, audio cu se duoc thay the khong bi nhan ban."
}