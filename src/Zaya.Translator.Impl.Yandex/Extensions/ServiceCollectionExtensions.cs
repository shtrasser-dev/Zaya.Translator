using Zaya.Translator.Impl.Yandex.Services;
using Zaya.Translator.Services;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering YandexTranslator services in a DI container.
/// </summary>
public static class YandexTranslatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="YandexTranslatorService"/> as a singleton <see cref="ITranslatorService"/>.
    /// </summary>
    public static IServiceCollection AddYandexTranslator(this IServiceCollection services)
    {
        services.AddSingleton<ITranslatorService, YandexTranslatorService>();
        return services;
    }
}
