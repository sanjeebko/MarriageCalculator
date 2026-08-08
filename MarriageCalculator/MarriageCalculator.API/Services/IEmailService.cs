using System.Threading.Tasks;

namespace MarriageCalculator.API.Services;

public interface IEmailService
{
    /// <summary>Whether an email provider is configured (Email:Host present).</summary>
    bool IsConfigured { get; }

    /// <summary>Sends an email. Returns false (with a logged warning) when unconfigured or on failure.</summary>
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody);
}
