# Versioning (Zaya.Translator)

| Axis | Source | Example |
|------|--------|---------|
| **ZayaPrimitivesVersion** | `Directory.Build.props` (supplies **Major**) | `1.0.0` |
| **translator interfaceVersion** | `Zaya.Translator.csproj` → **`ZayaVersionInterface`** → `Major.Interface.0` | `1.1.0` |
| **cache interfaceVersion** | `Zaya.TranslatorCache.csproj` → **`ZayaVersionInterface`** → `Major.Interface.0` | `1.0.0` |
| **pluginVersion** | Each Impl → **`ZayaVersionImpMajor`** + **`ZayaVersionImpMinor`**; Interface read from that plugin’s abstractions csproj via `ZayaInterfaceCsproj` → `Major.Interface.ImpMajor.ImpMinor` | `1.1.0.1` / `1.0.0.2` |
| **updateChannel** | Per interface `MAJOR.Interface` | Translator `1.1`, Cache `1.0` |

Rules:

- Abstractions: only `ZayaVersionInterface`. Version always ends with `.0`. Contract/assembly change → bump Interface.
- Plugin: only `ZayaVersionImpMajor` / `ZayaVersionImpMinor`. Interface digit is taken from the matching interface csproj (`Zaya.Translator` or `Zaya.TranslatorCache`).
- Do not set `<Version>` manually. `Directory.Build.targets` builds it and checks Major vs Primitives.
- Host loads a zip only if `interfaceVersion` **exactly** matches the host-shipped NuGet for that interface (`Zaya.Translator` or `Zaya.TranslatorCache`).
- **One interface → one floating GitHub tag:** `plugin-{Interface}-v{channel}-latest` (immutable: `plugin-{Interface}-v{pluginVersion}`).
  - `plugin-Zaya.Translator-v1.1-latest` → Google + Yandex
  - `plugin-Zaya.TranslatorCache-v1.0-latest` → Memory
- `build.cmd` writes `out/interfaces.json` describing those groups for the Publish workflow.

## plugin.json

Translator engine:

```json
{
  "id": "Google",
  "type": "translator",
  "interface": "Zaya.Translator",
  "interfaceVersion": "1.1.0",
  "pluginVersion": "1.1.0.1"
}
```

Cache engine:

```json
{
  "id": "Memory",
  "type": "translator-cache",
  "interface": "Zaya.TranslatorCache",
  "interfaceVersion": "1.0.0",
  "pluginVersion": "1.0.0.2"
}
```

Release body lists per-asset plugin versions.

## Bumping

1. Translator interface: raise `ZayaVersionInterface` in `Zaya.Translator.csproj`, update host, republish translator plugins.
2. Cache interface: raise `ZayaVersionInterface` in `Zaya.TranslatorCache.csproj`, update host, republish Memory plugin.
3. Single engine: raise `ZayaVersionImpMajor` / `ZayaVersionImpMinor` only in that Impl’s `.csproj`.
4. Run `build.cmd` / Publish workflow.
