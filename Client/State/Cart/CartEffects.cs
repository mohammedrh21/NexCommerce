using Client.Services;
using Client.Services.Http;
using Fluxor;

namespace Client.State.Cart;

public class CartEffects(ICartApiService cartApi, IToastService toast)
{
    [EffectMethod]
    public async Task HandleLoadCart(LoadCartAction _, IDispatcher dispatcher)
    {
        try
        {
            var cart = await cartApi.GetCartAsync();
            dispatcher.Dispatch(new LoadCartSuccessAction(cart));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadCartFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleAddToCart(AddToCartAction action, IDispatcher dispatcher)
    {
        try
        {
            var cart = await cartApi.AddItemAsync(action.Request);
            dispatcher.Dispatch(new AddToCartSuccessAction(cart));
            toast.Success("Item added to cart!");
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new AddToCartFailureAction(ex.Message));
            toast.Error($"Failed to add item: {ex.Message}");
        }
    }

    [EffectMethod]
    public async Task HandleUpdateCartItem(UpdateCartItemAction action, IDispatcher dispatcher)
    {
        try
        {
            var cart = await cartApi.UpdateItemAsync(action.ItemId, action.Request);
            dispatcher.Dispatch(new UpdateCartItemSuccessAction(cart));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new UpdateCartItemFailureAction(ex.Message));
            toast.Error($"Failed to update item: {ex.Message}");
        }
    }

    [EffectMethod]
    public async Task HandleRemoveCartItem(RemoveCartItemAction action, IDispatcher dispatcher)
    {
        try
        {
            var cart = await cartApi.RemoveItemAsync(action.ItemId);
            dispatcher.Dispatch(new RemoveCartItemSuccessAction(cart));
            toast.Info("Item removed.");
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new RemoveCartItemFailureAction(ex.Message));
            toast.Error($"Failed to remove item: {ex.Message}");
        }
    }

    [EffectMethod]
    public async Task HandleClearCart(ClearCartAction _, IDispatcher dispatcher)
    {
        try
        {
            await cartApi.ClearCartAsync();
            dispatcher.Dispatch(new ClearCartSuccessAction());
            toast.Info("Cart cleared.");
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new ClearCartFailureAction(ex.Message));
            toast.Error($"Failed to clear cart: {ex.Message}");
        }
    }
}
