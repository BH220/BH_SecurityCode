using BH_SecurityCode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model
{
    /// <summary>
    /// 공통코드 (BHB_CODE). 은행/카드사/신분증종류/소유자 등을 표현한다.
    /// </summary>
    public class CodeInfo
    {
        public int SqCode { get; set; }
        public string NmCode { get; set; } = "";
        public string TyCode { get; set; } = "";
        public string TxtNote { get; set; } = "";

        public CodeInfo() { }

        public CodeInfo(int sqCode, string nmCode, CodeTypes type, string note = "")
        {
            SqCode = sqCode;
            NmCode = nmCode;
            TyCode = ((int)type).ToString();
            TxtNote = note;
        }

        public override string ToString() => NmCode;
    }
}
