using System.Security.Cryptography;

namespace BH_SecurityCode.Mobile.Services
{
    /// <summary>캐시를 어디에 둘지.</summary>
    public enum ImageCacheLocation
    {
        내부저장소,
        SD카드,
    }

    public interface IImageCache
    {
        /// <summary>SD카드(제거 가능한 외부 저장소)가 꽂혀 있고 쓸 수 있는지. 꽂았다 뺄 수 있으므로 매번 확인한다.</summary>
        bool IsSdCardAvailable { get; }

        /// <summary>현재 저장 위치. 설정값이 SD카드인데 카드가 없으면 내부 저장소로 떨어진다.</summary>
        ImageCacheLocation Location { get; }

        /// <summary>실제 캐시 폴더 경로 (설정 화면 표시용)</summary>
        string CurrentPath { get; }

        Task<byte[]?> GetAsync(int imageNum);
        Task SaveAsync(int imageNum, byte[] data);

        /// <summary>캐시 사용량 (파일 수, 바이트)</summary>
        Task<(int Count, long Bytes)> GetUsageAsync();

        /// <summary>저장 위치를 바꾼 뒤, 이전 위치에 남은 캐시를 현재 위치로 옮긴다.</summary>
        Task<int> MoveToCurrentAsync();

        /// <summary>양쪽 위치의 캐시를 모두 비운다.</summary>
        Task ClearAsync();
    }

    /// <summary>
    /// 첨부 이미지 로컬 캐시. 한 번 받은 이미지는 다시 내려받지 않는다.
    ///
    /// 저장 위치는 둘 중 하나다.
    ///   내부 저장소 : FileSystem.AppDataDirectory (앱 샌드박스, 다른 앱이 못 읽음)
    ///   SD카드      : /storage/XXXX-XXXX/Android/data/(패키지)/files (권한 불필요, 갤러리에 안 뜸)
    ///
    /// SD카드는 카드를 뽑아 PC 에 꽂으면 그대로 읽히므로 파일을 그냥 두면 안 된다.
    /// 신분증·통장·카드 사진이라 위치와 무관하게 AES 로 암호화해서 저장한다.
    /// 키는 SecureStorage(안드로이드 키스토어)에 두고 기기 밖으로 나가지 않는다.
    /// </summary>
    public sealed class ImageCache : IImageCache
    {
        private const string FolderName = "imgcache";
        private const string KeyName = "bh.imgcache.key";

        private readonly IAppSettings _settings;
        private byte[]? _key;

        public ImageCache(IAppSettings settings)
        {
            _settings = settings;
        }

        public bool IsSdCardAvailable => SdCardRoot() != null;

        public ImageCacheLocation Location =>
            _settings.ImageCacheOnSdCard && IsSdCardAvailable
                ? ImageCacheLocation.SD카드
                : ImageCacheLocation.내부저장소;

        public string CurrentPath => CurrentRoot();

        public async Task<byte[]?> GetAsync(int imageNum)
        {
            if (imageNum <= 0)
                return null;

            // 설정은 SD카드인데 카드를 뺐을 수 있다. 양쪽 다 찾아본다.
            foreach (string root in AllRoots())
            {
                string path = Path.Combine(root, FileNameOf(imageNum));
                if (File.Exists(path) == false)
                    continue;

                try
                {
                    byte[] stored = await File.ReadAllBytesAsync(path);
                    return Decrypt(stored, await GetKeyAsync());
                }
                catch (Exception ex)
                {
                    // 키가 바뀌었거나 파일이 깨졌다. 캐시 미스로 보고 지운다.
                    System.Diagnostics.Debug.WriteLine($"[ImageCache] 복호화 실패 {path}: {ex.Message}");
                    TryDelete(path);
                }
            }
            return null;
        }

        public async Task SaveAsync(int imageNum, byte[] data)
        {
            if (imageNum <= 0 || data == null || data.Length == 0)
                return;

            try
            {
                string root = CurrentRoot();
                Directory.CreateDirectory(root);

                byte[] encrypted = Encrypt(data, await GetKeyAsync());
                string path = Path.Combine(root, FileNameOf(imageNum));

                // 쓰다 말면 다음에 복호화가 깨진다. 임시 파일로 쓴 뒤 바꿔치기한다.
                string temp = path + ".tmp";
                await File.WriteAllBytesAsync(temp, encrypted);
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(temp, path);
            }
            catch (Exception ex)
            {
                // 캐시는 없어도 동작해야 한다. 실패해도 넘어간다.
                System.Diagnostics.Debug.WriteLine($"[ImageCache] 저장 실패 {imageNum}: {ex.Message}");
            }
        }

        public Task<(int Count, long Bytes)> GetUsageAsync()
        {
            int count = 0;
            long bytes = 0;

            foreach (string root in AllRoots())
            {
                if (Directory.Exists(root) == false)
                    continue;

                foreach (string file in Directory.EnumerateFiles(root, "*.bin"))
                {
                    try
                    {
                        bytes += new FileInfo(file).Length;
                        count++;
                    }
                    catch (IOException)
                    {
                        // 지워지는 중이면 건너뛴다.
                    }
                }
            }
            return Task.FromResult((count, bytes));
        }

        /// <summary>
        /// 설정에서 위치를 바꾸면 이전 위치의 파일을 현재 위치로 옮긴다.
        /// 내부 저장소를 비우는 게 목적이므로 그냥 두지 않고 실제로 옮긴다.
        /// 옮기다 실패한 파일은 남겨둔다 — 조회는 양쪽을 다 보므로 다시 받지는 않는다.
        /// </summary>
        public async Task<int> MoveToCurrentAsync()
        {
            string target = CurrentRoot();
            int moved = 0;

            foreach (string source in AllRoots())
            {
                if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (Directory.Exists(source) == false)
                    continue;

                Directory.CreateDirectory(target);

                foreach (string file in Directory.EnumerateFiles(source, "*.bin"))
                {
                    string destination = Path.Combine(target, Path.GetFileName(file));
                    try
                    {
                        // 볼륨이 다르면 Move 가 안 되는 경우가 있어 복사 후 삭제한다.
                        await using (var input = File.OpenRead(file))
                        await using (var output = File.Create(destination))
                            await input.CopyToAsync(output);

                        File.Delete(file);
                        moved++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ImageCache] 이동 실패 {file}: {ex.Message}");
                        TryDelete(destination);
                    }
                }
            }
            return moved;
        }

        public Task ClearAsync()
        {
            foreach (string root in AllRoots())
            {
                if (Directory.Exists(root) == false)
                    continue;

                foreach (string file in Directory.EnumerateFiles(root))
                    TryDelete(file);
            }
            return Task.CompletedTask;
        }

        // ── 경로 ────────────────────────────────────────────────────

        private static string FileNameOf(int imageNum) => $"{imageNum}.bin";

        private string CurrentRoot()
        {
            if (_settings.ImageCacheOnSdCard)
            {
                string? sd = SdCardRoot();
                if (sd != null)
                    return Path.Combine(sd, FolderName);
            }
            return Path.Combine(FileSystem.AppDataDirectory, FolderName);
        }

        /// <summary>내부 + SD카드. 위치를 바꾼 직후에도 기존 파일을 찾을 수 있게 둘 다 본다.</summary>
        private static IEnumerable<string> AllRoots()
        {
            yield return Path.Combine(FileSystem.AppDataDirectory, FolderName);

            string? sd = SdCardRoot();
            if (sd != null)
                yield return Path.Combine(sd, FolderName);
        }

        /// <summary>
        /// SD카드의 앱 전용 폴더. GetExternalFilesDirs 의 0번은 내장 공용 저장소이고,
        /// 1번부터가 제거 가능한 볼륨(SD카드 / USB)이다. 권한은 필요 없다.
        /// </summary>
        private static string? SdCardRoot()
        {
#if ANDROID
            try
            {
                var dirs = Android.App.Application.Context?.GetExternalFilesDirs(null);
                if (dirs == null || dirs.Length < 2)
                    return null;

                for (int idx = 1; idx < dirs.Length; idx++)
                {
                    string? path = dirs[idx]?.AbsolutePath;
                    if (string.IsNullOrEmpty(path))
                        continue;

                    // 마운트가 풀렸으면 경로는 있어도 쓸 수 없다.
                    if (Directory.Exists(path) == false)
                    {
                        try { Directory.CreateDirectory(path); }
                        catch { continue; }
                    }
                    return path;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] SD카드 확인 실패: {ex.Message}");
            }
#endif
            return null;
        }

        // ── 암호화 ──────────────────────────────────────────────────

        /// <summary>AES-CBC. 앞 16바이트가 IV 다.</summary>
        private static byte[] Encrypt(byte[] plain, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            byte[] body = encryptor.TransformFinalBlock(plain, 0, plain.Length);

            byte[] result = new byte[aes.IV.Length + body.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(body, 0, result, aes.IV.Length, body.Length);
            return result;
        }

        private static byte[] Decrypt(byte[] stored, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;

            byte[] iv = new byte[16];
            Buffer.BlockCopy(stored, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(stored, iv.Length, stored.Length - iv.Length);
        }

        /// <summary>캐시 전용 키. 없으면 만들어 SecureStorage 에 넣는다.</summary>
        private async Task<byte[]> GetKeyAsync()
        {
            if (_key != null)
                return _key;

            string? saved = null;
            try
            {
                saved = await SecureStorage.Default.GetAsync(KeyName);
            }
            catch (Exception)
            {
                // 키스토어가 초기화됐다. 새 키를 만들면 기존 캐시는 못 읽고 다시 받는다.
            }

            if (string.IsNullOrEmpty(saved) == false)
            {
                try
                {
                    _key = Convert.FromBase64String(saved);
                    if (_key.Length == 32)
                        return _key;
                }
                catch (FormatException)
                {
                    // 값이 깨졌다. 아래에서 새로 만든다.
                }
            }

            _key = RandomNumberGenerator.GetBytes(32);
            try
            {
                await SecureStorage.Default.SetAsync(KeyName, Convert.ToBase64String(_key));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] 키 저장 실패: {ex.Message}");
            }
            return _key;
        }

        private static void TryDelete(string path)
        {
            try { File.Delete(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
