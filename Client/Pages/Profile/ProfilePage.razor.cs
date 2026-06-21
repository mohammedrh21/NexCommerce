using Client.Models.Users;
using Client.Services;
using Client.Services.Http;
using Client.State.Auth;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Client.Pages.Profile;

public partial class ProfilePage : FluxorComponent
{
    [Inject] private IState<AuthState> AuthState { get; set; } = default!;
    [Inject] private IUsersApiService UsersApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private IDispatcher Dispatcher { get; set; } = default!;

    private UserInfoDto? _user;
    private string? _fullName;
    private bool _isLoadingData = true;
    private bool _isSaving = false;
    private bool _showValidation = false;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadUserProfileAsync();
    }

    private async Task LoadUserProfileAsync()
    {
        try
        {
            _isLoadingData = true;
            _errorMessage = null;

            var userId = AuthState.Value.AuthResponse?.UserId;
            if (userId == null || userId == Guid.Empty)
            {
                throw new Exception("User is not authenticated correctly.");
            }

            _user = await UsersApi.GetByIdAsync(userId.Value);
            _fullName = _user?.FullName;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load profile details: {ex.Message}";
            ToastService.Error("Could not retrieve profile information.");
        }
        finally
        {
            _isLoadingData = false;
        }
    }

    private async Task UpdateProfileAsync()
    {
        _showValidation = true;

        if (string.IsNullOrWhiteSpace(_fullName) || _user == null)
        {
            return;
        }

        try
        {
            _isSaving = true;
            _errorMessage = null;

            var request = new UpdateUserRequest(_fullName.Trim());
            await UsersApi.UpdateAsync(_user.Id, request);

            // Sync user details in state
            Dispatcher.Dispatch(new UpdateUserInfoAction(request.FullName));

            // Update local model
            _user = _user with { FullName = request.FullName };

            ToastService.Success("Profile updated successfully!");
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to update profile: {ex.Message}";
            ToastService.Error("Update failed. Please try again.");
        }
        finally
        {
            _isSaving = false;
        }
    }
}
