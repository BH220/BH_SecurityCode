using BH_SecurityCode.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>
    /// 설정. 데스크톱 우하단 ⚙(ServerSettingWindow) + 자동 잠금 설정에 대응한다.
    /// </summary>
    public sealed partial class SettingsViewModel : BaseViewModel
    {
        private readonly IAppSettings _settings;
        private readonly ICredentialStore _credentials;
        private readonly IImageCache _cache;

        /// <summary>토글을 코드로 되돌릴 때 OnChanged 가 다시 돌지 않게 한다.</summary>
        private bool _suppressCacheToggle;

        public SettingsViewModel(IAppSettings settings, ISessionService session, ICredentialStore credentials,
            IImageCache cache)
        {
            _settings = settings;
            _credentials = credentials;
            _cache = cache;
            Session = session;

            _serverAddress = settings.ServerAddress;
            _lockMinutes = settings.LockMinutes;
            _maskSensitive = settings.MaskSensitive;
            _autoLogin = settings.AutoLogin;
            _cacheOnSdCard = settings.ImageCacheOnSdCard;
            Title = "설정";
        }

        public ISessionService Session { get; }

        public string AppVersion => $"{AppInfo.VersionString} ({AppInfo.BuildString})";
        public string PackageName => AppInfo.PackageName;
        public string DeviceText => $"{DeviceInfo.Manufacturer} {DeviceInfo.Model} · Android {DeviceInfo.VersionString}";

        [ObservableProperty]
        private string _serverAddress = "";

        [ObservableProperty]
        private string _serverError = "";

        [ObservableProperty]
        private int _lockMinutes;

        [ObservableProperty]
        private bool _maskSensitive;

        [ObservableProperty]
        private bool _autoLogin;

        // ── 이미지 캐시 ─────────────────────────────────────────────

        [ObservableProperty]
        private bool _cacheOnSdCard;

        [ObservableProperty]
        private string _cacheUsageText = "-";

        [ObservableProperty]
        private string _cachePath = "";

        [ObservableProperty]
        private bool _isSdCardAvailable;

        public string SdCardHint => IsSdCardAvailable
            ? "카드를 빼면 받은 사진을 다시 내려받습니다"
            : "SD카드가 없습니다";

        public bool HasServerError => string.IsNullOrEmpty(ServerError) == false;

        public string LockMinutesText => $"{LockMinutes}분";

        partial void OnServerErrorChanged(string value) => OnPropertyChanged(nameof(HasServerError));

        partial void OnIsSdCardAvailableChanged(bool value) => OnPropertyChanged(nameof(SdCardHint));

        partial void OnLockMinutesChanged(int value)
        {
            _settings.LockMinutes = value;
            OnPropertyChanged(nameof(LockMinutesText));
        }

        partial void OnMaskSensitiveChanged(bool value) => _settings.MaskSensitive = value;

        partial void OnAutoLoginChanged(bool value)
        {
            _settings.AutoLogin = value;

            if (value == false)
            {
                _ = _credentials.ClearAsync();
                ShowToast("저장된 로그인 정보를 삭제했습니다");
            }
        }

        partial void OnCacheOnSdCardChanged(bool value)
        {
            if (_suppressCacheToggle)
                return;

            _ = ApplyCacheLocationAsync(value);
        }

        [RelayCommand]
        private async Task AppearingAsync()
        {
            IsSdCardAvailable = _cache.IsSdCardAvailable;

            // 카드를 빼놓은 상태라면 설정은 SD카드여도 실제로는 내부에 저장된다. 표시를 실제와 맞춘다.
            _suppressCacheToggle = true;
            CacheOnSdCard = _cache.Location == ImageCacheLocation.SD카드;
            _suppressCacheToggle = false;

            await RefreshCacheUsageAsync();
        }

        private async Task ApplyCacheLocationAsync(bool useSdCard)
        {
            if (useSdCard && _cache.IsSdCardAvailable == false)
            {
                _suppressCacheToggle = true;
                CacheOnSdCard = false;
                _suppressCacheToggle = false;
                ShowToast("SD카드를 찾을 수 없습니다");
                return;
            }

            _settings.ImageCacheOnSdCard = useSdCard;

            IsBusy = true;
            try
            {
                int moved = await _cache.MoveToCurrentAsync();
                await RefreshCacheUsageAsync();
                ShowToast(moved > 0
                    ? $"캐시 {moved}개를 {_cache.Location} 로 옮겼습니다"
                    : $"저장 위치를 {_cache.Location} 로 바꿨습니다");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshCacheUsageAsync()
        {
            var (count, bytes) = await _cache.GetUsageAsync();
            CacheUsageText = count == 0 ? "없음" : $"{count}개 · {Size(bytes)}";
            CachePath = _cache.CurrentPath;
        }

        private static string Size(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB" };
            double value = bytes;
            int unit = 0;
            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }
            return $"{value:0.#} {units[unit]}";
        }

        [RelayCommand]
        private async Task ClearCacheAsync()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
            {
                bool confirm = await page.DisplayAlert(
                    "캐시 비우기",
                    "받아둔 첨부 사진을 모두 지웁니다.\n서버 데이터는 그대로이고, 다음에 열 때 다시 내려받습니다.",
                    "비우기",
                    "취소");

                if (confirm == false)
                    return;
            }

            await _cache.ClearAsync();
            await RefreshCacheUsageAsync();
            ShowToast("캐시를 비웠습니다");
        }

        // ── 서버 · 계정 ─────────────────────────────────────────────

        [RelayCommand]
        private void SaveServer()
        {
            if (IAppSettings.IsValidAddress(ServerAddress, out string error) == false)
            {
                ServerError = error;
                return;
            }

            ServerError = "";
            _settings.ServerAddress = ServerAddress;
            ServerAddress = _settings.ServerAddress;
            ShowToast("서버 주소를 저장했습니다");
        }

        [RelayCommand]
        private void LockLonger()
        {
            LockMinutes = Math.Min(LockMinutes + 5, 120);
        }

        [RelayCommand]
        private void LockShorter()
        {
            LockMinutes = Math.Max(LockMinutes - 5, 5);
        }

        /// <summary>
        /// 직접 로그아웃하는 것은 "다른 계정으로 들어가겠다"는 뜻으로 본다.
        /// 자동 로그인을 끄고 저장된 정보도 지운다. (자동 잠금으로 끊긴 경우는 지우지 않는다)
        /// </summary>
        [RelayCommand]
        private async Task SignOutAsync()
        {
            AutoLogin = false;
            await _credentials.ClearAsync();
            await Session.SignOutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
