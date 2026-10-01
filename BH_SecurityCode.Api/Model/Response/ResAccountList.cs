using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/account/list
    /// </summary>
    public class ResAccountList : ResBase
    {
        public List<AccountInfo> list { get; set; } = new List<AccountInfo>();
    }
}
