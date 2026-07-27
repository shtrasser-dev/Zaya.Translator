using System.Resources;

namespace Zaya.Translator.Impl.Yandex.Properties;

internal static class Resources
{
    private static readonly ResourceManager _rm =
        new("Zaya.Translator.Impl.Yandex.Properties.Resources", typeof(Resources).Assembly);

    public static ResourceManager ResourceManager => _rm;
}
