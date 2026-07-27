using Zaya.Primitives;

namespace Zaya.Translator.Services.Impl;

/// <summary>
/// Factory that conditionally wraps an <see cref="ITranslatorSession"/>
/// with a <see cref="CachingTranslatorSession"/> based on engine settings.
/// Translators call <see cref="TryWrap"/> in their <c>CreateSessionAsync</c>
/// to add caching transparently when enabled by the user.
/// </summary>
public static class CacheFactory
{
    /// <summary>
    /// Wraps the specified session with a <see cref="CachingTranslatorSession"/>
    /// if the <see cref="CacheSettingsKeys.EnableCache"/> setting is <c>true</c>.
    /// </summary>
    /// <param name="session">The inner translator session to wrap.</param>
    /// <param name="settings">
    /// A <see cref="SettingDescriptorList"/> bound to the engine settings dictionary.
    /// Must contain <see cref="CacheSettingsKeys.EnableCache"/> and optionally
    /// <see cref="CacheSettingsKeys.CacheTtlMinutes"/>.
    /// </param>
    /// <returns>
    /// The original <paramref name="session"/> if caching is disabled,
    /// or a new <see cref="CachingTranslatorSession"/> wrapping it otherwise.
    /// </returns>
    public static ITranslatorSession TryWrap(
        ITranslatorSession session, SettingDescriptorList settings)
    {
        if (!settings.GetValueAsBool(CacheSettingsKeys.EnableCache))
            return session;

        var ttlMinutes = settings.GetValueAsInt(CacheSettingsKeys.CacheTtlMinutes);
        TimeSpan? ttl = ttlMinutes > 0 ? TimeSpan.FromMinutes(ttlMinutes) : null;
        var cache = new MemoryTranslationCache(ttl);
        return new CachingTranslatorSession(session, cache);
    }
}
