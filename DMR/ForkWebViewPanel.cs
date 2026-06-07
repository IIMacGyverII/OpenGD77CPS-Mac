using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
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
		private string pendingScrollElementId;
		private bool initStarted;
		private Label statusLabel;

		public bool IsWebViewAvailable { get; private set; }

		public event Action<string> CustomNavigation;

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
			this.statusLabel = new Label
			{
				Dock = DockStyle.Fill,
				ForeColor = Theme.MutedForeground,
				Text = "Loading report…",
				TextAlign = ContentAlignment.MiddleCenter,
				Visible = true
			};
			this.Controls.Add(this.statusLabel);
		}

		/// <summary>Call from parent Form.Shown — WebView2 does not init until the control is shown.</summary>
		public void EnsureInitialized()
		{
			if (this.initStarted)
			{
				return;
			}
			this.initStarted = true;
			if (this.IsHandleCreated)
			{
				this.BeginInvoke(new Action(() => this.InitializeWebViewAsync()));
			}
			else
			{
				this.HandleCreated += (s, e) => this.BeginInvoke(new Action(() => this.InitializeWebViewAsync()));
			}
		}

		public void NavigateHtml(string html, string scrollToElementId = null)
		{
			this.pendingHtml = html ?? "";
			this.pendingScrollElementId = scrollToElementId;
			if (this.IsWebViewAvailable && this.webView != null && this.webView.CoreWebView2 != null)
			{
				this.webView.NavigateToString(this.pendingHtml);
			}
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
				this.webView.CoreWebView2.NavigationStarting += this.CoreWebView2_NavigationStarting;
				this.webView.CoreWebView2.NavigationCompleted += this.CoreWebView2_NavigationCompleted;
				this.IsWebViewAvailable = true;
				this.statusLabel.Visible = false;
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
				this.statusLabel.Visible = false;
				this.fallbackPanel.Visible = true;
				this.fallbackPanel.BringToFront();
			}
		}

		private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			this.TryScrollPending();
		}

		private void TryScrollPending()
		{
			if (string.IsNullOrEmpty(this.pendingScrollElementId) || this.webView == null || this.webView.CoreWebView2 == null)
			{
				return;
			}
			string id = this.pendingScrollElementId;
			this.pendingScrollElementId = null;
			if (id.IndexOf('\'') >= 0 || id.IndexOf('\\') >= 0)
			{
				return;
			}
			string script = "(()=>{var e=document.getElementById('" + id + "');if(e)e.scrollIntoView({block:'start'});})();";
			try
			{
				this.webView.ExecuteScriptAsync(script);
			}
			catch
			{
			}
		}

		private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
		{
			string uri = e.Uri;
			if (string.IsNullOrEmpty(uri) || !uri.StartsWith("fork://", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			e.Cancel = true;
			Action<string> handler = this.CustomNavigation;
			if (handler != null)
			{
				handler(uri);
			}
		}
	}
}