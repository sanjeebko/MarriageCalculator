using CommunityToolkit.Mvvm.ComponentModel;

namespace MarriageCalculator.Models;

public partial class GameSettingsModel : ObservableObject
{
    public int GameSettingsId { get; set; }
    public int MarriageGameId { get; set; }

    [ObservableProperty]
    private bool _murder;

    [ObservableProperty]
    public bool _kidnap;

    [ObservableProperty]
    public int _seenPoint;

    [ObservableProperty]
    public int _unseenPoint;

    [ObservableProperty]
    public double _pointRate;

    [ObservableProperty]
    public Currency _currency;

    [ObservableProperty]
    public bool _dublee;

    [ObservableProperty]
    public bool _dubleePointLess;

    [ObservableProperty]
    public int _dubleePointBonus;

    [ObservableProperty]
    public int _foulPoint;

    [ObservableProperty]
    public FoulPointBonusType _foulPointBonus;

    [ObservableProperty]
    public List<Currency> _currencies;

    public GameSettingsModel()
    {
        _currencies = [.. Enum.GetValues(typeof(Currency)).Cast<Currency>().Order()];
        Defaults();
    }

    public void Defaults()
    {
        Murder = true;
        Kidnap = false;
        SeenPoint = 3;
        UnseenPoint = 10;
        PointRate = 10;
        Currency = Currency.NPR_Rupee;
        Dublee = true;
        DubleePointLess = true;
        FoulPoint = 15;
        FoulPointBonus = FoulPointBonusType.NEXT_GAME;
    }
}

public static class GameSettingsModelExtension
{
    public static GameSettings ToGameSettings(this GameSettingsModel model)
    => new()
    {
        Murder = model.Murder,
        Kidnap = model.Kidnap,
        SeenPoint = model.SeenPoint,
        UnseenPoint = model.UnseenPoint,
        PointRate = model.PointRate,
        Currency = model.Currency,
        Dublee = model.Dublee,
        DubleePointLess = model.DubleePointLess,
        DubleePointBonus = model.DubleePointBonus,
        FoulPoint = model.FoulPoint,
        FoulPointBonus = model.FoulPointBonus,
    };
}

public static class GameSettingsExtension
{
    public static GameSettingsModel ToGameSettingsModel(this GameSettings model)
    => new()
    {
        Murder = model.Murder,
        Kidnap = model.Kidnap,
        SeenPoint = model.SeenPoint,
        UnseenPoint = model.UnseenPoint,
        PointRate = model.PointRate,
        Currency = model.Currency ,
        Dublee = model.Dublee,
        DubleePointLess = model.DubleePointLess,
        DubleePointBonus = model.DubleePointBonus,
        FoulPoint = model.FoulPoint,
        FoulPointBonus = model.FoulPointBonus 
    };
}

