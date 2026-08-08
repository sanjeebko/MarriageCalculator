using AutoMapper;

namespace MarriageCalculator.Mapping;

/// <summary>
/// AutoMapper profile for mapping between DTOs and Models
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // GameSettings mappings
        CreateMap<GameSettingsDto, GameSettings>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => ParseCurrency(src.Currency)))
            .ForMember(dest => dest.FoulPointBonus, opt => opt.MapFrom(src => ParseFoulPointBonus(src.FoulPointBonus)));

        CreateMap<GameSettings, GameSettingsDto>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
            .ForMember(dest => dest.FoulPointBonus, opt => opt.MapFrom(src => src.FoulPointBonus.ToString()));

        CreateMap<GameSettings, CreateGameSettingsDto>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
            .ForMember(dest => dest.FoulPointBonus, opt => opt.MapFrom(src => src.FoulPointBonus.ToString()));

        // Player mappings
        CreateMap<PlayerDto, Player>();
        CreateMap<Player, PlayerDto>();
        CreateMap<Player, CreatePlayerDto>();
        CreateMap<Player, UpdatePlayerDto>();

        // MarriageGameSet mappings
        CreateMap<MarriageGameSetDto, MarriageGameSet>();
        CreateMap<MarriageGameSet, MarriageGameSetDto>();
        CreateMap<MarriageGameSet, CreateMarriageGameSetDto>();

        // MarriageGame mappings
        CreateMap<MarriageGameDto, MarriageGame>();
        CreateMap<MarriageGame, MarriageGameDto>();
        CreateMap<MarriageGame, CreateMarriageGameDto>();

        // MarriageGameRound mappings
        CreateMap<MarriageGameRoundDto, MarriageGameRound>();
        CreateMap<MarriageGameRound, MarriageGameRoundDto>();
        CreateMap<MarriageGameRound, CreateMarriageGameRoundDto>();

        // MarriageGameScore mappings
        CreateMap<MarriageGameScoreDto, MarriageGameScore>();
        CreateMap<MarriageGameScore, MarriageGameScoreDto>();
        CreateMap<MarriageGameScore, CreateMarriageGameScoreDto>();

        // MarriageGameSetPlayer mappings
        CreateMap<MarriageGameSetPlayerDto, MarriageGameSetPlayer>();
        CreateMap<MarriageGameSetPlayer, MarriageGameSetPlayerDto>();
        CreateMap<MarriageGameSetPlayer, CreateMarriageGameSetPlayerDto>();
    }

    /// <summary>
    /// Parse Currency enum from string with fallback
    /// </summary>
    private static Currency ParseCurrency(string currencyString)
    {
        return Enum.TryParse<Currency>(currencyString, out var currency) ? currency : Currency.NPR_Rupee;
    }

    /// <summary>
    /// Parse FoulPointBonusType enum from string with fallback
    /// </summary>
    private static FoulPointBonusType ParseFoulPointBonus(string foulBonusString)
    {
        return Enum.TryParse<FoulPointBonusType>(foulBonusString, out var foulBonus) ? foulBonus : FoulPointBonusType.NO_FOUL_POINT;
    }
}