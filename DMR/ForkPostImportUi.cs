using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DMR
{
	internal sealed class ForkPendingDiffSnapshot
	{
		public string FolderPath;
		public AndroidImportDiffResult Diff;
		public bool HasPending;
		public int ChangeCount;
	}

	/// <summary>Shared post-import/export caption styling for F8, Studio, and batch dialogs.</summary>
	internal static class ForkPostImportUi
	{
		internal const string IniKeyDiffApprovedFolder = "ForkDiffApprovedFolder";
		internal const string IniKeyDiffApprovedStamp = "ForkDiffApprovedChannelsStamp";
		internal static readonly Color OkColor = Color.FromArgb(0x81, 0xC7, 0x84);
		internal static readonly Color WarnColor = Color.FromArgb(0xFF, 0xB7, 0x4D);
		internal static readonly Color ErrColor = Color.FromArgb(0xEF, 0x53, 0x50);
		internal static readonly Color HealthLinkColorDefault = Color.FromArgb(0x7E, 0xC8, 0xFF);
		internal const string BatchDialogHealthHintText = "Codeplug health warnings remain — click Health ⚠ (F7) below or OK to continue.";
		internal const string HealthButtonDefaultTip = "Full codeplug health report (F7)";
		internal const string MainToolbarHealthTipDefault = "Codeplug health report (F7) — scrolls to first warning; click names to open editors";
		internal const string BatchDialogHealthButton = "Health (F7)";
		internal const string PostImportHealthLinkTip = "Click to open codeplug health report (F7)";
		internal const string PostImportHealthButtonWarn = "Health ⚠ (F7)";
		internal const string PostImportReportHealthLink = "Open full health report (F7)";
		internal const string PostImportReportPathHint = "Click a highlighted name to open that channel, contact, zone, TG/Rx list, or scan list in the editor.";
		internal const string PostImportReportFootWarn = "Fix issues above or use amber status, Health ⚠ footer/toolbar/menu, or F7 for the full report.";
		internal const string PostImportReportFootOk = "Click a highlighted name to open the editor.";
		internal const string F8StudioBannerHealthHint = "Post-import scrolls to health · click names in health section, report link, amber status, Health ⚠ footer/toolbar, or F7";
		internal const string F8StudioBannerDiffHint = "Pending diff: amber status, Review diff ⚠ footer/toolbar/menu/status bar/report link (Ctrl+D in F8/Studio) · left status after pull";
		internal const string PreImportReportDiffLink = "Open Review diff… (Ctrl+D)";
		internal const string PreImportReportFootWarn = "Review channel changes before Path B import — amber status, Review diff ⚠ footer/toolbar/menu, status bar links, or Ctrl+D in F8/Studio.";
		internal const string ImportBlockedDiffTitle = "Review diff first";
		internal const string ExportOverwriteDiffTitle = "Overwrite pending diff?";
		internal const string BatchDialogDiffClearedHintText = "Review diff ⚠ shell cues cleared — backup Channels.csv matches codeplug.";
		internal const string MainToolbarDiffTipDefault = "Review channel changes before Path B import — Ctrl+D when pending, or F8/Studio";
		internal const string MainImportToolbarTipDefault = "Import PriInterPhone backup folder (File → Import CSV, Path B)";
		internal const string MainBackupToolbarTipDefault = "Android backup manager (F8) — F5 refresh report, Path B import/export";
		internal static readonly Color BackupToolbarColorDefault = Color.FromArgb(0x64, 0xB5, 0xF6);
		internal const string PendingDiffLinkTip = "Click to review channel changes before import (Ctrl+D)";
		internal const string PreImportDiffButtonDefault = "Review diff…";
		internal const string PreImportDiffButtonWarn = "Review diff ⚠ (Ctrl+D)";
		internal const string PreImportImportButtonDefault = "Import all";
		internal const string PreImportImportButtonF8Default = "Import all (Path B)";
		internal const string FolderCaptionDefaultTip = "Re-validate CSVs in the loaded folder (F5)";

		public static bool ImportHasHealthWarnings()
		{
			return CodeplugHealthSnapshot.Collect().HasWarning;
		}

		public static string HealthCategoryStatusNote(CodeplugHealthSnapshot snap)
		{
			if (snap == null || snap.WarningCategoryCount <= 1)
			{
				return "";
			}
			return " (" + snap.WarningCategoryCount + " cat)";
		}

		public static string HealthCategoryTooltip(CodeplugHealthSnapshot snap)
		{
			if (snap == null || !snap.HasWarning)
			{
				return ForkPostImportUi.MainToolbarHealthTipDefault;
			}
			if (snap.WarningCategoryCount > 1)
			{
				return ForkPostImportUi.PostImportHealthLinkTip + " — " + snap.WarningCategoryCount + " warning categories";
			}
			return ForkPostImportUi.PostImportHealthLinkTip;
		}

		public static string HealthMenuLabel(CodeplugHealthSnapshot snap)
		{
			if (snap == null || !snap.HasWarning)
			{
				return "Codeplug health report…";
			}
			if (snap.WarningCategoryCount > 1)
			{
				return "Codeplug health report ⚠ (" + snap.WarningCategoryCount + ")…";
			}
			return "Codeplug health report ⚠…";
		}

		public static string HealthToolbarLabel(CodeplugHealthSnapshot snap)
		{
			if (snap == null || !snap.HasWarning)
			{
				return "Health";
			}
			if (snap.WarningCategoryCount > 1)
			{
				return "Health ⚠ (" + snap.WarningCategoryCount + ")";
			}
			return "Health ⚠";
		}

		public static CodeplugHealthSnapshot CurrentHealthSnapshot()
		{
			return CodeplugHealthSnapshot.Collect();
		}

		public static string PostImportHealthButtonLabel(CodeplugHealthSnapshot snap)
		{
			if (snap != null && snap.WarningCategoryCount > 1)
			{
				return "Health ⚠ (" + snap.WarningCategoryCount + " · F7)";
			}
			return ForkPostImportUi.PostImportHealthButtonWarn;
		}

		public static string PostImportHealthStatusSuffix()
		{
			CodeplugHealthSnapshot snap = ForkPostImportUi.CurrentHealthSnapshot();
			if (!snap.HasWarning)
			{
				return "";
			}
			return ForkFilterEscape.PostImportHealthHint + ForkPostImportUi.HealthCategoryStatusNote(snap);
		}

		public static int PendingDiffChangeCount(AndroidImportDiffResult diff)
		{
			if (diff == null)
			{
				return 0;
			}
			return diff.Added + diff.Changed + diff.Deleted;
		}

		public static string DiffChangeCountNote(AndroidImportDiffResult diff)
		{
			int count = ForkPostImportUi.PendingDiffChangeCount(diff);
			if (count <= 1)
			{
				return "";
			}
			return " (" + count + " ch)";
		}

		public static string DiffButtonLabel(AndroidImportDiffResult diff)
		{
			string note = ForkPostImportUi.DiffChangeCountNote(diff);
			if (!string.IsNullOrEmpty(note))
			{
				return "Review diff ⚠" + note + " · Ctrl+D";
			}
			return ForkPostImportUi.PreImportDiffButtonWarn;
		}

		public static string DiffButtonTooltip(
			AndroidImportDiffResult diff,
			bool diffPreApproved,
			bool hasChannelsCsv,
			bool highlight)
		{
			if (!highlight)
			{
				return "Preview channel changes before import (Ctrl+D)";
			}
			int count = ForkPostImportUi.PendingDiffChangeCount(diff);
			if (count > 1)
			{
				return ForkPostImportUi.PendingDiffLinkTip + " — " + count + " channel changes";
			}
			return ForkPostImportUi.PendingDiffLinkTip;
		}

		public static string PreImportDiffStatusSuffix(AndroidImportDiffResult diff)
		{
			if (diff == null || !AndroidImportDiff.HasPendingDiffChanges(diff))
			{
				return "";
			}
			return ForkFilterEscape.PreImportDiffHint + ForkPostImportUi.DiffChangeCountNote(diff);
		}

		public static string DiffChangeCountToolbarNote(int changeCount)
		{
			if (changeCount <= 1)
			{
				return "";
			}
			return " (" + changeCount + ")";
		}

		public static string DiffToolbarLabel(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return "Review diff";
			}
			return "Review diff ⚠" + ForkPostImportUi.DiffChangeCountToolbarNote(snap.ChangeCount);
		}

		public static string ImportMenuLabel(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return "Import Android backup folder...";
			}
			if (snap.ChangeCount > 1)
			{
				return "Import Android backup folder ⚠ (" + snap.ChangeCount + " ch)...";
			}
			return "Import Android backup folder ⚠...";
		}

		public static string ImportToolbarLabel(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return "Import Android";
			}
			if (snap.ChangeCount > 1)
			{
				return "Import ⚠ (" + snap.ChangeCount + ")";
			}
			return "Import ⚠";
		}

		public static string BackupMenuLabel(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return "Android backup manager...";
			}
			if (snap.ChangeCount > 1)
			{
				return "Android backup manager ⚠ (" + snap.ChangeCount + " ch)...";
			}
			return "Android backup manager ⚠...";
		}

		public static string BackupToolbarLabel(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return "Backup…";
			}
			if (snap.ChangeCount > 1)
			{
				return "Backup ⚠ (" + snap.ChangeCount + ")";
			}
			return "Backup ⚠";
		}

		public static string BackupToolbarTooltip(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return ForkPostImportUi.MainBackupToolbarTipDefault;
			}
			return "Pending channel changes in last backup — open F8 to Review diff (Ctrl+D)";
		}

		public static string OpenBackupFolderTooltip(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return "Open last Android backup folder in Explorer";
			}
			return "Last backup has pending channel changes — open folder or Review diff (Ctrl+D)";
		}

		public static string ImportToolbarTooltip(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return ForkPostImportUi.MainImportToolbarTipDefault;
			}
			return "Pending channel changes in last backup — Review diff first (Ctrl+D) or import shows diff preview";
		}

		public static string DiffMenuLabel(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return "Review channel diff…";
			}
			if (snap.ChangeCount > 1)
			{
				return "Review channel diff ⚠ (" + snap.ChangeCount + ")…";
			}
			return "Review channel diff ⚠…";
		}

		public static string DiffToolbarTooltip(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return ForkPostImportUi.MainToolbarDiffTipDefault;
			}
			if (snap.ChangeCount > 1)
			{
				return ForkPostImportUi.PendingDiffLinkTip + " — " + snap.ChangeCount + " channel changes";
			}
			return ForkPostImportUi.PendingDiffLinkTip;
		}

		public static void MarkPendingDiffReviewed(string folderPath)
		{
			if (string.IsNullOrEmpty(folderPath))
			{
				return;
			}
			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			IniFileUtils.WriteProfileString("Setup", ForkPostImportUi.IniKeyDiffApprovedFolder, folderPath);
			IniFileUtils.WriteProfileString("Setup", ForkPostImportUi.IniKeyDiffApprovedStamp,
				AndroidImportDiff.GetChannelsCsvStamp(channelsPath));
		}

		public static void ClearPendingDiffReviewed(string folderPath)
		{
			string approvedFolder = IniFileUtils.getProfileStringWithDefault("Setup", ForkPostImportUi.IniKeyDiffApprovedFolder, "");
			if (string.IsNullOrEmpty(approvedFolder)
				|| string.IsNullOrEmpty(folderPath)
				|| string.Equals(approvedFolder, folderPath, StringComparison.OrdinalIgnoreCase))
			{
				IniFileUtils.WriteProfileString("Setup", ForkPostImportUi.IniKeyDiffApprovedFolder, "");
				IniFileUtils.WriteProfileString("Setup", ForkPostImportUi.IniKeyDiffApprovedStamp, "");
			}
		}

		public static bool TryRestoreDiffReviewState(string folderPath, out bool diffPreApproved, out string approvedStamp)
		{
			diffPreApproved = false;
			approvedStamp = "";
			if (string.IsNullOrEmpty(folderPath))
			{
				return false;
			}
			string channelsPath = Path.Combine(folderPath, "Channels.csv");
			if (!File.Exists(channelsPath))
			{
				return false;
			}
			string approvedFolder = IniFileUtils.getProfileStringWithDefault("Setup", ForkPostImportUi.IniKeyDiffApprovedFolder, "");
			approvedStamp = IniFileUtils.getProfileStringWithDefault("Setup", ForkPostImportUi.IniKeyDiffApprovedStamp, "");
			diffPreApproved = string.Equals(folderPath, approvedFolder, StringComparison.OrdinalIgnoreCase)
				&& AndroidImportDiff.IsDiffReviewCurrent(channelsPath, true, approvedStamp);
			return diffPreApproved;
		}

		public static bool NotifyImportBlockedByPendingDiff(
			IWin32Window owner,
			AndroidImportDiffResult diff,
			bool diffPreApproved,
			bool hasChannelsCsv,
			Action openReviewDiff)
		{
			if (!ForkPostImportUi.ShouldOfferDiffLink(diff, diffPreApproved, hasChannelsCsv))
			{
				return false;
			}
			DialogResult offer = ForkPostImportUi.ShowImportBlockedByPendingDiffDialog(owner, diff);
			if (offer == DialogResult.Yes && openReviewDiff != null)
			{
				openReviewDiff();
			}
			return true;
		}

		/// <summary>Main-window Ctrl+I: offer Review diff first; Yes stops import, No continues to folder picker/preview.</summary>
		public static bool OfferMainImportReviewPrompt(
			IWin32Window owner,
			ForkPendingDiffSnapshot snap,
			Action openReviewDiff)
		{
			if (snap == null || !snap.HasPending)
			{
				return true;
			}
			DialogResult offer = ForkPostImportUi.ShowImportBlockedByPendingDiffDialog(owner, snap.Diff);
			if (offer == DialogResult.Yes)
			{
				if (openReviewDiff != null)
				{
					openReviewDiff();
				}
				return false;
			}
			return true;
		}

		private static DialogResult ShowImportBlockedByPendingDiffDialog(
			IWin32Window owner,
			AndroidImportDiffResult diff)
		{
			int pending = ForkPostImportUi.PendingDiffChangeCount(diff);
			string suffix = pending > 1 ? " (" + pending + " ch)" : "";
			return MessageBox.Show(owner,
				pending + " channel change(s) need review before Path B import" + suffix + ".\n\n"
					+ "Open Review diff now (Ctrl+D)?\n\n"
					+ "Or use amber status, Review diff ⚠ footer/toolbar/menu, center/left status bar, or the report link.",
				ForkPostImportUi.ImportBlockedDiffTitle,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);
		}

		public static string ImportButtonLabel(
			AndroidImportDiffResult diff,
			bool diffPreApproved,
			bool hasChannelsCsv,
			string defaultLabel)
		{
			if (!ForkPostImportUi.ShouldOfferDiffLink(diff, diffPreApproved, hasChannelsCsv))
			{
				return defaultLabel;
			}
			string note = ForkPostImportUi.DiffChangeCountToolbarNote(
				ForkPostImportUi.PendingDiffChangeCount(diff));
			if (!string.IsNullOrEmpty(note))
			{
				return "Import ⚠" + note;
			}
			return "Import ⚠";
		}

		public static void ConfigureImportButton(
			Button button,
			AndroidImportDiffResult diff,
			bool diffPreApproved,
			bool hasChannelsCsv,
			bool canImport,
			string defaultLabel,
			ToolTip toolTip = null)
		{
			if (button == null)
			{
				return;
			}
			bool highlight = ForkPostImportUi.ShouldOfferDiffLink(diff, diffPreApproved, hasChannelsCsv);
			button.Enabled = canImport;
			Theme.ApplyStudioButton(button, canImport, false);
			button.Text = ForkPostImportUi.ImportButtonLabel(diff, diffPreApproved, hasChannelsCsv, defaultLabel);
			if (highlight)
			{
				button.ForeColor = ForkPostImportUi.WarnColor;
				button.FlatAppearance.BorderColor = ForkPostImportUi.WarnColor;
			}
			if (toolTip != null)
			{
				toolTip.SetToolTip(button,
					highlight
						? "Review diff first (Ctrl+D), then import (Ctrl+I)"
						: "Path B import all CSVs (Ctrl+I)");
			}
		}

		public static ForkPendingDiffSnapshot CollectPendingDiffSnapshot()
		{
			string folderPath = IniFileUtils.getProfileStringWithDefault("Setup", "LastAndroidBackupFolder", "");
			return ForkPostImportUi.CollectFolderPendingDiffSnapshot(folderPath);
		}

		public static ForkPendingDiffSnapshot CollectFolderPendingDiffSnapshot(string folderPath)
		{
			ForkPendingDiffSnapshot snap = new ForkPendingDiffSnapshot();
			snap.FolderPath = folderPath ?? "";
			if (string.IsNullOrEmpty(snap.FolderPath) || !Directory.Exists(snap.FolderPath))
			{
				return snap;
			}
			string channelsPath = Path.Combine(snap.FolderPath, "Channels.csv");
			if (!File.Exists(channelsPath))
			{
				return snap;
			}
			bool diffPreApproved;
			string approvedStamp;
			ForkPostImportUi.TryRestoreDiffReviewState(snap.FolderPath, out diffPreApproved, out approvedStamp);
			try
			{
				snap.Diff = AndroidImportDiff.Compute(channelsPath);
			}
			catch
			{
				return snap;
			}
			snap.HasPending = ForkPostImportUi.ShouldOfferDiffLink(snap.Diff, diffPreApproved, true);
			snap.ChangeCount = ForkPostImportUi.PendingDiffChangeCount(snap.Diff);
			return snap;
		}

		public static bool OfferExportOverwritePendingDiff(IWin32Window owner, ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return true;
			}
			string suffix = snap.ChangeCount > 1 ? " (" + snap.ChangeCount + " ch)" : "";
			DialogResult offer = MessageBox.Show(owner,
				snap.ChangeCount + " unreviewed channel change(s) in this backup" + suffix + ".\n\n"
					+ "Export will overwrite Channels.csv with your loaded codeplug.\n\n"
					+ "Continue export anyway?",
				ForkPostImportUi.ExportOverwriteDiffTitle,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);
			return offer == DialogResult.Yes;
		}

		public static void ConfigureDiffButton(
			Button button,
			AndroidImportDiffResult diff,
			bool diffPreApproved,
			bool hasChannelsCsv,
			ToolTip toolTip = null)
		{
			bool highlight = ForkPostImportUi.ShouldOfferDiffLink(diff, diffPreApproved, hasChannelsCsv);
			ForkPostImportUi.ApplyDiffButtonState(button, highlight, diff, diffPreApproved, hasChannelsCsv);
			if (toolTip != null)
			{
				toolTip.SetToolTip(button, ForkPostImportUi.DiffButtonTooltip(diff, diffPreApproved, hasChannelsCsv, highlight));
			}
		}

		public static void ClearDiffButton(Button button, ToolTip toolTip = null)
		{
			ForkPostImportUi.ApplyDiffButtonState(button, false, null, false, false);
			if (toolTip != null)
			{
				toolTip.SetToolTip(button, "Preview channel changes before import (Ctrl+D)");
			}
		}

		public static void ApplyBatchCaption(Label label, AndroidBatchResult batch)
		{
			if (label == null || batch == null)
			{
				return;
			}
			if (batch.HasErrors)
			{
				label.Text = batch.Operation + " failed — see report";
				label.ForeColor = ForkPostImportUi.ErrColor;
				return;
			}
			string prefix = string.Equals(batch.Operation, "Export", StringComparison.OrdinalIgnoreCase) ? "Exported" : "Import";
			string text = prefix + " complete ✓ — " + batch.StatsLine;
			label.ForeColor = ForkPostImportUi.OkColor;
			if (string.Equals(batch.Operation, "Import", StringComparison.OrdinalIgnoreCase)
				&& ForkPostImportUi.ImportHasHealthWarnings())
			{
				text += ForkPostImportUi.PostImportHealthStatusSuffix();
				label.ForeColor = ForkPostImportUi.WarnColor;
			}
			label.Text = text;
		}

		public static string BatchDialogHealthHint(AndroidBatchResult batch)
		{
			if (batch == null
				|| batch.HasErrors
				|| !string.Equals(batch.Operation, "Import", StringComparison.OrdinalIgnoreCase)
				|| !ForkPostImportUi.ImportHasHealthWarnings())
			{
				return null;
			}
			CodeplugHealthSnapshot snap = ForkPostImportUi.CurrentHealthSnapshot();
			if (snap.WarningCategoryCount > 1)
			{
				return "Codeplug health warnings remain in " + snap.WarningCategoryCount
					+ " categories — click Health ⚠ (F7) below or OK to continue.";
			}
			return ForkPostImportUi.BatchDialogHealthHintText;
		}

		public static bool ShouldOfferHealthLink(AndroidBatchResult batch)
		{
			return batch != null
				&& !batch.HasErrors
				&& string.Equals(batch.Operation, "Import", StringComparison.OrdinalIgnoreCase)
				&& ForkPostImportUi.ImportHasHealthWarnings();
		}

		public static bool ShouldOfferDiffLink(AndroidImportDiffResult diff, bool diffPreApproved, bool hasChannelsCsv)
		{
			return hasChannelsCsv
				&& diff != null
				&& !diffPreApproved
				&& AndroidImportDiff.HasPendingDiffChanges(diff);
		}

		public static void ConfigureDiffLink(Label label, AndroidImportDiffResult diff, bool diffPreApproved, bool hasChannelsCsv, Action openReviewDiff, ToolTip toolTip = null)
		{
			ForkPostImportUi.ClearDiffLink(label, toolTip);
			if (label == null || openReviewDiff == null || !ForkPostImportUi.ShouldOfferDiffLink(diff, diffPreApproved, hasChannelsCsv))
			{
				return;
			}
			label.Cursor = Cursors.Hand;
			EventHandler handler = (s, e) => openReviewDiff();
			label.Tag = handler;
			label.Click += handler;
			if (toolTip != null)
			{
				toolTip.SetToolTip(label, ForkPostImportUi.PendingDiffLinkTip);
			}
		}

		public static void ClearDiffLink(Label label, ToolTip toolTip = null, string defaultTip = null)
		{
			if (label == null)
			{
				return;
			}
			EventHandler handler = label.Tag as EventHandler;
			if (handler != null)
			{
				label.Click -= handler;
				label.Tag = null;
			}
			label.Cursor = Cursors.Default;
			if (toolTip != null && !string.IsNullOrEmpty(defaultTip))
			{
				toolTip.SetToolTip(label, defaultTip);
			}
		}

		public static Color FolderStatusCaptionColor(
			AndroidBackupValidationResult validation,
			AndroidContactIntegrityResult integrity,
			AndroidImportDiffResult diff,
			bool diffPreApproved,
			bool hasChannelsCsv)
		{
			if (validation != null && validation.HasBlockingErrors)
			{
				return ForkPostImportUi.ErrColor;
			}
			if (integrity != null && integrity.HasWarnings)
			{
				return ForkPostImportUi.WarnColor;
			}
			if (ForkPostImportUi.ShouldOfferDiffLink(diff, diffPreApproved, hasChannelsCsv))
			{
				return ForkPostImportUi.WarnColor;
			}
			if (validation != null && !validation.HasBlockingErrors)
			{
				return ForkPostImportUi.OkColor;
			}
			return Theme.MutedForeground;
		}

		public static void ConfigureHealthLink(Label label, AndroidBatchResult batch, Action openHealthReport, ToolTip toolTip = null)
		{
			ForkPostImportUi.ClearDiffLink(label, toolTip);
			ForkPostImportUi.ClearHealthLink(label, toolTip);
			if (label == null || openHealthReport == null || !ForkPostImportUi.ShouldOfferHealthLink(batch))
			{
				return;
			}
			label.Cursor = Cursors.Hand;
			EventHandler handler = (s, e) => openHealthReport();
			label.Tag = handler;
			label.Click += handler;
			if (toolTip != null)
			{
				toolTip.SetToolTip(label, ForkPostImportUi.HealthCategoryTooltip(ForkPostImportUi.CurrentHealthSnapshot()));
			}
		}

		public static void ClearHealthLink(Label label, ToolTip toolTip = null, string defaultTip = null)
		{
			if (label == null)
			{
				return;
			}
			EventHandler handler = label.Tag as EventHandler;
			if (handler != null)
			{
				label.Click -= handler;
				label.Tag = null;
			}
			label.Cursor = Cursors.Default;
			if (toolTip != null && !string.IsNullOrEmpty(defaultTip))
			{
				toolTip.SetToolTip(label, defaultTip);
			}
		}

		public static void ConfigureHealthButton(Button button, AndroidBatchResult batch, ToolTip toolTip = null)
		{
			bool highlight = ForkPostImportUi.ShouldOfferHealthLink(batch);
			ForkPostImportUi.ApplyHealthButtonState(button, highlight);
			if (toolTip != null)
			{
				toolTip.SetToolTip(button, highlight
					? ForkPostImportUi.HealthCategoryTooltip(ForkPostImportUi.CurrentHealthSnapshot())
					: ForkPostImportUi.HealthButtonDefaultTip);
			}
		}

		public static void ClearHealthButton(Button button, ToolTip toolTip = null)
		{
			ForkPostImportUi.ApplyHealthButtonState(button, false);
			if (toolTip != null)
			{
				toolTip.SetToolTip(button, ForkPostImportUi.HealthButtonDefaultTip);
			}
		}

		public static void ApplyMainHealthStatusLink(ToolStripStatusLabel label, CodeplugHealthSnapshot snap)
		{
			if (label == null)
			{
				return;
			}
			bool hasWarning = snap != null && snap.HasWarning;
			label.ForeColor = hasWarning ? ForkPostImportUi.WarnColor : Theme.Foreground;
			label.LinkColor = hasWarning ? ForkPostImportUi.WarnColor : ForkPostImportUi.HealthLinkColorDefault;
			label.VisitedLinkColor = hasWarning ? ForkPostImportUi.WarnColor : ForkPostImportUi.HealthLinkColorDefault;
			label.ActiveLinkColor = Color.White;
			label.ToolTipText = ForkPostImportUi.HealthCategoryTooltip(snap);
		}

		public static string MainPendingDiffStatusLabel(ForkPendingDiffSnapshot snap)
		{
			if (snap == null || !snap.HasPending)
			{
				return "";
			}
			return "▶ Review diff ⚠" + ForkPostImportUi.DiffChangeCountToolbarNote(snap.ChangeCount);
		}

		public static void ApplyMainPendingDiffStatusLink(ToolStripStatusLabel label, ForkPendingDiffSnapshot snap)
		{
			if (label == null)
			{
				return;
			}
			bool hasPending = snap != null && snap.HasPending;
			label.Visible = hasPending;
			if (!hasPending)
			{
				label.Text = "";
				label.ToolTipText = "";
				return;
			}
			label.Text = ForkPostImportUi.MainPendingDiffStatusLabel(snap);
			label.ForeColor = ForkPostImportUi.WarnColor;
			label.LinkColor = ForkPostImportUi.WarnColor;
			label.VisitedLinkColor = ForkPostImportUi.WarnColor;
			label.ActiveLinkColor = Color.White;
			label.ToolTipText = ForkPostImportUi.DiffToolbarTooltip(snap);
		}

		private static void ApplyHealthButtonState(Button button, bool highlight)
		{
			if (button == null)
			{
				return;
			}
			button.Text = highlight
				? ForkPostImportUi.PostImportHealthButtonLabel(ForkPostImportUi.CurrentHealthSnapshot())
				: ForkPostImportUi.BatchDialogHealthButton;
			button.ForeColor = highlight ? ForkPostImportUi.WarnColor : Theme.Foreground;
			button.FlatAppearance.BorderColor = highlight ? ForkPostImportUi.WarnColor : Theme.StudioCardBorder;
		}

		private static void ApplyDiffButtonState(
			Button button,
			bool highlight,
			AndroidImportDiffResult diff,
			bool diffPreApproved,
			bool hasChannelsCsv)
		{
			if (button == null)
			{
				return;
			}
			Theme.ApplyStudioButton(button, false, false);
			if (diffPreApproved && hasChannelsCsv)
			{
				button.Text = "Diff reviewed ✓";
			}
			else
			{
				button.Text = highlight
					? ForkPostImportUi.DiffButtonLabel(diff)
					: ForkPostImportUi.PreImportDiffButtonDefault;
			}
			if (highlight)
			{
				button.ForeColor = ForkPostImportUi.WarnColor;
				button.FlatAppearance.BorderColor = ForkPostImportUi.WarnColor;
			}
		}
	}
}