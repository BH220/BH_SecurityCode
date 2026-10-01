namespace BH_SecurityCode.Mobile.Services
{
    public interface IAppSettings
    {
        /// <summary>API 서버 주소 (스킴 포함). 데스크톱 ServerSetting.Address 와 같은 값.</summary>
        string ServerAddress { get; set; }

        /// <summary>세션 유지 시간(분). 데스크톱 자동 잠금과 같은 개념.</summary>
        int LockMinutes { get; set; }

        /// <summary>번호·비밀번호를 기본으로 가려서 보여줄지</summary>
        bool MaskSensitive { get; set; }

        /// <summary>마지막 로그인 아이디 (입력 편의)</summary>
        string LastUserId { get; set; }

        /// <summary>앱을 다시 켤 때 저장된 정보로 자동 로그인할지. 비밀번호는 SecureStorage 에 있다.</summary>
        bool AutoLogin { get; set; }

        /// <summary>첨부 이미지 캐시를 SD카드에 둘지. 카드가 없으면 내부 저장소로 떨어진다.</summary>
        bool ImageCacheOnSdCard { get; set; }

        /// <summary>입력값을 정리한다. ServerSetting.Normalize 와 동일한 규칙.</summary>
        static string NormalizeAddress(string? address)
        {
            string value = (address ?? "").Trim().TrimEnd('/');
            if (value.Length == 0)
                return "";
            if (value.Contains("://") == false)
                value = "https://" + value;
            return value;
        }

        /// <summary>
        /// http/https 절대 주소인지 검사한다. 데스크톱 ServerSetting.IsValid 와 같은 규칙에
        /// https 강제를 더한 것이다.
        ///
        /// 배포 빌드는 평문 HTTP 를 아예 차단하므로(network_security_config.xml) http 주소를 넣으면
        /// 저장은 되고 통신만 실패해 원인을 알기 어렵다. 그래서 입력 단계에서 막는다.
        /// Debug 빌드는 로컬 서버에 붙어야 하므로 http 를 허용한다.
        /// </summary>
        static bool IsValidAddress(string? address, out string error)
        {
            error = "";
            string value = NormalizeAddress(address);
            if (value.Length == 0)
            {
                error = "API 서버 주소를 입력하세요.";
                return false;
            }
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri) == false ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                error = "주소 형식이 올바르지 않습니다. 예) https://api.bhsoft.com";
                return false;
            }
#if !DEBUG
            if (uri.Scheme != Uri.UriSchemeHttps)
            {
                error = "https 주소만 사용할 수 있습니다. 예) https://api.bhsoft.com";
                return false;
            }
#endif
            if (uri.PathAndQuery != "/" || string.IsNullOrEmpty(uri.Fragment) == false)
            {
                error = "주소에는 경로를 포함하지 마세요. 예) https://api.bhsoft.com";
                return false;
            }
            return true;
        }
    }

    /// <summary>기기 로컬 설정. 민감한 값은 담지 않는다(토큰은 SecureStorage 로 별도 처리 예정).</summary>
    public sealed class AppSettings : IAppSettings
    {
        public string ServerAddress
        {
            get => Preferences.Get(nameof(ServerAddress), "");
            set
            {
                string address = IAppSettings.NormalizeAddress(value);
                Preferences.Set(nameof(ServerAddress), address);
                // ScApi 가 바로 새 주소를 쓰도록 함께 적용한다.
                ApiBootstrap.Apply(address);
            }
        }

        public int LockMinutes
        {
            get => Preferences.Get(nameof(LockMinutes), 10);
            set => Preferences.Set(nameof(LockMinutes), Math.Clamp(value, 1, 120));
        }

        public bool MaskSensitive
        {
            get => Preferences.Get(nameof(MaskSensitive), true);
            set => Preferences.Set(nameof(MaskSensitive), value);
        }

        public string LastUserId
        {
            get => Preferences.Get(nameof(LastUserId), "");
            set => Preferences.Set(nameof(LastUserId), value ?? "");
        }

        public bool AutoLogin
        {
            get => Preferences.Get(nameof(AutoLogin), false);
            set => Preferences.Set(nameof(AutoLogin), value);
        }

        public bool ImageCacheOnSdCard
        {
            get => Preferences.Get(nameof(ImageCacheOnSdCard), false);
            set => Preferences.Set(nameof(ImageCacheOnSdCard), value);
        }
    }
}
