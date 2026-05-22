using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Auth;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.Application.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, email, roles) = await identityService.LoginAsync(request.Email, request.Password);

        if (!succeeded)
            throw new UnauthorizedException("Invalid email or password.");

        var userGuid = Guid.Parse(userId);
        var accessToken = jwtTokenService.GenerateAccessToken(userGuid, email, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddMinutes(60);

        // Revoke any existing refresh tokens for this user
        await refreshTokenRepository.RevokeAllForUserAsync(userGuid, cancellationToken);

        refreshTokenRepository.Add(new Domain.Entities.RefreshToken
        {
            UserId = userGuid,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {Email} logged in successfully", email);

        var userInfo = await identityService.GetUserInfoAsync(userGuid);

        return new AuthResponseDto(
            UserId: userGuid,
            FullName: userInfo.FullName,
            Email: email,
            Roles: roles,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            AccessTokenExpiry: expiry);
    }
}
