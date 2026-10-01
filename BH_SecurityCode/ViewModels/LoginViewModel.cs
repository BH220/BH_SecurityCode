using System.Reflection;
using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Core.Configurations;
using BH_SecurityCode.Core.Helper;
using BH_SecurityCode.Messages;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using BH_SecurityCode.ViewModels.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BH_SecurityCode.ViewModels
{
    /// <summary>로그인 화면 (기존 ctlLogin)</summary>
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthManager _auth;
        private readonly IDialogService _dialog;

        [ObservableProperty]
        private string _userId = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isIdFocused;

        [ObservableProperty]
        private bool _isPasswordFocused;

        /// <summary>현재 설정된 API 서버 주소 표시용</summary>
        [ObservableProperty]
        private string _serverAddressText = "";

        /// <summary>화면 잠금 상태에서는 아이디를 바꿀 수 없다.</summary>
        [ObservableProperty]
        private bool _readOnly;

        /// <summary>화면 잠금 중 여부. 잠금 해제는 서버를 거치지 않고 로그인 때의 비밀번호와 비교한다.</summary>
        public bool IsLocked { get; private set; }

        /// <summary>잠금 해제 비교용. 로그인 성공 시 보관하고 로그아웃 시 지운다.</summary>
        private string _lockPassword = "";

        public string ProductName => "BH Security Code";
        public string VersionText => $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}";

        public LoginViewModel(IAuthManager auth, IDialogService dialog)
        {
            _auth = auth;
            _dialog = dialog;
            RefreshServerAddress();
        }

        /// <summary>기존 프로그램처럼 버튼은 항상 활성화하고, 빈 값은 메시지로 안내한다.</summary>
        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy)
                return;

            ErrorMessage = "";
            if (string.IsNullOrWhiteSpace(UserId))
            {
                ErrorMessage = "아이디를 입력하세요.";
                IsIdFocused = true;
                return;
            }
            if (string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "비밀번호를 입력하세요.";
                IsPasswordFocused = true;
                return;
            }

            // 화면 잠금 해제: 서버 세션은 살아 있으므로 로그인 때 비밀번호만 확인한다.
            if (IsLocked)
            {
                if (Password == _lockPassword)
                {
                    IsLocked = false;
                    Password = "";
                    Log.Info($"화면 잠금 해제: {UserId}");
                    WeakReferenceMessenger.Default.Send(new LoginSucceededMessage());
                }
                else
                {
                    ErrorMessage = "비밀번호가 일치하지 않습니다.";
                    IsPasswordFocused = true;
                }
                return;
            }

            IsBusy = true;
            try
            {
                string userId = UserId.Trim();
                var result = await _auth.LoginAsync(userId, Password);

                // 다른 곳에 세션이 있으면 기존 접속을 끊고 로그인할지 묻는다. (서버 409 DUPLICATE_SESSION)
                if (result.IsDuplicateSession)
                {
                    string detail = "";
                    if (result.ExistingSession != null)
                    {
                        detail = $"\r\n\r\n접속 위치: {result.ExistingSession.ip}";
                        if (result.ExistingSession.loginAt.HasValue)
                            detail += $"\r\n로그인 시각: {result.ExistingSession.loginAt:yyyy-MM-dd HH:mm:ss}";
                    }
                    bool force = _dialog.Confirm($"이미 다른 곳에서 로그인되어 있습니다.{detail}\r\n\r\n기존 접속을 종료하고 로그인하시겠습니까?", "중복 로그인");
                    if (force == false)
                    {
                        await _auth.AbortLoginAsync(userId);
                        ErrorMessage = "로그인을 취소했습니다.";
                        return;
                    }
                    result = await _auth.LoginAsync(userId, Password, force: true);
                }

                if (result.Success)
                {
                    Log.Info($"로그인 성공: {userId}");
                    _lockPassword = Password;
                    ReadOnly = true;
                    Password = "";
                    WeakReferenceMessenger.Default.Send(new LoginSucceededMessage());
                }
                else
                {
                    ErrorMessage = result.Message;
                    IsPasswordFocused = true;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Login failed");
                ErrorMessage = $"로그인 처리 중 오류가 발생했습니다.\r\n{ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Exit() => WeakReferenceMessenger.Default.Send(new ExitRequestedMessage());

        /// <summary>우하단 톱니바퀴: API 서버 주소 설정</summary>
        [RelayCommand]
        private void OpenSettings()
        {
            var vm = new ServerSettingViewModel(_dialog);
            if (_dialog.ShowDialog(vm) == true)
                RefreshServerAddress();
        }

        private void RefreshServerAddress()
        {
            string address = Config.ServerSetting.Address;
            ServerAddressText = string.IsNullOrEmpty(address) ? "서버: 미설정" : $"서버: {address}";
        }

        /// <summary>화면 잠금. 아이디는 고정하고 비밀번호만 다시 받는다.</summary>
        public void Lock()
        {
            IsLocked = true;
            ReadOnly = true;
            Password = "";
            ErrorMessage = "";
            IsPasswordFocused = true;
        }

        /// <summary>로그아웃 후 로그인 화면을 초기화한다. (clearUserId = true 면 잠금 상태도 해제)</summary>
        public void Reset(bool clearUserId)
        {
            if (clearUserId)
            {
                UserId = "";
                IsLocked = false;
                ReadOnly = false;
                _lockPassword = "";
            }
            Password = "";
            ErrorMessage = "";
            if (clearUserId || string.IsNullOrEmpty(UserId))
                IsIdFocused = true;
            else
                IsPasswordFocused = true;
        }
    }
}
