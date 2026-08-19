# Zaya.Translator

Pluggable translation and translation-cache abstractions for the Zaya ecosystem.

## Packages

| Package | Version | Role |
|---------|---------|------|
| **Zaya.Translator** | 2.0.0 | Abstractions: `ITranslatorService`, `ITranslatorSession` |
| **Zaya.Translator.Impl.Google** | 2.0.0.0 | Unofficial Google Translate |
| **Zaya.Translator.Impl.Yandex** | 2.0.0.0 | Yandex Cloud API v2 and free browser endpoint |
| **Zaya.TranslatorCache** | 2.0.0 | Abstractions: `ITranslatorCacheService` |
| **Zaya.TranslatorCache.Impl.Memory** | 2.0.0.0 | In-memory exact + TTL |

Requires [Zaya.Primitives](https://github.com/shtrasser-dev/Zaya.Primitives) **2.0.0**. Plugin engines also use [Zaya.Logging](https://github.com/shtrasser-dev/Zaya.Logging) **1.0.0**. See [versioning](versioning.md).

Floating release tags: `plugin-Zaya.Translator-v2.0-latest`, `plugin-Zaya.TranslatorCache-v2.0-latest`.

## Features

- **ITranslatorService** — engine metadata + `CreateSessionAsync`
- **ITranslatorSession** — single and batch `TranslateAsync`
- **ITranslatorCacheService** — `WrapSessionAsync`
- Failures surface as `LocalizedException`

## Installation

```xml
<PackageReference Include="Zaya.Translator" Version="2.0.0" />
<PackageReference Include="Zaya.TranslatorCache" Version="2.0.0" />
```

## Quick Start

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
```

## Next steps

- [Getting started](articles/getting-started.md)
- [Translator cache](articles/translator-cache.md)
- [Versioning](versioning.md)
- [API](api/index.md)
