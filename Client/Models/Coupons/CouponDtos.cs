using Client.Models.Common;

namespace Client.Models.Coupons;

public record CreateCouponRequest(
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    DateTime ExpiryDate,
    int UsageLimit,
    IEnumerable<CouponTranslationRequest> Translations);

public record UpdateCouponRequest(
    DiscountType? DiscountType,
    decimal? DiscountValue,
    DateTime? ExpiryDate,
    int? UsageLimit,
    bool? IsActive);

public record CouponTranslationRequest(
    string LanguageCode,
    string Name,
    string Description);

public record ValidateCouponRequest(
    string Code,
    decimal OrderAmount);

public record CouponTranslationDto(
    string LanguageCode,
    string Name,
    string Description);

public record CouponDto(
    Guid Id,
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    DateTime ExpiryDate,
    int UsageLimit,
    int UsedCount,
    bool IsActive,
    bool IsExpired,
    IEnumerable<CouponTranslationDto> Translations);

public record CouponValidationResultDto(
    bool IsValid,
    string? ErrorMessage,
    Guid? CouponId,
    string? Code,
    DiscountType? DiscountType,
    decimal? DiscountValue,
    decimal? DiscountAmount,
    decimal? FinalAmount);
