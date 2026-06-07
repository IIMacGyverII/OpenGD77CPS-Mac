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
		internal const string BatchDialogHealthHintText = "Codeplug health warnings remain — open the full report or press OK.";
		internal const string BatchDialogHealthButton = "Health (F7)";
		internal const string PostImportHealthLinkTip = "Click to open codeplug health report (F7)";
		internal const string PostImportHealthButtonWarn = "Health ⚠ (F7)";
		internal const string PostImportReportHealthLink = "Open full health report (F7)";
		internal const string PostImportReportFootWarn = "Click a channel/contact name to open the editor. Amber status, Health ⚠ footer, or F7 for the full report.";
		internal const string PostImportReportFootOk = "Click a name to open the editor.";
		internal const string F8StudioBannerHealthHint = "Post-import scrolls to health · click report link, amber status, Health ⚠ footer, or F7";

		public static bool ImportHasHealthWarnings()
		{
			return CodeplugHealthSnapshot.Collect().HasWarning;
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
				text += ForkFilterEscape.PostImportHealthHint;
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
				toolTip.SetToolTip(label, ForkPostImportUi.PostImportHealthLinkTip);
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

		public static void ConfigureHealthButton(Button button, AndroidBatchResult batch)
		{
			ForkPostImportUi.ApplyHealthButtonState(button, ForkPostImportUi.ShouldOfferHealthLink(batch));
		}

		public static void ClearHealthButton(Button button)
		{
			ForkPostImportUi.ApplyHealthButtonState(button, false);
		}

		private static void ApplyHealthButtonState(Button button, bool highlight)
		{
			if (button == null)
			{
				return;
			}
			button.Text = highlight ? ForkPostImportUi.PostImportHealthButtonWarn : ForkPostImportUi.BatchDialogHealthButton;
			button.ForeColor = highlight ? ForkPostImportUi.WarnColor : Theme.Foreground;
			button.FlatAppearance.BorderColor = highlight ? ForkPostImportUi.WarnColor : Theme.StudioCardBorder;
		}
	}
}