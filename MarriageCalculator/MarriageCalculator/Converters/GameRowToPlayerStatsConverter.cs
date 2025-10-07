using Microsoft.Maui.Controls;

namespace MarriageCalculator.Converters;

/// <summary>
/// Converter to extract PlayerStats from GameRowData for ScoreCapsuleControl
/// </summary>
public class GameRowToPlayerStatsConverter : IMultiValueConverter
{
    private readonly string _playerName;

    public GameRowToPlayerStatsConverter(string playerName)
    {
        _playerName = playerName;
    }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (values[0] is GameRowData gameRowData)
        {
            if (gameRowData.PlayerData.TryGetValue(_playerName, out var data))
            {
                // Parse the "Maal: X\nPoints: Y" format
                var lines = data.Split('\n');
                if (lines.Length >= 2)
                {
                    var maalValue = ExtractValue(lines[0]); // "Maal: X"
                    var pointsValue = ExtractValue(lines[1]); // "Points: Y"
                    
                    return new PlayerStats
                    {
                        Maal = int.TryParse(maalValue, out var m) ? m : 0,
                        Point = pointsValue,
                        Winner = gameRowData.IsWinner(_playerName),
                        Seen = gameRowData.HasSeen(_playerName)
                    };
                }
            }
        }
        
        return new PlayerStats { Maal = 0, Point = "0", Winner = false, Seen = false };
    }

    private string ExtractValue(string line)
    {
        var colonIndex = line.IndexOf(':');
        if (colonIndex > 0 && colonIndex < line.Length - 1)
        {
            return line.Substring(colonIndex + 1).Trim();
        }
        return "0";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}