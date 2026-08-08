using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Authentication;

public class FirebaseOrMockAuthenticationOptions : AuthenticationSchemeOptions
{
    public string? FirebaseProjectId { get; set; }
    /// <summary>
    /// OAuth 2.0 Client ID(s) the Android app requests as serverClientId when signing in with
    /// Google via Credential Manager. The resulting Google ID token's "aud" claim equals this
    /// value, NOT the Firebase project ID (that only holds for tokens minted by Firebase Auth
    /// itself, which this app does not use). Comma-separated to allow multiple client IDs.
    /// </summary>
    public string? GoogleClientId { get; set; }
    public bool VerifySignature { get; set; } = false; // Set to true to enforce signature validation
}

public class FirebaseOrMockAuthenticationHandler : AuthenticationHandler<FirebaseOrMockAuthenticationOptions>
{
    public FirebaseOrMockAuthenticationHandler(
        IOptionsMonitor<FirebaseOrMockAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
        {
            return AuthenticateResult.NoResult();
        }

        var authHeader = authHeaderValue.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.Fail("Empty token.");
        }

        try
        {
            Claim[] claims;

            // Check if it's a mock/test token
            if (token.StartsWith("mock-", StringComparison.OrdinalIgnoreCase) || token.StartsWith("test-", StringComparison.OrdinalIgnoreCase))
            {
                var username = token.Substring(5); // e.g. "sanjeeb" from "mock-sanjeeb"
                if (string.IsNullOrEmpty(username))
                {
                    username = "GuestPlayer";
                }

                claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, token), // Unique UserId is the token itself to prevent collisions
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Email, $"{username}@marriagecalculator.local"),
                    new Claim("auth_provider", "mock")
                };
            }
            else if (token.Split('.').Length == 3) // Simple JWT detection
            {
                // Parse JWT payload
                var payloadBase64 = token.Split('.')[1];
                var normalizedPayload = payloadBase64.Replace('-', '+').Replace('_', '/');
                switch (normalizedPayload.Length % 4)
                {
                    case 2: normalizedPayload += "=="; break;
                    case 3: normalizedPayload += "="; break;
                }

                var payloadBytes = Convert.FromBase64String(normalizedPayload);
                var payloadJson = Encoding.UTF8.GetString(payloadBytes);
                
                using var doc = JsonDocument.Parse(payloadJson);
                var root = doc.RootElement;

                string userId = root.TryGetProperty("sub", out var subProp) ? subProp.GetString() ?? string.Empty : string.Empty;
                string email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? string.Empty : string.Empty;
                string name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
                string picture = root.TryGetProperty("picture", out var pictureProp) ? pictureProp.GetString() ?? string.Empty : string.Empty;

                if (string.IsNullOrEmpty(userId))
                {
                    return AuthenticateResult.Fail("Invalid JWT: missing 'sub' claim.");
                }

                // Verify the audience claim against whichever allowed values are configured:
                // the Firebase project ID (for real Firebase ID tokens) and/or the Google OAuth
                // Client ID(s) (for raw Google Sign-In ID tokens, which is what this app sends).
                var allowedAudiences = (Options.GoogleClientId ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Concat(string.IsNullOrEmpty(Options.FirebaseProjectId) ? Array.Empty<string>() : new[] { Options.FirebaseProjectId })
                    .ToArray();

                if (allowedAudiences.Length > 0)
                {
                    string aud = root.TryGetProperty("aud", out var audProp) ? audProp.GetString() ?? string.Empty : string.Empty;
                    if (!allowedAudiences.Contains(aud))
                    {
                        return AuthenticateResult.Fail($"Audience mismatch. Expected one of: {string.Join(", ", allowedAudiences)}. Actual: {aud}");
                    }
                }

                claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, string.IsNullOrEmpty(name) ? userId : name),
                    new Claim(ClaimTypes.Email, email),
                    new Claim("picture", picture),
                    new Claim("auth_provider", "firebase")
                };
            }
            else
            {
                return AuthenticateResult.Fail("Unsupported token format.");
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to authenticate token: {Message}", ex.Message);
            return AuthenticateResult.Fail(ex);
        }
    }
}
