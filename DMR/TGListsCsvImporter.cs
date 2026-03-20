using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ReadWriteCsv;

namespace DMR
{
	/// <summary>
	/// Imports TG List (RxGroupList) data from CSV format compatible with Android PhoneDMRApp
	/// Format: TG List Name,Contact1,Contact2,...Contact32
	/// Where contacts are DMR TG IDs (integers), not contact names
	/// </summary>
	public class TGListsCsvImporter
	{
		/// <summary>
		/// Import TG Lists from a CSV file
		/// </summary>
		/// <param name="filePath">Path to TG_Lists.csv file</param>
		/// <param name="clearFirst">If true, clear existing TG Lists before import</param>
		/// <param name="mainForm">Reference to main form for UI updates</param>
		/// <param name="importedCount">Number of TG Lists successfully imported</param>
		/// <returns>True if import succeeded, false otherwise</returns>
		public static bool ImportTGListsFromCsv(string filePath, bool clearFirst, Form mainForm, out int importedCount)
		{
			importedCount = 0;
			
			try
			{
				if (!File.Exists(filePath))
				{
					// File doesn't exist - this is not an error, just skip silently
					return true;
				}

				// Clear existing TG Lists if requested
				if (clearFirst)
				{
					ClearAllTGLists();
				}

				// Build contact lookup map: DMR TG ID -> contact index
				Dictionary<int, int> contactIdMap = BuildContactIdMap();

				using (CsvFileReader reader = new CsvFileReader(filePath, Encoding.UTF8))
				{
					CsvRow row = new CsvRow();
					
					// Skip header row
					if (!reader.ReadRow(row))
					{
						// Empty file - not an error
						return true;
					}

					// Read each TG List
					while (reader.ReadRow(row))
					{
						try
						{
							if (row.Count < 1)
							{
								continue; // Empty row
							}

							string listName = (row[0] != null) ? row[0].Trim() : null;
							if (string.IsNullOrEmpty(listName))
							{
								continue; // No list name
							}

							// Find or create TG List
							int listIndex = FindOrCreateTGList(listName);
							if (listIndex == -1)
							{
								// No free slots - show warning and stop
								MessageBox.Show(
									"Maximum TG List capacity reached (" + RxListData.CNT_RX_LIST + " lists). " +
									"Imported " + importedCount + " lists before capacity limit.",
									"TG List Import Warning",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning);
								break;
							}

							// Clear existing contacts in this list
							RxGroupListForm.data.Default(listIndex);

							// Read contact IDs (up to 32)
							List<ushort> contactIndexes = new List<ushort>();
							int contactCount = 0;
							
							for (int col = 1; col < row.Count && contactCount < RxListOneData.CNT_CONTACT_PER_RX_LIST; col++)
							{
								string contactIdStr = (row[col] != null) ? row[col].Trim() : null;
								if (string.IsNullOrEmpty(contactIdStr))
								{
									continue; // Empty contact slot
								}

								// Parse DMR TG ID
								int dmrId;
								if (!int.TryParse(contactIdStr, out dmrId))
								{
									continue; // Invalid ID, skip
								}

								// Look up contact index by DMR ID
								int contactIndex;
								if (contactIdMap.TryGetValue(dmrId, out contactIndex))
								{
									// Add contact (convert to 1-based index)
									contactIndexes.Add((ushort)(contactIndex + 1));
									contactCount++;
								}
								// If contact doesn't exist, silently skip it
							}

							// Assign contacts to the list
							if (contactIndexes.Count > 0)
							{
								RxListOneData listData = RxGroupListForm.data[listIndex];
								listData.ContactList = contactIndexes.ToArray();
								RxGroupListForm.data[listIndex] = listData;
								
								// Update the index to reflect number of contacts + 1
								RxGroupListForm.data.SetIndex(listIndex, contactIndexes.Count + 1);
							}
							else
							{
								// Empty list, still mark as valid
								RxGroupListForm.data.SetIndex(listIndex, 1);
							}

							importedCount++;
						}
						catch (Exception ex)
						{
							// Log error for this row but continue processing
							System.Diagnostics.Debug.WriteLine("Error importing TG List row: " + ex.Message);
							continue;
						}
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Error importing TG Lists: " + ex.Message,
					"TG List Import Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return false;
			}
		}

		/// <summary>
		/// Build a map of DMR TG IDs to contact indexes (0-based)
		/// </summary>
		private static Dictionary<int, int> BuildContactIdMap()
		{
			Dictionary<int, int> map = new Dictionary<int, int>();
			
			for (int i = 0; i < ContactForm.data.Count; i++)
			{
				if (ContactForm.data.DataIsValid(i))
				{
					string callIdStr = ContactForm.data.GetCallID(i);
					if (!string.IsNullOrEmpty(callIdStr))
					{
						int dmrId;
						if (int.TryParse(callIdStr, out dmrId))
						{
							// Use first occurrence if duplicates exist
							if (!map.ContainsKey(dmrId))
							{
								map[dmrId] = i;
							}
						}
					}
				}
			}
			
			return map;
		}

		/// <summary>
		/// Find existing TG List by name, or create a new one
		/// </summary>
		/// <param name="name">Name of the TG List</param>
		/// <returns>Index of the TG List (0-based), or -1 if no slots available</returns>
		private static int FindOrCreateTGList(string name)
		{
			// First, check if list with this name already exists
			for (int i = 0; i < RxGroupListForm.data.Count; i++)
			{
				if (RxGroupListForm.data.DataIsValid(i))
				{
					string existingName = RxGroupListForm.data.GetName(i);
					if (existingName != null && existingName.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						return i; // Found existing list
					}
				}
			}

			// List doesn't exist, create new one
			int newIndex = RxGroupListForm.data.GetMinIndex();
			if (newIndex == -1)
			{
				return -1; // No free slots
			}

			// Initialize the list
			RxGroupListForm.data.SetName(newIndex, name);
			RxGroupListForm.data.SetIndex(newIndex, 1); // Mark as valid (1 = 0 contacts + 1)
			
			return newIndex;
		}

		/// <summary>
		/// Clear all existing TG Lists
		/// </summary>
		private static void ClearAllTGLists()
		{
			for (int i = 0; i < RxGroupListForm.data.Count; i++)
			{
				if (RxGroupListForm.data.DataIsValid(i))
				{
					RxGroupListForm.data.ClearIndex(i);
				}
			}
		}
	}
}
