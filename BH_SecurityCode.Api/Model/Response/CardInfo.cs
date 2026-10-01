using BH_SecurityCode.Core;
using Newtonsoft.Json;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// 카드 한 건. 목록(/api/security/card/list) 과 상세(/api/security/card/detail) 공용.
    /// images 는 상세에서만 채워진다.
    /// </summary>
    public class CardInfo
    {
        public int card_num { get; set; }
        public string name { get; set; }
        public string card_no { get; set; }
        public CdOwner owner { get; set; }
        public CdCardCompany card_type { get; set; }

        /// <summary>카드 명의자</summary>
        public string owner_name { get; set; }

        /// <summary>유효기간 yyyyMM (예: 202303)</summary>
        public string expire_date { get; set; }
        public string cvc { get; set; }

        /// <summary>expire_date(yyyyMM) ↔ DateTime 변환 (클라이언트 전용: 입력 DatePicker / 목록 표시용)</summary>
        [JsonIgnore]
        public DateTime? ExpireDate
        {
            get => DateTime.TryParseExact(expire_date, "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : null;
            set => expire_date = value?.ToString("yyyyMM") ?? "";
        }
        public string note { get; set; }
        public CdStatus status { get; set; }

        /// <summary>첨부 이미지 목록 (상세에서만)</summary>
        public List<CardImageInfo> images { get; set; } = new List<CardImageInfo>();
    }
}
