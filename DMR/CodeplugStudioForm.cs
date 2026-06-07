using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ReadWriteCsv;

namespace DMR
{
	/// <summary>
	/// Tier 3.2 — thin CSV-only workflow (5 backup files, validation, diff, Path B).
	/// </summary>
	public class CodeplugStudioForm : Form
	{
		private sealed class CsvTileMeta
		{
			public string FileName;
			public string Title;
			public string Glyph;
			public Color Accent;
		}

		private static readonly CsvTileMeta[] BackupTiles = new CsvTileMeta[]
		{
			new CsvTileMeta { FileName = "Channels.csv", Title = "Channels", Glyph = "CH", Accent = Color.FromArgb(0x42, 0xA5, 0xF5) },
			new CsvTileMeta { FileName = "Contacts.csv", Title = "Contacts", Glyph = "CT", Accent = Color.FromArgb(0xAB, 0x47, 0xBC) },
			new CsvTileMeta { FileName = "TG_Lists.csv", Title = "TG Lists", Glyph = "TG", Accent = Color.FromArgb(0xFF, 0xA7, 0x26) },
			new CsvTileMeta { FileName = "Zones.csv", Title = "Zones", Glyph = "ZN", Accent = Color.FromArgb(0x26, 0xA6, 0x96) },
			new CsvTileMeta { FileName = "DTMF.csv", Title = "DTMF", Glyph = "DT", Accent = Color.FromArgb(0x90, 0xA4, 0xAE) }
		};

		private const int TileHeight = 78;
		private const int TileGap = 10;
		private const string IniKeyStudioBounds = "CodeplugStudioBounds";

		private readonly MainForm mainForm;
		private readonly bool standaloneLaunch;
		private TextBox txtFolder;
		private Button btnRecent;
		private ContextMenuStrip recentMenu;
		private FlowLayoutPanel pnlCsvTiles;
		private ForkWebViewPanel webReport;
		private Label lblReportCaption;
		private Label lblReportStatus;
		private Button btnImportAll;
		private Button btnReviewDiff;
		private Button btnExportAll;
		private Button btnOpenFullCps;
		private Button btnHealth;
		private TextBox txtValidation;
		private readonly ToolTip csvTileTip = new ToolTip();
		private readonly ToolTip footerTip = new ToolTip();
		private AndroidBackupValidationResult lastValidation;
		private AndroidImportDiffResult lastDiff;
		private string lastDiffFolder = "";
		private string lastApprovedChannelsStamp = "";
		private bool diffPreApproved;

		public bool UserOpenedFullCps { get; private set; }

		public CodeplugStudioForm(MainForm owner, string launchFolder = null, bool standaloneLaunch = false)
		{
			this.mainForm = owner;
			this.standaloneLaunch = standaloneLaunch;
			this.Text = "PriInterPhone Codeplug Studio";
			this.FormBorderStyle = FormBorderStyle.Sizable;
			if (standaloneLaunch)
			{
				this.ShowInTaskbar = true;
				this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			}
			this.ApplyStudioWindowSize(owner);
			this.Font = Theme.UiFont;
			Theme.ApplyForkDialog(this);
			this.Padding = new Padding(0);

			Panel pnlHeader = this.BuildHeaderPanel();
			Button btnBrowse;
			Button btnPullAdb;
			Button btnPushAdb;
			LinkLabel lnkHelp;
			Panel pnlFolderCard = this.BuildFolderCard(out btnBrowse, out btnRecent, out btnPullAdb, out btnPushAdb, out lnkHelp);
			this.pnlCsvTiles = this.BuildCsvTileRow();
			Panel pnlReportCard = this.BuildReportCard();
			Panel pnlFooter = this.BuildFooterPanel();

			Panel pnlBody = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(16, 12, 16, 8),
				BackColor = Theme.Background
			};
			pnlBody.Controls.Add(pnlReportCard);
			pnlBody.Controls.Add(this.pnlCsvTiles);
			pnlBody.Controls.Add(pnlFolderCard);

			this.Controls.Add(pnlBody);
			this.Controls.Add(pnlFooter);
			this.Controls.Add(pnlHeader);

			btnBrowse.Click += this.btnBrowse_Click;
			btnRecent.Click += this.btnRecent_Click;
			btnPullAdb.Click += this.btnPullAdb_Click;
			btnPushAdb.Click += this.btnPushAdb_Click;
			lnkHelp.LinkClicked += this.lnkHelp_LinkClicked;
			this.txtFolder.Leave += this.txtFolder_Leave;

			this.Shown += this.CodeplugStudioForm_Shown;
			this.Resize += this.CodeplugStudioForm_Resize;
			this.FormClosing += this.CodeplugStudioForm_FormClosing;
			this.KeyPreview = true;
			this.KeyDown += this.CodeplugStudioForm_KeyDown;
			this.EnableFolderDragDrop(this);
			this.EnableFolderDragDrop(pnlBody);
			this.EnableFolderDragDrop(pnlHeader);
			this.EnableFolderDragDrop(pnlFooter);
			this.EnableFolderDragDrop(pnlFolderCard);
			this.EnableFolderDragDrop(pnlReportCard);

			string initial = launchFolder;
			if (string.IsNullOrEmpty(initial))
			{
				initial = IniFileUtils.getProfileStringWithDefault("Setup", "LastAndroidBackupFolder", "");
			}
			if (!string.IsNullOrEmpty(initial))
			{
				this.txtFolder.Text = initial;
				if (AndroidBackupFolderPicker.IsReadableBackupFolder(initial))
				{
					this.SetFolder(initial, false);
				}
			}
			this.RefreshRecentMenu();
		}

		private Panel BuildHeaderPanel()
		{
			Panel header = new Panel
			{
				Dock = DockStyle.Top,
				Height = Theme.Dpi(72),
				BackColor = Theme.Chrome,
				Padding = new Padding(Theme.Dpi(20), Theme.Dpi(14), Theme.Dpi(20), Theme.Dpi(10))
			};
			header.Paint += this.PaintHeaderAccent;

			Label lblTitle = new Label
			{
				Text = "Codeplug Studio",
				Font = new Font("Segoe UI", 18f, FontStyle.Bold),
				ForeColor = Theme.Foreground,
				AutoSize = true,
				Location = new Point(Theme.Dpi(20), Theme.Dpi(12))
			};
			string subText = "PriInterPhone Android CSV workflow  ·  Path B import/export  ·  " + AboutForm.FORK_NAME + " v" + AboutForm.FORK_VERSION;
			if (this.standaloneLaunch)
			{
				subText += "  ·  Standalone (--studio)";
			}
			Label lblSub = new Label
			{
				Text = subText,
				Font = Theme.UiFontSmall,
				ForeColor = Theme.MutedForeground,
				AutoSize = true,
				Location = new Point(Theme.Dpi(20), Theme.Dpi(42))
			};
			LinkLabel lnkShortcuts = new LinkLabel
			{
				Text = "Shortcuts (F1)",
				AutoSize = true,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			Theme.ApplyStudioLink(lnkShortcuts);
			lnkShortcuts.LinkClicked += this.lnkShortcuts_LinkClicked;
			header.Controls.Add(lnkShortcuts);
			header.Controls.Add(lblTitle);
			header.Controls.Add(lblSub);
			header.Resize += (s, e) =>
			{
				lnkShortcuts.Location = new Point(Math.Max(Theme.Dpi(20), header.ClientSize.Width - lnkShortcuts.Width - Theme.Dpi(20)), Theme.Dpi(14));
			};
			lnkShortcuts.Location = new Point(header.ClientSize.Width - lnkShortcuts.Width - Theme.Dpi(20), Theme.Dpi(14));
			return header;
		}

		private Panel BuildFolderCard(out Button btnBrowse, out Button btnRecent, out Button btnPullAdb, out Button btnPushAdb, out LinkLabel lnkHelp)
		{
			Panel card = new StudioCardPanel
			{
				Dock = DockStyle.Top,
				Height = Theme.Dpi(108),
				Margin = new Padding(0, 0, 0, Theme.Dpi(10)),
				Padding = new Padding(Theme.Dpi(14), Theme.Dpi(12), Theme.Dpi(14), Theme.Dpi(10))
			};

			Label lblFolder = new Label
			{
				Text = "BACKUP FOLDER",
				Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
				ForeColor = Theme.MutedForeground,
				AutoSize = true,
				Location = new Point(Theme.Dpi(14), Theme.Dpi(10))
			};

			this.txtFolder = new TextBox
			{
				Location = new Point(Theme.Dpi(14), Theme.Dpi(30)),
				Height = Theme.Dpi(28),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			Theme.ApplyStudioTextBox(this.txtFolder);
			ContextMenuStrip folderMenu = new ContextMenuStrip();
			Theme.ApplyForkContextMenu(folderMenu);
			ToolStripMenuItem mnuCopyPath = new ToolStripMenuItem("Copy folder path");
			mnuCopyPath.Click += this.mnuCopyFolderPath_Click;
			folderMenu.Items.Add(mnuCopyPath);
			this.txtFolder.ContextMenuStrip = folderMenu;

			btnRecent = new Button
			{
				Text = "Recent ▾",
				Size = new Size(Theme.Dpi(88), Theme.Dpi(28)),
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			Theme.ApplyStudioButton(btnRecent, false, false);

			btnBrowse = new Button
			{
				Text = "Browse…",
				Size = new Size(Theme.Dpi(96), Theme.Dpi(28)),
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			Theme.ApplyStudioButton(btnBrowse, false, true);

			this.recentMenu = new ContextMenuStrip();
			Theme.ApplyForkContextMenu(this.recentMenu);

			btnPullAdb = new Button
			{
				Text = "Pull (ADB)",
				Size = new Size(Theme.Dpi(108), Theme.Dpi(28)),
				Location = new Point(Theme.Dpi(14), Theme.Dpi(66))
			};
			Theme.ApplyStudioButton(btnPullAdb, false, false);

			btnPushAdb = new Button
			{
				Text = "Export && push",
				Size = new Size(Theme.Dpi(108), Theme.Dpi(28)),
				Location = new Point(Theme.Dpi(128), Theme.Dpi(66))
			};
			Theme.ApplyStudioButton(btnPushAdb, false, false);

			lnkHelp = new LinkLabel
			{
				Text = "MTP / USB help",
				AutoSize = true,
				Location = new Point(Theme.Dpi(248), Theme.Dpi(72))
			};
			Theme.ApplyStudioLink(lnkHelp);

			Label lblHint = new Label
			{
				Text = "F5 refresh · post-import scrolls to health · click amber status or F7 · drop folder, Recent, or double-click a CSV card",
				Font = Theme.UiFontSmall,
				ForeColor = Theme.MutedForeground,
				AutoSize = true,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};

			card.Controls.Add(lblFolder);
			card.Controls.Add(this.txtFolder);
			card.Controls.Add(btnRecent);
			card.Controls.Add(btnBrowse);
			card.Controls.Add(btnPullAdb);
			card.Controls.Add(btnPushAdb);
			card.Controls.Add(lnkHelp);
			card.Controls.Add(lblHint);
			Button browseForLayout = btnBrowse;
			Button recentForLayout = btnRecent;
			Label hintForLayout = lblHint;
			card.Resize += (s, e) => this.LayoutFolderCard(card, recentForLayout, browseForLayout, hintForLayout);
			return card;
		}

		private void LayoutFolderCard(Panel card, Button recent, Button browse, Label hint)
		{
			int pad = Theme.Dpi(14);
			int gap = Theme.Dpi(8);
			int right = card.ClientSize.Width - pad;
			browse.Location = new Point(right - browse.Width, Theme.Dpi(28));
			recent.Location = new Point(browse.Left - gap - recent.Width, Theme.Dpi(28));
			this.txtFolder.Width = Math.Max(Theme.Dpi(120), recent.Left - pad - gap);
			this.txtFolder.Location = new Point(pad, Theme.Dpi(30));
			hint.Location = new Point(right - hint.PreferredWidth, Theme.Dpi(72));
		}

		private FlowLayoutPanel BuildCsvTileRow()
		{
			FlowLayoutPanel row = new FlowLayoutPanel
			{
				Dock = DockStyle.Top,
				Height = Theme.Dpi(TileHeight + 8),
				Margin = new Padding(0, 0, 0, Theme.Dpi(10)),
				WrapContents = false,
				AutoScroll = false,
				Padding = new Padding(0),
				BackColor = Color.Transparent
			};
			foreach (CsvTileMeta meta in BackupTiles)
			{
				row.Controls.Add(this.CreateCsvTile(meta));
			}
			return row;
		}

		private Panel BuildReportCard()
		{
			StudioCardPanel card = new StudioCardPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(Theme.Dpi(12), Theme.Dpi(10), Theme.Dpi(12), Theme.Dpi(12))
			};

			Panel captionBar = new Panel
			{
				Dock = DockStyle.Top,
				Height = Theme.Dpi(28),
				BackColor = Color.Transparent
			};
			this.lblReportCaption = new Label
			{
				Text = "VALIDATION REPORT",
				Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
				ForeColor = Theme.MutedForeground,
				AutoSize = true,
				Location = new Point(0, Theme.Dpi(4))
			};
			this.lblReportStatus = new Label
			{
				Text = "No folder loaded",
				Font = Theme.UiFontSmall,
				ForeColor = Theme.MutedForeground,
				AutoSize = true,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			captionBar.Controls.Add(this.lblReportCaption);
			captionBar.Controls.Add(this.lblReportStatus);
			captionBar.Resize += (s, e) =>
			{
				this.lblReportStatus.Location = new Point(
					Math.Max(0, captionBar.ClientSize.Width - this.lblReportStatus.PreferredWidth),
					Theme.Dpi(4));
			};

			Panel webHost = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = Theme.StudioTextField,
				Padding = new Padding(1)
			};
			webHost.Paint += (s, e) =>
			{
				Rectangle r = webHost.ClientRectangle;
				r.Width--;
				r.Height--;
				using (Pen pen = new Pen(Theme.StudioCardBorder))
				{
					e.Graphics.DrawRectangle(pen, r);
				}
			};

			this.webReport = new ForkWebViewPanel { Dock = DockStyle.Fill };
			this.webReport.CustomNavigation += this.StudioWebReportNavigate;
			this.txtValidation = new TextBox { Multiline = true, ReadOnly = true, Visible = false };
			webHost.Controls.Add(this.webReport);

			card.Controls.Add(webHost);
			card.Controls.Add(captionBar);
			return card;
		}

		private Panel BuildFooterPanel()
		{
			Panel footer = new Panel
			{
				Dock = DockStyle.Bottom,
				Height = Theme.Dpi(56),
				BackColor = Theme.Chrome,
				Padding = new Padding(Theme.Dpi(16), Theme.Dpi(10), Theme.Dpi(16), Theme.Dpi(10))
			};
			footer.Paint += (s, e) =>
			{
				using (Pen pen = new Pen(Theme.StudioCardBorder))
				{
					e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
				}
			};

			FlowLayoutPanel actions = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false,
				AutoSize = false,
				Padding = new Padding(0),
				BackColor = Color.Transparent
			};

			this.btnImportAll = this.MakeFooterButton("Import all", true, false);
			this.btnReviewDiff = this.MakeFooterButton("Review diff…", false, false);
			this.btnExportAll = this.MakeFooterButton("Export all", false, false);
			Button btnOpenFolder = this.MakeFooterButton("Open folder", false, false);
			this.btnHealth = this.MakeFooterButton("Health (F7)", false, false);
			Button btnRawLog = this.MakeFooterButton("Raw log…", false, false);
			Button btnShortcut = this.MakeFooterButton("Desktop shortcut…", false, false);
			this.btnOpenFullCps = this.MakeFooterButton("Open full editor…", false, true);
			Button btnClose = this.MakeFooterButton("Close", false, false);
			btnClose.DialogResult = DialogResult.OK;
			this.AcceptButton = btnClose;
			this.CancelButton = btnClose;

			this.btnImportAll.Click += this.btnImportAll_Click;
			this.btnReviewDiff.Click += this.btnReviewDiff_Click;
			this.btnExportAll.Click += this.btnExportAll_Click;
			btnOpenFolder.Click += this.btnOpenFolder_Click;
			this.btnHealth.Click += this.btnHealth_Click;
			btnRawLog.Click += this.btnRawLog_Click;
			btnShortcut.Click += this.btnShortcut_Click;
			this.btnOpenFullCps.Click += this.btnOpenFullCps_Click;

			this.btnReviewDiff.Enabled = false;
			this.btnReviewDiff.Margin = new Padding(0, 0, Theme.Dpi(8), 0);
			this.footerTip.SetToolTip(this.btnImportAll, "Path B import all CSVs (Ctrl+I)");
			this.footerTip.SetToolTip(this.btnReviewDiff, "Preview channel changes before import (Ctrl+D)");
			this.footerTip.SetToolTip(this.btnExportAll, "Export codeplug to backup folder (Ctrl+E)");
			this.footerTip.SetToolTip(btnOpenFolder, "Open backup folder in Explorer");
			this.footerTip.SetToolTip(this.btnHealth, "Full codeplug health report (F7)");
			this.footerTip.SetToolTip(this.lblReportCaption, "Re-validate CSVs in the loaded folder (F5)");
			this.footerTip.SetToolTip(btnRawLog, "Plain-text validation, diff, and integrity log");
			this.footerTip.SetToolTip(btnShortcut, "Create PriInterPhone Codeplug Studio shortcut on Desktop (--studio)");
			this.footerTip.SetToolTip(this.btnOpenFullCps, "Show dock, tree, and full CPS editors");
			foreach (Control c in new Control[] { this.btnImportAll, this.btnReviewDiff, this.btnExportAll, btnOpenFolder, this.btnHealth, btnRawLog, btnShortcut, this.btnOpenFullCps, btnClose })
			{
				if (c != this.btnReviewDiff)
				{
					c.Margin = new Padding(0, 0, Theme.Dpi(8), 0);
				}
				actions.Controls.Add(c);
			}

			footer.Controls.Add(actions);
			return footer;
		}

		private Button MakeFooterButton(string text, bool primary, bool accent)
		{
			Button btn = new Button
			{
				Text = text,
				AutoSize = true,
				MinimumSize = new Size(Theme.Dpi(88), Theme.Dpi(32)),
				Padding = new Padding(Theme.Dpi(12), Theme.Dpi(4), Theme.Dpi(12), Theme.Dpi(4))
			};
			Theme.ApplyStudioButton(btn, primary, accent);
			return btn;
		}

		private void ApplyStudioWindowSize(IWin32Window owner)
		{
			this.MinimumSize = new Size(720, 560);
			if (this.TryRestoreStudioBounds())
			{
				return;
			}
			Control ownerControl = owner as Control;
			Screen screen = ownerControl != null && ownerControl.IsHandleCreated
				? Screen.FromControl(ownerControl)
				: Screen.PrimaryScreen;
			Rectangle area = screen.WorkingArea;
			int width = Math.Max(720, (int)Math.Round(area.Width * 0.75));
			int height = Math.Max(560, (int)Math.Round(area.Height * 0.75));
			width = Math.Min(width, area.Width);
			height = Math.Min(height, area.Height);
			this.Size = new Size(width, height);
			this.StartPosition = FormStartPosition.CenterScreen;
		}

		private bool TryRestoreStudioBounds()
		{
			string raw = IniFileUtils.getProfileStringWithDefault("Setup", IniKeyStudioBounds, "");
			if (string.IsNullOrEmpty(raw))
			{
				return false;
			}
			string[] parts = raw.Split(',');
			if (parts.Length != 4)
			{
				return false;
			}
			int x;
			int y;
			int w;
			int h;
			if (!int.TryParse(parts[0], out x) || !int.TryParse(parts[1], out y)
				|| !int.TryParse(parts[2], out w) || !int.TryParse(parts[3], out h))
			{
				return false;
			}
			if (w < this.MinimumSize.Width || h < this.MinimumSize.Height)
			{
				return false;
			}
			Rectangle bounds = new Rectangle(x, y, w, h);
			Rectangle area = Screen.FromRectangle(bounds).WorkingArea;
			if (!bounds.IntersectsWith(area))
			{
				return false;
			}
			this.StartPosition = FormStartPosition.Manual;
			this.Bounds = bounds;
			return true;
		}

		private void SaveStudioBounds()
		{
			Rectangle bounds = this.WindowState == FormWindowState.Normal ? this.Bounds : this.RestoreBounds;
			if (bounds.Width < this.MinimumSize.Width || bounds.Height < this.MinimumSize.Height)
			{
				return;
			}
			string value = bounds.X + "," + bounds.Y + "," + bounds.Width + "," + bounds.Height;
			IniFileUtils.WriteProfileString("Setup", IniKeyStudioBounds, value);
		}

		private void CodeplugStudioForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.mainForm.ClearForkDialogOwner(this);
			this.SaveStudioBounds();
		}

		private void CodeplugStudioForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.O)
			{
				this.btnBrowse_Click(this, EventArgs.Empty);
				e.Handled = true;
				return;
			}
			if (e.Control && e.KeyCode == Keys.I)
			{
				if (this.btnImportAll.Enabled)
				{
					this.btnImportAll_Click(this.btnImportAll, EventArgs.Empty);
				}
				e.Handled = true;
				return;
			}
			if (e.Control && e.KeyCode == Keys.E)
			{
				if (this.btnExportAll.Enabled)
				{
					this.btnExportAll_Click(this.btnExportAll, EventArgs.Empty);
				}
				e.Handled = true;
				return;
			}
			if (e.Control && e.KeyCode == Keys.D)
			{
				if (this.btnReviewDiff.Enabled)
				{
					this.btnReviewDiff_Click(this.btnReviewDiff, EventArgs.Empty);
				}
				e.Handled = true;
				return;
			}
			if (e.KeyCode == Keys.F1)
			{
				ForkKeyboardShortcutsForm.Show(this);
				e.Handled = true;
				return;
			}
			if (e.KeyCode == Keys.F7)
			{
				this.btnHealth_Click(this, EventArgs.Empty);
				e.Handled = true;
				return;
			}
			if (e.KeyCode == Keys.F5)
			{
				string folderPath = this.txtFolder.Text.Trim();
				if (AndroidBackupFolderPicker.IsReadableBackupFolder(folderPath))
				{
					this.SetFolder(folderPath, false);
				}
				e.Handled = true;
			}
		}

		private void lnkShortcuts_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ForkKeyboardShortcutsForm.Show(this);
		}

		private void btnRecent_Click(object sender, EventArgs e)
		{
			this.RefreshRecentMenu();
			if (this.recentMenu.Items.Count == 0)
			{
				return;
			}
			Button btn = sender as Button;
			if (btn == null)
			{
				this.recentMenu.Show(Cursor.Position);
				return;
			}
			this.recentMenu.Show(btn, new Point(0, btn.Height));
		}

		private void RefreshRecentMenu()
		{
			this.recentMenu.Items.Clear();
			IReadOnlyList<string> folders = StudioRecentFolders.Load();
			foreach (string path in folders)
			{
				bool readable = AndroidBackupFolderPicker.IsReadableBackupFolder(path);
				ToolStripMenuItem item = new ToolStripMenuItem(StudioRecentFolders.FormatMenuLabel(path))
				{
					Tag = path,
					ToolTipText = path,
					Enabled = readable
				};
				item.Click += this.recentFolderItem_Click;
				this.recentMenu.Items.Add(item);
			}
			if (this.btnRecent != null)
			{
				this.btnRecent.Enabled = this.recentMenu.Items.Count > 0;
			}
		}

		private void recentFolderItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem item = sender as ToolStripMenuItem;
			string path = item == null ? null : item.Tag as string;
			if (!string.IsNullOrEmpty(path))
			{
				this.SetFolder(path, true);
			}
		}

		private void EnableFolderDragDrop(Control target)
		{
			if (target == null)
			{
				return;
			}
			target.AllowDrop = true;
			target.DragEnter += this.FolderDragEnter;
			target.DragDrop += this.FolderDragDrop;
		}

		private void FolderDragEnter(object sender, DragEventArgs e)
		{
			string path = this.GetFirstDroppedFolder(e);
			if (path != null)
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void FolderDragDrop(object sender, DragEventArgs e)
		{
			string path = this.GetFirstDroppedFolder(e);
			if (!string.IsNullOrEmpty(path))
			{
				this.SetFolder(path, true);
			}
		}

		private string GetFirstDroppedFolder(DragEventArgs e)
		{
			if (e == null || e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				return null;
			}
			string[] paths = e.Data.GetData(DataFormats.FileDrop) as string[];
			if (paths == null)
			{
				return null;
			}
			foreach (string path in paths)
			{
				if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
				{
					return path;
				}
			}
			return null;
		}

		private Panel CreateCsvTile(CsvTileMeta meta)
		{
			StudioCsvTile tile = new StudioCsvTile(meta)
			{
				Height = Theme.Dpi(TileHeight),
				Width = Theme.Dpi(140),
				Margin = new Padding(0, 0, Theme.Dpi(TileGap), 0),
				Tag = meta.FileName,
				Cursor = Cursors.Hand
			};
			tile.DoubleClick += this.csvTile_DoubleClick;
			this.csvTileTip.SetToolTip(tile, "Double-click to open " + meta.FileName);
			return tile;
		}

		private void PaintHeaderAccent(object sender, PaintEventArgs e)
		{
			Panel header = sender as Panel;
			if (header == null)
			{
				return;
			}
			Rectangle bar = new Rectangle(0, header.Height - Theme.Dpi(3), header.Width, Theme.Dpi(3));
			using (LinearGradientBrush brush = new LinearGradientBrush(bar,
				Color.FromArgb(0x43, 0xA0, 0x47),
				Color.FromArgb(0x1E, 0x88, 0xE5),
				LinearGradientMode.Horizontal))
			{
				e.Graphics.FillRectangle(brush, bar);
			}
		}

		private void CodeplugStudioForm_Shown(object sender, EventArgs e)
		{
			this.mainForm.SetForkDialogOwner(this);
			this.webReport.EnsureInitialized();
			this.LayoutCsvTiles();
			this.OfferStudioShortcutOnce();
		}

		private void OfferStudioShortcutOnce()
		{
			if (!this.standaloneLaunch)
			{
				return;
			}
			string offered = IniFileUtils.getProfileStringWithDefault("Setup", "OfferedStudioDesktopShortcut", "");
			if (string.Equals(offered, "yes", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			IniFileUtils.WriteProfileString("Setup", "OfferedStudioDesktopShortcut", "yes");
			DialogResult create = MessageBox.Show(this,
				"Create a Desktop shortcut that opens Codeplug Studio directly?\n\n"
				+ "You can also use Desktop shortcut… in the footer any time.",
				"Codeplug Studio",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			if (create == DialogResult.Yes)
			{
				ForkDesktopShortcut.TryCreateStudioShortcut(this);
			}
		}

		private void StudioWebReportNavigate(string uri)
		{
			this.mainForm.HandleForkReportNavigation(uri);
		}

		private void CodeplugStudioForm_Resize(object sender, EventArgs e)
		{
			this.LayoutCsvTiles();
		}

		private void LayoutCsvTiles()
		{
			if (this.pnlCsvTiles == null || this.pnlCsvTiles.Controls.Count == 0)
			{
				return;
			}
			int count = this.pnlCsvTiles.Controls.Count;
			int gap = Theme.Dpi(TileGap);
			int available = Math.Max(Theme.Dpi(140) * count, this.pnlCsvTiles.ClientSize.Width);
			int tileWidth = Math.Max(Theme.Dpi(120), (available - gap * (count - 1)) / count);
			foreach (Control control in this.pnlCsvTiles.Controls)
			{
				control.Width = tileWidth;
			}
		}

		private void csvTile_DoubleClick(object sender, EventArgs e)
		{
			Panel tile = sender as Panel;
			string fileName = tile == null ? null : tile.Tag as string;
			if (string.IsNullOrEmpty(fileName))
			{
				return;
			}
			string folderPath = this.txtFolder.Text.Trim();
			if (!AndroidBackupFolderPicker.IsReadableBackupFolder(folderPath))
			{
				MessageBox.Show(this, "Choose a valid backup folder first.", "Open CSV",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			string path = Path.Combine(folderPath, fileName);
			if (!File.Exists(path))
			{
				MessageBox.Show(this, fileName + " is missing in this folder.", "Open CSV",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			try
			{
				Process.Start(path);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, path + "\n\n" + ex.Message, "Open CSV",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			string picked = AndroidBackupFolderPicker.PickFolder(this, this.txtFolder.Text, false);
			if (picked != null)
			{
				this.SetFolder(picked, true);
			}
		}

		private void btnPullAdb_Click(object sender, EventArgs e)
		{
			string pulled = AndroidAdbBackup.TryPickPulledFolder(this);
			if (!string.IsNullOrEmpty(pulled))
			{
				if (this.SetFolder(pulled, true))
				{
					AndroidImportDiff.OfferReviewAfterPullIfNeeded(
						this,
						this.lastDiff,
						this.diffPreApproved,
						this.lastValidation != null && this.lastValidation.HasBlockingErrors,
						() => this.TryApproveChannelDiff(pulled));
				}
			}
			else if (!AndroidAdbBackup.IsAdbAvailable())
			{
				MessageBox.Show(this,
					"adb.exe was not found. Copy the backup folder to your PC and use Browse…",
					"ADB not available",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		private void btnPushAdb_Click(object sender, EventArgs e)
		{
			if (!AndroidAdbBackup.IsAdbAvailable())
			{
				MessageBox.Show(this, "adb.exe was not found.", "ADB not available",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			AndroidAdbBackup.TryExportAndPushToPhone(this, this.mainForm);
		}

		private void txtFolder_Leave(object sender, EventArgs e)
		{
			string path = this.txtFolder.Text.Trim();
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			if (AndroidBackupFolderPicker.IsReadableBackupFolder(path))
			{
				this.SetFolder(path, false);
			}
		}

		private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AndroidBackupFolderPicker.ShowFolderUnavailableHelp(this, this.txtFolder.Text.Trim());
		}

		private void UpdateCsvTiles(string folderPath)
		{
			foreach (Control control in this.pnlCsvTiles.Controls)
			{
				StudioCsvTile tile = control as StudioCsvTile;
				if (tile == null)
				{
					continue;
				}
				string fileName = tile.Tag as string;
				if (string.IsNullOrEmpty(fileName))
				{
					continue;
				}
				string path = Path.Combine(folderPath ?? "", fileName);
				if (!File.Exists(path))
				{
					tile.SetTileState(false, 0, "Missing");
					continue;
				}
				int rows = CountCsvDataRows(path);
				tile.SetTileState(true, rows, rows >= 0 ? rows + " rows" : "Found");
			}
		}

		private static int CountCsvDataRows(string path)
		{
			try
			{
				int count = 0;
				using (CsvFileReader reader = new CsvFileReader(path, CsvEncoding.Utf8NoBom))
				{
					CsvRow row = new CsvRow();
					if (!reader.ReadRow(row))
					{
						return 0;
					}
					while (reader.ReadRow(row))
					{
						if (row.Count > 0)
						{
							count++;
						}
					}
				}
				return count;
			}
			catch
			{
				return -1;
			}
		}

		private bool SetFolder(string folderPath, bool showErrors, AndroidBatchResult operationResult = null)
		{
			folderPath = folderPath == null ? "" : folderPath.Trim();
			if (!AndroidBackupFolderPicker.IsReadableBackupFolder(folderPath))
			{
				if (showErrors)
				{
					AndroidBackupFolderPicker.ShowFolderUnavailableHelp(this, folderPath);
				}
				this.btnImportAll.Enabled = false;
				this.UpdateCsvTiles("");
				this.lblReportStatus.Text = "No folder loaded";
				this.lblReportStatus.ForeColor = Theme.MutedForeground;
				this.UpdateStudioTitle("");
				return false;
			}

			this.txtFolder.Text = folderPath;
			this.UpdateStudioTitle(folderPath);
			IniFileUtils.WriteProfileString("Setup", "LastAndroidBackupFolder", folderPath);
			StudioRecentFolders.Record(folderPath);
			this.RefreshRecentMenu();
			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			if (!string.Equals(this.lastDiffFolder, folderPath, StringComparison.OrdinalIgnoreCase))
			{
				this.lastDiffFolder = folderPath;
				this.diffPreApproved = false;
				this.lastApprovedChannelsStamp = "";
			}
			else if (this.diffPreApproved)
			{
				string stamp = AndroidImportDiff.GetChannelsCsvStamp(channelsPath);
				if (!string.Equals(stamp, this.lastApprovedChannelsStamp, StringComparison.Ordinal))
				{
					this.diffPreApproved = false;
				}
			}
			this.UpdateCsvTiles(folderPath);

			this.lastValidation = AndroidBackupValidator.ValidateFolder(folderPath);
			StringBuilder log = new StringBuilder(this.lastValidation.Summary);
			AndroidImportDiffResult diff = null;
			if (File.Exists(channelsPath))
			{
				diff = AndroidImportDiff.Compute(channelsPath);
				this.lastDiff = diff;
				log.Append("\n\n").Append(diff.Summary);
			}
			else
			{
				this.lastDiff = null;
			}
			if (diff != null && File.Exists(channelsPath) && !AndroidImportDiff.HasPendingDiffChanges(diff))
			{
				this.diffPreApproved = true;
				this.lastApprovedChannelsStamp = AndroidImportDiff.GetChannelsCsvStamp(channelsPath);
				this.lastDiffFolder = folderPath;
			}
			AndroidContactIntegrityResult integrity = AndroidContactIntegrityChecker.CheckFolder(folderPath);
			log.Append("\n\n").Append(integrity.Summary);
			if (integrity.HasWarnings && !string.IsNullOrEmpty(integrity.DetailText))
			{
				log.Append("\n\n").Append(integrity.DetailText);
			}
			this.txtValidation.Text = log.ToString();
			this.webReport.EnsureInitialized();
			string scrollId = AndroidBackupReportHtml.GetReportScrollTarget(
				operationResult, diff, integrity, this.lastValidation, this.diffPreApproved);
			this.webReport.NavigateHtml(
				AndroidBackupReportHtml.Build(folderPath, this.lastValidation, diff, integrity, operationResult),
				scrollId);

			string statusSummary = AndroidBackupReportHtml.GetFolderStatusSummary(
				this.lastValidation, integrity, diff, this.diffPreApproved, File.Exists(channelsPath));
			Color statusColor = Color.FromArgb(0x81, 0xC7, 0x84);
			if (this.lastValidation.HasBlockingErrors)
			{
				statusColor = Color.FromArgb(0xEF, 0x53, 0x50);
			}
			else if (integrity.HasWarnings
				|| (diff != null && !this.diffPreApproved && AndroidImportDiff.HasPendingDiffChanges(diff)))
			{
				statusColor = Color.FromArgb(0xFF, 0xB7, 0x4D);
			}
			this.SetReportStatusChip(statusSummary, statusColor);

			this.UpdateDiffImportButtons(channelsPath, diff);
			return true;
		}

		private void UpdateDiffImportButtons(string channelsPath, AndroidImportDiffResult diff)
		{
			bool hasChannels = File.Exists(channelsPath);
			bool pendingDiff = hasChannels && diff != null && AndroidImportDiff.HasPendingDiffChanges(diff) && !this.diffPreApproved;
			this.btnReviewDiff.Enabled = hasChannels;
			this.btnReviewDiff.Text = this.diffPreApproved && hasChannels ? "Diff reviewed ✓" : "Review diff…";
			Theme.ApplyStudioButton(this.btnReviewDiff, false, pendingDiff);
			bool canImport = this.lastValidation != null && !this.lastValidation.HasBlockingErrors && !pendingDiff;
			this.btnImportAll.Enabled = canImport;
			Theme.ApplyStudioButton(this.btnImportAll, canImport, false);
			this.footerTip.SetToolTip(this.btnImportAll,
				pendingDiff ? "Review diff first (Ctrl+D), then import (Ctrl+I)" : "Path B import all CSVs (Ctrl+I)");
		}

		private bool TryApproveChannelDiff(string folderPath)
		{
			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			if (!File.Exists(channelsPath))
			{
				return true;
			}
			if (AndroidImportDiff.IsDiffReviewCurrent(channelsPath, this.diffPreApproved, this.lastApprovedChannelsStamp)
				&& string.Equals(this.lastDiffFolder, folderPath, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			string stamp = AndroidImportDiff.GetChannelsCsvStamp(channelsPath);
			if (!AndroidImportDiff.ShowPreviewDialog(this, channelsPath, this.mainForm))
			{
				return false;
			}
			this.lastDiffFolder = folderPath;
			this.lastApprovedChannelsStamp = stamp;
			this.diffPreApproved = true;
			this.btnReviewDiff.Text = "Diff reviewed ✓";
			this.SetReportStatusChip("Diff reviewed ✓ — ready to import", Color.FromArgb(0x81, 0xC7, 0x84));
			this.UpdateDiffImportButtons(channelsPath, this.lastDiff);
			return true;
		}

		private void btnReviewDiff_Click(object sender, EventArgs e)
		{
			string folderPath = this.txtFolder.Text.Trim();
			if (!this.SetFolder(folderPath, true))
			{
				return;
			}
			this.TryApproveChannelDiff(folderPath);
		}

		private void btnImportAll_Click(object sender, EventArgs e)
		{
			if (!this.SetFolder(this.txtFolder.Text.Trim(), true))
			{
				return;
			}
			if (this.lastValidation != null && this.lastValidation.HasBlockingErrors)
			{
				MessageBox.Show(this, this.lastValidation.Summary, "Cannot import",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (this.lastValidation != null && (this.lastValidation.RelayZeroCount > 0 || this.lastValidation.DuplicateChannelNames > 0))
			{
				DialogResult warn = MessageBox.Show(this,
					this.lastValidation.Summary + "\n\nImport anyway?",
					"Validation warnings",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning);
				if (warn != DialogResult.Yes)
				{
					return;
				}
			}
			string folderPath = this.txtFolder.Text.Trim();
			if (!this.TryApproveChannelDiff(folderPath))
			{
				return;
			}
			AndroidBatchResult batch = this.mainForm.ImportAndroidBackupFolder(
				folderPath, this.diffPreApproved, false, true, this.lastApprovedChannelsStamp);
			if (batch == null)
			{
				return;
			}
			this.SetFolder(folderPath, false, batch);
			this.ApplyBatchStatusChip(batch);
		}

		private void btnExportAll_Click(object sender, EventArgs e)
		{
			string folderPath = this.txtFolder.Text.Trim();
			if (!AndroidBackupFolderPicker.IsReadableBackupFolder(folderPath))
			{
				string picked = AndroidBackupFolderPicker.PickFolder(this, this.txtFolder.Text, true);
				if (picked == null)
				{
					return;
				}
				if (!this.SetFolder(picked, true))
				{
					return;
				}
				folderPath = picked;
			}
			DialogResult confirm = MessageBox.Show(this,
				"Export Contacts, TG Lists, Channels, and Zones to:\n\n" + folderPath + "\n\nContinue?",
				"Export all",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes)
			{
				return;
			}
			AndroidBatchResult batch = this.mainForm.ExportAndroidBackupFolder(folderPath, false);
			if (batch == null)
			{
				return;
			}
			this.SetFolder(folderPath, false, batch);
			this.ApplyBatchStatusChip(batch);
		}

		private void UpdateStudioTitle(string folderPath)
		{
			const string baseTitle = "PriInterPhone Codeplug Studio";
			if (string.IsNullOrEmpty(folderPath))
			{
				this.Text = baseTitle;
				return;
			}
			string name = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			if (string.IsNullOrEmpty(name))
			{
				name = folderPath;
			}
			if (name.Length > 48)
			{
				name = name.Substring(0, 45) + "…";
			}
			this.Text = baseTitle + " — " + name;
		}

		private void SetReportStatusChip(string text, Color color)
		{
			if (this.lblReportStatus == null)
			{
				return;
			}
			ForkPostImportUi.ClearHealthLink(this.lblReportStatus, this.footerTip);
			ForkPostImportUi.ClearHealthButton(this.btnHealth);
			this.lblReportStatus.Text = text;
			this.lblReportStatus.ForeColor = color;
			Control parent = this.lblReportStatus.Parent;
			if (parent != null)
			{
				this.lblReportStatus.Location = new Point(
					Math.Max(0, parent.ClientSize.Width - this.lblReportStatus.PreferredWidth),
					Theme.Dpi(4));
			}
		}

		private void ApplyBatchStatusChip(AndroidBatchResult batch)
		{
			if (batch == null)
			{
				return;
			}
			ForkPostImportUi.ApplyBatchCaption(this.lblReportStatus, batch);
			ForkPostImportUi.ConfigureHealthLink(
				this.lblReportStatus, batch, () => this.mainForm.OpenCodeplugHealthReport(), this.footerTip);
			ForkPostImportUi.ConfigureHealthButton(this.btnHealth, batch);
			Control parent = this.lblReportStatus == null ? null : this.lblReportStatus.Parent;
			if (parent != null && this.lblReportStatus != null)
			{
				this.lblReportStatus.Location = new Point(
					Math.Max(0, parent.ClientSize.Width - this.lblReportStatus.PreferredWidth),
					Theme.Dpi(4));
			}
		}

		private void btnOpenFolder_Click(object sender, EventArgs e)
		{
			string folderPath = this.txtFolder.Text.Trim();
			if (!AndroidBackupFolderPicker.IsReadableBackupFolder(folderPath))
			{
				MessageBox.Show(this, "Choose a valid backup folder first.", "Open folder",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			try
			{
				Process.Start("explorer.exe", folderPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, folderPath + "\n\n" + ex.Message, "Open folder",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void btnHealth_Click(object sender, EventArgs e)
		{
			this.mainForm.OpenCodeplugHealthReport();
		}

		private void mnuCopyFolderPath_Click(object sender, EventArgs e)
		{
			string path = this.txtFolder.Text.Trim();
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			try
			{
				Clipboard.SetText(path);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Copy folder path",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void btnShortcut_Click(object sender, EventArgs e)
		{
			ForkDesktopShortcut.TryCreateStudioShortcut(this);
		}

		private void btnRawLog_Click(object sender, EventArgs e)
		{
			using (Form dlg = new Form())
			{
				dlg.Text = "Codeplug Studio — raw log";
				dlg.StartPosition = FormStartPosition.CenterParent;
				dlg.ClientSize = new Size(560, 400);
				dlg.Font = Theme.UiFont;
				Theme.ApplyForkDialog(dlg);
				TextBox txt = new TextBox
				{
					Dock = DockStyle.Fill,
					Multiline = true,
					ReadOnly = true,
					ScrollBars = ScrollBars.Both,
					Font = new Font("Consolas", 9f),
					Text = this.txtValidation.Text,
					BackColor = Color.FromArgb(0x06, 0x0D, 0x14),
					ForeColor = Color.FromArgb(0xE8, 0xEE, 0xF4)
				};
				dlg.Controls.Add(txt);
				dlg.ShowDialog(this);
			}
		}

		private void btnOpenFullCps_Click(object sender, EventArgs e)
		{
			this.UserOpenedFullCps = true;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private sealed class StudioCardPanel : Panel
		{
			public StudioCardPanel()
			{
				this.BackColor = Theme.StudioCard;
				this.DoubleBuffered = true;
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				base.OnPaint(e);
				Rectangle r = this.ClientRectangle;
				r.Width--;
				r.Height--;
				using (GraphicsPath path = CodeplugStudioForm.RoundRect(r, Theme.Dpi(8)))
				using (Pen pen = new Pen(Theme.StudioCardBorder))
				using (Brush brush = new SolidBrush(this.BackColor))
				{
					e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
					e.Graphics.FillPath(brush, path);
					e.Graphics.DrawPath(pen, path);
				}
			}
		}

		private sealed class StudioCsvTile : Panel
		{
			private readonly CsvTileMeta meta;
			private readonly Label lblGlyph;
			private readonly Label lblTitle;
			private readonly Label lblRows;
			private readonly Label lblStatus;
			private bool tileOk;
			private bool hover;

			public StudioCsvTile(CsvTileMeta meta)
			{
				this.meta = meta;
				this.BackColor = Theme.StudioCard;
				this.DoubleBuffered = true;
				this.Padding = new Padding(Theme.Dpi(10), Theme.Dpi(8), Theme.Dpi(8), Theme.Dpi(8));

				this.lblGlyph = new Label
				{
					Text = meta.Glyph,
					Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
					ForeColor = Color.White,
					TextAlign = ContentAlignment.MiddleCenter,
					Size = new Size(Theme.Dpi(34), Theme.Dpi(34)),
					Location = new Point(Theme.Dpi(10), Theme.Dpi(10)),
					BackColor = meta.Accent
				};

				this.lblTitle = new Label
				{
					Text = meta.Title,
					Font = Theme.UiFontBold,
					ForeColor = Theme.Foreground,
					AutoSize = true,
					Location = new Point(Theme.Dpi(52), Theme.Dpi(10))
				};
				this.lblRows = new Label
				{
					Name = "rows",
					Text = "—",
					Font = new Font("Segoe UI", 16f, FontStyle.Bold),
					ForeColor = Theme.Foreground,
					AutoSize = true,
					Location = new Point(Theme.Dpi(52), Theme.Dpi(28))
				};
				this.lblStatus = new Label
				{
					Name = "status",
					Text = "—",
					Font = Theme.UiFontSmall,
					ForeColor = Theme.MutedForeground,
					AutoSize = true,
					Location = new Point(Theme.Dpi(52), Theme.Dpi(52))
				};

				this.Controls.Add(this.lblGlyph);
				this.Controls.Add(this.lblTitle);
				this.Controls.Add(this.lblRows);
				this.Controls.Add(this.lblStatus);

				this.MouseEnter += (s, e) => { this.hover = true; this.Invalidate(); };
				this.MouseLeave += (s, e) => { this.hover = false; this.Invalidate(); };
				foreach (Control child in this.Controls)
				{
					child.MouseEnter += (s, e) => { this.hover = true; this.Invalidate(); };
					child.MouseLeave += (s, e) => { this.hover = false; this.Invalidate(); };
				}
			}

			public void SetTileState(bool ok, int rows, string statusText)
			{
				this.tileOk = ok;
				this.BackColor = ok ? Theme.StudioCardOk : Theme.StudioCardMiss;
				this.lblRows.Text = ok && rows >= 0 ? rows.ToString() : "—";
				this.lblStatus.Text = statusText;
				this.lblStatus.ForeColor = ok ? Color.FromArgb(0x81, 0xC7, 0x84) : Color.FromArgb(0xEF, 0x9A, 0x9A);
				this.Invalidate();
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				base.OnPaint(e);
				Rectangle r = this.ClientRectangle;
				r.Width--;
				r.Height--;
				Color border = this.hover
					? (this.tileOk ? Color.FromArgb(0x66, 0xBB, 0x6A) : Color.FromArgb(0xEF, 0x53, 0x50))
					: Theme.StudioCardBorder;
				using (GraphicsPath path = CodeplugStudioForm.RoundRect(r, Theme.Dpi(8)))
				using (Pen pen = new Pen(border, this.hover ? 2f : 1f))
				using (Brush brush = new SolidBrush(this.BackColor))
				{
					e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
					e.Graphics.FillPath(brush, path);
					e.Graphics.DrawPath(pen, path);
				}
				Rectangle glyph = new Rectangle(this.lblGlyph.Left, this.lblGlyph.Top, this.lblGlyph.Width, this.lblGlyph.Height);
			}
		}

		private static GraphicsPath RoundRect(Rectangle bounds, int radius)
		{
			int d = radius * 2;
			GraphicsPath path = new GraphicsPath();
			path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
			path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
			path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
			path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}
	}
}