# Zaya.Translator

Pluggable translation and translation-cache abstractions for the Zaya ecosystem — engines expose metadata and `SettingDescriptor`s; hosts create translator sessions and optionally wrap them with a cache plugin.

## Packages

| Package | Version | Role |
|---------|---------|------|
| **Zaya.Translator** | 1.1.0 | Abstractions: `ITranslatorService`, `ITranslatorSession` |
| **Zaya.Translator.Impl.Google** | 1.1.0.2 | Unofficial Google Translate (`translate.googleapis.com`) |
| **Zaya.Translator.Impl.Yandex** | 1.1.0.2 | Yandex Cloud API v2 and free browser endpoint |
| **Zaya.TranslatorCache** | 1.0.0 | Abstractions: `ITranslatorCacheService` |
| **Zaya.TranslatorCache.Impl.Memory** | 1.0.0.3 | In-memory exact + TTL cache |

Requires [Zaya.Primitives](https://github.com/shtrasser-dev/Zaya.Primitives) **1.0.0**. Plugin engines also use [Zaya.Logging](https://github.com/shtrasser-dev/Zaya.Logging) **1.0.0**.

Update channels (GitHub Releases):

- Translator engines: [`plugin-Zaya.Translator-v1.1-latest`](https://github.com/shtrasser-dev/Zaya.Translator/releases/tag/plugin-Zaya.Translator-v1.1-latest)
- Cache engine: [`plugin-Zaya.TranslatorCache-v1.0-latest`](https://github.com/shtrasser-dev/Zaya.Translator/releases/tag/plugin-Zaya.TranslatorCache-v1.0-latest)

See [versioning](docs/versioning.md) and [CHANGELOG](CHANGELOG.md).

Docs: [API & articles](https://shtrasser-dev.github.io/Zaya.Translator)

## Features

- **ITranslatorService** — engine id, localized name/description, `Settings`, `CreateSessionAsync`
- **ITranslatorSession** — `TranslateAsync(string)` / batch `TranslateAsync(IReadOnlyList<string>)`
- **ITranslatorCacheService** — wrap a raw session via `WrapSessionAsync` (exact/TTL settings on Memory)
- Failures surface as `LocalizedException` for host UI
- UI strings for engines/cache: `en`, `ru`, `zh-Hans`, `uk`, `de`, `pt`, `ja`, `ko`, `fr`, `tr`, `pl`

There is no separate `InitializeAsync`: create a session with defaults or an explicit settings dictionary.

## Installation

```xml
<PackageReference Include="Zaya.Translator" Version="1.1.0" />
<PackageReference Include="Zaya.Translator.Impl.Google" Version="1.1.0.2" />
<!-- or -->
<PackageReference Include="Zaya.Translator.Impl.Yandex" Version="1.1.0.2" />
<PackageReference Include="Zaya.TranslatorCache" Version="1.0.0" />
<PackageReference Include="Zaya.TranslatorCache.Impl.Memory" Version="1.0.0.3" />
```

Plugin zips for ScreenTranslator hosts (stable names) from the floating tags above:

- `Zaya.Translator.Impl.Google.zip`
- `Zaya.Translator.Impl.Yandex.zip`
- `Zaya.TranslatorCache.Impl.Memory.zip`

## Quick start (Google + Memory cache)

```csharp
using Zaya.Translator.Impl.Google;
using Zaya.TranslatorCache.Impl.Memory;

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
Resolve ITranslatorService (new / DI / Activator on entryPoint)
  → CreateSessionAsync(settings)
Resolve ITranslatorCacheService
  → WrapSessionAsync(rawSession, cacheSettings)
  → TranslateAsync(...)
  → Dispose session / services
```

## Google / Yandex notes

- Both engines accept a hidden `userAgent` setting (default Chrome desktop UA) for HTTP requests.
- Google batch mode wraps segments as `([…])` when the text does not already contain those markers.
- Yandex: `useApiKey` = false → free browser endpoint (unofficial); `useApiKey` = true → Yandex Cloud Translate API v2.
- Yandex exposes a single Chinese code (`zh`); the UI shows one Chinese option (not Simplified/Traditional separately).

## License

MIT — see [LICENSE](LICENSE).
