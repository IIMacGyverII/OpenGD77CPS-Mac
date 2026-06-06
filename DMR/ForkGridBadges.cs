using System;
using System.Drawing;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>Shared DataGridView badge styling (channels + contacts grids).</summary>
	public static class ForkGridBadges
	{
		public static string GetChannelModeBadge(string chModeS)
		{
			if (string.IsNullOrEmpty(chModeS))
			{
				return "-";
			}
			if (chModeS.IndexOf("Digital", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "D";
			}
			if (chModeS.IndexOf("Analog", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "A";
			}
			return chModeS.Substring(0, 1).ToUpperInvariant();
		}

		public static string GetContactCallTypeBadge(string callTypeS)
		{
			if (string.IsNullOrEmpty(callTypeS))
			{
				return "-";
			}
			if (callTypeS.IndexOf("Private", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "P";
			}
			if (callTypeS.IndexOf("Group", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "G";
			}
			if (callTypeS.IndexOf("All", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "A";
			}
			return callTypeS.Substring(0, 1).ToUpperInvariant();
		}

		public static void ApplyChannelModeStyle(DataGridViewCellFormattingEventArgs e, string modeText)
		{
			string badge = GetChannelModeBadge(modeText);
			e.Value = badge;
			e.FormattingApplied = true;
			if (badge == "D")
			{
				e.CellStyle.BackColor = Color.FromArgb(0xD6, 0xE8, 0xF7);
				e.CellStyle.ForeColor = Color.FromArgb(0x1A, 0x4A, 0x7A);
			}
			else if (badge == "A")
			{
				e.CellStyle.BackColor = Color.FromArgb(0xF5, 0xE6, 0xD0);
				e.CellStyle.ForeColor = Color.FromArgb(0x7A, 0x4A, 0x1A);
			}
			e.CellStyle.Font = Theme.UiFontBold;
		}

		public static void ApplyContactTypeStyle(DataGridViewCellFormattingEventArgs e, string badge)
		{
			badge = badge ?? "-";
			switch (badge)
			{
			case "G":
				e.CellStyle.BackColor = Color.FromArgb(0xC8, 0xE6, 0xC9);
				e.CellStyle.ForeColor = Color.FromArgb(0x1B, 0x5E, 0x20);
				break;
			case "P":
				e.CellStyle.BackColor = Color.FromArgb(0xFF, 0xE0, 0xB2);
				e.CellStyle.ForeColor = Color.FromArgb(0xE6, 0x51, 0x00);
				break;
			case "A":
				e.CellStyle.BackColor = Color.FromArgb(0xE0, 0xE0, 0xE0);
				e.CellStyle.ForeColor = Color.FromArgb(0x42, 0x42, 0x42);
				break;
			default:
				e.CellStyle.ForeColor = SystemColors.GrayText;
				break;
			}
			e.CellStyle.Font = Theme.UiFontBold;
		}

		public static void EnableGridPolish(DataGridView grid)
		{
			if (grid == null)
			{
				return;
			}
			typeof(DataGridView).InvokeMember("DoubleBuffered",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
				null, grid, new object[] { true });
			grid.BackgroundColor = Color.White;
			grid.DefaultCellStyle.BackColor = Color.White;
			grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(0xE8, 0xF0, 0xF8);
		}
	}
}