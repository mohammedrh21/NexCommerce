using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;

namespace NexCommerce.API.Controllers;

/// <summary>
/// Admin-only endpoints for managing platform users via ASP.NET Identity.
/// </summary>
[Authorize(Roles = "Admin")]
public class UsersController(IIdentityService identityService) : ApiControllerBase
{
    /// <summary>
    /// Returns a paged list of all registered users.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await identityService.GetAllUsersAsync(page, pageSize);
        return Ok(ApiResponse<IEnumerable<UserInfoDto>>.Ok(result));
    }

    /// <summary>
    /// Returns detailed info for a single user by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await identityService.GetUserInfoAsync(id);
        return Ok(ApiResponse<UserInfoDto>.Ok(result));
    }

    /// <summary>
    /// Updates the full name of a user.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct = default)
    {
        var succeeded = await identityService.UpdateUserAsync(id, request.FullName);
        if (!succeeded)
            return BadRequest(ApiResponse.Fail("Failed to update user."));

        return Ok(ApiResponse.OkNoData("User updated successfully."));
    }

    /// <summary>
    /// Permanently deletes a user account.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
    {
        var succeeded = await identityService.DeleteUserAsync(id);
        if (!succeeded)
            return NotFound(ApiResponse.Fail($"User {id} not found or could not be deleted."));

        return Ok(ApiResponse.OkNoData("User deleted successfully."));
    }
}

/// <summary>Request body for updating a user's profile.</summary>
public sealed record UpdateUserRequest(string FullName);
