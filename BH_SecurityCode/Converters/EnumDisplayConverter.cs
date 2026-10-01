using System.Globalization;
using System.Windows.Data;

namespace BH_SecurityCode.Converters
{
    /// <summary>Enum 멤버 이름의 선행 '_' 를 제거하여 표시한다. (_3자리25개 → 3자리25개)</summary>
    public class EnumDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString()?.TrimStart('_') ?? "";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
