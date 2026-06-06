using System;
using System.Collections.Generic;
using System.Linq;

namespace DMR
{
	public sealed class CodeplugHealthZoneRow
	{
		public string Name;
		public int ChannelCount;
	}

	public sealed class CodeplugHealthSnapshot
	{
		public int Channels;
		public int Digital;
		public int Analog;
		public int Contacts;
		public int Zones;
		public int TgLists;
		public int RelayZero;
		public List<string> RelayZeroNames = new List<string>();
		public int OrphanCount;
		public List<string> OrphanNames = new List<string>();
		public int DuplicateNameGroups;
		public List<string> DuplicateNameLines = new List<string>();
		public int EmptyZones;
		public List<CodeplugHealthZoneRow> ZoneRows = new List<CodeplugHealthZoneRow>();
		public int ChannelsNotInZone;
		public List<string> ChannelsNotInZoneNames = new List<string>();

		public bool HasWarning
		{
			get
			{
				return this.RelayZero > 0 || this.OrphanCount > 0 || this.DuplicateNameGroups > 0
					|| this.EmptyZones > 0 || this.ChannelsNotInZone > 0;
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
					}
				}
				if (ch.Contact > 0 && (ch.Contact > ContactForm.data.Count || !ContactForm.data.DataIsValid(ch.Contact - 1)))
				{
					snap.OrphanCount++;
					if (snap.OrphanNames.Count < nameListLimit)
					{
						snap.OrphanNames.Add(channelName);
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
				}
			}

			snap.Contacts = CountValidContacts();
			snap.Zones = ZoneForm.data.ValidCount;
			snap.TgLists = CountValidRxGroupLists();

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
						snap.ChannelsNotInZoneNames.Add(ChannelForm.data.GetName(i));
					}
				}
			}

			return snap;
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