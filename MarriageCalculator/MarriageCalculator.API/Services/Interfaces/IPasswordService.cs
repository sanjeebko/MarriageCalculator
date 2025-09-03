namespace MarriageCalculator.API.Services.Interfaces;

public interface IPasswordService
{
    bool ValidatePasswordStrength(string password);
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string hash, string salt);
    string GenerateSalt();
}