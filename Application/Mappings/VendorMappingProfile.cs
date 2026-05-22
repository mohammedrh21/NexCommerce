using AutoMapper;
using NexCommerce.Application.DTOs.Vendors;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Mappings;

public class VendorMappingProfile : Profile
{
    public VendorMappingProfile()
    {
        CreateMap<VendorProfileTranslation, VendorTranslationDto>()
            .ForMember(d => d.LanguageCode, o => o.MapFrom(s => s.Language.Code));

        CreateMap<VendorProfile, VendorProfileDto>()
            .ForMember(d => d.OwnerName,     o => o.Ignore())   // resolved in service
            .ForMember(d => d.OwnerEmail,    o => o.Ignore())
            .ForMember(d => d.ProductCount,  o => o.MapFrom(s => s.Products.Count))
            .ForMember(d => d.Translations,  o => o.MapFrom(s => s.Translations));

        CreateMap<VendorProfile, VendorListItemDto>()
            .ForMember(d => d.StoreName,    o => o.MapFrom(s => s.Translations.FirstOrDefault()!.StoreName))
            .ForMember(d => d.OwnerName,    o => o.Ignore())
            .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products.Count));
    }
}
