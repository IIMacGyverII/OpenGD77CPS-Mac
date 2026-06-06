using System;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Tier 1.4 — brief startup splash with fork branding and GD-77 warning.
	/// </summary>
	internal static class ForkSplashForm
	{
		private const string IniSection = "Setup";
		private const string IniKeyLastVersion = "LastSplashVersion";

		public static void ShowIfNeeded()
		{
#if OpenGD77
			string last = IniFileUtils.getProfileStringWithDefault(IniSection, IniKeyLastVersion, "");
			if (last == AboutForm.FORK_VERSION)
			{
				return;
			}
			using (Form splash = new Form())
			{
				splash.FormBorderStyle = FormBorderStyle.FixedDialog;
				splash.StartPosition = FormStartPosition.CenterScreen;
				splash.ClientSize = new Size(460, 200);
				splash.Text = AboutForm.FORK_NAME;
				splash.Font = Theme.UiFont;
				splash.MaximizeBox = false;
				splash.MinimizeBox = false;
				splash.ControlBox = false;
				Theme.ApplyForkDialog(splash);

				Label title = new Label
				{
					Location = new Point(20, 20),
					Size = new Size(420, 28),
					Text = AboutForm.FORK_NAME + "  v" + AboutForm.FORK_VERSION,
					Font = new Font(Theme.UiFont.FontFamily, 11f, FontStyle.Bold),
					ForeColor = Theme.Foreground
				};
				Label subtitle = new Label
				{
					Location = new Point(20, 52),
					Size = new Size(420, 22),
					Text = "PriInterPhone / DMRModHooks Android CSV workflow",
					ForeColor = Theme.MutedForeground
				};
				Label warning = new Label
				{
					Location = new Point(20, 82),
					Size = new Size(420, 72),
					Text =
						"\u26A0  Do NOT use this build to program a stock Radioddity/Baofeng GD-77.\n" +
						"Relay, timeslot, and contact fixes target the Android database only.",
					ForeColor = Color.FromArgb(0xFF, 0xA0, 0x50),
					Font = new Font(Theme.UiFont.FontFamily, 9.25f, FontStyle.Bold)
				};
				Label hint = new Label
				{
					Location = new Point(20, 168),
					Size = new Size(320, 20),
					Text = "Click anywhere to continue\u2026",
					ForeColor = Theme.MutedForeground
				};
				Button btnContinue = new Button
				{
					Location = new Point(352, 162),
					Size = new Size(88, 28),
					Text = "Continue",
					DialogResult = DialogResult.OK
				};
				splash.AcceptButton = btnContinue;
				splash.Controls.Add(title);
				splash.Controls.Add(subtitle);
				splash.Controls.Add(warning);
				splash.Controls.Add(hint);
				splash.Controls.Add(btnContinue);

				Timer timer = new Timer { Interval = 2800 };
				timer.Tick += (s, e) =>
				{
					timer.Stop();
					if (!splash.IsDisposed)
					{
						splash.DialogResult = DialogResult.OK;
					}
				};
				timer.Start();
				splash.Click += (s, e) => splash.DialogResult = DialogResult.OK;
				foreach (Control child in splash.Controls)
				{
					child.Click += (s, e) => splash.DialogResult = DialogResult.OK;
				}
				splash.ShowDialog();
				timer.Dispose();
			}
			IniFileUtils.WriteProfileString(IniSection, IniKeyLastVersion, AboutForm.FORK_VERSION);
#endif
		}
	}
}