using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/bank_code/detail { bank_code_num }
    /// </summary>
    public class ResBankCodeDetail : ResBase
    {
        public BankCodeInfo detail { get; set; }
    }
}
