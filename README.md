# OpenGD77 CPS — PriInterPhone / DMRModHooks Fork

**Windows Customer Programming Software** for editing codeplugs exported from the [PriInterPhone / DMRModHooks](https://github.com/IIMacGyverII/phonedmrapp) Android app (Ulefone Armor 26 Ultra, Comjot C-26).

> **Warning:** This is **not** stock OpenGD77 CPS. Do **not** use it to program a Radioddity/Baofeng GD-77 — Android-specific CSV fixes can corrupt a real GD-77 codeplug. The About dialog shows fork version **v1.2.0** and a red warning block.

## Download (recommended)

| | |
|---|---|
| **Latest release** | [**v1.2.1**](https://github.com/IIMacGyverII/OpenGD77CPS-Mac/releases/latest) (or [v1.2.0](https://github.com/IIMacGyverII/OpenGD77CPS-Mac/releases/tag/v1.2.0) zip until v1.2.1 asset is published) |
| **Asset** | Build from source for v1.2.1; previous binary: `OpenGD77CPS-Mac_Build_20260601_142528.zip` (v1.2.0) |
| **Requires** | Windows 7+, [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) |
| **Phone module** | [DMRModHooks v3.4.0+](https://github.com/IIMacGyverII/phonedmrapp/releases) (LSPosed) for full import fixes on device |

No installer — extract the zip and run `OpenGD77CPS.exe`.

## What this fork does

- Edits **37-column Android CSV** backups (Channels, Contacts, TG_Lists, Zones, DTMF).
- Fixes **PC-side** round-trip issues: relay field, outbound slot, lat/lon/use-location (v1.1–v1.2).
- Workflow: **Export on phone → edit on PC → import on phone** (no USB to the radio required for this path).

Standard OpenGD77 CPS targets handheld GD-77 radios over USB. This fork targets the **PriInterPhone SQLite/CSV pipeline** used by DMRModHooks.

## Quick start

### 1. Export from the phone

1. Open PriInterPhone → **LOCAL**.
2. Tap **EXPORT (OpenGD77)**.
3. Folder: `/sdcard/Download/DMR_Backups/YYYYMMDD_HHmmss/` (five CSV files).
4. Copy the folder to your PC (USB, cloud, or `adb pull`).

### 2. Import into this CPS (important)

Use **File → Import CSV Files…** (batch menu import — **Path B**).

Do **not** rely on per-channel **Import** buttons on the channel grid (**Path A** rejects the 37-column Android header).

### 3. Edit and export

1. Edit channels, contacts, zones, etc.
2. **File → Export CSV Files…** (Android format).
3. Copy the folder back to the phone and **IMPORT (OpenGD77)** from LOCAL.

### Import order on the phone

Contacts → TG_Lists → Channels → Zones → DTMF (enforced by `DirectDatabaseImporter` on Android).

## Version history (fork)

| Version | Summary |
|---------|---------|
| **1.2.0** | Lat/lon/Use Location from Android CSV; Path B cols 18–36 |
| **1.1.0** | Crash fix (CSV fields off `ChannelOne`); UI layout; clear static arrays on import |
| **1.0.0** | Relay + outbound slot export fixes; fork branding |

Details: [docs/RELEASE_NOTES_v1.2.0.md](docs/RELEASE_NOTES_v1.2.0.md)

## Documentation

| Doc | Description |
|-----|-------------|
| [docs/CODEBASE_DEEP_DIVE.md](docs/CODEBASE_DEEP_DIVE.md) | Import paths A/B/C, column map, pitfalls |
| [phonedmrapp OpenGD77Fork notes](https://github.com/IIMacGyverII/phonedmrapp/tree/main/OpenGD77Fork) | Release zips mirrored in main Android repo |

## Build from source

Prerequisites: Visual Studio 2019+ or Build Tools, **.NET Framework 4.8**, **x86** platform.

```powershell
cd OpenGD77CPS-Mac
# First-time only (WebView2, v1.5.9+):
tools\nuget.exe install Microsoft.Web.WebView2 -Version 1.0.2903.40 -OutputDirectory packages
msbuild OpenGD77CPS.sln /p:Configuration=Release /p:Platform=x86
# Output: bin\ReleaseOpenGD77\OpenGD77CPS.exe (includes runtimes\win-x86\native\WebView2Loader.dll)
```

Bump `FORK_VERSION` in `DMR/AboutForm.cs` before every release build.

## Repository layout

- `DMR/` — Main CPS forms (`MainForm`, `ChannelForm`, `ChannelsForm`, …)
- `docs/` — Maintainer documentation (tracked in git)
- `bin/`, `obj/` — Build output (gitignored)

Folders **not** in this repository (gitignored if present locally):

- `.github/` — Copied maintainer instructions from other repos
- `.claude/` — Editor/agent config
- `.docs/` — AI session summaries

Use this repo’s `docs/` (no leading dot) for project documentation.

## Related projects

- **Android mod:** https://github.com/IIMacGyverII/phonedmrapp  
- **Upstream OpenGD77:** https://www.opengd77.com  

## Support

- **Fork / CPS issues:** [Open an issue](https://github.com/IIMacGyverII/OpenGD77CPS-Mac/issues) in this repository  
- **Phone import, LSPosed, hardware:** [phonedmrapp issues](https://github.com/IIMacGyverII/phonedmrapp/issues)

## License

Inherits license terms from the original OpenGD77 CPS project. See `LICENSE` if present in tree.

## Credits

- OpenGD77: Roger Clark (VK3KYY) and contributors  
- PriInterPhone fork: [@IIMacGyverII](https://github.com/IIMacGyverII) — CSV/Android compatibility patches  

---

**Fork version:** 1.2.1  
**Last updated:** June 5, 2026