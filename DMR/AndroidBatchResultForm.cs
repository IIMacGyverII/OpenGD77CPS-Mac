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
		private readonly Label lblStats;
		private readonly Label lblDiffClearedHint;
		private readonly Label lblHealthHint;
		private readonly Button btnHealth;
		private readonly Button btnOk;
		private readonly bool showHealthFollowUp;
		private readonly bool showDiffClearedHint;

		public bool OpenHealthReportRequested { get; private set; }

		public AndroidBatchResultForm(AndroidBatchResult result)
		{
			this.result = result;
			this.Text = this.result.Title;
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.Font = new Font("Segoe UI", 9.75f);
			Theme.ApplyForkDialog(this);

			string healthHint = ForkPostImportUi.BatchDialogHealthHint(this.result);
			this.showHealthFollowUp = !string.IsNullOrEmpty(healthHint);
			this.showDiffClearedHint = this.result.PendingDiffCleared;

			int footerLines = 1;
			if (this.showDiffClearedHint)
			{
				footerLines++;
			}
			if (this.showHealthFollowUp)
			{
				footerLines += 2;
			}
			int minHeight = 320 + (footerLines * 32);
			this.MinimumSize = new Size(480, minHeight);
			this.ClientSize = new Size(520, Math.Max(400, minHeight));

			string statsText = this.result.StatsLine;
			if (ForkPostImportUi.ShouldOfferHealthLink(this.result))
			{
				statsText += ForkPostImportUi.PostImportHealthStatusSuffix();
			}
			this.lblStats = new Label
			{
				Dock = DockStyle.Top,
				AutoSize = false,
				Height = 40,
				Text = statsText
			};

			TextBox txtLog = new TextBox
			{
				Dock = DockStyle.Fill,
				Multiline = true,
				ReadOnly = true,
				ScrollBars = ScrollBars.Both,
				WordWrap = false,
				Font = new Font("Consolas", 9f),
				Text = this.result.DetailText
			};

			this.lblDiffClearedHint = new Label
			{
				Dock = DockStyle.Top,
				AutoSize = true,
				Text = ForkPostImportUi.BatchDialogDiffClearedHintText,
				Visible = this.showDiffClearedHint,
				ForeColor = ForkPostImportUi.WarnColor,
				Margin = new Padding(0, 0, 0, 4)
			};

			this.lblHealthHint = new Label
			{
				Dock = DockStyle.Top,
				AutoSize = true,
				Text = healthHint ?? "",
				Visible = this.showHealthFollowUp,
				ForeColor = ForkPostImportUi.WarnColor,
				Margin = new Padding(0, 0, 0, 6)
			};

			Panel buttonRow = new Panel
			{
				Dock = DockStyle.Top,
				Height = 32
			};

			this.btnHealth = new Button
			{
				Size = new Size(148, 28),
				Visible = this.showHealthFollowUp
			};
			Theme.ApplyStudioButton(this.btnHealth, false, false);
			this.btnHealth.Click += this.BtnHealth_Click;
			if (this.showHealthFollowUp)
			{
				ToolTip healthTip = new ToolTip();
				ForkPostImportUi.ConfigureHealthButton(this.btnHealth, this.result, healthTip);
			}

			this.btnOk = new Button
			{
				Size = new Size(100, 28),
				Text = "OK",
				DialogResult = DialogResult.OK
			};
			this.AcceptButton = this.btnOk;
			this.CancelButton = this.btnOk;
			buttonRow.Controls.Add(this.btnHealth);
			buttonRow.Controls.Add(this.btnOk);
			buttonRow.Resize += (s, e) => this.LayoutFooterButtons(buttonRow);

			Panel footerPanel = new Panel
			{
				Dock = DockStyle.Bottom,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink
			};
			footerPanel.Controls.Add(buttonRow);
			if (this.showHealthFollowUp)
			{
				footerPanel.Controls.Add(this.lblHealthHint);
			}
			if (this.showDiffClearedHint)
			{
				footerPanel.Controls.Add(this.lblDiffClearedHint);
			}

			CheckBox chkWarnings = new CheckBox
			{
				Dock = DockStyle.Bottom,
				AutoSize = true,
				Visible = this.result.Warnings.Count > 0,
				Text = this.result.Warnings.Count + " warning(s) — show in log above",
				Checked = true,
				Enabled = false
			};

			if (this.result.HasErrors)
			{
				this.lblStats.ForeColor = Color.Khaki;
			}
			else if (this.showHealthFollowUp || this.showDiffClearedHint)
			{
				this.lblStats.ForeColor = ForkPostImportUi.WarnColor;
			}
			else
			{
				this.lblStats.ForeColor = Color.LightGreen;
			}

			Panel contentHost = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(12, 12, 12, 6)
			};
			contentHost.Controls.Add(txtLog);
			contentHost.Controls.Add(this.lblStats);

			this.Controls.Add(contentHost);
			if (this.result.Warnings.Count > 0)
			{
				Panel warnHost = new Panel
				{
					Dock = DockStyle.Bottom,
					AutoSize = true,
					AutoSizeMode = AutoSizeMode.GrowAndShrink,
					Padding = new Padding(12, 4, 12, 0)
				};
				warnHost.Controls.Add(chkWarnings);
				this.Controls.Add(warnHost);
			}
			Panel footerHost = new Panel
			{
				Dock = DockStyle.Bottom,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				Padding = new Padding(12, 4, 12, 12)
			};
			footerHost.Controls.Add(footerPanel);
			this.Controls.Add(footerHost);

			if (this.showHealthFollowUp)
			{
				this.KeyPreview = true;
				this.KeyDown += (s, e) =>
				{
					if (e.KeyCode == Keys.F7)
					{
						this.BtnHealth_Click(this.btnHealth, EventArgs.Empty);
						e.Handled = true;
					}
				};
			}

			this.Resize += this.BatchResultForm_Resize;
			this.BatchResultForm_Resize(this, EventArgs.Empty);
			this.LayoutFooterButtons(buttonRow);
		}

		private void BatchResultForm_Resize(object sender, EventArgs e)
		{
			int wrapWidth = Math.Max(200, this.ClientSize.Width - 24);
			if (this.lblDiffClearedHint != null && this.lblDiffClearedHint.Visible)
			{
				this.lblDiffClearedHint.MaximumSize = new Size(wrapWidth, 0);
			}
			if (this.lblHealthHint != null && this.lblHealthHint.Visible)
			{
				this.lblHealthHint.MaximumSize = new Size(wrapWidth, 0);
			}
			if (this.lblStats != null)
			{
				using (Graphics g = this.CreateGraphics())
				{
					SizeF size = g.MeasureString(this.lblStats.Text, this.lblStats.Font, wrapWidth);
					this.lblStats.Height = Math.Max(28, (int)Math.Ceiling(size.Height) + 4);
				}
			}
		}

		private void LayoutFooterButtons(Panel buttonRow)
		{
			if (buttonRow == null || this.btnOk == null)
			{
				return;
			}
			this.btnOk.Location = new Point(Math.Max(0, buttonRow.ClientSize.Width - this.btnOk.Width), 0);
			if (this.btnHealth != null && this.btnHealth.Visible)
			{
				this.btnHealth.Location = new Point(0, 0);
			}
		}

		private void BtnHealth_Click(object sender, EventArgs e)
		{
			this.OpenHealthReportRequested = true;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}

}