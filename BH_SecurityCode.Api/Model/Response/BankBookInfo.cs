using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// 통장 한 건. 목록(/api/security/bank_book/list) 과 상세(/api/security/bank_book/detail) 공용.
    /// images 는 상세에서만 채워진다.
    /// </summary>
    public class BankBookInfo
    {
        public int bank_book_num { get; set; }
        public string name { get; set; }
        public CdBank bank_type { get; set; }
        public CdOwner owner_type { get; set; }
        /// <summary>예금주. 등록/수정 요청(update)에만 쓰며, 목록/상세 응답에는 내려오지 않는다. (서버가 owner_enc 를 응답에 포함하지 않음)</summary>
        public string owner { get; set; }
        public string book_no { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public string note { get; set; }
        public CdStatus status { get; set; }

        /// <summary>첨부 이미지 목록 (상세에서만)</summary>
        public List<BankBookImageInfo> images { get; set; } = new List<BankBookImageInfo>();
    }
}
