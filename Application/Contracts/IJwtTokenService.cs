namespace NexCommerce.Application.Contracts;

/// <summary>JWT token generation and validation — implemented in Infrastructure.</summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
    (bool Valid, Guid UserId) ValidateAccessToken(string token);
}
