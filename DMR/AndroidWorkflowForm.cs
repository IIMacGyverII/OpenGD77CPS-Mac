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
			this.ClientSize = new Size(520, 388);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			Label intro = new Label
			{
				Location = new Point(16, 16),
				Size = new Size(488, 228),
				Text =
					"This build is for PriInterPhone / DMRModHooks on Android — not stock Radioddity GD-77.\n\n" +
					"Phone workflow:\n" +
					"1. Export the 5-file backup folder from the radio app (Contacts, TG_Lists, Channels, Zones, DTMF).\n" +
					"2. On PC: use Pull from phone (ADB) if USB debugging is on, or copy the folder to Desktop/Documents (MTP browse in CPS does not work).\n" +
					"3. On PC: Ctrl+Shift+S opens Codeplug Studio (CSV-only) or F8 for Android backup manager.\n" +
					"   Tip: run CodeplugStudio.cmd, drag a folder onto OpenGD77CPS.exe, or create a Desktop shortcut from Studio.\n" +
					"   Validate, Review diff, then Import all (Path B). Or File → Import CSV… / Ctrl+I.\n" +
					"4. Edit channels in the tree; Android-only fields live in the PriInterPhone section on each channel.\n" +
					"5. Export with File → Export CSV… or Ctrl+E. Files are UTF-8 without BOM.\n" +
					"6. F7 opens codeplug health report — click any warning name to jump to that channel, contact, or zone.\n\n" +
					"Do not use the channel list grid Import/Clear buttons for phone backups — those use the 35-column grid format (Path A)."
			};

			this.lnkReleases = new LinkLabel
			{
				Location = new Point(16, 256),
				AutoSize = true,
				Text = "Fork releases on GitHub"
			};
			this.lnkReleases.LinkClicked += (s, e) => OpenUrl("https://github.com/IIMacGyverII/OpenGD77CPS-Mac/releases");

			this.lnkNotes = new LinkLabel
			{
				Location = new Point(16, 280),
				AutoSize = true,
				Text = "Release notes (v" + AboutForm.FORK_VERSION + ")"
			};
			this.lnkNotes.LinkClicked += (s, e) => OpenUrl("https://github.com/IIMacGyverII/OpenGD77CPS-Mac/releases/tag/v" + AboutForm.FORK_VERSION);

			this.lnkPhonedmrapp = new LinkLabel
			{
				Location = new Point(16, 304),
				AutoSize = true,
				Text = "DMRModHooks / phonedmrapp docs"
			};
			this.lnkPhonedmrapp.LinkClicked += (s, e) => OpenUrl("https://github.com/IIMacGyverII/phonedmrapp");

			this.chkDismiss = new CheckBox
			{
				Location = new Point(16, 332),
				AutoSize = true,
				Text = "Do not show this again"
			};

			this.btnOk = new Button
			{
				Location = new Point(412, 350),
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
			ShowWorkflow(owner, true);
#endif
		}

		public static void ShowWorkflow(IWin32Window owner)
		{
#if OpenGD77
			ShowWorkflow(owner, false);
#endif
		}

		private static void ShowWorkflow(IWin32Window owner, bool persistDismiss)
		{
			using (AndroidWorkflowForm dlg = new AndroidWorkflowForm())
			{
				dlg.ShowDialog(owner);
				if (persistDismiss && dlg.chkDismiss.Checked)
				{
					IniFileUtils.WriteProfileString(IniSection, IniKeyDismissed, "yes");
				}
			}
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