using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Mobile.Models;

namespace BH_SecurityCode.Mobile.Services
{
    /// <summary>조회/저장 결과. 실패 사유를 화면에 그대로 보여주기 위해 메시지를 함께 돌려준다.</summary>
    public sealed class VaultResult<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        public T? Data { get; init; }

        public static VaultResult<T> Ok(T data) => new() { Success = true, Data = data };
        public static VaultResult<T> Ok(T data, string message) => new() { Success = true, Data = data, Message = message };
        public static VaultResult<T> Fail(string message) => new() { Success = false, Message = message };
    }

    /// <summary>
    /// 화면이 쓰는 데이터 통로. 실제 호출은 BH_SecurityCode.Api 의 Manager 들이 담당한다.
    /// (보안코드 /api/security/bank_code/*, 통장 bank_book, 카드 card, 신분증 id, 계정 account)
    /// </summary>
    public interface IVaultDataService
    {
        /// <summary><paramref name="reload"/> 가 false 면 캐시를 쓴다. (홈 타일과 목록이 같은 조회를 반복하지 않도록)</summary>
        Task<VaultResult<List<VaultItem>>> GetItemsAsync(Menus category, bool reload = false);

        /// <summary>카테고리별 건수. 홈 타일에 표시한다.</summary>
        Task<VaultResult<Dictionary<Menus, int>>> GetCountsAsync(bool reload = false);

        Task<VaultResult<BankCodeInfo>> GetSecurityCodeAsync(int bankCodeNum);
        Task<VaultResult<BankBookInfo>> GetBankBookAsync(int bankBookNum);
        Task<VaultResult<CardInfo>> GetCardAsync(int cardNum);
        Task<VaultResult<IdCardInfo>> GetIdCardAsync(int idNum);
        Task<VaultResult<AccountInfo>> GetAccountAsync(int accountNum);

        // ── 저장 ────────────────────────────────────────────────────
        // Manager 의 UpdateAsync 는 키가 0 이면 등록, 0 보다 크면 수정으로 동작하므로 한 가지 경로로 쓴다.

        Task<VaultResult<bool>> SaveSecurityCodeAsync(BankCodeInfo model, List<int> deleteImageIds);
        Task<VaultResult<bool>> SaveBankBookAsync(BankBookInfo model, List<int> deleteImageIds);
        Task<VaultResult<bool>> SaveCardAsync(CardInfo model, List<int> deleteImageIds);
        Task<VaultResult<bool>> SaveIdCardAsync(IdCardInfo model, List<int> deleteImageIds);
        Task<VaultResult<bool>> SaveAccountAsync(AccountInfo model, List<int> deleteAccountIds);

        /// <summary>삭제(서버에서는 사용불가 처리).</summary>
        Task<VaultResult<bool>> DeleteAsync(Menus category, int key);

        /// <summary>첨부 이미지 바이너리를 채운다. (GET /api/security/image/{image_num})</summary>
        Task<VaultResult<byte[]>> LoadImageAsync(ImageInfo image);

        /// <summary>해당 카테고리 캐시만 버린다. 저장·삭제 후 호출.</summary>
        void Invalidate(Menus category);

        /// <summary>캐시를 모두 버린다. 로그인/로그아웃 시 호출.</summary>
        void Clear();
    }
}
