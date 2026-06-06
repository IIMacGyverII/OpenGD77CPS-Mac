# OpenGD77 CPS — PriInterPhone fork v1.5.9

**Date:** June 5, 2026

**Build:** `OpenGD77CPS-Mac_Build_20260605_205205.zip`

## Tier 3.1 — WebView2 hybrid spike (Android backup)

- **Report tab** in Android backup manager — dark-themed HTML validation report (files, metrics, diff preview, integrity warnings).
- **`ForkWebViewPanel`** — WebView2 host with fallback message + download link when runtime is missing; **Log** tab keeps plain-text output.
- **`AndroidBackupReportHtml`** — builds report from existing validation/diff/integrity results.
- NuGet: `Microsoft.Web.WebView2` 1.0.2903.40; `WebView2Loader.dll` copied to build output.

`FORK_VERSION` = **1.5.9**