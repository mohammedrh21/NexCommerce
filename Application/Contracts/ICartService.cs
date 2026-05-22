using NexCommerce.Application.DTOs.Cart;

namespace NexCommerce.Application.Contracts;

/// <summary>Read-only cart service. Mutations are handled by MediatR commands.</summary>
public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId, CancellationToken ct = default);
}
