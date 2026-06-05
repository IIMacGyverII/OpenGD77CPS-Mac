# Release v1.2.1 — UI workflow + CSV encoding

**Date:** June 5, 2026  
**Tag:** `v1.2.1`

## Changes

- **CSV encoding:** All Android backup import/export paths use UTF-8 **without BOM**; readers strip BOM on first line (`ReadWriteCsv/CsvEncoding.cs`, Pitfall 16).
- **Toolbar:** **Import Android** / **Export Android** buttons (same as File → Import/Export CSV).
- **Status bar:** Fork version + reminder to use File → Import CSV (Path B).
- **Title bar:** Shows `FORK_NAME` and `FORK_VERSION`.
- **Toolbar:** USB Read/Write hidden (still under Program menu for stock GD-77 use).
- **Docs:** `UI_MODERNIZATION_PLAN.md` corrected (live Path B vs dead `ChannelsCsvImporter` overload).

## Build

```powershell
msbuild OpenGD77CPS.sln /p:Configuration=Release /p:Platform=x86
```

Output: `bin\ReleaseOpenGD77\OpenGD77CPS.exe` — zip with `WeifenLuo.WinFormsUI.Docking.dll` and dependencies for release asset.