# OpenGD77 CPS — PriInterPhone fork v1.2.7

**Date:** June 5, 2026

## Fixes

- **File menu** — New, Save, Open, Android import/export, and Exit appear under top-level **File**, not **Setting**.
- **Shell layout** — Menu, toolbar, and status bar dock to the window top/bottom after the tree moves into the dock panel (removes stale 234px offset).
- **Menu item parenting** — `EnsureForkMainMenu()` re-attaches file actions if WinForms moved shared `ToolStripItem` instances; Read/Write no longer registered on unused **Program** menu before **Advanced**.

## i18n

- `English.xml` — Android import/export menu strings and **Advanced** top-level label.

## Version

`FORK_VERSION` = **1.2.7**

**Package (phonedmrapp):** `OpenGD77CPS-Mac_Build_20260605_154314.zip`