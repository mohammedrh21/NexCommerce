using Client.Models.Users;

namespace Client.Services.Http;

public interface IUsersApiService
{
    Task<IEnumerable<UserInfoDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<UserInfoDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class UsersApiService : BaseApiService, IUsersApiService
{
    public UsersApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<IEnumerable<UserInfoDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
        => GetAsync<IEnumerable<UserInfoDto>>($"api/v1/users?page={page}&pageSize={pageSize}", ct);

    public Task<UserInfoDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => GetAsync<UserInfoDto>($"api/v1/users/{id}", ct);

    public Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
        => PutAsync<UpdateUserRequest, object>($"api/v1/users/{id}", request, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => DeleteAsync($"api/v1/users/{id}", ct);
}
