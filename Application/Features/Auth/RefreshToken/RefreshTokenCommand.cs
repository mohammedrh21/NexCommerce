using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Auth;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponseDto>;

public sealed class RefreshTokenCommandHandler(
    IJwtTokenService jwtTokenService,
    IIdentityService identityService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenCommandHandler> logger,
    IConfiguration config)
    : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (valid, userId) = jwtTokenService.ValidateAccessToken(request.AccessToken);
        if (!valid)
            throw new UnauthorizedException("Invalid access token.");

        var storedToken = await refreshTokenRepository.FindValidAsync(userId, request.RefreshToken, cancellationToken);

        if (storedToken is null)
            throw new UnauthorizedException("Refresh token is invalid or expired.");

        var userInfo = await identityService.GetUserInfoAsync(userId);

        // Rotate refresh token
        storedToken.IsRevoked = true;
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();
        var newAccessToken  = jwtTokenService.GenerateAccessToken(userId, userInfo.Email, userInfo.Roles);
        var expiry          = DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:RefreshTokenInMinutes"] ?? "60"));

        refreshTokenRepository.Add(new Domain.Entities.RefreshToken
        {
            UserId     = userId,
            Token      = newRefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(config["Jwt:RefreshTokenInDays"] ?? "7"))
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Refresh token rotated for user {UserId}", userId);

        return new AuthResponseDto(
            UserId:            userId,
            FullName:          userInfo.FullName,
            Email:             userInfo.Email,
            Roles:             userInfo.Roles,
            AccessToken:       newAccessToken,
            RefreshToken:      newRefreshToken,
            AccessTokenExpiry: expiry);
    }
}
