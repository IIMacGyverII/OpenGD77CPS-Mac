# OpenGD77 CPS — PriInterPhone fork v1.9.20

**Date:** June 7, 2026

**Build:** `OpenGD77CPS-Mac_Build_20260607_143000.zip`

## Codeplug Studio — pull workflow + validation links

- **After Pull (ADB)** — when channel changes are detected, Studio offers to open **Review diff…** immediately
- **Relay=0 channels** — validation report lists affected channel names with clickable links
- **Integrity scroll** — loading a folder with integrity warnings scrolls to the **Integrity** section (when no diff scroll takes priority)
- Diff review skip logic uses shared `IsDiffReviewCurrent` helper (Studio + F8)

`FORK_VERSION` = **1.9.20**