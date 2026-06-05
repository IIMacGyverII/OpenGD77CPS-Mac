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
	}
}