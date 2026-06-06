using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace DMR
{
	/// <summary>
	/// Tier 3.1 — WebView2 host with graceful fallback when runtime is missing.
	/// </summary>
	public class ForkWebViewPanel : UserControl
	{
		private WebView2 webView;
		private Panel fallbackPanel;
		private string pendingHtml;
		private bool initStarted;

		public bool IsWebViewAvailable { get; private set; }

		public ForkWebViewPanel()
		{
			this.Dock = DockStyle.Fill;
			this.BackColor = Theme.Background;
			this.fallbackPanel = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = Theme.Background,
				Visible = false
			};
			Label lbl = new Label
			{
				Dock = DockStyle.Top,
				Height = 72,
				ForeColor = Theme.MutedForeground,
				Text = "WebView2 runtime not available.\r\nInstall Microsoft Edge WebView2 Runtime, or use the Log tab.\r\n"
					+ "https://developer.microsoft.com/microsoft-edge/webview2/"
			};
			LinkLabel lnk = new LinkLabel
			{
				AutoSize = true,
				LinkColor = Color.LightSkyBlue,
				Text = "Download WebView2 Runtime",
				Location = new Point(12, 78)
			};
			lnk.LinkClicked += (s, e) =>
			{
				try
				{
					Process.Start("https://go.microsoft.com/fwlink/p/?LinkId=2124703");
				}
				catch
				{
				}
			};
			this.fallbackPanel.Controls.Add(lnk);
			this.fallbackPanel.Controls.Add(lbl);
			this.Controls.Add(this.fallbackPanel);
			this.Load += this.ForkWebViewPanel_Load;
		}

		public void NavigateHtml(string html)
		{
			this.pendingHtml = html ?? "";
			if (this.IsWebViewAvailable && this.webView != null && this.webView.CoreWebView2 != null)
			{
				this.webView.NavigateToString(this.pendingHtml);
			}
		}

		private void ForkWebViewPanel_Load(object sender, EventArgs e)
		{
			if (this.initStarted)
			{
				return;
			}
			this.initStarted = true;
			this.BeginInvoke(new Action(() => this.InitializeWebViewAsync()));
		}

		private async void InitializeWebViewAsync()
		{
			try
			{
				this.webView = new WebView2
				{
					Dock = DockStyle.Fill,
					DefaultBackgroundColor = Theme.Background
				};
				this.Controls.Add(this.webView);
				this.webView.BringToFront();
				await this.webView.EnsureCoreWebView2Async(null);
				this.IsWebViewAvailable = true;
				this.fallbackPanel.Visible = false;
				if (!string.IsNullOrEmpty(this.pendingHtml))
				{
					this.webView.NavigateToString(this.pendingHtml);
				}
			}
			catch
			{
				this.IsWebViewAvailable = false;
				if (this.webView != null)
				{
					this.Controls.Remove(this.webView);
					this.webView.Dispose();
					this.webView = null;
				}
				this.fallbackPanel.Visible = true;
				this.fallbackPanel.BringToFront();
			}
		}
	}
}