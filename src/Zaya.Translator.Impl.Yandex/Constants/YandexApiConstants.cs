namespace Zaya.Translator.Impl.Yandex.Constants;

/// <summary>Yandex Translate Cloud / browser endpoint URLs, JSON keys, and query parameters.</summary>
internal static class YandexApiConstants
{
    public const string CloudTranslateUrl = "https://translate.api.cloud.yandex.net/translate/v2/translate";
    public const string BrowserTranslateUrl = "https://browser.translate.yandex.net/api/v1/tr.json/translate";
    public const string WebsiteOrigin = "https://translate.yandex.com";
    public const string WebsiteReferrer = "https://translate.yandex.com/";

    public const int MaxBrowserUrlLength = 1800;

    public const string TargetLanguageCode = "targetLanguageCode";
    public const string SourceLanguageCode = "sourceLanguageCode";
    public const string Texts = "texts";
    public const string Format = "format";
    public const string FormatPlainText = "PLAIN_TEXT";
    public const string Translations = "translations";
    public const string Text = "text";

    public const string ApiKeyAuthorizationPrefix = "Api-Key ";
    public const string ApplicationJson = "application/json";

    public const string LangQuery = "lang";
    public const string SrvQuery = "srv";
    public const string BrowserSrvValue = "browser_video_translation";
    public const string TextQuery = "text";
}
