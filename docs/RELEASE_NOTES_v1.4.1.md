# OpenGD77 CPS — PriInterPhone fork v1.4.1

**Date:** June 5, 2026

## Build fix — dark dock tabs (v1.4.0)

v1.4.0 assigned `dockPanel.Skin`, which is read-only in DockPanel Suite 3.x. v1.4.1 applies the fork palette via `VS2005Theme` + `Theme.ApplyDarkDockPanelSkin(theme.Skin)` and `dockPanel.Theme`.

Includes all v1.4.0 features: grid bulk edit, column visibility, dark dock tabs.

`FORK_VERSION` = **1.4.1**