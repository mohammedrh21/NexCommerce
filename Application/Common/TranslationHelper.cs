using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Common;

/// <summary>
/// Centralised translation resolution helpers — used by all Application services.
/// Resolves the best available translation for the requested language code,
/// falling back to English, then to the first available translation.
/// </summary>
internal static class TranslationHelper
{
    /// <summary>Resolves a <see cref="ProductTranslation"/> field for the given language.</summary>
    public static string Translate(
        IEnumerable<ProductTranslation> translations,
        string lang,
        Func<ProductTranslation, string> selector)
    {
        var t = translations.FirstOrDefault(x => x.Language.Code == lang)
             ?? translations.FirstOrDefault(x => x.Language.Code == "en")
             ?? translations.FirstOrDefault();
        return t is not null ? selector(t) : string.Empty;
    }

    /// <summary>Resolves a <see cref="CategoryTranslation"/> name for the given language.</summary>
    public static string TranslateCategory(
        IEnumerable<CategoryTranslation> translations,
        string lang)
    {
        var t = translations.FirstOrDefault(x => x.Language.Code == lang)
             ?? translations.FirstOrDefault(x => x.Language.Code == "en")
             ?? translations.FirstOrDefault();
        return t?.Name ?? string.Empty;
    }

    /// <summary>Resolves a <see cref="VendorProfileTranslation"/> store name for the given language.</summary>
    public static string TranslateVendor(
        IEnumerable<VendorProfileTranslation> translations,
        string lang)
    {
        var t = translations.FirstOrDefault(x => x.Language.Code == lang)
             ?? translations.FirstOrDefault(x => x.Language.Code == "en")
             ?? translations.FirstOrDefault();
        return t?.StoreName ?? string.Empty;
    }
}
