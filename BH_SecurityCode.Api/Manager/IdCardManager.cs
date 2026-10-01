using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Helper;
using BH_SecurityCode.Core.Utils;

namespace BH_SecurityCode.Api.Manager
{
    /// <summary>
    /// 신분증 API. 상세 조회는 아직 연동 전(<see cref="ApiNotImplementedException"/> → 화면에 "구현 필요" 안내).
    /// 저장 결과의 Success 가 true 인데 Message 가 있으면 "본문은 저장됐지만 일부 이미지 처리 실패" 경고다.
    /// </summary>
    public class IdCardManager : IIdCardManager
    {
        public async Task<List<IdCardInfo>> GetListAsync()
        {
            ResIdCardList res = await ScApi.Instance.PostResponseAsync<ResIdCardList>("/api/security/id/list");
            return res.result == 1 ? res.list.Where(x => x.status == CdStatus.사용).ToList() : new List<IdCardInfo>();
        }

        public async Task<IdCardInfo?> GetAsync(int idNum)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("id_num", idNum.ToString());
            ResIdCardDetail res = await ScApi.Instance.PostResAsync<ResIdCardDetail>("/api/security/id/detail", param);
            return res.result == 1 ? res.detail : null;
        }

        public async Task<(bool Success, string Message)> InsertAsync(IdCardInfo model)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (model.id_num > 0)
                param.Add("id_num", model.id_num.ToStringEx());
            param.Add("name", model.name);
            param.Add("id_type", ((int)model.id_type).ToStringEx());
            param.Add("name_kor", model.name_kor);
            param.Add("name_eng", model.name_eng);
            param.Add("name_cha", model.name_cha);
            param.Add("id_no", model.id_no);
            param.Add("license_no", model.license_no);
            param.Add("license_org", model.license_org);
            param.Add("issue_date", model.issue_date.HasValue ? model.issue_date.Value.ToString("yyyyMMdd") : "");
            param.Add("expired_date", model.expired_date.HasValue ? model.expired_date.Value.ToString("yyyyMMdd") : "");
            param.Add("address", model.address);
            param.Add("add_condition", model.add_condition);
            param.Add("note", model.note);
            param.Add("owner", ((int)model.owner).ToStringEx());
            ResIdCardSave update = await ScApi.Instance.PostResAsync<ResIdCardSave>("/api/security/id/update", param);
            if (update.result != 1)
                return (false, update.msg);

            if (model.images != null && model.images.Count > 0)
            {
                int idNum = update.id_num > 0 ? update.id_num : model.id_num;
                model.id_num = idNum;
                ResBase imgResutl = await ScApi.Instance.AddImages<ResImageAddBase>("/api/security/id/image/add", "id_num", idNum, model.images);
                return (imgResutl.result == 1, imgResutl.msg);
            }
            else
                return (update.result == 1, update.msg);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(IdCardInfo model, List<int> deleteImageIds)
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
                param.Add("id_image_num", nums);
                ResImageDelete res = await ScApi.Instance.PostResAsync<ResImageDelete>("/api/security/id/image/del", param);
                if (res.result != 1)
                    errors.Add($"이미지 삭제({nums}): {res.msg}");
            }
            return (true, errors.Count == 0 ? "" : string.Join("\r\n", errors));
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int idNum)
        {
            return await ChangeStatusAsync(new List<int> { idNum }, CdStatus.사용불가);
        }

        public async Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> idNums, CdStatus status)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("status", ((int)status).ToStringEx());
            param.Add("id_num_list", string.Join(",", idNums));
            ResBase update = await ScApi.Instance.PostResAsync<ResBase>("/api/security/id/status", param);
            return (update.result == 1, update.msg);
        }
    }
}
