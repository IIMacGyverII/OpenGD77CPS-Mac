using System.Text;

namespace DMR
{
	/// <summary>Shared dark-theme HTML helpers for fork WebView2 reports.</summary>
	public static class ForkReportHtml
	{
		public static string DocumentStart(string title)
		{
			StringBuilder html = new StringBuilder();
			html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
			html.Append("<style>");
			html.Append("body{font-family:'Segoe UI',sans-serif;background:#0a1520;color:#e8eef4;margin:16px;font-size:13px;line-height:1.5;}");
			html.Append("h1{font-size:18px;margin:0 0 10px;color:#fff;}");
			html.Append("h2{font-size:12px;margin:20px 0 8px;color:#a8b8c8;text-transform:uppercase;letter-spacing:.05em;}");
			html.Append(".path{color:#a8b8c8;font-size:12px;word-break:break-all;margin-bottom:14px;}");
			html.Append(".cards{display:flex;flex-wrap:wrap;gap:10px;margin:12px 0 18px;}");
			html.Append(".card{flex:1 1 120px;min-width:100px;background:#060d14;border:1px solid #1e3a5f;border-radius:6px;padding:10px 12px;}");
			html.Append(".card .num{font-size:22px;font-weight:700;color:#fff;}");
			html.Append(".card .lbl{font-size:11px;color:#a8b8c8;margin-top:2px;}");
			html.Append("table{border-collapse:collapse;width:100%;margin:8px 0 16px;}");
			html.Append("th,td{border:1px solid #1e3a5f;padding:7px 10px;text-align:left;}");
			html.Append("th{background:#060d14;color:#a8b8c8;font-weight:600;}");
			html.Append("tr:nth-child(even){background:#0d1c2a;}");
			html.Append(".ok{color:#81c784;}.warn{color:#ffb74d;}.err{color:#ef5350;}.miss{color:#ef9a9a;}");
			html.Append(".badge{display:inline-block;padding:3px 10px;border-radius:4px;font-size:11px;font-weight:600;}");
			html.Append(".badge-ok{background:#1b5e20;color:#c8e6c9;}.badge-warn{background:#e65100;color:#ffe0b2;}");
			html.Append(".badge-err{background:#b71c1c;color:#ffcdd2;}");
			html.Append("pre{white-space:pre-wrap;background:#060d14;border:1px solid #1e3a5f;padding:10px;border-radius:4px;font-size:12px;}");
			html.Append("ul{margin:6px 0;padding-left:20px;}");
			html.Append("a.drill{color:#7ec8ff;text-decoration:underline;}a.drill:hover{color:#fff;}");
			html.Append(".foot{color:#607080;font-size:11px;margin-top:24px;}");
			html.Append("</style></head><body>");
			html.Append("<h1>").Append(Escape(title)).Append("</h1>");
			return html.ToString();
		}

		public static string DocumentEnd()
		{
			return "<p class=\"foot\">" + Escape(AboutForm.FORK_NAME) + " v" + AboutForm.FORK_VERSION
				+ "</p></body></html>";
		}

		public static string DrillHref(string kind, int dataIndex)
		{
			return "fork://open/" + kind + "/" + dataIndex;
		}

		public static string DrillLink(string kind, int dataIndex, string label)
		{
			return "<a class=\"drill\" href=\"" + DrillHref(kind, dataIndex) + "\">" + Escape(label) + "</a>";
		}

		public static string BackupCsvHref(string fileName)
		{
			return "fork://backup-csv/" + fileName;
		}

		public static string BackupCsvLink(string fileName)
		{
			return "<a class=\"drill\" href=\"" + BackupCsvHref(fileName) + "\">" + Escape(fileName) + "</a>";
		}

		public static string Escape(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return "";
			}
			return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
		}

		public static void AppendMetricCards(StringBuilder html, params string[][] cards)
		{
			html.Append("<div class=\"cards\">");
			foreach (string[] card in cards)
			{
				if (card == null || card.Length < 2)
				{
					continue;
				}
				string css = card.Length > 2 ? card[2] : "";
				html.Append("<div class=\"card\"><div class=\"num ").Append(css).Append("\">")
					.Append(Escape(card[0])).Append("</div><div class=\"lbl\">")
					.Append(Escape(card[1])).Append("</div></div>");
			}
			html.Append("</div>");
		}
	}
}