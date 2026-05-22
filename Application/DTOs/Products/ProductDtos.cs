namespace NexCommerce.Application.DTOs.Products;

// ── Requests ─────────────────────────────────────────────────────────────────

public record CreateProductRequest(
    decimal Price,
    Guid CategoryId,
    IEnumerable<CreateProductTranslationRequest> Translations,
    IEnumerable<CreateProductVariantRequest> Variants);

public record CreateProductTranslationRequest(
    string LanguageCode,
    string Name,
    string Description);

public record CreateProductVariantRequest(
    string? Size,
    string? Color,
    string? SKU,
    int StockQuantity,
    int LowStockThreshold = 5,
    decimal? PriceAdjustment = null);

public record UpdateProductRequest(
    decimal? Price,
    Guid? CategoryId,
    IEnumerable<CreateProductTranslationRequest>? Translations,
    IEnumerable<UpdateProductVariantRequest>? Variants);

public record UpdateProductVariantRequest(
    Guid? Id,
    string? Size,
    string? Color,
    string? SKU,
    int? StockQuantity,
    int? LowStockThreshold,
    decimal? PriceAdjustment);

// ── Response DTOs ─────────────────────────────────────────────────────────────

public record ProductImageDto(
    Guid Id,
    string Url,
    string PublicId,
    bool IsPrimary);

public record ProductTranslationDto(
    string LanguageCode,
    string Name,
    string Description);

public record ProductVariantDto(
    Guid Id,
    string? Size,
    string? Color,
    string? SKU,
    int StockQuantity,
    int LowStockThreshold,
    decimal? PriceAdjustment,
    bool IsLowStock);

public record ProductListItemDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal Rating,
    int ReviewCount,
    string? PrimaryImageUrl,
    Guid CategoryId,
    string CategoryName,
    Guid VendorId,
    string VendorName,
    DateTime CreatedAt);

public record ProductDetailDto(
    Guid Id,
    decimal Price,
    decimal Rating,
    int ReviewCount,
    DateTime CreatedAt,
    Guid VendorId,
    string VendorName,
    Guid CategoryId,
    string CategoryName,
    IEnumerable<ProductTranslationDto> Translations,
    IEnumerable<ProductVariantDto> Variants,
    IEnumerable<ProductImageDto> Images);
