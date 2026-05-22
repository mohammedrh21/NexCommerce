using MediatR;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Cart;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.Application.Features.CartFeatures.UpdateCartItem;

public record UpdateCartItemCommand(Guid UserId, Guid CartItemId, UpdateCartItemRequest Request) : IRequest<Unit>;

public sealed class UpdateCartItemCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCartItemCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await cartRepository.FindItemAsync(request.CartItemId, request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart item not found.");

        // Validate sufficient stock against the requested new quantity
        var variants = await productRepository.GetVariantsByIdsAsync(
            [cartItem.ProductVariantId], cancellationToken);

        var variant = variants.FirstOrDefault()
            ?? throw new NotFoundException($"Product variant {cartItem.ProductVariantId} no longer exists.");

        if (variant.StockQuantity < request.Request.Quantity)
            throw new ValidationException($"Only {variant.StockQuantity} unit(s) available in stock.");

        cartItem.Quantity = request.Request.Quantity;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
