# Release v1.2.0 — PriInterPhone Fork

**Date:** June 1, 2026  
**Tag:** `v1.2.0`  
**Download:** [GitHub Releases](https://github.com/IIMacGyverII/OpenGD77CPS-Mac/releases/tag/v1.2.0) — `OpenGD77CPS-Mac_Build_20260601_142528.zip`

## Highlights

- **v1.2.0:** Latitude, Longitude, and Use Location import from Android 37-column CSV (Path B).
- **v1.1.0:** Fixed startup crash (CSV fields off `ChannelOne` struct); layout fixes; clear stale CSV arrays on import.
- **v1.0.0:** Relay `0→2` and Outbound Slot 1-based→0-based export for Android firmware compatibility.
- Fork branding, About-dialog warning — **not for stock GD-77**.

## Pair with Android

Use **DMRModHooks v3.4.0+** on the phone for contact type, encryption, Pitfall 12 (DMR ID), and channel mode `3↔4` mapping. This CPS handles the **PC edit** layer only.

## Install

1. Download the zip from Releases (do not build from source unless developing).
2. Extract and run `OpenGD77CPS.exe`.
3. Confirm About shows **fork v1.2.0** and the PriInterPhone warning.
4. Use **File → Import CSV Files…** (not channel-grid Import) for phone backups.

Full changelog: [docs/CODEBASE_DEEP_DIVE.md](CODEBASE_DEEP_DIVE.md). Extended release notes are also in [phonedmrapp `OpenGD77Fork/RELEASE_NOTES_20260601.md`](https://github.com/IIMacGyverII/phonedmrapp/blob/main/OpenGD77Fork/RELEASE_NOTES_20260601.md).