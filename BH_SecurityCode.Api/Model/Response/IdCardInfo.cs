using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// 신분증 한 건. 목록(/api/security/id/list) 과 상세(/api/security/id/detail) 공용.
    /// 목록은 id_num, owner, id_type, name, name_kor, license_org, issue_date, expired_date, note 만 내려온다.
    /// 나머지 항목과 images 는 상세에서만 채워진다.
    /// </summary>
    public class IdCardInfo
    {
        public int id_num { get; set; }
        public CdOwner owner { get; set; }

        /// <summary>신분증 별칭</summary>
        public string name { get; set; }
        public CdIdType id_type { get; set; }
        public string name_kor { get; set; }
        public string name_eng { get; set; }
        public string name_cha { get; set; }

        /// <summary>주민등록번호</summary>
        public string id_no { get; set; }

        /// <summary>면허번호 / 여권번호</summary>
        public string license_no { get; set; }

        /// <summary>발급기관</summary>
        public string license_org { get; set; }
        public DateTime? issue_date { get; set; }
        public DateTime? expired_date { get; set; }
        public string address { get; set; }

        /// <summary>추가기재사항</summary>
        public string add_condition { get; set; }
        public string note { get; set; }
        public CdStatus status { get; set; }
        public DateTime? created_at { get; set; }

        /// <summary>첨부 이미지 목록 (상세에서만)</summary>
        public List<IdCardImageInfo> images { get; set; } = new List<IdCardImageInfo>();
    }
}
