using System.Globalization;

namespace MarriageCalculator.Converter;

public class ConnectionStatusToColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 || values[0] is not bool isConnected || values[1] is not bool isConnecting)
        {
            return Colors.Gray; // Default color
        }

        // If connecting, show gray
        if (isConnecting)
        {
            return Colors.Gray;
        }

        // If connected, show green; otherwise gray
        return isConnected ? Colors.Green : Colors.Gray;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}