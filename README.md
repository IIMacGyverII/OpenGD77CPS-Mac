# OpenGD77 CPS - PriInterPhone Edition

**Customer Programming Software Modified for the PriInterPhone DMR Radio Android App**

## Overview

This is a **modified version** of the OpenGD77 Customer Programming Software (CPS), specifically adapted for use with the **PriInterPhone DMR Radio LSPosed mod** for Ulefone Armor 26 Ultra radios.

### What is OpenGD77?

OpenGD77 is an open-source firmware and programming ecosystem for DMR (Digital Mobile Radio) handheld and mobile radios. The standard OpenGD77 CPS is a Windows application that allows users to program DMR radios via CSV import/export or direct USB connection.

This fork extends the OpenGD77 CPS to support the **PriInterPhone Android app's extended CSV format**, enabling users to edit, manage, and program their PriInterPhone radio channels with advanced features like APRS, GPS coordinates, and encryption.

## Purpose & Use Case

### Specifically Designed For:
- **PriInterPhone DMR Radio** - LSPosed mod for Ulefone Armor 26 Ultra ([phonedmrapp repository](https://github.com/IIMacGyverII/phonedmrapp))
- Users who want to **edit DMR channel/contact programming on PC**
- Workflow: Export from Android app → Edit on PC → Import back to Android

### May Work With (But Not Officially Supported):
- Other OpenGD77-compatible radios (GD-77, DM-1801, RD-5R, etc.)
- Standard OpenGD77 firmware users
- Other DMR programming workflows that use OpenGD77 CSV format

**Note**: This CPS is optimized for the extended CSV format used by the PriInterPhone Android app, which includes additional fields for APRS, GPS coordinates, encryption settings, and zone management. While it can import/export standard OpenGD77 CSV files, it's designed with the PriInterPhone feature set in mind.

## Key Differences from Standard OpenGD77 CPS

### 1. **Extended CSV Format Support**
The PriInterPhone app exports **37-column CSV files** with extended fields:
- **APRS Settings**: APRS enable/disable per channel
- **GPS Coordinates**: Latitude/longitude for navigation
- **Encryption**: Digital channel encryption keys (DMR-only)
- **Zone Management**: Zone assignment and organization
- **Software Squelch**: Squelch level for analog monitoring
- **TG Lists**: Talk group list assignments

Standard OpenGD77 uses **25-column CSV files**. This fork can handle both formats but is optimized for the extended 37-column structure.

### 2. **Workflow Integration**
- Direct CSV workflow with the PriInterPhone Android app
- No USB cable required - export/import via SD card or cloud sync
- Designed for the app's backup structure: `Download/DMR/DMR_Backups/YYYYMMDD_HHmmss/`

### 3. **Feature Parity**
This fork maintains compatibility with standard OpenGD77 CSV format but adds:
- Extended field parsing for APRS, GPS, and encryption
- Zone management (Zones.csv support)
- TG List handling (TG_Lists.csv support)
- Validation for PriInterPhone-specific field ranges

## Requirements

### System Requirements
- **Windows** 7 or later
- **.NET Framework** 4.8

### Installation
1. Clone or download this repository
2. Build the solution in Visual Studio or MSBuild
3. Run OpenGD77CPS.exe

## Usage Workflow

### Export from PriInterPhone App
1. Open PriInterPhone app on your Ulefone radio
2. Go to **LOCAL** tab
3. Tap **📤 EXPORT (OpenGD77)** button
4. Files saved to `Download/DMR/DMR_Backups/YYYYMMDD_HHmmss/`
5. Transfer to your PC via:
   - USB cable (MTP mode)
   - Cloud sync (Google Drive, Dropbox, etc.)
   - ADB: `adb pull /sdcard/Download/DMR/DMR_Backups/`

### Edit on PC
1. Launch OpenGD77 CPS
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

# Build with MSBuild
msbuild OpenGD77CPS.sln /p:Configuration=Release /p:TargetFrameworkVersion=v4.8
```

Or open `OpenGD77CPS.sln` in Visual Studio and build normally.

### Project Structure
- **OpenGD77CPS/** - Main CPS application
- **OpenGD77CommDriver/** - Communication driver for USB programming (optional)
- **Extras/** - Additional tools and resources

### .NET Framework Version
- **Target**: .NET Framework 4.8
- **Platform**: Windows

## Known Limitations

- **CSV-Only Workflow**: Designed primarily for CSV export/import with the PriInterPhone app, not direct radio programming
- **Android-Specific Features**: Some extended fields (APRS, GPS, encryption) are specific to the PriInterPhone app and won't transfer to standard OpenGD77 radios

## Support

This is a specialized fork for the PriInterPhone DMR Radio project. For issues:
- **PriInterPhone app questions**: See [phonedmrapp repository](https://github.com/IIMacGyverII/phonedmrapp)
- **OpenGD77 CPS general questions**: See [OpenGD77 documentation](https://www.opengd77.com)
- **Fork-specific issues**: Open an issue in this repository

## License

This project inherits the license from the original OpenGD77 CPS project. See the LICENSE file for details.

## Credits

- **Original OpenGD77 Project**: Roger Clark, Daniel Caujolle-Bert, and contributors
- **PriInterPhone Edition**: Modified for extended CSV format support
- **PriInterPhone Mod**: Custom LSPosed module for Ulefone Armor 26 Ultra DMR radios

---

**Current Version**: Based on OpenGD77 CPS R2025.03.23.01  
**Last Updated**: March 23, 2026  
**Maintained By**: [@IIMacGyverII](https://github.com/IIMacGyverII)

