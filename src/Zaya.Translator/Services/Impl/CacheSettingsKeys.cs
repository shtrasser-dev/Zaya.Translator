namespace Zaya.Translator.Services.Impl;

/// <summary>
/// Machine-readable keys for translation cache settings.
/// These keys are passed as part of the engine settings dictionary
/// and consumed by <see cref="CacheFactory.TryWrap"/>.
/// </summary>
public static class CacheSettingsKeys
{
    /// <summary>Key for the boolean setting that enables or disables translation caching.</summary>
    public const string EnableCache = "enableCache";

    /// <summary>Key for the integer setting that specifies cache TTL in minutes (0 = unlimited).</summary>
    public const string CacheTtlMinutes = "cacheTtlMinutes";
}
