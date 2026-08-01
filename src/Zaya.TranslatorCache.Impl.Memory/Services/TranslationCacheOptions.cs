namespace Zaya.TranslatorCache.Impl.Memory.Services;

/// <summary>
/// Configuration for <see cref="MemoryTranslationCache"/>.
/// </summary>
public sealed class TranslationCacheOptions
{
    /// <summary>
    /// Optional entry lifetime. Null means entries never expire.
    /// </summary>
    public TimeSpan? Ttl { get; init; }
}
