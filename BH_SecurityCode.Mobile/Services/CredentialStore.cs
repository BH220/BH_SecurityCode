namespace BH_SecurityCode.Mobile.Services
{
    public interface ICredentialStore
    {
        /// <summary>저장된 로그인 정보. 없으면 null.</summary>
        Task<(string UserId, string Password)?> LoadAsync();

        Task SaveAsync(string userId, string password);

        Task ClearAsync();
    }

    /// <summary>
    /// 자동 로그인용 아이디·비밀번호 저장소.
    ///
    /// Preferences(평문 SharedPreferences)가 아니라 <see cref="SecureStorage"/> 를 쓴다.
    /// 안드로이드에서는 키스토어로 감싼 EncryptedSharedPreferences 에 들어가므로
    /// 루팅되지 않은 기기에서 다른 앱이나 adb 로 값을 그대로 읽을 수 없다.
    ///
    /// TODO: [API 연동] 서버가 refresh token 을 주면 비밀번호 대신 토큰을 여기에 담는 편이 낫다.
    ///       비밀번호를 기기에 두지 않아도 되고, 서버에서 무효화할 수 있다.
    /// </summary>
    public sealed class CredentialStore : ICredentialStore
    {
        private const string UserKey = "bh.cred.user";
        private const string PasswordKey = "bh.cred.pw";

        public async Task<(string UserId, string Password)?> LoadAsync()
        {
            try
            {
                string? userId = await SecureStorage.Default.GetAsync(UserKey);
                string? password = await SecureStorage.Default.GetAsync(PasswordKey);

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
                    return null;

                return (userId, password);
            }
            catch (Exception)
            {
                // 키스토어가 초기화(기기 잠금 변경 등)되면 복호화가 실패한다. 저장 안 된 것으로 본다.
                await ClearAsync();
                return null;
            }
        }

        public async Task SaveAsync(string userId, string password)
        {
            await SecureStorage.Default.SetAsync(UserKey, userId);
            await SecureStorage.Default.SetAsync(PasswordKey, password);
        }

        public Task ClearAsync()
        {
            SecureStorage.Default.Remove(UserKey);
            SecureStorage.Default.Remove(PasswordKey);
            return Task.CompletedTask;
        }
    }
}
