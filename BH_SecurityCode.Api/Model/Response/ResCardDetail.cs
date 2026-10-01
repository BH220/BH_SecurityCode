using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/card/detail { card_num }
    /// </summary>
    public class ResCardDetail : ResBase
    {
        public CardInfo detail { get; set; }
    }
}
