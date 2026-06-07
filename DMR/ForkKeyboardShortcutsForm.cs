using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	internal static class ForkKeyboardShortcutsForm
	{
		public static void Show(IWin32Window owner)
		{
			using (Form dlg = new Form())
			{
				dlg.Text = "Keyboard shortcuts — " + AboutForm.FORK_NAME;
				dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
				dlg.MaximizeBox = false;
				dlg.MinimizeBox = false;
				dlg.StartPosition = FormStartPosition.CenterParent;
				dlg.ClientSize = new Size(460, 400);
				dlg.Font = new Font("Segoe UI", 9.75f);
				Theme.ApplyForkDialog(dlg);
				Label body = new Label
				{
					Location = new Point(16, 16),
					Size = new Size(428, 340),
					Text =
						"Global (main window)\n" +
						"  Ctrl+I     Import Android backup folder (Path B)\n" +
						"  Ctrl+E     Export Android backup folder\n" +
						"  Ctrl+O     Open codeplug\n" +
						"  Ctrl+S     Save codeplug\n" +
						"  F1         PriInterPhone workflow help\n" +
						"  F7         Codeplug health report — scrolls to first warning\n" +
						"  F5         Refresh health report (F7 open) or backup report (F8/Studio)\n" +
						"  F8         Android backup manager (diff + import)\n" +
						"             Ctrl+D / Ctrl+I / Ctrl+E / F7 same as Studio while F8 is open\n" +
						"             Recent / drop folder — same recent list as Studio\n" +
						"             Double-click a file row to open that CSV\n" +
						"  Ctrl+Shift+F  Focus navigation tree filter (from anywhere)\n" +
						"  Ctrl+Shift+S  Codeplug Studio — CSV-only backup workflow\n" +
						"  CodeplugStudio.cmd  Thin Studio launcher in install folder\n" +
						"  --studio      Launch Studio only (optional folder path)\n" +
						"  folder path   Drag a backup folder onto OpenGD77CPS.exe to open Studio\n\n" +
						"Codeplug Studio\n" +
						"  F1         Keyboard shortcuts (this dialog)\n" +
						"  Ctrl+O     Browse backup folder\n" +
						"  Ctrl+I     Import all (Path B)\n" +
						"  Ctrl+D     Review diff…\n" +
						"  Ctrl+E     Export all\n" +
						"  F7         Codeplug health report\n" +
						"  F5         Refresh folder validation report\n" +
						"  Recent     Pick a recently used backup folder\n" +
						"  Raw log    Plain-text validation + diff log\n" +
						"  Desktop shortcut  Create --studio shortcut on Desktop\n" +
						"  Right-click folder path  Copy folder path\n" +
						"  Drop folder  Load backup folder from Explorer\n\n" +
						"Navigation tree\n" +
						"  Filter box Search zones, channels, contacts, etc.\n" +
						"  Ctrl+F     Focus tree filter (tree or main window)   Esc  Clear\n" +
						"  Ctrl+Shift+F  Focus tree filter from any editor/grid\n\n" +
						"Channels grid\n" +
						"  F2         Open channel editor\n" +
						"  Ctrl+F     Focus filter   Esc  Clear filter\n" +
						"  Del        Delete selected channel(s)\n" +
						"  Ctrl+D     Duplicate selected channel(s)\n" +
						"  Click row  Open channel in editor\n\n" +
						"Channel editor\n" +
						"  Ctrl+Z     Revert to state when channel was opened\n\n" +
						"Contacts grid\n" +
						"  F2         Open contact editor\n" +
						"  Ctrl+F     Focus filter   Esc  Clear filter\n" +
						"  Del        Delete selected contact(s)\n" +
						"  Click row  Open contact in editor\n" +
						"  Double-click Call ID   Look up on RadioID.net\n\n" +
						"Zones / TG lists / scan lists grids\n" +
						"  F2         Open editor for selected row\n" +
						"  Ctrl+F     Focus filter   Esc  Clear filter\n" +
						"  Click row  Open in editor\n\n" +
						"Zone / TG / scan list editors (Available list)\n" +
						"  Ctrl+F     Focus Available filter   Esc  Clear filter"
				};
				Button ok = new Button
				{
					Location = new Point(360, 362),
					Size = new Size(84, 28),
					Text = "OK",
					DialogResult = DialogResult.OK
				};
				dlg.AcceptButton = ok;
				dlg.Controls.Add(body);
				dlg.Controls.Add(ok);
				dlg.ShowDialog(owner);
			}
		}
	}
}