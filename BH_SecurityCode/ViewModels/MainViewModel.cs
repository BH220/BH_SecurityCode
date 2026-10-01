using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Constants;
using BH_SecurityCode.Core.Helper;
using BH_SecurityCode.Core.Session;
using BH_SecurityCode.Messages;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.BankBook;
using BH_SecurityCode.ViewModels.BankCode;
using BH_SecurityCode.ViewModels.Base;
using BH_SecurityCode.ViewModels.Card;
using BH_SecurityCode.ViewModels.IdCard;
using BH_SecurityCode.ViewModels.Site;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace BH_SecurityCode.ViewModels
{
    /// <summary>
    /// 메인 화면 (기존 frmMain). 좌측 메뉴 / 상단 기능 버튼 / 상태바 / 자동 잠금 타이머 / 로그인 오버레이를 관리한다.
    /// </summary>
    public partial class MainViewModel : ViewModelBase,
        IRecipient<LoginSucceededMessage>,
        IRecipient<UserActivityMessage>,
        IRecipient<StatusTextMessage>,
        IRecipient<ExitRequestedMessage>
    {
        /// <summary>자동 화면잠금 대기시간(초)</summary>
        private const int LockSeconds = 600;

        private readonly IDialogService _dialog;
        private readonly IAuthManager _auth;
        private readonly IServiceProvider _services;
        private readonly DispatcherTimer _clockTimer;
        private readonly DispatcherTimer _lockTimer;
        private int _leftSeconds = LockSeconds;
        private int _escClick;

        /// <summary>true 면 창 닫기 시 확인 없이 종료한다.</summary>
        public bool AllowClose { get; private set; }

        public string WindowTitle => "BH Security Code Management";

        public LoginViewModel Login { get; }
        public ObservableCollection<MenuItemViewModel> Menus { get; }

        [ObservableProperty]
        private bool _isLoggedIn;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasView))]
        private IFunctionHost? _currentView;

        public bool HasView => CurrentView != null;

        [ObservableProperty]
        private FunctionButtonState _buttons = new();

        [ObservableProperty]
        private string _statusText = "";

        [ObservableProperty]
        private string _userInfoText = "";

        [ObservableProperty]
        private string _loginUserText = "";

        [ObservableProperty]
        private string _nowText = "";

        [ObservableProperty]
        private string _continueText = "";

        public MainViewModel(IDialogService dialog, IAuthManager auth, IServiceProvider services, LoginViewModel login)
        {
            _dialog = dialog;
            _auth = auth;
            _services = services;
            Login = login;

            Menus = new ObservableCollection<MenuItemViewModel>
            {
                new(Core.Menus.보안코드, "보안코드", "S", "security_card.png"),
                new(Core.Menus.통장, "통장", "B", "bankbook.png"),
                new(Core.Menus.카드, "카드", "C", "credit_card.png"),
                new(Core.Menus.신분증, "신분증", "I", "id_card.png"),
                new(Core.Menus.계정, "계정", "X", "account.png"),
            };

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (_, _) => NowText = DateTime.Now.ToString("yyyy-MM-dd ddd HH:mm:ss");
            _clockTimer.Start();

            _lockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _lockTimer.Tick += OnLockTimerTick;

            UpdateContinueText();
            WeakReferenceMessenger.Default.RegisterAll(this);

            Login.Reset(clearUserId: true);
        }


        internal void InitializeAsync()
        {
            //InitializeSettings();
            //LoginPopup();
        }

        #region 메뉴 / 기능 실행
        [RelayCommand]
        private async Task SelectMenuAsync(Core.Menus? menuId)
        {
            if (IsLoggedIn == false || menuId == null)
                return;

            _escClick = 0;
            IFunctionHost? view = menuId switch
            {
                Core.Menus.보안코드 => _services.GetRequiredService<BankCodeListViewModel>(),
                Core.Menus.통장 => _services.GetRequiredService<BankBookListViewModel>(),
                Core.Menus.카드 => _services.GetRequiredService<CardListViewModel>(),
                Core.Menus.신분증 => _services.GetRequiredService<IdCardListViewModel>(),
                Core.Menus.계정 => _services.GetRequiredService<SiteListViewModel>(),
                _ => null,
            };

            if (view == null)
            {
                _dialog.ShowError($"[{menuId}] 메뉴는 현재 준비중 입니다.");
                return;
            }

            CloseView();
            CurrentView = view;
            Buttons = view.Buttons;
            foreach (var menu in Menus)
                menu.IsSelected = menu.MenuId == menuId;

            ResetLockTimer();
            await view.InitializeAsync();
        }

        [RelayCommand]
        private async Task RunFunctionAsync(string? functionId)
        {
            if (IsLoggedIn == false || string.IsNullOrEmpty(functionId))
                return;

            if (functionId == Functions.닫기)
            {
                if (CurrentView == null || Buttons.CloseEnabled == false)
                    return;
                // ESC 두 번 누르면 화면을 닫는다. (기존 동작 유지)
                if (_escClick > 0)
                {
                    _escClick = 0;
                    CloseView();
                }
                else
                {
                    _escClick++;
                    StatusText = "ESC 를 한 번 더 누르면 현재 화면을 닫습니다.";
                }
                return;
            }

            _escClick = 0;
            if (CurrentView == null)
                return;

            ResetLockTimer();
            await CurrentView.RunFunctionAsync(functionId);
        }

        [RelayCommand]
        private void CloseView()
        {
            CurrentView = null;
            Buttons = new FunctionButtonState();
            foreach (var menu in Menus)
                menu.IsSelected = false;
            StatusText = "";
        }
        #endregion

        #region 잠금 / 로그아웃 / 종료
        [RelayCommand]
        private void LockScreen()
        {
            if (IsLoggedIn == false)
                return;
            Log.Info("화면 잠금");
            CloseView();
            _lockTimer.Stop();
            IsLoggedIn = false;
            Login.Lock();
        }

        [RelayCommand]
        private void ExtendTime()
        {
            if (IsLoggedIn == false)
                return;
            ResetLockTimer();
        }

        /// <summary>
        /// 로그아웃 버튼(Alt+O). 매개변수 없이 둔다.
        /// bool 매개변수를 두면 AsyncRelayCommand&lt;bool&gt; 이 생성되어 CommandParameter 가 없는 바인딩에서는
        /// CanExecute 가 false 가 되어 버튼이 비활성화된다.
        /// </summary>
        [RelayCommand]
        public async Task LogoutAsync()
        {
            if (IsLoggedIn == false)
                return;
            if (_dialog.Confirm("로그아웃 하시겠습니까?", "로그아웃") == false)
                return;

            Log.Info($"로그아웃: {SessionManager.Instance.ID}");
            await _auth.LogoutAsync();
            CloseView();
            _lockTimer.Stop();
            IsLoggedIn = false;
            UserInfoText = "";
            LoginUserText = "";
            Login.Reset(clearUserId: true);
        }

        /// <summary>프로그램 종료 버튼. 확인 → 서버 로그아웃 → 종료</summary>
        [RelayCommand]
        private async Task ExitAsync()
        {
            if (await PrepareExitAsync())
                Application.Current.Shutdown();
        }

        /// <summary>
        /// 종료 준비. (확인 →) 서버 세션 로그아웃 → <see cref="AllowClose"/> 켬.
        /// 창 닫기(X) / 종료 버튼 / 로그인 화면의 종료가 공통으로 사용한다. 취소하면 false.
        /// 화면 잠금 상태는 IsLoggedIn 이 false 지만 서버 세션은 살아 있으므로 세션 기준으로 로그아웃한다.
        /// </summary>
        public async Task<bool> PrepareExitAsync(bool confirm = true)
        {
            if (AllowClose)
                return true;
            if (confirm && _dialog.Confirm("프로그램을 종료하시겠습니까?", "프로그램 종료") == false)
                return false;

            if (SessionManager.Instance.IsLive)
            {
                try
                {
                    Log.Info($"종료 로그아웃: {SessionManager.Instance.ID}");
                    await _auth.LogoutAsync();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, "종료 시 로그아웃 실패");
                }
            }

            _lockTimer.Stop();
            IsLoggedIn = false;
            AllowClose = true;
            return true;
        }
        #endregion

        #region 잠금 타이머
        private void OnLockTimerTick(object? sender, EventArgs e)
        {
            _leftSeconds--;
            if (_leftSeconds < 0)
            {
                _lockTimer.Stop();
                LockScreen();
            }
            else
            {
                UpdateContinueText();
            }
        }

        private void ResetLockTimer()
        {
            _leftSeconds = LockSeconds;
            UpdateContinueText();
        }

        private void UpdateContinueText()
            => ContinueText = $"{_leftSeconds / 60}:{_leftSeconds % 60:00} 연장하기";
        #endregion

        #region 메시지 수신
        public void Receive(LoginSucceededMessage message)
        {
            var session = SessionManager.Instance;
            IsLoggedIn = true;
            LoginUserText = $"{session.Name} 님 로그인 중..";
            UserInfoText = $"접속자: {session.Name} [{session.ID}]";
            StatusText = "메뉴를 선택하세요.";
            ResetLockTimer();
            _lockTimer.Start();
        }

        public void Receive(UserActivityMessage message) => ResetLockTimer();

        public void Receive(StatusTextMessage message) => StatusText = message.Text;

        /// <summary>로그인 화면의 종료 버튼. 확인 없이 종료하되 잠금 상태의 서버 세션은 로그아웃한다.</summary>
        public void Receive(ExitRequestedMessage message) => _ = ExitFromLoginAsync();

        private async Task ExitFromLoginAsync()
        {
            await PrepareExitAsync(confirm: false);
            Application.Current.Shutdown();
        }
        #endregion
    }
}
