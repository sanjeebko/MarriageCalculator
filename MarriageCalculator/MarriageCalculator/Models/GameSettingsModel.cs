using CommunityToolkit.Mvvm.ComponentModel;
using MarriageCalculator.Core.Models;
using MarriageCalculator.Services;

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
    public Currency  _currency;

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

    private readonly IMarriageGameServices _marriageGameServices;

    public GameSettingsModel(IMarriageGameServices marriageGameServices)
    {
        Defaults();
        _marriageGameServices = marriageGameServices;
        _currencies = _marriageGameServices.GetCurrency().Result;
    }

    public void Defaults()
    {
        Murder = true;
        Kidnap = false;
        SeenPoint = 3;
        UnseenPoint = 10;
        PointRate = 10;
        Currency =  Core.Models.Currency.GBP_Pence;        
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
    public static GameSettingsModel ToGameSettingsModel(this GameSettings model, IMarriageGameServices marriageGameServices)
    => new(marriageGameServices)
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

