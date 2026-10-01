using System.Globalization;

namespace BH_SecurityCode.Mobile.Converters
{
    /// <summary>bool 을 뒤집는다. "가려져 있을 때만 보이는" 아이콘 같은 곳에 쓴다.</summary>
    public sealed class InvertedBool : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool flag ? !flag : false;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool flag ? !flag : false;
    }
}
