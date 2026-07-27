# Zaya.Translator

Pluggable translation abstractions for the Zaya ecosystem — engines expose metadata and `SettingDescriptor`s, hosts pass settings into `CreateSessionAsync`.

## Packages

| Package | Version | Role |
|---------|---------|------|
| **Zaya.Translator** | 0.1.0 | Abstractions: `ITranslatorService`, `ITranslatorSession`, translation cache |
| **Zaya.Translator.Impl.Google** | 0.1.0 | Unofficial Google Translate |
| **Zaya.Translator.Impl.Yandex** | 0.1.0 | Yandex Cloud API v2 and free browser endpoint |

## Features

- **ITranslatorService** — engine metadata + `CreateSessionAsync`
- **ITranslatorSession** — single and batch `TranslateAsync`
- Optional cache via `enableCache` / `cacheTtlMinutes`
- Failures surface as `LocalizedException`

There is no separate `InitializeAsync`: create a session and translate.

## Installation

```xml
<PackageReference Include="Zaya.Translator" Version="0.1.0" />
<PackageReference Include="Zaya.Translator.Impl.Google" Version="0.1.0" />
```

## Quick Start

```csharp
using Zaya.Translator.Impl.Google.Services;

using var translator = new GoogleTranslatorService();
using var session = await translator.CreateSessionAsync(new Dictionary<string, object>
{
    ["autoDetectLanguage"] = true,
    ["targetLanguage"] = "ru",
});

var text = await session.TranslateAsync("Hello, world!");
Console.WriteLine(text);
```

## Next Steps

- **[Getting Started](articles/getting-started.md)** — engines, settings, cache
- **[API Reference](xref:Zaya.Translator.Services)** — generated from source
