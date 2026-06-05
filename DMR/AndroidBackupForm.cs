using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Android backup folder UI (UI plan Tier 2.2 MVP) — Path B import/export in one place.
	/// </summary>
	public class AndroidBackupForm : Form
	{
		private readonly MainForm mainForm;
		private readonly TextBox txtFolder;
		private readonly ListView lstFiles;
		private readonly Button btnBrowse;
		private readonly Button btnImportAll;
		private readonly Button btnExportAll;
		private readonly Button btnOpenFolder;
		private readonly Label lblHint;

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
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ClientSize = new Size(520, 360);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkChrome(this, null, null, null);

			this.lblHint = new Label
			{
				Location = new Point(12, 12),
				Size = new Size(496, 40),
				Text = "Select a phone export folder (DMR_Backups\\YYYYMMDD_HHmmss). Import order: Contacts → TG_Lists → Channels → Zones."
			};

			this.txtFolder = new TextBox
			{
				Location = new Point(12, 58),
				Size = new Size(396, 23),
				ReadOnly = true
			};

			this.btnBrowse = new Button
			{
				Location = new Point(414, 56),
				Size = new Size(94, 27),
				Text = "Browse…"
			};
			this.btnBrowse.Click += this.btnBrowse_Click;

			this.lstFiles = new ListView
			{
				Location = new Point(12, 92),
				Size = new Size(496, 180),
				View = View.Details,
				FullRowSelect = true,
				HeaderStyle = ColumnHeaderStyle.Nonclickable
			};
			this.lstFiles.Columns.Add("File", 220);
			this.lstFiles.Columns.Add("Status", 260);

			this.btnImportAll = new Button
			{
				Location = new Point(12, 284),
				Size = new Size(120, 28),
				Text = "Import all (Path B)"
			};
			this.btnImportAll.Click += this.btnImportAll_Click;

			this.btnExportAll = new Button
			{
				Location = new Point(140, 284),
				Size = new Size(120, 28),
				Text = "Export all"
			};
			this.btnExportAll.Click += this.btnExportAll_Click;

			this.btnOpenFolder = new Button
			{
				Location = new Point(268, 284),
				Size = new Size(120, 28),
				Text = "Open folder"
			};
			this.btnOpenFolder.Click += this.btnOpenFolder_Click;

			Button btnClose = new Button
			{
				Location = new Point(408, 284),
				Size = new Size(100, 28),
				Text = "Close",
				DialogResult = DialogResult.OK
			};
			this.AcceptButton = btnClose;
			this.CancelButton = btnClose;

			this.Controls.Add(this.lblHint);
			this.Controls.Add(this.txtFolder);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.lstFiles);
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
				item.SubItems.Add(exists ? "Found" : "Missing (optional except Channels/Contacts)");
				item.ForeColor = exists ? Color.LightGreen : Color.Salmon;
				this.lstFiles.Items.Add(item);
			}
		}

		private void btnImportAll_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.txtFolder.Text) || !Directory.Exists(this.txtFolder.Text))
			{
				MessageBox.Show(this, "Choose a backup folder first.", "Android backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			this.mainForm.ImportAndroidBackupFolder(this.txtFolder.Text);
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