using Newtonsoft.Json;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/id/image/add { id_num, image_data(파일), image_name }
    /// </summary>
    public class ResIdCardImageAdd : ResImageAddBase
    {
        public int id_image_num { get; set; }

        /// <summary>연결 키 = id_image_num</summary>
        [JsonIgnore]
        public override int LinkImageNum => id_image_num;
    }
}
