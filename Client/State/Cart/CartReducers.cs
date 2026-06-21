using Fluxor;

namespace Client.State.Cart;

public static class CartReducers
{
    [ReducerMethod]
    public static CartState OnLoadCart(CartState state, LoadCartAction _) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static CartState OnLoadCartSuccess(CartState state, LoadCartSuccessAction action) =>
        state with { IsLoading = false, Cart = action.Cart, Error = null };

    [ReducerMethod]
    public static CartState OnLoadCartFailure(CartState state, LoadCartFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static CartState OnAddToCart(CartState state, AddToCartAction _) =>
        state with { IsUpdating = true, Error = null };

    [ReducerMethod]
    public static CartState OnAddToCartSuccess(CartState state, AddToCartSuccessAction action) =>
        state with { IsUpdating = false, Cart = action.Cart };

    [ReducerMethod]
    public static CartState OnAddToCartFailure(CartState state, AddToCartFailureAction action) =>
        state with { IsUpdating = false, Error = action.Error };

    [ReducerMethod]
    public static CartState OnUpdateCartItem(CartState state, UpdateCartItemAction _) =>
        state with { IsUpdating = true, Error = null };

    [ReducerMethod]
    public static CartState OnUpdateCartItemSuccess(CartState state, UpdateCartItemSuccessAction action) =>
        state with { IsUpdating = false, Cart = action.Cart };

    [ReducerMethod]
    public static CartState OnUpdateCartItemFailure(CartState state, UpdateCartItemFailureAction action) =>
        state with { IsUpdating = false, Error = action.Error };

    [ReducerMethod]
    public static CartState OnRemoveCartItem(CartState state, RemoveCartItemAction _) =>
        state with { IsUpdating = true, Error = null };

    [ReducerMethod]
    public static CartState OnRemoveCartItemSuccess(CartState state, RemoveCartItemSuccessAction action) =>
        state with { IsUpdating = false, Cart = action.Cart };

    [ReducerMethod]
    public static CartState OnRemoveCartItemFailure(CartState state, RemoveCartItemFailureAction action) =>
        state with { IsUpdating = false, Error = action.Error };

    [ReducerMethod]
    public static CartState OnClearCart(CartState state, ClearCartAction _) =>
        state with { IsUpdating = true, Error = null };

    [ReducerMethod]
    public static CartState OnClearCartSuccess(CartState state, ClearCartSuccessAction _) =>
        state with { IsUpdating = false, Cart = null };

    [ReducerMethod]
    public static CartState OnClearCartFailure(CartState state, ClearCartFailureAction action) =>
        state with { IsUpdating = false, Error = action.Error };

    [ReducerMethod]
    public static CartState OnClearCartError(CartState state, ClearCartErrorAction _) =>
        state with { Error = null };
}
