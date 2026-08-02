using System.Text;
using System.Text.Json;
using Zaya.Translator.Impl.Yandex.Constants;
using Zaya.Translator.Services;

namespace Zaya.Translator.Impl.Yandex.Services;

/// <summary>
/// Translation session using Yandex Translate.
/// Uses either the Cloud API v2 (with API key) or the free browser endpoint (without key).
/// </summary>
public sealed class YandexTranslatorSession : ITranslatorSession
{
    private static readonly HttpClient SharedHttp = CreateHttpClient();

    private readonly string? _sourceLanguage;
    private readonly string _targetLanguage;
    private readonly string? _apiKey;
    private readonly bool _useCloudApi;
    private readonly string _userAgent;
    private bool _disposed;

    internal YandexTranslatorSession(
        string? sourceLanguage,
        string targetLanguage,
        string? apiKey,
        bool useApiKey,
        string userAgent)
    {
        _sourceLanguage = sourceLanguage;
        _targetLanguage = targetLanguage;
        _apiKey = apiKey;
        _useCloudApi = useApiKey && !string.IsNullOrWhiteSpace(apiKey);
        _userAgent = string.IsNullOrWhiteSpace(userAgent)
            ? SettingsConstants.DefaultUserAgent
            : userAgent;
    }

    /// <inheritdoc />
    public async Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return string.Empty;

        var result = await CallApiAsync(new[] { text }, cancellationToken);
        return result[0];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(texts);

        if (texts.Count == 0)
            return Array.Empty<string>();

        return await CallApiAsync(texts, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    private async Task<IReadOnlyList<string>> CallApiAsync(IReadOnlyList<string> texts, CancellationToken ct)
    {
        return _useCloudApi
            ? await CallCloudApiAsync(texts, ct)
            : await CallBrowserApiAsync(texts, ct);
    }

    private async Task<IReadOnlyList<string>> CallCloudApiAsync(IReadOnlyList<string> texts, CancellationToken ct)
    {
        var body = new Dictionary<string, object>
        {
            [YandexApiConstants.TargetLanguageCode] = Norm(_targetLanguage),
            [YandexApiConstants.Texts] = texts,
            [YandexApiConstants.Format] = YandexApiConstants.FormatPlainText,
        };

        if (_sourceLanguage is not null)
            body[YandexApiConstants.SourceLanguageCode] = Norm(_sourceLanguage);

        using var request = new HttpRequestMessage(HttpMethod.Post, YandexApiConstants.CloudTranslateUrl);
        request.Headers.TryAddWithoutValidation(
            HttpHeaderConstants.Authorization,
            YandexApiConstants.ApiKeyAuthorizationPrefix + _apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            YandexApiConstants.ApplicationJson);

        HttpResponseMessage response;
        try
        {
            response = await SharedHttp.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new YandexTranslateRequestException(ex.Message);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new YandexTranslateRequestException(ex.Message);
        }

        using (response)
        {
            var responseJson = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                throw new YandexTranslateRequestException(
                    $"Cloud API {(int)response.StatusCode} ({response.ReasonPhrase}): {responseJson}");
            }

            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var results = new List<string>();
                foreach (var t in doc.RootElement.GetProperty(YandexApiConstants.Translations).EnumerateArray())
                    results.Add(t.GetProperty(YandexApiConstants.Text).GetString() ?? string.Empty);
                return results;
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                throw new YandexTranslateParseException();
            }
        }
    }

    private async Task<IReadOnlyList<string>> CallBrowserApiAsync(IReadOnlyList<string> texts, CancellationToken ct)
    {
        var lang = FormatBrowserLang(_sourceLanguage, _targetLanguage);

        var baseUrl =
            $"{YandexApiConstants.BrowserTranslateUrl}?{YandexApiConstants.LangQuery}={lang}" +
            $"&{YandexApiConstants.SrvQuery}={YandexApiConstants.BrowserSrvValue}";
        var results = new List<string>(texts.Count);
        var chunk = new List<string>();
        var url = baseUrl;

        foreach (var text in texts)
        {
            var addition = $"&{YandexApiConstants.TextQuery}={Uri.EscapeDataString(text)}";
            if (chunk.Count > 0 && url.Length + addition.Length > YandexApiConstants.MaxBrowserUrlLength)
            {
                results.AddRange(await SendBrowserChunkAsync(url, ct));
                chunk.Clear();
                url = baseUrl;
            }

            chunk.Add(text);
            url += addition;
        }

        if (chunk.Count > 0)
            results.AddRange(await SendBrowserChunkAsync(url, ct));

        return results;
    }

    private async Task<IReadOnlyList<string>> SendBrowserChunkAsync(string url, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation(HttpHeaderConstants.UserAgent, _userAgent);
        request.Headers.TryAddWithoutValidation(HttpHeaderConstants.Origin, YandexApiConstants.WebsiteOrigin);
        request.Headers.Referrer = new Uri(YandexApiConstants.WebsiteReferrer);

        HttpResponseMessage response;
        try
        {
            response = await SharedHttp.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new YandexTranslateRequestException(ex.Message);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new YandexTranslateRequestException(ex.Message);
        }

        using (response)
        {
            var responseJson = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                throw new YandexTranslateRequestException(
                    $"Browser API {(int)response.StatusCode} ({response.ReasonPhrase}): {responseJson}");
            }

            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var results = new List<string>();
                foreach (var elem in doc.RootElement.GetProperty(YandexApiConstants.Text).EnumerateArray())
                    results.Add(elem.GetString() ?? string.Empty);
                return results;
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                throw new YandexTranslateParseException();
            }
        }
    }

    /// <summary>
    /// Browser endpoint: <c>en-ru</c> when source is known; target-only (<c>ru</c>) enables auto-detect.
    /// A leading dash (<c>-ru</c>) is rejected by the API with HTTP 400.
    /// </summary>
    internal static string FormatBrowserLang(string? sourceLanguage, string targetLanguage) =>
        sourceLanguage is not null
            ? $"{Norm(sourceLanguage)}-{Norm(targetLanguage)}"
            : Norm(targetLanguage);

    private static HttpClient CreateHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        };
        return new HttpClient(handler);
    }

    /// <summary>
    /// Maps BCP-47 tags to Yandex language codes.
    /// Yandex exposes a single Chinese code (<c>zh</c>).
    /// </summary>
    private static string Norm(string bcp47) => bcp47 switch
    {
        LanguageCodeConstants.ChineseSimplified or LanguageCodeConstants.ChineseTraditional or LanguageCodeConstants.Chinese
            => LanguageCodeConstants.Chinese,
        _ => bcp47,
    };
}
