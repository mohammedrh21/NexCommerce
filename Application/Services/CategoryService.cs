using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Categories;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Services;

public sealed class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<IEnumerable<CategoryListItemDto>> GetAllAsync(string lang, CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetAllAsync(ct);
        return categories.Select(c => MapListItem(c, lang));
    }

    public async Task<IEnumerable<CategoryListItemDto>> GetRootCategoriesAsync(string lang, CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetRootCategoriesAsync(ct);
        return categories.Select(c => MapListItem(c, lang));
    }

    public async Task<IEnumerable<CategoryListItemDto>> GetSubCategoriesAsync(Guid parentId, string lang, CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetSubCategoriesAsync(parentId, ct);
        return categories.Select(c => MapListItem(c, lang));
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, string lang, CancellationToken ct = default)
    {
        var category = await categoryRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException($"Category {id} not found.");

        return MapDto(category, lang);
    }

    private static CategoryListItemDto MapListItem(Category c, string lang) => new(
        Id:               c.Id,
        Name:             TranslationHelper.TranslateCategory(c.Translations, lang),
        ParentId:         c.ParentId,
        ParentName:       c.Parent is not null ? TranslationHelper.TranslateCategory(c.Parent.Translations, lang) : null,
        SubCategoryCount: c.SubCategories.Count,
        ProductCount:     c.Products.Count);

    private static CategoryDto MapDto(Category c, string lang) => new(
        Id:           c.Id,
        ParentId:     c.ParentId,
        ParentName:   c.Parent is not null ? TranslationHelper.TranslateCategory(c.Parent.Translations, lang) : null,
        Translations: c.Translations.Select(t => new CategoryTranslationDto(t.Language.Code, t.Name)),
        ProductCount: c.Products.Count);
}
