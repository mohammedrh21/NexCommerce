using System;
using System.Linq;
using System.Threading.Tasks;
using Client.Infrastructure.Auth;
using Client.Models.Auth;
using Client.Services;
using Client.Services.Http;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace Client.State.Auth;

public class AuthEffects
{
    private readonly IAuthApiService _authApi;
    private readonly ITokenStorageService _tokenStorage;
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly NavigationManager _navigationManager;
    private readonly IToastService _toastService;

    public AuthEffects(
        IAuthApiService authApi,
        ITokenStorageService tokenStorage,
        CustomAuthStateProvider authStateProvider,
        NavigationManager navigationManager,
        IToastService toastService)
    {
        _authApi = authApi;
        _tokenStorage = tokenStorage;
        _authStateProvider = authStateProvider;
        _navigationManager = navigationManager;
        _toastService = toastService;
    }

    [EffectMethod]
    public async Task HandleLogin(LoginAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _authApi.LoginAsync(action.Request);
            
            await _tokenStorage.SetAccessTokenAsync(response.AccessToken);
            await _tokenStorage.SetRefreshTokenAsync(response.RefreshToken);
            _authStateProvider.NotifyUserAuthentication(response.AccessToken);

            dispatcher.Dispatch(new LoginSuccessAction(response));
            _toastService.Success($"Welcome back, {response.FullName}!");

            // Determine redirect URL
            var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var returnUrl = query.Get("returnUrl");
            if (!string.IsNullOrEmpty(returnUrl))
            {
                _navigationManager.NavigateTo(returnUrl);
            }
            else
            {
                // Role-based default navigation
                if (response.Roles.Contains("Admin"))
                {
                    _navigationManager.NavigateTo("/admin");
                }
                else if (response.Roles.Contains("Vendor"))
                {
                    _navigationManager.NavigateTo("/vendor");
                }
                else
                {
                    _navigationManager.NavigateTo("/");
                }
            }
        }
        catch (Exception ex)
        {
            string errorMessage = ex.Message;
            dispatcher.Dispatch(new LoginFailureAction(errorMessage));
            _toastService.Error(errorMessage, "Login Failed");
        }
    }

    [EffectMethod]
    public async Task HandleRegister(RegisterAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _authApi.RegisterAsync(action.Request);
            
            await _tokenStorage.SetAccessTokenAsync(response.AccessToken);
            await _tokenStorage.SetRefreshTokenAsync(response.RefreshToken);
            _authStateProvider.NotifyUserAuthentication(response.AccessToken);

            dispatcher.Dispatch(new RegisterSuccessAction(response));
            _toastService.Success("Account created successfully!");
            
            if (response.Roles.Contains("Admin"))
            {
                _navigationManager.NavigateTo("/admin");
            }
            else if (response.Roles.Contains("Vendor"))
            {
                _navigationManager.NavigateTo("/vendor");
            }
            else
            {
                _navigationManager.NavigateTo("/");
            }
        }
        catch (Exception ex)
        {
            string errorMessage = ex.Message;
            dispatcher.Dispatch(new RegisterFailureAction(errorMessage));
            _toastService.Error(errorMessage, "Registration Failed");
        }
    }

    [EffectMethod]
    public async Task HandleLogout(LogoutAction action, IDispatcher dispatcher)
    {
        try
        {
            await _tokenStorage.ClearTokensAsync();
            _authStateProvider.NotifyUserLogout();
            dispatcher.Dispatch(new ClearAuthErrorAction());
            _toastService.Info("You have been logged out.");
            _navigationManager.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            _toastService.Error($"Failed to logout: {ex.Message}");
        }
    }
}
