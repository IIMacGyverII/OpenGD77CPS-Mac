using System.Windows.Forms;

namespace DMR
{
	/// <summary>Esc clears fork grid/tree filter text boxes.</summary>
	internal static class ForkFilterEscape
	{
		public static void WireEscapeClear(TextBox box)
		{
			if (box == null)
			{
				return;
			}
			box.PreviewKeyDown += ForkFilterEscape_TextBox_PreviewKeyDown;
		}

		private static void ForkFilterEscape_TextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode != Keys.Escape)
			{
				return;
			}
			TextBox box = sender as TextBox;
			if (box == null || string.IsNullOrEmpty(box.Text))
			{
				return;
			}
			box.Clear();
			e.IsInputKey = true;
		}
	}
}