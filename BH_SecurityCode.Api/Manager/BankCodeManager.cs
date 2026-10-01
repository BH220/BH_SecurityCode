using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Utils;
using System.Reflection.Emit;
using System.Text;

namespace BH_SecurityCode.Api.Manager
{

    /// <summary>
    /// 아직 연동 전이라 모든 메서드가 <see cref="ApiNotImplementedException"/> 을 던진다. (화면에는 "구현 필요" 안내로 표시)
    /// </summary>
    public class BankCodeManager : IBankCodeManager
    {
        public async Task<List<BankCodeInfo>> GetListAsync()
        {
            ResBankCodeList res = await ScApi.Instance.PostResponseAsync<ResBankCodeList>("/api/security/bank_code/list");
            return res.result == 1 ? res.list.Where(x => x.status == CdStatus.사용).ToList() : new List<BankCodeInfo>();
        }

        public async Task<BankCodeInfo?> GetDetailAsync(int bankCodeNum)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("bank_code_num", bankCodeNum.ToString());
            ResBankCodeDetail res = await ScApi.Instance.PostResAsync<ResBankCodeDetail>("/api/security/bank_code/detail", param);
            return res.result == 1 ? res.detail : null; 
        }
          
        public async Task<(bool Success, string Message)> InsertAsync(BankCodeInfo model)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (model.bank_code_num > 0)
                param.Add("bank_code_num", model.bank_code_num.ToStringEx());
            param.Add("name", model.name);
            param.Add("bank_type", ((int)model.bank_type).ToStringEx());
            param.Add("code_align", ((int)model.code_align).ToStringEx()); 
            param.Add("owner_type", ((int)model.owner_type).ToStringEx()); 
            param.Add("code_qty", model.code_qty.ToStringEx()); 
            param.Add("serial", model.serial); 
            param.Add("note", model.note);
            StringBuilder sb = new StringBuilder();
            sb.AppendJoin(",", model.codes.Select(x => $"{x.First().Key}:{x.First().Value}"));
            param.Add("code_values", sb.ToString());
            ResBankCodeSave update = await ScApi.Instance.PostResAsync<ResBankCodeSave>("/api/security/bank_code/update", param);
            if (update.result != 1)
                return (false, update.msg);

            if (model.images != null && model.images.Count > 0)
            {
                int bankCodeNum = update.bank_code_num > 0 ? update.bank_code_num : model.bank_code_num;
                model.bank_code_num = bankCodeNum;
                ResBase imgResult = await ScApi.Instance.AddImages<ResBankCodeImageAdd>("/api/security/bank_code/image/add", "bank_code_num", bankCodeNum, model.images);
                return (imgResult.result == 1, imgResult.msg);
            }
            else
                return (update.result == 1, update.msg);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(BankCodeInfo model, List<int> deleteImageIds)
        {
            var (success, message) = await InsertAsync(model);
            if (success == false)
                return (false, message);

            var errors = new List<string>();
            if (string.IsNullOrEmpty(message) == false)
                errors.Add(message);
             
            if(deleteImageIds != null && deleteImageIds.Count > 0)
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                string nums = string.Join(",", deleteImageIds);
                param.Add("bank_code_image_num", nums);
                ResImageDelete res = await ScApi.Instance.PostResAsync<ResImageDelete>("/api/security/bank_code/image/del", param);
                if (res.result != 1)
                    errors.Add($"이미지 삭제({nums}): {res.msg}");
            }
            return (true, errors.Count == 0 ? "" : string.Join("\r\n", errors));
        }

        public async Task<(bool Success, string Message, int BankCodeNum)> SaveAsync(BankCodeInfo info, IReadOnlyDictionary<int, string> codes)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (info.bank_code_num > 0)
                param.Add("bank_code_num", info.bank_code_num.ToStringEx());
            param.Add("name", info.name);
            param.Add("bank_type", ((int)info.bank_type).ToStringEx());
            param.Add("code_align", ((int)info.code_align).ToStringEx());
            param.Add("owner_type", ((int)info.owner_type).ToStringEx());
            param.Add("code_qty", info.code_qty.ToStringEx());
            param.Add("serial", info.serial);
            param.Add("note", info.note);
            StringBuilder sb = new StringBuilder();
            sb.AppendJoin(",", codes.Select(c => $"{c.Key}:{c.Value}"));
            param.Add("code_values", sb.ToString());  
            ResBankCodeSave update = await ScApi.Instance.PostResAsync<ResBankCodeSave>("/api/security/bank_code/update", param);

            return (update.result == 1, update.msg, update.bank_code_num);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int bankCodeNum)
        {
            return await ChangeStatusAsync(new List<int>() { bankCodeNum }, CdStatus.사용불가);
        }

        public async Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> bankCodeNums, CdStatus status)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("status", ((int)status).ToStringEx());
            param.Add("bank_code_num_list", string.Join(",", bankCodeNums));
            ResBase update = await ScApi.Instance.PostResAsync<ResBase>("/api/security/bank_code/status", param);
            return (update.result == 1, update.msg);
        }
    }
}
