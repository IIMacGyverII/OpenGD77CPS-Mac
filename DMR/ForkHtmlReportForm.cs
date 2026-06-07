using System;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>Large modal HTML report (WebView2) — used from status bar health and backup manager.</summary>
	public class ForkHtmlReportForm : Form
	{
		private readonly ForkWebViewPanel webPanel;
		private readonly string html;
		private readonly Button btnRefresh;

		public event Action<string> CustomNavigation;
		public event Action RefreshRequested;

		private readonly string initialScrollElementId;

		public ForkHtmlReportForm(string title, string html, int width, int height, bool showRefresh = false, string scrollToElementId = null)
		{
			this.html = html ?? "";
			this.initialScrollElementId = scrollToElementId;
			this.Text = title;
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.MinimumSize = new Size(480, 360);
			this.ClientSize = new Size(width, height);
			this.Font = Theme.UiFont;
			Theme.ApplyForkDialog(this);
			this.KeyPreview = true;
			this.KeyDown += this.ForkHtmlReportForm_KeyDown;

			this.webPanel = new ForkWebViewPanel { Dock = DockStyle.Fill };
			this.webPanel.CustomNavigation += this.ForwardCustomNavigation;
			Button btnClose = new Button
			{
				Text = "Close",
				DialogResult = DialogResult.OK,
				Size = new Size(90, 28),
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			Panel bottom = new Panel { Dock = DockStyle.Bottom, Height = 40 };
			int right = width - 12;
			btnClose.Location = new Point(right - btnClose.Width, 6);
			right -= btnClose.Width + 8;
			if (showRefresh)
			{
				this.btnRefresh = new Button
				{
					Text = "Refresh (F5)",
					Size = new Size(110, 28),
					Anchor = AnchorStyles.Top | AnchorStyles.Right
				};
				this.btnRefresh.Location = new Point(right - this.btnRefresh.Width, 6);
				this.btnRefresh.Click += this.btnRefresh_Click;
				bottom.Controls.Add(this.btnRefresh);
			}
			bottom.Controls.Add(btnClose);
			this.Controls.Add(this.webPanel);
			this.Controls.Add(bottom);
			this.AcceptButton = btnClose;
			this.CancelButton = btnClose;

			this.Shown += this.ForkHtmlReportForm_Shown;
		}

		public void NavigateHtml(string html, string scrollToElementId = null)
		{
			this.webPanel.NavigateHtml(html ?? "", scrollToElementId);
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			this.RequestRefresh();
		}

		private void ForkHtmlReportForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5 && this.btnRefresh != null)
			{
				this.RequestRefresh();
				e.Handled = true;
			}
		}

		private void RequestRefresh()
		{
			Action handler = this.RefreshRequested;
			if (handler != null)
			{
				handler();
			}
		}

		private void ForwardCustomNavigation(string uri)
		{
			Action<string> handler = this.CustomNavigation;
			if (handler != null)
			{
				handler(uri);
			}
		}

		private void ForkHtmlReportForm_Shown(object sender, System.EventArgs e)
		{
			this.webPanel.EnsureInitialized();
			this.webPanel.NavigateHtml(this.html, this.initialScrollElementId);
		}
	}
}