using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Model;
using BH_SecurityCode.Core; 

namespace BH_SecurityCode.Api.Manager
{
    public class CodeManager : ICodeManager
    {
        private readonly Dictionary<CodeTypes, List<CodeInfo>> _cache = new();

        public Task<List<CodeInfo>> GetCodesAsync(CodeTypes codeType)
        {
            if (_cache.TryGetValue(codeType, out var cached) == false)
            {
                cached = codeType switch
                {
                    CodeTypes.Bank => FromEnum<CdBank>(codeType),
                    CodeTypes.Card => FromEnum<CdCardCompany>(codeType),
                    CodeTypes.ID_Card => FromEnum<CdIdType>(codeType),
                    CodeTypes.Person => FromEnum<CdOwner>(codeType),
                    _ => new List<CodeInfo>(),
                };
                _cache[codeType] = cached;
            }
            return Task.FromResult(cached);
        }

        /// <summary>enum 멤버(이름 = 표시명, 값 = 코드) → CodeInfo</summary>
        private static List<CodeInfo> FromEnum<TEnum>(CodeTypes codeType) where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(x => new CodeInfo(Convert.ToInt32(x), x.ToString(), codeType))
                .ToList();
        }
    }
}
