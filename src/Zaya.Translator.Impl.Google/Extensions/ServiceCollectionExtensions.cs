using Zaya.Translator.Impl.Google.Services;
using Zaya.Translator.Services;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering GoogleTranslator services in a DI container.
/// </summary>
public static class GoogleTranslatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="GoogleTranslatorService"/> as a singleton <see cref="ITranslatorService"/>.
    /// </summary>
    public static IServiceCollection AddGoogleTranslator(this IServiceCollection services)
    {
        services.AddSingleton<ITranslatorService, GoogleTranslatorService>();
        return services;
    }
}
