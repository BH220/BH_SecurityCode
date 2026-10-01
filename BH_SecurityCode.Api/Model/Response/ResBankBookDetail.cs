using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/bank_book/detail { bank_book_num }
    /// </summary>
    public class ResBankBookDetail : ResBase
    {
        public BankBookInfo detail { get; set; }
    }
}
