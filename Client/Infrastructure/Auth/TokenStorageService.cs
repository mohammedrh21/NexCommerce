using Blazored.LocalStorage;

namespace Client.Infrastructure.Auth;

public interface ITokenStorageService
{
    Task<string?> GetAccessTokenAsync();
    Task SetAccessTokenAsync(string token);
    Task<string?> GetRefreshTokenAsync();
    Task SetRefreshTokenAsync(string token);
    Task ClearTokensAsync();
}

public class TokenStorageService : ITokenStorageService
{
    private readonly ILocalStorageService _localStorage;
    private const string AccessTokenKey = "authToken";
    private const string RefreshTokenKey = "refreshToken";

    public TokenStorageService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(AccessTokenKey);
    }

    public async Task SetAccessTokenAsync(string token)
    {
        await _localStorage.SetItemAsync(AccessTokenKey, token);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(RefreshTokenKey);
    }

    public async Task SetRefreshTokenAsync(string token)
    {
        await _localStorage.SetItemAsync(RefreshTokenKey, token);
    }

    public async Task ClearTokensAsync()
    {
        await _localStorage.RemoveItemAsync(AccessTokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
    }
}
