# OpenGD77 CPS — PriInterPhone fork v1.9.21

**Date:** June 7, 2026

**Build:** `OpenGD77CPS-Mac_Build_20260607_144000.zip`

## Codeplug Studio — smarter diff + report scroll

- **No changes = auto-approved** — when CSV matches the loaded codeplug (0 add/change/delete), diff review is skipped; status shows `No channel changes — ready to import`
- **Export scroll** — after export, report scrolls to **Last operation**
- **Validation scroll** — relay=0 / duplicate-name warnings scroll to **Validation** when no higher-priority section applies
- **F8 pull prompt** — Android backup manager (F8) offers Review diff after ADB pull, same as Studio
- Shared `HasPendingDiffChanges` / `OfferReviewAfterPullIfNeeded` helpers in `AndroidImportDiff`

`FORK_VERSION` = **1.9.21**