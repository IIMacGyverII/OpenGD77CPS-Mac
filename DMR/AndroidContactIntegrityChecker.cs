using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReadWriteCsv;

namespace DMR
{
	public sealed class AndroidContactIntegrityResult
	{
		public List<string> Warnings = new List<string>();
		public int UnresolvedContacts;
		public int RelayZeroCount;
		public int UnknownChannelModeCount;

		public bool HasWarnings
		{
			get { return this.Warnings.Count > 0; }
		}

		public string Summary
		{
			get
			{
				if (!this.HasWarnings)
				{
					return "OK: No contact/relay/channel-mode integrity issues detected.";
				}
				StringBuilder sb = new StringBuilder();
				sb.Append("INTEGRITY: ").Append(this.Warnings.Count).Append(" warning(s)");
				if (this.UnresolvedContacts > 0)
				{
					sb.Append(" — ").Append(this.UnresolvedContacts).Append(" unresolved contact(s)");
				}
				if (this.RelayZeroCount > 0)
				{
					sb.Append(" — ").Append(this.RelayZeroCount).Append(" relay=0");
				}
				if (this.UnknownChannelModeCount > 0)
				{
					sb.Append(" — ").Append(this.UnknownChannelModeCount).Append(" unknown channel_mode");
				}
				return sb.ToString();
			}
		}

		public string DetailText
		{
			get { return string.Join(Environment.NewLine, this.Warnings.ToArray()); }
		}
	}

	/// <summary>
	/// Tier 2.6 — cross-check Channels.csv DMR IDs against Contacts.csv (Pitfall 12).
	/// </summary>
	public static class AndroidContactIntegrityChecker
	{
		public static AndroidContactIntegrityResult CheckFolder(string folderPath)
		{
			AndroidContactIntegrityResult result = new AndroidContactIntegrityResult();
			if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
			{
				return result;
			}

			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			string contactsPath = Path.Combine(folderPath, "Contacts.csv");
			if (!File.Exists(channelsPath))
			{
				return result;
			}

			HashSet<string> contactDmrIds = LoadContactDmrIds(contactsPath);
			bool contactsMissing = !File.Exists(contactsPath);

			try
			{
				using (CsvFileReader reader = new CsvFileReader(channelsPath, CsvEncoding.Utf8NoBom))
				{
					CsvRow header = new CsvRow();
					if (!reader.ReadRow(header) || header.Count == 0)
					{
						return result;
					}

					bool hasId = ((List<string>)header)[0].Trim() == "_id";
					if (!hasId && ((List<string>)header)[0].Trim() != "Channel Number")
					{
						return result;
					}

					int nameCol = hasId ? 2 : 1;
					int dmrIdCol = hasId ? 11 : 10;
					int relayCol = hasId ? 31 : 30;
					int modeCol = hasId ? 34 : 33;

					CsvRow row = new CsvRow();
					while (reader.ReadRow(row))
					{
						List<string> cols = (List<string>)row;
						if (cols.Count <= nameCol)
						{
							continue;
						}
						string channelName = cols[nameCol].Trim();
						if (string.IsNullOrEmpty(channelName))
						{
							continue;
						}

						if (cols.Count > dmrIdCol)
						{
							string dmrId = cols[dmrIdCol].Trim();
							if (!string.IsNullOrEmpty(dmrId) && dmrId != "0" && dmrId != "None")
							{
								if (contactsMissing)
								{
									result.UnresolvedContacts++;
									result.Warnings.Add(channelName + ": DMR ID " + dmrId + " — Contacts.csv missing");
								}
								else if (!contactDmrIds.Contains(dmrId))
								{
									result.UnresolvedContacts++;
									result.Warnings.Add(channelName + ": DMR ID " + dmrId + " not in Contacts.csv");
								}
							}
						}

						if (cols.Count > relayCol)
						{
							int relayVal;
							if (int.TryParse(cols[relayCol].Trim(), out relayVal) && relayVal == 0)
							{
								result.RelayZeroCount++;
								result.Warnings.Add(channelName + ": relay=0 will cause \"operation failed\" on phone (coerced to 2 on CPS import)");
							}
						}

						if (cols.Count > modeCol)
						{
							int modeVal;
							if (int.TryParse(cols[modeCol].Trim(), out modeVal)
								&& modeVal != 0 && modeVal != 3 && modeVal != 4)
							{
								result.UnknownChannelModeCount++;
								result.Warnings.Add(channelName + ": channel_mode=" + modeVal + " (expected 0 Direct, 3 CPS double-slot, or 4 phone double-slot)");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				result.Warnings.Add("Integrity check error: " + ex.Message);
			}

			return result;
		}

		private static HashSet<string> LoadContactDmrIds(string contactsPath)
		{
			HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (!File.Exists(contactsPath))
			{
				return ids;
			}

			using (CsvFileReader reader = new CsvFileReader(contactsPath, CsvEncoding.Utf8NoBom))
			{
				CsvRow header = new CsvRow();
				if (!reader.ReadRow(header) || header.Count == 0)
				{
					return ids;
				}

				List<string> headerCols = (List<string>)header;
				int idCol = 1;
				if (headerCols[0].Trim() == "_id")
				{
					idCol = 2;
				}
				else if (headerCols[0].Trim() == "Contact Name")
				{
					idCol = 1;
				}
				else if (headerCols.Count > 1 && headerCols[1].Trim().Equals("ID", StringComparison.OrdinalIgnoreCase))
				{
					idCol = 1;
				}

				CsvRow row = new CsvRow();
				while (reader.ReadRow(row))
				{
					List<string> cols = (List<string>)row;
					if (cols.Count > idCol)
					{
						string id = cols[idCol].Trim();
						if (!string.IsNullOrEmpty(id) && id != "0" && id != "None")
						{
							ids.Add(id);
						}
					}
				}
			}
			return ids;
		}
	}

}