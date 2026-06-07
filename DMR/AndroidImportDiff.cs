using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ReadWriteCsv;

namespace DMR
{
	public sealed class AndroidChannelCsvSnapshot
	{
		public string ChannelNumber = "";
		public string Name = "";
		public string ChModeS = "";
		public string RxFreq = "";
		public string TxFreq = "";
		public string Bandwidth = "";
		public string TxColor = "";
		public string RepeaterSlot = "";
		public string ContactName = "";
		public string TgListName = "";
		public string RxTone = "";
		public string TxTone = "";
		public string Squelch = "";
		public string Power = "";
		public string RxOnly = "";
		public string Relay = "";
		public string ChannelMode = "";
		public string OutboundSlot = "";
		public string Latitude = "";
		public string Longitude = "";
		public string UseLocation = "";
	}

	public sealed class AndroidImportDiffRow
	{
		public string Status;
		public string ChannelName;
		public string ChannelNumber;
		public string Details;
	}

	public sealed class AndroidImportDiffResult
	{
		public int Added;
		public int Changed;
		public int Deleted;
		public int Unchanged;
		public List<AndroidImportDiffRow> Rows = new List<AndroidImportDiffRow>();
		public string Summary = "";
	}

	/// <summary>
	/// Tier 2.5 — compare Android Channels.csv (Path B) to loaded codeplug before import.
	/// </summary>
	public static class AndroidImportDiff
	{
		public static AndroidImportDiffResult Compute(string channelsCsvPath)
		{
			AndroidImportDiffResult result = new AndroidImportDiffResult();
			if (string.IsNullOrEmpty(channelsCsvPath) || !File.Exists(channelsCsvPath))
			{
				result.Summary = "Channels.csv not found.";
				return result;
			}

			Dictionary<string, AndroidChannelCsvSnapshot> csvByName =
				new Dictionary<string, AndroidChannelCsvSnapshot>(StringComparer.OrdinalIgnoreCase);
			List<string> csvOrder = new List<string>();

			try
			{
				using (CsvFileReader reader = new CsvFileReader(channelsCsvPath, CsvEncoding.Utf8NoBom))
				{
					CsvRow header = new CsvRow();
					if (!reader.ReadRow(header) || header.Count == 0)
					{
						result.Summary = "Channels.csv is empty.";
						return result;
					}

					string first = ((List<string>)header)[0].Trim();
					bool hasId = first == "_id";
					if (!hasId && first != "Channel Number")
					{
						result.Summary = "Not Android Path B format.";
						return result;
					}

					CsvRow row = new CsvRow();
					while (reader.ReadRow(row))
					{
						AndroidChannelCsvSnapshot snap = ParseAndroidRow((List<string>)row, hasId);
						if (string.IsNullOrEmpty(snap.Name))
						{
							continue;
						}
						csvOrder.Add(snap.Name);
						csvByName[snap.Name] = snap;
					}
				}
			}
			catch (Exception ex)
			{
				result.Summary = "Could not read Channels.csv: " + ex.Message;
				return result;
			}

			Dictionary<string, int> loadedIndexByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < ChannelForm.data.Count; i++)
			{
				if (!ChannelForm.data.DataIsValid(i))
				{
					continue;
				}
				string name = ChannelForm.data[i].Name;
				if (!string.IsNullOrEmpty(name) && !loadedIndexByName.ContainsKey(name))
				{
					loadedIndexByName[name] = i;
				}
			}

			HashSet<string> seenCsv = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (string csvName in csvOrder)
			{
				if (!seenCsv.Add(csvName))
				{
					continue;
				}

				AndroidChannelCsvSnapshot csvSnap = csvByName[csvName];
				int loadedIndex;
				if (!loadedIndexByName.TryGetValue(csvName, out loadedIndex))
				{
					result.Added++;
					result.Rows.Add(new AndroidImportDiffRow
					{
						Status = "Added",
						ChannelName = csvName,
						ChannelNumber = csvSnap.ChannelNumber,
						Details = FormatSnapshotSummary(csvSnap)
					});
					continue;
				}

				AndroidChannelCsvSnapshot loadedSnap = SnapshotFromLoaded(loadedIndex);
				string fieldDiffs = CompareSnapshots(loadedSnap, csvSnap);
				if (string.IsNullOrEmpty(fieldDiffs))
				{
					result.Unchanged++;
					result.Rows.Add(new AndroidImportDiffRow
					{
						Status = "Unchanged",
						ChannelName = csvName,
						ChannelNumber = csvSnap.ChannelNumber,
						Details = ""
					});
				}
				else
				{
					result.Changed++;
					result.Rows.Add(new AndroidImportDiffRow
					{
						Status = "Changed",
						ChannelName = csvName,
						ChannelNumber = csvSnap.ChannelNumber,
						Details = fieldDiffs
					});
				}
			}

			foreach (KeyValuePair<string, int> pair in loadedIndexByName)
			{
				if (!csvByName.ContainsKey(pair.Key))
				{
					result.Deleted++;
					AndroidChannelCsvSnapshot loadedSnap = SnapshotFromLoaded(pair.Value);
					result.Rows.Add(new AndroidImportDiffRow
					{
						Status = "Deleted",
						ChannelName = pair.Key,
						ChannelNumber = "",
						Details = "In codeplug, not in CSV — removed on import (clearFirst)."
					});
				}
			}

			StringBuilder summary = new StringBuilder();
			summary.Append("Import replaces all channels (clearFirst). ");
			summary.Append(result.Added).Append(" added, ");
			summary.Append(result.Changed).Append(" changed, ");
			summary.Append(result.Deleted).Append(" deleted, ");
			summary.Append(result.Unchanged).Append(" unchanged.");
			result.Summary = summary.ToString();
			return result;
		}

		/// <summary>File stamp used to invalidate diff review when Channels.csv changes on disk (re-pull, edit).</summary>
		public static bool HasPendingDiffChanges(AndroidImportDiffResult diff)
		{
			return diff != null && (diff.Added > 0 || diff.Changed > 0 || diff.Deleted > 0);
		}

		public static void OfferReviewAfterPullIfNeeded(
			IWin32Window owner,
			AndroidImportDiffResult diff,
			bool diffPreApproved,
			bool hasBlockingErrors,
			Action reviewAction)
		{
			if (hasBlockingErrors || diffPreApproved || !HasPendingDiffChanges(diff))
			{
				return;
			}
			int pending = diff.Added + diff.Changed + diff.Deleted;
			DialogResult offer = MessageBox.Show(owner,
				"Phone backup pulled.\n\n" + pending + " channel change(s) detected vs your loaded codeplug.\n\nOpen Review diff now?",
				"Review import changes",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			if (offer == DialogResult.Yes && reviewAction != null)
			{
				reviewAction();
			}
		}

		public static bool IsDiffReviewCurrent(string channelsCsvPath, bool preApproved, string approvedStamp)
		{
			if (!preApproved || string.IsNullOrEmpty(channelsCsvPath) || !File.Exists(channelsCsvPath))
			{
				return false;
			}
			return string.Equals(approvedStamp, GetChannelsCsvStamp(channelsCsvPath), StringComparison.Ordinal);
		}

		public static string GetChannelsCsvStamp(string channelsCsvPath)
		{
			if (string.IsNullOrEmpty(channelsCsvPath) || !File.Exists(channelsCsvPath))
			{
				return "";
			}
			FileInfo info = new FileInfo(channelsCsvPath);
			return info.LastWriteTimeUtc.Ticks.ToString() + ":" + info.Length.ToString();
		}

		public static int FindLoadedChannelIndexByName(string channelName)
		{
			if (string.IsNullOrEmpty(channelName))
			{
				return -1;
			}
			for (int i = 0; i < ChannelForm.data.Count; i++)
			{
				if (!ChannelForm.data.DataIsValid(i))
				{
					continue;
				}
				if (string.Equals(ChannelForm.data[i].Name, channelName, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		public static bool ShowPreviewDialog(IWin32Window owner, string channelsCsvPath, MainForm mainForm = null)
		{
			if (mainForm == null)
			{
				mainForm = owner as MainForm;
			}
			using (AndroidImportDiffForm form = new AndroidImportDiffForm(channelsCsvPath, mainForm))
			{
				return form.ShowDialog(owner) == DialogResult.OK;
			}
		}

		private static AndroidChannelCsvSnapshot ParseAndroidRow(List<string> cols, bool hasIdColumn)
		{
			AndroidChannelCsvSnapshot snap = new AndroidChannelCsvSnapshot();
			if (cols == null || cols.Count < 16)
			{
				return snap;
			}

			int col = 0;
			if (hasIdColumn)
			{
				col++;
			}

			snap.ChannelNumber = SafeCol(cols, col++);
			snap.Name = SafeCol(cols, col++);
			snap.ChModeS = SafeCol(cols, col++);
			snap.RxFreq = SafeCol(cols, col++);
			snap.TxFreq = SafeCol(cols, col++);
			snap.Bandwidth = SafeCol(cols, col++);
			snap.TxColor = SafeCol(cols, col++);
			snap.RepeaterSlot = SafeCol(cols, col++);
			snap.ContactName = SafeCol(cols, col++);
			snap.TgListName = SafeCol(cols, col++);

			col++; // DMR ID
			col++; // TS1_TA_Tx
			col++; // TS2_TA_Tx

			snap.RxTone = SafeCol(cols, col++);
			snap.TxTone = SafeCol(cols, col++);
			snap.Squelch = SafeCol(cols, col++);
			snap.Power = SafeCol(cols, col++);
			snap.RxOnly = SafeCol(cols, col++);

			for (int skip = 0; skip < 7; skip++)
			{
				col++;
			}

			snap.Latitude = SafeCol(cols, col++);
			snap.Longitude = SafeCol(cols, col++);
			snap.UseLocation = SafeCol(cols, col++);

			col++; // encrypt switch
			col++; // encrypt key

			string relay = SafeCol(cols, col++);
			int relayVal;
			if (int.TryParse(relay, out relayVal) && relayVal == 0)
			{
				relay = "2";
			}
			snap.Relay = relay;

			col++; // interrupt
			col++; // active

			string outbound = SafeCol(cols, col++);
			int slotVal;
			if (int.TryParse(outbound, out slotVal))
			{
				snap.OutboundSlot = (slotVal + 1).ToString();
			}
			else
			{
				snap.OutboundSlot = outbound;
			}

			snap.ChannelMode = SafeCol(cols, col++);

			return snap;
		}

		private static string SafeCol(List<string> cols, int index)
		{
			if (index < 0 || index >= cols.Count)
			{
				return "";
			}
			return cols[index].Trim();
		}

		private static AndroidChannelCsvSnapshot SnapshotFromLoaded(int index)
		{
			ChannelForm.ChannelOne ch = ChannelForm.data[index];
			AndroidChannelCsvSnapshot snap = new AndroidChannelCsvSnapshot();
			snap.Name = ch.Name;
			snap.ChModeS = ch.ChModeS;
			snap.RxFreq = NormalizeFreq(ch.RxFreq);
			snap.TxFreq = NormalizeFreq(ch.TxFreq);
			snap.Bandwidth = ch.BandwidthString;
			snap.TxColor = ch.TxColor.ToString();
			snap.RepeaterSlot = ch.RepeaterSlotS;
			snap.ContactName = GetContactName(ch.Contact);
			snap.TgListName = ch.RxGroupListString;
			if (snap.TgListName == Settings.SZ_NONE)
			{
				snap.TgListName = "";
			}
			snap.RxTone = ch.RxTone;
			snap.TxTone = ch.TxTone;
			snap.Squelch = ch.SquelchString;
			snap.Power = ch.PowerString;
			snap.RxOnly = ch.OnlyRxString;
			snap.Relay = ch.Relay.ToString();
			snap.ChannelMode = ch.ChannelMode.ToString();
			snap.OutboundSlot = ch.OutboundSlot.ToString();
			snap.Latitude = ChannelForm.CsvLatitudes[index].ToString(System.Globalization.CultureInfo.InvariantCulture);
			snap.Longitude = ChannelForm.CsvLongitudes[index].ToString(System.Globalization.CultureInfo.InvariantCulture);
			snap.UseLocation = ChannelForm.CsvUseLocations[index] ? "Yes" : "No";
			return snap;
		}

		private static string GetContactName(int contactOneBased)
		{
			if (contactOneBased <= 0)
			{
				return "";
			}
			int idx = contactOneBased - 1;
			if (idx >= 0 && idx < ContactForm.data.Count && ContactForm.data.DataIsValid(idx))
			{
				return ContactForm.data.GetName(idx);
			}
			return "";
		}

		private static string NormalizeFreq(string freq)
		{
			if (string.IsNullOrEmpty(freq))
			{
				return "";
			}
			decimal d;
			if (decimal.TryParse(freq.Trim(), System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out d))
			{
				return d.ToString("0.#####", System.Globalization.CultureInfo.InvariantCulture);
			}
			return freq.Trim();
		}

		private static string CompareSnapshots(AndroidChannelCsvSnapshot loaded, AndroidChannelCsvSnapshot csv)
		{
			StringBuilder diffs = new StringBuilder();
			AppendDiff(diffs, "Type", loaded.ChModeS, csv.ChModeS);
			AppendDiff(diffs, "Rx", loaded.RxFreq, NormalizeFreq(csv.RxFreq));
			AppendDiff(diffs, "Tx", loaded.TxFreq, NormalizeFreq(csv.TxFreq));
			AppendDiff(diffs, "BW", loaded.Bandwidth, csv.Bandwidth);
			AppendDiff(diffs, "Color", loaded.TxColor, csv.TxColor);
			AppendDiff(diffs, "Slot", loaded.RepeaterSlot, csv.RepeaterSlot);
			AppendDiff(diffs, "Contact", loaded.ContactName, csv.ContactName);
			AppendDiff(diffs, "TG", loaded.TgListName, csv.TgListName);
			AppendDiff(diffs, "RxTone", loaded.RxTone, csv.RxTone);
			AppendDiff(diffs, "TxTone", loaded.TxTone, csv.TxTone);
			AppendDiff(diffs, "SQL", loaded.Squelch, csv.Squelch);
			AppendDiff(diffs, "Power", loaded.Power, csv.Power);
			AppendDiff(diffs, "RxOnly", loaded.RxOnly, csv.RxOnly);
			AppendDiff(diffs, "Relay", loaded.Relay, csv.Relay);
			AppendDiff(diffs, "Mode", loaded.ChannelMode, csv.ChannelMode);
			AppendDiff(diffs, "OutSlot", loaded.OutboundSlot, csv.OutboundSlot);
			AppendDiff(diffs, "Lat", loaded.Latitude, csv.Latitude);
			AppendDiff(diffs, "Lon", loaded.Longitude, csv.Longitude);
			AppendDiff(diffs, "UseLoc", loaded.UseLocation, csv.UseLocation);
			return diffs.ToString().TrimEnd(',', ' ');
		}

		private static void AppendDiff(StringBuilder sb, string label, string oldVal, string newVal)
		{
			string a = (oldVal ?? "").Trim();
			string b = (newVal ?? "").Trim();
			if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			if (sb.Length > 0)
			{
				sb.Append("; ");
			}
			sb.Append(label).Append(": ").Append(a).Append(" \u2192 ").Append(b);
		}

		private static string FormatSnapshotSummary(AndroidChannelCsvSnapshot snap)
		{
			return snap.ChModeS + " " + snap.RxFreq + "/" + snap.TxFreq
				+ (string.IsNullOrEmpty(snap.ContactName) ? "" : (" contact=" + snap.ContactName));
		}
	}

}