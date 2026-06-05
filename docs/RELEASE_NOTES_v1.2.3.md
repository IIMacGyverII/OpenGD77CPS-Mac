# OpenGD77 CPS — PriInterPhone fork v1.2.3

**Date:** June 5, 2026

## New

- **Android backup manager** (`File → Android backup manager…`, toolbar **Backup…**) — folder checklist, Import all / Export all (Path B).
- **Toolbar:** **Open folder** (last backup path in Explorer).
- **Channel grid:** **Ct** column (G/P/A contact type badge); **F2** open editor, **Del** delete, **Ctrl+D** duplicate.
- **Health strip:** orphan contact count (`orphan ct: N`).
- **Channel editor:** Android section titled *PriInterPhone / Android (CSV only)*; lat/lon/use-location tooltips.

## Unchanged

- Path B import: `ChannelsForm.ImportFromCsvFile(..., MainForm, ...)`.
- UTF-8 CSV without BOM.