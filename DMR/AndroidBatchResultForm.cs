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

		public bool OpenHealthReportRequested { get; private set; }

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

			string statsText = this.result.StatsLine;
			if (ForkPostImportUi.ShouldOfferHealthLink(this.result))
			{
				statsText += ForkFilterEscape.PostImportHealthHint;
			}
			Label lblStats = new Label
			{
				Location = new Point(12, 12),
				Size = new Size(496, 36),
				Text = statsText,
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

			string healthHint = ForkPostImportUi.BatchDialogHealthHint(this.result);
			bool showHealthFollowUp = !string.IsNullOrEmpty(healthHint);
			Label lblHealthHint = new Label
			{
				Location = new Point(12, 360),
				Size = new Size(496, 32),
				Text = healthHint ?? "",
				Visible = showHealthFollowUp,
				ForeColor = ForkPostImportUi.WarnColor,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
			};

			Button btnHealth = new Button
			{
				Location = new Point(12, showHealthFollowUp ? 392 : 364),
				Size = new Size(132, 28),
				Visible = showHealthFollowUp,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};
			Theme.ApplyStudioButton(btnHealth, false, false);
			btnHealth.Click += this.BtnHealth_Click;
			ToolTip healthTip = new ToolTip();
			if (showHealthFollowUp)
			{
				ForkPostImportUi.ConfigureHealthButton(btnHealth, this.result, healthTip);
			}

			Button btnOk = new Button
			{
				Location = new Point(408, showHealthFollowUp ? 392 : 364),
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
			else if (showHealthFollowUp)
			{
				lblStats.ForeColor = ForkPostImportUi.WarnColor;
			}
			else
			{
				lblStats.ForeColor = Color.LightGreen;
			}

			this.Controls.Add(lblStats);
			this.Controls.Add(txtLog);
			this.Controls.Add(chkWarnings);
			if (showHealthFollowUp)
			{
				this.Controls.Add(lblHealthHint);
				this.Controls.Add(btnHealth);
				this.ClientSize = new Size(520, 432);
				this.KeyPreview = true;
				this.KeyDown += (s, e) =>
				{
					if (e.KeyCode == Keys.F7)
					{
						this.BtnHealth_Click(btnHealth, EventArgs.Empty);
						e.Handled = true;
					}
				};
			}
			this.Controls.Add(btnOk);
		}

		private void BtnHealth_Click(object sender, EventArgs e)
		{
			this.OpenHealthReportRequested = true;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}

}