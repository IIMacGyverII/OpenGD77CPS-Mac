using System.Windows.Forms;

namespace DMR
{
	/// <summary>Esc clears and Ctrl+F focuses fork grid/tree filter text boxes.</summary>
	internal static class ForkFilterEscape
	{
		public static bool TryFocusFilter(ref Keys keyData, TextBox box)
		{
			if (keyData != (Keys.Control | Keys.F) || box == null)
			{
				return false;
			}
			box.Focus();
			box.SelectAll();
			return true;
		}

		public static bool TryFocusTreeFilterGlobal(ref Keys keyData, TextBox box)
		{
			if (keyData != (Keys.Control | Keys.Shift | Keys.F) || box == null)
			{
				return false;
			}
			box.Focus();
			box.SelectAll();
			return true;
		}
		public static void WireEscapeClear(TextBox box)
		{
			if (box == null)
			{
				return;
			}
			box.PreviewKeyDown += ForkFilterEscape_TextBox_PreviewKeyDown;
		}

		public static void WireTreeFocusFilter(Control tree, TextBox box)
		{
			if (tree == null || box == null)
			{
				return;
			}
			tree.KeyDown += ForkFilterEscape_Tree_KeyDown;
			tree.Tag = box;
		}

		private static void ForkFilterEscape_Tree_KeyDown(object sender, KeyEventArgs e)
		{
			if (!e.Control || e.KeyCode != Keys.F)
			{
				return;
			}
			Control tree = sender as Control;
			TextBox box = tree == null ? null : tree.Tag as TextBox;
			if (box == null)
			{
				return;
			}
			box.Focus();
			box.SelectAll();
			e.Handled = true;
			e.SuppressKeyPress = true;
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