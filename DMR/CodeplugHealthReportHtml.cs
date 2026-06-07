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
			html.Append("<p class=\"path\">Click a highlighted name to open that channel, contact, zone, TG/Rx list, or scan list in the editor. Counts refresh automatically as you fix issues.</p>");
			ForkReportHtml.AppendMetricCards(html,
				new[] { snap.Channels.ToString(), "Channels", "" },
				new[] { snap.Digital.ToString(), "Digital", "ok" },
				new[] { snap.Analog.ToString(), "Analog", "warn" },
				new[] { snap.Contacts.ToString(), "Contacts", "" },
				new[] { snap.Zones.ToString(), "Zones", "" },
				new[] { snap.TgLists.ToString(), "TG lists", "" },
				new[] { snap.ScanLists.ToString(), "Scan lists", "" });

			string badgeClass = snap.HasWarning ? "badge-warn" : "badge-ok";
			string badgeText = snap.HasWarning ? "Review warnings" : "Healthy";
			html.Append("<h2>Status <span class=\"badge ").Append(badgeClass).Append("\">")
				.Append(badgeText).Append("</span></h2>");

			if (snap.RelayZero > 0)
			{
				AppendDrillList(html, "Relay = 0", snap.RelayZero, snap.RelayZeroDrill, "channel",
					"warn", "These channels may show &quot;operation failed&quot; on the phone.", "health-relay-zero");
			}

			if (snap.OrphanCount > 0)
			{
				AppendDrillList(html, "Missing TX contact", snap.OrphanCount, snap.OrphanDrill, "channel",
					"err", "Digital channels reference a contact that is not in the codeplug.", "health-orphan-contact");
			}

			if (snap.DuplicateNameGroups > 0)
			{
				AppendDuplicateGroupList(html, "Duplicate channel names", snap.DuplicateNameGroups,
					snap.DuplicateChannelGroups, "channel",
					"warn", "Duplicate names can confuse Android import and zone editing.", "health-dup-ch-names");
			}

			if (snap.DuplicateDmrIdGroups > 0)
			{
				AppendDrillList(html, "Duplicate contact DMR IDs", snap.DuplicateDmrIdGroups, snap.DuplicateDmrIdDrill, "contact",
					"warn", "Multiple contacts share the same Call ID — phone lookup and TX routing may be ambiguous.", "health-dup-dmr-id");
			}

			if (snap.DuplicateContactNameGroups > 0)
			{
				AppendDuplicateGroupList(html, "Duplicate contact names", snap.DuplicateContactNameGroups,
					snap.DuplicateContactGroups, "contact",
					"warn", "Multiple contacts share the same name — grids and imports may be hard to distinguish.", "health-dup-ct-names");
			}

			if (snap.DigitalNoContact > 0)
			{
				AppendDrillList(html, "Digital channels without TX contact", snap.DigitalNoContact, snap.DigitalNoContactDrill, "channel",
					"warn", "Digital channels should reference a contact for TX routing.", "health-dig-no-contact");
			}

			if (snap.EmptyZones > 0)
			{
				html.Append("<h2 id=\"health-empty-zones\">Empty zones <span class=\"badge badge-warn\">").Append(snap.EmptyZones).Append("</span></h2>");
				html.Append("<p class=\"warn\">Zones with no member channels.</p><ul>");
				int shown = 0;
				foreach (CodeplugHealthZoneRow row in snap.ZoneRows)
				{
					if (row.ChannelCount != 0)
					{
						continue;
					}
					html.Append("<li>").Append(ForkReportHtml.DrillLink("zone", row.ZoneIndex, row.Name)).Append("</li>");
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
				AppendDrillList(html, "Channels not in any zone", snap.ChannelsNotInZone, snap.ChannelsNotInZoneDrill, "channel",
					"warn", "These channels exist but are not assigned to a zone.", "health-no-zone");
			}

			if (snap.EmptyTgLists > 0)
			{
				AppendDrillList(html, "Empty TG/Rx lists", snap.EmptyTgLists, snap.EmptyTgDrill, "tglist",
					"warn", "TG/Rx group lists with no member contacts.", "health-empty-tg");
			}

			if (snap.InvalidTgRefs > 0)
			{
				AppendDrillList(html, "Invalid TG/Rx contact refs", snap.InvalidTgRefs, snap.InvalidTgRefDrill, "tglist",
					"warn", "TG/Rx lists reference missing contacts or non-group (private/all-call) entries.", "health-bad-tg-ref");
			}

			if (snap.TgRows.Count > 0)
			{
				html.Append("<h2>TG/Rx list breakdown</h2>");
				html.Append("<table><tr><th>TG/Rx list</th><th>Contacts</th><th>Bad refs</th></tr>");
				int tgTableRows = 0;
				foreach (CodeplugHealthTgRow row in snap.TgRows)
				{
					string rowClass = row.ContactCount == 0 ? "miss" : (row.InvalidRefCount > 0 ? "warn" : "");
					html.Append("<tr><td class=\"").Append(rowClass).Append("\">")
						.Append(ForkReportHtml.DrillLink("tglist", row.TgIndex, row.Name)).Append("</td><td>")
						.Append(row.ContactCount).Append("</td><td>")
						.Append(row.InvalidRefCount > 0 ? row.InvalidRefCount.ToString() : "—").Append("</td></tr>");
					tgTableRows++;
					if (tgTableRows >= 40)
					{
						break;
					}
				}
				if (snap.TgRows.Count > tgTableRows)
				{
					html.Append("<tr><td colspan=\"3\">… and ").Append(snap.TgRows.Count - tgTableRows).Append(" more TG/Rx lists</td></tr>");
				}
				html.Append("</table>");
			}

			if (snap.EmptyScanLists > 0)
			{
				AppendDrillList(html, "Empty scan lists", snap.EmptyScanLists, snap.EmptyScanDrill, "scanlist",
					"warn", "Scan lists with no member channels.", "health-empty-scan");
			}

			if (snap.InvalidScanRefs > 0)
			{
				AppendDrillList(html, "Invalid scan channel refs", snap.InvalidScanRefs, snap.InvalidScanRefDrill, "scanlist",
					"warn", "Scan lists reference channels that are missing or invalid in the codeplug.", "health-bad-scan-ref");
			}

			if (snap.ScanRows.Count > 0)
			{
				html.Append("<h2>Scan list breakdown</h2>");
				html.Append("<table><tr><th>Scan list</th><th>Channels</th><th>Bad refs</th></tr>");
				int scanTableRows = 0;
				foreach (CodeplugHealthScanRow row in snap.ScanRows)
				{
					string rowClass = row.ChannelCount == 0 ? "miss" : (row.InvalidRefCount > 0 ? "warn" : "");
					html.Append("<tr><td class=\"").Append(rowClass).Append("\">")
						.Append(ForkReportHtml.DrillLink("scanlist", row.ScanIndex, row.Name)).Append("</td><td>")
						.Append(row.ChannelCount).Append("</td><td>")
						.Append(row.InvalidRefCount > 0 ? row.InvalidRefCount.ToString() : "—").Append("</td></tr>");
					scanTableRows++;
					if (scanTableRows >= 40)
					{
						break;
					}
				}
				if (snap.ScanRows.Count > scanTableRows)
				{
					html.Append("<tr><td colspan=\"3\">… and ").Append(snap.ScanRows.Count - scanTableRows).Append(" more scan lists</td></tr>");
				}
				html.Append("</table>");
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
						.Append(ForkReportHtml.DrillLink("zone", row.ZoneIndex, row.Name)).Append("</td><td>")
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
				html.Append("<p class=\"ok\">No relay=0, orphan-contact, duplicate channel/contact names, duplicate DMR ID, digital-no-contact, zone, TG/Rx-list, or scan-list issues detected in the loaded codeplug.</p>");
			}

			html.Append(ForkReportHtml.DocumentEnd());
			return html.ToString();
		}

		public static string GetScrollTarget(CodeplugHealthSnapshot snap)
		{
			if (snap == null || !snap.HasWarning)
			{
				return null;
			}
			if (snap.RelayZero > 0)
			{
				return "health-relay-zero";
			}
			if (snap.OrphanCount > 0)
			{
				return "health-orphan-contact";
			}
			if (snap.DuplicateNameGroups > 0)
			{
				return "health-dup-ch-names";
			}
			if (snap.DuplicateDmrIdGroups > 0)
			{
				return "health-dup-dmr-id";
			}
			if (snap.DuplicateContactNameGroups > 0)
			{
				return "health-dup-ct-names";
			}
			if (snap.DigitalNoContact > 0)
			{
				return "health-dig-no-contact";
			}
			if (snap.EmptyZones > 0)
			{
				return "health-empty-zones";
			}
			if (snap.ChannelsNotInZone > 0)
			{
				return "health-no-zone";
			}
			if (snap.EmptyTgLists > 0)
			{
				return "health-empty-tg";
			}
			if (snap.InvalidTgRefs > 0)
			{
				return "health-bad-tg-ref";
			}
			if (snap.EmptyScanLists > 0)
			{
				return "health-empty-scan";
			}
			if (snap.InvalidScanRefs > 0)
			{
				return "health-bad-scan-ref";
			}
			return null;
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

		/// <summary>Compact post-import health block for Codeplug Studio WebView2 report.</summary>
		public static void AppendStudioPostImportSection(StringBuilder html)
		{
			CodeplugHealthSnapshot snap = CodeplugHealthSnapshot.Collect();
			const int maxItems = 8;
			string badgeClass = snap.HasWarning ? "badge-warn" : "badge-ok";
			string badgeText = snap.HasWarning ? "Review warnings" : "Healthy";
			html.Append("<h2 id=\"studio-post-import-health\">Loaded codeplug health <span class=\"badge ").Append(badgeClass).Append("\">")
				.Append(badgeText).Append("</span></h2>");

			ForkReportHtml.AppendMetricCards(html,
				new[] { snap.Channels.ToString(), "Channels", "" },
				new[] { snap.Digital.ToString(), "Digital", "ok" },
				new[] { snap.Analog.ToString(), "Analog", "warn" },
				new[] { snap.Contacts.ToString(), "Contacts", "" },
				new[] { snap.Zones.ToString(), "Zones", "" });

			if (snap.RelayZero > 0)
			{
				AppendDrillList(html, "Relay = 0", snap.RelayZero, snap.RelayZeroDrill, "channel",
					"warn", "These channels may show operation failed on the phone.", "health-relay-zero", maxItems);
			}
			if (snap.OrphanCount > 0)
			{
				AppendDrillList(html, "Missing TX contact", snap.OrphanCount, snap.OrphanDrill, "channel",
					"err", "Digital channels reference a contact not in the codeplug.", "health-orphan-contact", maxItems);
			}
			if (snap.DuplicateNameGroups > 0)
			{
				AppendDuplicateGroupList(html, "Duplicate channel names", snap.DuplicateNameGroups,
					snap.DuplicateChannelGroups, "channel", "warn",
					"Duplicate names can confuse Android import and zone editing.", "health-dup-ch-names", maxItems);
			}
			if (snap.DuplicateDmrIdGroups > 0)
			{
				AppendDrillList(html, "Duplicate contact DMR IDs", snap.DuplicateDmrIdGroups, snap.DuplicateDmrIdDrill, "contact",
					"warn", "Multiple contacts share the same Call ID.", "health-dup-dmr-id", maxItems);
			}
			if (snap.DigitalNoContact > 0)
			{
				AppendDrillList(html, "Digital without TX contact", snap.DigitalNoContact, snap.DigitalNoContactDrill, "channel",
					"warn", "Digital channels should reference a contact for TX routing.", "health-dig-no-contact", maxItems);
			}
			if (snap.ChannelsNotInZone > 0)
			{
				AppendDrillList(html, "Channels not in any zone", snap.ChannelsNotInZone, snap.ChannelsNotInZoneDrill, "channel",
					"warn", "These channels are not assigned to a zone.", "health-no-zone", maxItems);
			}
			if (!snap.HasWarning)
			{
				html.Append("<p class=\"ok\">No codeplug health issues detected after import.</p>");
				html.Append("<p class=\"foot\">").Append(ForkPostImportUi.PostImportReportFootOk).Append("</p>");
				return;
			}

			html.Append("<p class=\"warn\">").Append(ForkReportHtml.HealthReportLink(ForkPostImportUi.PostImportReportHealthLink))
				.Append(" — also amber status chip, Health ⚠ footer, or press F7.</p>");
			html.Append("<p class=\"foot\">").Append(ForkPostImportUi.PostImportReportFootWarn).Append("</p>");
		}

		private static void AppendDrillList(StringBuilder html, string title, int total,
			List<CodeplugHealthDrillItem> items, string kind, string css, string hint,
			string elementId = null, int maxItems = int.MaxValue)
		{
			string badge = css == "err" ? "badge-err" : "badge-warn";
			html.Append("<h2");
			if (!string.IsNullOrEmpty(elementId))
			{
				html.Append(" id=\"").Append(elementId).Append("\"");
			}
			html.Append(">").Append(ForkReportHtml.Escape(title)).Append(" <span class=\"badge ").Append(badge).Append("\">")
				.Append(total).Append("</span></h2>");
			if (!string.IsNullOrEmpty(hint))
			{
				html.Append("<p class=\"").Append(css).Append("\">").Append(hint).Append("</p>");
			}
			html.Append("<ul>");
			int shown = 0;
			if (items != null)
			{
				foreach (CodeplugHealthDrillItem item in items)
				{
					if (shown >= maxItems)
					{
						break;
					}
					html.Append("<li class=\"").Append(css).Append("\">")
						.Append(ForkReportHtml.DrillLink(kind, item.Index, item.Name)).Append("</li>");
					shown++;
				}
			}
			if (total > shown)
			{
				html.Append("<li>… and ").Append(total - shown).Append(" more — press F7</li>");
			}
			html.Append("</ul>");
		}

		private static void AppendDuplicateGroupList(StringBuilder html, string title, int total,
			List<CodeplugHealthDuplicateGroup> groups, string kind, string css, string hint,
			string elementId = null, int maxGroups = int.MaxValue)
		{
			string badge = css == "err" ? "badge-err" : "badge-warn";
			html.Append("<h2");
			if (!string.IsNullOrEmpty(elementId))
			{
				html.Append(" id=\"").Append(elementId).Append("\"");
			}
			html.Append(">").Append(ForkReportHtml.Escape(title)).Append(" <span class=\"badge ").Append(badge).Append("\">")
				.Append(total).Append("</span></h2>");
			if (!string.IsNullOrEmpty(hint))
			{
				html.Append("<p class=\"").Append(css).Append("\">").Append(hint).Append("</p>");
			}
			html.Append("<ul>");
			int shown = 0;
			if (groups != null)
			{
				foreach (CodeplugHealthDuplicateGroup group in groups)
				{
					if (shown >= maxGroups)
					{
						break;
					}
					html.Append("<li class=\"").Append(css).Append("\">")
						.Append(ForkReportHtml.Escape(group.Name)).Append(" (×").Append(group.Indices.Count).Append("): ");
					for (int i = 0; i < group.Indices.Count; i++)
					{
						if (i > 0)
						{
							html.Append(", ");
						}
						string label = "#" + (group.Indices[i] + 1);
						html.Append(ForkReportHtml.DrillLink(kind, group.Indices[i], label));
					}
					html.Append("</li>");
					shown++;
				}
			}
			if (total > shown)
			{
				html.Append("<li>… and ").Append(total - shown).Append(" more — press F7</li>");
			}
			html.Append("</ul>");
		}
	}
}