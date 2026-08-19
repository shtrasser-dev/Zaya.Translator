using Zaya.Logging.Services;
using Zaya.Primitives;
using Zaya.Primitives.Settings;
using Zaya.Translator.Impl.Yandex.Constants;
using Zaya.Translator.Impl.Yandex.Services.Impl;
using Zaya.Translator.Services;

namespace Zaya.Translator.Impl.Yandex;

/// <summary>
/// Translation engine using Yandex Translate REST API.
/// </summary>
public sealed class YandexTranslatorService : ITranslatorService
{
    private readonly ILoggingWrapper _loggingWrapper;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance using <see cref="EmptyLoggingWrapper.Instance"/>.
    /// </summary>
    public YandexTranslatorService() : this(EmptyLoggingWrapper.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified logging wrapper.
    /// </summary>
    /// <param name="loggingWrapper">Logging wrapper used when creating sessions.</param>
    public YandexTranslatorService(ILoggingWrapper loggingWrapper)
    {
        _loggingWrapper = loggingWrapper;
    }

    private static LocalizedString Loc(string key)
        => new(key, culture => Properties.Resources.ResourceManager.GetString(key, culture)!);

    /// <inheritdoc />
    public string EngineId => EngineConstants.EngineId;

    /// <inheritdoc />
    public LocalizedString DisplayName => Loc(LocalizationConstants.EngineName);

    /// <inheritdoc />
    public LocalizedString Description => Loc(LocalizationConstants.EngineDesc);

    /// <inheritdoc />
    public bool IsAvailable => true;

    /// <inheritdoc />
    public IReadOnlyList<SettingDescriptor> Settings => SettingsDescriptorsConstants.Settings;

    /// <inheritdoc />
    public Task<ITranslatorSession> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        var list = new SettingDescriptorList(SettingsDescriptorsConstants.Settings);
        return CreateSessionAsync(list, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ITranslatorSession> CreateSessionAsync(IReadOnlyDictionary<string, object> engineSettings, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        var list = new SettingDescriptorList(SettingsDescriptorsConstants.Settings);
        list.Bind(engineSettings);
        return CreateSessionAsync(list, cancellationToken);
    }

    private Task<ITranslatorSession> CreateSessionAsync(SettingDescriptorList settings, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        var autoDetect = settings.GetValueAsBool(SettingsConstants.AutoDetectLanguage);
        var sourceLang = autoDetect ? null : settings.GetValueAsString(SettingsConstants.SourceLanguage);
        var targetLang = settings.GetValueAsString(SettingsConstants.TargetLanguage);
        var useApiKey = settings.GetValueAsBool(SettingsConstants.UseApiKey);
        var apiKey = useApiKey ? settings.GetValueAsString(SettingsConstants.ApiKey) : null;
        var userAgent = settings.GetValueAsString(SettingsConstants.UserAgent);
        if (string.IsNullOrWhiteSpace(userAgent))
            userAgent = SettingsConstants.DefaultUserAgent;

        ITranslatorSession session = new YandexTranslatorSession(sourceLang, targetLang, apiKey, useApiKey, userAgent);
        return Task.FromResult(_loggingWrapper.Wrap<ITranslatorSession>(session));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
