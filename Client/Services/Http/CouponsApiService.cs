using Client.Models.Common;
using Client.Models.Coupons;

namespace Client.Services.Http;

public interface ICouponsApiService
{
    Task<PagedResult<CouponDto>> GetAllAsync(int page = 1, int pageSize = 20, string lang = "en", CancellationToken ct = default);
    Task<CouponDto> GetByIdAsync(Guid id, string lang = "en", CancellationToken ct = default);
    Task<CouponDto> CreateAsync(CreateCouponRequest request, CancellationToken ct = default);
    Task<CouponDto> UpdateAsync(Guid id, UpdateCouponRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<CouponValidationResultDto> ValidateAsync(ValidateCouponRequest request, CancellationToken ct = default);
}

public class CouponsApiService : BaseApiService, ICouponsApiService
{
    public CouponsApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<PagedResult<CouponDto>> GetAllAsync(int page = 1, int pageSize = 20, string lang = "en", CancellationToken ct = default)
        => GetAsync<PagedResult<CouponDto>>($"api/v1/coupons?page={page}&pageSize={pageSize}&lang={lang}", ct);

    public Task<CouponDto> GetByIdAsync(Guid id, string lang = "en", CancellationToken ct = default)
        => GetAsync<CouponDto>($"api/v1/coupons/{id}?lang={lang}", ct);

    public Task<CouponDto> CreateAsync(CreateCouponRequest request, CancellationToken ct = default)
        => PostAsync<CreateCouponRequest, CouponDto>("api/v1/coupons", request, ct);

    public Task<CouponDto> UpdateAsync(Guid id, UpdateCouponRequest request, CancellationToken ct = default)
        => PutAsync<UpdateCouponRequest, CouponDto>($"api/v1/coupons/{id}", request, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => DeleteAsync($"api/v1/coupons/{id}", ct);

    public Task<CouponValidationResultDto> ValidateAsync(ValidateCouponRequest request, CancellationToken ct = default)
        => PostAsync<ValidateCouponRequest, CouponValidationResultDto>("api/v1/coupons/validate", request, ct);
}
