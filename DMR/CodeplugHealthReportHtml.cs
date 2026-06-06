using System.Collections.Generic;
using System.Text;

namespace DMR
{
	public static class CodeplugHealthReportHtml
	{
		public static string Build(CodeplugHealthSnapshot snap)
		{
			if (snap == null)
			{
				return ForkReportHtml.DocumentStart("Codeplug health") + ForkReportHtml.DocumentEnd();
			}

			StringBuilder html = new StringBuilder();
			html.Append(ForkReportHtml.DocumentStart("Codeplug health"));
			ForkReportHtml.AppendMetricCards(html,
				new[] { snap.Channels.ToString(), "Channels", "" },
				new[] { snap.Digital.ToString(), "Digital", "ok" },
				new[] { snap.Analog.ToString(), "Analog", "warn" },
				new[] { snap.Contacts.ToString(), "Contacts", "" },
				new[] { snap.Zones.ToString(), "Zones", "" },
				new[] { snap.TgLists.ToString(), "TG lists", "" });

			string badgeClass = snap.HasWarning ? "badge-warn" : "badge-ok";
			string badgeText = snap.HasWarning ? "Review warnings" : "Healthy";
			html.Append("<h2>Status <span class=\"badge ").Append(badgeClass).Append("\">")
				.Append(badgeText).Append("</span></h2>");

			if (snap.RelayZero > 0)
			{
				AppendWarningList(html, "Relay = 0", snap.RelayZero, snap.RelayZeroNames,
					"warn", "These channels may show &quot;operation failed&quot; on the phone.");
			}

			if (snap.OrphanCount > 0)
			{
				AppendWarningList(html, "Missing TX contact", snap.OrphanCount, snap.OrphanNames,
					"err", "Digital channels reference a contact that is not in the codeplug.");
			}

			if (snap.DuplicateNameGroups > 0)
			{
				AppendWarningList(html, "Duplicate channel names", snap.DuplicateNameGroups, snap.DuplicateNameLines,
					"warn", "Duplicate names can confuse Android import and zone editing.");
			}

			if (snap.EmptyZones > 0)
			{
				html.Append("<h2>Empty zones <span class=\"badge badge-warn\">").Append(snap.EmptyZones).Append("</span></h2>");
				html.Append("<p class=\"warn\">Zones with no member channels.</p><ul>");
				int shown = 0;
				foreach (CodeplugHealthZoneRow row in snap.ZoneRows)
				{
					if (row.ChannelCount != 0)
					{
						continue;
					}
					html.Append("<li>").Append(ForkReportHtml.Escape(row.Name)).Append("</li>");
					shown++;
					if (shown >= 12)
					{
						break;
					}
				}
				if (snap.EmptyZones > shown)
				{
					html.Append("<li>… and ").Append(snap.EmptyZones - shown).Append(" more</li>");
				}
				html.Append("</ul>");
			}

			if (snap.ChannelsNotInZone > 0)
			{
				AppendWarningList(html, "Channels not in any zone", snap.ChannelsNotInZone, snap.ChannelsNotInZoneNames,
					"warn", "These channels exist but are not assigned to a zone.");
			}

			if (snap.ZoneRows.Count > 0)
			{
				html.Append("<h2>Zone breakdown</h2>");
				html.Append("<table><tr><th>Zone</th><th>Channels</th></tr>");
				int tableRows = 0;
				foreach (CodeplugHealthZoneRow row in snap.ZoneRows)
				{
					string rowClass = row.ChannelCount == 0 ? "miss" : "";
					html.Append("<tr><td class=\"").Append(rowClass).Append("\">")
						.Append(ForkReportHtml.Escape(row.Name)).Append("</td><td>")
						.Append(row.ChannelCount).Append("</td></tr>");
					tableRows++;
					if (tableRows >= 40)
					{
						break;
					}
				}
				if (snap.ZoneRows.Count > tableRows)
				{
					html.Append("<tr><td colspan=\"2\">… and ").Append(snap.ZoneRows.Count - tableRows).Append(" more zones</td></tr>");
				}
				html.Append("</table>");
			}

			if (!snap.HasWarning)
			{
				html.Append("<p class=\"ok\">No relay=0, orphan-contact, duplicate-name, or zone issues detected in the loaded codeplug.</p>");
			}

			html.Append(ForkReportHtml.DocumentEnd());
			return html.ToString();
		}

		public static string Build(
			int channels, int digital, int analog,
			int contacts, int zones, int tgLists,
			int relayZero, List<string> relayZeroNames,
			int orphanCount, List<string> orphanNames)
		{
			CodeplugHealthSnapshot snap = new CodeplugHealthSnapshot
			{
				Channels = channels,
				Digital = digital,
				Analog = analog,
				Contacts = contacts,
				Zones = zones,
				TgLists = tgLists,
				RelayZero = relayZero,
				RelayZeroNames = relayZeroNames ?? new List<string>(),
				OrphanCount = orphanCount,
				OrphanNames = orphanNames ?? new List<string>()
			};
			return Build(snap);
		}

		private static void AppendWarningList(StringBuilder html, string title, int total, List<string> names, string css, string hint)
		{
			string badge = css == "err" ? "badge-err" : "badge-warn";
			html.Append("<h2>").Append(ForkReportHtml.Escape(title)).Append(" <span class=\"badge ").Append(badge).Append("\">")
				.Append(total).Append("</span></h2>");
			if (!string.IsNullOrEmpty(hint))
			{
				html.Append("<p class=\"").Append(css).Append("\">").Append(hint).Append("</p>");
			}
			html.Append("<ul>");
			if (names != null)
			{
				foreach (string name in names)
				{
					html.Append("<li class=\"").Append(css).Append("\">").Append(ForkReportHtml.Escape(name)).Append("</li>");
				}
			}
			if (names == null || total > names.Count)
			{
				html.Append("<li>… and ").Append(total - (names == null ? 0 : names.Count)).Append(" more</li>");
			}
			html.Append("</ul>");
		}
	}
}