# BH_SecurityCode

보안카드 · 통장 · 카드 · 신분증 · 계정 정보를 통합 관리하는 Windows 데스크톱 프로그램.
기존 WinForms(.NET Framework 4.7, DB 직접 접근) 프로젝트를 **WPF(.NET 8) + CommunityToolkit.Mvvm** 으로 마이그레이션한 버전이다.

## 솔루션 구성

| 프로젝트 | 역할 |
|---|---|
| `BH_SecurityCode.Core` | 공통 모델(`Models/`), Enum, 메뉴/기능 상수(`Constants/`), 세션, 설정, 로그 |
| `BH_SecurityCode.Api` | 서버 API 클라이언트(`ScApi`), 응답 모델(`Model/Response`), 도메인별 매니저(`Manager/`). **로그인 외 매니저는 API 연동 전 스텁(빈 구현)** |
| `BH_SecurityCode` | WPF 메인 애플리케이션 (MVVM, 테마 L1 "페이퍼 인디고" — `Themes/Colors.xaml` 토큰만 바꾸면 팔레트 교체 가능) |
| `BH_SecurityCode.LogStore` | 로그 저장 라이브러리 |
| `BH_SecurityCode.Installer` / `Launcher` / `Uninstaller` | 설치/실행/제거 도구 (스켈레톤) |

## WPF 프로젝트 구조 (`BH_SecurityCode/`)

```
App.xaml(.cs)        DI 컨테이너 구성(Ioc.Default), 단일 실행, 전역 예외 처리
Themes/              테마 (Colors = 색상 토큰 / Controls / DataGrid)
Converters/          값 변환기 + 리소스 사전
Helpers/             PasswordBox 바인딩, 포커스, DialogResult, DataGrid 더블클릭 첨부 속성
Services/            IDialogService(메시지/모달/파일 선택), IClipboardService
Messages/            Messenger 메시지 (로그인 성공, 사용자 활동, 상태 텍스트, 종료 요청)
ViewModels/
  Base/              ViewModelBase, DialogViewModelBase, ListViewModelBase<T>, EditViewModelBase, FunctionButtonState
  MainViewModel      메뉴 / 기능 버튼 / 상태바 / 자동 잠금 타이머 (기존 frmMain)
  LoginViewModel     로그인 오버레이 (기존 ctlLogin)
  BankCode/ BankBook/ Card/ IdCard/ Site/   목록·입력·보기 ViewModel
  Common/            이미지 첨부(ImageGridViewModel), 메시지 박스
Views/               ViewModel 과 1:1 대응하는 View (코드비하인드는 InitializeComponent 만)
```

- 목록 화면은 `ContentControl` + `ViewTemplates.xaml`(DataTemplate) 로 전환된다.
- 모달 창은 `DialogService` 의 ViewModel→Window 매핑으로 열리며, `DialogBehavior.DialogResult` 첨부 속성으로 닫힌다.
- 상단 기능 버튼(추가/수정/삭제/갱신/검색/…)은 `IFunctionHost.RunFunctionAsync(functionId)` 로 현재 화면에 전달된다.

## 단축키

| 키 | 동작 |
|---|---|
| Ctrl+Shift+S / B / C / I / X | 보안코드 / 통장 / 카드 / 신분증 / 계정 |
| Ins / F8 / Del / F7 | 추가 / 수정 / 삭제 / 복사 |
| F5 / F6 / F11 / F12 | 갱신 / 검색 / 엑셀 / 인쇄 |
| F1 / F2 | 보안카드 간편보기 / 상세보기 |
| ESC (2회) | 현재 화면 닫기 |
| Alt+R / Alt+L / Alt+O | 시간 연장 / 화면 잠금 / 로그아웃 |

## API 연동 (TODO)

애플리케이션은 DB에 직접 접근하지 않는다. 모든 데이터는 `BH_SecurityCode.Api/Manager/*Manager.cs` 를 통해 가져오며,
현재 로그인(`AuthManager`) 외 매니저는 `// TODO: [API 연동]` 주석과 함께 빈 결과를 반환하는 스텁이다. 응답 모델은 `Model/Response` (`Res{항목}List`, `{항목}Info` 등)에 준비되어 있다.

| 매니저 | 대체할 API (예시) |
|---|---|
| `AuthManager` | 연동 완료 — `/api/auth/login`, `/login/force`, `/login/abort`, `/logout` |
| `CodeManager` | `/api/code/list` |
| `BankCodeManager` | `/api/bankcode/list, get, detail/list, insert, update, delete` |
| `BankBookManager` | `/api/bankbook/...` |
| `CardManager` | `/api/card/...` |
| `IdCardManager` | `/api/idcard/...` |
| `SiteManager` | `/api/site/...`, `/api/account/...` |


## 빌드

```
dotnet build BH_SecurityCode.sln
```

- .NET 8 SDK 필요 (`net8.0-windows`)
- 로그: `C:\Bh Soft\logs\BH Security Code\BH_SecurityCode\`
- 로그인: BHS_Api 실서버 인증 (`POST /api/auth/login`, 중복 세션이면 확인 후 `/login/force`). 우하단 ⚙ 에서 서버 주소 설정.
