using System.Resources;

namespace Zaya.TranslatorCache.Impl.Memory.Properties;

internal static class Resources
{
    private static readonly ResourceManager _rm =
        new("Zaya.TranslatorCache.Impl.Memory.Properties.Resources", typeof(Resources).Assembly);

    public static ResourceManager ResourceManager => _rm;
}
