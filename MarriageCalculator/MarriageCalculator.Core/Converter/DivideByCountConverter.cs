using System.Globalization;

namespace MarriageCalculator.Core.Converter;

public class DivideByCountMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is double totalWidth && values[1] is int itemCount && itemCount > 0)
        { return totalWidth / itemCount -5* itemCount;  }
        return values[0];
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
