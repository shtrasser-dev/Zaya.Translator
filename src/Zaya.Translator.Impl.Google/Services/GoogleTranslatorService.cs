using Zaya.Primitives;
using Zaya.Translator.Impl.Google.Constants;
using Zaya.Translator.Services;
using Zaya.Translator.Services.Impl;

namespace Zaya.Translator.Impl.Google.Services;

/// <summary>
/// Translation engine using Google Translate (unofficial free endpoint).
/// </summary>
public sealed class GoogleTranslatorService : ITranslatorService
{
    private const string EngineIdValue = "google";

    private static IReadOnlyList<SettingDescriptor> _settings = BuildSettings();

    private bool _disposed;

    private static LocalizedString Loc(string key)
        => new(key, culture => Properties.Resources.ResourceManager.GetString(key, culture)!);

    /// <inheritdoc />
    public string EngineId => EngineIdValue;

    /// <inheritdoc />
    public LocalizedString DisplayName => Loc(LocalizationConstants.EngineName);

    /// <inheritdoc />
    public LocalizedString Description => Loc(LocalizationConstants.EngineDesc);

    /// <inheritdoc />
    public bool IsAvailable => true;

    /// <inheritdoc />
    public IReadOnlyList<SettingDescriptor> Settings => _settings;

    /// <inheritdoc />
    public Task<ITranslatorSession> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        var list = new SettingDescriptorList(_settings);
        return CreateSessionAsync(list, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ITranslatorSession> CreateSessionAsync(IReadOnlyDictionary<string, object> engineSettings, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        var list = new SettingDescriptorList(_settings);
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

        var session = new GoogleTranslatorSession(sourceLang, targetLang) as ITranslatorSession;
        session = CacheFactory.TryWrap(session, settings);
        return Task.FromResult(session);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    private static IReadOnlyList<SettingDescriptor> BuildSettings()
    {
        return [
            new BooleanSettingDescriptor(SettingsConstants.AutoDetectLanguage, Loc(LocalizationConstants.AutoDetectLanguage))
            {
                Description = Loc(LocalizationConstants.AutoDetectLanguage_Desc),
                DefaultValue = true,
            },
            new EnumSettingDescriptor(SettingsConstants.SourceLanguage, Loc(LocalizationConstants.SourceLanguage))
            {
                Description = Loc(LocalizationConstants.SourceLanguage_Desc),
                DefaultValue = "en",
                // Missing key must follow DefaultValue (true): hide source until user turns auto-detect off.
                IsVisible = s => s.GetValueOrDefault(SettingsConstants.AutoDetectLanguage) is false,
                IsRequired = s => s.GetValueOrDefault(SettingsConstants.AutoDetectLanguage) is false,
                Options = Languages.All,
            },
            new EnumSettingDescriptor(SettingsConstants.TargetLanguage, Loc(LocalizationConstants.TargetLanguage))
            {
                Description = Loc(LocalizationConstants.TargetLanguage_Desc),
                IsRequired = static _ => true,
                DefaultValue = "ru",
                Options = Languages.All,
            },
            ..CacheSettingDescriptors.All,
        ];
    }
}
