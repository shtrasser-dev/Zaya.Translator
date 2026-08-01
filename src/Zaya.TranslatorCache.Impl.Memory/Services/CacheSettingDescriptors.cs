using Zaya.Primitives;
using Zaya.TranslatorCache.Impl.Memory.Constants;

namespace Zaya.TranslatorCache.Impl.Memory.Services;

/// <summary>
/// Provides the <see cref="SettingDescriptor"/> list for memory cache configuration.
/// </summary>
public static class CacheSettingDescriptors
{
    private static LocalizedString Loc(string key)
        => new(key, culture => Properties.Resources.ResourceManager.GetString(key, culture)!);

    /// <summary>
    /// Gets the descriptor list containing cache-related settings.
    /// </summary>
    public static readonly IReadOnlyList<SettingDescriptor> All = [
        new BooleanSettingDescriptor(CacheSettingsKeys.EnableCache, Loc("Cache_EnableCache"))
        {
            Description = Loc("Cache_EnableCache_Desc"),
            DefaultValue = true,
        },
        new IntegerSettingDescriptor(CacheSettingsKeys.CacheTtlMinutes, Loc("Cache_CacheTtlMinutes"))
        {
            Description = Loc("Cache_CacheTtlMinutes_Desc"),
            DefaultValue = 0,
            MinValue = 0,
            MaxValue = 10080,
            IsVisible = s => s.GetValueOrDefault(CacheSettingsKeys.EnableCache) as bool? ?? true,
        },
    ];
}
