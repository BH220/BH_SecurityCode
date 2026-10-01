using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// 보안카드 한 건. 목록(/api/security/bank_code/list) 과 상세(/api/security/bank_code/detail) 공용.
    /// codes, images 는 상세에서만 채워진다.
    /// </summary>
    public class BankCodeInfo
    {
        public int bank_code_num { get; set; }
        public string name { get; set; }
        public CdBank bank_type { get; set; }
        public CdOwner owner_type { get; set; }
        public int code_qty { get; set; }
        public string serial { get; set; }
        public CdAlign code_align { get; set; }
        public string note { get; set; }
        public CdStatus status { get; set; }

        /// <summary>코드 목록. 서버 형식은 [{"1":"1234"},{"2":"3456"}] (순번 → 4자리 코드)</summary>
        public List<Dictionary<int, string>> codes { get; set; } = new List<Dictionary<int, string>>();

        /// <summary>첨부 이미지 목록 (상세에서만)</summary>
        public List<BankCodeImageInfo> images { get; set; } = new List<BankCodeImageInfo>();

        /// <summary>codes 를 순번 → 코드 사전으로 펼친다.</summary>
        public Dictionary<int, string> GetCodeMap()
        {
            var map = new Dictionary<int, string>();
            foreach (var item in codes)
                foreach (var pair in item)
                    map[pair.Key] = pair.Value;
            return map;
        }
    }
}
