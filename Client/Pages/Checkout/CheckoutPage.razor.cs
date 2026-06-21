using Client.Models.Cart;
using Client.Services;
using Client.Models.Common;
using Client.Models.Coupons;
using Client.Models.Orders;
using Client.Models.Payments;
using Client.Services.Http;
using Client.State.Auth;
using Client.State.Cart;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Client.Pages.Checkout;

public partial class CheckoutPage : FluxorComponent, IAsyncDisposable
{
    [Inject] private IState<AuthState> AuthState { get; set; } = default!;
    [Inject] private ICartApiService CartApi { get; set; } = default!;
    [Inject] private IOrdersApiService OrdersApi { get; set; } = default!;
    [Inject] private ICouponsApiService CouponsApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IStripeInteropService StripeInterop { get; set; } = default!;
    [Inject] private IPaymentsApiService PaymentsApi { get; set; } = default!;
    [Inject] private IDispatcher Dispatcher { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;

    private CartDto? _cart;
    private bool _isLoading = true;
    private bool _isPlacingOrder = false;
    private bool _isApplyingCoupon = false;
    private bool _showValidation = false;

    private string? _shippingAddress;
    private string? _cardholderName;
    private string? _stripeInitError;
    private bool _shouldInitStripe = false;
    private string? _couponCode;
    private string? _couponError;
    private CouponValidationResultDto? _validatedCoupon;
    private decimal _discount = 0;

    private OrderDto? _placedOrder;
    private bool _orderPlaced = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCartAsync();
    }

    private async Task LoadCartAsync()
    {
        try
        {
            _isLoading = true;
            _cart = await CartApi.GetCartAsync();
        }
        catch (Exception)
        {
            _cart = null;
        }
        finally
        {
            _isLoading = false;
            _shouldInitStripe = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_shouldInitStripe && _cart is not null && !_orderPlaced)
        {
            _shouldInitStripe = false;
            await InitializeStripeAsync();
        }
    }

    private async Task InitializeStripeAsync()
    {
        try
        {
            var publishableKey = Configuration["StripePublishableKey"] ?? "pk_test_placeholder";
            _stripeInitError = await StripeInterop.InitializeAsync(publishableKey);
        }
        catch (Exception ex)
        {
            _stripeInitError = ex.Message;
        }
        StateHasChanged();
    }

    public new async ValueTask DisposeAsync()
    {
        try
        {
            await StripeInterop.DestroyAsync();
        }
        catch
        {
            // Ignore
        }
        await base.DisposeAsync();
    }

    private async Task ApplyCouponAsync()
    {
        if (string.IsNullOrWhiteSpace(_couponCode) || _cart is null) return;

        try
        {
            _isApplyingCoupon = true;
            _couponError = null;

            var result = await CouponsApi.ValidateAsync(new ValidateCouponRequest(_couponCode.Trim(), _cart.SubTotal));

            if (result.IsValid)
            {
                _validatedCoupon = result;
                _discount = result.DiscountAmount ?? 0;
                ToastService.Success($"Coupon applied! Saving {_discount:C}");
            }
            else
            {
                _couponError = result.ErrorMessage ?? "Invalid coupon code.";
                _validatedCoupon = null;
                _discount = 0;
            }
        }
        catch (Exception)
        {
            _couponError = "Could not validate coupon. Please try again.";
        }
        finally
        {
            _isApplyingCoupon = false;
        }
    }

    private void RemoveCoupon()
    {
        _validatedCoupon = null;
        _discount = 0;
        _couponCode = null;
        _couponError = null;
    }

    private async Task PlaceOrderAsync()
    {
        _showValidation = true;

        if (string.IsNullOrWhiteSpace(_shippingAddress) || _cart is null || string.IsNullOrWhiteSpace(_cardholderName))
        {
            ToastService.Error("Please fill in all required fields.");
            return;
        }

        try
        {
            _isPlacingOrder = true;

            OrderDto order;
            if (_placedOrder is null)
            {
                var request = new PlaceOrderRequest(
                    Items: _cart.Items.Select(i => new PlaceOrderItemRequest(i.ProductVariantId, i.Quantity)),
                    CouponCode: _validatedCoupon?.Code,
                    ShippingAddress: _shippingAddress.Trim());

                order = await OrdersApi.PlaceOrderAsync(request);
                _placedOrder = order;
            }
            else
            {
                order = _placedOrder;
            }

            if (order.Payment is null || string.IsNullOrEmpty(order.Payment.StripePaymentIntentId))
            {
                throw new Exception("Payment gateway did not respond correctly. Please try again.");
            }

            var clientSecret = order.Payment.StripePaymentIntentId;
            var paymentIntentId = await StripeInterop.ConfirmCardPaymentAsync(clientSecret, _cardholderName.Trim());

            if (string.IsNullOrEmpty(paymentIntentId))
            {
                throw new Exception("Payment confirmation failed. Please check your card details.");
            }

            var processRequest = new ProcessPaymentRequest(order.Id, paymentIntentId);
            var paymentResult = await PaymentsApi.ConfirmPaymentAsync(processRequest);

            if (paymentResult.Status != PaymentStatus.Succeeded)
            {
                throw new Exception("Payment verification failed on the server.");
            }

            // Order placed and paid successfully
            _placedOrder = order with { Status = OrderStatus.Processing };
            _orderPlaced = true;

            // Clear card inputs
            await StripeInterop.DestroyAsync();

            // Clear the cart in global state
            Dispatcher.Dispatch(new ClearCartAction());

            ToastService.Success("Payment successful! Your order has been placed.");
        }
        catch (Exception ex)
        {
            ToastService.Error(ex.Message);
        }
        finally
        {
            _isPlacingOrder = false;
        }
    }
}
