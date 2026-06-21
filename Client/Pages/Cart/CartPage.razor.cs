using Client.Models.Cart;
using Client.Services;
using Client.Models.Coupons;
using Client.Services.Http;
using Client.State.Auth;
using Client.State.Cart;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Cart;

public partial class CartPage : FluxorComponent
{
    [Inject] private IState<AuthState> AuthState { get; set; } = default!;
    [Inject] private IState<CartState> CartState { get; set; } = default!;
    [Inject] private IDispatcher Dispatcher { get; set; } = default!;
    [Inject] private ICouponsApiService CouponsApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private bool _isApplyingCoupon = false;
    private string? _couponCode;
    private string? _couponError;
    private CouponValidationResultDto? _appliedCoupon;

    private decimal _discount => GetDiscount();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (AuthState.Value.IsAuthenticated)
        {
            Dispatcher.Dispatch(new LoadCartAction());
        }
    }

    private decimal GetDiscount()
    {
        var cart = CartState.Value.Cart;
        if (_appliedCoupon is null || cart is null) return 0;

        if (_appliedCoupon.DiscountType == Client.Models.Common.DiscountType.Percentage)
        {
            return Math.Round(cart.SubTotal * (_appliedCoupon.DiscountValue ?? 0) / 100, 2);
        }
        else
        {
            return Math.Min(_appliedCoupon.DiscountValue ?? 0, cart.SubTotal);
        }
    }

    private void Increment(CartItemDto item)
    {
        if (item.Quantity >= item.AvailableStock) return;
        Dispatcher.Dispatch(new UpdateCartItemAction(item.Id, new UpdateCartItemRequest(item.Quantity + 1)));
    }

    private void Decrement(CartItemDto item)
    {
        if (item.Quantity <= 1) return;
        Dispatcher.Dispatch(new UpdateCartItemAction(item.Id, new UpdateCartItemRequest(item.Quantity - 1)));
    }

    private void RemoveItem(Guid itemId)
    {
        Dispatcher.Dispatch(new RemoveCartItemAction(itemId));
    }

    private void ClearCart()
    {
        Dispatcher.Dispatch(new ClearCartAction());
        _appliedCoupon = null;
        _couponCode = null;
        _couponError = null;
    }

    private async Task ApplyCouponAsync()
    {
        var cart = CartState.Value.Cart;
        if (string.IsNullOrWhiteSpace(_couponCode) || cart is null) return;

        try
        {
            _isApplyingCoupon = true;
            _couponError = null;

            var result = await CouponsApi.ValidateAsync(new ValidateCouponRequest(_couponCode.Trim(), cart.SubTotal));

            if (result.IsValid)
            {
                _appliedCoupon = result;
                ToastService.Success($"Coupon applied! You save {GetDiscount():C}");
            }
            else
            {
                _couponError = result.ErrorMessage ?? "Invalid coupon code.";
                _appliedCoupon = null;
            }
        }
        catch (Exception)
        {
            _couponError = "Failed to validate coupon. Please try again.";
        }
        finally
        {
            _isApplyingCoupon = false;
        }
    }

    private void RemoveCoupon()
    {
        _appliedCoupon = null;
        _couponCode = null;
        _couponError = null;
        ToastService.Info("Coupon removed.");
    }
}
