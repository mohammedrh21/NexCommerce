using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;

namespace NexCommerce.Infrastructure.Identity;

/// <summary>
/// Concrete implementation of <see cref="IIdentityService"/> backed by
/// ASP.NET Core Identity <see cref="UserManager{TUser}"/>.
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _logger      = logger;
    }

    // ── Registration ───────────────────────────────────────────────────────────
    public async Task<(bool Succeeded, string UserId, IEnumerable<string> Errors)> RegisterAsync(
        string fullName, string email, string password, string role)
    {
        var user = new ApplicationUser
        {
            FullName         = fullName,
            Email            = email,
            UserName         = email,
            CreatedAt        = DateTime.UtcNow,
            IsEmailVerified  = false,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Registration failed for {Email}: {Errors}", email, string.Join(", ", errors));
            return (false, string.Empty, errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Role assignment failed for {Email}: {Errors}", email,
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User {Email} registered with role {Role}", email, role);
        return (true, user.Id.ToString(), Enumerable.Empty<string>());
    }

    // ── Login ──────────────────────────────────────────────────────────────────
    public async Task<(bool Succeeded, string UserId, string Email, IEnumerable<string> Roles)> LoginAsync(
        string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
        {
            _logger.LogWarning("Login failed for {Email}", email);
            return (false, string.Empty, string.Empty, Enumerable.Empty<string>());
        }

        var roles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("User {Email} logged in", email);
        return (true, user.Id.ToString(), user.Email!, roles);
    }

    // ── Queries ────────────────────────────────────────────────────────────────
    public async Task<bool> UserExistsAsync(string email)
        => await _userManager.FindByEmailAsync(email) is not null;

    public async Task<UserInfoDto> GetUserInfoAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException($"User '{userId}' was not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return new UserInfoDto(user.Id, user.FullName, user.Email!, user.IsEmailVerified, user.CreatedAt, roles);
    }

    public async Task<IEnumerable<UserInfoDto>> GetAllUsersAsync(int page, int pageSize)
    {
        var users = _userManager.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new List<UserInfoDto>(users.Count);
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserInfoDto(u.Id, u.FullName, u.Email!, u.IsEmailVerified, u.CreatedAt, roles));
        }

        return result;
    }

    // ── Mutations ──────────────────────────────────────────────────────────────
    public async Task<bool> AssignRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUserAsync(Guid userId, string fullName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.FullName = fullName;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}
