using BH_SecurityCode.Api.Model;
using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Interface
{
    /// <summary>
    /// 공통코드. 코드 조회 API 가 없으므로 DB(bhs_code) 와 맞춰 둔 <c>BH_SecurityCode.Core.CodeEnum</c> 의 enum 에서 만든다.
    /// </summary>
    public interface ICodeManager
    {
        Task<List<CodeInfo>> GetCodesAsync(CodeTypes codeType);
    }
}
