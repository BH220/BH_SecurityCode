using BH_SecurityCode.Api;
using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Session;
using BH_SecurityCode.Mobile.Models;

namespace BH_SecurityCode.Mobile.Services
{
    /// <summary>
    /// BH_SecurityCode.Api 의 Manager 를 그대로 호출한다. 엔드포인트·파라미터·응답 파싱은 모두 Api 프로젝트가 갖고 있고,
    /// 여기서는 화면이 쓰기 좋은 형태로 옮기고 실패 사유를 판별하는 일만 한다.
    ///
    /// Manager 들은 실패를 빈 목록/null 로만 알려주므로(메시지가 없다),
    /// 결과가 비었을 때만 세션 상태와 /health 를 확인해 "서버 연결 실패 / 세션 만료 / 데이터 없음" 을 구분한다.
    /// </summary>
    public sealed class ApiVaultDataService : IVaultDataService
    {
        private readonly IBankCodeManager _bankCode;
        private readonly IBankBookManager _bankBook;
        private readonly ICardManager _card;
        private readonly IIdCardManager _idCard;
        private readonly IAccountManager _account;
        private readonly IImageManager _image;
        private readonly IImageCache _cache;

        private readonly Dictionary<Menus, List<VaultItem>> _lists = new();

        public ApiVaultDataService(
            IBankCodeManager bankCode,
            IBankBookManager bankBook,
            ICardManager card,
            IIdCardManager idCard,
            IAccountManager account,
            IImageManager image,
            IImageCache cache)
        {
            _bankCode = bankCode;
            _bankBook = bankBook;
            _card = card;
            _idCard = idCard;
            _account = account;
            _image = image;
            _cache = cache;
        }

        public void Clear() => _lists.Clear();

        public void Invalidate(Menus category) => _lists.Remove(category);

        public async Task<VaultResult<List<VaultItem>>> GetItemsAsync(Menus category, bool reload = false)
        {
            if (reload == false && _lists.TryGetValue(category, out var cached))
                return VaultResult<List<VaultItem>>.Ok(cached);

            try
            {
                var items = category switch
                {
                    Menus.보안코드 => (await _bankCode.GetListAsync()).Select(VaultItem.From).ToList(),
                    Menus.통장 => (await _bankBook.GetListAsync()).Select(VaultItem.From).ToList(),
                    Menus.카드 => (await _card.GetListAsync()).Select(VaultItem.From).ToList(),
                    Menus.신분증 => (await _idCard.GetListAsync()).Select(VaultItem.From).ToList(),
                    Menus.계정 => (await _account.GetListAsync()).Select(VaultItem.From).ToList(),
                    _ => new List<VaultItem>(),
                };

                if (items.Count == 0)
                {
                    string? problem = await DiagnoseAsync();
                    if (problem != null)
                        return VaultResult<List<VaultItem>>.Fail(problem);
                }

                _lists[category] = items;
                return VaultResult<List<VaultItem>>.Ok(items);
            }
            catch (Exception ex)
            {
                return VaultResult<List<VaultItem>>.Fail(await DiagnoseAsync() ?? ex.Message);
            }
        }

        public async Task<VaultResult<Dictionary<Menus, int>>> GetCountsAsync(bool reload = false)
        {
            var counts = new Dictionary<Menus, int>();
            string message = "";

            foreach (var meta in Categories.All)
            {
                var result = await GetItemsAsync(meta.Id, reload);
                counts[meta.Id] = result.Data?.Count ?? 0;
                if (result.Success == false && message.Length == 0)
                    message = result.Message;
            }

            return message.Length > 0
                ? VaultResult<Dictionary<Menus, int>>.Fail(message)
                : VaultResult<Dictionary<Menus, int>>.Ok(counts);
        }

        public Task<VaultResult<BankCodeInfo>> GetSecurityCodeAsync(int bankCodeNum) =>
            DetailAsync(() => _bankCode.GetDetailAsync(bankCodeNum));

        public Task<VaultResult<BankBookInfo>> GetBankBookAsync(int bankBookNum) =>
            DetailAsync(() => _bankBook.GetAsync(bankBookNum));

        public Task<VaultResult<CardInfo>> GetCardAsync(int cardNum) =>
            DetailAsync(() => _card.GetAsync(cardNum));

        public Task<VaultResult<IdCardInfo>> GetIdCardAsync(int idNum) =>
            DetailAsync(() => _idCard.GetAsync(idNum));

        public Task<VaultResult<AccountInfo>> GetAccountAsync(int accountNum) =>
            DetailAsync(() => _account.GetAsync(accountNum));

        // ── 저장 ────────────────────────────────────────────────────

        public Task<VaultResult<bool>> SaveSecurityCodeAsync(BankCodeInfo model, List<int> deleteImageIds) =>
            SaveAsync(Menus.보안코드, () => _bankCode.UpdateAsync(model, deleteImageIds));

        public Task<VaultResult<bool>> SaveBankBookAsync(BankBookInfo model, List<int> deleteImageIds) =>
            SaveAsync(Menus.통장, () => _bankBook.UpdateAsync(model, deleteImageIds));

        public Task<VaultResult<bool>> SaveCardAsync(CardInfo model, List<int> deleteImageIds) =>
            SaveAsync(Menus.카드, () => _card.UpdateAsync(model, deleteImageIds));

        public Task<VaultResult<bool>> SaveIdCardAsync(IdCardInfo model, List<int> deleteImageIds) =>
            SaveAsync(Menus.신분증, () => _idCard.UpdateAsync(model, deleteImageIds));

        public Task<VaultResult<bool>> SaveAccountAsync(AccountInfo model, List<int> deleteAccountIds) =>
            SaveAsync(Menus.계정, () => _account.UpdateAsync(model, deleteAccountIds));

        public Task<VaultResult<bool>> DeleteAsync(Menus category, int key) => SaveAsync(category, () => category switch
        {
            Menus.보안코드 => _bankCode.DeleteAsync(key),
            Menus.통장 => _bankBook.DeleteAsync(key),
            Menus.카드 => _card.DeleteAsync(key),
            Menus.신분증 => _idCard.DeleteAsync(key),
            Menus.계정 => _account.DeleteAsync(key),
            _ => Task.FromResult((false, "알 수 없는 종류입니다.")),
        });

        /// <summary>쓰기 공통 처리. 성공하면 해당 카테고리 캐시를 버려 목록이 다시 조회되게 한다.</summary>
        private async Task<VaultResult<bool>> SaveAsync(Menus category, Func<Task<(bool Success, string Message)>> action)
        {
            try
            {
                var (success, message) = await action();
                if (success == false)
                    return VaultResult<bool>.Fail(string.IsNullOrWhiteSpace(message)
                        ? await DiagnoseAsync() ?? "저장에 실패했습니다."
                        : message);

                Invalidate(category);
                // 일부만 실패한 경우(이미지 등)는 성공이지만 메시지가 따라온다.
                return VaultResult<bool>.Ok(true, message ?? "");
            }
            catch (Exception ex)
            {
                return VaultResult<bool>.Fail(await DiagnoseAsync() ?? ex.Message);
            }
        }

        /// <summary>
        /// 첨부 이미지 바이너리. 메모리 → 로컬 캐시 → 서버 순으로 찾는다.
        /// 서버에서 받은 것은 캐시에 넣어 다음부터는 다시 내려받지 않는다.
        /// </summary>
        public async Task<VaultResult<byte[]>> LoadImageAsync(ImageInfo image)
        {
            try
            {
                if (image.data is { Length: > 0 })
                    return VaultResult<byte[]>.Ok(image.data);

                byte[]? cached = await _cache.GetAsync(image.image_num);
                if (cached is { Length: > 0 })
                {
                    image.data = cached;
                    return VaultResult<byte[]>.Ok(cached);
                }

                var (success, message) = await _image.LoadDataAsync(image);
                if (success == false || image.data == null || image.data.Length == 0)
                    return VaultResult<byte[]>.Fail(string.IsNullOrEmpty(message) ? "이미지를 내려받지 못했습니다." : message);

                await _cache.SaveAsync(image.image_num, image.data);
                return VaultResult<byte[]>.Ok(image.data);
            }
            catch (Exception ex)
            {
                return VaultResult<byte[]>.Fail(ex.Message);
            }
        }

        private async Task<VaultResult<T>> DetailAsync<T>(Func<Task<T?>> load) where T : class
        {
            try
            {
                T? detail = await load();
                if (detail == null)
                    return VaultResult<T>.Fail(await DiagnoseAsync() ?? "항목을 찾을 수 없습니다.");

                return VaultResult<T>.Ok(detail);
            }
            catch (Exception ex)
            {
                return VaultResult<T>.Fail(await DiagnoseAsync() ?? ex.Message);
            }
        }

        /// <summary>비어있는 결과의 원인을 찾는다. 정상(데이터가 실제로 없음)이면 null.</summary>
        private static async Task<string?> DiagnoseAsync()
        {
            if (SessionManager.Instance.IsLive == false)
                return "세션이 만료되었습니다. 다시 로그인하세요.";

            bool alive = await Task.Run(() => ScApi.Instance.IsAlive());
            return alive ? null : "서버에 연결할 수 없습니다. 설정에서 서버 주소를 확인하세요.";
        }
    }
}
