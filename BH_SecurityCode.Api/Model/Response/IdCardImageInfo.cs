using Newtonsoft.Json;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// 신분증 첨부 이미지 (bhr_id_image)
    /// </summary>
    public class IdCardImageInfo : ImageInfo
    {
        public int id_image_num { get; set; }

        /// <summary>image/del 에 쓰는 연결 키</summary>
        [JsonIgnore]
        public override int LinkImageNum { get => id_image_num; set => id_image_num = value; }
    }
}
