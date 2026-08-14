# Getting Started — Translator Cache

Zaya.TranslatorCache (same repo as Zaya.Translator) provides pluggable cache engines. Hosts discover `ITranslatorCacheService` implementations, bind `SettingDescriptor`s, then call `WrapSessionAsync` on a raw translator session.

## Memory cache

```csharp
using Zaya.TranslatorCache.Impl.Memory;

using var cache = new MemoryTranslatorCacheService();
using var session = await cache.WrapSessionAsync(rawSession, new Dictionary<string, object>
{
    ["enableCache"] = true,
    ["cacheTtlMinutes"] = 0,
});
```

## Settings

| Key | Default | Notes |
|-----|---------|--------|
| `enableCache` | `true` | When false, `WrapSessionAsync` returns the inner session |
| `cacheTtlMinutes` | `0` | `0` = no TTL eviction |

## DI

```csharp
services.AddMemoryTranslatorCache();
```

## Next steps

- [Versioning](../versioning.md)
