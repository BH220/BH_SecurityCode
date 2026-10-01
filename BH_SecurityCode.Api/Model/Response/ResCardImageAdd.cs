using Newtonsoft.Json;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/card/image/add { card_num, image_data(파일), image_name }
    /// </summary>
    public class ResCardImageAdd : ResImageAddBase
    {
        public int card_image_num { get; set; }

        /// <summary>연결 키 = card_image_num</summary>
        [JsonIgnore]
        public override int LinkImageNum => card_image_num;
    }
}
