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
		private readonly Button btnBrowse;
		private readonly Button btnImportAll;
		private readonly Button btnExportAll;
		private readonly Button btnOpenFolder;
		private readonly Label lblHint;
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
			this.MinimumSize = new Size(540, 480);
			this.ClientSize = new Size(540, 460);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			this.lblHint = new Label
			{
				Location = new Point(12, 12),
				Size = new Size(516, 36),
				Text = "Phone export folder. Path B validation + channel diff preview before import. Order: Contacts → TG_Lists → Channels → Zones."
			};

			this.txtFolder = new TextBox
			{
				Location = new Point(12, 52),
				Size = new Size(416, 23),
				ReadOnly = true,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			this.btnBrowse = new Button
			{
				Location = new Point(434, 50),
				Size = new Size(94, 27),
				Text = "Browse…",
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			this.btnBrowse.Click += this.btnBrowse_Click;

			this.lstFiles = new ListView
			{
				Location = new Point(12, 84),
				Size = new Size(516, 120),
				View = View.Details,
				FullRowSelect = true,
				HeaderStyle = ColumnHeaderStyle.Nonclickable,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			this.lstFiles.Columns.Add("File", 160);
			this.lstFiles.Columns.Add("Status", 340);

			this.txtValidation = new TextBox
			{
				Location = new Point(12, 212),
				Size = new Size(516, 180),
				Multiline = true,
				ReadOnly = true,
				ScrollBars = ScrollBars.Vertical,
				Font = new Font("Consolas", 9f),
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
			};

			this.btnImportAll = new Button
			{
				Location = new Point(12, 402),
				Size = new Size(130, 28),
				Text = "Import all (Path B)",
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};
			this.btnImportAll.Click += this.btnImportAll_Click;

			this.btnExportAll = new Button
			{
				Location = new Point(148, 402),
				Size = new Size(100, 28),
				Text = "Export all",
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};
			this.btnExportAll.Click += this.btnExportAll_Click;

			this.btnOpenFolder = new Button
			{
				Location = new Point(254, 402),
				Size = new Size(100, 28),
				Text = "Open folder",
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};
			this.btnOpenFolder.Click += this.btnOpenFolder_Click;

			Button btnClose = new Button
			{
				Location = new Point(428, 402),
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
			this.Controls.Add(this.lstFiles);
			this.Controls.Add(this.txtValidation);
			this.Controls.Add(this.btnImportAll);
			this.Controls.Add(this.btnExportAll);
			this.Controls.Add(this.btnOpenFolder);
			this.Controls.Add(btnClose);

			string last = IniFileUtils.getProfileStringWithDefault("Setup", "LastAndroidBackupFolder", "");
			if (!string.IsNullOrEmpty(last) && Directory.Exists(last))
			{
				this.SetFolder(last);
			}
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog dlg = new FolderBrowserDialog())
			{
				dlg.Description = "Select Android backup folder from the phone";
				if (!string.IsNullOrEmpty(this.txtFolder.Text))
				{
					dlg.SelectedPath = this.txtFolder.Text;
				}
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					this.SetFolder(dlg.SelectedPath);
				}
			}
		}

		private void SetFolder(string folderPath)
		{
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
			if (File.Exists(channelsPath))
			{
				AndroidImportDiffResult diff = AndroidImportDiff.Compute(channelsPath);
				log.Append("\n\n").Append(diff.Summary);
			}
			this.txtValidation.Text = log.ToString();
			this.btnImportAll.Enabled = !this.lastValidation.HasBlockingErrors;
		}

		private void btnImportAll_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.txtFolder.Text) || !Directory.Exists(this.txtFolder.Text))
			{
				MessageBox.Show(this, "Choose a backup folder first.", "Android backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
			this.SetFolder(this.txtFolder.Text);
		}

		private void btnExportAll_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.txtFolder.Text) || !Directory.Exists(this.txtFolder.Text))
			{
				using (FolderBrowserDialog dlg = new FolderBrowserDialog())
				{
					dlg.Description = "Select folder to export Android CSV files";
					if (dlg.ShowDialog(this) != DialogResult.OK)
					{
						return;
					}
					this.SetFolder(dlg.SelectedPath);
				}
			}
			this.mainForm.ExportAndroidBackupFolder(this.txtFolder.Text);
			this.SetFolder(this.txtFolder.Text);
		}

		private void btnOpenFolder_Click(object sender, EventArgs e)
		{
			this.mainForm.OpenAndroidBackupFolder();
		}
	}
}