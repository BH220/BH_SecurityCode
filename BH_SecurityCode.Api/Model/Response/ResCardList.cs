using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/card/list
    /// </summary>
    public class ResCardList : ResBase
    {
        public List<CardInfo> list { get; set; } = new List<CardInfo>();
    }
}
