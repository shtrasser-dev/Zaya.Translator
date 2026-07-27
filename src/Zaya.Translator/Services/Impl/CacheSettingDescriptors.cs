using Zaya.Primitives;

namespace Zaya.Translator.Services.Impl;

/// <summary>
/// Provides the <see cref="SettingDescriptor"/> list for cache configuration.
/// Translators include <see cref="All"/> in their own <c>BuildSettings()</c>
/// return value so that the cache settings are rendered and read together with
/// the translator's own settings.
/// </summary>
public static class CacheSettingDescriptors
{
    private static LocalizedString Loc(string key)
        => new(key, culture => Properties.Resources.Instance.GetString(key, culture));

    /// <summary>
    /// Gets the descriptor list containing cache-related settings:
    /// <list type="bullet">
    ///   <item><description><see cref="CacheSettingsKeys.EnableCache"/> – boolean toggle (default: true).</description></item>
    ///   <item><description><see cref="CacheSettingsKeys.CacheTtlMinutes"/> – integer TTL in minutes (default: 0 = unlimited).</description></item>
    /// </list>
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
        },
    ];
}
