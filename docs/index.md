# Zaya.Translator

Pluggable translation and translation-cache abstractions for the Zaya ecosystem.

## Packages

| Package | Version | Role |
|---------|---------|------|
| **Zaya.Translator** | 1.1.0 | Abstractions: `ITranslatorService`, `ITranslatorSession` |
| **Zaya.Translator.Impl.Google** | 1.1.0.0 | Unofficial Google Translate |
| **Zaya.Translator.Impl.Yandex** | 1.1.0.0 | Yandex Cloud API v2 and free browser endpoint |
| **Zaya.TranslatorCache** | 1.0.0 | Abstractions: `ITranslatorCacheService` |
| **Zaya.TranslatorCache.Impl.Memory** | 1.0.0.0 | In-memory exact + TTL + fuzzy recent |

Requires [Zaya.Primitives](https://github.com/shtrasser-dev/Zaya.Primitives) **1.0.0**. See [versioning](versioning.md).

## Features

- **ITranslatorService** — engine metadata + `CreateSessionAsync`
- **ITranslatorSession** — single and batch `TranslateAsync`
- **ITranslatorCacheService** — `WrapSessionAsync` for optional caching
- Failures surface as `LocalizedException`

## Installation

```xml
<PackageReference Include="Zaya.Translator" Version="1.1.0" />
<PackageReference Include="Zaya.TranslatorCache" Version="1.0.0" />
```

## Quick Start

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
using var session = await cache.WrapSessionAsync(raw);

var text = await session.TranslateAsync("Hello, world!");
Console.WriteLine(text);
```

## Next Steps

- **[Getting Started](articles/getting-started.md)** — translator engines
- **[Translator Cache](articles/translator-cache.md)** — cache engines and settings
- **[API Reference](xref:Zaya.Translator.Services)** — generated from source
