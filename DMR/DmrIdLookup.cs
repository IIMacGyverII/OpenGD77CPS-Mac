using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DMR
{
	internal static class DmrIdLookup
	{
		private const int AllCallId = 16777215;
		private const string MenuText = "Look up on RadioID.net";
		private const string UrlFormat = "https://www.radioid.net/database/view?id={0}";

		public static bool TryParseDmrId(string text, out int dmrId)
		{
			dmrId = 0;
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			if (!int.TryParse(text.Trim(), out dmrId))
			{
				return false;
			}
			return dmrId >= 1 && dmrId <= 16776415 && dmrId != AllCallId;
		}

		public static void OpenInBrowser(int dmrId)
		{
			try
			{
				Process.Start(string.Format(UrlFormat, dmrId));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Could not open browser: " + ex.Message, "DMR ID Lookup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		public static void AttachContextMenu(Control control, Func<string> getDmrIdText)
		{
			ContextMenuStrip menu = new ContextMenuStrip();
			menu.Opening += (sender, e) =>
			{
				menu.Items.Clear();
				string text;
				int dmrId;
				try
				{
					text = getDmrIdText();
				}
				catch
				{
					e.Cancel = true;
					return;
				}
				if (!TryParseDmrId(text, out dmrId))
				{
					e.Cancel = true;
					return;
				}
				menu.Items.Add(MenuText, null, (s, args) => OpenInBrowser(dmrId));
			};
			control.ContextMenuStrip = menu;
		}
	}
}