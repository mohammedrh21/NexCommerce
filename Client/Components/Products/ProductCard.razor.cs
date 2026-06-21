using Microsoft.AspNetCore.Components;
using Client.Models.Products;
using System.Threading.Tasks;

namespace Client.Components.Products;

public partial class ProductCard : ComponentBase
{
    [Parameter] public ProductListItemDto Product { get; set; } = default!;
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public bool IsInWishlist { get; set; }
    [Parameter] public EventCallback<ProductListItemDto> OnWishlistToggle { get; set; }
    [Parameter] public EventCallback<ProductListItemDto> OnAddToCart { get; set; }

    protected async Task HandleWishlistToggle()
    {
        if (OnWishlistToggle.HasDelegate)
        {
            await OnWishlistToggle.InvokeAsync(Product);
        }
    }

    protected async Task HandleAddToCart()
    {
        if (OnAddToCart.HasDelegate)
        {
            await OnAddToCart.InvokeAsync(Product);
        }
    }
}
