using HeThongThuyetMinhDuLich.Api.Models;

namespace HeThongThuyetMinhDuLich.Api.Services;

public static class LanguageCatalog
{
    private static readonly Dictionary<string, (string IsoCode, string DisplayName, int SortOrder)> SupportedLanguages =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["vi"] = ("vi", "Tiếng Việt", 0),
            ["en"] = ("en", "English", 1),
            ["zh-cn"] = ("zh-CN", "中文(简体)", 2)
        };

    public static string NormalizeIsoCode(string? isoCode)
    {
        if (string.IsNullOrWhiteSpace(isoCode))
        {
            return string.Empty;
        }

        var key = isoCode.Trim();
        return SupportedLanguages.TryGetValue(key, out var language)
            ? language.IsoCode
            : key;
    }

    public static string NormalizeDisplayName(string? isoCode, string? displayName)
    {
        var normalizedIsoCode = NormalizeIsoCode(isoCode);
        if (SupportedLanguages.TryGetValue(normalizedIsoCode, out var language))
        {
            return language.DisplayName;
        }

        return string.IsNullOrWhiteSpace(displayName) ? normalizedIsoCode : displayName.Trim();
    }

    public static int GetSortOrder(string? isoCode)
    {
        var normalizedIsoCode = NormalizeIsoCode(isoCode);
        return SupportedLanguages.TryGetValue(normalizedIsoCode, out var language)
            ? language.SortOrder
            : int.MaxValue;
    }

    public static string GetDisplayLabel(NgonNgu? language)
    {
        if (language is null)
        {
            return string.Empty;
        }

        var normalizedIsoCode = NormalizeIsoCode(language.MaNgonNguQuocTe);
        var normalizedDisplayName = NormalizeDisplayName(normalizedIsoCode, language.TenNgonNgu);
        return string.IsNullOrWhiteSpace(normalizedIsoCode)
            ? normalizedDisplayName
            : $"{normalizedDisplayName} ({normalizedIsoCode})";
    }
}