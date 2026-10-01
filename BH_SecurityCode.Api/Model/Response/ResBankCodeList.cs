using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    public class ResBankCodeList:ResBase
    {
        public List<BankCodeInfo> list { get; set; } = new List<BankCodeInfo>();
    }
}
