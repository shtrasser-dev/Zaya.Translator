# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
History starts at the current release line; older releases are not backfilled.

## [Unreleased]

### Changed

- **Zaya.Primitives `2.0.0`:** settings types moved to `Zaya.Primitives.Settings`; versions reset to `2.0.0` / `2.0.0.0`.

## [1.2.0.0] - 2026-08-15

### Added

- **Google / Yandex `1.2.0.0`:** constructors take `ILoggingWrapper`; sessions via `Wrap`.
- **Memory `1.1.0.0`:** constructors take `ILoggingWrapper`; wrapped sessions use `Wrap`.
- **`plugin.json`:** `entryPoint` — `Zaya.Translator.Impl.Google.GoogleTranslatorService`, `…Yandex.YandexTranslatorService`, `…Memory.MemoryTranslatorCacheService`.

### Changed

- Plugin engines depend on published `Zaya.Logging` **1.0.0** instead of a sibling project reference.
- Settings descriptors extracted to `SettingsDescriptorsConstants`; engine ids to `EngineConstants`.
- **Memory:** `CacheSettingsKeys` / `CacheSettingDescriptors` replaced by internal `SettingsConstants` / `SettingsDescriptorsConstants` (same layout as Google/Yandex).
- **Google / Yandex:** exceptions moved to `Exceptions/` (one class per file); namespaces `…Exceptions`.
- **Impl layout (Screenshot-style):** public engine service at project root; sessions and helpers under `Services/Impl/`; interfaces under `Services/`.
- Safer plugin satellite copy (plugin `.resources.dll` only).
- GitHub Release notes are taken from this file’s `[Unreleased]` section.
