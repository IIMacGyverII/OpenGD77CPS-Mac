using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ReadWriteCsv;

namespace DMR
{
	/// <summary>
	/// Tier 3.2 spike — thin CSV-only workflow (5 backup files, validation, diff, Path B).
	/// Full OpenGD77 CPS remains available via "Open full editor".
	/// </summary>
	public class CodeplugStudioForm : Form
	{
		private static readonly string[] BackupFiles = new string[]
		{
			"Channels.csv",
			"Contacts.csv",
			"TG_Lists.csv",
			"Zones.csv",
			"DTMF.csv"
		};

		private readonly MainForm mainForm;
		private readonly TextBox txtFolder;
		private readonly FlowLayoutPanel pnlCsvTiles;
		private readonly ForkWebViewPanel webReport;
		private readonly Label lblReportCaption;
		private readonly Button btnImportAll;
		private readonly Button btnReviewDiff;
		private readonly Button btnExportAll;
		private readonly Button btnOpenFullCps;
		private readonly TextBox txtValidation;
		private readonly ToolTip csvTileTip = new ToolTip();
		private AndroidBackupValidationResult lastValidation;
		private string lastDiffFolder = "";
		private bool diffPreApproved;

		public bool UserOpenedFullCps { get; private set; }

		public CodeplugStudioForm(MainForm owner)
		{
			this.mainForm = owner;
			this.Text = "PriInterPhone Codeplug Studio";
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.ApplyStudioWindowSize(owner);
			this.Font = Theme.UiFont;
			Theme.ApplyForkDialog(this);

			Label lblIntro = new Label
			{
				Dock = DockStyle.Top,
				Height = 36,
				Text = "CSV-only workflow — load the 5-file phone backup, validate, review diff, then import or export (Path B). Double-click a CSV tile to open that file."
			};

			Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 118 };
			this.txtFolder = new TextBox
			{
				Location = new Point(12, 10),
				Size = new Size(560, 23),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			this.txtFolder.Leave += this.txtFolder_Leave;

			Button btnBrowse = new Button
			{
				Location = new Point(578, 8),
				Size = new Size(94, 27),
				Text = "Browse…",
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			btnBrowse.Click += this.btnBrowse_Click;

			Button btnPullAdb = new Button
			{
				Location = new Point(12, 42),
				Size = new Size(140, 27),
				Text = "Pull (ADB)…"
			};
			btnPullAdb.Click += this.btnPullAdb_Click;

			Button btnPushAdb = new Button
			{
				Location = new Point(158, 42),
				Size = new Size(140, 27),
				Text = "Export && push…"
			};
			btnPushAdb.Click += this.btnPushAdb_Click;

			LinkLabel lnkHelp = new LinkLabel
			{
				Location = new Point(306, 47),
				AutoSize = true,
				Text = "MTP/USB help"
			};
			lnkHelp.LinkClicked += this.lnkHelp_LinkClicked;

			this.pnlCsvTiles = new FlowLayoutPanel
			{
				Location = new Point(12, 74),
				Size = new Size(660, 38),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				WrapContents = false,
				AutoScroll = true
			};
			foreach (string file in BackupFiles)
			{
				Panel tile = this.CreateCsvTile(file);
				this.pnlCsvTiles.Controls.Add(tile);
			}

			topPanel.Controls.Add(this.txtFolder);
			topPanel.Controls.Add(btnBrowse);
			topPanel.Controls.Add(btnPullAdb);
			topPanel.Controls.Add(btnPushAdb);
			topPanel.Controls.Add(lnkHelp);
			topPanel.Controls.Add(this.pnlCsvTiles);

			this.lblReportCaption = new Label
			{
				Text = "Validation report",
				Dock = DockStyle.Top,
				Height = 22,
				ForeColor = Theme.MutedForeground,
				Padding = new Padding(4, 4, 0, 0)
			};
			this.webReport = new ForkWebViewPanel { Dock = DockStyle.Fill };
			this.txtValidation = new TextBox { Multiline = true, ReadOnly = true, Visible = false };

			Panel reportPanel = new Panel { Dock = DockStyle.Fill };
			reportPanel.Controls.Add(this.webReport);
			reportPanel.Controls.Add(this.lblReportCaption);

			this.btnImportAll = new Button
			{
				Location = new Point(12, 8),
				Size = new Size(118, 28),
				Text = "Import all"
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

			Button btnOpenFolder = new Button
			{
				Location = new Point(356, 8),
				Size = new Size(90, 28),
				Text = "Open folder"
			};
			btnOpenFolder.Click += this.btnOpenFolder_Click;

			Button btnHealth = new Button
			{
				Location = new Point(452, 8),
				Size = new Size(90, 28),
				Text = "Health (F7)"
			};
			btnHealth.Click += this.btnHealth_Click;

			this.btnOpenFullCps = new Button
			{
				Location = new Point(548, 8),
				Size = new Size(130, 28),
				Text = "Open full editor…"
			};
			this.btnOpenFullCps.Click += this.btnOpenFullCps_Click;

			Button btnClose = new Button
			{
				Location = new Point(688, 8),
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
			bottomPanel.Controls.Add(btnOpenFolder);
			bottomPanel.Controls.Add(btnHealth);
			bottomPanel.Controls.Add(this.btnOpenFullCps);
			bottomPanel.Controls.Add(btnClose);

			this.Controls.Add(reportPanel);
			this.Controls.Add(topPanel);
			this.Controls.Add(bottomPanel);
			this.Controls.Add(lblIntro);

			this.Shown += this.CodeplugStudioForm_Shown;
			this.Resize += this.CodeplugStudioForm_Resize;

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

		private void ApplyStudioWindowSize(IWin32Window owner)
		{
			Control ownerControl = owner as Control;
			Screen screen = ownerControl != null && ownerControl.IsHandleCreated
				? Screen.FromControl(ownerControl)
				: Screen.PrimaryScreen;
			Rectangle area = screen.WorkingArea;
			int width = Math.Max(720, (int)Math.Round(area.Width * 0.75));
			int height = Math.Max(560, (int)Math.Round(area.Height * 0.75));
			width = Math.Min(width, area.Width);
			height = Math.Min(height, area.Height);
			this.MinimumSize = new Size(720, 560);
			this.Size = new Size(width, height);
			this.StartPosition = FormStartPosition.CenterScreen;
		}

		private Panel CreateCsvTile(string fileName)
		{
			Panel tile = new Panel
			{
				Size = new Size(124, 34),
				Margin = new Padding(0, 0, 6, 0),
				BackColor = Color.FromArgb(0x12, 0x1E, 0x2C),
				BorderStyle = BorderStyle.FixedSingle,
				Tag = fileName
			};
			Label lblName = new Label
			{
				Text = fileName.Replace(".csv", ""),
				Location = new Point(6, 2),
				AutoSize = true,
				Font = Theme.UiFontBold,
				ForeColor = Theme.Foreground
			};
			Label lblStatus = new Label
			{
				Name = "status",
				Text = "—",
				Location = new Point(6, 18),
				AutoSize = true,
				Font = Theme.UiFontSmall,
				ForeColor = Theme.MutedForeground
			};
			tile.Controls.Add(lblName);
			tile.Controls.Add(lblStatus);
			tile.Cursor = Cursors.Hand;
			tile.DoubleClick += this.csvTile_DoubleClick;
			this.csvTileTip.SetToolTip(tile, "Double-click to open " + fileName);
			return tile;
		}

		private void CodeplugStudioForm_Shown(object sender, EventArgs e)
		{
			this.webReport.EnsureInitialized();
			this.LayoutCsvTiles();
		}

		private void CodeplugStudioForm_Resize(object sender, EventArgs e)
		{
			this.LayoutCsvTiles();
		}

		private void LayoutCsvTiles()
		{
			if (this.pnlCsvTiles == null || this.pnlCsvTiles.Controls.Count == 0)
			{
				return;
			}
			int count = this.pnlCsvTiles.Controls.Count;
			int gap = 6;
			int available = Math.Max(124 * count, this.pnlCsvTiles.ClientSize.Width - gap);
			int tileWidth = Math.Max(124, (available - gap * (count - 1)) / count);
			foreach (Control control in this.pnlCsvTiles.Controls)
			{
				control.Width = tileWidth;
			}
		}

		private void csvTile_DoubleClick(object sender, EventArgs e)
		{
			Panel tile = sender as Panel;
			string fileName = tile == null ? null : tile.Tag as string;
			if (string.IsNullOrEmpty(fileName))
			{
				return;
			}
			string folderPath = this.txtFolder.Text.Trim();
			if (!AndroidBackupFolderPicker.IsReadableBackupFolder(folderPath))
			{
				MessageBox.Show(this, "Choose a valid backup folder first.", "Open CSV",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			string path = Path.Combine(folderPath, fileName);
			if (!File.Exists(path))
			{
				MessageBox.Show(this, fileName + " is missing in this folder.", "Open CSV",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			try
			{
				Process.Start(path);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, path + "\n\n" + ex.Message, "Open CSV",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
					"adb.exe was not found. Copy the backup folder to your PC and use Browse…",
					"ADB not available",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		private void btnPushAdb_Click(object sender, EventArgs e)
		{
			if (!AndroidAdbBackup.IsAdbAvailable())
			{
				MessageBox.Show(this, "adb.exe was not found.", "ADB not available",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
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

		private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AndroidBackupFolderPicker.ShowFolderUnavailableHelp(this, this.txtFolder.Text.Trim());
		}

		private void UpdateCsvTiles(string folderPath)
		{
			foreach (Control control in this.pnlCsvTiles.Controls)
			{
				Panel tile = control as Panel;
				if (tile == null)
				{
					continue;
				}
				string fileName = tile.Tag as string;
				Label lblStatus = null;
				foreach (Control child in tile.Controls)
				{
					Label label = child as Label;
					if (label != null && label.Name == "status")
					{
						lblStatus = label;
						break;
					}
				}
				if (lblStatus == null || string.IsNullOrEmpty(fileName))
				{
					continue;
				}
				string path = Path.Combine(folderPath ?? "", fileName);
				if (!File.Exists(path))
				{
					lblStatus.Text = "Missing";
					lblStatus.ForeColor = Color.Salmon;
					tile.BackColor = Color.FromArgb(0x2A, 0x14, 0x14);
					continue;
				}
				int rows = CountCsvDataRows(path);
				lblStatus.Text = rows >= 0 ? rows + " row(s)" : "Found";
				lblStatus.ForeColor = Color.LightGreen;
				tile.BackColor = Color.FromArgb(0x12, 0x28, 0x1C);
			}
		}

		private static int CountCsvDataRows(string path)
		{
			try
			{
				int count = 0;
				using (CsvFileReader reader = new CsvFileReader(path, CsvEncoding.Utf8NoBom))
				{
					CsvRow row = new CsvRow();
					if (!reader.ReadRow(row))
					{
						return 0;
					}
					while (reader.ReadRow(row))
					{
						if (row.Count > 0)
						{
							count++;
						}
					}
				}
				return count;
			}
			catch
			{
				return -1;
			}
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
				this.UpdateCsvTiles("");
				return false;
			}

			this.txtFolder.Text = folderPath;
			IniFileUtils.WriteProfileString("Setup", "LastAndroidBackupFolder", folderPath);
			if (!string.Equals(this.lastDiffFolder, folderPath, StringComparison.OrdinalIgnoreCase))
			{
				this.lastDiffFolder = folderPath;
				this.diffPreApproved = false;
			}
			this.UpdateCsvTiles(folderPath);

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
			if (integrity.HasWarnings && !string.IsNullOrEmpty(integrity.DetailText))
			{
				log.Append("\n\n").Append(integrity.DetailText);
			}
			this.txtValidation.Text = log.ToString();
			this.webReport.EnsureInitialized();
			this.webReport.NavigateHtml(AndroidBackupReportHtml.Build(folderPath, this.lastValidation, diff, integrity));
			this.lblReportCaption.Text = integrity.HasWarnings || this.lastValidation.HasBlockingErrors
				? "Validation report — review warnings"
				: "Validation report — ready";
			bool hasChannels = File.Exists(channelsPath);
			this.btnReviewDiff.Enabled = hasChannels;
			this.btnReviewDiff.Text = this.diffPreApproved && hasChannels ? "Diff reviewed ✓" : "Review diff…";
			this.btnImportAll.Enabled = !this.lastValidation.HasBlockingErrors;
			return true;
		}

		private bool TryApproveChannelDiff(string folderPath)
		{
			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			if (!File.Exists(channelsPath))
			{
				return true;
			}
			if (this.diffPreApproved && string.Equals(this.lastDiffFolder, folderPath, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (!AndroidImportDiff.ShowPreviewDialog(this, channelsPath))
			{
				return false;
			}
			this.lastDiffFolder = folderPath;
			this.diffPreApproved = true;
			this.btnReviewDiff.Text = "Diff reviewed ✓";
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
				MessageBox.Show(this, this.lastValidation.Summary, "Cannot import",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			this.mainForm.ImportAndroidBackupFolder(folderPath, this.diffPreApproved);
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
			string folderPath = this.txtFolder.Text.Trim();
			if (!AndroidBackupFolderPicker.IsReadableBackupFolder(folderPath))
			{
				MessageBox.Show(this, "Choose a valid backup folder first.", "Open folder",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			try
			{
				Process.Start("explorer.exe", folderPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, folderPath + "\n\n" + ex.Message, "Open folder",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void btnHealth_Click(object sender, EventArgs e)
		{
			this.mainForm.OpenCodeplugHealthReport();
		}

		private void btnOpenFullCps_Click(object sender, EventArgs e)
		{
			this.UserOpenedFullCps = true;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}