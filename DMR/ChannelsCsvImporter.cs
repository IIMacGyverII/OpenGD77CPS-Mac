using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ReadWriteCsv;

namespace DMR
{
	/// <summary>
	/// Imports Channel data from CSV format compatible with Android PhoneDMRApp
	/// Handles both standard OpenGD77 format (28 columns) and Android export format (29 columns with _id)
	/// </summary>
	public class ChannelsCsvImporter
	{
		/// <summary>
		/// Import Channels from a CSV file
		/// </summary>
		/// <param name="filePath">Path to Channels.csv file</param>
		/// <param name="clearFirst">If true, clear existing channels before import</param>
		/// <param name="mainForm">Reference to main form for UI updates</param>
		/// <param name="importedCount">Number of channels successfully imported</param>
		/// <returns>True if import succeeded, false otherwise</returns>
		public static bool ImportChannelsFromCsv(string filePath, bool clearFirst, Form mainForm, out int importedCount)
		{
			importedCount = 0;
			
			try
			{
				if (!File.Exists(filePath))
				{
					MessageBox.Show(
						"File not found: " + filePath,
						"Channel Import Error",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return false;
				}

				// Clear existing channels if requested
				if (clearFirst)
				{
					ClearAllChannels();
				}

				using (CsvFileReader reader = new CsvFileReader(filePath, Encoding.UTF8))
				{
					CsvRow headerRow = new CsvRow();
					
					// Read header row
					if (!reader.ReadRow(headerRow))
					{
						MessageBox.Show(
							"Empty CSV file",
							"Channel Import Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
						return false;
					}

					// Detect if CSV has _id column (Android export format)
					// Format with _id: "_id,Channel Number,Channel Name,..." (37 columns new, 29 legacy)
					// Format without _id: "Channel Number,Channel Name,..." (36 columns new, 28 legacy)
					bool hasIdColumn = false;
					bool hasNewFields = false; // Additional fields: Encrypt, Relay, Interrupt, etc.
					int fieldOffset = 0;
					
					if (headerRow.Count >= 29 && headerRow[0] != null && headerRow[0].Trim().Equals("_id", StringComparison.OrdinalIgnoreCase))
					{
						hasIdColumn = true;
						fieldOffset = 1; // Skip _id column when parsing
						hasNewFields = headerRow.Count >= 37;
						System.Diagnostics.Debug.WriteLine("Detected Android export format with _id column (" + headerRow.Count + " fields, newFields=" + hasNewFields + ")");
					}
					else if (headerRow.Count >= 28 && headerRow[0] != null && headerRow[0].Trim().Equals("Channel Number", StringComparison.OrdinalIgnoreCase))
					{
						hasIdColumn = false;
						fieldOffset = 0; // No offset needed
						hasNewFields = headerRow.Count >= 36;
						System.Diagnostics.Debug.WriteLine("Detected OpenGD77 format (" + headerRow.Count + " fields, newFields=" + hasNewFields + ")");
					}
					else
					{
						MessageBox.Show(
							"Unknown CSV format. Expected 28, 29, 36, or 37 columns.\nFirst column: " + (headerRow.Count > 0 ? headerRow[0] : "empty"),
							"Channel Import Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
						return false;
					}

					// Expected minimum field count
					int minFields;
					if (hasNewFields)
					{
						minFields = hasIdColumn ? 37 : 36;
					}
					else
					{
						minFields = hasIdColumn ? 29 : 28;
					}

					// Build lookup maps
					Dictionary<string, int> contactNameMap = BuildContactNameMap();

					// Read each channel
					int skippedCount = 0;
					CsvRow row = new CsvRow();
					
					while (reader.ReadRow(row))
					{
						try
						{
							if (row.Count < minFields)
							{
								skippedCount++;
								continue; // Invalid row
							}

							// Parse channel fields (with _id offset if present)
							// Note: We ignore the _id column for OpenGD77 CPS (it uses channel numbers, not _id)
							
							int channelNumber = ParseInt(GetField(row, 0, fieldOffset), 0);
							string channelName = GetField(row, 1, fieldOffset);
							string channelType = GetField(row, 2, fieldOffset);
							string rxFreq = GetField(row, 3, fieldOffset);
							string txFreq = GetField(row, 4, fieldOffset);
							string bandwidth = GetField(row, 5, fieldOffset);
							int colorCode = ParseInt(GetField(row, 6, fieldOffset), 1);
							int timeslot = ParseInt(GetField(row, 7, fieldOffset), 1);
							string contactName = GetField(row, 8, fieldOffset);
							string tgListName = GetField(row, 9, fieldOffset);
							string scanList = GetField(row, 10, fieldOffset);
							string admit = GetField(row, 11, fieldOffset);
							string inCall = GetField(row, 12, fieldOffset);
							string rxTone = GetField(row, 13, fieldOffset);
							string txTone = GetField(row, 14, fieldOffset);
							string squelch = GetField(row, 15, fieldOffset);
							string power = GetField(row, 16, fieldOffset);

							// Skip empty channels
							if (string.IsNullOrEmpty(channelName) || channelName.Equals("None", StringComparison.OrdinalIgnoreCase))
							{
								skippedCount++;
								continue;
							}

							// Find or create channel slot
							int channelIndex = channelNumber - 1; // Convert to 0-based
							if (channelIndex < 0 || channelIndex >= ChannelForm.CurCntCh)
							{
								// Channel number out of range
								skippedCount++;
								continue;
							}

							// Create/update channel
							ChannelForm.ChannelOne channel;
							if (ChannelForm.data.DataIsValid(channelIndex))
							{
								channel = ChannelForm.data[channelIndex];
							}
							else
							{
								ChannelForm.data.SetIndex(channelIndex, 0);
								ChannelForm.data.Default(channelIndex);
								channel = ChannelForm.data[channelIndex];
							}

							// Set basic fields
							channel.Name = channelName;
							channel.ChModeS = channelType; // "Digital" or "Analog"
							channel.RxFreq = rxFreq;
							channel.TxFreq = txFreq;
							channel.TxColor = colorCode;
							channel.RepeaterSlotS = timeslot.ToString();

							// Handle contact (lookup by name)
							if (!string.IsNullOrEmpty(contactName) && !contactName.Equals("None", StringComparison.OrdinalIgnoreCase))
							{
								int contactIndex;
								if (contactNameMap.TryGetValue(contactName, out contactIndex))
								{
									channel.Contact = contactIndex + 1; // Convert to 1-based
								}
								else
								{
									channel.Contact = 0; // Contact not found
								}
							}
							else
							{
								channel.Contact = 0;
							}

							// Handle TG List (RxGroupList)
							if (!string.IsNullOrEmpty(tgListName) && !tgListName.Equals("None", StringComparison.OrdinalIgnoreCase))
							{
								int tgListIndex = FindTGListByName(tgListName);
								if (tgListIndex != -1)
								{
									channel.RxGroupList = tgListIndex + 1; // Convert to 1-based
								}
								else
								{
									channel.RxGroupList = 0; // TG List not found
								}
							}
							else
							{
								channel.RxGroupList = 0;
							}

							// Handle other fields
							channel.ScanListString = scanList;
							channel.RxTone = rxTone;
							channel.TxTone = txTone;
							channel.PowerString = power;

							// NEW FIELDS (29-36): Parse Android-specific fields and store in reserve bytes
							if (hasNewFields && row.Count >= minFields)
							{
								// Fields 29-36 (with fieldOffset applied):
								// 29: Encrypt Switch, 30: Encrypt Key, 31: Relay, 32: Interrupt,
								// 33: Active, 34: Outbound Slot, 35: Channel Mode, 36: Contact Type
								
								string encryptSwitch = GetField(row, 29, fieldOffset);
								string encryptKey = GetField(row, 30, fieldOffset);  // Cannot store (string)
								string relay = GetField(row, 31, fieldOffset);
								string interrupt = GetField(row, 32, fieldOffset);
								string active = GetField(row, 33, fieldOffset);
								string outboundSlot = GetField(row, 34, fieldOffset);
								string channelMode = GetField(row, 35, fieldOffset);
								string contactTypeStr = GetField(row, 36, fieldOffset);
								
								// Store in reserve fields (now accessible via properties)
								int encryptSwitchVal;
								if (int.TryParse(encryptSwitch, out encryptSwitchVal))
									channel.EncryptSwitch = encryptSwitchVal;
								
								int relayVal;
								if (int.TryParse(relay, out relayVal))
									channel.Relay = relayVal;
								
								int interruptVal;
								if (int.TryParse(interrupt, out interruptVal))
									channel.Interrupt = interruptVal;
								
								int activeVal;
								if (int.TryParse(active, out activeVal))
									channel.Active = activeVal;
								
								int outboundSlotVal;
								if (int.TryParse(outboundSlot, out outboundSlotVal))
									channel.OutboundSlot = outboundSlotVal;
								
								int channelModeVal;
								if (int.TryParse(channelMode, out channelModeVal))
									channel.ChannelMode = channelModeVal;
								
								int contactTypeVal;
								if (int.TryParse(contactTypeStr, out contactTypeVal))
									channel.AndroidContactType = contactTypeVal;
								
								// Note: EncryptKey (string) cannot be stored in binary codeplug
								System.Diagnostics.Debug.WriteLine("CH" + channelNumber + " Android fields imported: " +
									"encrypt=" + encryptSwitch + ",relay=" + relay + ",interrupt=" + interrupt + 
									",active=" + active + ",slot=" + outboundSlot + ",mode=" + channelMode + 
									",contactType=" + contactTypeStr);
							}

							// Save channel back to data
							ChannelForm.data[channelIndex] = channel;
							ChannelForm.data.SetIndex(channelIndex, 0); // Mark as valid

							importedCount++;
						}
						catch (Exception ex)
						{
							// Log error for this row but continue processing
							System.Diagnostics.Debug.WriteLine("Error importing channel row: " + ex.Message);
							skippedCount++;
							continue;
						}
					}

					if (skippedCount > 0)
					{
						System.Diagnostics.Debug.WriteLine("Skipped " + skippedCount + " invalid channel rows");
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Error importing channels: " + ex.Message,
					"Channel Import Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return false;
			}
		}

		/// <summary>
		/// Get field value with offset (to handle _id column)
		/// </summary>
		private static string GetField(CsvRow row, int fieldIndex, int offset)
		{
			int actualIndex = fieldIndex + offset;
			if (actualIndex < row.Count && row[actualIndex] != null)
			{
				return row[actualIndex].Trim();
			}
			return "";
		}

		/// <summary>
		/// Parse integer with default value
		/// </summary>
		private static int ParseInt(string value, int defaultValue)
		{
			int result;
			if (int.TryParse(value, out result))
			{
				return result;
			}
			return defaultValue;
		}

		/// <summary>
		/// Build a map of contact names to contact indexes (0-based)
		/// </summary>
		private static Dictionary<string, int> BuildContactNameMap()
		{
			Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			
			for (int i = 0; i < ContactForm.data.Count; i++)
			{
				if (ContactForm.data.DataIsValid(i))
				{
					string name = ContactForm.data.GetName(i);
					if (!string.IsNullOrEmpty(name))
					{
						// Use first occurrence if duplicates exist
						if (!map.ContainsKey(name))
						{
							map[name] = i;
						}
					}
				}
			}
			
			return map;
		}

		/// <summary>
		/// Find TG List by name
		/// </summary>
		private static int FindTGListByName(string name)
		{
			for (int i = 0; i < RxGroupListForm.data.Count; i++)
			{
				if (RxGroupListForm.data.DataIsValid(i))
				{
					string existingName = RxGroupListForm.data.GetName(i);
					if (existingName != null && existingName.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						return i; // Found TG List
					}
				}
			}
			return -1; // Not found
		}

		/// <summary>
		/// Clear all existing channels
		/// </summary>
		private static void ClearAllChannels()
		{
			for (int i = 0; i < ChannelForm.data.Count; i++)
			{
				if (ChannelForm.data.DataIsValid(i))
				{
					ChannelForm.data.ClearIndex(i);
				}
			}
		}
	}
}
