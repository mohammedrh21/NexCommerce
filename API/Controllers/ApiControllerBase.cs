using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NexCommerce.API.Controllers;

/// <summary>
/// Abstract base for all API controllers.
/// Centralises common claim-extraction properties so controllers stay thin and DRY.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>The authenticated user's ID. Throws if user is not authenticated.</summary>
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>The authenticated user's ID, or null if the request is anonymous.</summary>
    protected Guid? CurrentUserIdNullable =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>All roles assigned to the authenticated user.</summary>
    protected IEnumerable<string> CurrentUserRoles =>
        User.FindAll(ClaimTypes.Role).Select(c => c.Value);

    /// <summary>True when the current user holds the Admin role.</summary>
    protected bool IsAdmin => User.IsInRole("Admin");

    /// <summary>True when the current user holds the Vendor role.</summary>
    protected bool IsVendor => User.IsInRole("Vendor");
}
