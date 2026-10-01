using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/id/detail { id_num }
    /// </summary>
    public class ResIdCardDetail : ResBase
    {
        public IdCardInfo detail { get; set; }
    }
}
