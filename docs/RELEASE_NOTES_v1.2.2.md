# OpenGD77 CPS — PriInterPhone fork v1.2.2

**Date:** June 5, 2026  
**Build:** Increment `FORK_VERSION` in `DMR/AboutForm.cs` before shipping a zip to `phonedmrapp/OpenGD77Fork/`.

## UI (Tier 1)

- **Welcome dialog** (`AndroidWorkflowForm`) on first run — Path B vs grid import, links to fork releases and phonedmrapp.
- **Dark theme** (`Theme.cs`) — menu, toolbar, status bar, DockPanel skin aligned with DMRModHooks navy palette.
- **Advanced menu** — USB Read/Write, firmware loader, calibration, and stock OpenGD77 extras moved off the top bar; Android import/export stay on **File** and the toolbar.
- **Codeplug health** — status strip shows channel/contact counts and `relay=0` warning count.
- **Channel list labels** — grid Import/Export buttons marked as *not Android* (Path A); phone backups use **File → Import Android backup folder** (Path B).
- **UTF-8 no BOM** — unchanged from v1.2.1 (`CsvEncoding` on all Android CSV paths).

## Unchanged

- Live Path B import remains `ChannelsForm.ImportFromCsvFile(..., MainForm, ...)` (~line 621).
- Do not use the `Form` overload / `ChannelsCsvImporter` alone for production Android imports.

## Upgrade

Replace the CPS zip from [OpenGD77CPS-Mac releases](https://github.com/IIMacGyverII/OpenGD77CPS-Mac/releases) or build `Release|x86` (`ReleaseOpenGD77` output folder).