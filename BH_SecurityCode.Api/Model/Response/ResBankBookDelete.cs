using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/bank_book/delete { bank_book_num } - 사용불가(101002) 처리
    /// </summary>
    public class ResBankBookDelete : ResBase
    {
        public int bank_book_num { get; set; }
        public CdStatus status { get; set; }
    }
}
