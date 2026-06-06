# OpenGD77 CPS — PriInterPhone fork v1.5.8

**Date:** June 5, 2026

**Build:** `OpenGD77CPS-Mac_Build_20260605_204430.zip`

## Tier 2.1 — Channel grid badge polish

- **Mode column** — `D` / `A` badges with blue (digital) and amber (analog) backgrounds; column narrowed for scanability.
- **Type column** — `G` / `P` / `A` contact-type badges color-coded (green / orange / gray).
- Toolbar hint updated to mention badge colors.

## Dead code cleanup

- Removed unused `ImportFromCsvFile(..., Form, ...)` overload (was never called; live Path B remains `MainForm` overload).
- Removed `ChannelsCsvImporter.cs` and stock-only `VoteScanForm` (zero external references).

`FORK_VERSION` = **1.5.8**