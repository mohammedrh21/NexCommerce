namespace NexCommerce.Application.DTOs.Vendors;

// ── Requests ─────────────────────────────────────────────────────────────────

public record CreateVendorProfileRequest(
    IEnumerable<VendorTranslationRequest> Translations);

public record UpdateVendorProfileRequest(
    IEnumerable<VendorTranslationRequest> Translations);

public record VendorTranslationRequest(
    string LanguageCode,
    string StoreName,
    string Description);

public record ApproveVendorRequest(bool IsApproved);

// ── Response DTOs ─────────────────────────────────────────────────────────────

public record VendorTranslationDto(
    string LanguageCode,
    string StoreName,
    string Description);

public record VendorProfileDto(
    Guid Id,
    Guid UserId,
    string OwnerName,
    string OwnerEmail,
    bool IsApproved,
    DateTime CreatedAt,
    int ProductCount,
    IEnumerable<VendorTranslationDto> Translations);

public record VendorListItemDto(
    Guid Id,
    Guid UserId,
    string StoreName,
    string OwnerName,
    bool IsApproved,
    int ProductCount,
    DateTime CreatedAt);

public record VendorStatsDto(
    int TotalProducts,
    int TotalOrders,
    decimal TotalRevenue,
    int PendingOrders,
    int LowStockProducts);
