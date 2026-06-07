# OpenGD77 CPS — PriInterPhone fork v1.9.3

**Date:** June 7, 2026

**Build:** `OpenGD77CPS-Mac_Build_20260607_120333.zip`

## Tier 3.2 — Codeplug Studio (MVP spike)

First slice of the thin CSV-only workflow planned in the UI modernization roadmap:

- **`CodeplugStudioForm`** — streamlined window for the 5-file phone backup (Channels, Contacts, TG_Lists, Zones, DTMF)
- **CSV tiles** — per-file status with row counts at a glance
- **Validation + diff** — reuses Path B validator, WebView2 report, and channel diff preview before import
- **Import / export all** — same Path B batch as Android backup manager (no grid Path A)
- **Open full editor** — jump into the full CPS tree/channel editors when you need more than CSV workflow
- **Launch modes:**
  - **File → Codeplug Studio…** or toolbar **Studio** button
  - **Ctrl+Shift+S** shortcut
  - **`OpenGD77CPS.exe --studio`** — opens Studio first; close without "Open full editor" exits the app

Full OpenGD77 CPS (USB, stock menus) remains under **Advanced** for power users.

`FORK_VERSION` = **1.9.3**