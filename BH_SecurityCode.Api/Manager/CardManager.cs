using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Utils;
using System;

namespace BH_SecurityCode.Api.Manager
{
    /// <summary>
    /// 아직 연동 전이라 모든 메서드가 <see cref="ApiNotImplementedException"/> 을 던진다. (화면에는 "구현 필요" 안내로 표시)
    /// </summary>
    public class CardManager : ICardManager
    {
        public async Task<List<CardInfo>> GetListAsync()
        {
            ResCardList res = await ScApi.Instance.PostResponseAsync<ResCardList>("/api/security/card/list"); 
            return res.result == 1? res.list.Where(x=>x.status == CdStatus.사용).ToList() : new List<CardInfo>();
        }

        public async Task<CardInfo?> GetAsync(int cardNum)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("card_num", cardNum.ToString());
            ResCardDetail res = await ScApi.Instance.PostResAsync<ResCardDetail>("/api/security/card/detail", param);
            return res.result == 1 ? res.detail : null;
        }

        public async Task<(bool Success, string Message)> InsertAsync(CardInfo model)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (model.card_num > 0)
                param.Add("card_num", model.card_num.ToStringEx());
            param.Add("name", model.name);
            param.Add("card_no", model.card_no);
            param.Add("expire_date", model.expire_date);
            param.Add("cvc", model.cvc);
            param.Add("note", model.note);
            param.Add("owner", ((int)model.owner).ToStringEx());
            param.Add("card_type", ((int)model.card_type).ToStringEx());
            param.Add("owner_name", model.owner_name);
            ResCardSave update = await ScApi.Instance.PostResAsync<ResCardSave>("/api/security/card/update", param);
            if (update.result != 1)
                return (false, update.msg);

            if (model.images != null && model.images.Count > 0)
            {
                int cardNum = update.card_num > 0 ? update.card_num : model.card_num;
                model.card_num = cardNum;
                ResBase imgResutl = await ScApi.Instance.AddImages<ResImageAddBase>("/api/security/card/image/add", "card_num", cardNum, model.images);
                return (imgResutl.result == 1, imgResutl.msg);
            }
            else
                return (update.result == 1, update.msg);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(CardInfo model, List<int> deleteImageIds)
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
                param.Add("card_image_num", nums);
                ResImageDelete res = await ScApi.Instance.PostResAsync<ResImageDelete>("/api/security/card/image/del", param);
                if (res.result != 1)
                    errors.Add($"이미지 삭제({nums}): {res.msg}");
            }
            return (true, errors.Count == 0 ? "" : string.Join("\r\n", errors));
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int cardNum)
        {
            return await ChangeStatusAsync(new List<int>() { cardNum }, CdStatus.사용불가);
        }

        public async Task<(bool Success, string Message)> ChangeStatusAsync(IEnumerable<int> cardNums, CdStatus status)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("status", ((int)status).ToStringEx());
            param.Add("card_num_list", string.Join(",", cardNums));
            ResBase update = await ScApi.Instance.PostResAsync<ResBase>("/api/security/card/status", param);
            return (update.result == 1, update.msg);
        }
    }
}
