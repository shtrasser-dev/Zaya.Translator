using Zaya.Logging.Services;
using Zaya.Primitives;
using Zaya.Translator.Services;
using Zaya.TranslatorCache.Impl.Memory.Constants;
using Zaya.TranslatorCache.Impl.Memory.Services.Impl;
using Zaya.TranslatorCache.Services;

namespace Zaya.TranslatorCache.Impl.Memory;

/// <summary>
/// In-memory translation cache engine (exact dictionary + optional TTL).
/// </summary>
public sealed class MemoryTranslatorCacheService : ITranslatorCacheService
{
    private static readonly IReadOnlyList<SettingDescriptor> SettingsList = SettingsDescriptorsConstants.Settings;

    private readonly ILoggingWrapper _loggingWrapper;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance using <see cref="EmptyLoggingWrapper.Instance"/>.
    /// </summary>
    public MemoryTranslatorCacheService() : this(EmptyLoggingWrapper.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified logging wrapper.
    /// </summary>
    /// <param name="loggingWrapper">Logging wrapper used when wrapping sessions.</param>
    public MemoryTranslatorCacheService(ILoggingWrapper loggingWrapper)
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
    public IReadOnlyList<SettingDescriptor> Settings => SettingsList;

    /// <inheritdoc />
    public Task<ITranslatorSession> WrapSessionAsync(
        ITranslatorSession inner,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(inner);
        cancellationToken.ThrowIfCancellationRequested();
        var list = new SettingDescriptorList(SettingsList);
        return Task.FromResult(Wrap(inner, list));
    }

    /// <inheritdoc />
    public Task<ITranslatorSession> WrapSessionAsync(
        ITranslatorSession inner,
        IReadOnlyDictionary<string, object> engineSettings,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(engineSettings);
        cancellationToken.ThrowIfCancellationRequested();
        var list = new SettingDescriptorList(SettingsList);
        list.Bind(engineSettings);
        return Task.FromResult(Wrap(inner, list));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    private ITranslatorSession Wrap(ITranslatorSession inner, SettingDescriptorList settings)
    {
        if (!settings.GetValueAsBool(SettingsConstants.EnableCache))
            return _loggingWrapper.Wrap<ITranslatorSession>(inner);

        var ttlMinutes = settings.GetValueAsInt(SettingsConstants.CacheTtlMinutes);
        TimeSpan? ttl = ttlMinutes > 0 ? TimeSpan.FromMinutes(ttlMinutes) : null;

        var cache = new MemoryTranslationCache(new TranslationCacheOptions { Ttl = ttl });
        return _loggingWrapper.Wrap<ITranslatorSession>(new CachingTranslatorSession(inner, cache));
    }
}
