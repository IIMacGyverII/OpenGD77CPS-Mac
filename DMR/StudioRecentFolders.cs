using System;
using System.Collections.Generic;
using System.IO;

namespace DMR
{
	/// <summary>
	/// Persists recently used Android backup folders (Studio + F8).
	/// </summary>
	internal static class StudioRecentFolders
	{
		private const string IniKey = "StudioRecentBackupFolders";
		private const char Separator = '|';
		private const int MaxEntries = 8;

		public static IReadOnlyList<string> Load()
		{
			string raw = IniFileUtils.getProfileStringWithDefault("Setup", IniKey, "");
			List<string> result = new List<string>();
			if (string.IsNullOrEmpty(raw))
			{
				SeedFromLastFolder(result);
				return result;
			}
			string[] parts = raw.Split(Separator);
			foreach (string part in parts)
			{
				string path = part == null ? "" : part.Trim();
				if (string.IsNullOrEmpty(path))
				{
					continue;
				}
				if (result.Contains(path))
				{
					continue;
				}
				result.Add(path);
				if (result.Count >= MaxEntries)
				{
					break;
				}
			}
			if (result.Count == 0)
			{
				SeedFromLastFolder(result);
			}
			return result;
		}

		public static void Record(string folderPath)
		{
			folderPath = folderPath == null ? "" : folderPath.Trim();
			if (string.IsNullOrEmpty(folderPath))
			{
				return;
			}
			List<string> entries = new List<string>(Load());
			entries.RemoveAll(p => string.Equals(p, folderPath, StringComparison.OrdinalIgnoreCase));
			entries.Insert(0, folderPath);
			while (entries.Count > MaxEntries)
			{
				entries.RemoveAt(entries.Count - 1);
			}
			Save(entries);
		}

		public static string FormatMenuLabel(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return "";
			}
			string name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			if (string.IsNullOrEmpty(name))
			{
				name = path;
			}
			if (name.Length > 42)
			{
				name = name.Substring(0, 39) + "…";
			}
			return name;
		}

		private static void SeedFromLastFolder(List<string> result)
		{
			string last = IniFileUtils.getProfileStringWithDefault("Setup", "LastAndroidBackupFolder", "");
			if (!string.IsNullOrEmpty(last) && !result.Contains(last))
			{
				result.Add(last);
			}
		}

		private static void Save(List<string> entries)
		{
			IniFileUtils.WriteProfileString("Setup", IniKey, string.Join(Separator.ToString(), entries.ToArray()));
		}
	}
}