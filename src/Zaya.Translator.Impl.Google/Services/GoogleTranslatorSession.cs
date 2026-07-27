using System.Net.Http;
using System.Text;
using System.Text.Json;
using Zaya.Translator.Services;

namespace Zaya.Translator.Impl.Google.Services;

/// <summary>
/// Translation session using Google Translate (unofficial free endpoint).
/// </summary>
public sealed class GoogleTranslatorSession : ITranslatorSession
{
    private const string BaseUrl = "https://translate.googleapis.com/translate_a/single";

    private static readonly HttpClient SharedHttp = CreateHttpClient();

    private readonly string? _sourceLanguage;
    private readonly string _targetLanguage;
    private bool _disposed;

    internal GoogleTranslatorSession(string? sourceLanguage, string targetLanguage)
    {
        _sourceLanguage = sourceLanguage;
        _targetLanguage = targetLanguage;
    }

    /// <inheritdoc />
    public async Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return "";

        var sl = _sourceLanguage is not null ? Norm(_sourceLanguage) : "auto";
        var tl = Norm(_targetLanguage);
        var url = $"{BaseUrl}?client=gtx&sl={sl}&tl={tl}&dt=t&q={Uri.EscapeDataString(text)}";

        string response;
        try
        {
            response = await SharedHttp.GetStringAsync(url, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new GoogleTranslateRequestException(ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new GoogleTranslateRequestException(ex.Message);
        }

        try
        {
            using var doc = JsonDocument.Parse(response);
            if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
                throw new GoogleTranslateParseException();

            var segments = doc.RootElement[0];
            if (segments.ValueKind != JsonValueKind.Array)
                throw new GoogleTranslateParseException();

            var translated = new StringBuilder();
            for (var i = 0; i < segments.GetArrayLength(); i++)
            {
                var item = segments[i];
                if (item.ValueKind != JsonValueKind.Array || item.GetArrayLength() == 0)
                    continue;
                var seg = item[0].GetString();
                if (seg is not null)
                    translated.Append(seg);
            }
            return translated.ToString();
        }
        catch (GoogleTranslateParseException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw new GoogleTranslateParseException();
        }
        catch (InvalidOperationException)
        {
            throw new GoogleTranslateParseException();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(texts);

        var results = new List<string>(texts.Count);
        foreach (var text in texts)
            results.Add(await TranslateAsync(text, cancellationToken));
        return results;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    private static HttpClient CreateHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        };
        return new HttpClient(handler);
    }

    private static string Norm(string bcp47) => bcp47 switch
    {
        "zh-Hans" => "zh-CN",
        "zh-Hant" => "zh-TW",
        _ => bcp47,
    };
}
