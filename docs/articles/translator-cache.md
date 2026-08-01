# Getting Started — Translator Cache

Zaya.TranslatorCache (same repo as Zaya.Translator) provides pluggable cache engines. Hosts discover `ITranslatorCacheService` implementations, bind `SettingDescriptor`s, then call `WrapSessionAsync` on a raw translator session.

## Memory cache

```csharp
using Zaya.TranslatorCache.Impl.Memory.Services;

using var cache = new MemoryTranslatorCacheService();
using var session = await cache.WrapSessionAsync(rawSession, new Dictionary<string, object>
{
    ["enableCache"] = true,
    ["cacheTtlMinutes"] = 0,
    ["enableFuzzyRecent"] = true,
    ["recentWindowSize"] = 10,
    ["minFuzzyLength"] = 20,
    ["levenshteinThreshold"] = 8,
});
```

## Settings

| Key | Default | Notes |
|-----|---------|--------|
| `enableCache` | `true` | When false, `WrapSessionAsync` returns the inner session |
| `cacheTtlMinutes` | `0` | `0` = no TTL eviction |
| `enableFuzzyRecent` | `true` | Stabilize OCR flicker via recent fuzzy match |
| `recentWindowSize` | `10` | Recent sources kept for fuzzy matching |
| `minFuzzyLength` | `20` | Shorter texts use exact cache only |
| `levenshteinThreshold` | `8` | Max edit distance as % of longer length |

## DI

```csharp
services.AddMemoryTranslatorCache();
```

## Next steps

- **[API Reference](xref:Zaya.TranslatorCache.Services)** — generated from source
