using MediatR;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.Application.Features.CartFeatures.ClearCart;

public record ClearCartCommand(Guid UserId) : IRequest<Unit>;

public sealed class ClearCartCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ClearCartCommand, Unit>
{
    public async Task<Unit> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.FindByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("Cart not found.");

        foreach (var item in cart.Items.ToList())
            cartRepository.RemoveItem(item);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
