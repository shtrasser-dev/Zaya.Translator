namespace Zaya.TranslatorCache.Impl.Memory.Services.Impl;

/// <summary>
/// Configuration for <see cref="MemoryTranslationCache"/>.
/// </summary>
internal sealed class TranslationCacheOptions
{
    /// <summary>
    /// Optional entry lifetime. Null means entries never expire.
    /// </summary>
    public TimeSpan? Ttl { get; init; }
}
