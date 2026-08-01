using System.Diagnostics.CodeAnalysis;

namespace Zaya.TranslatorCache.Impl.Memory.Services;

/// <summary>
/// A cache store for translation results, keyed by source text.
/// </summary>
public interface ITranslationCache
{
    /// <summary>
    /// Attempts to retrieve a cached translation for the specified source text.
    /// </summary>
    bool TryGet(string sourceText, [NotNullWhen(true)] out string? translation);

    /// <summary>
    /// Stores a translation result for the specified source text.
    /// </summary>
    void Set(string sourceText, string translation);

    /// <summary>
    /// Removes all entries from the cache.
    /// </summary>
    void Clear();
}
