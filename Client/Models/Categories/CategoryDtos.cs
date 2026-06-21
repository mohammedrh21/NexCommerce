namespace Client.Models.Categories;

public record CreateCategoryRequest(
    Guid? ParentId,
    IEnumerable<CategoryTranslationRequest> Translations);

public record UpdateCategoryRequest(
    Guid? ParentId,
    IEnumerable<CategoryTranslationRequest> Translations);

public record CategoryTranslationRequest(
    string LanguageCode,
    string Name);

public record CategoryTranslationDto(
    string LanguageCode,
    string Name);

public record CategoryDto(
    Guid Id,
    Guid? ParentId,
    string? ParentName,
    IEnumerable<CategoryTranslationDto> Translations,
    int ProductCount);

public record CategoryListItemDto(
    Guid Id,
    string Name,
    Guid? ParentId,
    string? ParentName,
    int SubCategoryCount,
    int ProductCount);
