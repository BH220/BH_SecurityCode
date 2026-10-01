using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/bank_code/delete { bank_code_num } - 행을 지우지 않고 사용불가(101002) 로 내린다.
    /// </summary>
    public class ResBankCodeDelete : ResBase
    {
        public int bank_code_num { get; set; }
        public CdStatus status { get; set; }
    }
}
