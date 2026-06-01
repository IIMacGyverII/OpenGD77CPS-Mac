# OpenGD77 CPS Fork — Codebase Deep Dive

**Author**: AI-generated during DMRModHooks dev session, June 2026  
**Fork**: IIMacGyverII/OpenGD77CPS-Mac (patched for PriInterPhone / DMRModHooks Android app)  
**Build target**: .NET Framework 4 / C# 4 (via `C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe`)  
**⚠️ Not compatible with real Radioddity GD-77 — this fork patches things that would corrupt a stock GD-77 codeplug**

---

## Table of Contents

1. [Project Layout](#1-project-layout)
2. [Data Model — ChannelOne Struct](#2-data-model--channelone-struct)
3. [Static CSV-Only Arrays](#3-static-csv-only-arrays)
4. [Import Paths — CRITICAL ARCHITECTURE](#4-import-paths--critical-architecture)
5. [Export Path](#5-export-path)
6. [Android CSV Column Mapping (37 columns)](#6-android-csv-column-mapping-37-columns)
7. [DispData / SaveData Index Mapping](#7-dispdata--savedata-index-mapping)
8. [Known Bugs & Pitfalls](#8-known-bugs--pitfalls)
9. [Build & Package Workflow](#9-build--package-workflow)
10. [Fork Version Conventions](#10-fork-version-conventions)

---

## 1. Project Layout

```
OpenGD77CPS-Mac/
├── OpenGD77CPS.sln / .csproj       # Solution
├── DMR/                             # All form + business logic code
│   ├── MainForm.cs                  # Main MDI window; hosts all sub-forms; menu actions
│   ├── ChannelForm.cs               # Channel editor form (ChannelOne struct, static arrays, DispData/SaveData)
│   ├── ChannelsForm.cs              # Channel list grid + TWO IMPORT PATHS + Android CSV export
│   ├── ChannelsCsvImporter.cs       # ⚠️ DEAD CODE — Android importer never called; correct but unused
│   ├── ContactForm.cs               # Contact editor (DMR IDs)
│   ├── ContactsForm.cs              # Contact list grid
│   ├── RxGroupListForm.cs           # TG List (RX Group) form
│   ├── ZoneForm.cs                  # Zone editor
│   ├── ZonesCsvImporter.cs          # Zone CSV import
│   ├── ZonesCsvExporter.cs          # Zone CSV export
│   ├── TGListsCsvImporter.cs        # TG List CSV import
│   ├── TGListsCsvExporter.cs        # TG List CSV export
│   ├── AboutForm.cs                 # Fork version constants (FORK_VERSION, FORK_NAME)
│   └── ...other forms...
├── Settings.cs                      # Codeplug binary load/save (Marshal.PtrToStructure)
├── ReadWriteCsv/                    # CSV reader/writer utility
├── bin/ReleaseOpenGD77/             # Build output → OpenGD77CPS.exe
└── docs/
    └── CODEBASE_DEEP_DIVE.md        # This file
```

---

## 2. Data Model — ChannelOne Struct

**File**: `DMR/ChannelForm.cs`  
**Type**: `public struct ChannelOne : IVerify<ChannelOne>`

### CRITICAL CONSTRAINT — No New Instance Fields

`ChannelOne` is stored in a binary `.g77` codeplug file. On load, `Settings.smethod_62` calls `Marshal.PtrToStructure(IntPtr, typeof(ChannelOne))` to deserialize it directly from raw bytes. This means:

- **The struct's binary layout is fixed** — it must exactly match the on-disk format.
- **Adding any new instance fields (even `int`, `double`, `bool`) to the struct will shift the binary layout**, causing `AccessViolationException` on startup when the app tries to read a codeplug.
- **Do NOT add fields to `ChannelOne`** — instead, use the static arrays pattern (see §3).

### Existing Fields in ChannelOne (selected subset)

| Property | Type | Notes |
|---|---|---|
| `Name` | string | Channel name (max 16 chars) |
| `ChMode` / `ChModeS` | int / string | 0=Digital, 1=Analog |
| `RxFreq` / `TxFreq` | string | Frequency as Hz string |
| `TxColor` | int | DMR Colour Code (0–15) |
| `RepeaterSlot` / `RepeaterSlotS` | int / string | Timeslot (CPS is **1-based**: 1=TS1, 2=TS2) |
| `Contact` | int | Contact index **1-based** into ContactForm.data; 0 = None |
| `RxGroupList` | int | TG/RX group index 1-based; 0 = None |
| `Relay` | int | 0=invalid(→2), 1=relay-disconnect ON, 2=normal |
| `Interrupt` | int | 0=OFF, 1=Open, 2=Transport. Must be 2 for Digital, 0 for Analog |
| `OutboundSlot` | int | **CPS 1-based** (1=TS1, 2=TS2). Android CSV is **0-based** — convert on import/export |
| `EncryptSwitch` | int | 0=off, 1=on |
| `Active` | int | Whether this is the active channel |
| `ChannelMode` | int | 0=Direct, 3=Double Slot |
| `AndroidContactType` | int | 0=PERSON(Private), 1=GROUP, 2=ALL |
| `RxTone` / `TxTone` | string | CTCSS/DCS tone |
| `SquelchString` | string | Squelch level |
| `PowerString` | string | "Low" / "High" / "P1"–"P9" |
| `BandwidthString` | string | "12.5" or "25" |
| `OnlyRxString` | string | "No" / "Yes" |

### Fields That Do NOT Exist on ChannelOne

- `localId` — not a channel field (it's on DmrManager in the Android app)
- `Latitude`, `Longitude`, `UseLocation` — CSV-only, stored in static arrays
- `EncryptKey` — CSV-only, stored in static array (can't be in binary codeplug)

---

## 3. Static CSV-Only Arrays

**File**: `DMR/ChannelForm.cs`  
**Location**: Class-level statics on `ChannelForm`, after `public static Channel data;`

These four arrays hold data that must round-trip through CSV but cannot be stored in the binary `.g77` codeplug (either because the field is a reference type, or because the binary layout is fixed):

```csharp
internal static string[] CsvEncryptKeys   = new string[1024];  // Encrypt key (string — can't go in struct)
internal static double[] CsvLatitudes     = new double[1024];  // GPS latitude
internal static double[] CsvLongitudes    = new double[1024];  // GPS longitude
internal static bool[]   CsvUseLocations  = new bool[1024];    // Use Location flag
```

**Key**: the array index is `i` — the same 0-based data array index used by `ChannelForm.data[i]` and `ChannelForm.Tag`.

### Lifecycle

| Event | What happens |
|---|---|
| App start | Arrays initialized to `default(T)` (0.0, false, null) |
| CSV import | Arrays written: `CsvLatitudes[foundIndex] = lat` |
| Channel editor open (DispData) | Arrays read: `txtLatitude.Text = CsvLatitudes[index].ToString(...)` |
| Channel editor save (SaveData) | Arrays written from UI: `CsvLatitudes[index] = lat` |
| CSV export | Arrays read: `CsvLatitudes[i]` for each valid channel `i` |
| Binary codeplug save (.g77) | **Arrays are NOT saved** — data is lost! |
| Binary codeplug load (.g77) | **Arrays are NOT restored** — will show 0/false after loading a .g77 file |

### Clearing Arrays Before Import

When importing with "Clear All", arrays **must** be cleared too or stale values from a previous import will bleed through:

```csharp
// In ClearAllChannels() in ChannelsCsvImporter.cs AND in ImportFromCsvFile clearFirst block:
System.Array.Clear(ChannelForm.CsvEncryptKeys,  0, ChannelForm.CsvEncryptKeys.Length);
System.Array.Clear(ChannelForm.CsvLatitudes,    0, ChannelForm.CsvLatitudes.Length);
System.Array.Clear(ChannelForm.CsvLongitudes,   0, ChannelForm.CsvLongitudes.Length);
System.Array.Clear(ChannelForm.CsvUseLocations, 0, ChannelForm.CsvUseLocations.Length);
```

---

## 4. Import Paths — CRITICAL ARCHITECTURE

There are **three** import paths in this codebase. Only **one** is fully functional for the Android 37-column format.

### Path A: `ChannelsForm.import()` — OLD OpenGD77 button import (⚠️ DOES NOT support Android CSV)

**File**: `DMR/ChannelsForm.cs`, private method `import(bool clearFirst)`  
**Triggered by**: "Import" and "Import (Clear All)" buttons on the ChannelsForm grid  

How it works:
1. Opens a file dialog
2. Reads header row
3. Checks `csvRow.SequenceEqual(SZ_EXPORT_HEADER_TEXT)` — must exactly match the **35-column OpenGD77 export header**
4. If header doesn't match → `MessageBox.Show("DataFormatError")` — **Android CSV will show this error!**
5. If header matches → reads 35 columns sequentially via `((List<string>)csvRow)[num++]`

**No lat/lon support. No Android format support. Cannot be fixed to support Android without a header check bypass.**

### Path B: `ChannelsForm.ImportFromCsvFile()` — Static batch import (⚠️ PARTIALLY supports Android CSV)

**File**: `DMR/ChannelsForm.cs`, public static method `ImportFromCsvFile(filePath, clearFirst, mainForm, out importedCount)`  
**Triggered by**: MainForm menu (File → Import CSV batch)  

How it works:
1. Reads header row
2. Detects format: `isOpenGD77Format` (starts with "CH_DATA") vs `isAndroidFormat` (starts with "Channel Number" or "_id")
3. For OpenGD77 format: uses same logic as Path A (35 columns)
4. For Android format: reads columns positionally with a `col` counter, skipping `_id` if `hasIdColumn=true`

**BUG (as of v1.1.0)**: The Android branch stops at column 17 (Rx Only). Columns 18–36 (including Latitude at 26, Longitude at 27, Use Location at 28) are never read. **This is why lat/lon always shows 0 after import.**

**Also**: When `clearFirst=true`, the static CSV arrays are NOT cleared in this path (only `ChannelForm.data.ClearIndexAndReset` is called). See §3 for why this matters.

### Path C: `ChannelsCsvImporter.ImportChannelsFromCsv()` — New importer (⚠️ DEAD CODE — never called!)

**File**: `DMR/ChannelsCsvImporter.cs`  
**Triggered by**: Nothing — this class is never instantiated or called from anywhere!

This class was written specifically to handle the Android 37-column format including lat/lon. It correctly:
- Detects `hasIdColumn` and `hasNewFields`
- Uses `GetField(row, N, fieldOffset)` for consistent column access
- Reads lat/lon into `CsvLatitudes[channelIndex]` / `CsvLongitudes[channelIndex]`
- Reads all columns 25–35

**The fix is to wire this up, OR to add equivalent lat/lon reading code to Path B.**

### The Correct Fix (implemented in v1.2.0+)

Add columns 18–36 reading to Path B's Android handler in `ImportFromCsvFile()`, and clear static arrays when `clearFirst=true`. The column-by-column `col++` approach already used in the Android block is the correct pattern to extend.

---

## 5. Export Path

### Android CSV Export

**File**: `DMR/ChannelsForm.cs`, `ExportToAndroidCsvFile(filePath)`  
**Triggered by**: MainForm menu (File → Export CSV → Android format)

Writes 37-column CSV:
- Col 0: `_id` = `i + 1` (data index + 1)
- Col 1: Channel Number = `i + 1`
- Cols 2–17: standard channel fields
- Cols 18–24: Zone Skip, All Skip, TOT, VOX, No Beep, No Eco, APRS — all hardcoded defaults ("No", "0", "Off", "None")
- Col 25: Latitude from `ChannelForm.CsvLatitudes[i]`
- Col 26: Longitude from `ChannelForm.CsvLongitudes[i]`
- Col 27: Use Location from `ChannelForm.CsvUseLocations[i]`
- Col 28: Encrypt Switch — always "0" (can't recover from binary)
- Col 29: Encrypt Key from `ChannelForm.CsvEncryptKeys[i]`
- Cols 30–35: Relay, Interrupt, Active, Outbound Slot (converted 1-based→0-based), Channel Mode, Contact Type

**Important**: Encrypt Switch is always exported as "0" because the binary `.g77` codeplug does not store the encrypt switch state reliably after a round-trip through binary save/load.

### OpenGD77 / Standard Export

**File**: `DMR/ChannelsForm.cs`, existing export button code  
Uses `SZ_EXPORT_HEADER_TEXT` (35 columns, CH_DATA format).

---

## 6. Android CSV Column Mapping (37 columns)

This is the definitive column reference for the Android `Channels.csv` backup format.

| Col (0-based) | Header | ChannelOne field | Notes |
|---|---|---|---|
| 0 | `_id` | — | Database row ID; equals Channel Number for sequential imports |
| 1 | `Channel Number` | — | 1-based display number; used as array key via `channelNumber - 1` |
| 2 | `Channel Name` | `Name` | |
| 3 | `Channel Type` | `ChModeS` | "Analog" or "Digital" |
| 4 | `Rx Frequency` | `RxFreq` | Hz string, e.g. "162500000" |
| 5 | `Tx Frequency` | `TxFreq` | Hz string |
| 6 | `Bandwidth (kHz)` | `BandwidthString` | "12.5" or "25" |
| 7 | `Colour Code` | `TxColor` | 0–15, Digital only |
| 8 | `Timeslot` | `RepeaterSlotS` | "1" or "2" |
| 9 | `Contact` | `Contact` (via name lookup) | Contact name |
| 10 | `TG List` | `RxGroupList` (via name lookup) | TG List name |
| 11 | `DMR ID` | — | Always empty in CPS exports |
| 12 | `TS1_TA_Tx` | — | Skip |
| 13 | `TS2_TA_Tx ID` | — | Skip |
| 14 | `RX Tone` | `RxTone` | |
| 15 | `TX Tone` | `TxTone` | |
| 16 | `Squelch` | `SquelchString` | |
| 17 | `Power` | `PowerString` | |
| 18 | `Rx Only` | `OnlyRxString` | |
| 19 | `Zone Skip` | — | Skip (hardcoded "No" on export) |
| 20 | `All Skip` | — | Skip (hardcoded "No" on export) |
| 21 | `TOT` | — | Skip (hardcoded "0" on export) |
| 22 | `VOX` | — | Skip (hardcoded "Off" on export) |
| 23 | `No Beep` | — | Skip (hardcoded "No" on export) |
| 24 | `No Eco` | — | Skip (hardcoded "No" on export) |
| 25 | `APRS` | — | Skip (hardcoded "None" on export) |
| **26** | **`Latitude`** | **`CsvLatitudes[i]`** | **GPS latitude (double, InvariantCulture)** |
| **27** | **`Longitude`** | **`CsvLongitudes[i]`** | **GPS longitude (double, InvariantCulture)** |
| **28** | **`Use Location`** | **`CsvUseLocations[i]`** | **"Yes" / "No"** |
| 29 | `Encrypt Switch` | `EncryptSwitch` | Always exported as "0" |
| 30 | `Encrypt Key` | `CsvEncryptKeys[i]` | |
| 31 | `Relay` | `Relay` | 1=disconnect ON, 2=normal (0→2 on import) |
| 32 | `Interrupt` | `Interrupt` | 2 for Digital, 0 for Analog |
| 33 | `Active` | `Active` | |
| 34 | `Outbound Slot` | `OutboundSlot - 1` | Android 0-based; CPS 1-based → subtract 1 on export, add 1 on import |
| 35 | `Channel Mode` | `ChannelMode` | 0=Direct, 3=Double Slot |
| 36 | `Contact Type` | `AndroidContactType` | 0=PERSON, 1=GROUP, 2=ALL |

### `fieldOffset` Convention in ChannelsCsvImporter

`ChannelsCsvImporter` uses `GetField(row, N, fieldOffset)` = `row[N + fieldOffset]`:

- Android format (`_id` present): `fieldOffset = 1` — shifts all indices by +1 to skip the `_id` column
- OpenGD77 format: `fieldOffset = 0`

So `GetField(row, 26, 1)` = `row[27]` = Longitude for Android format. This is the **correct** pattern used in `ChannelsCsvImporter` — but that file is dead code! `ImportFromCsvFile` uses a different `col++` counter pattern instead.

---

## 7. DispData / SaveData Index Mapping

**How channel index flows from import → display:**

1. `ImportFromCsvFile` Android handler: calls `ChannelForm.data.GetMinIndex()` → assigns `foundIndex` (0-based sequential data array index)
2. `CsvLatitudes[foundIndex] = lat` — static array keyed by foundIndex
3. `ChannelForm.data.SetIndex(foundIndex, 1)` — marks slot as valid
4. `ChannelsForm.DispData()` builds the grid:
   - `for (int i = 0; i < ChannelForm.data.Count; i++) { if (DataIsValid(i)) ... Rows[...].Tag = i; }`
   - Each row's `Tag` = `i` (data array index = foundIndex from import)
5. User double-clicks row → `ChannelForm` opens with `base.Tag = i`
6. `ChannelForm.DispData()`:
   - `int num = Convert.ToInt32(base.Tag)` = `i`
   - `int index = num % 1024` = `i`
   - `txtLatitude.Text = ChannelForm.CsvLatitudes[index].ToString(...)` = `CsvLatitudes[i]`

**The index chain is self-consistent** as long as the import writes to `CsvLatitudes[foundIndex]` and foundIndex = the same value used as the data array index.

**Pitfall**: If `channelNumber - 1` is used as the static array key (as in `ChannelsCsvImporter`), but `foundIndex` from `GetMinIndex()` (in `ImportFromCsvFile`) is different (e.g. because of gaps in channel numbering), the indices diverge and DispData reads from the wrong slot.

**Resolution for `ImportFromCsvFile`**: Always use `foundIndex` as the key for static arrays, not `channelNumber - 1`. The `foundIndex` is what gets stored in `Tag` and used by DispData.

---

## 8. Known Bugs & Pitfalls

### Bug 1: Lat/Lon not shown after import (ROOT CAUSE of user-reported issue)

**File**: `ChannelsForm.cs`, `ImportFromCsvFile()`, Android format block  
**Problem**: After reading column 17 (Rx Only), the code stops and falls through to `ChannelForm.data[foundIndex] = value`. Columns 18–36 (including Latitude at 26, Longitude at 27, UseLocation at 28) are never read.  
**Fix**: Add column reading for cols 18–36 after reading Rx Only (see §9 below).

### Bug 2: Static arrays not cleared on clearFirst import via ImportFromCsvFile

**File**: `ChannelsForm.cs`, `ImportFromCsvFile()`, clearFirst block  
**Problem**: `ChannelForm.data.ClearIndexAndReset(n)` clears the struct data but NOT the static CSV arrays. Stale lat/lon values from a previous import persist.  
**Fix**: Add `System.Array.Clear(ChannelForm.CsvLatitudes, ...)` etc. in the clearFirst block.

### Bug 3: ChannelsCsvImporter is dead code

**File**: `ChannelsCsvImporter.cs`  
**Problem**: This class was written but never called. It correctly handles lat/lon for the Android format. It could be wired up to replace the Android branch in `ImportFromCsvFile`, or simply deleted to reduce confusion.  
**Status**: Dead code as of v1.1.0. Kept for reference.

### Bug 4: import() buttons on ChannelsForm reject Android CSV

**File**: `ChannelsForm.cs`, `import()` method  
**Problem**: The Import and "Import (Clear All)" buttons check `csvRow.SequenceEqual(SZ_EXPORT_HEADER_TEXT)`. The Android 37-column header never matches this 35-element array. Always shows "DataFormatError" for Android CSV.  
**Workaround**: Use File → Import CSV from the MainForm menu instead (calls `ImportFromCsvFile`).

### Bug 5: Encrypt Switch always exported as 0

**File**: `ChannelsForm.cs`, `ExportToAndroidCsvFile()`  
**Problem**: Encrypt switch setting is stored in the binary `.g77` codeplug but after a load/save cycle, the value returned by `EncryptSwitch` may not be reliable. The export hardcodes "0" (off).  
**Impact**: Encryption settings must be configured on the Android device directly; they don't survive a round-trip through this CPS.

### Bug 6: OnlyRxString / BandwidthString encoding difference

The Android app uses "Analog"/"Digital" for channel type. CPS uses "Analogue"/"Digital" internally. The `ChModeS` property setter handles this translation. If you ever compare directly, account for the spelling difference.

### Pitfall: Relay field value 0

Android firmware: 0=invalid, 1=relay-disconnect ON, 2=normal. OpenGD77 CPS internal model: 0=normal(unchecked), 1=relay-disconnect(checked). The export/import conversion:
- CPS model 0 → export "0" → Android importer coerces 0 → 2 (normal)  
- CPS model 1 → export "1" → Android uses as-is (relay-disconnect)

### Pitfall: OutboundSlot 0-based vs 1-based

- Android CSV: 0-based (0=TS1, 1=TS2)
- CPS internal model: 1-based (1=TS1, 2=TS2)
- On import: `value.OutboundSlot = outboundSlotVal + 1`
- On export: `csvRow.Add(Math.Max(0, channelOne.OutboundSlot - 1).ToString())`

---

## 9. Build & Package Workflow

```powershell
# Kill running app if any, then build
Stop-Process -Name "OpenGD77CPS" -ErrorAction SilentlyContinue
cd "C:\Users\Joshua\Documents\android\OpenGD77CPS-Mac"
& "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" OpenGD77CPS.sln /p:Configuration=Release /v:minimal

# Output: bin\ReleaseOpenGD77\OpenGD77CPS.exe

# Package for DMRModHooks phonedmrapp repo
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$zipPath = "C:\Users\Joshua\Documents\android\phonedmrapp\OpenGD77Fork\OpenGD77CPS-Mac_Build_${stamp}.zip"
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory(
    "C:\Users\Joshua\Documents\android\OpenGD77CPS-Mac\bin\ReleaseOpenGD77",
    $zipPath,
    [System.IO.Compression.CompressionLevel]::Optimal,
    $false
)
```

---

## 10. Fork Version Conventions

**File**: `DMR/AboutForm.cs`

```csharp
public const string FORK_VERSION = "1.x.x";   // Bump on EVERY release build
public const string FORK_NAME    = "DMRModHooks / PriInterPhone fork";
```

Rules:
- **PATCH** bump: rebuild, cosmetic fix, no behavior change
- **MINOR** bump: new field handled, new fix
- **MAJOR** bump: breaking CSV column layout change

After bumping: commit in fork repo with `Fork v1.x.x — description`, tag `fork-v1.x.x`, push.

---

## Appendix: Why ChannelsCsvImporter Exists But Is Dead Code

During development, `ChannelsCsvImporter.cs` was created as a clean replacement for the Android import path in `ChannelsForm.cs`. It correctly handles all 37 columns including lat/lon. However, it was never wired up — none of the existing import call sites (buttons or menu) were updated to call it.

The class is syntactically correct and logically sound. The simplest fix is to call it from `ImportFromCsvFile`'s Android branch, or to inline the equivalent logic there.

The reason lat/lon still shows 0 even with this class present: **the import button path rejects Android CSV entirely** (Path A), and **the menu import path never reads past column 17** (Path B). `ChannelsCsvImporter` (Path C) never runs.
