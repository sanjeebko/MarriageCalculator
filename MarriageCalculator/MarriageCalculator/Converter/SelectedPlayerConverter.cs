using MarriageCalculator.Core.Models;
using System.Globalization;

namespace MarriageCalculator.Converter
{
    public class SelectedPlayerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedPlayer = value as Player;
            var currentPlayer = parameter as Player;

            if (selectedPlayer == currentPlayer)
            {
                return Colors.LightGreen; // Highlight color for selected player
            }

            return Colors.Transparent; // Default color for unselected player
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
