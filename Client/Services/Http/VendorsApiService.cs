using Client.Models.Common;
using Client.Models.Vendors;

namespace Client.Services.Http;

public interface IVendorsApiService
{
    Task<PagedResult<VendorListItemDto>> GetAllAsync(int page = 1, int pageSize = 10, string lang = "en", CancellationToken ct = default);
    Task<VendorProfileDto> GetByIdAsync(Guid id, string lang = "en", CancellationToken ct = default);
    Task<VendorProfileDto> GetMyProfileAsync(string lang = "en", CancellationToken ct = default);
    Task<VendorStatsDto> GetStatsAsync(Guid id, CancellationToken ct = default);
    Task<VendorProfileDto> CreateProfileAsync(CreateVendorProfileRequest request, CancellationToken ct = default);
    Task<VendorProfileDto> UpdateProfileAsync(Guid id, UpdateVendorProfileRequest request, CancellationToken ct = default);
    Task<VendorProfileDto> ApproveVendorAsync(Guid id, bool isApproved, CancellationToken ct = default);
}

public class VendorsApiService : BaseApiService, IVendorsApiService
{
    public VendorsApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<PagedResult<VendorListItemDto>> GetAllAsync(int page = 1, int pageSize = 10, string lang = "en", CancellationToken ct = default)
        => GetAsync<PagedResult<VendorListItemDto>>($"api/v1/vendors?page={page}&pageSize={pageSize}&lang={lang}", ct);

    public Task<VendorProfileDto> GetByIdAsync(Guid id, string lang = "en", CancellationToken ct = default)
        => GetAsync<VendorProfileDto>($"api/v1/vendors/{id}?lang={lang}", ct);

    public Task<VendorProfileDto> GetMyProfileAsync(string lang = "en", CancellationToken ct = default)
        => GetAsync<VendorProfileDto>($"api/v1/vendors/me?lang={lang}", ct);

    public Task<VendorStatsDto> GetStatsAsync(Guid id, CancellationToken ct = default)
        => GetAsync<VendorStatsDto>($"api/v1/vendors/{id}/stats", ct);

    public Task<VendorProfileDto> CreateProfileAsync(CreateVendorProfileRequest request, CancellationToken ct = default)
        => PostAsync<CreateVendorProfileRequest, VendorProfileDto>("api/v1/vendors", request, ct);

    public Task<VendorProfileDto> UpdateProfileAsync(Guid id, UpdateVendorProfileRequest request, CancellationToken ct = default)
        => PutAsync<UpdateVendorProfileRequest, VendorProfileDto>($"api/v1/vendors/{id}", request, ct);

    public Task<VendorProfileDto> ApproveVendorAsync(Guid id, bool isApproved, CancellationToken ct = default)
        => PatchAsync<bool, VendorProfileDto>($"api/v1/vendors/{id}/approve", isApproved, ct);
}
