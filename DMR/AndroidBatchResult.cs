using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Tier 2.4 — aggregated import/export outcome for scrollable summary dialog.
	/// </summary>
	public sealed class AndroidBatchResult
	{
		public string Operation = "Import";
		public string FolderPath = "";
		public int FilesAttempted;
		public int FilesSucceeded;
		public int ContactsCount;
		public int TgListsCount;
		public int ChannelsCount;
		public int ZonesCount;
		public int ZonesSkipped;
		public readonly List<string> LogLines = new List<string>();
		public readonly List<string> Warnings = new List<string>();

		public bool HasErrors
		{
			get
			{
				foreach (string line in this.LogLines)
				{
					if (line.StartsWith("\u2717") || line.StartsWith("✗"))
					{
						return true;
					}
				}
				return false;
			}
		}

		public void AddLog(string line)
		{
			this.LogLines.Add(line);
			if (line.StartsWith("\u2713") || line.StartsWith("✓"))
			{
				this.FilesSucceeded++;
			}
		}

		public void AddWarning(string warning)
		{
			if (!string.IsNullOrEmpty(warning))
			{
				this.Warnings.Add(warning);
			}
		}

		public string Title
		{
			get
			{
				if (this.HasErrors)
				{
					return this.Operation + " completed with errors";
				}
				return this.Operation + " complete";
			}
		}

		public string StatsLine
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(this.FilesAttempted).Append(" file(s) processed");
				if (this.FilesSucceeded > 0)
				{
					sb.Append(" — ").Append(this.FilesSucceeded).Append(" succeeded");
				}
				if (this.ContactsCount > 0)
				{
					sb.Append(" — ").Append(this.ContactsCount).Append(" contact(s)");
				}
				if (this.TgListsCount > 0)
				{
					sb.Append(" — ").Append(this.TgListsCount).Append(" TG list(s)");
				}
				if (this.ChannelsCount > 0)
				{
					sb.Append(" — ").Append(this.ChannelsCount).Append(" channel(s)");
				}
				if (this.ZonesCount > 0 || this.ZonesSkipped > 0)
				{
					sb.Append(" — ").Append(this.ZonesCount).Append(" zone(s)");
					if (this.ZonesSkipped > 0)
					{
						sb.Append(" (").Append(this.ZonesSkipped).Append(" skipped)");
					}
				}
				return sb.ToString();
			}
		}

		public string DetailText
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				if (!string.IsNullOrEmpty(this.FolderPath))
				{
					sb.AppendLine("Folder: " + this.FolderPath);
					sb.AppendLine();
				}
				foreach (string line in this.LogLines)
				{
					sb.AppendLine(line);
				}
				if (this.Warnings.Count > 0)
				{
					sb.AppendLine();
					sb.AppendLine("Warnings:");
					foreach (string warning in this.Warnings)
					{
						sb.AppendLine("  • " + warning);
					}
				}
				return sb.ToString().TrimEnd();
			}
		}

		public static void ShowDialog(IWin32Window owner, AndroidBatchResult result)
		{
			using (AndroidBatchResultForm form = new AndroidBatchResultForm(result))
			{
				form.ShowDialog(owner);
			}
		}
	}

}