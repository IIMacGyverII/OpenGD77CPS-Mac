using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	internal static class DmrIdLookup
	{
		private const int AllCallId = 16777215;
		public const string MenuText = "Look up on RadioID.net";
		private const string UrlFormat = "https://www.radioid.net/database/view?id={0}";
		private const string InvalidIdMessage = "Select a contact with a valid DMR ID (not All-call).";

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

		public static bool TryOpenFromText(string text, IWin32Window owner)
		{
			int dmrId;
			if (!TryParseDmrId(text, out dmrId))
			{
				MessageBox.Show(owner, InvalidIdMessage, "DMR ID Lookup", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return false;
			}
			OpenInBrowser(dmrId);
			return true;
		}

		public static void UpdateLookupEnabled(Control lookupControl, string dmrIdText)
		{
			if (lookupControl == null)
			{
				return;
			}
			int dmrId;
			lookupControl.Enabled = TryParseDmrId(dmrIdText, out dmrId);
		}

		public static Button CreateLookupButton(Func<string> getDmrIdText, IWin32Window owner)
		{
			Button button = new Button();
			button.Text = MenuText;
			button.AutoSize = true;
			button.UseVisualStyleBackColor = true;
			button.Click += (s, e) => TryOpenFromText(getDmrIdText(), owner);
			return button;
		}

		public static LinkLabel CreateLookupLink(Func<string> getDmrIdText, IWin32Window owner)
		{
			LinkLabel link = new LinkLabel();
			link.Text = MenuText;
			link.AutoSize = true;
			link.LinkClicked += (s, e) =>
			{
				e.Link.Visited = true;
				TryOpenFromText(getDmrIdText(), owner);
			};
			return link;
		}

		public static void AttachContextMenu(Control control, Func<string> getDmrIdText, IWin32Window owner)
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