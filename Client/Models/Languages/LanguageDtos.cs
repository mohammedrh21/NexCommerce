namespace Client.Models.Languages;

public record LanguageDto(
    Guid Id,
    string Code,
    string Name,
    bool IsDefault);
