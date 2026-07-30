# Versioning (Zaya.Translator)

| Axis | Source | Example |
|------|--------|---------|
| **ZayaPrimitivesVersion** | `Directory.Build.props` (supplies **Major**) | `1.0.0` |
| **interfaceVersion** | `Zaya.Translator.csproj` → only **`ZayaVersionInterface`** → `Major.Interface.0` | `1.0.0` |
| **pluginVersion** | Each Impl → **`ZayaVersionImpMajor`** + **`ZayaVersionImpMinor`**; Interface read from abstractions → `Major.Interface.ImpMajor.ImpMinor` | `1.0.0.0` |
| **updateChannel** | Interface `MAJOR.Interface` | `1.0` → `plugin-v1.0-latest` |

Rules:

- Abstractions: only `ZayaVersionInterface`. Version always ends with `.0`. Contract/assembly change → bump Interface.
- Plugin: only `ZayaVersionImpMajor` / `ZayaVersionImpMinor`. Interface digit is taken from `Zaya.Translator.csproj` automatically.
- Do not set `<Version>` manually. `Directory.Build.targets` builds it and checks Major vs Primitives.
- Host loads a zip only if `interfaceVersion` **exactly** matches host’s `Zaya.Translator` version.
- Updater uses `plugin-v{updateChannel}-latest` (not Primitives).

## plugin.json

```json
{
  "id": "Google",
  "type": "translator",
  "interface": "Zaya.Translator",
  "interfaceVersion": "1.0.0",
  "pluginVersion": "1.0.0.0"
}
```

Release body lists per-asset plugin versions (`Zaya.Translator.Impl.Google.zip=1.0.0.0`).

## Bumping

1. Interface: raise `ZayaVersionInterface` in `Zaya.Translator.csproj`, update host, republish plugins.
2. Single engine: raise `ZayaVersionImpMajor` / `ZayaVersionImpMinor` only in that Impl’s `.csproj`.
3. Run `build.cmd` / Publish workflow.
