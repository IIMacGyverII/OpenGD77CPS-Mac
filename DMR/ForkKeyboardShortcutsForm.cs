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
				dlg.ClientSize = new Size(460, 320);
				dlg.Font = new Font("Segoe UI", 9.75f);
				Theme.ApplyForkDialog(dlg);
				Label body = new Label
				{
					Location = new Point(16, 16),
					Size = new Size(428, 260),
					Text =
						"Global (main window)\n" +
						"  Ctrl+I     Import Android backup folder (Path B)\n" +
						"  Ctrl+E     Export Android backup folder\n" +
						"  Ctrl+O     Open codeplug\n" +
						"  Ctrl+S     Save codeplug\n" +
						"  F1         PriInterPhone workflow help\n" +
						"  F7         Codeplug health report (HTML)\n" +
						"  F8         Android backup manager (diff + import)\n\n" +
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
					Location = new Point(360, 282),
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