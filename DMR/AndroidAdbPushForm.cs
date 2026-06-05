using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Export Path B CSVs and adb push to the phone's DMR_Backups folder.
	/// </summary>
	public class AndroidAdbPushForm : Form
	{
		private readonly MainForm mainForm;
		private readonly ComboBox cmbDevice;
		private readonly TextBox txtFolderName;
		private readonly ListBox lstExisting;
		private readonly CheckBox chkOverwrite;
		private readonly Label lblStatus;
		private readonly Button btnRefresh;
		private readonly Button btnPush;
		private readonly Button btnCancel;

		private AndroidAdbListResult lastList;

		public string RemoteFolderName { get; private set; }
		public string RemotePath { get; private set; }

		public AndroidAdbPushForm(MainForm mainForm)
		{
			this.mainForm = mainForm;
			this.Text = "Export && push to phone (ADB)";
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ClientSize = new Size(460, 400);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			Label lblDevice = new Label { Location = new Point(12, 12), AutoSize = true, Text = "Device" };
			this.cmbDevice = new ComboBox
			{
				Location = new Point(12, 32),
				Size = new Size(436, 23),
				DropDownStyle = ComboBoxStyle.DropDownList
			};

			Label lblFolder = new Label { Location = new Point(12, 62), AutoSize = true, Text = "Phone folder name (any label — e.g. June 5 2026 or 20260605_120842)" };
			this.txtFolderName = new TextBox
			{
				Location = new Point(12, 82),
				Size = new Size(436, 23),
				Text = AndroidAdbBackup.DefaultBackupFolderName()
			};

			Label lblExisting = new Label
			{
				Location = new Point(12, 112),
				AutoSize = true,
				Text = "Existing on phone (double-click to overwrite)"
			};
			this.lstExisting = new ListBox
			{
				Location = new Point(12, 132),
				Size = new Size(436, 120)
			};
			this.lstExisting.DoubleClick += this.lstExisting_DoubleClick;

			this.chkOverwrite = new CheckBox
			{
				Location = new Point(12, 258),
				AutoSize = true,
				Text = "Overwrite if folder already exists on phone"
			};

			this.lblStatus = new Label
			{
				Location = new Point(12, 282),
				Size = new Size(436, 48),
				Text = "Exports CSVs from the loaded codeplug, then adb push to Download/DMR/DMR_Backups/. Name the folder anything you like."
			};

			this.btnRefresh = new Button { Location = new Point(12, 336), Size = new Size(88, 28), Text = "Refresh" };
			this.btnRefresh.Click += this.btnRefresh_Click;

			this.btnPush = new Button { Location = new Point(252, 336), Size = new Size(120, 28), Text = "Export && push" };
			this.btnPush.Click += this.btnPush_Click;

			this.btnCancel = new Button
			{
				Location = new Point(378, 336),
				Size = new Size(70, 28),
				Text = "Cancel",
				DialogResult = DialogResult.Cancel
			};

			this.Controls.Add(lblDevice);
			this.Controls.Add(this.cmbDevice);
			this.Controls.Add(lblFolder);
			this.Controls.Add(this.txtFolderName);
			this.Controls.Add(lblExisting);
			this.Controls.Add(this.lstExisting);
			this.Controls.Add(this.chkOverwrite);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.btnPush);
			this.Controls.Add(this.btnCancel);

			this.CancelButton = this.btnCancel;
			this.Shown += this.AndroidAdbPushForm_Shown;
		}

		private void AndroidAdbPushForm_Shown(object sender, EventArgs e)
		{
			this.RefreshListAsync();
		}

		private void lstExisting_DoubleClick(object sender, EventArgs e)
		{
			if (this.lstExisting.SelectedItem != null)
			{
				this.txtFolderName.Text = this.lstExisting.SelectedItem.ToString();
				this.chkOverwrite.Checked = true;
			}
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			this.RefreshListAsync();
		}

		private void RefreshListAsync()
		{
			this.SetBusy(true, "Detecting phone…");
			Task<AndroidAdbListResult> task = AndroidAdbBackup.ListBackupsAsync(this.GetSelectedSerial());
			task.ContinueWith(t =>
			{
				if (this.IsDisposed)
				{
					return;
				}
				this.BeginInvoke(new Action(() => this.ApplyListResult(t)));
			});
		}

		private void ApplyListResult(Task<AndroidAdbListResult> task)
		{
			if (task.IsFaulted)
			{
				this.SetBusy(false, "Error: " + task.Exception.GetBaseException().Message);
				return;
			}

			this.lastList = task.Result;
			this.cmbDevice.Items.Clear();
			if (this.lastList.Devices != null)
			{
				foreach (AndroidAdbDevice dev in this.lastList.Devices)
				{
					string label = dev.Serial;
					if (!string.IsNullOrEmpty(dev.Model))
					{
						label += " — " + dev.Model;
					}
					this.cmbDevice.Items.Add(new DeviceItem(dev, label));
				}
			}
			if (this.cmbDevice.Items.Count > 0)
			{
				this.cmbDevice.SelectedIndex = 0;
			}

			this.lstExisting.Items.Clear();
			if (this.lastList.Success && this.lastList.BackupFolders != null)
			{
				foreach (string folder in this.lastList.BackupFolders)
				{
					this.lstExisting.Items.Add(folder);
				}
			}

			if (!this.lastList.Success)
			{
				this.SetBusy(false, this.lastList.ErrorMessage);
				this.btnPush.Enabled = this.cmbDevice.Items.Count > 0;
				return;
			}

			this.SetBusy(false, "Ready. " + this.lastList.BackupFolders.Count + " folder(s) on phone.");
			this.btnPush.Enabled = this.cmbDevice.Items.Count > 0;
		}

		private async void btnPush_Click(object sender, EventArgs e)
		{
			string folderName;
			try
			{
				folderName = AndroidAdbBackup.NormalizePushFolderName(this.txtFolderName.Text);
			}
			catch (InvalidOperationException ex)
			{
				MessageBox.Show(this, ex.Message, "Folder name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			string serial = this.GetSelectedSerial();
			if (string.IsNullOrEmpty(serial))
			{
				MessageBox.Show(this, "No adb device selected.", "Export && push", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			bool existsOnPhone = this.lastList != null && this.lastList.BackupFolders != null
				&& this.lastList.BackupFolders.Exists(f => string.Equals(f, folderName, StringComparison.OrdinalIgnoreCase));
			bool overwrite = this.chkOverwrite.Checked || existsOnPhone;
			if (existsOnPhone && !this.chkOverwrite.Checked)
			{
				DialogResult warn = MessageBox.Show(this,
					"Folder " + folderName + " already exists on the phone.\n\nOverwrite it?",
					"Confirm overwrite",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning);
				if (warn != DialogResult.Yes)
				{
					return;
				}
				overwrite = true;
			}

			this.SetBusy(true, "Exporting CSVs from codeplug…");
			try
			{
				string staging = AndroidAdbBackup.CreatePushStagingFolder(folderName);
				if (!this.mainForm.ExportAndroidBackupFolder(staging, false))
				{
					throw new InvalidOperationException("Export failed — fix codeplug errors and try again.");
				}

				string remotePath = await AndroidAdbBackup.PushBackupAsync(serial, staging, folderName, overwrite, msg =>
				{
					if (!this.IsDisposed)
					{
						this.BeginInvoke(new Action(() => this.lblStatus.Text = msg));
					}
				});

				this.RemoteFolderName = folderName;
				this.RemotePath = remotePath;
				this.DialogResult = DialogResult.OK;
				MessageBox.Show(this,
					"Pushed to phone:\n" + remotePath + "\n\n" +
					"On the radio app:\n" +
					"1. Open LOCAL\n" +
					"2. Tap IMPORT (OpenGD77)\n" +
					"3. Select folder " + folderName,
					"Export && push complete",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				this.Close();
			}
			catch (Exception ex)
			{
				this.SetBusy(false, ex.Message);
				MessageBox.Show(this, ex.Message, "Export && push failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private string GetSelectedSerial()
		{
			DeviceItem item = this.cmbDevice.SelectedItem as DeviceItem;
			return item == null ? null : item.Device.Serial;
		}

		private void SetBusy(bool busy, string status)
		{
			this.lblStatus.Text = status;
			this.btnRefresh.Enabled = !busy;
			this.btnPush.Enabled = !busy && this.cmbDevice.Items.Count > 0;
			this.cmbDevice.Enabled = !busy;
			this.txtFolderName.Enabled = !busy;
			this.lstExisting.Enabled = !busy;
			this.chkOverwrite.Enabled = !busy;
			this.Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
		}

		private sealed class DeviceItem
		{
			public readonly AndroidAdbDevice Device;
			private readonly string label;

			public DeviceItem(AndroidAdbDevice device, string label)
			{
				this.Device = device;
				this.label = label;
			}

			public override string ToString()
			{
				return this.label;
			}
		}
	}

}