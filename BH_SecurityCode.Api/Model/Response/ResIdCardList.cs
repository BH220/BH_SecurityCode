using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/id/list
    /// </summary>
    public class ResIdCardList : ResBase
    {
        public List<IdCardInfo> list { get; set; } = new List<IdCardInfo>();
    }
}
