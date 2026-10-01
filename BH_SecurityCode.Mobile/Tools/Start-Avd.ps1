<#
.SYNOPSIS
    A35 규격 AVD 를 부팅한다. 이미 켜져 있으면 그대로 둔다.

.EXAMPLE
    .\Tools\Start-Avd.ps1
    .\Tools\Start-Avd.ps1 -Wipe          # 데이터 초기화 후 부팅
    .\Tools\Start-Avd.ps1 -TimeoutSec 300
#>
[CmdletBinding()]
param(
    [string]$Name = 'Galaxy_A35_API35',
    [switch]$Wipe,
    [int]$TimeoutSec = 180
)

$ErrorActionPreference = 'Stop'

$sdkRoot = 'C:\Program Files (x86)\Android\android-sdk'
$emulator = Join-Path $sdkRoot 'emulator\emulator.exe'
$adb = Join-Path $sdkRoot 'platform-tools\adb.exe'
$avdConfig = Join-Path $env:USERPROFILE ".android\avd\$Name.avd\config.ini"

if (-not (Test-Path $emulator)) { throw "에뮬레이터가 설치되지 않았습니다: $emulator" }
if (-not (Test-Path $avdConfig)) { throw "AVD '$Name' 이 없습니다. 먼저 .\Tools\New-AvdA35.ps1 을 실행하세요." }

$env:ANDROID_SDK_ROOT = $sdkRoot

# 이미 부팅된 에뮬레이터가 있는지 확인
& $adb start-server | Out-Null
$running = & $adb devices | Select-String '^emulator-\d+\s+device' | ForEach-Object { ($_ -split '\s+')[0] }
if ($running) {
    Write-Host "이미 실행 중인 에뮬레이터: $($running -join ', ')" -ForegroundColor Green
    exit 0
}


# 하드웨어 가속(WHPX/AEHD) 사용 가능 여부 확인 — 없으면 부팅이 사실상 불가능하다
$accelCheck = Join-Path (Split-Path $emulator) 'emulator-check.exe'
if (Test-Path $accelCheck) {
    $accelOut = & $accelCheck accel 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Warning '하드웨어 가속을 사용할 수 없습니다. 에뮬레이터가 매우 느리거나 부팅되지 않습니다.'
        Write-Host '  관리자 PowerShell 에서 아래 실행 후 재부팅하세요.' -ForegroundColor Yellow
        Write-Host '  Enable-WindowsOptionalFeature -Online -FeatureName HypervisorPlatform -All' -ForegroundColor Yellow
        Write-Host ('  진단: ' + ($accelOut -join ' ')) -ForegroundColor DarkGray
        Write-Host ''
    }
}

Write-Host "에뮬레이터 부팅: $Name" -ForegroundColor Cyan
$emuArgs = @('-avd', $Name, '-gpu', 'host', '-no-boot-anim', '-netdelay', 'none', '-netspeed', 'full')
if ($Wipe) { $emuArgs += '-wipe-data' }
Start-Process -FilePath $emulator -ArgumentList $emuArgs -WorkingDirectory (Split-Path $emulator)

Write-Host "부팅 대기 (최대 ${TimeoutSec}초)..." -NoNewline
$deadline = (Get-Date).AddSeconds($TimeoutSec)
$booted = $false
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 3
    Write-Host '.' -NoNewline
    $serial = & $adb devices | Select-String '^emulator-\d+\s+device' | ForEach-Object { ($_ -split '\s+')[0] } | Select-Object -First 1
    if ($serial) {
        $prop = (& $adb -s $serial shell getprop sys.boot_completed 2>$null)
        if ("$prop".Trim() -eq '1') { $booted = $true; break }
    }
}
Write-Host ''

if (-not $booted) {
    Write-Warning "제한 시간 안에 부팅이 끝나지 않았습니다. 에뮬레이터 창의 오류 메시지를 확인하세요."
    exit 1
}

$serial = & $adb devices | Select-String '^emulator-\d+\s+device' | ForEach-Object { ($_ -split '\s+')[0] } | Select-Object -First 1
$size = (& $adb -s $serial shell wm size) -join ' '
$density = (& $adb -s $serial shell wm density) -join ' '
Write-Host "부팅 완료: $serial" -ForegroundColor Green
Write-Host "  $size / $density"
Write-Host ''
Write-Host '배포: .\Tools\Deploy-Android.ps1'
