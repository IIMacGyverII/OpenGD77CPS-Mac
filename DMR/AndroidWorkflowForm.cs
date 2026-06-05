using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// First-run welcome for the PriInterPhone Android CSV workflow (UI plan §1.1).
	/// </summary>
	public class AndroidWorkflowForm : Form
	{
		private const string IniSection = "Setup";
		private const string IniKeyDismissed = "DismissedAndroidWorkflow";

		private readonly CheckBox chkDismiss;
		private readonly Button btnOk;
		private readonly LinkLabel lnkReleases;
		private readonly LinkLabel lnkNotes;
		private readonly LinkLabel lnkPhonedmrapp;

		public AndroidWorkflowForm()
		{
			this.Text = AboutForm.FORK_NAME;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.StartPosition = FormStartPosition.CenterParent;
			this.ClientSize = new Size(520, 360);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			Label intro = new Label
			{
				Location = new Point(16, 16),
				Size = new Size(488, 200),
				Text =
					"This build is for PriInterPhone / DMRModHooks on Android — not stock Radioddity GD-77.\n\n" +
					"Phone workflow:\n" +
					"1. Export the 5-file backup folder from the radio app (Contacts, TG_Lists, Channels, Zones, DTMF).\n" +
					"2. On PC use File → Import CSV Files… or toolbar Import Android (Ctrl+I). That is Path B (37-column Android headers).\n" +
					"3. Edit channels in the tree; Android-only fields live in the PriInterPhone section on each channel.\n" +
					"4. Export with File → Export CSV… or toolbar Export Android (Ctrl+E). Files are UTF-8 without BOM.\n\n" +
					"Do not use the channel list grid Import/Clear buttons for phone backups — those use the 35-column grid format (Path A)."
			};

			this.lnkReleases = new LinkLabel
			{
				Location = new Point(16, 228),
				AutoSize = true,
				Text = "Fork releases on GitHub"
			};
			this.lnkReleases.LinkClicked += (s, e) => OpenUrl("https://github.com/IIMacGyverII/OpenGD77CPS-Mac/releases");

			this.lnkNotes = new LinkLabel
			{
				Location = new Point(16, 252),
				AutoSize = true,
				Text = "Release notes (v" + AboutForm.FORK_VERSION + ")"
			};
			this.lnkNotes.LinkClicked += (s, e) => OpenUrl("https://github.com/IIMacGyverII/OpenGD77CPS-Mac/blob/main/docs/RELEASE_NOTES_v1.2.0.md");

			this.lnkPhonedmrapp = new LinkLabel
			{
				Location = new Point(16, 276),
				AutoSize = true,
				Text = "DMRModHooks / phonedmrapp docs"
			};
			this.lnkPhonedmrapp.LinkClicked += (s, e) => OpenUrl("https://github.com/IIMacGyverII/phonedmrapp");

			this.chkDismiss = new CheckBox
			{
				Location = new Point(16, 304),
				AutoSize = true,
				Text = "Do not show this again"
			};

			this.btnOk = new Button
			{
				Location = new Point(412, 322),
				Size = new Size(88, 28),
				Text = "OK",
				DialogResult = DialogResult.OK
			};
			this.AcceptButton = this.btnOk;

			this.Controls.Add(intro);
			this.Controls.Add(this.lnkReleases);
			this.Controls.Add(this.lnkNotes);
			this.Controls.Add(this.lnkPhonedmrapp);
			this.Controls.Add(this.chkDismiss);
			this.Controls.Add(this.btnOk);
		}

		public static void ShowIfNeeded(IWin32Window owner)
		{
#if OpenGD77
			if (IniFileUtils.getProfileStringWithDefault(IniSection, IniKeyDismissed, "no") == "yes")
			{
				return;
			}
			using (AndroidWorkflowForm dlg = new AndroidWorkflowForm())
			{
				dlg.ShowDialog(owner);
				if (dlg.chkDismiss.Checked)
				{
					IniFileUtils.WriteProfileString(IniSection, IniKeyDismissed, "yes");
				}
			}
#endif
		}

		private static void OpenUrl(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				MessageBox.Show(url, "Open in browser", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}
}