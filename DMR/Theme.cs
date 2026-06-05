using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// DMRModHooks-aligned dark chrome for the PriInterPhone fork (UI plan §1.2).
	/// </summary>
	public static class Theme
	{
		public static readonly Color Background = Color.FromArgb(0x0A, 0x15, 0x20);
		public static readonly Color Chrome = Color.FromArgb(0x06, 0x0D, 0x14);
		public static readonly Color Accent = Color.FromArgb(0x1E, 0x3A, 0x5F);
		public static readonly Color Foreground = Color.FromArgb(0xE8, 0xEE, 0xF4);
		public static readonly Color MutedForeground = Color.FromArgb(0xA8, 0xB8, 0xC8);

		public static void ApplyForkChrome(Form form, MenuStrip menu, ToolStrip toolbar, StatusStrip status)
		{
			form.BackColor = Background;
			form.ForeColor = Foreground;
			if (menu != null)
			{
				ApplyMenuStrip(menu);
			}
			if (toolbar != null)
			{
				ApplyToolStrip(toolbar);
			}
			if (status != null)
			{
				ApplyStatusStrip(status);
			}
		}

		private static void ApplyMenuStrip(MenuStrip menu)
		{
			menu.BackColor = Chrome;
			menu.ForeColor = Foreground;
			foreach (ToolStripItem item in menu.Items)
			{
				ApplyToolStripItem(item);
			}
		}

		private static void ApplyToolStrip(ToolStrip strip)
		{
			strip.BackColor = Chrome;
			strip.ForeColor = Foreground;
			foreach (ToolStripItem item in strip.Items)
			{
				ApplyToolStripItem(item);
			}
		}

		private static void ApplyStatusStrip(StatusStrip status)
		{
			status.BackColor = Chrome;
			status.ForeColor = MutedForeground;
			foreach (ToolStripItem item in status.Items)
			{
				item.BackColor = Chrome;
				item.ForeColor = MutedForeground;
			}
		}

		private static void ApplyToolStripItem(ToolStripItem item)
		{
			item.BackColor = Chrome;
			item.ForeColor = Foreground;
			if (item is ToolStripMenuItem menuItem)
			{
				foreach (ToolStripItem child in menuItem.DropDownItems)
				{
					ApplyToolStripItem(child);
				}
			}
		}
	}
}