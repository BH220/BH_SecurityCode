using Newtonsoft.Json;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/bank_book/image/add { bank_book_num, image_data(파일), image_name }
    /// </summary>
    public class ResBankBookImageAdd : ResImageAddBase
    {
        public int bank_book_image_num { get; set; }

        /// <summary>연결 키 = bank_book_image_num</summary>
        [JsonIgnore]
        public override int LinkImageNum => bank_book_image_num;
    }
}
