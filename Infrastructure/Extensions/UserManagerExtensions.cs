using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexCommerce.Infrastructure.Identity;

namespace NexCommerce.Infrastructure.Extensions;

public static class UserManagerExtensions
{
    public static async Task<Dictionary<Guid, string>> GetUserNamesByIdsAsync(
        this UserManager<ApplicationUser> userManager, 
        IEnumerable<Guid> userIds)
    {
        var ids = userIds.Distinct().ToList();
        return await userManager.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName);
    }
}
