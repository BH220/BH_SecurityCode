using Newtonsoft.Json;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/bank_code/image/add { bank_code_num, image_data(파일), image_name }
    /// </summary>
    public class ResBankCodeImageAdd : ResImageAddBase
    {
        public int bank_code_image_num { get; set; }

        /// <summary>연결 키 = bank_code_image_num</summary>
        [JsonIgnore]
        public override int LinkImageNum => bank_code_image_num;
    }
}
