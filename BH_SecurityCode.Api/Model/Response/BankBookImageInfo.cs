using Newtonsoft.Json;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// 통장 첨부 이미지 (bhr_bank_book_image)
    /// </summary>
    public class BankBookImageInfo : ImageInfo
    {
        public int bank_book_image_num { get; set; }

        /// <summary>image/del 에 쓰는 연결 키</summary>
        [JsonIgnore]
        public override int LinkImageNum { get => bank_book_image_num; set => bank_book_image_num = value; }
    }
}
