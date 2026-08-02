namespace Zaya.Translator.Impl.Google.Constants;

/// <summary>Google Translate free endpoint URL, query parameters, and batch markers.</summary>
internal static class GoogleApiConstants
{
    public const string TranslateUrl = "https://translate.googleapis.com/translate_a/single";
    public const int MaxUrlLength = 1800;

    public const string MarkerOpen = "([";
    public const string MarkerClose = "])";
    public const string BatchSegmentPattern = @"\(\[(.*?)\]\)";

    public const string ClientQuery = "client";
    public const string ClientGtx = "gtx";
    public const string SourceLanguageQuery = "sl";
    public const string TargetLanguageQuery = "tl";
    public const string DataTypeQuery = "dt";
    public const string DataTypeTranslation = "t";
    public const string QueryText = "q";
}
