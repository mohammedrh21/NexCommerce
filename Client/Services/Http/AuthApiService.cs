using Client.Models.Auth;

namespace Client.Services.Http;

public interface IAuthApiService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
}

public class AuthApiService : BaseApiService, IAuthApiService
{
    public AuthApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        return await PostAsync<RegisterRequest, AuthResponseDto>("api/v1/auth/register", request, ct);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        return await PostAsync<LoginRequest, AuthResponseDto>("api/v1/auth/login", request, ct);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        return await PostAsync<RefreshTokenRequest, AuthResponseDto>("api/v1/auth/refresh-token", request, ct);
    }
}
