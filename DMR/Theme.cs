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

		/// <summary>
		/// Main shell only (menu, toolbar, status). Does not set Form.ForeColor — MDI/dock
		/// children inherit it and channel labels become invisible on light panels.
		/// </summary>
		public static void ApplyForkChrome(Form form, MenuStrip menu, ToolStrip toolbar, StatusStrip status)
		{
			if (form != null)
			{
				form.BackColor = Background;
			}
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

		/// <summary>Small fork dialogs (welcome, Android backup) with dark background.</summary>
		public static void ApplyForkDialog(Form form)
		{
			if (form == null)
			{
				return;
			}
			form.BackColor = Background;
			form.ForeColor = Foreground;
		}

		/// <summary>Restore readable labels on decompiled editor forms (light panel + dark text).</summary>
		public static void ApplyStandardEditorColors(Control root)
		{
			if (root == null)
			{
				return;
			}
			if (root is Label || root is CheckBox || root is RadioButton)
			{
				root.ForeColor = SystemColors.ControlText;
			}
			else if (root is GroupBox)
			{
				root.ForeColor = SystemColors.ControlText;
			}
			foreach (Control child in root.Controls)
			{
				ApplyStandardEditorColors(child);
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