using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Vendors;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Features.Vendors.CreateProfile;

public record CreateVendorProfileCommand(Guid UserId, CreateVendorProfileRequest Request) : IRequest<VendorProfileDto>;

public sealed class CreateVendorProfileCommandHandler(
    IVendorRepository vendorRepository,
    ILanguageRepository languageRepository,
    IUnitOfWork unitOfWork,
    IVendorService vendorService,
    ILogger<CreateVendorProfileCommandHandler> logger)
    : IRequestHandler<CreateVendorProfileCommand, VendorProfileDto>
{
    public async Task<VendorProfileDto> Handle(CreateVendorProfileCommand command, CancellationToken cancellationToken)
    {
        var userId = command.UserId;
        var request = command.Request;

        var exists = await vendorRepository.ExistsForUserAsync(userId, cancellationToken);
        if (exists)
            throw new ValidationException("Vendor profile already exists.");

        var langCodes = request.Translations.Select(t => t.LanguageCode).ToList();
        var languages = await languageRepository.GetByCodesAsync(langCodes, cancellationToken);

        var vendor = new VendorProfile { UserId = userId };
        vendor.Translations = request.Translations.Select(t => new VendorProfileTranslation
        {
            StoreName      = t.StoreName,
            Description    = t.Description,
            LanguageId     = languages.First(l => l.Code == t.LanguageCode).Id,
            VendorProfileId = vendor.Id
        }).ToList();

        vendorRepository.Add(vendor);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Vendor profile created for user {UserId}", userId);
        return await vendorService.GetByIdAsync(vendor.Id, "en", cancellationToken);
    }
}
