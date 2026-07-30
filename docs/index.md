# Zaya.Translator

Pluggable translation abstractions for the Zaya ecosystem — engines expose metadata and `SettingDescriptor`s, hosts pass settings into `CreateSessionAsync`.

## Packages

| Package | Version | Role |
|---------|---------|------|
| **Zaya.Translator** | 1.0.0 | Abstractions: `ITranslatorService`, `ITranslatorSession`, translation cache |
| **Zaya.Translator.Impl.Google** | 1.0.0.0 | Unofficial Google Translate |
| **Zaya.Translator.Impl.Yandex** | 1.0.0.0 | Yandex Cloud API v2 and free browser endpoint |

Requires [Zaya.Primitives](https://github.com/shtrasser-dev/Zaya.Primitives) **1.0.0**. See [versioning](versioning.md).

## Features

- **ITranslatorService** — engine metadata + `CreateSessionAsync`
- **ITranslatorSession** — single and batch `TranslateAsync`
- Optional cache via `enableCache` / `cacheTtlMinutes`
- Failures surface as `LocalizedException`

There is no separate `InitializeAsync`: create a session and translate.

## Installation

```xml
<PackageReference Include="Zaya.Translator" Version="1.0.0" />
<PackageReference Include="Zaya.Translator.Impl.Google" Version="1.0.0.0" />
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
