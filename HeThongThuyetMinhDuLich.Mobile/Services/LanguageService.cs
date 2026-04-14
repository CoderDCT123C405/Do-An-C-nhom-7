using HeThongThuyetMinhDuLich.Mobile.Resources.Localization;
using System.Globalization;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public sealed class LanguageService
{
    private const string PreferenceKey = "app.language";

    public event EventHandler? LanguageChanged;

    public IReadOnlyList<AppLanguageOption> SupportedLanguages { get; }

    public string CurrentLanguageCode { get; private set; }

    public AppLanguageOption CurrentLanguage => SupportedLanguages.First(x => x.Code == CurrentLanguageCode);

    public LanguageService()
    {
        SupportedLanguages =
        [
            new AppLanguageOption("vi", "Tieng Viet", "VI"),
            new AppLanguageOption("en", "English", "EN"),
            new AppLanguageOption("zh-CN", "中文(简体)", "中文"),
            new AppLanguageOption("fr-FR", "Français", "FR")
        ];

        var savedCode = Preferences.Default.Get(PreferenceKey, "vi");
        CurrentLanguageCode = NormalizeCode(savedCode);
        ApplyCulture(CurrentLanguageCode);
    }

    public bool SetLanguage(string? languageCode)
    {
        var normalizedCode = NormalizeCode(languageCode);
        if (string.Equals(CurrentLanguageCode, normalizedCode, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        CurrentLanguageCode = normalizedCode;
        Preferences.Default.Set(PreferenceKey, CurrentLanguageCode);
        ApplyCulture(CurrentLanguageCode);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public string GetText(string key)
    {
        var currentCulture = CultureInfo.GetCultureInfo(CurrentLanguageCode);
        var localizedText = AppStrings.GetString(key, currentCulture);
        if (!string.Equals(localizedText, key, StringComparison.Ordinal))
        {
            return localizedText;
        }

        return AppStrings.GetString(key, CultureInfo.GetCultureInfo("vi"));
    }

    public string Format(string key, params object[] args)
    {
        return string.Format(GetText(key), args);
    }

    private string NormalizeCode(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return "vi";
        }

        var matched = SupportedLanguages.FirstOrDefault(x => string.Equals(x.Code, languageCode, StringComparison.OrdinalIgnoreCase));
        return matched?.Code ?? "vi";
    }

    private static void ApplyCulture(string languageCode)
    {
        var culture = CultureInfo.GetCultureInfo(languageCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        AppStrings.Culture = culture;
    }
}

public sealed record AppLanguageOption(string Code, string NativeName, string ShortLabel);