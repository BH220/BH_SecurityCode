using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// 계정(사이트) 한 건. 목록(/api/security/account/list) 과 상세(/api/security/account/detail) 공용.
    /// id(대표 아이디) 는 목록에서만, status 와 items(하위 계정) 는 상세에서만 채워진다.
    /// </summary>
    public class AccountInfo
    {
        public int account_num { get; set; }
        public CdOwner owner { get; set; }

        /// <summary>사이트명</summary>
        public string name { get; set; }
        public string address { get; set; }
        public string note { get; set; }
        public CdStatus status { get; set; }

        /// <summary>대표 아이디 (목록에서만)</summary>
        public string id { get; set; }

        /// <summary>아직 서버에 올리지 않은 신규 첨부 (image/add 대상)</summary>
        [JsonIgnore]
        public bool IsNew => account_num <= 0;

        /// <summary>하위 계정(아이디/비밀번호) 목록 (상세에서만)</summary>
        public List<AccountItemInfo> items { get; set; } = new List<AccountItemInfo>();
    }
}
