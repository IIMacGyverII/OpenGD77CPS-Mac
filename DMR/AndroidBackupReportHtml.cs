using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DMR
{
	/// <summary>
	/// Tier 3.1 spike — HTML validation report for WebView2 panel in Android backup manager.
	/// </summary>
	public static class AndroidBackupReportHtml
	{
		private static readonly string[] BackupFiles = new string[]
		{
			"Contacts.csv",
			"TG_Lists.csv",
			"Channels.csv",
			"Zones.csv",
			"DTMF.csv"
		};

		public static string Build(
			string folderPath,
			AndroidBackupValidationResult validation,
			AndroidImportDiffResult diff,
			AndroidContactIntegrityResult integrity,
			AndroidBatchResult operationResult = null)
		{
			StringBuilder html = new StringBuilder();
			html.Append(ForkReportHtml.DocumentStart("Android backup report"));
			html.Append("<div class=\"path\">").Append(ForkReportHtml.Escape(folderPath ?? "")).Append("</div>");

			if (operationResult != null)
			{
				AppendOperationResult(html, operationResult);
				if (string.Equals(operationResult.Operation, "Import", StringComparison.OrdinalIgnoreCase)
					&& !operationResult.HasErrors)
				{
					CodeplugHealthReportHtml.AppendStudioPostImportSection(html);
				}
			}

			int csvRows = validation != null ? validation.CsvChannelRows : 0;
			int added = diff != null ? diff.Added : 0;
			int changed = diff != null ? diff.Changed : 0;
			int warnCount = integrity != null ? integrity.Warnings.Count : 0;
			ForkReportHtml.AppendMetricCards(html,
				new[] { csvRows.ToString(), "CSV channels", "" },
				new[] { added.ToString(), "To add", "ok" },
				new[] { changed.ToString(), "To change", changed > 0 ? "warn" : "" },
				new[] { warnCount.ToString(), "Warnings", warnCount > 0 ? "warn" : "" });

			html.Append("<h2>Files</h2><table><tr><th>File</th><th>Status</th></tr>");
			if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
			{
				foreach (string file in BackupFiles)
				{
					bool exists = File.Exists(Path.Combine(folderPath, file));
					html.Append("<tr><td>").Append(ForkReportHtml.Escape(file)).Append("</td><td class=\"")
						.Append(exists ? "ok" : "miss").Append("\">")
						.Append(exists ? "Found" : "Missing").Append("</td></tr>");
				}
			}
			else
			{
				html.Append("<tr><td colspan=\"2\" class=\"err\">Folder not found</td></tr>");
			}
			html.Append("</table>");

			if (validation != null)
			{
				string badgeClass = validation.HasBlockingErrors ? "badge-err" : "badge-ok";
				string badgeText = validation.HasBlockingErrors ? "Blocking errors" : "Ready";
				html.Append("<h2>Validation <span class=\"badge ").Append(badgeClass).Append("\">")
					.Append(badgeText).Append("</span></h2>");
				html.Append("<table><tr><th>Metric</th><th>Value</th></tr>");
				AppendMetricRow(html, "CSV channel rows", validation.CsvChannelRows.ToString());
				AppendMetricRow(html, "Loaded channels", validation.LoadedChannelCount.ToString());
				AppendMetricRow(html, "Relay=0 rows", validation.RelayZeroCount.ToString(), validation.RelayZeroCount > 0);
				AppendMetricRow(html, "Duplicate names", validation.DuplicateChannelNames.ToString(), validation.DuplicateChannelNames > 0);
				html.Append("</table>");
				if (!string.IsNullOrEmpty(validation.Summary))
				{
					html.Append("<pre>").Append(ForkReportHtml.Escape(validation.Summary)).Append("</pre>");
				}
			}

			if (diff != null)
			{
				html.Append("<h2>Import diff preview</h2>");
				html.Append("<table><tr><th>Added</th><th>Changed</th><th>Deleted</th><th>Unchanged</th></tr><tr>");
				html.Append("<td class=\"ok\">").Append(diff.Added).Append("</td>");
				html.Append("<td class=\"warn\">").Append(diff.Changed).Append("</td>");
				html.Append("<td class=\"err\">").Append(diff.Deleted).Append("</td>");
				html.Append("<td>").Append(diff.Unchanged).Append("</td></tr></table>");

				if (diff.Rows != null && diff.Rows.Count > 0)
				{
					html.Append("<p class=\"foot\">Click a channel name (blue link) to open it in the editor. New channels appear only after import.</p>");
					html.Append("<table><tr><th>Status</th><th>Channel</th><th>#</th><th>Details</th></tr>");
					int shown = 0;
					int changedRowCount = 0;
					foreach (AndroidImportDiffRow row in diff.Rows)
					{
						if (row.Status != "Unchanged")
						{
							changedRowCount++;
						}
					}
					const int maxRows = 120;
					foreach (AndroidImportDiffRow row in diff.Rows)
					{
						if (row.Status == "Unchanged")
						{
							continue;
						}
						string rowClass = row.Status == "Added" ? "ok" : (row.Status == "Deleted" ? "err" : "warn");
						int channelIndex = AndroidImportDiff.FindLoadedChannelIndexByName(row.ChannelName);
						string channelCell = channelIndex >= 0
							? ForkReportHtml.DrillLink("channel", channelIndex, row.ChannelName)
							: ForkReportHtml.Escape(row.ChannelName);
						html.Append("<tr><td class=\"").Append(rowClass).Append("\">").Append(ForkReportHtml.Escape(row.Status))
							.Append("</td><td>").Append(channelCell)
							.Append("</td><td>").Append(ForkReportHtml.Escape(row.ChannelNumber))
							.Append("</td><td>").Append(ForkReportHtml.Escape(row.Details)).Append("</td></tr>");
						if (++shown >= maxRows)
						{
							break;
						}
					}
					html.Append("</table>");
					if (changedRowCount > shown)
					{
						html.Append("<p class=\"warn\">Showing first ").Append(shown).Append(" of ")
							.Append(changedRowCount).Append(" changed rows. Use <b>Review diff…</b> for the full list.</p>");
					}
				}
				else if (!string.IsNullOrEmpty(diff.Summary))
				{
					html.Append("<pre>").Append(ForkReportHtml.Escape(diff.Summary)).Append("</pre>");
				}
			}

			if (integrity != null)
			{
				string iBadge = integrity.HasWarnings ? "badge-warn" : "badge-ok";
				html.Append("<h2>Integrity <span class=\"badge ").Append(iBadge).Append("\">")
					.Append(integrity.HasWarnings ? "Warnings" : "OK").Append("</span></h2>");
				html.Append("<p>").Append(ForkReportHtml.Escape(integrity.Summary)).Append("</p>");
				if (integrity.HasWarnings && integrity.Warnings != null && integrity.Warnings.Count > 0)
				{
					html.Append("<ul>");
					int shown = 0;
					foreach (string warning in integrity.Warnings)
					{
						html.Append("<li class=\"warn\">").Append(ForkReportHtml.Escape(warning)).Append("</li>");
						if (++shown >= 30)
						{
							break;
						}
					}
					html.Append("</ul>");
					if (integrity.Warnings.Count > shown)
					{
						html.Append("<p class=\"warn\">").Append(integrity.Warnings.Count - shown)
							.Append(" more warning(s) — use Raw log for full list.</p>");
					}
				}
			}

			html.Append(ForkReportHtml.DocumentEnd());
			return html.ToString();
		}

		private static void AppendMetricRow(StringBuilder html, string label, string value, bool warn = false)
		{
			html.Append("<tr><td>").Append(ForkReportHtml.Escape(label)).Append("</td><td class=\"")
				.Append(warn ? "warn" : "").Append("\">").Append(ForkReportHtml.Escape(value)).Append("</td></tr>");
		}

		private static void AppendOperationResult(StringBuilder html, AndroidBatchResult batch)
		{
			string badgeClass = batch.HasErrors ? "badge-err" : (batch.Warnings.Count > 0 ? "badge-warn" : "badge-ok");
			html.Append("<h2>Last operation <span class=\"badge ").Append(badgeClass).Append("\">")
				.Append(ForkReportHtml.Escape(batch.Title)).Append("</span></h2>");

			ForkReportHtml.AppendMetricCards(html,
				new[] { batch.FilesSucceeded.ToString(), "Files OK", batch.HasErrors ? "err" : "ok" },
				new[] { batch.ChannelsCount.ToString(), "Channels", batch.ChannelsCount > 0 ? "ok" : "" },
				new[] { batch.ContactsCount.ToString(), "Contacts", batch.ContactsCount > 0 ? "ok" : "" },
				new[] { batch.ZonesCount.ToString(), "Zones", batch.ZonesCount > 0 ? "ok" : "" });

			if (batch.LogLines.Count > 0)
			{
				html.Append("<table><tr><th>Step</th><th>Result</th></tr>");
				foreach (string line in batch.LogLines)
				{
					if (string.IsNullOrEmpty(line))
					{
						continue;
					}
					string css = line.StartsWith("✗") ? "err" : (line.StartsWith("✓") ? "ok" : "");
					html.Append("<tr><td colspan=\"2\" class=\"").Append(css).Append("\">")
						.Append(ForkReportHtml.Escape(line)).Append("</td></tr>");
				}
				html.Append("</table>");
			}

			if (batch.Warnings.Count > 0)
			{
				html.Append("<h2>Operation warnings</h2><ul>");
				int shown = 0;
				foreach (string warning in batch.Warnings)
				{
					html.Append("<li class=\"warn\">").Append(ForkReportHtml.Escape(warning)).Append("</li>");
					if (++shown >= 20)
					{
						break;
					}
				}
				html.Append("</ul>");
			}

			html.Append("<p class=\"foot\">").Append(ForkReportHtml.Escape(batch.StatsLine)).Append("</p>");
		}

	}
}