using Fluxor;

namespace Client.State.Auth;

public static class AuthReducers
{
    [ReducerMethod]
    public static AuthState ReduceLoginAction(AuthState state, LoginAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static AuthState ReduceLoginSuccessAction(AuthState state, LoginSuccessAction action) =>
        state with
        {
            IsLoading = false,
            IsAuthenticated = true,
            AuthResponse = action.Response,
            Error = null
        };

    [ReducerMethod]
    public static AuthState ReduceLoginFailureAction(AuthState state, LoginFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static AuthState ReduceRegisterAction(AuthState state, RegisterAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static AuthState ReduceRegisterSuccessAction(AuthState state, RegisterSuccessAction action) =>
        state with
        {
            IsLoading = false,
            IsAuthenticated = true,
            AuthResponse = action.Response,
            Error = null
        };

    [ReducerMethod]
    public static AuthState ReduceRegisterFailureAction(AuthState state, RegisterFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static AuthState ReduceLogoutAction(AuthState state, LogoutAction action) =>
        new AuthState();

    [ReducerMethod]
    public static AuthState ReduceSetAuthenticatedAction(AuthState state, SetAuthenticatedAction action) =>
        state with
        {
            IsAuthenticated = true,
            AuthResponse = action.Response,
            Error = null
        };

    [ReducerMethod]
    public static AuthState ReduceClearAuthErrorAction(AuthState state, ClearAuthErrorAction action) =>
        state with { Error = null };

    [ReducerMethod]
    public static AuthState ReduceUpdateUserInfoAction(AuthState state, UpdateUserInfoAction action) =>
        state.AuthResponse is null ? state : state with
        {
            AuthResponse = state.AuthResponse with { FullName = action.FullName }
        };
}
