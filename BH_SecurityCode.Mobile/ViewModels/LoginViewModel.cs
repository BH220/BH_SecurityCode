using BH_SecurityCode.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>
    /// 로그인. 데스크톱 LoginViewModel(ctlLogin 오버레이) 을 전체 화면으로 옮겼다.
    ///
    /// 자동 로그인은 "앱을 새로 켤 때" 한 번만 동작한다. 자동 잠금으로 세션이 끊겨 돌아온 경우에는
    /// 아이디·비밀번호를 채워만 두고 로그인 버튼을 누르게 한다 — 그렇지 않으면 잠금이 무의미해진다.
    /// </summary>
    public sealed partial class LoginViewModel : BaseViewModel
    {
        private readonly ISessionService _session;
        private readonly IAppSettings _settings;
        private readonly ICredentialStore _credentials;

        /// <summary>이 프로세스에서 자동 로그인을 이미 시도했는지 (앱 실행당 1회)</summary>
        private bool _autoLoginTried;

        public LoginViewModel(ISessionService session, IAppSettings settings, ICredentialStore credentials)
        {
            _session = session;
            _settings = settings;
            _credentials = credentials;

            _userId = settings.LastUserId;
            _autoLogin = settings.AutoLogin;
        }

        [ObservableProperty]
        private string _userId = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _errorText = "";

        /// <summary>자동 로그인 사용</summary>
        [ObservableProperty]
        private bool _autoLogin;

        /// <summary>자동 로그인이 실행되는 중임을 알리는 안내</summary>
        [ObservableProperty]
        private bool _isAutoSigningIn;

        public bool HasError => string.IsNullOrEmpty(ErrorText) == false;

        public string ServerText =>
            string.IsNullOrEmpty(_settings.ServerAddress) ? "서버 주소가 설정되지 않았습니다" : _settings.ServerAddress;

        partial void OnErrorTextChanged(string value) => OnPropertyChanged(nameof(HasError));

        partial void OnUserIdChanged(string value) => ErrorText = "";

        partial void OnPasswordChanged(string value) => ErrorText = "";

        partial void OnAutoLoginChanged(bool value)
        {
            _settings.AutoLogin = value;

            // 껐으면 저장해둔 비밀번호도 지운다.
            if (value == false)
                _ = _credentials.ClearAsync();
        }

        [RelayCommand]
        private async Task AppearingAsync()
        {
            ErrorText = "";
            OnPropertyChanged(nameof(ServerText));

            // 설정 화면에서 바꿨을 수 있으므로 다시 읽는다. (이 ViewModel 은 싱글턴이다)
            if (AutoLogin != _settings.AutoLogin)
                AutoLogin = _settings.AutoLogin;

            // 세션이 아직 살아 있는데 로그인 화면이 다시 뜬 경우(액티비티 재생성 등)에는 그냥 통과시킨다.
            if (_session.IsAuthenticated)
            {
                await Shell.Current.GoToAsync("//main/home");
                return;
            }

            var saved = await _credentials.LoadAsync();
            if (saved == null)
            {
                Password = "";
                return;
            }

            // 저장된 정보를 채워 둔다. 잠금 해제 후 돌아온 경우엔 버튼만 누르면 된다.
            UserId = saved.Value.UserId;
            Password = saved.Value.Password;

            if (_settings.AutoLogin && _autoLoginTried == false)
            {
                _autoLoginTried = true;
                IsAutoSigningIn = true;
                try
                {
                    await SignInAsync();
                }
                finally
                {
                    IsAutoSigningIn = false;
                }
            }
        }

        [RelayCommand]
        private Task SignInAsync() => SignInCoreAsync(false);

        private async Task SignInCoreAsync(bool force)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorText = "";
            try
            {
                var outcome = await _session.SignInAsync(UserId, Password, force);

                if (outcome.DuplicateSession)
                {
                    await HandleDuplicateSessionAsync(outcome);
                    return;
                }

                if (outcome.Success == false)
                {
                    ErrorText = outcome.Message;
                    return;
                }

                _settings.LastUserId = UserId.Trim();

                if (AutoLogin)
                    await _credentials.SaveAsync(UserId.Trim(), Password);
                else
                    await _credentials.ClearAsync();

                await Shell.Current.GoToAsync("//main/home");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>다른 곳에 세션이 있을 때. 데스크톱과 같이 확인을 받고 기존 세션을 끊는다.</summary>
        private async Task HandleDuplicateSessionAsync(SignInOutcome outcome)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page == null)
            {
                ErrorText = outcome.Message;
                return;
            }

            string detail = string.IsNullOrEmpty(outcome.ExistingSessionText)
                ? ""
                : $"\n\n기존 접속: {outcome.ExistingSessionText}";

            bool force = await page.DisplayAlert(
                "이미 로그인되어 있습니다",
                $"다른 곳의 접속을 끊고 이 기기에서 로그인할까요?{detail}",
                "여기서 로그인",
                "취소");

            if (force == false)
            {
                await _session.AbortAsync(UserId.Trim());
                ErrorText = "로그인을 취소했습니다.";
                return;
            }

            IsBusy = false;
            await SignInCoreAsync(true);
        }

        /// <summary>서버 주소 설정으로 이동 (데스크톱 우하단 ⚙)</summary>
        [RelayCommand]
        private async Task OpenServerSettingAsync()
        {
            await Shell.Current.GoToAsync("serversetting");
        }
    }
}
