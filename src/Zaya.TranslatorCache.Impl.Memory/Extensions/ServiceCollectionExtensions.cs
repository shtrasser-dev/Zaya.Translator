using Zaya.TranslatorCache.Impl.Memory.Services;
using Zaya.TranslatorCache.Services;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering the memory translator-cache engine.
/// </summary>
public static class MemoryTranslatorCacheServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="MemoryTranslatorCacheService"/> as a singleton <see cref="ITranslatorCacheService"/>.
    /// </summary>
    public static IServiceCollection AddMemoryTranslatorCache(this IServiceCollection services)
    {
        services.AddSingleton<ITranslatorCacheService, MemoryTranslatorCacheService>();
        return services;
    }
}
