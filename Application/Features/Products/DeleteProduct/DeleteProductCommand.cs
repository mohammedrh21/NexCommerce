using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.Application.Features.Products.DeleteProduct;

public record DeleteProductCommand(Guid ProductId, Guid RequestingUserId, bool IsAdmin) : IRequest;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteProductCommandHandler> logger)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.FindWithVendorAsync(command.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product {command.ProductId} not found.");

        if (!command.IsAdmin && product.Vendor.UserId != command.RequestingUserId)
            throw new ForbiddenException("You do not have permission to delete this product.");

        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product {ProductId} deleted by user {UserId}", command.ProductId, command.RequestingUserId);
    }
}
