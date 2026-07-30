# Zaya.Translator

Pluggable translation abstractions for the Zaya ecosystem — engines expose metadata and `SettingDescriptor`s, hosts pass settings into `CreateSessionAsync`.

## Packages

| Package | Version | Role |
|---------|---------|------|
| **Zaya.Translator** | 1.0.0 | Abstractions: `ITranslatorService`, `ITranslatorSession`, translation cache |
| **Zaya.Translator.Impl.Google** | 1.0.0.0 | Unofficial Google Translate (`translate.googleapis.com`) |
| **Zaya.Translator.Impl.Yandex** | 1.0.0.0 | Yandex Cloud API v2 and free browser endpoint |

Requires [Zaya.Primitives](https://github.com/shtrasser-dev/Zaya.Primitives) **1.0.0**. Update channel: `plugin-v1.0-latest`. See [versioning](docs/versioning.md).

Docs: [API & articles](https://shtrasser-dev.github.io/Zaya.Translator)

## Features

- **ITranslatorService** — engine id, localized name/description, `Settings`, `CreateSessionAsync`
- **ITranslatorSession** — `TranslateAsync(string)` / batch `TranslateAsync(IReadOnlyList<string>)`
- Optional in-memory cache via shared cache settings (`enableCache`, `cacheTtlMinutes`)
- Failures surface as `LocalizedException` for host UI

There is no separate `InitializeAsync`: create a session with defaults or an explicit settings dictionary.

## Installation

```xml
<PackageReference Include="Zaya.Translator" Version="1.0.0" />
<PackageReference Include="Zaya.Translator.Impl.Google" Version="1.0.0.0" />
<!-- or -->
<PackageReference Include="Zaya.Translator.Impl.Yandex" Version="1.0.0.0" />
```

Plugin zips for ScreenTranslator hosts (stable names) from GitHub Releases (`plugin-v1.0-latest`):

- `Zaya.Translator.Impl.Google.zip`
- `Zaya.Translator.Impl.Yandex.zip`

## Quick start (Google)

```csharp
using Zaya.Translator.Impl.Google.Services;

using var translator = new GoogleTranslatorService();

using var session = await translator.CreateSessionAsync(new Dictionary<string, object>
{
    ["autoDetectLanguage"] = true,
    ["targetLanguage"] = "ru",
    ["enableCache"] = true,
    ["cacheTtlMinutes"] = 60,
});

var text = await session.TranslateAsync("Hello, world!");
Console.WriteLine(text);
```

Or DI:

```csharp
services.AddGoogleTranslator();
// services.AddYandexTranslator();
```

## Engine lifecycle

```
Resolve ITranslatorService (new / DI / plugin host)
  → Read DisplayName / Description / Settings
  → CreateSessionAsync(settings)
  → TranslateAsync(...)
  → Dispose session / service
```

## Yandex notes

- `useApiKey` = false → free browser endpoint (unofficial)
- `useApiKey` = true → Yandex Cloud Translate API v2 (requires API key)
- Yandex exposes a single Chinese code (`zh`); the UI shows one Chinese option (not Simplified/Traditional separately)

## License

MIT
