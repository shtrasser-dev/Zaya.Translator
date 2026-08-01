# Getting Started

## Overview

Zaya.Translator provides pluggable translation engines. Hosts discover `ITranslatorService` implementations, bind `SettingDescriptor`s, then call `CreateSessionAsync` and `TranslateAsync`. Optional caching is a separate interface in the same repo — see [Translator Cache](translator-cache.md).

## Google Translate

```csharp
using Zaya.Translator.Impl.Google.Services;

using var service = new GoogleTranslatorService();
using var session = await service.CreateSessionAsync(new Dictionary<string, object>
{
    ["autoDetectLanguage"] = true,
    ["targetLanguage"] = "ru",
});

var translated = await session.TranslateAsync("Good morning");
```

Uses the unofficial `translate.googleapis.com` endpoint. Chinese maps to `zh-CN` / `zh-TW`.

## Yandex Translate

```csharp
using Zaya.Translator.Impl.Yandex.Services;

using var service = new YandexTranslatorService();
using var session = await service.CreateSessionAsync(new Dictionary<string, object>
{
    ["autoDetectLanguage"] = false,
    ["sourceLanguage"] = "en",
    ["targetLanguage"] = "ru",
    ["useApiKey"] = true,
    ["apiKey"] = Environment.GetEnvironmentVariable("YANDEX_TRANSLATE_API_KEY")!,
});
```

- `useApiKey` false → free browser endpoint
- `useApiKey` true → Cloud API v2
- Language list uses a single Chinese option (`zh`); Simplified/Traditional are not distinguished by the Yandex API

## Caching

Use `ITranslatorCacheService` / Memory plugin to wrap sessions. See [Translator Cache](translator-cache.md).

## DI

```csharp
services.AddGoogleTranslator();
// or
services.AddYandexTranslator();
services.AddMemoryTranslatorCache();
```

Register one engine as `ITranslatorService`, or resolve concrete types when hosting multiple engines.

## Next steps

- **[Translator Cache](translator-cache.md)** — wrap sessions with caching
- **[API Reference](xref:Zaya.Translator.Services)** — generated from source
