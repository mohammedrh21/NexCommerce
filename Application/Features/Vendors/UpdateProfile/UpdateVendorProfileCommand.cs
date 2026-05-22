using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Vendors;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Features.Vendors.UpdateProfile;

public record UpdateVendorProfileCommand(Guid VendorId, Guid UserId, UpdateVendorProfileRequest Request) : IRequest<VendorProfileDto>;

public sealed class UpdateVendorProfileCommandHandler(
    IVendorRepository vendorRepository,
    ILanguageRepository languageRepository,
    IUnitOfWork unitOfWork,
    IVendorService vendorService,
    ILogger<UpdateVendorProfileCommandHandler> logger)
    : IRequestHandler<UpdateVendorProfileCommand, VendorProfileDto>
{
    public async Task<VendorProfileDto> Handle(UpdateVendorProfileCommand command, CancellationToken cancellationToken)
    {
        var vendorId = command.VendorId;
        var request = command.Request;

        var vendor = await vendorRepository.FindByIdAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException($"Vendor {vendorId} not found.");

        if (vendor.UserId != command.UserId)
            throw new ForbiddenException("You can only update your own vendor profile.");

        var langCodes = request.Translations.Select(t => t.LanguageCode).ToList();
        var languages = await languageRepository.GetByCodesAsync(langCodes, cancellationToken);

        foreach (var t in request.Translations)
        {
            var existing = vendor.Translations.FirstOrDefault(x => x.Language.Code == t.LanguageCode);
            if (existing is not null)
            {
                existing.StoreName   = t.StoreName;
                existing.Description = t.Description;
            }
            else
            {
                vendor.Translations.Add(new VendorProfileTranslation
                {
                    StoreName       = t.StoreName,
                    Description     = t.Description,
                    LanguageId      = languages.First(l => l.Code == t.LanguageCode).Id,
                    VendorProfileId = vendorId
                });
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Vendor profile {VendorId} updated", vendorId);
        
        return await vendorService.GetByIdAsync(vendorId, "en", cancellationToken);
    }
}
