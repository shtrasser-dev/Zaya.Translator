using System.Diagnostics.CodeAnalysis;
using Zaya.TranslatorCache.Impl.Memory.Services;

namespace Zaya.TranslatorCache.Impl.Memory.Services.Impl;

/// <summary>
/// In-memory exact translation cache with optional TTL eviction.
/// Thread-safe only for single-threaded consumers (the translation loop is single-threaded).
/// </summary>
internal sealed class MemoryTranslationCache : ITranslationCache
{
    private readonly TimeSpan? _ttl;
    private readonly Dictionary<string, CacheEntry> _entries = new(StringComparer.Ordinal);

    public MemoryTranslationCache(TimeSpan? ttl = null)
    {
        _ttl = ttl;
    }

    public MemoryTranslationCache(TranslationCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _ttl = options.Ttl;
    }

    public bool TryGet(string sourceText, [NotNullWhen(true)] out string? translation)
    {
        ArgumentNullException.ThrowIfNull(sourceText);

        if (_entries.TryGetValue(sourceText, out var entry))
        {
            if (_ttl.HasValue && entry.IsExpired(_ttl.Value))
            {
                _entries.Remove(sourceText);
                translation = null;
                return false;
            }

            translation = entry.Translated;
            return true;
        }

        translation = null;
        return false;
    }

    public void Set(string sourceText, string translation)
    {
        ArgumentNullException.ThrowIfNull(sourceText);
        ArgumentNullException.ThrowIfNull(translation);

        _entries[sourceText] = new CacheEntry(translation, DateTime.UtcNow);
    }

    public void Clear()
    {
        _entries.Clear();
    }

    private readonly struct CacheEntry
    {
        public string Translated { get; }
        public DateTime CreatedAt { get; }

        public CacheEntry(string translated, DateTime createdAt)
        {
            Translated = translated;
            CreatedAt = createdAt;
        }

        public bool IsExpired(TimeSpan ttl) => DateTime.UtcNow - CreatedAt > ttl;
    }
}
