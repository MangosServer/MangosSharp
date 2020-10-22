//
//  Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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
using System.Threading.Tasks;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Map;
using Mangos.Common.Legacy;
using Mangos.DataStores;

namespace Mangos.Cluster.DataStores
{
    public class WS_DBCDatabase
    {
        private readonly ClusterServiceLocator clusterServiceLocator;
        private readonly DataStoreProvider dataStoreProvider;

        public WS_DBCDatabase(DataStoreProvider dataStoreProvider, ClusterServiceLocator clusterServiceLocator)
        {
            this.dataStoreProvider = dataStoreProvider;
            this.clusterServiceLocator = clusterServiceLocator;
        }

        private readonly string MapDBC = "Map.dbc";
        public Dictionary<int, MapInfo> Maps = new Dictionary<int, MapInfo>();

        public async Task InitializeMapsAsync()
        {
            try
            {
                var data = await dataStoreProvider.GetDataStoreAsync(MapDBC);
                for (int i = 0, loopTo = data.Rows - 1; i <= loopTo; i++)
                {
                    var m = new MapInfo
                    {
                        ID = data.ReadInt(i, 0),
                        Type = (MapTypes)data.ReadInt(i, 2),
                        Name = data.ReadString(i, 4),
                        ParentMap = data.ReadInt(i, 3),
                        ResetTime = data.ReadInt(i, 38),
                    };
                    Maps.Add(m.ID, m);
                }

                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Maps Initialized.", data.Rows - 1);
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
            public int ResetTime;

            public bool IsDungeon => Type == MapTypes.MAP_INSTANCE || Type == MapTypes.MAP_RAID;

            public bool IsRaid => Type == MapTypes.MAP_RAID;

            public bool IsBattleGround => Type == MapTypes.MAP_BATTLEGROUND;

            public bool HasResetTime => ResetTime != 0;
        }

        private readonly string WorldSafeLocsDBC = "WorldSafeLocs.dbc";
        public Dictionary<int, TWorldSafeLoc> WorldSafeLocs = new Dictionary<int, TWorldSafeLoc>();

        public async Task InitializeWorldSafeLocsAsync()
        {
            try
            {
                var data = await dataStoreProvider.GetDataStoreAsync(WorldSafeLocsDBC);
                for (int i = 0, loopTo = data.Rows - 1; i <= loopTo; i++)
                {
                    var WorldSafeLoc = new TWorldSafeLoc
                    {
                        ID = data.ReadInt(i, 0),
                        map = (uint)data.ReadInt(i, 1),
                        x = data.ReadFloat(i, 2),
                        y = data.ReadFloat(i, 3),
                        z = data.ReadFloat(i, 4),
                    };
                    WorldSafeLocs.Add(WorldSafeLoc.ID, WorldSafeLoc);
                }

                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} WorldSafeLocs Initialized.", data.Rows - 1);
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
            clusterServiceLocator._WorldCluster.GetWorldDatabase().Query("SELECT * FROM battleground_template", ref MySQLQuery);
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

            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "World: {0} Battlegrounds Initialized.", MySQLQuery.Rows.Count);
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

        private readonly string ChatChannelsDBC = "ChatChannels.dbc";
        public Dictionary<int, ChatChannelInfo> ChatChannelsInfo = new Dictionary<int, ChatChannelInfo>();

        public async Task InitializeChatChannelsAsync()
        {
            try
            {
                var data = await dataStoreProvider.GetDataStoreAsync(ChatChannelsDBC);
                for (int i = 0, loopTo = data.Rows - 1; i <= loopTo; i++)
                {
                    var ChatChannels = new ChatChannelInfo
                    {
                        Index = data.ReadInt(i, 0),
                        Flags = data.ReadInt(i, 1),
                        Name = data.ReadString(i, 3),
                    };
                    ChatChannelsInfo.Add(ChatChannels.Index, ChatChannels);
                }

                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ChatChannels Initialized.", data.Rows - 1);
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

        private readonly string ChrRacesDBC = "ChrRaces.dbc";

        public async Task InitializeCharRacesAsync()
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
                var data = await dataStoreProvider.GetDataStoreAsync(ChrRacesDBC);
                for (int i = 0, loopTo = data.Rows - 1; i <= loopTo; i++)
                {
                    raceID = data.ReadInt(i, 0);
                    factionID = data.ReadInt(i, 2);
                    modelM = data.ReadInt(i, 4);
                    modelF = data.ReadInt(i, 5);
                    teamID = data.ReadInt(i, 8);
                    cinematicID = data.ReadInt(i, 16);
                    CharRaces[(byte)raceID] = new TCharRace((short)factionID, modelM, modelF, (byte)teamID, cinematicID);
                }

                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ChrRace Loaded.", data.Rows - 1);
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : ChrRaces.dbc missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private readonly string ChrClassesDBC = "ChrClasses.dbc";

        public async Task InitializeCharClassesAsync()
        {
            try
            {
                // Loading from DBC
                int classID;
                int cinematicID;
                var dataStore = await dataStoreProvider.GetDataStoreAsync(ChrClassesDBC);
                for (int i = 0, loopTo = dataStore.Rows - 1; i <= loopTo; i++)
                {
                    classID = dataStore.ReadInt(i, 0);
                    cinematicID = dataStore.ReadInt(i, 5); // or 14 or 15?
                    CharClasses[(byte)classID] = new TCharClass(cinematicID);
                }

                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ChrClasses Loaded.", dataStore.Rows - 1);
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