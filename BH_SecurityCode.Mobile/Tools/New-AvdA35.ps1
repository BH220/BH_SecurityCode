<#
.SYNOPSIS
    갤럭시 A35 사양에 맞춘 Android 에뮬레이터(AVD)를 생성한다. 최초 1회만 실행하면 된다.

.DESCRIPTION
    A35 5G 실제 사양에 맞춰 해상도 1080x2340 / 450dpi / 4GB RAM 으로 만든다.
    논리 화면 크기는 384 x 832 dp 로 실기기와 동일해진다.
    PC 에뮬레이터는 x86_64 이미지를 쓰므로 csproj Debug 구성에 android-x64 RID 가 함께 들어가 있다.

.EXAMPLE
    .\Tools\New-AvdA35.ps1
    .\Tools\New-AvdA35.ps1 -Force        # 같은 이름의 AVD 를 지우고 다시 만든다
#>
[CmdletBinding()]
param(
    [string]$Name = 'Galaxy_A35_API35',
    [string]$Image = 'system-images;android-35;google_apis;x86_64',
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$sdkRoot = 'C:\Program Files (x86)\Android\android-sdk'
$jdk = 'C:\Program Files (x86)\Android\openjdk\jdk-17.0.14'
$avdManager = Join-Path $sdkRoot 'cmdline-tools\12.0\bin\avdmanager.bat'
$imageDir = Join-Path $sdkRoot ('system-images\' + ($Image -split ';')[1] + '\' + ($Image -split ';')[2] + '\' + ($Image -split ';')[3])

if (-not (Test-Path $avdManager)) { throw "avdmanager 를 찾을 수 없습니다: $avdManager" }
if (-not (Test-Path $imageDir)) {
    throw @"
시스템 이미지가 없습니다: $Image
아래 명령으로 먼저 설치하세요.
  `$env:JAVA_HOME='$jdk'
  & '$sdkRoot\cmdline-tools\12.0\bin\sdkmanager.bat' --sdk_root='$sdkRoot' '$Image'
"@
}

$env:JAVA_HOME = $jdk
$env:ANDROID_SDK_ROOT = $sdkRoot

$avdHome = Join-Path $env:USERPROFILE '.android\avd'
$configPath = Join-Path $avdHome "$Name.avd\config.ini"

if ((Test-Path $configPath) -and (-not $Force)) {
    Write-Host "AVD '$Name' 이 이미 있습니다. 다시 만들려면 -Force 를 주세요." -ForegroundColor Yellow
    exit 0
}

Write-Host "[1/2] AVD 생성: $Name" -ForegroundColor Cyan
$createArgs = @('create', 'avd', '-n', $Name, '-k', $Image, '--abi', 'x86_64', '-d', 'medium_phone')
if ($Force) { $createArgs += '--force' }
# -d medium_phone 을 주면 하드웨어 프로필 선택 프롬프트가 뜨지 않는다. 세부 사양은 아래에서 덮어쓴다.
& $avdManager @createArgs
if ($LASTEXITCODE -ne 0) { throw "AVD 생성 실패 (exit $LASTEXITCODE)" }

Write-Host '[2/2] A35 사양으로 config.ini 조정' -ForegroundColor Cyan
if (-not (Test-Path $configPath)) { throw "config.ini 를 찾을 수 없습니다: $configPath" }

# 갤럭시 A35 5G: 6.6" 1080x2340 (19.5:9), 450dpi → 384 x 832 dp
$settings = [ordered]@{
    'avd.ini.displayname'      = 'Galaxy A35 (API 35)'
    'hw.lcd.width'             = '1080'
    'hw.lcd.height'            = '2340'
    'hw.lcd.density'           = '450'
    'hw.ramSize'               = '4096'
    'vm.heapSize'              = '512'
    'hw.cpu.ncore'             = '4'
    'disk.dataPartition.size'  = '8G'
    'hw.keyboard'              = 'yes'
    'hw.gpu.enabled'           = 'yes'
    'hw.gpu.mode'              = 'host'
    'hw.initialOrientation'    = 'portrait'
    'showDeviceFrame'          = 'no'
    'skin.dynamic'             = 'yes'
    'skin.name'                = '1080x2340'
    'hw.camera.back'           = 'virtualscene'
    'hw.camera.front'          = 'emulated'
}

$config = @{}
foreach ($line in (Get-Content $configPath)) {
    if ($line -match '^\s*([^=#]+?)\s*=\s*(.*)$') { $config[$Matches[1]] = $Matches[2] }
}
foreach ($key in $settings.Keys) { $config[$key] = $settings[$key] }
$config.Remove('skin.path')
$config.Remove('skin.path.backup')

$out = foreach ($key in ($config.Keys | Sort-Object)) { "$key=$($config[$key])" }
Set-Content -Path $configPath -Value $out -Encoding ascii

Write-Host ''
Write-Host "완료: $Name" -ForegroundColor Green
Write-Host '  해상도 : 1080 x 2340 @ 450dpi (= 384 x 832 dp, 실기기와 동일)'
Write-Host '  RAM    : 4096MB / CPU 4코어 / 데이터 8GB'
Write-Host ''
Write-Host '실행: .\Tools\Start-Avd.ps1'
