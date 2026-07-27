# Zaya.Translator

Pluggable translation abstractions for the Zaya ecosystem — engines expose metadata and `SettingDescriptor`s, hosts pass settings into `CreateSessionAsync`.

## Packages

| Package | Version | Role |
|---------|---------|------|
| **Zaya.Translator** | 0.1.0 | Abstractions: `ITranslatorService`, `ITranslatorSession`, translation cache |
| **Zaya.Translator.Impl.Google** | 0.1.0 | Unofficial Google Translate (`translate.googleapis.com`) |
| **Zaya.Translator.Impl.Yandex** | 0.1.0 | Yandex Cloud API v2 and free browser endpoint |

Docs: [API & articles](https://shtrasser-dev.github.io/Zaya.Translator)

## Features

- **ITranslatorService** — engine id, localized name/description, `Settings`, `CreateSessionAsync`
- **ITranslatorSession** — `TranslateAsync(string)` / batch `TranslateAsync(IReadOnlyList<string>)`
- Optional in-memory cache via shared cache settings (`enableCache`, `cacheTtlMinutes`)
- Failures surface as `LocalizedException` for host UI

There is no separate `InitializeAsync`: create a session with defaults or an explicit settings dictionary.

## Installation

```xml
<PackageReference Include="Zaya.Translator" Version="0.1.0" />
<PackageReference Include="Zaya.Translator.Impl.Google" Version="0.1.0" />
<!-- or -->
<PackageReference Include="Zaya.Translator.Impl.Yandex" Version="0.1.0" />
```

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
