using System;
using System.IO;
using System.Windows.Forms;

namespace DMR
{
	/// <summary>
	/// Folder selection for Android CSV backups. Windows FolderBrowserDialog cannot use MTP phone paths.
	/// </summary>
	public static class AndroidBackupFolderPicker
	{
		public const string PickDescriptionImport =
			"Select the backup folder on THIS PC (Desktop, Documents, Downloads, etc.).\r\n\r\n" +
			"Copy the dated folder from your phone first (Download\\DMR\\DMR_Backups\\YYYYMMDD_HHmmss).\r\n" +
			"Do not pick a folder still inside the phone (This PC → phone → Internal storage) — Windows blocks that here.";

		public const string PickDescriptionExport =
			"Select a folder on THIS PC to write Android CSV files.\r\n\r\n" +
			"After export, copy the folder to your phone for import in the radio app.";

		public static string PickFolder(IWin32Window owner, string lastFolder, bool forExport)
		{
			using (FolderBrowserDialog dlg = new FolderBrowserDialog())
			{
				dlg.Description = forExport ? PickDescriptionExport : PickDescriptionImport;
				dlg.RootFolder = Environment.SpecialFolder.Desktop;
				string start = ResolveStartingPath(lastFolder);
				if (!string.IsNullOrEmpty(start))
				{
					dlg.SelectedPath = start;
				}
				if (dlg.ShowDialog(owner) != DialogResult.OK)
				{
					return null;
				}
				if (!IsReadableBackupFolder(dlg.SelectedPath))
				{
					ShowFolderUnavailableHelp(owner, dlg.SelectedPath);
					return null;
				}
				return dlg.SelectedPath;
			}
		}

		public static string ResolveStartingPath(string lastFolder)
		{
			if (IsReadableBackupFolder(lastFolder))
			{
				return lastFolder;
			}
			string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			if (IsReadableBackupFolder(desktop))
			{
				return desktop;
			}
			return null;
		}

		public static bool IsReadableBackupFolder(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return false;
			}
			try
			{
				if (!Directory.Exists(path))
				{
					return false;
				}
				Directory.GetFileSystemEntries(path);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool LooksLikePhoneStoragePath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return false;
			}
			if (path.IndexOf("Internal shared storage", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
			if (path.IndexOf("This PC", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
			if (path.IndexOf("Armor 26", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
			if (path.IndexOf("Comjot", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
			if (path.Length >= 2 && path[1] == ':')
			{
				return false;
			}
			if (path.StartsWith("\\\\", StringComparison.Ordinal))
			{
				return false;
			}
			return !Path.IsPathRooted(path);
		}

		public static void ShowFolderUnavailableHelp(IWin32Window owner, string path)
		{
			string lead = LooksLikePhoneStoragePath(path)
				? "That path is on the phone's USB storage. Windows cannot open it from this dialog."
				: "That folder cannot be read from disk.";

			MessageBox.Show(owner,
				lead + "\n\n" +
				"1. In File Explorer, open This PC → your phone → Internal storage.\n" +
				"2. Go to Download\\DMR\\DMR_Backups\\ and copy the dated folder (e.g. 20260314_131206).\n" +
				"3. Paste it on your PC (Desktop or Documents).\n" +
				"4. In CPS, browse to the copied folder on your PC — or paste the path (e.g. C:\\Users\\You\\Desktop\\20260314_131206).\n\n" +
				(path == null ? "" : ("Tried: " + path)),
				"Copy backup to PC first",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}
	}

}