using MediatR;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.Application.Features.CartFeatures.RemoveCartItem;

public record RemoveCartItemCommand(Guid UserId, Guid CartItemId) : IRequest<Unit>;

public sealed class RemoveCartItemCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveCartItemCommand, Unit>
{
    public async Task<Unit> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await cartRepository.FindItemAsync(request.CartItemId, request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart item not found.");

        cartRepository.RemoveItem(cartItem);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
