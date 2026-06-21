using Client.Models.Auth;
using Client.State.Auth;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Auth;

public partial class LoginPage : FluxorComponent
{
    [Inject]
    private IState<AuthState> AuthState { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private readonly LoginModel _loginModel = new();
    private bool _showPassword;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new ClearAuthErrorAction());
        
        // If already authenticated, redirect immediately based on role
        if (AuthState.Value.IsAuthenticated && AuthState.Value.AuthResponse != null)
        {
            RedirectUser();
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (AuthState.Value.IsAuthenticated && AuthState.Value.AuthResponse != null)
        {
            RedirectUser();
        }
    }

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }

    private void HandleSubmit()
    {
        if (AuthState.Value.IsLoading) return;

        var request = new LoginRequest(_loginModel.Email, _loginModel.Password);
        Dispatcher.Dispatch(new LoginAction(request));
    }

    private void RedirectUser()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var returnUrl = query.Get("returnUrl");
        if (!string.IsNullOrEmpty(returnUrl))
        {
            NavigationManager.NavigateTo(returnUrl);
        }
        else
        {
            if (AuthState.Value.IsAdmin)
            {
                NavigationManager.NavigateTo("/admin");
            }
            else if (AuthState.Value.IsVendor)
            {
                NavigationManager.NavigateTo("/vendor");
            }
            else
            {
                NavigationManager.NavigateTo("/");
            }
        }
    }
}
