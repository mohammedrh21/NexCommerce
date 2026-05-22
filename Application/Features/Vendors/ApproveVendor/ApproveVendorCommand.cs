using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Vendors;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.Application.Features.Vendors.ApproveVendor;

public record ApproveVendorCommand(Guid VendorId, bool IsApproved) : IRequest<VendorProfileDto>;

public sealed class ApproveVendorCommandHandler(
    IVendorRepository vendorRepository,
    IUnitOfWork unitOfWork,
    IVendorService vendorService,
    ILogger<ApproveVendorCommandHandler> logger)
    : IRequestHandler<ApproveVendorCommand, VendorProfileDto>
{
    public async Task<VendorProfileDto> Handle(ApproveVendorCommand command, CancellationToken cancellationToken)
    {
        var vendorId = command.VendorId;
        var isApproved = command.IsApproved;

        var vendor = await vendorRepository.FindByIdAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException($"Vendor {vendorId} not found.");

        vendor.IsApproved = isApproved;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Vendor {VendorId} approval set to {IsApproved}", vendorId, isApproved);
        
        return await vendorService.GetByIdAsync(vendorId, "en", cancellationToken);
    }
}
