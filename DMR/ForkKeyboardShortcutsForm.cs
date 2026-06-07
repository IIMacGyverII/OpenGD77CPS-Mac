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
				dlg.ClientSize = new Size(460, 360);
				dlg.Font = new Font("Segoe UI", 9.75f);
				Theme.ApplyForkDialog(dlg);
				Label body = new Label
				{
					Location = new Point(16, 16),
					Size = new Size(428, 300),
					Text =
						"Global (main window)\n" +
						"  Ctrl+I     Import Android backup folder (Path B)\n" +
						"  Ctrl+E     Export Android backup folder\n" +
						"  Ctrl+O     Open codeplug\n" +
						"  Ctrl+S     Save codeplug\n" +
						"  F1         PriInterPhone workflow help\n" +
						"  F7         Codeplug health report — click names to open editors\n" +
						"  F8         Android backup manager (diff + import)\n" +
						"             Ctrl+D / Ctrl+I / Ctrl+E / F7 same as Studio while F8 is open\n" +
						"             Double-click a file row to open that CSV\n" +
						"  Ctrl+Shift+S  Codeplug Studio — CSV-only backup workflow\n" +
						"  --studio      Launch Studio only (optional folder path)\n\n" +
						"Codeplug Studio\n" +
						"  F1         Keyboard shortcuts (this dialog)\n" +
						"  Ctrl+O     Browse backup folder\n" +
						"  Ctrl+I     Import all (Path B)\n" +
						"  Ctrl+D     Review diff…\n" +
						"  Ctrl+E     Export all\n" +
						"  F7         Codeplug health report\n" +
						"  Recent     Pick a recently used backup folder\n" +
						"  Drop folder  Load backup folder from Explorer\n\n" +
						"Navigation tree\n" +
						"  Filter box Search zones, channels, contacts, etc.\n\n" +
						"Channels grid\n" +
						"  F2         Open channel editor\n" +
						"  Del        Delete selected channel(s)\n" +
						"  Ctrl+D     Duplicate selected channel(s)\n" +
						"  Click row  Open channel in editor\n\n" +
						"Channel editor\n" +
						"  Ctrl+Z     Revert to state when channel was opened\n\n" +
						"Contacts grid\n" +
						"  F2         Open contact editor\n" +
						"  Click row  Open contact in editor\n" +
						"  Double-click Call ID   Look up on RadioID.net"
				};
				Button ok = new Button
				{
					Location = new Point(360, 322),
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