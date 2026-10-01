using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/account/detail { account_num } - detail.items 에 하위 계정 목록이 담긴다.
    /// </summary>
    public class ResAccountDetail : ResBase
    {
        public AccountInfo detail { get; set; }
    }
}
