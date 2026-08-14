using System.Diagnostics.CodeAnalysis;

namespace Zaya.TranslatorCache.Impl.Memory.Services;

/// <summary>
/// A cache store for translation results, keyed by source text.
/// </summary>
internal interface ITranslationCache
{
    bool TryGet(string sourceText, [NotNullWhen(true)] out string? translation);

    void Set(string sourceText, string translation);

    void Clear();
}
