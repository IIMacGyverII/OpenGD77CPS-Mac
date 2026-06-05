# OpenGD77 CPS — PriInterPhone fork v1.4.3

**Date:** June 5, 2026

## Fix — channel grid double-click

- Double-click **any cell** in the Channels grid (not only the row-number column) opens/switches the channel editor.
- **F2** unchanged.
- Resolves `MainForm` when `MdiParent` is null (dock/MDI edge case).
- Refreshes channel tree if needed before opening; direct fallback loads editor by data index.

`FORK_VERSION` = **1.4.3**