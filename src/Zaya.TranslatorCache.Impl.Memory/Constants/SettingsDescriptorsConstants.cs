using Zaya.Primitives;

namespace Zaya.TranslatorCache.Impl.Memory.Constants;

internal static class SettingsDescriptorsConstants
{
    private static LocalizedString Loc(string key)
        => new(key, culture => Properties.Resources.ResourceManager.GetString(key, culture)!);

    public static IReadOnlyList<SettingDescriptor> Settings { get; } =
    [
        new BooleanSettingDescriptor(SettingsConstants.EnableCache, Loc(LocalizationConstants.EnableCache))
        {
            Description = Loc(LocalizationConstants.EnableCache_Desc),
            DefaultValue = true,
        },
        new IntegerSettingDescriptor(SettingsConstants.CacheTtlMinutes, Loc(LocalizationConstants.CacheTtlMinutes))
        {
            Description = Loc(LocalizationConstants.CacheTtlMinutes_Desc),
            DefaultValue = 0,
            MinValue = 0,
            MaxValue = 10080,
            IsVisible = s => s.GetValueOrDefault(SettingsConstants.EnableCache) as bool? ?? true,
        },
    ];
}
