using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexCommerce.Domain.Entities;
using NexCommerce.Infrastructure.Identity;

namespace NexCommerce.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    private static readonly string[] Roles = ["Admin", "Vendor", "Customer"];

    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var logger      = serviceProvider.GetRequiredService<ILogger<RoleManager<IdentityRole<Guid>>>>();
        var dbContext   = serviceProvider.GetRequiredService<NexCommerceDbContext>();

        // Seed Roles
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (result.Succeeded)
                    logger.LogInformation("Role '{Role}' created.", role);
                else
                    logger.LogError("Failed to create role '{Role}': {Errors}", role,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed Languages
        if (!await dbContext.Languages.AnyAsync())
        {
            dbContext.Languages.AddRange(
                new Language { Id = Guid.NewGuid(), Code = "en", Name = "English", IsDefault = true },
                new Language { Id = Guid.NewGuid(), Code = "ar", Name = "Arabic", IsDefault = false }
            );
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded default languages (en, ar).");
        }
    }
}
