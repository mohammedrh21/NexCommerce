using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Auth;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Features.Auth.Register;

public record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    string Role) : IRequest<AuthResponseDto>;

public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    IJwtTokenService jwtTokenService,
    IVendorRepository vendorRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ILogger<RegisterCommandHandler> logger,
    IConfiguration config)
    : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, errors) = await identityService.RegisterAsync(
            request.FullName, request.Email, request.Password, request.Role);

        if (!succeeded)
            throw new ValidationException(errors);

        var userGuid = Guid.Parse(userId);

        // If registering as Vendor, create a pending VendorProfile
        if (request.Role == "Vendor")
        {
            vendorRepository.Add(new VendorProfile { UserId = userGuid });
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Vendor profile created for user {UserId}", userGuid);
        }

        // Issue tokens
        var roles = new[] { request.Role };
        var accessToken  = jwtTokenService.GenerateAccessToken(userGuid, request.Email, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var expiry       = DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:AccessTokenMinutes"] ?? "60"));

        refreshTokenRepository.Add(new Domain.Entities.RefreshToken
        {
            UserId     = userGuid,
            Token      = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(config["Jwt:RefreshTokenInDays"] ?? "7"))
        });
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Fire-and-forget welcome email
        _ = emailService.SendWelcomeEmailAsync(request.Email, request.FullName, cancellationToken);

        logger.LogInformation("User {Email} registered successfully with role {Role}", request.Email, request.Role);

        return new AuthResponseDto(
            UserId:           userGuid,
            FullName:         request.FullName,
            Email:            request.Email,
            Roles:            roles,
            AccessToken:      accessToken,
            RefreshToken:     refreshToken,
            AccessTokenExpiry: expiry);
    }
}
