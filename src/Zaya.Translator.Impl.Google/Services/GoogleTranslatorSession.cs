using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Zaya.Translator.Impl.Google.Constants;
using Zaya.Translator.Services;

namespace Zaya.Translator.Impl.Google.Services;

/// <summary>
/// Translation session using Google Translate (unofficial free endpoint).
/// Batch requests wrap each segment as <c>([…])</c> so multiple blocks share one HTTP call.
/// </summary>
public sealed class GoogleTranslatorSession : ITranslatorSession
{
    private static readonly HttpClient SharedHttp = CreateHttpClient();
    private static readonly Regex BatchSegmentRegex = new(
        GoogleApiConstants.BatchSegmentPattern,
        RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly string? _sourceLanguage;
    private readonly string _targetLanguage;
    private readonly string _userAgent;
    private bool _disposed;

    internal GoogleTranslatorSession(string? sourceLanguage, string targetLanguage, string userAgent)
    {
        _sourceLanguage = sourceLanguage;
        _targetLanguage = targetLanguage;
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

        return await TranslateRawAsync(text, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> TranslateAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(texts);

        if (texts.Count == 0)
            return Array.Empty<string>();

        if (texts.Count == 1)
            return [await TranslateAsync(texts[0], cancellationToken).ConfigureAwait(false)];

        // Markers must stay unique delimiters; fall back if a segment already contains them.
        if (texts.Any(t => t.Contains(GoogleApiConstants.MarkerOpen, StringComparison.Ordinal)
                           || t.Contains(GoogleApiConstants.MarkerClose, StringComparison.Ordinal)))
        {
            var sequential = new List<string>(texts.Count);
            foreach (var text in texts)
                sequential.Add(await TranslateAsync(text, cancellationToken).ConfigureAwait(false));
            return sequential;
        }

        var results = new string[texts.Count];
        var chunkIndices = new List<int>();
        var chunkTexts = new List<string>();
        var urlPrefix = BuildUrlPrefix();

        for (var i = 0; i < texts.Count; i++)
        {
            var candidate = WrapBatch(chunkTexts.Append(texts[i]));
            var encodedLength = Uri.EscapeDataString(candidate).Length;
            if (chunkTexts.Count > 0 && urlPrefix.Length + encodedLength > GoogleApiConstants.MaxUrlLength)
            {
                await TranslateChunkAsync(chunkTexts, chunkIndices, results, cancellationToken)
                    .ConfigureAwait(false);
                chunkIndices.Clear();
                chunkTexts.Clear();
            }

            chunkIndices.Add(i);
            chunkTexts.Add(texts[i]);
        }

        if (chunkTexts.Count > 0)
        {
            await TranslateChunkAsync(chunkTexts, chunkIndices, results, cancellationToken)
                .ConfigureAwait(false);
        }

        return results;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    internal static string WrapBatch(IEnumerable<string> texts)
    {
        var sb = new StringBuilder();
        foreach (var text in texts)
            sb.Append(GoogleApiConstants.MarkerOpen).Append(text).Append(GoogleApiConstants.MarkerClose);
        return sb.ToString();
    }

    /// <summary>Splits a batched Google response back into per-segment translations.</summary>
    internal static IReadOnlyList<string> UnwrapBatch(string translated, int expectedCount)
    {
        var matches = BatchSegmentRegex.Matches(translated);
        if (matches.Count != expectedCount)
            throw new GoogleTranslateParseException();

        var results = new string[expectedCount];
        for (var i = 0; i < matches.Count; i++)
            results[i] = matches[i].Groups[1].Value;
        return results;
    }

    private async Task TranslateChunkAsync(
        List<string> chunkTexts,
        List<int> chunkIndices,
        string[] results,
        CancellationToken cancellationToken)
    {
        if (chunkTexts.Count == 1)
        {
            results[chunkIndices[0]] = await TranslateAsync(chunkTexts[0], cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        var wrapped = WrapBatch(chunkTexts);
        var translated = await TranslateRawAsync(wrapped, cancellationToken).ConfigureAwait(false);
        var parts = UnwrapBatch(translated, chunkTexts.Count);
        for (var i = 0; i < parts.Count; i++)
            results[chunkIndices[i]] = parts[i];
    }

    private async Task<string> TranslateRawAsync(string text, CancellationToken cancellationToken)
    {
        var url = BuildUrlPrefix() + Uri.EscapeDataString(text);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation(HttpHeaderConstants.UserAgent, _userAgent);

        string response;
        try
        {
            using var httpResponse = await SharedHttp.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new GoogleTranslateRequestException(
                    $"Google Translate {(int)httpResponse.StatusCode} ({httpResponse.ReasonPhrase}): {response}");
            }
        }
        catch (GoogleTranslateRequestException)
        {
            throw;
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

    private string BuildUrlPrefix()
    {
        var sl = _sourceLanguage is not null ? Norm(_sourceLanguage) : LanguageCodeConstants.Auto;
        var tl = Norm(_targetLanguage);
        return $"{GoogleApiConstants.TranslateUrl}" +
               $"?{GoogleApiConstants.ClientQuery}={GoogleApiConstants.ClientGtx}" +
               $"&{GoogleApiConstants.SourceLanguageQuery}={sl}" +
               $"&{GoogleApiConstants.TargetLanguageQuery}={tl}" +
               $"&{GoogleApiConstants.DataTypeQuery}={GoogleApiConstants.DataTypeTranslation}" +
               $"&{GoogleApiConstants.QueryText}=";
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
        LanguageCodeConstants.ChineseSimplified => LanguageCodeConstants.ChineseChina,
        LanguageCodeConstants.ChineseTraditional => LanguageCodeConstants.ChineseTaiwan,
        _ => bcp47,
    };
}
