using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Android backup folder UI (UI plan Tier 2.2) — Path B import/export + pre-import validation.
	/// </summary>
	public class AndroidBackupForm : Form
	{
		private readonly MainForm mainForm;
		private readonly TextBox txtFolder;
		private readonly ListView lstFiles;
		private readonly TabControl tabReport;
		private readonly TextBox txtValidation;
		private readonly ForkWebViewPanel webReport;
		private readonly Button btnBrowse;
		private readonly Button btnPullAdb;
		private readonly Button btnPushAdb;
		private readonly LinkLabel lnkUsbHelp;
		private readonly Button btnImportAll;
		private readonly Button btnExportAll;
		private readonly Button btnOpenFolder;
		private readonly Label lblHint;
		private readonly CheckBox chkIntegrity;
		private readonly TextBox txtIntegrity;
		private AndroidBackupValidationResult lastValidation;

		private static readonly string[] BackupFiles = new string[]
		{
			"Contacts.csv",
			"TG_Lists.csv",
			"Channels.csv",
			"Zones.csv",
			"DTMF.csv"
		};

		public AndroidBackupForm(MainForm owner)
		{
			this.mainForm = owner;
			this.Text = "Android backup — Path B";
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.MinimumSize = new Size(560, 520);
			this.ClientSize = new Size(560, 520);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			this.lblHint = new Label
			{
				Location = new Point(12, 12),
				Size = new Size(516, 48),
				Text = "Path B: Pull from phone (ADB) or copy to PC for import. Export && push (ADB) sends CSVs to the phone. Log + HTML report tabs."
			};

			this.txtFolder = new TextBox
			{
				Location = new Point(12, 64),
				Size = new Size(416, 23),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			this.txtFolder.Leave += this.txtFolder_Leave;

			this.btnBrowse = new Button
			{
				Location = new Point(434, 62),
				Size = new Size(94, 27),
				Text = "Browse PC…",
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			this.btnBrowse.Click += this.btnBrowse_Click;

			this.btnPullAdb = new Button
			{
				Location = new Point(12, 88),
				Size = new Size(150, 27),
				Text = "Pull from phone (ADB)…"
			};
			this.btnPullAdb.Click += this.btnPullAdb_Click;

			this.btnPushAdb = new Button
			{
				Location = new Point(168, 88),
				Size = new Size(150, 27),
				Text = "Export && push (ADB)…"
			};
			this.btnPushAdb.Click += this.btnPushAdb_Click;

			this.lnkUsbHelp = new LinkLabel
			{
				Location = new Point(324, 93),
				AutoSize = true,
				Text = "MTP/USB browse blocked? Use ADB or copy to PC (help)"
			};
			this.lnkUsbHelp.LinkClicked += this.lnkUsbHelp_LinkClicked;

			this.lstFiles = new ListView
			{
				Location = new Point(12, 120),
				Size = new Size(516, 120),
				View = View.Details,
				FullRowSelect = true,
				HeaderStyle = ColumnHeaderStyle.Nonclickable,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			this.lstFiles.Columns.Add("File", 160);
			this.lstFiles.Columns.Add("Status", 340);

			this.tabReport = new TabControl
			{
				Location = new Point(12, 248),
				Size = new Size(536, 110),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
			};
			TabPage tabLog = new TabPage("Log");
			this.txtValidation = new TextBox
			{
				Dock = DockStyle.Fill,
				Multiline = true,
				ReadOnly = true,
				ScrollBars = ScrollBars.Vertical,
				Font = new Font("Consolas", 9f),
				BackColor = Color.FromArgb(0x06, 0x0D, 0x14),
				ForeColor = Color.FromArgb(0xE8, 0xEE, 0xF4),
				BorderStyle = BorderStyle.None
			};
			tabLog.Controls.Add(this.txtValidation);
			TabPage tabHtml = new TabPage("Report");
			this.webReport = new ForkWebViewPanel();
			tabHtml.Controls.Add(this.webReport);
			this.tabReport.TabPages.Add(tabLog);
			this.tabReport.TabPages.Add(tabHtml);

			this.chkIntegrity = new CheckBox
			{
				Location = new Point(12, 368),
				Size = new Size(516, 20),
				AutoSize = true,
				Visible = false,
				Text = "Contact integrity warnings"
			};
			this.chkIntegrity.CheckedChanged += this.chkIntegrity_CheckedChanged;

			this.txtIntegrity = new TextBox
			{
				Location = new Point(12, 390),
				Size = new Size(536, 72),
				Multiline = true,
				ReadOnly = true,
				Visible = false,
				ScrollBars = ScrollBars.Vertical,
				Font = new Font("Consolas", 9f),
				ForeColor = Color.Khaki,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			this.btnImportAll = new Button
			{
				Location = new Point(12, 472),
				Size = new Size(130, 28),
				Text = "Import all (Path B)",
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};
			this.btnImportAll.Click += this.btnImportAll_Click;

			this.btnExportAll = new Button
			{
				Location = new Point(148, 472),
				Size = new Size(100, 28),
				Text = "Export all",
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};
			this.btnExportAll.Click += this.btnExportAll_Click;

			this.btnOpenFolder = new Button
			{
				Location = new Point(254, 472),
				Size = new Size(100, 28),
				Text = "Open folder",
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};
			this.btnOpenFolder.Click += this.btnOpenFolder_Click;

			Button btnClose = new Button
			{
				Location = new Point(448, 472),
				Size = new Size(100, 28),
				Text = "Close",
				DialogResult = DialogResult.OK,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};
			this.AcceptButton = btnClose;
			this.CancelButton = btnClose;

			this.Controls.Add(this.lblHint);
			this.Controls.Add(this.txtFolder);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.btnPullAdb);
			this.Controls.Add(this.btnPushAdb);
			this.Controls.Add(this.lnkUsbHelp);
			this.Controls.Add(this.lstFiles);
			this.Controls.Add(this.tabReport);
			this.Controls.Add(this.chkIntegrity);
			this.Controls.Add(this.txtIntegrity);
			this.Controls.Add(this.btnImportAll);
			this.Controls.Add(this.btnExportAll);
			this.Controls.Add(this.btnOpenFolder);
			this.Controls.Add(btnClose);

			string last = IniFileUtils.getProfileStringWithDefault("Setup", "LastAndroidBackupFolder", "");
			if (!string.IsNullOrEmpty(last))
			{
				this.txtFolder.Text = last;
				if (AndroidBackupFolderPicker.IsReadableBackupFolder(last))
				{
					this.SetFolder(last, false);
				}
			}
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			string picked = AndroidBackupFolderPicker.PickFolder(this, this.txtFolder.Text, false);
			if (picked != null)
			{
				this.SetFolder(picked, true);
			}
		}

		private void btnPullAdb_Click(object sender, EventArgs e)
		{
			string pulled = AndroidAdbBackup.TryPickPulledFolder(this);
			if (!string.IsNullOrEmpty(pulled))
			{
				this.SetFolder(pulled, true);
			}
			else if (!AndroidAdbBackup.IsAdbAvailable())
			{
				MessageBox.Show(this,
					"adb.exe was not found.\n\nInstall Android platform-tools, add adb to PATH, or use ADB path… in the pull dialog.\n\nYou can still copy the backup folder to your PC and use Browse PC…",
					"ADB not available",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		private void btnPushAdb_Click(object sender, EventArgs e)
		{
			if (!AndroidAdbBackup.IsAdbAvailable())
			{
				MessageBox.Show(this,
					"adb.exe was not found.\n\nInstall Android platform-tools or set ADB path in the push dialog.",
					"ADB not available",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}
			AndroidAdbBackup.TryExportAndPushToPhone(this, this.mainForm);
		}

		private void txtFolder_Leave(object sender, EventArgs e)
		{
			string path = this.txtFolder.Text.Trim();
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			if (AndroidBackupFolderPicker.IsReadableBackupFolder(path))
			{
				this.SetFolder(path, false);
			}
		}

		private void lnkUsbHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AndroidBackupFolderPicker.ShowFolderUnavailableHelp(this, this.txtFolder.Text.Trim());
		}

		private void chkIntegrity_CheckedChanged(object sender, EventArgs e)
		{
			this.txtIntegrity.Visible = this.chkIntegrity.Visible && this.chkIntegrity.Checked
				&& !string.IsNullOrEmpty(this.txtIntegrity.Text);
		}

		private bool SetFolder(string folderPath, bool showErrors)
		{
			folderPath = folderPath == null ? "" : folderPath.Trim();
			if (!AndroidBackupFolderPicker.IsReadableBackupFolder(folderPath))
			{
				if (showErrors)
				{
					AndroidBackupFolderPicker.ShowFolderUnavailableHelp(this, folderPath);
				}
				this.btnImportAll.Enabled = false;
				return false;
			}

			this.txtFolder.Text = folderPath;
			IniFileUtils.WriteProfileString("Setup", "LastAndroidBackupFolder", folderPath);
			this.lstFiles.Items.Clear();
			foreach (string file in BackupFiles)
			{
				string path = Path.Combine(folderPath, file);
				bool exists = File.Exists(path);
				ListViewItem item = new ListViewItem(file);
				item.SubItems.Add(exists ? "Found" : "Missing");
				item.ForeColor = exists ? Color.LightGreen : Color.Salmon;
				this.lstFiles.Items.Add(item);
			}

			this.lastValidation = AndroidBackupValidator.ValidateFolder(folderPath);
			StringBuilder log = new StringBuilder(this.lastValidation.Summary);
			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			AndroidImportDiffResult diff = null;
			if (File.Exists(channelsPath))
			{
				diff = AndroidImportDiff.Compute(channelsPath);
				log.Append("\n\n").Append(diff.Summary);
			}
			AndroidContactIntegrityResult integrity = AndroidContactIntegrityChecker.CheckFolder(folderPath);
			log.Append("\n\n").Append(integrity.Summary);
			this.txtValidation.Text = log.ToString();
			this.webReport.NavigateHtml(AndroidBackupReportHtml.Build(folderPath, this.lastValidation, diff, integrity));
			this.chkIntegrity.Visible = integrity.HasWarnings;
			this.chkIntegrity.Text = integrity.Summary;
			this.chkIntegrity.Checked = integrity.HasWarnings;
			this.txtIntegrity.Text = integrity.DetailText;
			this.txtIntegrity.Visible = integrity.HasWarnings && this.chkIntegrity.Checked;
			this.btnImportAll.Enabled = !this.lastValidation.HasBlockingErrors;
			return true;
		}

		private void btnImportAll_Click(object sender, EventArgs e)
		{
			if (!this.SetFolder(this.txtFolder.Text.Trim(), true))
			{
				return;
			}
			if (this.lastValidation != null && this.lastValidation.HasBlockingErrors)
			{
				MessageBox.Show(this, this.lastValidation.Summary, "Cannot import", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (this.lastValidation != null && (this.lastValidation.RelayZeroCount > 0 || this.lastValidation.DuplicateChannelNames > 0))
			{
				DialogResult warn = MessageBox.Show(this,
					this.lastValidation.Summary + "\n\nImport anyway?",
					"Validation warnings",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning);
				if (warn != DialogResult.Yes)
				{
					return;
				}
			}
			this.mainForm.ImportAndroidBackupFolder(this.txtFolder.Text);
			this.SetFolder(this.txtFolder.Text, false);
		}

		private void btnExportAll_Click(object sender, EventArgs e)
		{
			if (!AndroidBackupFolderPicker.IsReadableBackupFolder(this.txtFolder.Text.Trim()))
			{
				string picked = AndroidBackupFolderPicker.PickFolder(this, this.txtFolder.Text, true);
				if (picked == null)
				{
					return;
				}
				this.SetFolder(picked, true);
			}
			this.mainForm.ExportAndroidBackupFolder(this.txtFolder.Text);
			this.SetFolder(this.txtFolder.Text, false);
		}

		private void btnOpenFolder_Click(object sender, EventArgs e)
		{
			this.mainForm.OpenAndroidBackupFolder();
		}
	}
}