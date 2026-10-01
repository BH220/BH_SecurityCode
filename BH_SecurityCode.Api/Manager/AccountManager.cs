using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Utils;

namespace BH_SecurityCode.Api.Manager
{

    /// <summary>
    /// 아직 연동 전이라 모든 메서드가 <see cref="ApiNotImplementedException"/> 을 던진다. (화면에는 "구현 필요" 안내로 표시)
    /// </summary>
    public class AccountManager : IAccountManager
    {
        public async Task<List<AccountInfo>> GetListAsync()
        {
            ResAccountList res = await ScApi.Instance.PostResponseAsync<ResAccountList>("/api/security/account/list");
            return res.result == 1 ? res.list.Where(x => x.status == CdStatus.사용).ToList() : new List<AccountInfo>();
        }

        public async Task<AccountInfo?> GetAsync(int accountNum)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("account_num", accountNum.ToString());
            ResAccountDetail res = await ScApi.Instance.PostResAsync<ResAccountDetail>("/api/security/account/detail", param);
            return res.result == 1 ? res.detail : null;
        }

        public async Task<(bool Success, string Message)> InsertAsync(AccountInfo model)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (model.account_num > 0)
                param.Add("account_num", model.account_num.ToStringEx());
            param.Add("owner", ((int)model.owner).ToStringEx());
            param.Add("name", model.name);
            param.Add("address", model.address);
            param.Add("note", model.note);
            ResAccountSave update = await ScApi.Instance.PostResAsync<ResAccountSave>("/api/security/account/update", param);
            if (update.result != 1)
                return (false, update.msg);

            var errors = new List<string>();
            if (model.items != null && model.items.Count > 0)
            {
                int account_num = update.account_num > 0 ? update.account_num : model.account_num;
                model.account_num = account_num;
                foreach (AccountItemInfo item in model.items.Where(x => x.IsNewOrUpdate))
                {
                    Dictionary<string, string> itemParam = new Dictionary<string, string>();
                    if (item.account_detail_num > 0)
                        itemParam.Add("account_detail_num", item.account_detail_num.ToStringEx());
                    // 신규 등록이면 하위 항목에는 account_num 이 아직 없다. 위에서 확정된 값을 쓴다.
                    itemParam.Add("account_num", account_num.ToStringEx());
                    itemParam.Add("id", item.id);
                    itemParam.Add("pw", item.pw);
                    ResBase itemResult = await ScApi.Instance.PostResAsync<ResBase>("/api/security/account/sub/update", itemParam);
                    if (itemResult.result != 1)
                        errors.Add($"계정 하위 항목({item.id}): {itemResult.msg}");
                }
            }
            if (errors.Count > 0 || update.result != 1)
                return (false, $"{update.msg}, {string.Join(", ", errors)}");
            else
                return (true, update.msg);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(AccountInfo model, List<int> deleteAccountIds)
        {
            var (success, message) = await InsertAsync(model);
            if (success == false)
                return (false, message);

            var errors = new List<string>();
            if (string.IsNullOrEmpty(message) == false)
                errors.Add(message);

            if (deleteAccountIds != null && deleteAccountIds.Count > 0)
            {
                string nums = string.Join(",", deleteAccountIds);
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("status", ((int)CdStatus.사용불가).ToStringEx());
                param.Add("account_num_list", nums);
                ResBase update = await ScApi.Instance.PostResAsync<ResBase>("/api/security/account/sub/status", param);
                if (update.result != 1)
                    errors.Add($"계정 하위 항목 삭제({nums}): {update.msg}");
            }
            return (true, errors.Count == 0 ? "" : string.Join("\r\n", errors));
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int accountNum)
        { 
            return await ChangeStatusAsync(new List<int>() { accountNum }, CdStatus.사용불가);
        }

        public async Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> accountNums, CdStatus status)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("status", ((int)status).ToStringEx());
            param.Add("account_num_list", string.Join(",", accountNums)); 
            ResBase update = await ScApi.Instance.PostResAsync<ResBase>("/api/security/account/status", param);
            return (update.result == 1, update.msg);
        }
    }
}
