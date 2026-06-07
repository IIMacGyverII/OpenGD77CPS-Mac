using System;
using System.Collections.Generic;
using System.Linq;

namespace DMR
{
	public sealed class CodeplugHealthZoneRow
	{
		public int ZoneIndex;
		public string Name;
		public int ChannelCount;
	}

	public sealed class CodeplugHealthScanRow
	{
		public int ScanIndex;
		public string Name;
		public int ChannelCount;
		public int InvalidRefCount;
	}

	public sealed class CodeplugHealthTgRow
	{
		public int TgIndex;
		public string Name;
		public int ContactCount;
		public int InvalidRefCount;
	}

	public sealed class CodeplugHealthDrillItem
	{
		public int Index;
		public string Name;
	}

	public sealed class CodeplugHealthDuplicateGroup
	{
		public string Name;
		public List<int> Indices = new List<int>();
	}

	public sealed class CodeplugHealthSnapshot
	{
		public int Channels;
		public int Digital;
		public int Analog;
		public int Contacts;
		public int Zones;
		public int TgLists;
		public int ScanLists;
		public int EmptyScanLists;
		public List<CodeplugHealthDrillItem> EmptyScanDrill = new List<CodeplugHealthDrillItem>();
		public int InvalidScanRefs;
		public List<CodeplugHealthDrillItem> InvalidScanRefDrill = new List<CodeplugHealthDrillItem>();
		public List<CodeplugHealthScanRow> ScanRows = new List<CodeplugHealthScanRow>();
		public int EmptyTgLists;
		public List<CodeplugHealthDrillItem> EmptyTgDrill = new List<CodeplugHealthDrillItem>();
		public int InvalidTgRefs;
		public List<CodeplugHealthDrillItem> InvalidTgRefDrill = new List<CodeplugHealthDrillItem>();
		public List<CodeplugHealthTgRow> TgRows = new List<CodeplugHealthTgRow>();
		public int RelayZero;
		public List<string> RelayZeroNames = new List<string>();
		public List<CodeplugHealthDrillItem> RelayZeroDrill = new List<CodeplugHealthDrillItem>();
		public int OrphanCount;
		public List<string> OrphanNames = new List<string>();
		public List<CodeplugHealthDrillItem> OrphanDrill = new List<CodeplugHealthDrillItem>();
		public int DuplicateNameGroups;
		public List<string> DuplicateNameLines = new List<string>();
		public List<CodeplugHealthDuplicateGroup> DuplicateChannelGroups = new List<CodeplugHealthDuplicateGroup>();
		public int EmptyZones;
		public List<CodeplugHealthZoneRow> ZoneRows = new List<CodeplugHealthZoneRow>();
		public int ChannelsNotInZone;
		public List<string> ChannelsNotInZoneNames = new List<string>();
		public List<CodeplugHealthDrillItem> ChannelsNotInZoneDrill = new List<CodeplugHealthDrillItem>();
		public int DuplicateDmrIdGroups;
		public List<string> DuplicateDmrIdLines = new List<string>();
		public List<CodeplugHealthDrillItem> DuplicateDmrIdDrill = new List<CodeplugHealthDrillItem>();
		public int DuplicateContactNameGroups;
		public List<string> DuplicateContactNameLines = new List<string>();
		public List<CodeplugHealthDuplicateGroup> DuplicateContactGroups = new List<CodeplugHealthDuplicateGroup>();
		public int DigitalNoContact;
		public List<string> DigitalNoContactNames = new List<string>();
		public List<CodeplugHealthDrillItem> DigitalNoContactDrill = new List<CodeplugHealthDrillItem>();

		public bool HasWarning
		{
			get
			{
				return this.RelayZero > 0 || this.OrphanCount > 0 || this.DuplicateNameGroups > 0
					|| this.EmptyZones > 0 || this.ChannelsNotInZone > 0 || this.DuplicateDmrIdGroups > 0
					|| this.DuplicateContactNameGroups > 0 || this.DigitalNoContact > 0
					|| this.EmptyScanLists > 0 || this.InvalidScanRefs > 0
					|| this.EmptyTgLists > 0 || this.InvalidTgRefs > 0;
			}
		}

#if OpenGD77
		public static CodeplugHealthSnapshot Collect(int nameListLimit = 12)
		{
			CodeplugHealthSnapshot snap = new CodeplugHealthSnapshot();
			Dictionary<string, List<int>> nameToChannels = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
			HashSet<int> channelsInZones = new HashSet<int>();

			for (int i = 0; i < ChannelForm.data.Count; i++)
			{
				if (!ChannelForm.data.DataIsValid(i))
				{
					continue;
				}
				snap.Channels++;
				switch (ChannelForm.data.GetChMode(i))
				{
				case 1:
					snap.Digital++;
					break;
				case 0:
					snap.Analog++;
					break;
				}
				ChannelForm.ChannelOne ch = ChannelForm.data[i];
				string channelName = ChannelForm.data.GetName(i);
				if (!nameToChannels.ContainsKey(channelName))
				{
					nameToChannels[channelName] = new List<int>();
				}
				nameToChannels[channelName].Add(i + 1);
				if (ch.Relay == 0)
				{
					snap.RelayZero++;
					if (snap.RelayZeroNames.Count < nameListLimit)
					{
						snap.RelayZeroNames.Add(channelName);
						snap.RelayZeroDrill.Add(new CodeplugHealthDrillItem { Index = i, Name = channelName });
					}
				}
				if (ChannelForm.data.GetChMode(i) == 1 && ch.Contact == 0)
				{
					snap.DigitalNoContact++;
					if (snap.DigitalNoContactNames.Count < nameListLimit)
					{
						snap.DigitalNoContactNames.Add(channelName);
						snap.DigitalNoContactDrill.Add(new CodeplugHealthDrillItem { Index = i, Name = channelName });
					}
				}
				if (ch.Contact > 0 && (ch.Contact > ContactForm.data.Count || !ContactForm.data.DataIsValid(ch.Contact - 1)))
				{
					snap.OrphanCount++;
					if (snap.OrphanNames.Count < nameListLimit)
					{
						snap.OrphanNames.Add(channelName);
						snap.OrphanDrill.Add(new CodeplugHealthDrillItem { Index = i, Name = channelName });
					}
				}
			}

			foreach (KeyValuePair<string, List<int>> entry in nameToChannels)
			{
				if (entry.Value.Count <= 1)
				{
					continue;
				}
				snap.DuplicateNameGroups++;
				if (snap.DuplicateNameLines.Count < nameListLimit)
				{
					string chNums = string.Join(", ", entry.Value.ConvertAll(n => "#" + n));
					snap.DuplicateNameLines.Add(entry.Key + " (×" + entry.Value.Count + "): " + chNums);
					CodeplugHealthDuplicateGroup group = new CodeplugHealthDuplicateGroup { Name = entry.Key };
					foreach (int oneBased in entry.Value)
					{
						group.Indices.Add(oneBased - 1);
					}
					snap.DuplicateChannelGroups.Add(group);
				}
			}

			snap.Contacts = CountValidContacts();
			snap.Zones = ZoneForm.data.ValidCount;
			snap.TgLists = CountValidRxGroupLists();
			CollectTgListHealth(snap, nameListLimit);
			CollectScanHealth(snap, nameListLimit);
			CollectDuplicateDmrIds(snap, nameListLimit);
			CollectDuplicateContactNames(snap, nameListLimit);

			for (int z = 0; z < ZoneForm.NUM_ZONES; z++)
			{
				if (!ZoneForm.data.ZoneChIsValid(z))
				{
					continue;
				}
				int chCount = 0;
				ushort[] chList = ZoneForm.data[z].ChList;
				for (int c = 0; c < chList.Length; c++)
				{
					int chIndex = chList[c];
					if (chIndex == 0 || chIndex == 65535)
					{
						break;
					}
					if (ChannelForm.data.DataIsValid(chIndex - 1))
					{
						chCount++;
						channelsInZones.Add(chIndex - 1);
					}
				}
				if (chCount == 0)
				{
					snap.EmptyZones++;
				}
				snap.ZoneRows.Add(new CodeplugHealthZoneRow
				{
					ZoneIndex = z,
					Name = ZoneForm.data.GetName(z),
					ChannelCount = chCount
				});
			}

			for (int i = 0; i < ChannelForm.data.Count; i++)
			{
				if (!ChannelForm.data.DataIsValid(i))
				{
					continue;
				}
				if (!channelsInZones.Contains(i))
				{
					snap.ChannelsNotInZone++;
					if (snap.ChannelsNotInZoneNames.Count < nameListLimit)
					{
						string chName = ChannelForm.data.GetName(i);
						snap.ChannelsNotInZoneNames.Add(chName);
						snap.ChannelsNotInZoneDrill.Add(new CodeplugHealthDrillItem { Index = i, Name = chName });
					}
				}
			}

			return snap;
		}

		private static void CollectScanHealth(CodeplugHealthSnapshot snap, int nameListLimit)
		{
			for (int s = 0; s < NormalScanForm.data.Count; s++)
			{
				if (!NormalScanForm.data.DataIsValid(s))
				{
					continue;
				}
				snap.ScanLists++;
				int chCount = 0;
				int invalidRefs = 0;
				ushort[] chList = NormalScanForm.data[s].ChList;
				for (int i = 0; i < chList.Length; i++)
				{
					ushort chRef = chList[i];
					if (chRef <= 1 || chRef > 1025)
					{
						continue;
					}
					chCount++;
					if (!ChannelForm.data.DataIsValid(chRef - 2))
					{
						invalidRefs++;
					}
				}
				if (chCount == 0)
				{
					snap.EmptyScanLists++;
					if (snap.EmptyScanDrill.Count < nameListLimit)
					{
						snap.EmptyScanDrill.Add(new CodeplugHealthDrillItem
						{
							Index = s,
							Name = NormalScanForm.data[s].Name
						});
					}
				}
				if (invalidRefs > 0)
				{
					snap.InvalidScanRefs += invalidRefs;
					if (snap.InvalidScanRefDrill.Count < nameListLimit)
					{
						string label = NormalScanForm.data[s].Name + " (" + invalidRefs + " bad ref"
							+ (invalidRefs == 1 ? "" : "s") + ")";
						snap.InvalidScanRefDrill.Add(new CodeplugHealthDrillItem
						{
							Index = s,
							Name = label
						});
					}
				}
				snap.ScanRows.Add(new CodeplugHealthScanRow
				{
					ScanIndex = s,
					Name = NormalScanForm.data[s].Name,
					ChannelCount = chCount,
					InvalidRefCount = invalidRefs
				});
			}
		}

		private static void CollectTgListHealth(CodeplugHealthSnapshot snap, int nameListLimit)
		{
			for (int t = 0; t < RxGroupListForm.data.Count; t++)
			{
				if (!RxGroupListForm.data.DataIsValid(t))
				{
					continue;
				}
				int contactCount = RxGroupListForm.data.GetContactCntByIndex(t);
				int invalidRefs = 0;
				for (int i = 0; i < contactCount; i++)
				{
					ushort contactNum = RxGroupListForm.data[t].ContactList[i];
					if (contactNum == 0)
					{
						continue;
					}
					int idx = contactNum - 1;
					if (!ContactForm.data.DataIsValid(idx) || !ContactForm.data.IsGroupCall(idx))
					{
						invalidRefs++;
					}
				}
				if (contactCount == 0)
				{
					snap.EmptyTgLists++;
					if (snap.EmptyTgDrill.Count < nameListLimit)
					{
						snap.EmptyTgDrill.Add(new CodeplugHealthDrillItem
						{
							Index = t,
							Name = RxGroupListForm.data[t].Name
						});
					}
				}
				if (invalidRefs > 0)
				{
					snap.InvalidTgRefs += invalidRefs;
					if (snap.InvalidTgRefDrill.Count < nameListLimit)
					{
						string label = RxGroupListForm.data[t].Name + " (" + invalidRefs + " bad ref"
							+ (invalidRefs == 1 ? "" : "s") + ")";
						snap.InvalidTgRefDrill.Add(new CodeplugHealthDrillItem
						{
							Index = t,
							Name = label
						});
					}
				}
				snap.TgRows.Add(new CodeplugHealthTgRow
				{
					TgIndex = t,
					Name = RxGroupListForm.data[t].Name,
					ContactCount = contactCount,
					InvalidRefCount = invalidRefs
				});
			}
		}

		private static void CollectDuplicateContactNames(CodeplugHealthSnapshot snap, int nameListLimit)
		{
			Dictionary<string, List<int>> nameToIndices = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < ContactForm.data.Count; i++)
			{
				if (!ContactForm.data.DataIsValid(i))
				{
					continue;
				}
				string name = (ContactForm.data.GetName(i) ?? "").Trim();
				if (string.IsNullOrEmpty(name))
				{
					continue;
				}
				if (!nameToIndices.ContainsKey(name))
				{
					nameToIndices[name] = new List<int>();
				}
				nameToIndices[name].Add(i + 1);
			}

			foreach (KeyValuePair<string, List<int>> entry in nameToIndices)
			{
				if (entry.Value.Count <= 1)
				{
					continue;
				}
				snap.DuplicateContactNameGroups++;
				if (snap.DuplicateContactNameLines.Count < nameListLimit)
				{
					string nums = string.Join(", ", entry.Value.ConvertAll(n => "#" + n));
					snap.DuplicateContactNameLines.Add(entry.Key + " (×" + entry.Value.Count + "): " + nums);
					CodeplugHealthDuplicateGroup group = new CodeplugHealthDuplicateGroup { Name = entry.Key };
					foreach (int oneBased in entry.Value)
					{
						group.Indices.Add(oneBased - 1);
					}
					snap.DuplicateContactGroups.Add(group);
				}
			}
		}

		private static void CollectDuplicateDmrIds(CodeplugHealthSnapshot snap, int nameListLimit)
		{
			Dictionary<string, List<string>> idToNames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < ContactForm.data.Count; i++)
			{
				if (!ContactForm.data.DataIsValid(i))
				{
					continue;
				}
				string callId = (ContactForm.data.GetCallID(i) ?? "").Trim();
				if (string.IsNullOrEmpty(callId) || callId == "16777215")
				{
					continue;
				}
				if (!idToNames.ContainsKey(callId))
				{
					idToNames[callId] = new List<string>();
				}
				string contactName = ContactForm.data.GetName(i);
				if (!idToNames[callId].Contains(contactName))
				{
					idToNames[callId].Add(contactName);
				}
			}

			foreach (KeyValuePair<string, List<string>> entry in idToNames)
			{
				if (entry.Value.Count <= 1)
				{
					continue;
				}
				snap.DuplicateDmrIdGroups++;
				if (snap.DuplicateDmrIdLines.Count < nameListLimit)
				{
					snap.DuplicateDmrIdLines.Add("ID " + entry.Key + ": " + string.Join(", ", entry.Value.ToArray()));
					int contactIndex = FindContactIndexByName(entry.Value[0]);
					if (contactIndex >= 0)
					{
						snap.DuplicateDmrIdDrill.Add(new CodeplugHealthDrillItem
						{
							Index = contactIndex,
							Name = entry.Value[0]
						});
					}
				}
			}
		}

		private static int FindContactIndexByName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return -1;
			}
			for (int i = 0; i < ContactForm.data.Count; i++)
			{
				if (ContactForm.data.DataIsValid(i) && string.Equals(ContactForm.data.GetName(i), name, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		private static int CountValidContacts()
		{
			int count = 0;
			for (int i = 0; i < ContactForm.data.Count; i++)
			{
				if (ContactForm.data.DataIsValid(i))
				{
					count++;
				}
			}
			return count;
		}

		private static int CountValidRxGroupLists()
		{
			int count = 0;
			for (int i = 0; i < RxGroupListForm.data.Count; i++)
			{
				if (RxGroupListForm.data.DataIsValid(i))
				{
					count++;
				}
			}
			return count;
		}
#endif
	}
}