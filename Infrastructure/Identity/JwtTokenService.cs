using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NexCommerce.Application.Contracts;

namespace NexCommerce.Infrastructure.Identity;

/// <summary>
/// JWT access-token generation and validation using HMAC-SHA256 symmetric signing.
/// Refresh tokens are cryptographically random opaque strings.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration config, ILogger<JwtTokenService> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>Creates a signed JWT access token for the given user identity.</summary>
    public string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles)
    {
        var secret  = _config["Jwt:Secret"]   ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer   = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var expiry   = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 60;

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier,     userId.ToString()),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: creds);

        _logger.LogDebug("Access token generated for user {UserId}, expires in {Expiry}m", userId, expiry);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Creates a cryptographically secure random refresh token.</summary>
    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    /// <summary>
    /// Validates signature and extracts UserId from an access token without checking expiry.
    /// Used during the refresh-token flow.
    /// </summary>
    public (bool Valid, Guid UserId) ValidateAccessToken(string token)
    {
        try
        {
            var secret = _config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

            var handler    = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateIssuer           = false,   // issuer already validated at request time
                ValidateAudience         = false,   // audience already validated at request time
                ValidateLifetime         = false,   // intentionally skip — allow expired tokens for refresh
                ClockSkew                = TimeSpan.Zero,
            };

            var principal = handler.ValidateToken(token, parameters, out _);
            var rawId     = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (Guid.TryParse(rawId, out var userId))
                return (true, userId);

            _logger.LogWarning("Token validation: userId claim missing or unparseable.");
            return (false, Guid.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed.");
            return (false, Guid.Empty);
        }
    }
}
