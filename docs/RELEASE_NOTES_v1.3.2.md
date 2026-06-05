# OpenGD77 CPS — PriInterPhone Fork v1.3.2

**Date:** June 5, 2026

## Tier 2.10 — Pull backup from phone (ADB)

Skip MTP folder browsing: list `Download/DMR/DMR_Backups/` on the phone and `adb pull` to a local folder.

- **Android backup manager** → **Pull from phone (ADB)…**
- **File → Import** offers ADB when `adb.exe` is on PATH (or set via **ADB path…**)
- Pulled backups land in `%LocalAppData%\PriInterPhoneCPS\adb_pull\{timestamp}\`
- Folder picker remains the fallback when adb is unavailable

**Phone setup:** USB debugging enabled, authorize PC when prompted.