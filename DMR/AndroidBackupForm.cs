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
		private readonly TextBox txtValidation;
		private readonly ForkWebViewPanel webReport;
		private readonly SplitContainer splitMain;
		private readonly Button btnBrowse;
		private readonly Button btnPullAdb;
		private readonly Button btnPushAdb;
		private readonly LinkLabel lnkUsbHelp;
		private readonly Button btnImportAll;
		private readonly Button btnReviewDiff;
		private readonly Button btnExportAll;
		private readonly Button btnOpenFolder;
		private readonly Button btnRawLog;
		private readonly Label lblHint;
		private readonly Label lblReportCaption;
		private AndroidBackupValidationResult lastValidation;
		private AndroidImportDiffResult lastDiff;
		private string lastDiffFolder = "";
		private string lastApprovedChannelsStamp = "";
		private bool diffPreApproved;

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
			this.Text = "Android backup — PriInterPhone Path B";
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.MinimumSize = new Size(640, 520);
			this.ClientSize = new Size(720, 580);
			this.Font = Theme.UiFont;
			Theme.ApplyForkDialog(this);

			this.lblHint = new Label
			{
				Dock = DockStyle.Top,
				Height = 40,
				Text = "PC → phone: Export all + Push (ADB) → IMPORT on phone.  PC ← phone: Pull (ADB) → Review diff → Import all (Path B).  Report below validates CSVs."
			};

			Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 200 };
			this.txtFolder = new TextBox
			{
				Location = new Point(12, 8),
				Size = new Size(500, 23),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			this.txtFolder.Leave += this.txtFolder_Leave;

			this.btnBrowse = new Button
			{
				Location = new Point(518, 6),
				Size = new Size(94, 27),
				Text = "Browse PC…",
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			this.btnBrowse.Click += this.btnBrowse_Click;

			this.btnPullAdb = new Button
			{
				Location = new Point(12, 38),
				Size = new Size(150, 27),
				Text = "Pull from phone (ADB)…"
			};
			this.btnPullAdb.Click += this.btnPullAdb_Click;

			this.btnPushAdb = new Button
			{
				Location = new Point(168, 38),
				Size = new Size(150, 27),
				Text = "Export && push (ADB)…"
			};
			this.btnPushAdb.Click += this.btnPushAdb_Click;

			this.lnkUsbHelp = new LinkLabel
			{
				Location = new Point(324, 43),
				AutoSize = true,
				Text = "MTP/USB blocked? ADB or copy to PC (help)"
			};
			this.lnkUsbHelp.LinkClicked += this.lnkUsbHelp_LinkClicked;

			this.lstFiles = new ListView
			{
				Location = new Point(12, 72),
				Size = new Size(600, 118),
				View = View.Details,
				FullRowSelect = true,
				HeaderStyle = ColumnHeaderStyle.Nonclickable,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
			};
			this.lstFiles.Columns.Add("File", 160);
			this.lstFiles.Columns.Add("Status", 420);

			topPanel.Controls.Add(this.txtFolder);
			topPanel.Controls.Add(this.btnBrowse);
			topPanel.Controls.Add(this.btnPullAdb);
			topPanel.Controls.Add(this.btnPushAdb);
			topPanel.Controls.Add(this.lnkUsbHelp);
			topPanel.Controls.Add(this.lstFiles);

			this.lblReportCaption = new Label
			{
				Text = "Validation report",
				Dock = DockStyle.Top,
				Height = 22,
				ForeColor = Theme.MutedForeground,
				Padding = new Padding(4, 4, 0, 0)
			};
			this.webReport = new ForkWebViewPanel { Dock = DockStyle.Fill };
			this.txtValidation = new TextBox { Multiline = true, ReadOnly = true };

			Panel reportPanel = new Panel { Dock = DockStyle.Fill };
			reportPanel.Controls.Add(this.webReport);
			reportPanel.Controls.Add(this.lblReportCaption);

			this.splitMain = new SplitContainer
			{
				Dock = DockStyle.Fill,
				Orientation = Orientation.Horizontal,
				SplitterDistance = 200,
				FixedPanel = FixedPanel.Panel1
			};
			this.splitMain.Panel1.Controls.Add(topPanel);
			this.splitMain.Panel2.Controls.Add(reportPanel);

			this.btnImportAll = new Button
			{
				Location = new Point(12, 8),
				Size = new Size(118, 28),
				Text = "Import all (Path B)"
			};
			this.btnImportAll.Click += this.btnImportAll_Click;

			this.btnReviewDiff = new Button
			{
				Location = new Point(136, 8),
				Size = new Size(118, 28),
				Text = "Review diff…",
				Enabled = false
			};
			this.btnReviewDiff.Click += this.btnReviewDiff_Click;

			this.btnExportAll = new Button
			{
				Location = new Point(260, 8),
				Size = new Size(90, 28),
				Text = "Export all"
			};
			this.btnExportAll.Click += this.btnExportAll_Click;

			this.btnOpenFolder = new Button
			{
				Location = new Point(356, 8),
				Size = new Size(90, 28),
				Text = "Open folder"
			};
			this.btnOpenFolder.Click += this.btnOpenFolder_Click;

			this.btnRawLog = new Button
			{
				Location = new Point(452, 8),
				Size = new Size(80, 28),
				Text = "Raw log…"
			};
			this.btnRawLog.Click += this.btnRawLog_Click;

			Button btnClose = new Button
			{
				Location = new Point(608, 8),
				Size = new Size(100, 28),
				Text = "Close",
				DialogResult = DialogResult.OK,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			this.AcceptButton = btnClose;
			this.CancelButton = btnClose;

			Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
			bottomPanel.Controls.Add(this.btnImportAll);
			bottomPanel.Controls.Add(this.btnReviewDiff);
			bottomPanel.Controls.Add(this.btnExportAll);
			bottomPanel.Controls.Add(this.btnOpenFolder);
			bottomPanel.Controls.Add(this.btnRawLog);
			bottomPanel.Controls.Add(btnClose);

			this.Controls.Add(this.splitMain);
			this.Controls.Add(bottomPanel);
			this.Controls.Add(this.lblHint);

			this.Shown += this.AndroidBackupForm_Shown;

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

		private void AndroidBackupForm_Shown(object sender, EventArgs e)
		{
			this.webReport.EnsureInitialized();
		}

		private void btnRawLog_Click(object sender, EventArgs e)
		{
			using (Form dlg = new Form())
			{
				dlg.Text = "Android backup — raw log";
				dlg.StartPosition = FormStartPosition.CenterParent;
				dlg.ClientSize = new Size(560, 400);
				dlg.Font = Theme.UiFont;
				Theme.ApplyForkDialog(dlg);
				TextBox txt = new TextBox
				{
					Dock = DockStyle.Fill,
					Multiline = true,
					ReadOnly = true,
					ScrollBars = ScrollBars.Both,
					Font = new Font("Consolas", 9f),
					Text = this.txtValidation.Text,
					BackColor = Color.FromArgb(0x06, 0x0D, 0x14),
					ForeColor = Color.FromArgb(0xE8, 0xEE, 0xF4)
				};
				dlg.Controls.Add(txt);
				dlg.ShowDialog(this);
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
				if (this.SetFolder(pulled, true))
				{
					AndroidImportDiff.OfferReviewAfterPullIfNeeded(
						this,
						this.lastDiff,
						this.diffPreApproved,
						this.lastValidation != null && this.lastValidation.HasBlockingErrors,
						() => this.TryApproveChannelDiff(pulled));
				}
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
			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			if (!string.Equals(this.lastDiffFolder, folderPath, StringComparison.OrdinalIgnoreCase))
			{
				this.lastDiffFolder = folderPath;
				this.diffPreApproved = false;
				this.lastApprovedChannelsStamp = "";
			}
			else if (this.diffPreApproved)
			{
				string stamp = AndroidImportDiff.GetChannelsCsvStamp(channelsPath);
				if (!string.Equals(stamp, this.lastApprovedChannelsStamp, StringComparison.Ordinal))
				{
					this.diffPreApproved = false;
				}
			}
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
			AndroidImportDiffResult diff = null;
			if (File.Exists(channelsPath))
			{
				diff = AndroidImportDiff.Compute(channelsPath);
				this.lastDiff = diff;
				log.Append("\n\n").Append(diff.Summary);
				if (!AndroidImportDiff.HasPendingDiffChanges(diff))
				{
					this.diffPreApproved = true;
					this.lastApprovedChannelsStamp = AndroidImportDiff.GetChannelsCsvStamp(channelsPath);
					this.lastDiffFolder = folderPath;
				}
			}
			else
			{
				this.lastDiff = null;
			}
			AndroidContactIntegrityResult integrity = AndroidContactIntegrityChecker.CheckFolder(folderPath);
			log.Append("\n\n").Append(integrity.Summary);
			if (integrity.HasWarnings && !string.IsNullOrEmpty(integrity.DetailText))
			{
				log.Append("\n\n").Append(integrity.DetailText);
			}
			this.txtValidation.Text = log.ToString();
			this.webReport.EnsureInitialized();
			this.webReport.NavigateHtml(AndroidBackupReportHtml.Build(folderPath, this.lastValidation, diff, integrity));
			this.lblReportCaption.Text = integrity.HasWarnings || this.lastValidation.HasBlockingErrors
				? "Validation report — review warnings below"
				: "Validation report — ready to import";
			bool hasChannels = File.Exists(channelsPath);
			bool pendingDiff = hasChannels && diff != null && AndroidImportDiff.HasPendingDiffChanges(diff) && !this.diffPreApproved;
			this.btnReviewDiff.Enabled = hasChannels;
			this.btnReviewDiff.Text = this.diffPreApproved && hasChannels ? "Diff reviewed ✓" : "Review diff…";
			this.btnReviewDiff.BackColor = pendingDiff ? Color.FromArgb(0x1E, 0x5A, 0x8F) : SystemColors.Control;
			this.btnReviewDiff.ForeColor = pendingDiff ? Color.White : SystemColors.ControlText;
			this.btnImportAll.Enabled = !this.lastValidation.HasBlockingErrors && !pendingDiff;
			return true;
		}

		private bool TryApproveChannelDiff(string folderPath)
		{
			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			if (!File.Exists(channelsPath))
			{
				return true;
			}
			if (AndroidImportDiff.IsDiffReviewCurrent(channelsPath, this.diffPreApproved, this.lastApprovedChannelsStamp)
				&& string.Equals(this.lastDiffFolder, folderPath, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			string stamp = AndroidImportDiff.GetChannelsCsvStamp(channelsPath);
			if (!AndroidImportDiff.ShowPreviewDialog(this, channelsPath))
			{
				return false;
			}
			this.lastDiffFolder = folderPath;
			this.lastApprovedChannelsStamp = stamp;
			this.diffPreApproved = true;
			this.btnReviewDiff.Text = "Diff reviewed ✓";
			this.btnReviewDiff.BackColor = SystemColors.Control;
			this.btnReviewDiff.ForeColor = SystemColors.ControlText;
			this.btnImportAll.Enabled = !this.lastValidation.HasBlockingErrors;
			return true;
		}

		private void btnReviewDiff_Click(object sender, EventArgs e)
		{
			string folderPath = this.txtFolder.Text.Trim();
			if (!this.SetFolder(folderPath, true))
			{
				return;
			}
			this.TryApproveChannelDiff(folderPath);
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
			string folderPath = this.txtFolder.Text.Trim();
			if (!this.TryApproveChannelDiff(folderPath))
			{
				return;
			}
			this.mainForm.ImportAndroidBackupFolder(
				folderPath, this.diffPreApproved, true, false, this.lastApprovedChannelsStamp);
			this.SetFolder(folderPath, false);
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