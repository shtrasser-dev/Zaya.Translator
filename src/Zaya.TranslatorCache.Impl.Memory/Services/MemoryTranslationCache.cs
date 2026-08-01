using System.Diagnostics.CodeAnalysis;

namespace Zaya.TranslatorCache.Impl.Memory.Services;

/// <summary>
/// In-memory exact translation cache with optional TTL eviction.
/// Thread-safe only for single-threaded consumers (the translation loop is single-threaded).
/// </summary>
public sealed class MemoryTranslationCache : ITranslationCache
{
    private readonly TimeSpan? _ttl;
    private readonly Dictionary<string, CacheEntry> _entries = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance with optional TTL.
    /// </summary>
    public MemoryTranslationCache(TimeSpan? ttl = null)
    {
        _ttl = ttl;
    }

    /// <summary>
    /// Initializes a new instance with the specified options.
    /// </summary>
    public MemoryTranslationCache(TranslationCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _ttl = options.Ttl;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Set(string sourceText, string translation)
    {
        ArgumentNullException.ThrowIfNull(sourceText);
        ArgumentNullException.ThrowIfNull(translation);

        _entries[sourceText] = new CacheEntry(translation, DateTime.UtcNow);
    }

    /// <inheritdoc />
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
