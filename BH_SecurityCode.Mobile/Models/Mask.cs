using System.Text;

namespace BH_SecurityCode.Mobile.Models
{
    /// <summary>민감한 값을 가려서 보여준다. 구분자(-, 공백)는 살려 자릿수 감을 유지한다.</summary>
    public static class Mask
    {
        private const char Dot = '•'; // •

        /// <summary>끝 <paramref name="tail"/> 자리만 남기고 가린다. "123-45-6789012" → "•••-••-•••9012"</summary>
        public static string Tail(string? value, int tail = 4)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            var digits = value.Where(char.IsLetterOrDigit).ToArray();
            if (digits.Length <= tail)
                return value;

            int keepFrom = digits.Length - tail;
            var sb = new StringBuilder(value.Length);
            int seen = 0;
            foreach (char ch in value)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(seen >= keepFrom ? ch : Dot);
                    seen++;
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        /// <summary>전부 가린다. 비밀번호처럼 길이도 알리지 않아야 하는 값에 쓴다.</summary>
        public static string All(string? value) =>
            string.IsNullOrEmpty(value) ? "" : new string(Dot, Math.Clamp(value.Length, 6, 12));
    }
}
