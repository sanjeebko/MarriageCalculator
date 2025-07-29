namespace MarriageCalculator.Core.DTOs;

public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Deleted { get; set; }
}

public class CreatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class GameSettingsDto
{
    public int Id { get; set; }
    public bool Murder { get; set; }
    public bool Kidnap { get; set; }
    public int SeenPoint { get; set; }
    public int UnseenPoint { get; set; }
    public double PointRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool Dublee { get; set; }
    public bool DubleePointLess { get; set; }
    public int DubleePointBonus { get; set; }
    public int FoulPoint { get; set; }
    public string FoulPointBonus { get; set; } = string.Empty;
    public bool Audio { get; set; }
}

public class CreateGameSettingsDto
{
    public bool Murder { get; set; }
    public bool Kidnap { get; set; }
    public int SeenPoint { get; set; }
    public int UnseenPoint { get; set; }
    public double PointRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool Dublee { get; set; }
    public bool DubleePointLess { get; set; }
    public int DubleePointBonus { get; set; }
    public int FoulPoint { get; set; }
    public string FoulPointBonus { get; set; } = string.Empty;
    public bool Audio { get; set; }
}

public class MarriageGameSetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime LastPlayed { get; set; }
    public DateTime Created { get; set; }
    public bool IsActive { get; set; }
    public int GameSettingsId { get; set; }
}

public class CreateMarriageGameSetDto
{
    public string Name { get; set; } = string.Empty;
    public int GameSettingsId { get; set; }
}

public class MarriageGameDto
{
    public int Id { get; set; }
    public int Sequence { get; set; }
    public int MarriageGameRoundId { get; set; }
    public int WinnerId { get; set; }
    public int DealerId { get; set; }
    public int TotalMaal { get; set; }
    public bool ClosedRound { get; set; }
    public DateTime CreatedTime { get; set; }
}

public class CreateMarriageGameDto
{
    public int Sequence { get; set; }
    public int MarriageGameRoundId { get; set; }
    public int WinnerId { get; set; }
    public int DealerId { get; set; }
    public int TotalMaal { get; set; }
    public bool ClosedRound { get; set; }
}

public class MarriageGameRoundDto
{
    public int Id { get; set; }
    public int Sequence { get; set; }
    public int MarriageGameSetId { get; set; }
    public bool Completed { get; set; }
}

public class CreateMarriageGameRoundDto
{
    public int Sequence { get; set; }
    public int MarriageGameSetId { get; set; }
    public bool Completed { get; set; }
}

public class MarriageGameScoreDto
{
    public int Id { get; set; }
    public int MarriageGameId { get; set; }
    public int PlayerId { get; set; }
    public bool Seen { get; set; }
    public bool Playing { get; set; }
    public int Maal { get; set; }
    public int BonusPoint { get; set; }
    public bool Duply { get; set; }
    public bool Winner { get; set; }
    public int Score { get; set; }
    public double MoneyWon { get; set; }
    public bool Deal { get; set; }
    public int Position { get; set; }
}

public class CreateMarriageGameScoreDto
{
    public int MarriageGameId { get; set; }
    public int PlayerId { get; set; }
    public bool Seen { get; set; }
    public bool Playing { get; set; }
    public int Maal { get; set; }
    public int BonusPoint { get; set; }
    public bool Duply { get; set; }
    public bool Winner { get; set; }
    public int Score { get; set; }
    public double MoneyWon { get; set; }
    public bool Deal { get; set; }
    public int Position { get; set; }
}

public class MarriageGameSetPlayerDto
{
    public int MarriageGameSetId { get; set; }
    public int PlayerId { get; set; }
}

public class CreateMarriageGameSetPlayerDto
{
    public int MarriageGameSetId { get; set; }
    public int PlayerId { get; set; }
}

public class DatabaseInfoDto
{
    public bool CanConnect { get; set; }
    public string Provider { get; set; } = string.Empty;
    public int TableCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}