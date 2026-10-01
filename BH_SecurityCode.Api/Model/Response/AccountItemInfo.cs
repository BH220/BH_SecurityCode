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
    /// 계정 상세(하위 아이디/비밀번호) 한 건 (bhr_account_detail). 계정 상세보기 응답의 items 요소.
    /// </summary>
    public class AccountItemInfo
    {
        [JsonIgnore]
        public bool IsNewOrUpdate { get; set; } = false;
        public int account_detail_num { get; set; }
        public int account_num { get; set; }
        public string id { get; set; }
        public string pw { get; set; }
        public CdStatus status { get; set; }
    }
}
