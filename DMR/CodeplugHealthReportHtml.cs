using System.Collections.Generic;
using System.Text;

namespace DMR
{
	public static class CodeplugHealthReportHtml
	{
		public static string Build(
			int channels, int digital, int analog,
			int contacts, int zones, int tgLists,
			int relayZero, List<string> relayZeroNames,
			int orphanCount, List<string> orphanNames)
		{
			StringBuilder html = new StringBuilder();
			html.Append(ForkReportHtml.DocumentStart("Codeplug health"));
			ForkReportHtml.AppendMetricCards(html,
				new[] { channels.ToString(), "Channels", "" },
				new[] { digital.ToString(), "Digital", "ok" },
				new[] { analog.ToString(), "Analog", "warn" },
				new[] { contacts.ToString(), "Contacts", "" },
				new[] { zones.ToString(), "Zones", "" },
				new[] { tgLists.ToString(), "TG lists", "" });

			bool hasWarning = relayZero > 0 || orphanCount > 0;
			string badgeClass = hasWarning ? "badge-warn" : "badge-ok";
			string badgeText = hasWarning ? "Review warnings" : "Healthy";
			html.Append("<h2>Status <span class=\"badge ").Append(badgeClass).Append("\">")
				.Append(badgeText).Append("</span></h2>");

			if (relayZero > 0)
			{
				html.Append("<h2>Relay = 0 <span class=\"badge badge-warn\">").Append(relayZero).Append("</span></h2>");
				html.Append("<p class=\"warn\">These channels may show &quot;operation failed&quot; on the phone.</p><ul>");
				foreach (string name in relayZeroNames)
				{
					html.Append("<li>").Append(ForkReportHtml.Escape(name)).Append("</li>");
				}
				if (relayZero > relayZeroNames.Count)
				{
					html.Append("<li>… and ").Append(relayZero - relayZeroNames.Count).Append(" more</li>");
				}
				html.Append("</ul>");
			}

			if (orphanCount > 0)
			{
				html.Append("<h2>Missing TX contact <span class=\"badge badge-err\">").Append(orphanCount).Append("</span></h2>");
				html.Append("<ul>");
				foreach (string name in orphanNames)
				{
					html.Append("<li class=\"err\">").Append(ForkReportHtml.Escape(name)).Append("</li>");
				}
				if (orphanCount > orphanNames.Count)
				{
					html.Append("<li>… and ").Append(orphanCount - orphanNames.Count).Append(" more</li>");
				}
				html.Append("</ul>");
			}

			if (!hasWarning)
			{
				html.Append("<p class=\"ok\">No relay=0 or orphan-contact issues detected in the loaded codeplug.</p>");
			}

			html.Append(ForkReportHtml.DocumentEnd());
			return html.ToString();
		}
	}
}