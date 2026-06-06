using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DMR
{
	/// <summary>
	/// DMRModHooks-aligned dark chrome for the PriInterPhone fork (UI plan §1.2).
	/// </summary>
	public static class Theme
	{
		/// <summary>Tier 1.2 — default UI font for fork shell and editors.</summary>
		public static readonly Font UiFont = new Font("Segoe UI", 9.75f, FontStyle.Regular);

		public static readonly Font UiFontBold = new Font("Segoe UI", 9.75f, FontStyle.Bold);

		public static readonly Font UiFontSmall = new Font("Segoe UI", 9f, FontStyle.Regular);

		/// <summary>WinForms scale factor from MainForm startup (96 DPI = 1.0).</summary>
		public static SizeF GetDpiScale()
		{
			return Settings.smethod_6();
		}

		/// <summary>Scale a 96-DPI design pixel value to the current display DPI.</summary>
		public static int Dpi(int designPixels)
		{
			return (int)Math.Round(designPixels * GetDpiScale().Width);
		}

		/// <summary>Scale controls added after Form.Scale() (e.g. ChannelForm Android section).</summary>
		public static void ScaleNewControlTree(Control root)
		{
			if (root == null)
			{
				return;
			}
			SizeF factor = GetDpiScale();
			if (Math.Abs(factor.Width - 1f) < 0.02f && Math.Abs(factor.Height - 1f) < 0.02f)
			{
				return;
			}
			root.Scale(factor);
		}

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

		/// <summary>Tier 1.2 — dark MDI document tabs aligned with fork chrome.</summary>
		public static void ApplyDarkDockPanelSkin(DockPanelSkin skin)
		{
			if (skin == null)
			{
				return;
			}

			skin.AutoHideStripSkin.TextFont = UiFontSmall;
			skin.AutoHideStripSkin.DockStripGradient.StartColor = Chrome;
			skin.AutoHideStripSkin.DockStripGradient.EndColor = Chrome;
			skin.AutoHideStripSkin.TabGradient.StartColor = Accent;
			skin.AutoHideStripSkin.TabGradient.EndColor = Background;
			skin.AutoHideStripSkin.TabGradient.TextColor = Foreground;

			DockPaneStripGradient document = skin.DockPaneStripSkin.DocumentGradient;
			document.DockStripGradient.StartColor = Chrome;
			document.DockStripGradient.EndColor = Chrome;
			document.ActiveTabGradient.StartColor = Accent;
			document.ActiveTabGradient.EndColor = Background;
			document.ActiveTabGradient.TextColor = Foreground;
			document.InactiveTabGradient.StartColor = Chrome;
			document.InactiveTabGradient.EndColor = Background;
			document.InactiveTabGradient.TextColor = MutedForeground;
			skin.DockPaneStripSkin.TextFont = UiFontSmall;

			DockPaneStripToolWindowGradient tool = skin.DockPaneStripSkin.ToolWindowGradient;
			tool.DockStripGradient.StartColor = Chrome;
			tool.DockStripGradient.EndColor = Chrome;
			tool.ActiveCaptionGradient.StartColor = Accent;
			tool.ActiveCaptionGradient.EndColor = Background;
			tool.ActiveCaptionGradient.TextColor = Foreground;
			tool.ActiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
			tool.ActiveTabGradient.StartColor = Accent;
			tool.ActiveTabGradient.EndColor = Background;
			tool.ActiveTabGradient.TextColor = Foreground;
			tool.InactiveCaptionGradient.StartColor = Chrome;
			tool.InactiveCaptionGradient.EndColor = Background;
			tool.InactiveCaptionGradient.TextColor = MutedForeground;
			tool.InactiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
			tool.InactiveTabGradient.StartColor = Chrome;
			tool.InactiveTabGradient.EndColor = Background;
			tool.InactiveTabGradient.TextColor = MutedForeground;
		}
	}
}