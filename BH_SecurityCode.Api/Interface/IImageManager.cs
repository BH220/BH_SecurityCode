using BH_SecurityCode.Api.Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Interface
{
    /// <summary>
    /// 첨부 이미지 원본 - GET /api/security/image/{image_num} (인증 필요, 바이너리 응답).
    /// 보안카드/통장/카드/신분증 모든 항목이 공용으로 쓴다.
    /// </summary>
    public interface IImageManager
    {
        /// <summary>
        /// 이미지 바이너리를 받아 <see cref="ImageInfo.data"/> 에 채운다.
        /// 이미 채워져 있으면(신규 첨부 / 이전에 받아 둔 것) 서버에 다시 요청하지 않는다.
        /// </summary>
        Task<(bool Success, string Message)> LoadDataAsync(ImageInfo image);
    }
}
