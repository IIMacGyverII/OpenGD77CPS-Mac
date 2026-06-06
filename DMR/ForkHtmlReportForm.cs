using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>Large modal HTML report (WebView2) — used from status bar health and backup manager.</summary>
	public class ForkHtmlReportForm : Form
	{
		private readonly ForkWebViewPanel webPanel;
		private readonly string html;

		public ForkHtmlReportForm(string title, string html, int width, int height)
		{
			this.html = html ?? "";
			this.Text = title;
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.MinimumSize = new Size(480, 360);
			this.ClientSize = new Size(width, height);
			this.Font = Theme.UiFont;
			Theme.ApplyForkDialog(this);

			this.webPanel = new ForkWebViewPanel { Dock = DockStyle.Fill };
			Button btnClose = new Button
			{
				Text = "Close",
				DialogResult = DialogResult.OK,
				Size = new Size(90, 28),
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			Panel bottom = new Panel { Dock = DockStyle.Bottom, Height = 40 };
			btnClose.Location = new Point(width - btnClose.Width - 12, 6);
			bottom.Controls.Add(btnClose);
			this.Controls.Add(this.webPanel);
			this.Controls.Add(bottom);
			this.AcceptButton = btnClose;
			this.CancelButton = btnClose;

			this.Shown += this.ForkHtmlReportForm_Shown;
		}

		private void ForkHtmlReportForm_Shown(object sender, System.EventArgs e)
		{
			this.webPanel.EnsureInitialized();
			this.webPanel.NavigateHtml(this.html);
		}
	}
}