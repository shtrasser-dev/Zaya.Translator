namespace Zaya.Translator.Services;

/// <summary>
/// Represents an active translation session with a fixed language pair and configuration.
/// </summary>
public interface ITranslatorSession : IDisposable
{
    /// <summary>
    /// Translates the specified text to the target language configured at session creation.
    /// </summary>
    /// <param name="text">The text to translate. Must not be null.</param>
    /// <param name="cancellationToken">Token to cancel the translation operation.</param>
    /// <returns>The translated text.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
    Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Translates a batch of texts to the target language configured at session creation.
    /// </summary>
    /// <param name="texts">The texts to translate. Must not be null.</param>
    /// <param name="cancellationToken">Token to cancel the translation operation.</param>
    /// <returns>The translated texts in the same order.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="texts"/> is null.</exception>
    Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
}
