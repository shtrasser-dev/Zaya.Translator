#nullable enable
namespace Zaya.Translator.Properties;

/// <summary>
/// Strongly-typed resource accessor for <c>Zaya.Translator</c> localized strings.
/// Wraps a <see cref="System.Resources.ResourceManager"/> for the embedded
/// <c>Properties/Resources.resx</c> file.
/// </summary>
internal sealed class Resources
{
    private readonly System.Resources.ResourceManager _rm = new(
        "Zaya.Translator.Properties.Resources",
        typeof(Resources).Assembly);

    /// <summary>
    /// Gets the singleton instance of this resource accessor.
    /// </summary>
    internal static Resources Instance { get; } = new();

    /// <summary>
    /// Returns the localized string for the specified resource name and culture.
    /// Falls back to <c>"#{name}#"</c> when the resource is not found.
    /// </summary>
    /// <param name="name">The resource key (e.g. "Cache_EnableCache").</param>
    /// <param name="culture">The target culture; uses invariant culture when null.</param>
    /// <returns>The localized string, or <c>"#{name}#"</c> as a fallback.</returns>
    internal string GetString(string name, System.Globalization.CultureInfo? culture = null)
        => _rm.GetString(name, culture) ?? $"#{name}#";
}
