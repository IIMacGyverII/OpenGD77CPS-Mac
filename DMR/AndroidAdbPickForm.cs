using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Pick a phone backup by timestamp and adb pull to a local folder.
	/// </summary>
	public class AndroidAdbPickForm : Form
	{
		private readonly ComboBox cmbDevice;
		private readonly ListBox lstBackups;
		private readonly Label lblStatus;
		private readonly Button btnRefresh;
		private readonly Button btnPull;
		private readonly Button btnAdbPath;
		private readonly Button btnCancel;

		private AndroidAdbListResult lastList;
		private bool suppressDeviceChange;

		public string LocalFolderPath { get; private set; }

		public AndroidAdbPickForm()
		{
			this.Text = "Pull backup from phone (ADB)";
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ClientSize = new Size(460, 360);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			Label lblDevice = new Label
			{
				Location = new Point(12, 12),
				AutoSize = true,
				Text = "Device"
			};
			this.cmbDevice = new ComboBox
			{
				Location = new Point(12, 32),
				Size = new Size(436, 23),
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			this.cmbDevice.SelectedIndexChanged += this.cmbDevice_SelectedIndexChanged;

			Label lblBackups = new Label
			{
				Location = new Point(12, 62),
				AutoSize = true,
				Text = "DMR_Backups on phone (newest first)"
			};
			this.lstBackups = new ListBox
			{
				Location = new Point(12, 82),
				Size = new Size(436, 200)
			};
			this.lstBackups.DoubleClick += this.lstBackups_DoubleClick;

			this.lblStatus = new Label
			{
				Location = new Point(12, 288),
				Size = new Size(436, 32),
				Text = "Detecting adb…"
			};

			this.btnRefresh = new Button
			{
				Location = new Point(12, 324),
				Size = new Size(88, 28),
				Text = "Refresh"
			};
			this.btnRefresh.Click += this.btnRefresh_Click;

			this.btnAdbPath = new Button
			{
				Location = new Point(106, 324),
				Size = new Size(88, 28),
				Text = "ADB path…"
			};
			this.btnAdbPath.Click += this.btnAdbPath_Click;

			this.btnPull = new Button
			{
				Location = new Point(268, 324),
				Size = new Size(88, 28),
				Text = "Pull && use",
				Enabled = false
			};
			this.btnPull.Click += this.btnPull_Click;

			this.btnCancel = new Button
			{
				Location = new Point(360, 324),
				Size = new Size(88, 28),
				Text = "Cancel",
				DialogResult = DialogResult.Cancel
			};

			this.Controls.Add(lblDevice);
			this.Controls.Add(this.cmbDevice);
			this.Controls.Add(lblBackups);
			this.Controls.Add(this.lstBackups);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.btnAdbPath);
			this.Controls.Add(this.btnPull);
			this.Controls.Add(this.btnCancel);

			this.CancelButton = this.btnCancel;
			this.Shown += this.AndroidAdbPickForm_Shown;
		}

		private void AndroidAdbPickForm_Shown(object sender, EventArgs e)
		{
			this.RefreshListAsync();
		}

		private void cmbDevice_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.suppressDeviceChange)
			{
				return;
			}
			this.RefreshBackupsForSelectedDeviceAsync();
		}

		private void lstBackups_DoubleClick(object sender, EventArgs e)
		{
			if (this.btnPull.Enabled)
			{
				this.btnPull_Click(sender, e);
			}
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			this.RefreshListAsync();
		}

		private void btnAdbPath_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dlg = new OpenFileDialog())
			{
				dlg.Title = "Locate adb.exe";
				dlg.Filter = "adb.exe|adb.exe|All files|*.*";
				string current = AndroidAdbBackup.ResolveAdbExecutable();
				if (!string.IsNullOrEmpty(current))
				{
					dlg.InitialDirectory = System.IO.Path.GetDirectoryName(current);
					dlg.FileName = System.IO.Path.GetFileName(current);
				}
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					AndroidAdbBackup.SetConfiguredAdbPath(dlg.FileName);
					this.RefreshListAsync();
				}
			}
		}

		private void RefreshListAsync()
		{
			this.SetBusy(true, "Listing phone backups…");
			string preferredSerial = this.GetSelectedSerial();
			Task<AndroidAdbListResult> task = AndroidAdbBackup.ListBackupsAsync(preferredSerial);
			task.ContinueWith(t =>
			{
				if (this.IsDisposed)
				{
					return;
				}
				this.BeginInvoke(new Action(() => this.ApplyListResult(t)));
			});
		}

		private void RefreshBackupsForSelectedDeviceAsync()
		{
			string serial = this.GetSelectedSerial();
			if (string.IsNullOrEmpty(serial))
			{
				return;
			}
			this.SetBusy(true, "Reading backups on phone…");
			Task<AndroidAdbListResult> task = AndroidAdbBackup.ListBackupsAsync(serial);
			task.ContinueWith(t =>
			{
				if (this.IsDisposed)
				{
					return;
				}
				this.BeginInvoke(new Action(() =>
				{
					if (t.IsFaulted)
					{
						this.SetBusy(false, "Error: " + t.Exception.GetBaseException().Message);
						return;
					}
					this.lastList = t.Result;
					this.PopulateBackupList();
					if (!this.lastList.Success)
					{
						this.SetBusy(false, this.lastList.ErrorMessage);
						return;
					}
					this.SetBusy(false, "Found " + this.lastList.BackupFolders.Count + " backup folder(s) on phone.");
				}));
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
			this.suppressDeviceChange = true;
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
			this.suppressDeviceChange = false;

			if (!this.lastList.Success)
			{
				this.lstBackups.Items.Clear();
				this.btnPull.Enabled = false;
				this.SetBusy(false, this.lastList.ErrorMessage);
				return;
			}

			this.PopulateBackupList();
			this.SetBusy(false, "Found " + this.lastList.BackupFolders.Count + " backup folder(s) on phone.");
		}

		private void PopulateBackupList()
		{
			this.lstBackups.Items.Clear();
			if (this.lastList == null || !this.lastList.Success || this.lastList.BackupFolders == null)
			{
				this.btnPull.Enabled = false;
				return;
			}
			foreach (string folder in this.lastList.BackupFolders)
			{
				this.lstBackups.Items.Add(folder);
			}
			if (this.lstBackups.Items.Count > 0)
			{
				this.lstBackups.SelectedIndex = 0;
			}
			this.btnPull.Enabled = this.lstBackups.Items.Count > 0;
		}

		private async void btnPull_Click(object sender, EventArgs e)
		{
			if (this.lstBackups.SelectedItem == null)
			{
				return;
			}
			string folderName = this.lstBackups.SelectedItem.ToString();
			string serial = this.GetSelectedSerial();
			this.SetBusy(true, "Pulling " + folderName + "…");
			try
			{
				string localPath = await AndroidAdbBackup.PullBackupAsync(serial, folderName, msg =>
				{
					if (!this.IsDisposed)
					{
						this.BeginInvoke(new Action(() => this.lblStatus.Text = msg));
					}
				});
				this.LocalFolderPath = localPath;
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				this.SetBusy(false, ex.Message);
				MessageBox.Show(this, ex.Message, "ADB pull failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			this.btnPull.Enabled = !busy && this.lstBackups.SelectedItem != null;
			this.btnAdbPath.Enabled = !busy;
			this.cmbDevice.Enabled = !busy;
			this.lstBackups.Enabled = !busy;
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