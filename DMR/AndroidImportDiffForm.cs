using System;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Tier 2.5 pre-import diff — Apply commits to Path B import; Cancel leaves codeplug unchanged.
	/// </summary>
	public class AndroidImportDiffForm : Form
	{
		private readonly AndroidImportDiffResult diffResult;
		private readonly DataGridView grid;
		private readonly Label lblSummary;
		private readonly CheckBox chkShowUnchanged;

		public AndroidImportDiffForm(string channelsCsvPath)
		{
			this.diffResult = AndroidImportDiff.Compute(channelsCsvPath);
			this.Text = "Channel import preview";
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.MinimumSize = new Size(640, 420);
			this.ClientSize = new Size(720, 480);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			this.lblSummary = new Label
			{
				Location = new Point(12, 12),
				Size = new Size(696, 40),
				Text = this.diffResult.Summary + "\r\nReview changes before applying Path B import.",
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			this.chkShowUnchanged = new CheckBox
			{
				Location = new Point(12, 56),
				AutoSize = true,
				Text = "Show unchanged channels",
				ForeColor = Theme.Foreground
			};
			this.chkShowUnchanged.CheckedChanged += this.chkShowUnchanged_CheckedChanged;

			this.grid = new DataGridView
			{
				Location = new Point(12, 82),
				Size = new Size(696, 340),
				ReadOnly = true,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				RowHeadersVisible = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				BackgroundColor = Theme.Chrome,
				GridColor = Theme.Accent,
				DefaultCellStyle = new DataGridViewCellStyle
				{
					BackColor = Theme.Background,
					ForeColor = Theme.Foreground,
					SelectionBackColor = Theme.Accent,
					SelectionForeColor = Theme.Foreground
				},
				ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
				{
					BackColor = Theme.Chrome,
					ForeColor = Theme.Foreground
				},
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
			};
			this.grid.Columns.Add("Status", "Status");
			this.grid.Columns.Add("Channel", "Channel");
			this.grid.Columns.Add("Details", "Changed fields");
			this.grid.Columns[0].FillWeight = 12;
			this.grid.Columns[1].FillWeight = 22;
			this.grid.Columns[2].FillWeight = 66;

			Button btnApply = new Button
			{
				Location = new Point(512, 432),
				Size = new Size(96, 28),
				Text = "Apply import",
				DialogResult = DialogResult.OK,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};

			Button btnCancel = new Button
			{
				Location = new Point(612, 432),
				Size = new Size(96, 28),
				Text = "Cancel",
				DialogResult = DialogResult.Cancel,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};

			this.AcceptButton = btnApply;
			this.CancelButton = btnCancel;

			this.Controls.Add(this.lblSummary);
			this.Controls.Add(this.chkShowUnchanged);
			this.Controls.Add(this.grid);
			this.Controls.Add(btnApply);
			this.Controls.Add(btnCancel);

			this.PopulateGrid(false);
		}

		private void chkShowUnchanged_CheckedChanged(object sender, EventArgs e)
		{
			this.PopulateGrid(this.chkShowUnchanged.Checked);
		}

		private void PopulateGrid(bool showUnchanged)
		{
			this.grid.Rows.Clear();
			foreach (AndroidImportDiffRow row in this.diffResult.Rows)
			{
				if (!showUnchanged && row.Status == "Unchanged")
				{
					continue;
				}
				int idx = this.grid.Rows.Add(row.Status, row.ChannelName, row.Details);
				DataGridViewRow gridRow = this.grid.Rows[idx];
				if (row.Status == "Added")
				{
					gridRow.DefaultCellStyle.ForeColor = Color.LightGreen;
				}
				else if (row.Status == "Deleted")
				{
					gridRow.DefaultCellStyle.ForeColor = Color.Salmon;
				}
				else if (row.Status == "Changed")
				{
					gridRow.DefaultCellStyle.ForeColor = Color.Khaki;
				}
			}
		}
	}

}