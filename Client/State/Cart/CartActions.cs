using Client.Models.Cart;

namespace Client.State.Cart;

// Load cart
public record LoadCartAction;
public record LoadCartSuccessAction(CartDto Cart);
public record LoadCartFailureAction(string Error);

// Add item
public record AddToCartAction(AddToCartRequest Request);
public record AddToCartSuccessAction(CartDto Cart);
public record AddToCartFailureAction(string Error);

// Update item quantity
public record UpdateCartItemAction(Guid ItemId, UpdateCartItemRequest Request);
public record UpdateCartItemSuccessAction(CartDto Cart);
public record UpdateCartItemFailureAction(string Error);

// Remove item
public record RemoveCartItemAction(Guid ItemId);
public record RemoveCartItemSuccessAction(CartDto Cart);
public record RemoveCartItemFailureAction(string Error);

// Clear cart
public record ClearCartAction;
public record ClearCartSuccessAction;
public record ClearCartFailureAction(string Error);

// Clear error
public record ClearCartErrorAction;
