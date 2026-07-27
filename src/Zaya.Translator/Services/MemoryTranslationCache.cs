using System.Diagnostics.CodeAnalysis;

namespace Zaya.Translator.Services;

/// <summary>
/// In-memory implementation of <see cref="ITranslationCache"/> with optional
/// time-to-live (TTL) eviction. Expired entries are removed lazily on access.
/// Thread-safe only for single-threaded consumers (the translation loop is single-threaded).
/// </summary>
public sealed class MemoryTranslationCache : ITranslationCache
{
    private readonly TimeSpan? _ttl;
    private readonly Dictionary<string, CacheEntry> _entries = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryTranslationCache"/> class.
    /// </summary>
    /// <param name="ttl">
    /// Optional entry lifetime. When set, entries older than this duration are removed
    /// on the next <see cref="TryGet"/> access. Null means entries never expire.
    /// </param>
    public MemoryTranslationCache(TimeSpan? ttl = null)
    {
        _ttl = ttl;
    }

    /// <inheritdoc />
    public bool TryGet(string sourceText, [NotNullWhen(true)] out string? translation)
    {
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
        _entries[sourceText] = new CacheEntry(translation, DateTime.UtcNow);
    }

    /// <inheritdoc />
    public void Clear() => _entries.Clear();
}

/// <summary>
/// Internal struct representing a single cached translation entry
/// with its creation timestamp.
/// </summary>
internal readonly struct CacheEntry
{
    /// <summary>The translated text.</summary>
    public string Translated { get; }

    /// <summary>UTC timestamp when the entry was created.</summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initializes a new cache entry.
    /// </summary>
    public CacheEntry(string translated, DateTime createdAt)
    {
        Translated = translated;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Returns true if the entry's age exceeds the specified TTL.
    /// </summary>
    public bool IsExpired(TimeSpan ttl) => DateTime.UtcNow - CreatedAt > ttl;
}
