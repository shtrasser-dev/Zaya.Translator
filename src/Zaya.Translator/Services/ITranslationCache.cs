using System.Diagnostics.CodeAnalysis;

namespace Zaya.Translator.Services;

/// <summary>
/// A cache store for translation results, keyed by source text.
/// Implementations can add custom eviction policies, TTL, or persistence.
/// </summary>
public interface ITranslationCache
{
    /// <summary>
    /// Attempts to retrieve a cached translation for the specified source text.
    /// </summary>
    /// <param name="sourceText">The source text to look up.</param>
    /// <param name="translation">The cached translation if found and valid, otherwise null.</param>
    /// <returns>True if a valid cached translation was found; false otherwise.</returns>
    bool TryGet(string sourceText, [NotNullWhen(true)] out string? translation);

    /// <summary>
    /// Stores a translation result for the specified source text.
    /// </summary>
    /// <param name="sourceText">The source text key.</param>
    /// <param name="translation">The translated text to cache.</param>
    void Set(string sourceText, string translation);

    /// <summary>
    /// Removes all entries from the cache.
    /// </summary>
    void Clear();
}
