using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/{bank_code|bank_book|card|id}/image/del { *_image_num } - 모든 항목 공용
    /// </summary>
    public class ResImageDelete : ResBase
    {
        public int image_num { get; set; }
    }
}
