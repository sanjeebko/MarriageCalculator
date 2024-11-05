using MarriageCalculator.Core.Models;
using MarriageCalculator.Models;
using System.Text.Json; 

namespace MarriageCalculator.Services;

public static class GameSettingsService
{

    private const string SettingsKey = "GameSettings";
    public static void SaveSettings(GameSettings settings)
    {
        var json = JsonSerializer.Serialize(settings);
        Preferences.Set(SettingsKey, json);
    }
    public static GameSettings LoadSettings()
    {
        var json = Preferences.Get(SettingsKey, string.Empty);

        var gameSettings = string.IsNullOrEmpty(json) ? GetDefaultSettings() : JsonSerializer.Deserialize<GameSettings>(json);

        return gameSettings??GetDefaultSettings();
    }

    private static GameSettings GetDefaultSettings()
    {
        return new GameSettings
        {
            Id = 0,
            MarriageGameId = 1,
            Murder = true,
            Kidnap = false,
            SeenPoint = 10,
            UnseenPoint = 5,
            PointRate = 10,
            Currency = Currency.NPR_Rupee,
            Dublee = false,
            DubleePointLess = true,
            DubleePointBonus = 5,
            FoulPoint = 15,
            FoulPointBonus = FoulPointBonusType.THIS_GAME,
        };
    }
}
