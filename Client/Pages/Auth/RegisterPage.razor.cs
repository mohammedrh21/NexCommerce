using Client.Models.Auth;
using Client.State.Auth;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Auth;

public partial class RegisterPage : FluxorComponent
{
    [Inject]
    private IState<AuthState> AuthState { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private readonly RegisterModel _registerModel = new();
    private bool _showPassword;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new ClearAuthErrorAction());

        // Redirect if already authenticated
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

    private void SelectRole(string role)
    {
        if (AuthState.Value.IsLoading) return;
        _registerModel.Role = role;
    }

    private void HandleSubmit()
    {
        if (AuthState.Value.IsLoading) return;

        var request = new RegisterRequest(
            _registerModel.FullName,
            _registerModel.Email,
            _registerModel.Password,
            _registerModel.Role
        );
        Dispatcher.Dispatch(new RegisterAction(request));
    }

    private void RedirectUser()
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
