# Versioning (Zaya.Translator)

Three independent axes — do not bump them together unless required.

| Axis | Source | Example |
|------|--------|---------|
| **primitivesChannel** | `ZayaPrimitivesVersion` → `MAJOR.MINOR` | `0.4` |
| **interfaceVersion** | Version of package **Zaya.Translator** | `0.4.0` |
| **pluginVersion** | Version of each **Impl** (Google / Yandex) | may differ per engine |

Host must ship the same **Zaya.Translator** assembly as `interfaceVersion`. Engine-only fixes: bump only that Impl’s `<Version>`.

Release body lists `Zaya.Translator.Impl.Google.zip=0.4.0` lines for the host updater. Floating tag: `plugin-v{channel}-latest`.
