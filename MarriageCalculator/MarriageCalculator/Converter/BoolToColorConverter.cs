using System.Globalization;

namespace MarriageCalculator.Converter;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool hasError && parameter is string colorParameter)
        {
            var colors = colorParameter.Split('|');
            if (colors.Length >= 2)
            {
                var errorColor = colors[0]; // Error color
                var infoColor = colors[1];  // Info color
                
                if (hasError)
                {
                    return errorColor switch
                    {
                        "Error" => Color.FromArgb("#DC2626"), // Red color for errors
                        _ => Color.FromArgb("#DC2626")
                    };
                }
                else
                {
                    return infoColor switch
                    {
                        "Info" => Color.FromArgb("#059669"), // Green color for info/success
                        _ => Color.FromArgb("#059669")
                    };
                }
            }
        }
        
        // Default to info color
        return Color.FromArgb("#059669");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}