namespace NexCommerce.Application.DTOs.Languages;

public record LanguageDto(
    Guid Id,
    string Code,
    string Name,
    bool IsDefault);
