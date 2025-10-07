using System.Globalization;
using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Converter;

/// <summary>
/// Converter for DataGrid row background colors based on game completion status
/// Active (incomplete) games: Vibrant gradient background with green accent
/// Completed (locked) games: Muted gray gradient with lock icon aesthetic
/// </summary>
public class DataGridColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            // Check if the value is null
            if (value == null)
                return CreateActiveGradient();

            // This converter only works with MarriageRoundAndGamesModel
            if (value is MarriageRoundAndGamesModel model)
            {
                return model.Completed ? Colors.LightGray : Colors.LightGreen   ;
            }

            // Default to active state if not the expected model type
            return CreateActiveGradient();
        }
        catch (Exception)
        {
            // If any error occurs, return default active gradient
            return CreateActiveGradient();
        }
    }

    /// <summary>
    /// Creates a vibrant gradient for active (incomplete) games
    /// Green to light blue gradient suggesting "ready to play"
    /// </summary>
    private LinearGradientBrush CreateActiveGradient()
    {
        try
        {
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Color.FromArgb("#E8F5E9"), Offset = 0.0f },  // Very light green
                    new GradientStop { Color = Color.FromArgb("#C8E6C9"), Offset = 0.5f },  // Light green
                    new GradientStop { Color = Color.FromArgb("#A5D6A7"), Offset = 1.0f }   // Soft green
                }
            };
        }
        catch
        {
            // Fallback to solid color if gradient creation fails
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.LightGreen, Offset = 0.0f }
                }
            };
        }
    }

    /// <summary>
    /// Creates a muted gray gradient for completed (locked) games
    /// Gray gradient with slight warmth suggesting "archived/completed"
    /// </summary>
    private LinearGradientBrush CreateCompletedGradient()
    {
        try
        {
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Color.FromArgb("#F5F5F5"), Offset = 0.0f },  // Very light gray
                    new GradientStop { Color = Color.FromArgb("#E0E0E0"), Offset = 0.5f },  // Light gray
                    new GradientStop { Color = Color.FromArgb("#BDBDBD"), Offset = 1.0f }   // Medium gray
                }
            };
        }
        catch
        {
            // Fallback to solid color if gradient creation fails
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.LightGray, Offset = 0.0f }
                }
            };
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
