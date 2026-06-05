using System;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Tier 2.4 — scrollable import/export summary (replaces cramped MessageBox chains).
	/// </summary>
	public class AndroidBatchResultForm : Form
	{
		private readonly AndroidBatchResult result;

		public AndroidBatchResultForm(AndroidBatchResult result)
		{
			this.result = result;
			this.Text = this.result.Title;
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.MinimumSize = new Size(480, 360);
			this.ClientSize = new Size(520, 400);
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			Label lblStats = new Label
			{
				Location = new Point(12, 12),
				Size = new Size(496, 36),
				Text = this.result.StatsLine,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			TextBox txtLog = new TextBox
			{
				Location = new Point(12, 52),
				Size = new Size(496, 280),
				Multiline = true,
				ReadOnly = true,
				ScrollBars = ScrollBars.Both,
				WordWrap = false,
				Font = new Font("Consolas", 9f),
				Text = this.result.DetailText,
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
			};

			CheckBox chkWarnings = new CheckBox
			{
				Location = new Point(12, 338),
				AutoSize = true,
				Visible = this.result.Warnings.Count > 0,
				Text = this.result.Warnings.Count + " warning(s) — show in log above",
				Checked = true,
				Enabled = false,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};

			Button btnOk = new Button
			{
				Location = new Point(408, 364),
				Size = new Size(100, 28),
				Text = "OK",
				DialogResult = DialogResult.OK,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};
			this.AcceptButton = btnOk;
			this.CancelButton = btnOk;

			if (this.result.HasErrors)
			{
				lblStats.ForeColor = Color.Khaki;
			}
			else
			{
				lblStats.ForeColor = Color.LightGreen;
			}

			this.Controls.Add(lblStats);
			this.Controls.Add(txtLog);
			this.Controls.Add(chkWarnings);
			this.Controls.Add(btnOk);
		}
	}

}