using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    /// <summary>
    /// POST /api/security/{항목}/status { status, {항목}_num_list } - 사용상태 일괄 변경. 모든 항목 공용
    /// </summary>
    public class ResStatusChange : ResBase
    {
        /// <summary>변경된 건수</summary>
        public int changed { get; set; }
        public CdStatus status { get; set; }
    }
}
