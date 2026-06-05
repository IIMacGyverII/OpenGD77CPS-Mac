using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReadWriteCsv;

namespace DMR
{
	public sealed class AndroidBackupValidationResult
	{
		public bool HasBlockingErrors;
		public int RelayZeroCount;
		public int DuplicateChannelNames;
		public string Summary = "";
	}

	/// <summary>
	/// Pre-import checks for phone backup folders (UI plan §2.2 / §2.5 lite).
	/// </summary>
	public static class AndroidBackupValidator
	{
		private const int MinAndroidChannelColumns = 30;

		public static AndroidBackupValidationResult ValidateFolder(string folderPath)
		{
			AndroidBackupValidationResult result = new AndroidBackupValidationResult();
			StringBuilder log = new StringBuilder();

			if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
			{
				result.HasBlockingErrors = true;
				result.Summary = "Folder not found.";
				return result;
			}

			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			string contactsPath = Path.Combine(folderPath, "Contacts.csv");

			if (!File.Exists(channelsPath))
			{
				log.AppendLine("ERROR: Channels.csv missing (required for Path B).");
				result.HasBlockingErrors = true;
			}
			else
			{
				ValidateChannelsCsv(channelsPath, result, log);
			}

			if (!File.Exists(contactsPath))
			{
				log.AppendLine("WARN: Contacts.csv missing — import channels without contact names may fail.");
			}
			else
			{
				ValidateContactsHeader(contactsPath, log);
			}

			foreach (string optional in new string[] { "TG_Lists.csv", "Zones.csv", "DTMF.csv" })
			{
				string path = Path.Combine(folderPath, optional);
				log.AppendLine(File.Exists(path) ? ("OK: " + optional) : ("—: " + optional + " not present"));
			}

			if (result.RelayZeroCount > 0)
			{
				log.AppendLine("WARN: " + result.RelayZeroCount + " channel row(s) with relay=0 (coerced to 2 on import).");
			}
			if (result.DuplicateChannelNames > 0)
			{
				log.AppendLine("WARN: " + result.DuplicateChannelNames + " duplicate channel name(s) in CSV.");
			}

			result.Summary = log.ToString().TrimEnd();
			return result;
		}

		private static void ValidateChannelsCsv(string path, AndroidBackupValidationResult result, StringBuilder log)
		{
			try
			{
				using (CsvFileReader reader = new CsvFileReader(path, CsvEncoding.Utf8NoBom))
				{
					CsvRow header = new CsvRow();
					if (!reader.ReadRow(header) || header.Count == 0)
					{
						log.AppendLine("ERROR: Channels.csv is empty.");
						result.HasBlockingErrors = true;
						return;
					}

					string first = ((List<string>)header)[0].Trim();
					bool hasId = first == "_id";
					if (!hasId && first != "Channel Number")
					{
						log.AppendLine("ERROR: Channels.csv header must start with '_id' or 'Channel Number' (got '" + first + "').");
						log.AppendLine("       Use File → Import Android backup (Path B), not grid import.");
						result.HasBlockingErrors = true;
						return;
					}

					if (hasId && header.Count < MinAndroidChannelColumns)
					{
						log.AppendLine("WARN: Channels.csv has " + header.Count + " columns; expected ~37 for Android Path B.");
					}
					else
					{
						log.AppendLine("OK: Channels.csv Path B header (" + header.Count + " columns, _id=" + hasId + ").");
					}

					int relayCol = hasId ? 31 : 30;
					int nameCol = hasId ? 2 : 1;
					HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					int rowNum = 1;

					CsvRow row = new CsvRow();
					while (reader.ReadRow(row))
					{
						rowNum++;
						List<string> cols = (List<string>)row;
						if (cols.Count <= nameCol)
						{
							continue;
						}
						string name = cols[nameCol].Trim();
						if (!string.IsNullOrEmpty(name))
						{
							if (!names.Add(name))
							{
								result.DuplicateChannelNames++;
							}
						}
						if (cols.Count > relayCol)
						{
							int relayVal;
							if (int.TryParse(cols[relayCol].Trim(), out relayVal) && relayVal == 0)
							{
								result.RelayZeroCount++;
							}
						}
					}
					log.AppendLine("OK: " + (rowNum - 1) + " channel data row(s) scanned.");
				}
			}
			catch (Exception ex)
			{
				log.AppendLine("ERROR: Channels.csv — " + ex.Message);
				result.HasBlockingErrors = true;
			}
		}

		private static void ValidateContactsHeader(string path, StringBuilder log)
		{
			try
			{
				using (CsvFileReader reader = new CsvFileReader(path, CsvEncoding.Utf8NoBom))
				{
					CsvRow header = new CsvRow();
					if (reader.ReadRow(header) && header.Count > 0)
					{
						string first = ((List<string>)header)[0].Trim();
						if (first == "_id")
						{
							log.AppendLine("OK: Contacts.csv Android header.");
						}
						else
						{
							log.AppendLine("WARN: Contacts.csv may not be Android format (first column: '" + first + "').");
						}
					}
				}
			}
			catch (Exception ex)
			{
				log.AppendLine("WARN: Contacts.csv — " + ex.Message);
			}
		}
	}
}