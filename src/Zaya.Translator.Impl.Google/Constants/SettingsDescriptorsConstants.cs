using Zaya.Primitives;

namespace Zaya.Translator.Impl.Google.Constants;

internal static class SettingsDescriptorsConstants
{
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
            // Missing key must follow DefaultValue (true): hide source until user turns auto-detect off.
            IsVisible = s => s.GetValueOrDefault(SettingsConstants.AutoDetectLanguage) is false,
            IsRequired = s => s.GetValueOrDefault(SettingsConstants.AutoDetectLanguage) is false,
            Options = Languages.All,
        },
        new EnumSettingDescriptor(SettingsConstants.TargetLanguage, Loc(LocalizationConstants.TargetLanguage))
        {
            Description = Loc(LocalizationConstants.TargetLanguage_Desc),
            IsRequired = static _ => true,
            DefaultValue = LanguageCodeConstants.English,
            Options = Languages.All,
        },
        new StringSettingDescriptor(SettingsConstants.UserAgent, Loc(LocalizationConstants.UserAgent))
        {
            Description = Loc(LocalizationConstants.UserAgent_Desc),
            DefaultValue = SettingsConstants.DefaultUserAgent,
            IsVisible = static _ => false,
            IsRequired = static _ => false,
        },
    ];
}
