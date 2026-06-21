using Client.Models.Cart;
using Fluxor;

namespace Client.State.Cart;

[FeatureState]
public record CartState
{
    public CartDto? Cart { get; init; }
    public bool IsLoading { get; init; }
    public bool IsUpdating { get; init; }
    public string? Error { get; init; }

    public int TotalItems => Cart?.TotalItems ?? 0;
    public decimal SubTotal => Cart?.SubTotal ?? 0;

    public CartState()
    {
        Cart = null;
        IsLoading = false;
        IsUpdating = false;
        Error = null;
    }
}
