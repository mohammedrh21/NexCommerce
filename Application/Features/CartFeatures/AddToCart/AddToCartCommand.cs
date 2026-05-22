using MediatR;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Cart;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Features.CartFeatures.AddToCart;

public record AddToCartCommand(Guid UserId, AddToCartRequest Request) : IRequest<Unit>;

public sealed class AddToCartCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddToCartCommand, Unit>
{
    public async Task<Unit> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Validate variant exists and has sufficient stock
        var variants = await productRepository.GetVariantsByIdsAsync(
            [request.Request.ProductVariantId], cancellationToken);

        var variant = variants.FirstOrDefault()
            ?? throw new NotFoundException($"Product variant {request.Request.ProductVariantId} not found.");

        if (variant.StockQuantity < request.Request.Quantity)
            throw new ValidationException($"Only {variant.StockQuantity} unit(s) available in stock.");

        // Get or create the cart
        var cart = await cartRepository.FindByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null)
        {
            cart = new Cart { UserId = request.UserId };
            cartRepository.AddCart(cart);
        }

        // Increment quantity if the variant is already in cart, otherwise add new item
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == request.Request.ProductVariantId);
        if (existingItem is not null)
        {
            existingItem.Quantity += request.Request.Quantity;
        }
        else
        {
            cartRepository.AddItem(new CartItem
            {
                ProductVariantId = request.Request.ProductVariantId,
                Quantity         = request.Request.Quantity,
                Cart             = cart
            });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
