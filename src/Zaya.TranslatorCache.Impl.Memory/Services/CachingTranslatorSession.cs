using Zaya.Translator.Services;

namespace Zaya.TranslatorCache.Impl.Memory.Services;

/// <summary>
/// Decorator over <see cref="ITranslatorSession"/> that caches translation results
/// using an <see cref="ITranslationCache"/> instance.
/// </summary>
public sealed class CachingTranslatorSession : ITranslatorSession
{
    private readonly ITranslatorSession _inner;
    private readonly ITranslationCache _cache;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingTranslatorSession"/> class.
    /// </summary>
    public CachingTranslatorSession(ITranslatorSession inner, ITranslationCache cache)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    public async Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);

        if (_cache.TryGet(text, out var cached))
            return cached;

        var result = await _inner.TranslateAsync(text, cancellationToken);
        _cache.Set(text, result);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(texts);

        var result = new string[texts.Count];
        var uncachedIndices = new List<int>(texts.Count);

        for (int i = 0; i < texts.Count; i++)
        {
            if (_cache.TryGet(texts[i], out var cached))
                result[i] = cached;
            else
                uncachedIndices.Add(i);
        }

        if (uncachedIndices.Count == 0)
            return result;

        var uncachedTexts = uncachedIndices.Select(i => texts[i]).ToList();
        var fresh = await _inner.TranslateAsync(uncachedTexts, cancellationToken);

        for (int j = 0; j < uncachedIndices.Count; j++)
        {
            var idx = uncachedIndices[j];
            var translated = fresh[j];
            _cache.Set(texts[idx], translated);
            result[idx] = translated;
        }

        return result;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _inner.Dispose();
    }
}
