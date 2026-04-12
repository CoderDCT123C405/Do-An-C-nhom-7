using System.Globalization;
using System.Reflection;
using System.Resources;

namespace HeThongThuyetMinhDuLich.Mobile.Resources.Localization;

public static class AppStrings
{
    private static readonly ResourceManager ResourceManager = new(
        "HeThongThuyetMinhDuLich.Mobile.Resources.Localization.AppStrings",
        typeof(AppStrings).GetTypeInfo().Assembly);

    public static CultureInfo? Culture { get; set; }

    public static string GetString(string key, CultureInfo? culture = null)
    {
        return ResourceManager.GetString(key, culture ?? Culture ?? CultureInfo.CurrentUICulture) ?? key;
    }
}