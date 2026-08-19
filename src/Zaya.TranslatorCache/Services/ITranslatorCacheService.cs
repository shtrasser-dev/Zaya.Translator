using Zaya.Primitives;
using Zaya.Primitives.Settings;
using Zaya.Translator.Services;

namespace Zaya.TranslatorCache.Services;

/// <summary>
/// Pluggable translation-cache engine. Hosts discover implementations, bind
/// <see cref="Settings"/>, then wrap a raw <see cref="ITranslatorSession"/>
/// via <see cref="WrapSessionAsync(ITranslatorSession, IReadOnlyDictionary{string, object}, CancellationToken)"/>.
/// </summary>
public interface ITranslatorCacheService : IDisposable
{
    /// <summary>
    /// Gets a unique identifier for this cache engine (e.g. "memory-translator-cache").
    /// Used for profile serialization and engine lookup.
    /// </summary>
    string EngineId { get; }

    /// <summary>
    /// Gets the UI display name for this engine (localized).
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Gets the UI description for this engine (localized).
    /// </summary>
    LocalizedString Description { get; }

    /// <summary>
    /// Gets whether this cache engine is available on the current system.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Gets the list of engine-specific settings that can be configured via UI.
    /// </summary>
    IReadOnlyList<SettingDescriptor> Settings { get; }

    /// <summary>
    /// Wraps a raw translator session with caching according to default engine settings.
    /// </summary>
    /// <param name="inner">The session to wrap. Ownership transfers to the returned session.</param>
    /// <param name="cancellationToken">Token to cancel wrapping.</param>
    /// <returns>A session that may cache translation results.</returns>
    Task<ITranslatorSession> WrapSessionAsync(
        ITranslatorSession inner,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Wraps a raw translator session with caching according to the specified settings.
    /// </summary>
    /// <param name="inner">The session to wrap. Ownership transfers to the returned session.</param>
    /// <param name="engineSettings">Engine-specific settings dictionary.</param>
    /// <param name="cancellationToken">Token to cancel wrapping.</param>
    /// <returns>A session that may cache translation results (or <paramref name="inner"/> if caching is disabled).</returns>
    Task<ITranslatorSession> WrapSessionAsync(
        ITranslatorSession inner,
        IReadOnlyDictionary<string, object> engineSettings,
        CancellationToken cancellationToken = default);
}
