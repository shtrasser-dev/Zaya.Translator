# Zaya.Translator

Pluggable translation and translation-cache abstractions for the Zaya ecosystem — engines expose metadata and `SettingDescriptor`s; hosts create translator sessions and optionally wrap them with a cache plugin.

## Packages

| Package | Version | Role |
|---------|---------|------|
| **Zaya.Translator** | 1.1.0 | Abstractions: `ITranslatorService`, `ITranslatorSession` |
| **Zaya.Translator.Impl.Google** | 1.1.0.0 | Unofficial Google Translate (`translate.googleapis.com`) |
| **Zaya.Translator.Impl.Yandex** | 1.1.0.0 | Yandex Cloud API v2 and free browser endpoint |
| **Zaya.TranslatorCache** | 1.0.0 | Abstractions: `ITranslatorCacheService` |
| **Zaya.TranslatorCache.Impl.Memory** | 1.0.0.0 | In-memory exact + TTL + fuzzy recent cache |

Requires [Zaya.Primitives](https://github.com/shtrasser-dev/Zaya.Primitives) **1.0.0**. Update channel: `plugin-v1.1-latest`. See [versioning](docs/versioning.md).

Docs: [API & articles](https://shtrasser-dev.github.io/Zaya.Translator)

## Features

- **ITranslatorService** — engine id, localized name/description, `Settings`, `CreateSessionAsync`
- **ITranslatorSession** — `TranslateAsync(string)` / batch `TranslateAsync(IReadOnlyList<string>)`
- **ITranslatorCacheService** — wrap a raw session via `WrapSessionAsync` (exact/TTL/fuzzy settings on Memory)
- Failures surface as `LocalizedException` for host UI

There is no separate `InitializeAsync`: create a session with defaults or an explicit settings dictionary.

## Installation

```xml
<PackageReference Include="Zaya.Translator" Version="1.1.0" />
<PackageReference Include="Zaya.Translator.Impl.Google" Version="1.1.0.0" />
<!-- or -->
<PackageReference Include="Zaya.Translator.Impl.Yandex" Version="1.1.0.0" />
<PackageReference Include="Zaya.TranslatorCache" Version="1.0.0" />
<PackageReference Include="Zaya.TranslatorCache.Impl.Memory" Version="1.0.0.0" />
```

Plugin zips for ScreenTranslator hosts (stable names) from GitHub Releases (`plugin-v1.1-latest`):

- `Zaya.Translator.Impl.Google.zip`
- `Zaya.Translator.Impl.Yandex.zip`
- `Zaya.TranslatorCache.Impl.Memory.zip`

## Quick start (Google + Memory cache)

```csharp
using Zaya.Translator.Impl.Google.Services;
using Zaya.TranslatorCache.Impl.Memory.Services;

using var translator = new GoogleTranslatorService();
using var cache = new MemoryTranslatorCacheService();

using var raw = await translator.CreateSessionAsync(new Dictionary<string, object>
{
    ["autoDetectLanguage"] = true,
    ["targetLanguage"] = "ru",
});
using var session = await cache.WrapSessionAsync(raw, new Dictionary<string, object>
{
    ["enableCache"] = true,
    ["enableFuzzyRecent"] = true,
});

var text = await session.TranslateAsync("Hello, world!");
Console.WriteLine(text);
```

Or DI:

```csharp
services.AddGoogleTranslator();
// services.AddYandexTranslator();
services.AddMemoryTranslatorCache();
```

## Engine lifecycle

```
Resolve ITranslatorService (new / DI / plugin host)
  → CreateSessionAsync(settings)
Resolve ITranslatorCacheService
  → WrapSessionAsync(rawSession, cacheSettings)
  → TranslateAsync(...)
  → Dispose session / services
```

## Yandex notes

- `useApiKey` = false → free browser endpoint (unofficial)
- `useApiKey` = true → Yandex Cloud Translate API v2 (requires API key)
- Yandex exposes a single Chinese code (`zh`); the UI shows one Chinese option (not Simplified/Traditional separately)

## License

MIT — see [LICENSE](LICENSE).
