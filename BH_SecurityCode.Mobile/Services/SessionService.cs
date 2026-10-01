using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Core.Session;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BH_SecurityCode.Mobile.Services
{
    /// <summary>로그인 시도 결과. 중복 세션이면 사용자에게 강제 로그인 여부를 물어야 한다.</summary>
    public sealed class SignInOutcome
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        public bool ServerUnavailable { get; init; }
        public bool DuplicateSession { get; init; }

        /// <summary>중복 세션 안내에 붙일 기존 접속 정보 ("192.168.0.5 · 09-10 14:22")</summary>
        public string ExistingSessionText { get; init; } = "";
    }

    public interface ISessionService
    {
        bool IsAuthenticated { get; }
        string UserId { get; }
        string UserName { get; }

        /// <summary>남은 세션 시간 "12:34"</summary>
        string RemainingText { get; }

        bool IsExpiringSoon { get; }

        /// <summary>세션이 만료돼 로그인 화면으로 보내야 할 때</summary>
        event EventHandler? Expired;

        /// <summary>로그인. <paramref name="force"/> 면 다른 곳의 세션을 끊는다.</summary>
        Task<SignInOutcome> SignInAsync(string userId, string password, bool force = false);

        /// <summary>중복 세션 안내에서 로그인을 포기한 경우 서버에 알린다.</summary>
        Task AbortAsync(string userId);

        /// <summary>시간 연장 (데스크톱 Alt+R)</summary>
        void Extend();

        Task SignOutAsync();
    }

    /// <summary>
    /// 로그인 상태와 세션 잔여 시간을 관리한다. 인증은 <see cref="IAuthManager"/>(POST /api/auth/login)가 수행하고,
    /// 발급된 uuid 는 Core 의 <see cref="SessionManager"/> 에 담겨 이후 API 의 Bearer 토큰으로 쓰인다.
    /// 잔여 시간은 서버가 준 timeout(분)을 쓰고, 0(무제한)이면 기기 설정값을 쓴다.
    /// </summary>
    public sealed partial class SessionService : ObservableObject, ISessionService
    {
        private readonly IAppSettings _settings;
        private readonly IAuthManager _auth;
        private readonly IVaultDataService _data;

        private IDispatcherTimer? _timer;
        private DateTime _expiresAt;
        private int _timeoutMinutes;

        public SessionService(IAppSettings settings, IAuthManager auth, IVaultDataService data)
        {
            _settings = settings;
            _auth = auth;
            _data = data;
        }

        [ObservableProperty]
        private bool _isAuthenticated;

        [ObservableProperty]
        private string _userId = "";

        [ObservableProperty]
        private string _userName = "";

        [ObservableProperty]
        private string _remainingText = "--:--";

        [ObservableProperty]
        private bool _isExpiringSoon;

        public event EventHandler? Expired;

        public async Task<SignInOutcome> SignInAsync(string userId, string password, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new SignInOutcome { Message = "아이디를 입력하세요." };
            if (string.IsNullOrWhiteSpace(password))
                return new SignInOutcome { Message = "비밀번호를 입력하세요." };

            if (string.IsNullOrWhiteSpace(_settings.ServerAddress))
                return new SignInOutcome { ServerUnavailable = true, Message = "API 서버 주소를 먼저 설정하세요." };

            // 매번 적용한다. 설정에서 주소를 바꿨을 수 있다.
            ApiBootstrap.Apply(_settings);

            var result = await _auth.LoginAsync(userId.Trim(), password, force);

            if (result.Success == false)
            {
                return new SignInOutcome
                {
                    Message = result.Message,
                    ServerUnavailable = result.ServerUnavailable,
                    DuplicateSession = result.IsDuplicateSession,
                    ExistingSessionText = Describe(result.ExistingSession),
                };
            }

            UserId = SessionManager.Instance.ID;
            UserName = string.IsNullOrWhiteSpace(SessionManager.Instance.Name)
                ? SessionManager.Instance.ID
                : SessionManager.Instance.Name;
            IsAuthenticated = true;

            // 서버가 정한 세션 시간(분). 0 이면 무제한이라는 뜻이라 기기 설정값으로 잠근다.
            _timeoutMinutes = SessionManager.Instance.Timeout > 0
                ? SessionManager.Instance.Timeout
                : _settings.LockMinutes;

            _data.Clear();
            Extend();
            return new SignInOutcome { Success = true };
        }

        public Task AbortAsync(string userId) => _auth.AbortLoginAsync(userId);

        public void Extend()
        {
            if (_timeoutMinutes <= 0)
                _timeoutMinutes = _settings.LockMinutes;

            _expiresAt = DateTime.Now.AddMinutes(_timeoutMinutes);
            Tick();
            StartTimer();
        }

        public async Task SignOutAsync()
        {
            StopTimer();
            try
            {
                await _auth.LogoutAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SessionService] 로그아웃 실패: {ex.Message}");
            }
            finally
            {
                SessionManager.Clear();
                _data.Clear();
                IsAuthenticated = false;
                UserId = "";
                UserName = "";
                RemainingText = "--:--";
                IsExpiringSoon = false;
            }
        }

        private static string Describe(ExistingSessionData? existing)
        {
            if (existing == null)
                return "";

            var parts = new List<string>();
            if (string.IsNullOrWhiteSpace(existing.ip) == false)
                parts.Add(existing.ip);
            if (existing.lastActivityAt.HasValue)
                parts.Add(existing.lastActivityAt.Value.ToString("MM-dd HH:mm") + " 활동");
            else if (existing.loginAt.HasValue)
                parts.Add(existing.loginAt.Value.ToString("MM-dd HH:mm") + " 로그인");

            return string.Join(" · ", parts);
        }

        private void StartTimer()
        {
            if (_timer != null)
                return;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
                return;

            _timer = dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.IsRepeating = true;
            _timer.Tick += (_, _) => Tick();
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer?.Stop();
            _timer = null;
        }

        private void Tick()
        {
            if (IsAuthenticated == false)
                return;

            var left = _expiresAt - DateTime.Now;
            if (left <= TimeSpan.Zero)
            {
                StopTimer();
                SessionManager.Clear();
                _data.Clear();
                IsAuthenticated = false;
                RemainingText = "--:--";
                IsExpiringSoon = false;
                Expired?.Invoke(this, EventArgs.Empty);
                return;
            }

            RemainingText = $"{(int)left.TotalMinutes:00}:{left.Seconds:00}";
            IsExpiringSoon = left <= TimeSpan.FromMinutes(1);
        }
    }
}
