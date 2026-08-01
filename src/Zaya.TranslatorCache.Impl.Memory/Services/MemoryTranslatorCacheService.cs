using Zaya.Primitives;
using Zaya.Translator.Services;
using Zaya.TranslatorCache.Impl.Memory.Constants;
using Zaya.TranslatorCache.Services;

namespace Zaya.TranslatorCache.Impl.Memory.Services;

/// <summary>
/// In-memory translation cache engine (exact dictionary + optional TTL).
/// </summary>
public sealed class MemoryTranslatorCacheService : ITranslatorCacheService
{
    /// <summary>Stable engine id used in profiles and plugin discovery.</summary>
    public const string EngineIdValue = "memory-translator-cache";

    private static readonly IReadOnlyList<SettingDescriptor> SettingsList = CacheSettingDescriptors.All;

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

    private static ITranslatorSession Wrap(ITranslatorSession inner, SettingDescriptorList settings)
    {
        if (!settings.GetValueAsBool(CacheSettingsKeys.EnableCache))
            return inner;

        var ttlMinutes = settings.GetValueAsInt(CacheSettingsKeys.CacheTtlMinutes);
        TimeSpan? ttl = ttlMinutes > 0 ? TimeSpan.FromMinutes(ttlMinutes) : null;

        var cache = new MemoryTranslationCache(new TranslationCacheOptions { Ttl = ttl });
        return new CachingTranslatorSession(inner, cache);
    }
}
