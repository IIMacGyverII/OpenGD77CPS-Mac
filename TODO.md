# To do (next time we work on the fork)

## Keep the Android Encrypt Switch through a CSV round trip

**Problem.** `ChannelsForm` exports column 29 (*Encrypt Switch*) as a hard-coded `0`
(`DMR/ChannelsForm.cs`, the `// Column 29: Encrypt Switch` block in the Android CSV export,
`csvRow.Add("0")`). A channel with encryption on in the Android app (DMRModHooks module or the
DMR Radio standalone app) therefore comes back with encryption **off** after it passes through
the CPS, although the key itself survives (it is kept in `ChannelForm.CsvEncryptKeys[]`).

**Fix.** Keep the switch CSV-only, the same way the key is kept:

- add `internal static int[] CsvEncryptSwitch = new int[1024];` next to `CsvEncryptKeys` in
  `DMR/ChannelForm.cs`, and clear it wherever `CsvEncryptKeys` is cleared;
- in `ImportFromCsvFile`, where column 29 is parsed (`value.EncryptSwitch = encSw`), also store
  `CsvEncryptSwitch[foundIndex] = encSw` (Android values: 1 = on, 2 = off; treat 0 as off);
- in the export, write `CsvEncryptSwitch[i]` (1 or 0, matching what the module and the app read:
  1 = on, anything else = off) instead of the constant `0`;
- optionally show it next to the key in the channel editor, like the Soft SQ fields (v2.0.44).

**Test.** Export a backup from the Android app with one encrypted DMR channel, import it in the
CPS (Android CSV path B), export again, restore on the phone: the channel must still be encrypted
with the same key. In the standalone app, Diagnostics → *Check backup round trip* compares every
field after a restore.

Reported 2026-10-18 during the standalone app's CSV audit (phonedmrapp `CHANGELOG_DRAFT.md`).
