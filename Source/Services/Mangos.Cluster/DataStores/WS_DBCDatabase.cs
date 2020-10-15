//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mangos.Common;
using Mangos.Common.DataStores;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Map;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Cluster.DataStores
{
	public class WS_DBCDatabase
	{
		private readonly string MapDBC = "dbc" + Path.DirectorySeparatorChar + "Map.dbc";
		public Dictionary<int, MapInfo> Maps = new Dictionary<int, MapInfo>();

		public void InitializeMaps()
		{
			try
			{
				var data = new BufferedDbc(MapDBC);
				for (int i = 0, loopTo = new BufferedDbc(MapDBC).Rows - 1; i <= loopTo; i++)
				{
					var m = new MapInfo()
					{
						ID = data.Read<int>(i, 0),
						Type = (MapTypes)data.Read<int>(i, 2),
						Name = data.Read<string>(i, 4),
						ParentMap = data.Read<int>(i, 3),
						ResetTime = data.Read<int>(i, 38),
					};
					Maps.Add(m.ID, m);
				}

				ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Maps Initialized.", new BufferedDbc(MapDBC).Rows - 1);
				new BufferedDbc(MapDBC).Dispose();
			}
			catch (DirectoryNotFoundException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("DBC File : Maps.dbc missing.");
				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}

		public class MapInfo
		{
			public int ID;
			public MapTypes Type = MapTypes.MAP_COMMON;
			public string Name = "";
			public int ParentMap = -1;
			public int ResetTime = 0;

			public bool IsDungeon
			{
				get
				{
					return Type == MapTypes.MAP_INSTANCE || Type == MapTypes.MAP_RAID;
				}
			}

			public bool IsRaid
			{
				get
				{
					return Type == MapTypes.MAP_RAID;
				}
			}

			public bool IsBattleGround
			{
				get
				{
					return Type == MapTypes.MAP_BATTLEGROUND;
				}
			}

			public bool HasResetTime
			{
				get
				{
					return ResetTime != 0;
				}
			}
		}

		private readonly string WorldSafeLocsDBC = "dbc" + Path.DirectorySeparatorChar + "WorldSafeLocs.dbc";
		public Dictionary<int, TWorldSafeLoc> WorldSafeLocs = new Dictionary<int, TWorldSafeLoc>();

		public void InitializeWorldSafeLocs()
		{
			try
			{
				var data = new BufferedDbc(WorldSafeLocsDBC);
				for (int i = 0, loopTo = new BufferedDbc(WorldSafeLocsDBC).Rows - 1; i <= loopTo; i++)
				{
					var WorldSafeLoc = new TWorldSafeLoc()
					{
						ID = data.Read<int>(i, 0),
						map = data.Read<uint>(i, 1),
						x = data.Read<float>(i, 2),
						y = data.Read<float>(i, 3),
						z = data.Read<float>(i, 4),
					};
					WorldSafeLocs.Add(WorldSafeLoc.ID, WorldSafeLoc);
				}

				ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} WorldSafeLocs Initialized.", new BufferedDbc(WorldSafeLocsDBC).Rows - 1);
				new BufferedDbc(WorldSafeLocsDBC).Dispose();
			}
			catch (DirectoryNotFoundException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("DBC File : WorldSafeLocs.dbc missing.");
				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}

		public class TWorldSafeLoc
		{
			public int ID;
			public uint map;
			public float x;
			public float y;
			public float z;
		}

		public Dictionary<byte, TBattleground> Battlegrounds = new Dictionary<byte, TBattleground>();

		public void InitializeBattlegrounds()
		{
			byte Entry;
			var MySQLQuery = new DataTable();
			ClusterServiceLocator._WorldCluster.GetWorldDatabase().Query(string.Format("SELECT * FROM battleground_template"), ref MySQLQuery);
			foreach (DataRow row in MySQLQuery.Rows)
			{
				Entry = row.As<byte>("id");
				Battlegrounds.Add(Entry, new TBattleground());

				// TODO: the MAPId needs to be located from somewhere other than the template file
				// BUG: THIS IS AN UGLY HACK UNTIL THE ABOVE IS FIXED
				// Battlegrounds(Entry).Map = row.Item("Map")
				Battlegrounds[Entry].MinPlayersPerTeam = row.As<byte>("MinPlayersPerTeam");
				Battlegrounds[Entry].MaxPlayersPerTeam = row.As<byte>("MaxPlayersPerTeam");
				Battlegrounds[Entry].MinLevel = row.As<byte>("MinLvl");
				Battlegrounds[Entry].MaxLevel = row.As<byte>("MaxLvl");
				Battlegrounds[Entry].AllianceStartLoc = row.As<int>("AllianceStartLoc");
				Battlegrounds[Entry].AllianceStartO = row.As<float>("AllianceStartO");
				Battlegrounds[Entry].HordeStartLoc = row.As<int>("HordeStartLoc");
				Battlegrounds[Entry].HordeStartO = row.As<float>("HordeStartO");
			}

			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "World: {0} Battlegrounds Initialized.", MySQLQuery.Rows.Count);
		}

		public class TBattleground
		{
			// Public Map As UInteger
			public byte MinPlayersPerTeam;
			public byte MaxPlayersPerTeam;
			public byte MinLevel;
			public byte MaxLevel;
			public int AllianceStartLoc;
			public float AllianceStartO;
			public int HordeStartLoc;
			public float HordeStartO;
		}

		private readonly string ChatChannelsDBC = "dbc" + Path.DirectorySeparatorChar + "ChatChannels.dbc";
		public Dictionary<int, ChatChannelInfo> ChatChannelsInfo = new Dictionary<int, ChatChannelInfo>();

		public void InitializeChatChannels()
		{
			try
			{
				var data = new BufferedDbc(ChatChannelsDBC);
				for (int i = 0, loopTo = new BufferedDbc(ChatChannelsDBC).Rows - 1; i <= loopTo; i++)
				{
					var ChatChannels = new ChatChannelInfo()
					{
						Index = data.Read<int>(i, 0),
						Flags = data.Read<int>(i, 1),
						Name = data.Read<string>(i, 3),
					};
					ChatChannelsInfo.Add(ChatChannels.Index, ChatChannels);
				}

				ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ChatChannels Initialized.", new BufferedDbc(ChatChannelsDBC).Rows - 1);
				new BufferedDbc(ChatChannelsDBC).Dispose();
			}
			catch (DirectoryNotFoundException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("DBC File : ChatChannels.dbc missing.");
				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}

		public class ChatChannelInfo
		{
			public int Index;
			public int Flags;
			public string Name;
		}

		private readonly string ChrRacesDBC = "dbc" + Path.DirectorySeparatorChar + "ChrRaces.dbc";

		public void InitializeCharRaces()
		{
			try
			{
				// Loading from DBC
				int raceID;
				int factionID;
				int modelM;
				int modelF;
				int teamID; // 1 = Horde / 7 = Alliance
				int cinematicID;
				var data = new BufferedDbc(ChrRacesDBC);
				for (int i = 0, loopTo = new BufferedDbc(ChrRacesDBC).Rows - 1; i <= loopTo; i++)
				{
					raceID = data.Read<int>(i, 0);
					factionID = data.Read<int>(i, 2);
					modelM = data.Read<int>(i, 4);
					modelF = data.Read<int>(i, 5);
					teamID = data.Read<int>(i, 8);
					cinematicID = data.Read<int>(i, 16);
					CharRaces[(byte)raceID] = new TCharRace((short)factionID, modelM, modelF, (byte)teamID, cinematicID);
				}

				ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ChrRace Loaded.", new BufferedDbc(ChrRacesDBC).Rows - 1);
				new BufferedDbc(ChrRacesDBC).Dispose();
			}
			catch (DirectoryNotFoundException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("DBC File : ChrRaces.dbc missing.");
				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}

		private readonly string ChrClassesDBC = "dbc" + Path.DirectorySeparatorChar + "ChrClasses.dbc";

		public void InitializeCharClasses()
		{
			try
			{
				// Loading from DBC
				int classID;
				int cinematicID;
				for (int i = 0, loopTo = new BufferedDbc(ChrClassesDBC).Rows - 1; i <= loopTo; i++)
				{
					var data = new BufferedDbc(ChrClassesDBC);
					classID = data.Read<int>(i, 0);
					cinematicID = data.Read<int>(i, 5); // or 14 or 15?
					CharClasses[(byte)classID] = new TCharClass(cinematicID);
				}

				ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ChrClasses Loaded.", new BufferedDbc(ChrClassesDBC).Rows - 1);
				new BufferedDbc(ChrClassesDBC).Dispose();
			}
			catch (DirectoryNotFoundException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("DBC File : ChrClasses.dbc missing.");
				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}

		public Dictionary<int, TCharRace> CharRaces = new Dictionary<int, TCharRace>();

		public class TCharRace
		{
			public short FactionID;
			public int ModelMale;
			public int ModelFemale;
			public byte TeamID;
			public int CinematicID;

			public TCharRace(short faction, int modelM, int modelF, byte team, int cinematic)
			{
				FactionID = faction;
				ModelMale = modelM;
				ModelFemale = modelF;
				TeamID = team;
				CinematicID = cinematic;
			}
		}

		public Dictionary<int, TCharClass> CharClasses = new Dictionary<int, TCharClass>();

		public class TCharClass
		{
			public int CinematicID;

			public TCharClass(int cinematic)
			{
				CinematicID = cinematic;
			}
		}
	}
}