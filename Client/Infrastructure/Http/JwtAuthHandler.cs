using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Client.Infrastructure.Auth;
using Client.Models.Common;
using Client.Models.Auth;

namespace Client.Infrastructure.Http;

public class JwtAuthHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly IServiceProvider _serviceProvider;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public JwtAuthHandler(ITokenStorageService tokenStorage, IServiceProvider serviceProvider)
    {
        _tokenStorage = tokenStorage;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? "";
        
        // Skip token injection for login, register, and refresh token endpoints
        var isAuthEndpoint = url.Contains("auth/login", StringComparison.OrdinalIgnoreCase) ||
                             url.Contains("auth/register", StringComparison.OrdinalIgnoreCase) ||
                             url.Contains("auth/refresh-token", StringComparison.OrdinalIgnoreCase);

        if (!isAuthEndpoint)
        {
            var token = await _tokenStorage.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !isAuthEndpoint)
        {
            var refreshed = await TryRefreshTokenAsync(cancellationToken);
            if (refreshed)
            {
                // Retry request with new token
                var newToken = await _tokenStorage.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    
                    // Resend the request
                    response = await base.SendAsync(request, cancellationToken);
                }
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var accessToken = await _tokenStorage.GetAccessTokenAsync();
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                await LogoutUserAsync();
                return false;
            }

            // Call refresh-token endpoint directly via inner handler to bypass this handler
            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/refresh-token");
            refreshRequest.Content = JsonContent.Create(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });

            var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);
            if (refreshResponse.IsSuccessStatusCode)
            {
                var apiResponse = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>(_jsonOptions, cancellationToken);
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    await _tokenStorage.SetAccessTokenAsync(apiResponse.Data.AccessToken);
                    await _tokenStorage.SetRefreshTokenAsync(apiResponse.Data.RefreshToken);

                    var authStateProvider = _serviceProvider.GetService(typeof(AuthenticationStateProvider)) as CustomAuthStateProvider;
                    authStateProvider?.NotifyUserAuthentication(apiResponse.Data.AccessToken);
                    return true;
                }
            }

            // If refresh fails, log out the user
            await LogoutUserAsync();
            return false;
        }
        catch
        {
            await LogoutUserAsync();
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task LogoutUserAsync()
    {
        await _tokenStorage.ClearTokensAsync();
        var authStateProvider = _serviceProvider.GetService(typeof(AuthenticationStateProvider)) as CustomAuthStateProvider;
        authStateProvider?.NotifyUserLogout();
    }
}

