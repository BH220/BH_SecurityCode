using Newtonsoft.Json;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// 카드 첨부 이미지 (bhr_card_image)
    /// </summary>
    public class CardImageInfo : ImageInfo
    {
        public int card_image_num { get; set; }

        /// <summary>image/del 에 쓰는 연결 키</summary>
        [JsonIgnore]
        public override int LinkImageNum { get => card_image_num; set => card_image_num = value; }
    }
}
