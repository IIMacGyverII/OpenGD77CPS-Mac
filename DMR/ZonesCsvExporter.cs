using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReadWriteCsv;

namespace DMR
{
	/// <summary>
	/// Exports Zone data to CSV format compatible with Android PhoneDMRApp
	/// Supports compound key format: channelNum|frequency|name for unique channel identification
	/// </summary>
	public class ZonesCsvExporter
	{
		private const int MAX_CHANNELS_PER_ZONE = ZoneForm.NUM_CHANNELS_PER_ZONE;
		private const bool USE_COMPOUND_KEYS = true; // Set to false for name-only format

		/// <summary>
		/// Export all zones to a CSV file
		/// Format: Zone Name, CH1, CH2, ..., CH80
		/// </summary>
		public static bool ExportZonesToCsv(string filePath)
		{
			try
			{
				using (CsvFileWriter writer = new CsvFileWriter(filePath, false, Encoding.UTF8))
				{
					// Write header row
					CsvRow headerRow = new CsvRow();
					headerRow.Add("Zone Name");
					for (int i = 1; i <= MAX_CHANNELS_PER_ZONE; i++)
					{
						headerRow.Add("CH" + i);
					}
					writer.WriteRow(headerRow);

					// Write each zone
					foreach (ZoneForm.ZoneOne zone in ZoneForm.data.ZoneList)
					{
						if (string.IsNullOrEmpty(zone.Name) || zone.Name == "")
						{
							continue; // Skip empty zones
						}

						CsvRow zoneRow = new CsvRow();
						zoneRow.Add(zone.Name);

						// Add channels (up to 80)
						for (int i = 0; i < MAX_CHANNELS_PER_ZONE; i++)
						{
							ushort channelIndex = zone.ChList[i];
							
							// Empty slot (0 or 65535)
							if (channelIndex == 0 || channelIndex == 65535)
							{
								zoneRow.Add("");
								continue;
							}

							// Convert to 0-indexed
							int channelId = channelIndex - 1;

							// Validate channel exists
							if (!ChannelForm.data.DataIsValid(channelId))
							{
								zoneRow.Add("");
								continue;
							}

							// Get channel reference (compound key or name only)
							string channelRef = GetChannelReference(channelId);
							zoneRow.Add(channelRef);
						}

						writer.WriteRow(zoneRow);
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(
					"Error exporting zones: " + ex.Message,
					"Zone Export Error",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Error);
				return false;
			}
		}

		/// <summary>
		/// Get channel reference string (compound key or name only)
		/// Compound key format: "channelNum|frequency|name"
		/// </summary>
		private static string GetChannelReference(int channelId)
		{
			if (!ChannelForm.data.DataIsValid(channelId))
			{
				return "";
			}

			ChannelForm.ChannelOne channel = ChannelForm.data[channelId];
			
			if (USE_COMPOUND_KEYS)
			{
				// Compound key: "1|446.00625|N1CJ: CARLA 31 Sunset Ridge"
				int channelNum = channelId + 1; // 1-indexed channel number
				double freqMHz = channel.RxFreqDec / 100000.0; // Convert to MHz
				string name = channel.Name;
				
				return string.Format("{0}|{1:0.00000}|{2}", channelNum, freqMHz, name);
			}
			else
			{
				// Name-only format (legacy)
				return channel.Name;
			}
		}
	}
}
