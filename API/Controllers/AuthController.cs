using MediatR;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Auth;
using NexCommerce.Application.Features.Auth.Login;
using NexCommerce.Application.Features.Auth.RefreshToken;
using NexCommerce.Application.Features.Auth.Register;

namespace NexCommerce.API.Controllers;

public class AuthController(IMediator mediator) : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterCommand(request.FullName, request.Email, request.Password, request.Role), ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request.AccessToken, request.RefreshToken), ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }
}
