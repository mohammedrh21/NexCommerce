using AutoMapper;
using NexCommerce.Application.DTOs.Categories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryTranslation, CategoryTranslationDto>()
            .ForMember(d => d.LanguageCode, o => o.MapFrom(s => s.Language.Code));

        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ParentName,    o => o.MapFrom(s => s.Parent != null
                ? s.Parent.Translations.FirstOrDefault()!.Name : null))
            .ForMember(d => d.ProductCount,  o => o.MapFrom(s => s.Products.Count))
            .ForMember(d => d.Translations,  o => o.MapFrom(s => s.Translations));

        CreateMap<Category, CategoryListItemDto>()
            .ForMember(d => d.Name,             o => o.MapFrom(s => s.Translations.FirstOrDefault()!.Name))
            .ForMember(d => d.ParentName,        o => o.MapFrom(s => s.Parent != null
                ? s.Parent.Translations.FirstOrDefault()!.Name : null))
            .ForMember(d => d.SubCategoryCount,  o => o.MapFrom(s => s.SubCategories.Count))
            .ForMember(d => d.ProductCount,      o => o.MapFrom(s => s.Products.Count));
    }
}
