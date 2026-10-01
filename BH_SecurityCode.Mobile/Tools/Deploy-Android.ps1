<#
.SYNOPSIS
    연결된 안드로이드 기기(실기기 또는 A35 규격 에뮬레이터)에 앱을 빌드·배포·실행한다.

.DESCRIPTION
    1) adb 로 연결 상태를 확인한다. -StartEmulator 를 주면 AVD 를 먼저 부팅한다.
    2) dotnet build -t:Run 으로 APK 를 빌드하고 설치한 뒤 실행한다.
    3) -Logcat 을 주면 앱 프로세스의 로그를 이어서 출력한다.

.EXAMPLE
    .\Tools\Deploy-Android.ps1 -StartEmulator          # 에뮬레이터 부팅 + 배포
    .\Tools\Deploy-Android.ps1 -Logcat                 # 이미 켜진 기기에 배포 + 로그
    .\Tools\Deploy-Android.ps1 -Configuration Release
    .\Tools\Deploy-Android.ps1 -Serial R3CX90ABCDE     # 기기가 여러 대일 때
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    # 대상 기기 시리얼. 생략하면 연결된 기기가 1대일 때만 자동 선택한다.
    [string]$Serial = '',

    # 배포 전에 A35 규격 AVD 를 부팅한다.
    [switch]$StartEmulator,

    # 배포 후 logcat 을 따라간다.
    [switch]$Logcat,

    # 배포 없이 연결 상태만 점검한다.
    [switch]$CheckOnly,

    # 기기의 localhost:<포트> 를 PC 의 같은 포트로 넘긴다. (BHS_Api 개발 서버가 127.0.0.1 로만 열려 있어서 필요)
    # 0 을 주면 하지 않는다.
    [int]$ReversePort = 8169
)

$ErrorActionPreference = 'Stop'

$projectDir = Split-Path -Parent $PSScriptRoot
$project = Join-Path $projectDir 'BH_SecurityCode.Mobile.csproj'
$framework = 'net9.0-android35.0'
$packageId = 'com.bhsoft.securitycode'
$sdkRoot = 'C:\Program Files (x86)\Android\android-sdk'
$adb = Join-Path $sdkRoot 'platform-tools\adb.exe'

if (-not (Test-Path $adb)) { throw "adb 를 찾을 수 없습니다: $adb" }

# --- 1. 연결 확인 ---------------------------------------------------------
Write-Host '[1/3] 기기 연결 확인' -ForegroundColor Cyan

if ($StartEmulator) {
    & (Join-Path $PSScriptRoot 'Start-Avd.ps1')
    if ($LASTEXITCODE -ne 0) { throw '에뮬레이터 부팅 실패' }
}

& $adb start-server | Out-Null

$lines = & $adb devices | Select-Object -Skip 1 | Where-Object { $_.Trim().Length -gt 0 }
$devices = @()
$unauthorized = @()
foreach ($line in $lines) {
    $parts = $line -split '\s+'
    if ($parts.Count -lt 2) { continue }
    if ($parts[1] -eq 'device') { $devices += $parts[0] }
    elseif ($parts[1] -eq 'unauthorized') { $unauthorized += $parts[0] }
}

if ($unauthorized.Count -gt 0) {
    Write-Warning "USB 디버깅 승인 대기 중: $($unauthorized -join ', ')"
    Write-Host  "  → 휴대전화 화면의 'USB 디버깅을 허용하시겠습니까?' 에서 [이 컴퓨터에서 항상 허용] 체크 후 [허용]" -ForegroundColor Yellow
}

if ($devices.Count -eq 0) {
    Write-Host ''
    Write-Host '연결된 기기가 없습니다.' -ForegroundColor Red
    Write-Host ''
    Write-Host '에뮬레이터로 진행하려면:' -ForegroundColor Yellow
    Write-Host '  .\Tools\New-AvdA35.ps1      # 최초 1회 AVD 생성'
    Write-Host '  .\Tools\Deploy-Android.ps1 -StartEmulator'
    Write-Host ''
    Write-Host '실기기로 진행하려면 휴대전화에서 아래를 확인하세요.' -ForegroundColor Yellow
    Write-Host '  1) 설정 > 휴대전화 정보 > 소프트웨어 정보 > [빌드번호] 7번 탭 → 개발자 옵션 활성화'
    Write-Host '  2) 설정 > 개발자 옵션 > [USB 디버깅] 켜기'
    Write-Host '  3) USB 케이블 연결 후 알림창의 USB 설정을 [파일 전송 / Android Auto] 로 변경'
    Write-Host '  4) 데이터 전송을 지원하는 케이블/포트 사용 (충전 전용 케이블은 인식되지 않음)'
    exit 1
}

if ($Serial -eq '') {
    if ($devices.Count -gt 1) {
        throw "기기가 여러 대 연결되어 있습니다($($devices -join ', ')). -Serial 로 하나를 지정하세요."
    }
    $Serial = $devices[0]
}
elseif ($devices -notcontains $Serial) {
    throw "지정한 기기를 찾을 수 없습니다: $Serial"
}

$model = (& $adb -s $Serial shell getprop ro.product.model).Trim()
$release = (& $adb -s $Serial shell getprop ro.build.version.release).Trim()
$sdk = (& $adb -s $Serial shell getprop ro.build.version.sdk).Trim()
$abi = (& $adb -s $Serial shell getprop ro.product.cpu.abi).Trim()
$density = (& $adb -s $Serial shell wm density) -join ' '
$size = (& $adb -s $Serial shell wm size) -join ' '
Write-Host "  기기   : $model ($Serial)"
Write-Host "  OS     : Android $release (API $sdk)"
Write-Host "  ABI    : $abi"
Write-Host "  화면   : $size / $density"

if ([int]$sdk -lt 34) {
    Write-Warning "이 앱의 최소 지원 버전은 API 34(Android 14)입니다. 이 기기(API $sdk)에는 설치되지 않습니다."
}
if ($Configuration -eq 'Release' -and $abi -ne 'arm64-v8a') {
    Write-Warning "Release 구성은 arm64-v8a 전용입니다. 이 기기의 ABI 는 $abi 이므로 Debug 로 배포하세요."
}

# PC 에서 도는 API 서버를 기기에서 127.0.0.1 로 부를 수 있게 한다.
if ($ReversePort -gt 0) {
    & $adb -s $Serial reverse "tcp:$ReversePort" "tcp:$ReversePort" | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  포워딩 : 기기 127.0.0.1:$ReversePort → PC 127.0.0.1:$ReversePort"
    }
    else {
        Write-Warning "adb reverse 실패 (포트 $ReversePort). PC 의 API 서버를 기기에서 부를 수 없습니다."
    }
}

if ($CheckOnly) { exit 0 }

# --- 2. 빌드 + 배포 + 실행 ------------------------------------------------
Write-Host "[2/3] 빌드 · 설치 · 실행 ($Configuration)" -ForegroundColor Cyan
& dotnet build $project -c $Configuration -f $framework -t:Run "-p:AdbTarget=-s $Serial"
if ($LASTEXITCODE -ne 0) { throw "배포 실패 (exit $LASTEXITCODE)" }

# --- 3. 로그 ---------------------------------------------------------------
if (-not $Logcat) {
    Write-Host '[3/3] 완료. 로그를 보려면 -Logcat 을 붙여 다시 실행하세요.' -ForegroundColor Green
    exit 0
}

Write-Host '[3/3] logcat (Ctrl+C 로 종료)' -ForegroundColor Cyan
$pidText = (& $adb -s $Serial shell pidof $packageId).Trim()
if ($pidText -eq '') {
    Write-Warning '앱 프로세스를 찾지 못해 전체 로그를 출력합니다.'
    & $adb -s $Serial logcat -v time
}
else {
    & $adb -s $Serial logcat -v time --pid $pidText
}
