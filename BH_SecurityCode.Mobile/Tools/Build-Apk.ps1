<#
.SYNOPSIS
    USB 로 옮겨서 설치할 수 있는 단독 APK 를 만든다.

.DESCRIPTION
    Debug APK 는 .NET 어셈블리를 기기에 따로 밀어넣는 방식(fast deployment)이라
    파일만 복사해서 설치하면 실행되지 않는다. 그래서 Release 로 묶는다.

    Release 는 csproj 설정대로 AOT + SdkOnly 링크가 적용되고, 서명 키를 지정하지 않았으므로
    개발용(디버그) 키로 서명된다. 사내 배포/테스트용으로는 그대로 쓰면 되고,
    스토어에 올릴 때는 별도 키스토어를 지정해야 한다.

.EXAMPLE
    .\Tools\Build-Apk.ps1                 # arm64 (갤럭시 A35 등 실기기)
    .\Tools\Build-Apk.ps1 -Abi x64        # x86_64 (에뮬레이터 검증용)
    .\Tools\Build-Apk.ps1 -Install        # 만든 뒤 연결된 기기에 바로 설치
#>
[CmdletBinding()]
param(
    [ValidateSet('arm64', 'x64')]
    [string]$Abi = 'arm64',

    # 만든 APK 를 연결된 기기에 바로 설치한다.
    [switch]$Install
)

$ErrorActionPreference = 'Stop'

$projectDir = Split-Path -Parent $PSScriptRoot
$project = Join-Path $projectDir 'BH_SecurityCode.Mobile.csproj'
$framework = 'net9.0-android35.0'
$rid = "android-$Abi"
$outDir = Join-Path $projectDir 'apk'
$sdkRoot = 'C:\Program Files (x86)\Android\android-sdk'
$adb = Join-Path $sdkRoot 'platform-tools\adb.exe'

Write-Host "[1/3] Release APK 빌드 ($rid)" -ForegroundColor Cyan
Write-Host '  AOT 컴파일이라 몇 분 걸립니다.'

& dotnet publish $project -c Release -f $framework `
    "-p:AndroidPackageFormat=apk" "-p:RuntimeIdentifier=$rid" "-p:RuntimeIdentifiers=$rid"
if ($LASTEXITCODE -ne 0) { throw "빌드 실패 (exit $LASTEXITCODE)" }

Write-Host '[2/3] 산출물 정리' -ForegroundColor Cyan
$candidates = Get-ChildItem (Join-Path $projectDir 'bin\Release') -Recurse -Filter '*-Signed.apk' -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -like "*$rid*" -or $_.FullName -notlike '*android-*' } |
    Sort-Object LastWriteTime -Descending

if (-not $candidates) { throw 'APK 를 찾지 못했습니다.' }
$apk = $candidates[0]

if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

# 버전은 csproj 의 ApplicationDisplayVersion 을 읽어 파일명에 넣는다.
[xml]$csproj = Get-Content $project
$version = ($csproj.Project.PropertyGroup.ApplicationDisplayVersion | Where-Object { $_ }) -join ''
if (-not $version) { $version = '1.0.0' }

$target = Join-Path $outDir ("BH_SecurityCode_{0}_{1}.apk" -f $version, $Abi)
Copy-Item $apk.FullName $target -Force

$sizeMb = [math]::Round((Get-Item $target).Length / 1MB, 1)
Write-Host ''
Write-Host "완료: $target" -ForegroundColor Green
Write-Host ("  크기 : {0} MB / ABI : {1} / 서명 : 개발용 키" -f $sizeMb, $rid)
Write-Host ''

if (-not $Install) {
    Write-Host '기기에 옮겨 설치하려면:'
    Write-Host '  1) USB 로 폰에 복사 → 파일 앱에서 APK 실행'
    Write-Host '  2) "출처를 알 수 없는 앱 설치" 를 허용해야 합니다.'
    Write-Host '  3) 앱 실행 후 로그인 화면 아래 [API 서버] 에서 서버 주소를 넣으세요.'
    exit 0
}

Write-Host '[3/3] 연결된 기기에 설치' -ForegroundColor Cyan
if (-not (Test-Path $adb)) { throw "adb 를 찾을 수 없습니다: $adb" }

$devices = & $adb devices | Select-Object -Skip 1 | Where-Object { $_ -match '\sdevice$' }
if (-not $devices) { throw '연결된 기기가 없습니다.' }

& $adb install -r $target
if ($LASTEXITCODE -ne 0) { throw "설치 실패 (exit $LASTEXITCODE)" }
Write-Host '설치 완료' -ForegroundColor Green
