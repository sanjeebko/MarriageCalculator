using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // PBKDF2 iterations

    public bool ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // At least 8 characters long
        if (password.Length < 8)
            return false;

        // At least one capital letter
        if (!Regex.IsMatch(password, @"[A-Z]"))
            return false;

        // At least one number OR one symbol
        bool hasNumber = Regex.IsMatch(password, @"[0-9]");
        bool hasSymbol = Regex.IsMatch(password, @"[^a-zA-Z0-9]");

        return hasNumber || hasSymbol;
    }

    public string HashPassword(string password, out string salt)
    {
        // Generate salt
        salt = GenerateSalt();
        
        // Hash password with salt
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);
        
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            // Hash the provided password with the stored salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), Iterations, HashAlgorithmName.SHA256);
            var testHash = pbkdf2.GetBytes(HashSize);
            var storedHash = Convert.FromBase64String(hash);
            
            // Compare the hashes securely
            return CryptographicOperations.FixedTimeEquals(testHash, storedHash);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[SaltSize];
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
}