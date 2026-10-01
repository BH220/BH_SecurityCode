using BH_SecurityCode.Api;

namespace BH_SecurityCode.Mobile.Services
{
    /// <summary>
    /// ScApi 에 서버 주소를 밀어 넣는다.
    ///
    /// ScApi 는 BH_SecurityCode.Core 의 <c>Config.ServerSetting</c>(JSON 파일)에서 주소를 읽는데,
    /// 그 파일 경로가 윈도우 기준(%ProgramData%\BH Soft\settings)이라 안드로이드에서는 실패할 수 있다.
    /// 모바일은 주소를 기기 Preferences 에 갖고 있으므로, 시작할 때와 서버 주소를 바꿀 때마다
    /// <see cref="ScApi.SetUrl"/> 로 직접 지정한다. Core 설정 파일 접근이 터져도 앱은 계속 동작해야 하므로 감싼다.
    /// </summary>
    public static class ApiBootstrap
    {
        public static string LastError { get; private set; } = "";

        public static bool Apply(string? address)
        {
            LastError = "";
            if (string.IsNullOrWhiteSpace(address))
                return false;

            try
            {
                ScApi.Instance.SetUrl(address);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[ApiBootstrap] SetUrl 실패: {ex}");
                return false;
            }
        }

        public static bool Apply(IAppSettings settings) => Apply(settings.ServerAddress);
    }
}
