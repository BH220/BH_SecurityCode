using BH_SecurityCode.Core.Configurations.Setting;

namespace BH_SecurityCode.Core.Configurations
{
    /// <summary>
    /// 프로그램 설정값을 관리하는 클래스
    /// </summary>
    public class Config
    {
        /// <summary>암호화 설정 파일(IsEncryption = true)에 사용하는 키</summary>
        public const string AesKey = "BH_SecurityCode::Settings::v1";

        private static ServerSetting? _serverSetting;

        /// <summary>API 서버 설정. 최초 접근 시 JSON 파일에서 로드된다. (<see cref="ServerSetting.FilePath"/>)</summary>
        public static ServerSetting ServerSetting
        {
            get
            {
                if (_serverSetting == null)
                {
                    _serverSetting = new ServerSetting();
                    _serverSetting.Load();
                }
                return _serverSetting;
            }
        }

        public static Mutex? RunMutex { get; set; }
        public static bool IsExitPossible { get; set; }
    }
}
