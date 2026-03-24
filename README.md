# OpenGD77 CPS for macOS - PriInterPhone Edition

**macOS-compatible Customer Programming Software for the PriInterPhone DMR Radio Android App**

## Overview

This is a **macOS port** of the OpenGD77 Customer Programming Software (CPS), specifically adapted for use with the **PriInterPhone DMR Radio LSPosed mod** for Ulefone Armor 26 Ultra radios.

### What is OpenGD77?

OpenGD77 is an open-source firmware and programming ecosystem for DMR (Digital Mobile Radio) handheld and mobile radios. The standard OpenGD77 CPS is a Windows-only application that allows users to program DMR radios via CSV import/export or direct USB connection.

This fork brings OpenGD77 CPS functionality to **macOS**, enabling Mac users to edit, manage, and program their PriInterPhone Android DMR radio using the familiar OpenGD77 CSV workflow.

## Purpose & Use Case

### Specifically Designed For:
- **PriInterPhone DMR Radio** - LSPosed mod for Ulefone Armor 26 Ultra ([phonedmrapp repository](https://github.com/IIMacGyverII/phonedmrapp))
- Users who want to **edit DMR channel/contact programming on a Mac**
- Cross-platform workflow: Export from Android app → Edit on Mac → Import back to Android

### May Work With (But Not Officially Supported):
- Other OpenGD77-compatible radios (GD-77, DM-1801, RD-5R, etc.)
- Standard OpenGD77 firmware users on macOS
- Other DMR programming workflows that use OpenGD77 CSV format

**Note**: This CPS is optimized for the extended CSV format used by the PriInterPhone Android app, which includes additional fields for APRS, GPS coordinates, encryption settings, and zone management. While it can import/export standard OpenGD77 CSV files, it's designed with the PriInterPhone feature set in mind.

## Key Differences from Standard OpenGD77 CPS

### 1. **Platform**
- **Standard OpenGD77 CPS**: Windows-only (.NET Framework 4.x)
- **This Fork**: macOS-compatible (.NET Framework 4.8 via Mono)

### 2. **Extended CSV Format Support**
The PriInterPhone app exports **37-column CSV files** with extended fields:
- **APRS Settings**: APRS enable/disable per channel
- **GPS Coordinates**: Latitude/longitude for navigation
- **Encryption**: Digital channel encryption keys (DMR-only)
- **Zone Management**: Zone assignment and organization
- **Software Squelch**: Squelch level for analog monitoring
- **TG Lists**: Talk group list assignments

Standard OpenGD77 uses **25-column CSV files**. This fork can handle both formats but is optimized for the extended 37-column structure.

### 3. **Workflow Integration**
- Direct CSV workflow with the PriInterPhone Android app
- No USB cable required - export/import via SD card or cloud sync
- Designed for the app's backup structure: `Download/DMR/DMR_Backups/YYYYMMDD_HHmmss/`

### 4. **Feature Parity**
This fork maintains compatibility with standard OpenGD77 CSV format but adds:
- Extended field parsing for APRS, GPS, and encryption
- Zone management (Zones.csv support)
- TG List handling (TG_Lists.csv support)
- Validation for PriInterPhone-specific field ranges

## Requirements

### System Requirements
- **macOS** 10.13 (High Sierra) or later
- **Mono Framework** 6.x or later (provides .NET 4.8 runtime on macOS)
- **Target Framework**: .NET Framework 4.8

### Installation
1. Install Mono Framework: https://www.mono-project.com/download/stable/
2. Clone or download this repository
3. Build or run the CPS using Mono

```bash
# Build the project
msbuild OpenGD77CPS.sln /p:Configuration=Release

# Run the CPS
mono bin/Release/OpenGD77CPS.exe
```

## Usage Workflow

### Export from PriInterPhone App
1. Open PriInterPhone app on your Ulefone radio
2. Go to **LOCAL** tab
3. Tap **📤 EXPORT (OpenGD77)** button
4. Files saved to `Download/DMR/DMR_Backups/YYYYMMDD_HHmmss/`
5. Transfer to your Mac via:
   - USB cable (MTP mode)
   - Cloud sync (Google Drive, Dropbox, etc.)
   - ADB: `adb pull /sdcard/Download/DMR/DMR_Backups/`

### Edit on Mac
1. Launch OpenGD77 CPS on macOS
2. **Import** → Select the backup folder containing CSV files
3. Edit channels, contacts, zones, TG lists as needed
4. **Export** → Save back to a folder

### Import to PriInterPhone App
1. Transfer edited CSV files back to Android device
2. Place in `Download/DMR/DMR_Backups/MyEditedBackup/`
3. Open PriInterPhone app
4. Go to **LOCAL** tab
5. Tap **📥 IMPORT (OpenGD77)** button
6. Select your edited backup folder
7. Channels refresh automatically

## CSV Files Explained

The PriInterPhone workflow uses **5 CSV files**:

1. **Channels.csv** (37 columns) - All channel definitions
   - Standard OpenGD77 fields (name, frequency, color code, timeslot, etc.)
   - Extended fields: APRS, GPS lat/lon, encryption, zones
2. **Contacts.csv** - DMR contact list (talk group IDs)
3. **TG_Lists.csv** - Talk group list assignments per channel
4. **Zones.csv** - Zone definitions (up to 80 channels per zone)
5. **DTMF.csv** - DTMF contact definitions

All files use **OpenGD77-compatible format** with CRLF line endings and tab-prefixed frequencies.

## Extended Field Details

### APRS Settings (Column 25)
- `TX` - APRS enabled for this channel
- `None` - APRS disabled

### GPS Coordinates (Columns 26-28)
- Latitude: Decimal degrees (e.g., `37.123456`)
- Longitude: Decimal degrees (e.g., `-122.123456`)
- Use Location: Always `No` for compatibility

### Encryption (Columns 29-30)
- **Digital channels only** (Analog does not support encryption)
- Encrypt Switch: `0` = disabled, `1` = enabled
- Encrypt Key: 32-character hex key (empty if disabled)

### Advanced Settings (Columns 31-36)
- Relay, Interrupt, Active, Outbound Slot, Channel Mode, Contact Type
- Used by firmware for advanced call routing and state management

## Related Projects

- **PriInterPhone DMR Radio App**: https://github.com/IIMacGyverII/phonedmrapp
- **Standard OpenGD77 Project**: https://www.opengd77.com
- **OpenGD77 Firmware**: https://github.com/LibreDMR/OpenGD77

## Development

### Building from Source
```bash
# Restore NuGet packages
nuget restore OpenGD77CPS.sln

# Build with Mono
msbuild OpenGD77CPS.sln /p:Configuration=Release /p:TargetFrameworkVersion=v4.8
```

### Project Structure
- **OpenGD77CPS/** - Main CPS application
- **OpenGD77CommDriver/** - Communication driver for USB programming (optional)
- **Extras/** - Additional tools and resources

### .NET Framework Version
- **Target**: .NET Framework 4.8
- **Runtime**: Mono 6.x+ on macOS
- **Compatibility**: Windows .NET Framework 4.8 (for cross-platform development)

## Known Limitations

- **USB Programming**: Direct USB programming to OpenGD77 radios may have limited support on macOS (driver compatibility)
- **CSV-Only Workflow**: Designed primarily for CSV export/import with the PriInterPhone app, not direct radio programming
- **Android-Specific Features**: Some extended fields (APRS, GPS, encryption) are specific to the PriInterPhone app and won't transfer to standard OpenGD77 radios

## Support

This is a specialized fork for the PriInterPhone DMR Radio project. For issues:
- **PriInterPhone app questions**: See [phonedmrapp repository](https://github.com/IIMacGyverII/phonedmrapp)
- **OpenGD77 CPS general questions**: See [OpenGD77 documentation](https://www.opengd77.com)
- **macOS-specific issues**: Open an issue in this repository

## License

This project inherits the license from the original OpenGD77 CPS project. See the LICENSE file for details.

## Credits

- **Original OpenGD77 Project**: Roger Clark, Daniel Caujolle-Bert, and contributors
- **macOS Port**: Maintained for PriInterPhone DMR Radio integration
- **PriInterPhone Mod**: Custom LSPosed module for Ulefone Armor 26 Ultra DMR radios

---

**Current Version**: Based on OpenGD77 CPS R2025.03.23.01  
**Last Updated**: March 23, 2026  
**Maintained By**: [@IIMacGyverII](https://github.com/IIMacGyverII)

