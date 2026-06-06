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
			AndroidContactIntegrityResult integrity)
		{
			StringBuilder html = new StringBuilder();
			html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
			html.Append("<style>");
			html.Append("body{font-family:'Segoe UI',sans-serif;background:#0a1520;color:#e8eef4;margin:12px;font-size:13px;line-height:1.45;}");
			html.Append("h1{font-size:16px;margin:0 0 8px;color:#fff;}");
			html.Append("h2{font-size:13px;margin:18px 0 6px;color:#a8b8c8;text-transform:uppercase;letter-spacing:.04em;}");
			html.Append(".path{color:#a8b8c8;font-size:12px;word-break:break-all;margin-bottom:12px;}");
			html.Append("table{border-collapse:collapse;width:100%;margin:6px 0 14px;}");
			html.Append("th,td{border:1px solid #1e3a5f;padding:6px 8px;text-align:left;}");
			html.Append("th{background:#060d14;color:#a8b8c8;font-weight:600;}");
			html.Append("tr:nth-child(even){background:#0d1c2a;}");
			html.Append(".ok{color:#81c784;}.warn{color:#ffb74d;}.err{color:#ef5350;}.miss{color:#ef9a9a;}");
			html.Append(".badge{display:inline-block;padding:2px 8px;border-radius:4px;font-size:11px;font-weight:600;}");
			html.Append(".badge-ok{background:#1b5e20;color:#c8e6c9;}.badge-warn{background:#e65100;color:#ffe0b2;}");
			html.Append(".badge-err{background:#b71c1c;color:#ffcdd2;}");
			html.Append("pre{white-space:pre-wrap;background:#060d14;border:1px solid #1e3a5f;padding:10px;border-radius:4px;font-size:12px;}");
			html.Append("ul{margin:4px 0;padding-left:18px;}");
			html.Append("</style></head><body>");

			html.Append("<h1>Android backup report</h1>");
			html.Append("<div class=\"path\">").Append(Escape(folderPath ?? "")).Append("</div>");

			html.Append("<h2>Files</h2><table><tr><th>File</th><th>Status</th></tr>");
			if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
			{
				foreach (string file in BackupFiles)
				{
					bool exists = File.Exists(Path.Combine(folderPath, file));
					html.Append("<tr><td>").Append(Escape(file)).Append("</td><td class=\"")
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
					html.Append("<pre>").Append(Escape(validation.Summary)).Append("</pre>");
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
					html.Append("<table><tr><th>Status</th><th>Channel</th><th>#</th><th>Details</th></tr>");
					int shown = 0;
					foreach (AndroidImportDiffRow row in diff.Rows)
					{
						if (row.Status == "Unchanged")
						{
							continue;
						}
						string rowClass = row.Status == "Added" ? "ok" : (row.Status == "Deleted" ? "err" : "warn");
						html.Append("<tr><td class=\"").Append(rowClass).Append("\">").Append(Escape(row.Status))
							.Append("</td><td>").Append(Escape(row.ChannelName))
							.Append("</td><td>").Append(Escape(row.ChannelNumber))
							.Append("</td><td>").Append(Escape(row.Details)).Append("</td></tr>");
						if (++shown >= 40)
						{
							break;
						}
					}
					html.Append("</table>");
					if (diff.Rows.Count > shown)
					{
						html.Append("<p class=\"warn\">Showing first ").Append(shown).Append(" changed rows only.</p>");
					}
				}
				else if (!string.IsNullOrEmpty(diff.Summary))
				{
					html.Append("<pre>").Append(Escape(diff.Summary)).Append("</pre>");
				}
			}

			if (integrity != null)
			{
				string iBadge = integrity.HasWarnings ? "badge-warn" : "badge-ok";
				html.Append("<h2>Integrity <span class=\"badge ").Append(iBadge).Append("\">")
					.Append(integrity.HasWarnings ? "Warnings" : "OK").Append("</span></h2>");
				html.Append("<p>").Append(Escape(integrity.Summary)).Append("</p>");
				if (integrity.HasWarnings && integrity.Warnings != null && integrity.Warnings.Count > 0)
				{
					html.Append("<ul>");
					int shown = 0;
					foreach (string warning in integrity.Warnings)
					{
						html.Append("<li class=\"warn\">").Append(Escape(warning)).Append("</li>");
						if (++shown >= 30)
						{
							break;
						}
					}
					html.Append("</ul>");
					if (integrity.Warnings.Count > shown)
					{
						html.Append("<p class=\"warn\">").Append(integrity.Warnings.Count - shown)
							.Append(" more warning(s) — see Log tab.</p>");
					}
				}
			}

			html.Append("<p style=\"color:#607080;font-size:11px;margin-top:20px;\">")
				.Append(AboutForm.FORK_NAME).Append(" v").Append(AboutForm.FORK_VERSION)
				.Append(" — Path B Android backup</p>");
			html.Append("</body></html>");
			return html.ToString();
		}

		private static void AppendMetricRow(StringBuilder html, string label, string value, bool warn = false)
		{
			html.Append("<tr><td>").Append(Escape(label)).Append("</td><td class=\"")
				.Append(warn ? "warn" : "").Append("\">").Append(Escape(value)).Append("</td></tr>");
		}

		private static string Escape(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return "";
			}
			return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
		}
	}
}