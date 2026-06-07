using System;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>Shared post-import/export caption styling for F8, Studio, and batch dialogs.</summary>
	internal static class ForkPostImportUi
	{
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

		public static void ConfigureHealthLink(Label label, AndroidBatchResult batch, Action openHealthReport, ToolTip toolTip = null)
		{
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
	}
}