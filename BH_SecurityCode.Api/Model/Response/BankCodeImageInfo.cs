using Newtonsoft.Json;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// 보안카드 첨부 이미지 (bhr_bank_code_image)
    /// </summary>
    public class BankCodeImageInfo : ImageInfo
    {
        public int bank_code_image_num { get; set; }

        /// <summary>image/del 에 쓰는 연결 키</summary>
        [JsonIgnore]
        public override int LinkImageNum { get => bank_code_image_num; set => bank_code_image_num = value; }
    }
}
