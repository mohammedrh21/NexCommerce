namespace Client.Models.Users;

public record UserInfoDto(
    Guid Id,
    string FullName,
    string Email,
    bool IsEmailVerified,
    DateTime CreatedAt,
    IEnumerable<string> Roles);

public record UpdateUserRequest(string FullName);
