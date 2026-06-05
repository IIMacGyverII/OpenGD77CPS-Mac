# OpenGD77 CPS — PriInterPhone Fork v1.3.3

**Date:** June 5, 2026

## Tier 2.6 — Contact integrity checker

Android backup manager now cross-checks `Channels.csv` against `Contacts.csv` before import:

- Unresolved DMR ID (Pitfall 12)
- `relay=0` rows
- Unknown `channel_mode` values

Warnings appear in the validation panel with an expandable detail list.

## Tier 1.3 — High DPI manifest

`app.manifest` enables Per-Monitor V2 DPI awareness for clearer UI on 125–150% display scaling.