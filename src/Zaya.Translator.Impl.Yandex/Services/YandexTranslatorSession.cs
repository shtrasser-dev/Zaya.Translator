using System.Net.Http;
using System.Text;
using System.Text.Json;
using Zaya.Translator.Services;

namespace Zaya.Translator.Impl.Yandex.Services;

/// <summary>
/// Translation session using Yandex Translate.
/// Uses either the Cloud API v2 (with API key) or the free browser endpoint (without key).
/// </summary>
public sealed class YandexTranslatorSession : ITranslatorSession
{
    private const string CloudApiUrl = "https://translate.api.cloud.yandex.net/translate/v2/translate";
    private const string BrowserApiUrl = "https://browser.translate.yandex.net/api/v1/tr.json/translate";
    private const int MaxBrowserUrlLength = 1800;

    private static readonly HttpClient SharedHttp = CreateHttpClient();

    private readonly string? _sourceLanguage;
    private readonly string _targetLanguage;
    private readonly string? _apiKey;
    private readonly bool _useCloudApi;
    private bool _disposed;

    internal YandexTranslatorSession(string? sourceLanguage, string targetLanguage, string? apiKey, bool useApiKey)
    {
        _sourceLanguage = sourceLanguage;
        _targetLanguage = targetLanguage;
        _apiKey = apiKey;
        _useCloudApi = useApiKey && !string.IsNullOrWhiteSpace(apiKey);
    }

    /// <inheritdoc />
    public async Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return "";

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
            ["targetLanguageCode"] = Norm(_targetLanguage),
            ["texts"] = texts,
            ["format"] = "PLAIN_TEXT",
        };

        if (_sourceLanguage is not null)
            body["sourceLanguageCode"] = Norm(_sourceLanguage);

        using var request = new HttpRequestMessage(HttpMethod.Post, CloudApiUrl);
        request.Headers.TryAddWithoutValidation("Authorization", $"Api-Key {_apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

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
                foreach (var t in doc.RootElement.GetProperty("translations").EnumerateArray())
                    results.Add(t.GetProperty("text").GetString() ?? "");
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
        var lang = _sourceLanguage is not null
            ? $"{Norm(_sourceLanguage)}-{Norm(_targetLanguage)}"
            : $"-{Norm(_targetLanguage)}";

        var baseUrl = $"{BrowserApiUrl}?lang={lang}&srv=browser_video_translation";
        var results = new List<string>(texts.Count);
        var chunk = new List<string>();
        var url = baseUrl;

        foreach (var text in texts)
        {
            var addition = $"&text={Uri.EscapeDataString(text)}";
            if (chunk.Count > 0 && url.Length + addition.Length > MaxBrowserUrlLength)
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

    private static async Task<IReadOnlyList<string>> SendBrowserChunkAsync(string url, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
        request.Headers.TryAddWithoutValidation("Origin", "https://translate.yandex.com");
        request.Headers.Referrer = new Uri("https://translate.yandex.com/");

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
                foreach (var elem in doc.RootElement.GetProperty("text").EnumerateArray())
                    results.Add(elem.GetString() ?? "");
                return results;
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                throw new YandexTranslateParseException();
            }
        }
    }

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
        "zh-Hans" or "zh-Hant" or "zh" => "zh",
        _ => bcp47,
    };
}
