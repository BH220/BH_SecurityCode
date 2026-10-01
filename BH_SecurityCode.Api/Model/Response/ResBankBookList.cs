using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/bank_book/list
    /// </summary>
    public class ResBankBookList : ResBase
    {
        public List<BankBookInfo> list { get; set; } = new List<BankBookInfo>();
    }
}
