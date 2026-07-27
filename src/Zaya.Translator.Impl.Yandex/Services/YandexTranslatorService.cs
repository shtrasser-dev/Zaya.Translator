using Zaya.Primitives;
using Zaya.Translator.Impl.Yandex.Constants;
using Zaya.Translator.Services;
using Zaya.Translator.Services.Impl;

namespace Zaya.Translator.Impl.Yandex.Services;

/// <summary>
/// Translation engine using Yandex Translate REST API.
/// </summary>
public sealed class YandexTranslatorService : ITranslatorService
{
    private const string EngineIdValue = "yandex";

    private static readonly IReadOnlyList<EnumOption> LanguageOptions = CreateLanguageOptions();
    private static readonly IReadOnlyList<SettingDescriptor> SettingsList = BuildSettings();

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
    public IReadOnlyList<SettingDescriptor> Settings => SettingsList;

    /// <inheritdoc />
    public Task<ITranslatorSession> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        var list = new SettingDescriptorList(SettingsList);
        return CreateSessionAsync(list, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ITranslatorSession> CreateSessionAsync(IReadOnlyDictionary<string, object> engineSettings, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        var list = new SettingDescriptorList(SettingsList);
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

        var session = new YandexTranslatorSession(sourceLang, targetLang, apiKey, useApiKey) as ITranslatorSession;
        session = CacheFactory.TryWrap(session, settings);
        return Task.FromResult(session);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    /// <summary>
    /// Yandex APIs expose a single Chinese code (<c>zh</c>), so Hans/Hant are collapsed in the UI.
    /// </summary>
    private static IReadOnlyList<EnumOption> CreateLanguageOptions()
    {
        var options = new List<EnumOption>(Languages.All.Count);
        var chineseAdded = false;
        foreach (var option in Languages.All)
        {
            if (option.Value is "zh-Hans" or "zh-Hant")
            {
                if (chineseAdded)
                    continue;
                options.Add(new EnumOption("zh", option.DisplayName, option.Description));
                chineseAdded = true;
                continue;
            }

            options.Add(option);
        }

        return options;
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
                IsVisible = s => s.GetValueOrDefault(SettingsConstants.AutoDetectLanguage) is not true,
                IsRequired = s => s.GetValueOrDefault(SettingsConstants.AutoDetectLanguage) is not true,
                Options = LanguageOptions,
            },
            new EnumSettingDescriptor(SettingsConstants.TargetLanguage, Loc(LocalizationConstants.TargetLanguage))
            {
                Description = Loc(LocalizationConstants.TargetLanguage_Desc),
                IsRequired = static _ => true,
                DefaultValue = "ru",
                Options = LanguageOptions,
            },
            new BooleanSettingDescriptor(SettingsConstants.UseApiKey, Loc(LocalizationConstants.UseApiKey))
            {
                Description = Loc(LocalizationConstants.UseApiKey_Desc),
                DefaultValue = false,
            },
            new PasswordSettingDescriptor(SettingsConstants.ApiKey, Loc(LocalizationConstants.ApiKey))
            {
                Description = Loc(LocalizationConstants.ApiKey_Desc),
                IsVisible = s => s.GetValueOrDefault(SettingsConstants.UseApiKey) is true,
                IsRequired = s => s.GetValueOrDefault(SettingsConstants.UseApiKey) is true,
            },
            ..CacheSettingDescriptors.All,
        ];
    }
}
