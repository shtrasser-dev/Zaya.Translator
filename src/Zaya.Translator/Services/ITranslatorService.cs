using Zaya.Primitives;
using Zaya.Primitives.Settings;

namespace Zaya.Translator.Services;

/// <summary>
/// Provides text translation capabilities.
/// Source and target languages are declared via <see cref="Settings"/> and passed at session creation.
/// </summary>
public interface ITranslatorService : IDisposable
{
    /// <summary>
    /// Gets a unique identifier for this translation engine (e.g., "azure-ai", "google").
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
    /// Gets whether this translation engine is available on the current system.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Gets the list of engine-specific settings that can be configured via UI.
    /// </summary>
    IReadOnlyList<SettingDescriptor> Settings { get; }

    /// <summary>
    /// Creates a new translation session with the default engine settings.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel session creation.</param>
    /// <returns>An active translation session ready to translate text.</returns>
    Task<ITranslatorSession> CreateSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new translation session with the specified engine settings.
    /// </summary>
    /// <param name="engineSettings">Engine-specific settings dictionary.</param>
    /// <param name="cancellationToken">Token to cancel session creation.</param>
    /// <returns>An active translation session ready to translate text.</returns>
    Task<ITranslatorSession> CreateSessionAsync(IReadOnlyDictionary<string, object> engineSettings, CancellationToken cancellationToken = default);
}
