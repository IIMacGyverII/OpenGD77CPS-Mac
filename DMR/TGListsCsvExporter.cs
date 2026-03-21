using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReadWriteCsv;

namespace DMR
{
	/// <summary>
	/// Exports TG List (RxGroupList) data to CSV format compatible with Android PhoneDMRApp
	/// Format: TG List Name,Contact1,Contact2,...Contact32
	/// Where contacts are DMR TG IDs (integers like 11904, 47), not contact names
	/// </summary>
	public class TGListsCsvExporter
	{
		private const int MAX_CONTACTS_PER_LIST = 32;

		/// <summary>
		/// Export all TG Lists to a CSV file
		/// Format: TG List Name, Contact1, Contact2, ..., Contact32
		/// </summary>
		public static bool ExportTGListsToCsv(string filePath)
		{
			try
			{
				using (CsvFileWriter writer = new CsvFileWriter(filePath, false, Encoding.UTF8))
				{
					// Write header row
					CsvRow headerRow = new CsvRow();
					headerRow.Add("TG List Name");
					for (int i = 1; i <= MAX_CONTACTS_PER_LIST; i++)
					{
						headerRow.Add("Contact" + i);
					}
					writer.WriteRow(headerRow);

					// Write each TG List
					for (int listIndex = 0; listIndex < RxGroupListForm.data.Count; listIndex++)
					{
						if (!RxGroupListForm.data.DataIsValid(listIndex))
						{
							continue; // Skip empty TG Lists
						}

						string listName = RxGroupListForm.data.GetName(listIndex);
						if (string.IsNullOrEmpty(listName))
						{
							continue; // Skip lists with no name
						}

						CsvRow listRow = new CsvRow();
						listRow.Add(listName);

						// Get contact list for this TG List
						RxListOneData listData = RxGroupListForm.data[listIndex];
						ushort[] contactIndexes = listData.ContactList;

						// Add up to 32 contacts
						int contactCount = 0;
						if (contactIndexes != null)
						{
							for (int i = 0; i < contactIndexes.Length && contactCount < MAX_CONTACTS_PER_LIST; i++)
							{
								ushort contactIndex1Based = contactIndexes[i];
								
								// Skip empty slots (0 or 65535)
								if (contactIndex1Based == 0 || contactIndex1Based == 65535)
								{
									continue;
								}

								// Convert to 0-based index
								int contactId = contactIndex1Based - 1;

								// Validate contact exists
								if (contactId >= 0 && contactId < ContactForm.data.Count && ContactForm.data.DataIsValid(contactId))
								{
									// Get DMR TG ID (CallId property - this is the numeric TG ID like "11904")
									string callId = ContactForm.data.GetCallID(contactId);
									if (!string.IsNullOrEmpty(callId))
									{
										listRow.Add(callId);
										contactCount++;
									}
								}
							}
						}

						// Fill remaining columns with empty strings
						while (listRow.Count < (MAX_CONTACTS_PER_LIST + 1))
						{
							listRow.Add("");
						}

						writer.WriteRow(listRow);
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(
					"Error exporting TG Lists: " + ex.Message,
					"TG List Export Error",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Error);
				return false;
			}
		}
	}
}
