using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Utils;

namespace BH_SecurityCode.Api.Manager
{
    /// <summary>
    /// 통장 API (/api/security/bank_book/*). IdCardManager / CardManager 와 같은 구조.
    /// 저장 결과의 Success 가 true 인데 Message 가 있으면 "본문은 저장됐지만 일부 이미지 처리 실패" 경고다.
    /// </summary>
    public class BankBookManager : IBankBookManager
    {
        public async Task<List<BankBookInfo>> GetListAsync()
        {
            ResBankBookList res = await ScApi.Instance.PostResponseAsync<ResBankBookList>("/api/security/bank_book/list");
            return res.result == 1 ? res.list.Where(x => x.status == CdStatus.사용).ToList() : new List<BankBookInfo>();
        }

        public async Task<BankBookInfo?> GetAsync(int bankBookNum)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("bank_book_num", bankBookNum.ToStringEx());
            ResBankBookDetail res = await ScApi.Instance.PostResAsync<ResBankBookDetail>("/api/security/bank_book/detail", param);
            return res.result == 1 ? res.detail : null;
        }

        public async Task<(bool Success, string Message)> InsertAsync(BankBookInfo model)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (model.bank_book_num > 0)
                param.Add("bank_book_num", model.bank_book_num.ToStringEx());
            param.Add("name", model.name);
            param.Add("owner", model.owner);                   // 예금주 (응답에는 없고 요청에만 쓴다)
            param.Add("bank_type", ((int)model.bank_type).ToStringEx());
            param.Add("owner_type", ((int)model.owner_type).ToStringEx());
            param.Add("book_no", model.book_no);
            param.Add("start_date", model.start_date.HasValue ? model.start_date.Value.ToString("yyyyMMdd") : "");
            param.Add("end_date", model.end_date.HasValue ? model.end_date.Value.ToString("yyyyMMdd") : "");
            param.Add("note", model.note);
            ResBankBookSave update = await ScApi.Instance.PostResAsync<ResBankBookSave>("/api/security/bank_book/update", param);
            if (update.result != 1)
                return (false, update.msg);

            if (model.images != null && model.images.Count > 0)
            {
                // 신규 등록이면 서버가 발급한 bank_book_num 으로 이미지를 연결한다.
                int bankBookNum = update.bank_book_num > 0 ? update.bank_book_num : model.bank_book_num;
                model.bank_book_num = bankBookNum;
                ResBase imgResult = await ScApi.Instance.AddImages<ResBankBookImageAdd>("/api/security/bank_book/image/add", "bank_book_num", bankBookNum, model.images);
                return (imgResult.result == 1, imgResult.msg);
            }
            else
                return (update.result == 1, update.msg);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(BankBookInfo model, List<int> deleteImageIds)
        {
            var (success, message) = await InsertAsync(model);
            if (success == false)
                return (false, message);

            var errors = new List<string>();
            if (string.IsNullOrEmpty(message) == false)
                errors.Add(message); 

            if (deleteImageIds != null && deleteImageIds.Count > 0)
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                string nums = string.Join(",", deleteImageIds);
                param.Add("bank_book_image_num", nums);
                ResImageDelete res = await ScApi.Instance.PostResAsync<ResImageDelete>("/api/security/bank_book/image/del", param);
                if (res.result != 1)
                    errors.Add($"이미지 삭제({nums}): {res.msg}");
            }
            return (true, errors.Count == 0 ? "" : string.Join("\r\n", errors));
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int bankBookNum)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("bank_book_num", bankBookNum.ToStringEx());
            ResBase update = await ScApi.Instance.PostResAsync<ResBase>("/api/security/bank_book/delete", param);
            return (update.result == 1, update.msg);
        }

        public async Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> bankBookNums, CdStatus status)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("status", ((int)status).ToStringEx());
            param.Add("bank_book_num_list", string.Join(",", bankBookNums));
            ResBase update = await ScApi.Instance.PostResAsync<ResBase>("/api/security/bank_book/status", param);
            return (update.result == 1, update.msg);
        }
    }
}
