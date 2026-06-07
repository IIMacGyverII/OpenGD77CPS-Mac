# OpenGD77 CPS — PriInterPhone fork v1.9.19

**Date:** June 7, 2026

**Build:** `OpenGD77CPS-Mac_Build_20260607_142000.zip`

## Import safety + validation drill-down

- **Diff stamp enforced on import** — `ImportAndroidBackupFolder` re-checks `Channels.csv` stamp; stale “diff reviewed” approval cannot skip review after a re-pull
- **Duplicate channel names** — validation report lists duplicate CSV names with clickable links to loaded codeplug channels
- **Studio status chip** — shows `Diff reviewed ✓ — ready to import` after approval
- **Footer tooltips** — Import (Ctrl+I), Review diff (Ctrl+D), Export (Ctrl+E), Health (F7)

`FORK_VERSION` = **1.9.19**