using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ReadWriteCsv;

namespace DMR
{
	/// <summary>
	/// Imports Zone data from CSV format compatible with Android PhoneDMRApp
	/// Auto-detects compound key format (channelNum|frequency|name) vs name-only format
	/// </summary>
	public class ZonesCsvImporter
	{
		private const int MAX_CHANNELS_PER_ZONE = ZoneForm.NUM_CHANNELS_PER_ZONE;

		/// <summary>
		/// Import zones from a CSV file
		/// Format: Zone Name, CH1, CH2, ..., CH80
		/// </summary>
		public static bool ImportZonesFromCsv(string filePath, bool clearExisting, out int importedCount, out int skippedCount)
		{
			try
			{
				importedCount = 0;
				skippedCount = 0;
				
				if (!File.Exists(filePath))
				{
					System.Windows.Forms.MessageBox.Show(
						"File not found: " + filePath,
						"Zone Import Error",
						System.Windows.Forms.MessageBoxButtons.OK,
						System.Windows.Forms.MessageBoxIcon.Error);
					return false;
				}

				// Build channel lookup maps
				Dictionary<string, int> channelNameMap = BuildChannelNameMap();
				Dictionary<string, int> channelCompoundKeyMap = BuildChannelCompoundKeyMap();

				// Clear existing zones if requested
				if (clearExisting)
				{
					ClearAllZones();
				}

				using (CsvFileReader reader = new CsvFileReader(filePath, Encoding.UTF8))
				{
					CsvRow row = new CsvRow();
					
					// Skip header row
					if (!reader.ReadRow(row))
					{
						System.Windows.Forms.MessageBox.Show(
							"Empty CSV file",
							"Zone Import Error",
							System.Windows.Forms.MessageBoxButtons.OK,
							System.Windows.Forms.MessageBoxIcon.Error);
						return false;
					}

					// Read each zone

					while (reader.ReadRow(row))
					{
						if (row.Count < 1)
						{
							continue; // Empty row
						}

						string zoneName = (row[0] != null) ? row[0].Trim() : null;
						if (string.IsNullOrEmpty(zoneName))
						{
							continue; // No zone name
						}

						// Find or create zone
						int zoneIndex = FindOrCreateZone(zoneName);
						if (zoneIndex == -1)
						{
							skippedCount++;
							continue; // No free zone slots
						}

						// Clear existing channels in zone
						ZoneForm.data.ZoneList[zoneIndex].ChList = new ushort[MAX_CHANNELS_PER_ZONE];

						// Read channel references
						int channelSlot = 0;
						for (int col = 1; col < row.Count && channelSlot < MAX_CHANNELS_PER_ZONE; col++)
						{
							string channelRef = (row[col] != null) ? row[col].Trim() : null;
							if (string.IsNullOrEmpty(channelRef))
							{
								continue; // Empty channel slot
							}

							// Resolve channel reference to channel ID
							int channelId = ResolveChannelReference(channelRef, channelCompoundKeyMap, channelNameMap);
							if (channelId != -1)
							{
								// Add to zone (convert to 1-indexed)
								ZoneForm.data.ZoneList[zoneIndex].ChList[channelSlot] = (ushort)(channelId + 1);
								channelSlot++;
							}
						}

						importedCount++;
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				importedCount = 0;
				skippedCount = 0;
				System.Windows.Forms.MessageBox.Show(
					"Error importing zones: " + ex.Message,
					"Zone Import Error",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Error);
				return false;
			}
		}

		/// <summary>
		/// Build map of channel names to channel IDs (0-indexed)
		/// </summary>
		private static Dictionary<string, int> BuildChannelNameMap()
		{
			Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			
			for (int i = 0; i < ChannelForm.data.Count; i++)
			{
				if (ChannelForm.data.DataIsValid(i))
				{
					ChannelForm.ChannelOne channel = ChannelForm.data[i];
					if (!string.IsNullOrEmpty(channel.Name))
					{
						// Use first occurrence if duplicates exist
						if (!map.ContainsKey(channel.Name))
						{
							map[channel.Name] = i;
						}
					}
				}
			}

			return map;
		}

		/// <summary>
		/// Build map of compound keys to channel IDs (0-indexed)
		/// Compound key format: "channelNum|frequency|name"
		/// </summary>
		private static Dictionary<string, int> BuildChannelCompoundKeyMap()
		{
			Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			
			for (int i = 0; i < ChannelForm.data.Count; i++)
			{
				if (ChannelForm.data.DataIsValid(i))
				{
					ChannelForm.ChannelOne channel = ChannelForm.data[i];
					
					int channelNum = i + 1; // 1-indexed
					double freqMHz = channel.RxFreqDec / 100000.0;
					string name = channel.Name ?? "";
					
					// Format: "1|446.00625|N1CJ: CARLA"
					string compoundKey = string.Format("{0}|{1:0.00000}|{2}", channelNum, freqMHz, name);
					map[compoundKey] = i;
					
					// Also try alternate frequency formats for robustness
					string compoundKey2 = string.Format("{0}|{1:0.000000}|{2}", channelNum, freqMHz, name);
					if (!map.ContainsKey(compoundKey2))
					{
						map[compoundKey2] = i;
					}
				}
			}

			return map;
		}

		/// <summary>
		/// Resolve channel reference (compound key or name) to channel ID
		/// Returns -1 if not found
		/// </summary>
		private static int ResolveChannelReference(string channelRef, 
			Dictionary<string, int> compoundKeyMap, Dictionary<string, int> nameMap)
		{
			if (string.IsNullOrEmpty(channelRef))
			{
				return -1;
			}

			// Auto-detect format: if contains "|" it's a compound key
			if (channelRef.Contains("|"))
			{
				// Try compound key lookup
				int channelId;
				if (compoundKeyMap.TryGetValue(channelRef, out channelId))
				{
					return channelId;
				}

				// Try parsing compound key components (channelNum|freq|name)
				string[] parts = channelRef.Split('|');
				if (parts.Length >= 3)
				{
					// Try matching by frequency + name (more reliable than channel number)
					double freq;
					if (double.TryParse(parts[1], out freq))
					{
						string name = parts[2];
						return FindChannelByFreqAndName(freq, name);
					}
				}

				// Compound key not found
				return -1;
			}
			else
			{
				// Name-only format (legacy)
				int channelId;
				if (nameMap.TryGetValue(channelRef, out channelId))
				{
					return channelId;
				}

				return -1;
			}
		}

		/// <summary>
		/// Find channel by frequency and name (fallback method)
		/// </summary>
		private static int FindChannelByFreqAndName(double freqMHz, string name)
		{
			double toleranceMHz = 0.0001; // 100 Hz tolerance (0.0001 MHz)

			for (int i = 0; i < ChannelForm.data.Count; i++)
			{
				if (ChannelForm.data.DataIsValid(i))
				{
					ChannelForm.ChannelOne channel = ChannelForm.data[i];
					
					double channelFreqMHz = channel.RxFreqDec / 100000.0;
					
					// Check frequency match (within tolerance)
					if (Math.Abs(channelFreqMHz - freqMHz) < toleranceMHz)
					{
						// Check name match (case-insensitive)
						if (string.Equals(channel.Name, name, StringComparison.OrdinalIgnoreCase))
						{
							return i;
						}
					}
				}
			}

			return -1;
		}

		/// <summary>
		/// Find existing zone by name or create new one
		/// Returns zone index, or -1 if no free slots
		/// </summary>
		private static int FindOrCreateZone(string zoneName)
		{
			// Try to find existing zone
			for (int i = 0; i < ZoneForm.data.Count; i++)
			{
				if (!string.IsNullOrEmpty(ZoneForm.data.ZoneList[i].Name) &&
					ZoneForm.data.ZoneList[i].Name.Equals(zoneName, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}

			// Create new zone in first free slot
			for (int i = 0; i < ZoneForm.data.Count; i++)
			{
				if (string.IsNullOrEmpty(ZoneForm.data.ZoneList[i].Name))
				{
					ZoneForm.data.SetIndex(i, 1); // Mark zone as valid
					ZoneForm.data.ZoneList[i].Name = zoneName;
					ZoneForm.data.ZoneList[i].ChList = new ushort[MAX_CHANNELS_PER_ZONE];
					return i;
				}
			}

			return -1; // No free zone slots
		}

		/// <summary>
		/// Clear all zones (used when importing with "clear existing" option)
		/// </summary>
		private static void ClearAllZones()
		{
			for (int i = 0; i < ZoneForm.data.Count; i++)
			{
				ZoneForm.data.SetIndex(i, 0); // Mark zone as invalid
				ZoneForm.data.ZoneList[i] = new ZoneForm.ZoneOne(i);
			}
		}
	}
}
