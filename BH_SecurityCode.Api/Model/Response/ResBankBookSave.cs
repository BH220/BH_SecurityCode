using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/bank_book/update (bank_book_num 이 없거나 0 이면 등록)
    /// </summary>
    public class ResBankBookSave : ResBase
    {
        public int bank_book_num { get; set; }

        /// <summary>true 면 신규 등록, false 면 수정</summary>
        public bool created { get; set; }
    }
}
