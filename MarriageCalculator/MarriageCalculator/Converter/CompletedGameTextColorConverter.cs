using System.Globalization;

namespace MarriageCalculator.Converter;

/// <summary>
/// Converter for DataGrid text colors based on game completion status
/// Active games: Full opacity dark text
/// Completed games: Reduced opacity gray text for "locked" appearance
/// </summary>
public class CompletedGameTextColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCompleted)
        {
            if (isCompleted)
            {
                // Completed game - Muted gray text
                return Color.FromArgb("#757575"); // Medium gray
            }
            else
            {
                // Active game - Full color text
                return Color.FromArgb("#2D3748"); // Dark slate
            }
        }
        
        return Color.FromArgb("#2D3748"); // Default to active
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

