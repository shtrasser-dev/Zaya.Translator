using Zaya.Primitives;

namespace Zaya.Translator.Impl.Yandex.Constants;

internal static class SettingsDescriptorsConstants
{
    private static readonly IReadOnlyList<EnumOption> LanguageOptions = CreateLanguageOptions();

    private static LocalizedString Loc(string key)
        => new(key, culture => Properties.Resources.ResourceManager.GetString(key, culture)!);

    public static IReadOnlyList<SettingDescriptor> Settings { get; } =
    [
        new BooleanSettingDescriptor(SettingsConstants.AutoDetectLanguage, Loc(LocalizationConstants.AutoDetectLanguage))
        {
            Description = Loc(LocalizationConstants.AutoDetectLanguage_Desc),
            DefaultValue = true,
        },
        new EnumSettingDescriptor(SettingsConstants.SourceLanguage, Loc(LocalizationConstants.SourceLanguage))
        {
            Description = Loc(LocalizationConstants.SourceLanguage_Desc),
            DefaultValue = LanguageCodeConstants.English,
            IsVisible = s => s.GetValueOrDefault(SettingsConstants.AutoDetectLanguage) is false,
            IsRequired = s => s.GetValueOrDefault(SettingsConstants.AutoDetectLanguage) is false,
            Options = LanguageOptions,
        },
        new EnumSettingDescriptor(SettingsConstants.TargetLanguage, Loc(LocalizationConstants.TargetLanguage))
        {
            Description = Loc(LocalizationConstants.TargetLanguage_Desc),
            IsRequired = static _ => true,
            DefaultValue = LanguageCodeConstants.English,
            Options = LanguageOptions,
        },
        new BooleanSettingDescriptor(SettingsConstants.UseApiKey, Loc(LocalizationConstants.UseApiKey))
        {
            Description = Loc(LocalizationConstants.UseApiKey_Desc),
            DefaultValue = false,
        },
        new PasswordSettingDescriptor(SettingsConstants.ApiKey, Loc(LocalizationConstants.ApiKey))
        {
            Description = Loc(LocalizationConstants.ApiKey_Desc),
            IsVisible = s => s.GetValueOrDefault(SettingsConstants.UseApiKey) is true,
            IsRequired = s => s.GetValueOrDefault(SettingsConstants.UseApiKey) is true,
        },
        new StringSettingDescriptor(SettingsConstants.UserAgent, Loc(LocalizationConstants.UserAgent))
        {
            Description = Loc(LocalizationConstants.UserAgent_Desc),
            DefaultValue = SettingsConstants.DefaultUserAgent,
            IsVisible = static _ => false,
            IsRequired = static _ => false,
        },
    ];

    /// <summary>
    /// Yandex APIs expose a single Chinese code (<c>zh</c>), so Hans/Hant are collapsed in the UI.
    /// </summary>
    private static IReadOnlyList<EnumOption> CreateLanguageOptions()
    {
        var options = new List<EnumOption>(Languages.All.Count);
        var chineseAdded = false;
        foreach (var option in Languages.All)
        {
            if (option.Value is LanguageCodeConstants.ChineseSimplified or LanguageCodeConstants.ChineseTraditional)
            {
                if (chineseAdded)
                    continue;
                options.Add(new EnumOption(LanguageCodeConstants.Chinese, option.DisplayName, option.Description));
                chineseAdded = true;
                continue;
            }

            options.Add(option);
        }

        return options;
    }
}
