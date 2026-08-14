using System.Globalization;
using Zaya.Primitives;
using Zaya.Translator.Impl.Google.Constants;

namespace Zaya.Translator.Impl.Google.Exceptions;

/// <summary>
/// Thrown when the Google Translate response cannot be parsed.
/// </summary>
public sealed class GoogleTranslateParseException : LocalizedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleTranslateParseException"/> class.
    /// </summary>
    public GoogleTranslateParseException()
        : base(LocalizationConstants.Err_ParseFailed) { }

    /// <inheritdoc />
    public override string GetLocalizedMessage(CultureInfo culture)
        => Properties.Resources.ResourceManager.GetString(LocalizationConstants.Err_ParseFailed, culture)
           ?? base.GetLocalizedMessage(culture);
}
