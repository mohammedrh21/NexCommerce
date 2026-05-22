using AutoMapper;
using NexCommerce.Application.DTOs.Products;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(d => d.IsLowStock, o => o.MapFrom(s => s.StockQuantity <= s.LowStockThreshold));

        CreateMap<ProductImage, ProductImageDto>();

        // ProductTranslation → dto (lang resolved in service)
        CreateMap<ProductTranslation, ProductTranslationDto>()
            .ForMember(d => d.LanguageCode, o => o.MapFrom(s => s.Language.Code));

        // Full detail
        CreateMap<Product, ProductDetailDto>()
            .ForMember(d => d.VendorName,    o => o.MapFrom(s => GetVendorName(s)))
            .ForMember(d => d.CategoryName,  o => o.MapFrom(s => GetCategoryName(s)))
            .ForMember(d => d.ReviewCount,   o => o.MapFrom(s => s.Reviews.Count))
            .ForMember(d => d.Translations,  o => o.MapFrom(s => s.Translations))
            .ForMember(d => d.Variants,      o => o.MapFrom(s => s.Variants))
            .ForMember(d => d.Images,        o => o.MapFrom(s => s.Images));
    }

    private static string GetVendorName(Product p) =>
        p.Vendor?.Translations?.FirstOrDefault()?.StoreName ?? string.Empty;

    private static string GetCategoryName(Product p) =>
        p.Category?.Translations?.FirstOrDefault()?.Name ?? string.Empty;
}
