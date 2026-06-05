# OpenGD77 CPS — PriInterPhone Fork v1.3.4

**Date:** June 5, 2026

## Tier 2.10b — Export && push to phone (ADB)

Round-trip without MTP copy:

- **Android backup manager** → **Export && push (ADB)…**
- **File → Export** offers ADB when `adb.exe` is available
- Exports Path B CSVs from the loaded codeplug, then `adb push` to `/sdcard/Download/DMR/DMR_Backups/{timestamp}/`
- Optional overwrite of an existing phone backup folder
- Reminder to **IMPORT (OpenGD77)** on the phone after push

**Phone step after push:** LOCAL → IMPORT (OpenGD77) → pick the pushed folder.