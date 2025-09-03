using MarriageCalculator.Core.Models;
using MarriageCalculator.Core.Extensions;
using System.Globalization;
using System.Collections.ObjectModel;

namespace MarriageCalculator.Converter
{
    public class SelectedPlayerConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var selectedPlayer = value as Player;
            var currentPlayer = parameter as Player;
            if(selectedPlayer is null  || currentPlayer is null)
            {
                return Colors.Transparent;             
            }
            if (selectedPlayer == currentPlayer)
            {
                return Colors.LightGreen; // Highlight color for selected player
            }

            return Colors.Transparent; // Default color for unselected player
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to check if a player is in the active players collection and return appropriate background color
    /// </summary>
    public class PlayerSelectionStatusConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is not Player player)
                {
                    return Color.FromArgb("#4A4E69"); // Default background color
                }
                
                // Handle both ObservableCollection<Player> and ObservableRangeCollection<Player>
                if (parameter is not IEnumerable<Player> activePlayersCollection)
                {
                    return Color.FromArgb("#4A4E69"); // Default background color
                }

                // Check if the player is in the active players collection
                bool isSelected = activePlayersCollection.Any(p => p.Id == player.Id);
                
                return isSelected 
                    ? Color.FromArgb("#2ECC71") // Green background for selected players
                    : Color.FromArgb("#4A4E69"); // Default background for unselected players
            }
            catch (Exception ex)
            {
                return Color.FromArgb("#4A4E69"); // Default background color
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    /// <summary>
    /// Converter to capitalize the first letter of a string using ToFirstCharUpper extension method
    /// </summary>
    public class FirstCharUpperConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str.ToFirstCharUpper();
            }
            return value ?? string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CurrencyToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Currency currency)
            {
                return currency.ToDescriptionString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DivideByCountMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double totalWidth && values[1] is int itemCount && itemCount > 0)
            { return totalWidth / itemCount - 5 * itemCount; }
            return values[0];
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class FoulPointBonusTypeToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FoulPointBonusType foulBonusType)
            {
                return foulBonusType.GetDescription();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AllTrueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return false;

            foreach (var value in values)
            {
                if (value is bool boolValue && !boolValue)
                    return false;
                if (value is not bool)
                    return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// MultiBinding converter to check if a player is in the active players collection
    /// </summary>
    public class PlayerInActivePlayersMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values?.Length != 2)
                {
                    return Color.FromArgb("#4A4E69");
                }

                if (values[0] is not Player player)
                {
                    return Color.FromArgb("#4A4E69");
                }

                if (values[1] is not IEnumerable<Player> activePlayersCollection)
                {
                    return Color.FromArgb("#4A4E69");
                }

                bool isSelected = activePlayersCollection.Any(p => p.Id == player.Id);
                
                return isSelected 
                    ? Color.FromArgb("#2ECC71") // Green background for selected players
                    : Color.FromArgb("#4A4E69"); // Default background for unselected players
            }
            catch (Exception ex)
            {
                return Color.FromArgb("#4A4E69");
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Simple converter that uses the static ViewModel reference to check player selection status
    /// </summary>
    public class SimplePlayerSelectionConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is not Player player)
                {
                    return Color.FromArgb("#4A4E69");
                }

                // Get the ViewModel from the parameter (should be the BindingContext)
                if (parameter is PlayerSettingsViewModel viewModel)
                {
                    bool isSelected = viewModel.ActivePlayers.Any(p => p.Id == player.Id);
                    
                    return isSelected 
                        ? Color.FromArgb("#2ECC71") // Green background for selected players
                        : Color.FromArgb("#4A4E69"); // Default background for unselected players
                }
                
                return Color.FromArgb("#4A4E69");
            }
            catch (Exception ex)
            {
                return Color.FromArgb("#4A4E69");
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to check if a player name exists in a comma-separated string of names
    /// </summary>
    public class PlayerNameInListConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is not string activePlayerNames || parameter is not string playerName)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(activePlayerNames) || string.IsNullOrWhiteSpace(playerName))
                {
                    return false;
                }

                // Check if the player name exists in the comma-separated list
                var names = activePlayerNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                bool isSelected = names.Contains(playerName, StringComparer.OrdinalIgnoreCase);
                
                return isSelected;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Direct converter that uses ViewModel method to get background color
    /// </summary>
    public class DirectPlayerBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is not Player player)
                {
                    return Color.FromArgb("#4A4E69");
                }

                if (parameter is not PlayerSettingsViewModel viewModel)
                {
                    return Color.FromArgb("#4A4E69");
                }

                var backgroundColor = viewModel.GetPlayerBackgroundColor(player);
                
                return backgroundColor;
            }
            catch (Exception)
            {
                return Color.FromArgb("#4A4E69");
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
