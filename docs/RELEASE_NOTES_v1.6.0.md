# OpenGD77 CPS — PriInterPhone fork v1.6.0

**Date:** June 5, 2026

**Build:** `OpenGD77CPS-Mac_Build_20260605_205806.zip`

## WebView2 reports — now actually visible

v1.5.9 hid the HTML report behind a small **Log** tab that loaded only when selected. v1.6.0 fixes that:

- **Status bar health click** (`Ch … | Ct …` center label) opens a **large HTML codeplug report** with metric cards — no more plain MessageBox.
- **Android backup manager** — validation report fills the **lower half** of the window (split layout); no tab hunting. **Raw log…** button for text output.
- **WebView2** initializes as soon as the dialog opens (not when a hidden tab is first clicked).
- **Channels grid** hint spells out badge colors (D=blue, A=amber, G/P/A).

`FORK_VERSION` = **1.6.0**