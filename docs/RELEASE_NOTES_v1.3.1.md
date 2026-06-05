# OpenGD77 CPS — PriInterPhone Fork v1.3.1

**Date:** June 5, 2026

## Fix — phone USB / MTP folder picker

Windows `FolderBrowserDialog` cannot open backup folders still on the phone (`This PC → Armor 26 Ultra → Internal shared storage`). Users saw *"The folder can't be used"* when picking `DMR_Backups\YYYYMMDD_HHmmss` over USB.

- Browse dialog now starts on **Desktop** and explains **copy the folder to your PC first**.
- **Android backup manager** accepts a **pasted local path** (e.g. `C:\Users\You\Desktop\20260314_131206`).
- Help link and clearer errors when a phone path is detected.
- Welcome dialog step added: copy before import.

**Workflow:** File Explorer → copy dated folder from phone → paste on PC → browse/paste that PC path in CPS.