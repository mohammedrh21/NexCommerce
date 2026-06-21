using Client.Models.Auth;
using Fluxor;

namespace Client.State.Auth;

[FeatureState]
public record AuthState
{
    public bool IsLoading { get; init; }
    public bool IsAuthenticated { get; init; }
    public string? Error { get; init; }
    public AuthResponseDto? AuthResponse { get; init; }

    public string UserName => AuthResponse?.FullName ?? string.Empty;
    public string UserEmail => AuthResponse?.Email ?? string.Empty;
    public IEnumerable<string> Roles => AuthResponse?.Roles ?? [];
    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsVendor => Roles.Contains("Vendor");
    public bool IsCustomer => Roles.Contains("Customer");

    public AuthState()
    {
        IsLoading = false;
        IsAuthenticated = false;
        Error = null;
        AuthResponse = null;
    }
}
