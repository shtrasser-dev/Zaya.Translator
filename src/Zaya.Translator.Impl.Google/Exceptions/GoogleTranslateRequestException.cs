using System.Globalization;
using Zaya.Primitives;
using Zaya.Translator.Impl.Google.Constants;

namespace Zaya.Translator.Impl.Google.Exceptions;

/// <summary>
/// Thrown when a Google Translate HTTP request fails.
/// </summary>
public sealed class GoogleTranslateRequestException : LocalizedException
{
    private readonly string _detail;

    /// <summary>
    /// Gets the technical failure detail.
    /// </summary>
    public string Detail => _detail;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleTranslateRequestException"/> class.
    /// </summary>
    public GoogleTranslateRequestException(string detail)
        : base(LocalizationConstants.Err_RequestFailed)
    {
        _detail = detail;
    }

    /// <inheritdoc />
    public override string GetLocalizedMessage(CultureInfo culture)
    {
        var format = Properties.Resources.ResourceManager.GetString(LocalizationConstants.Err_RequestFailed, culture)
                     ?? base.GetLocalizedMessage(culture);
        return string.Format(format, _detail);
    }
}
