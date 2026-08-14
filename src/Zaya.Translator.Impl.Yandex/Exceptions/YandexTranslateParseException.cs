using System.Globalization;
using Zaya.Primitives;
using Zaya.Translator.Impl.Yandex.Constants;

namespace Zaya.Translator.Impl.Yandex.Exceptions;

/// <summary>
/// Thrown when the Yandex Translate response cannot be parsed.
/// </summary>
public sealed class YandexTranslateParseException : LocalizedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YandexTranslateParseException"/> class.
    /// </summary>
    public YandexTranslateParseException()
        : base(LocalizationConstants.Err_ParseFailed) { }

    /// <inheritdoc />
    public override string GetLocalizedMessage(CultureInfo culture)
        => Properties.Resources.ResourceManager.GetString(LocalizationConstants.Err_ParseFailed, culture)
           ?? base.GetLocalizedMessage(culture);
}
