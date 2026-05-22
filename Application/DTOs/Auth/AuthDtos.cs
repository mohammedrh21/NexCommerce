namespace NexCommerce.Application.DTOs.Auth;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string Role = "Customer");

public record LoginRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken);

public record AuthResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    IEnumerable<string> Roles,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry);

public record UserProfileDto(
    Guid Id,
    string FullName,
    string Email,
    bool IsEmailVerified,
    DateTime CreatedAt,
    IEnumerable<string> Roles);
