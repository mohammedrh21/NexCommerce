namespace NexCommerce.Application.Contracts;

/// <summary>User identity operations — implemented in Infrastructure using ASP.NET Identity.</summary>
public interface IIdentityService
{
    Task<(bool Succeeded, string UserId, IEnumerable<string> Errors)> RegisterAsync(
        string fullName, string email, string password, string role);

    Task<(bool Succeeded, string UserId, string Email, IEnumerable<string> Roles)> LoginAsync(
        string email, string password);

    Task<bool> UserExistsAsync(string email);
    Task<UserInfoDto> GetUserInfoAsync(Guid userId);
    Task<bool> AssignRoleAsync(Guid userId, string role);
    Task<IEnumerable<UserInfoDto>> GetAllUsersAsync(int page, int pageSize);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> UpdateUserAsync(Guid userId, string fullName);
}

public record UserInfoDto(
    Guid Id,
    string FullName,
    string Email,
    bool IsEmailVerified,
    DateTime CreatedAt,
    IEnumerable<string> Roles);
