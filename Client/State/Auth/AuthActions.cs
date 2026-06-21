using Client.Models.Auth;

namespace Client.State.Auth;

// Login
public record LoginAction(LoginRequest Request);
public record LoginSuccessAction(AuthResponseDto Response);
public record LoginFailureAction(string Error);

// Register
public record RegisterAction(RegisterRequest Request);
public record RegisterSuccessAction(AuthResponseDto Response);
public record RegisterFailureAction(string Error);

// Logout
public record LogoutAction;

// Set authenticated state on app load (from stored tokens)
public record SetAuthenticatedAction(AuthResponseDto Response);

// Clear error
public record ClearAuthErrorAction;

// Update profile info
public record UpdateUserInfoAction(string FullName);
