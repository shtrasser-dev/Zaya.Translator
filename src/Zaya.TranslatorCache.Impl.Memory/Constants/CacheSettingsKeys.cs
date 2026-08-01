namespace Zaya.TranslatorCache.Impl.Memory.Constants;

/// <summary>Machine-readable keys for memory translation-cache settings.</summary>
public static class CacheSettingsKeys
{
    /// <summary>Key for the boolean setting that enables or disables translation caching.</summary>
    public const string EnableCache = "enableCache";

    /// <summary>Key for the integer setting that specifies cache TTL in minutes (0 = unlimited).</summary>
    public const string CacheTtlMinutes = "cacheTtlMinutes";
}
