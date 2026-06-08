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
			AndroidBatchResult operationResult = null,
			bool diffPreApproved = false)
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
					string fileCell = exists ? ForkReportHtml.BackupCsvLink(file) : ForkReportHtml.Escape(file);
					html.Append("<tr><td>").Append(fileCell).Append("</td><td class=\"")
						.Append(exists ? "ok" : "miss").Append("\">")
						.Append(exists ? "Found" : "Missing").Append("</td></tr>");
				}
			}
			else
			{
				html.Append("<tr><td colspan=\"2\" class=\"err\">Folder not found</td></tr>");
			}
			html.Append("</table>");
			html.Append("<p class=\"foot\">Click a filename to open it from the loaded backup folder.</p>");

			if (validation != null)
			{
				string badgeClass = validation.HasBlockingErrors ? "badge-err" : "badge-ok";
				string badgeText = validation.HasBlockingErrors ? "Blocking errors" : "Ready";
				html.Append("<h2 id=\"studio-validation\">Validation <span class=\"badge ").Append(badgeClass).Append("\">")
					.Append(badgeText).Append("</span></h2>");
				html.Append("<table><tr><th>Metric</th><th>Value</th></tr>");
				AppendMetricRow(html, "CSV channel rows", validation.CsvChannelRows.ToString());
				AppendMetricRow(html, "Loaded channels", validation.LoadedChannelCount.ToString());
				AppendMetricRow(html, "Relay=0 rows", validation.RelayZeroCount.ToString(), validation.RelayZeroCount > 0);
				AppendMetricRow(html, "Duplicate names", validation.DuplicateChannelNames.ToString(), validation.DuplicateChannelNames > 0);
				html.Append("</table>");
				if (validation.RelayZeroChannelNameList != null && validation.RelayZeroChannelNameList.Count > 0)
				{
					html.Append("<p class=\"warn\">Channels with relay=0 in CSV (coerced to 2 on CPS import):</p><ul>");
					foreach (string name in validation.RelayZeroChannelNameList)
					{
						int channelIndex = AndroidImportDiff.FindLoadedChannelIndexByName(name);
						string nameCell = channelIndex >= 0
							? ForkReportHtml.DrillLink("channel", channelIndex, name)
							: ForkReportHtml.Escape(name);
						html.Append("<li class=\"warn\">").Append(nameCell).Append("</li>");
					}
					html.Append("</ul>");
				}
				if (validation.DuplicateChannelNameList != null && validation.DuplicateChannelNameList.Count > 0)
				{
					html.Append("<p class=\"warn\">Duplicate channel names in CSV (").Append(validation.DuplicateChannelNames)
						.Append(" duplicate row(s)):</p><ul>");
					foreach (string name in validation.DuplicateChannelNameList)
					{
						int channelIndex = AndroidImportDiff.FindLoadedChannelIndexByName(name);
						string nameCell = channelIndex >= 0
							? ForkReportHtml.DrillLink("channel", channelIndex, name)
							: ForkReportHtml.Escape(name);
						html.Append("<li class=\"warn\">").Append(nameCell).Append("</li>");
					}
					html.Append("</ul>");
				}
				if (!string.IsNullOrEmpty(validation.Summary))
				{
					html.Append("<pre>").Append(ForkReportHtml.Escape(validation.Summary)).Append("</pre>");
				}
			}

			if (diff != null)
			{
				bool pendingDiff = ForkPostImportUi.ShouldOfferDiffLink(diff, diffPreApproved, true);
				html.Append("<h2 id=\"studio-import-diff\">Import diff preview");
				if (pendingDiff)
				{
					html.Append(" <span class=\"badge badge-warn\">Review required</span>");
				}
				html.Append("</h2>");
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
				if (pendingDiff)
				{
					html.Append("<p class=\"warn\">").Append(ForkReportHtml.ReviewDiffLink(ForkPostImportUi.PreImportReportDiffLink))
						.Append(" — also Review diff ⚠ / Import ⚠ / Export ⚠ footer/toolbar/menu, or center/left status bar.</p>");
					html.Append("<p class=\"foot\">").Append(ForkPostImportUi.PreImportReportFootWarn).Append("</p>");
				}
			}

			if (integrity != null)
			{
				string iBadge = integrity.HasWarnings ? "badge-warn" : "badge-ok";
				html.Append("<h2 id=\"studio-integrity\">Integrity <span class=\"badge ").Append(iBadge).Append("\">")
					.Append(integrity.HasWarnings ? "Warnings" : "OK").Append("</span></h2>");
				html.Append("<p>").Append(ForkReportHtml.Escape(integrity.Summary)).Append("</p>");
				if (integrity.HasWarnings && integrity.Warnings != null && integrity.Warnings.Count > 0)
				{
					html.Append("<p class=\"foot\">Click a highlighted channel or DMR ID link to open it in the loaded codeplug editor.</p>");
					html.Append("<ul>");
					int shown = 0;
					foreach (string warning in integrity.Warnings)
					{
						html.Append("<li class=\"warn\">").Append(FormatIntegrityWarning(warning)).Append("</li>");
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

		private static string FormatIntegrityWarning(string warning)
		{
			if (string.IsNullOrEmpty(warning))
			{
				return "";
			}
			int colon = warning.IndexOf(':');
			if (colon <= 0)
			{
				return ForkReportHtml.Escape(warning);
			}
			string channelName = warning.Substring(0, colon).Trim();
			if (string.IsNullOrEmpty(channelName))
			{
				return ForkReportHtml.Escape(warning);
			}
			int channelIndex = AndroidImportDiff.FindLoadedChannelIndexByName(channelName);
			string tail = warning.Substring(colon);
			if (channelIndex < 0)
			{
				return LinkDmrIdsInText(warning);
			}
			return ForkReportHtml.DrillLink("channel", channelIndex, channelName) + LinkDmrIdsInText(tail);
		}

		private static string LinkDmrIdsInText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return "";
			}
			const string marker = "DMR ID ";
			StringBuilder html = new StringBuilder();
			int pos = 0;
			while (pos < text.Length)
			{
				int idx = text.IndexOf(marker, pos, StringComparison.OrdinalIgnoreCase);
				if (idx < 0)
				{
					html.Append(ForkReportHtml.Escape(text.Substring(pos)));
					break;
				}
				html.Append(ForkReportHtml.Escape(text.Substring(pos, idx - pos)));
				html.Append("DMR ID ");
				int idStart = idx + marker.Length;
				int idEnd = idStart;
				while (idEnd < text.Length && char.IsDigit(text[idEnd]))
				{
					idEnd++;
				}
				string idStr = text.Substring(idStart, idEnd - idStart);
				int contactIndex = AndroidImportDiff.FindContactIndexByCallId(idStr);
				if (contactIndex >= 0)
				{
					html.Append(ForkReportHtml.DrillLink("contact", contactIndex, idStr));
				}
				else
				{
					html.Append(ForkReportHtml.Escape(idStr));
				}
				pos = idEnd;
			}
			return html.ToString();
		}

		public static string GetFolderStatusSummary(
			AndroidBackupValidationResult validation,
			AndroidContactIntegrityResult integrity,
			AndroidImportDiffResult diff,
			bool diffPreApproved,
			bool hasChannelsCsv)
		{
			if (validation != null && validation.HasBlockingErrors)
			{
				return "Blocking errors — fix before import";
			}
			if (integrity != null && integrity.HasWarnings)
			{
				return "Warnings — review before import";
			}
			if (diff != null && !diffPreApproved && AndroidImportDiff.HasPendingDiffChanges(diff))
			{
				int pending = diff.Added + diff.Changed + diff.Deleted;
				return pending + " channel change(s) — Review diff… (Ctrl+D)";
			}
			if (diffPreApproved && hasChannelsCsv && diff != null && !AndroidImportDiff.HasPendingDiffChanges(diff))
			{
				return "No channel changes — ready to import";
			}
			if (diffPreApproved && hasChannelsCsv)
			{
				return "Diff reviewed ✓ — ready to import";
			}
			return "Ready to import";
		}

		private static void AppendMetricRow(StringBuilder html, string label, string value, bool warn = false)
		{
			html.Append("<tr><td>").Append(ForkReportHtml.Escape(label)).Append("</td><td class=\"")
				.Append(warn ? "warn" : "").Append("\">").Append(ForkReportHtml.Escape(value)).Append("</td></tr>");
		}

		private static void AppendOperationResult(StringBuilder html, AndroidBatchResult batch)
		{
			string badgeClass = batch.HasErrors ? "badge-err" : (batch.Warnings.Count > 0 ? "badge-warn" : "badge-ok");
			html.Append("<h2 id=\"studio-last-operation\">Last operation <span class=\"badge ").Append(badgeClass).Append("\">")
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

		public static string GetReportScrollTarget(
			AndroidBatchResult operationResult,
			AndroidImportDiffResult diff,
			AndroidContactIntegrityResult integrity,
			AndroidBackupValidationResult validation,
			bool diffPreApproved)
		{
			if (operationResult != null
				&& string.Equals(operationResult.Operation, "Import", StringComparison.OrdinalIgnoreCase)
				&& !operationResult.HasErrors)
			{
				string healthTarget = CodeplugHealthReportHtml.GetScrollTarget(CodeplugHealthSnapshot.Collect());
				return healthTarget ?? "studio-post-import-health";
			}
			if (operationResult != null
				&& string.Equals(operationResult.Operation, "Export", StringComparison.OrdinalIgnoreCase)
				&& !operationResult.HasErrors)
			{
				return "studio-last-operation";
			}
			if (diff != null && !diffPreApproved && AndroidImportDiff.HasPendingDiffChanges(diff))
			{
				return "studio-import-diff";
			}
			if (integrity != null && integrity.HasWarnings)
			{
				return "studio-integrity";
			}
			if (validation != null && (validation.RelayZeroCount > 0 || validation.DuplicateChannelNames > 0))
			{
				return "studio-validation";
			}
			return null;
		}

	}
}