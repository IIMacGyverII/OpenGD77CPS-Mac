# OpenGD77 CPS — PriInterPhone Fork v1.3.0

**Date:** June 5, 2026

## Tier 2.5 — Pre-import channel diff preview

Before Path B import applies changes, the fork shows a **channel import preview** dialog:

- **Added** — channel in CSV, not in loaded codeplug
- **Changed** — same name, field-level diff (frequencies, contact, TG, relay, lat/lon, etc.)
- **Deleted** — in codeplug but missing from CSV (import uses `clearFirst` and replaces all channels)
- **Apply import** / **Cancel** — cancel leaves the in-memory codeplug untouched

The Android backup manager shows diff counts in its validation panel when `Channels.csv` is present.

## Files

- `DMR/AndroidImportDiff.cs` — CSV vs codeplug comparison
- `DMR/AndroidImportDiffForm.cs` — preview grid dialog
- `DMR/MainForm.cs` — import flow uses preview when `Channels.csv` exists
- `DMR/AndroidBackupForm.cs` — validation text includes diff summary