using Microsoft.JSInterop;

namespace Client.Services;

public interface IStripeInteropService
{
    Task<string?> InitializeAsync(string publishableKey);
    Task<string?> CreatePaymentMethodAsync(string cardholderName);
    Task<string?> ConfirmCardPaymentAsync(string clientSecret, string cardholderName);
    Task DestroyAsync();
}

/// <summary>
/// Wraps Stripe.js Elements via JS interop for secure card tokenization in Blazor WASM.
/// </summary>
public class StripeInteropService(IJSRuntime js) : IStripeInteropService, IAsyncDisposable
{
    private IJSObjectReference? _module;

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/stripe-interop.js");
        return _module;
    }

    /// <summary>Mounts Stripe Elements card input into the #stripe-card-element div.</summary>
    public async Task<string?> InitializeAsync(string publishableKey)
    {
        try
        {
            var module = await GetModuleAsync();
            return await module.InvokeAsync<string?>("initializeStripe", publishableKey);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>Creates a Stripe PaymentMethod and returns its ID for server-side processing.</summary>
    public async Task<string?> CreatePaymentMethodAsync(string cardholderName)
    {
        try
        {
            var module = await GetModuleAsync();
            return await module.InvokeAsync<string?>("createPaymentMethod", cardholderName);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Confirms a Stripe card payment on the client.</summary>
    public async Task<string?> ConfirmCardPaymentAsync(string clientSecret, string cardholderName)
    {
        try
        {
            var module = await GetModuleAsync();
            return await module.InvokeAsync<string?>("confirmCardPayment", clientSecret, cardholderName);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Cleans up Stripe Elements to prevent memory leaks.</summary>
    public async Task DestroyAsync()
    {
        if (_module is not null)
        {
            try { await _module.InvokeVoidAsync("destroyStripe"); }
            catch { /* Ignore */ }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DestroyAsync();
        if (_module is not null) await _module.DisposeAsync();
    }
}
