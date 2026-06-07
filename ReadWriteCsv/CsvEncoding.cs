using System.IO;
using System.Text;

namespace ReadWriteCsv
{
	/// <summary>
	/// UTF-8 without BOM for Android/PriInterPhone CSV round-trip (Pitfall 16).
	/// </summary>
	public static class CsvEncoding
	{
		public static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

		public static string StripBom(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return line;
			}
			if (line[0] == '\uFEFF')
			{
				return line.Substring(1);
			}
			return line;
		}

		public static bool FileStartsWithUtf8Bom(string path)
		{
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return false;
			}
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				if (stream.Length < 3)
				{
					return false;
				}
				byte[] header = new byte[3];
				stream.Read(header, 0, 3);
				return header[0] == 0xEF && header[1] == 0xBB && header[2] == 0xBF;
			}
		}
	}
}