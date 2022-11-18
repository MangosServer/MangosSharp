//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Map;
using Mangos.Common.Legacy;
using Mangos.DataStores;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Mangos.Cluster.DataStores;

public class WsDbcDatabase
{
    private readonly ClusterServiceLocator _clusterServiceLocator;
    private readonly DataStoreProvider _dataStoreProvider;

    public WsDbcDatabase(DataStoreProvider dataStoreProvider, ClusterServiceLocator clusterServiceLocator)
    {
        _dataStoreProvider = dataStoreProvider;
        _clusterServiceLocator = clusterServiceLocator;
    }

    private readonly string _mapDbc = "Map.dbc";
    public Dictionary<int, MapInfo> Maps = new();

    public async Task InitializeMapsAsync()
    {
        try
        {
            var data = await _dataStoreProvider.GetDataStoreAsync(_mapDbc);
            for (int i = 0, loopTo = data.Rows - 1; i <= loopTo; i++)
            {
                MapInfo m = new()
                {
                    Id = data.ReadInt(i, 0),
                    Type = (MapTypes)data.ReadInt(i, 2),
                    Name = data.ReadString(i, 4),
                    ParentMap = data.ReadInt(i, 3),
                    ResetTime = data.ReadInt(i, 38),
                };
                Maps.Add(m.Id, m);
            }

            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Maps Initialized.", data.Rows - 1);
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
        public int Id;
        public MapTypes Type = MapTypes.MAP_COMMON;
        public string Name = "";
        public int ParentMap = -1;
        public int ResetTime;

        public bool IsDungeon => Type is MapTypes.MAP_INSTANCE or MapTypes.MAP_RAID;

        public bool IsRaid => Type == MapTypes.MAP_RAID;

        public bool IsBattleGround => Type == MapTypes.MAP_BATTLEGROUND;

        public bool HasResetTime => ResetTime != 0;
    }

    private readonly string _worldSafeLocsDbc = "WorldSafeLocs.dbc";
    public Dictionary<int, WorldSafeLoc> WorldSafeLocs = new();

    public async Task InitializeWorldSafeLocsAsync()
    {
        try
        {
            var data = await _dataStoreProvider.GetDataStoreAsync(_worldSafeLocsDbc);
            for (int i = 0, loopTo = data.Rows - 1; i <= loopTo; i++)
            {
                WorldSafeLoc worldSafeLoc = new()
                {
                    Id = data.ReadInt(i, 0),
                    Map = (uint)data.ReadInt(i, 1),
                    X = data.ReadFloat(i, 2),
                    Y = data.ReadFloat(i, 3),
                    Z = data.ReadFloat(i, 4),
                };
                WorldSafeLocs.Add(worldSafeLoc.Id, worldSafeLoc);
            }

            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} WorldSafeLocs Initialized.", data.Rows - 1);
        }
        catch (DirectoryNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("DBC File : WorldSafeLocs.dbc missing.");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    public class WorldSafeLoc
    {
        public int Id;
        public uint Map;
        public float X;
        public float Y;
        public float Z;
    }

    public Dictionary<byte, Battleground> Battlegrounds = new();

    public void InitializeBattlegrounds()
    {
        byte entry;
        DataTable mySqlQuery = new();
        _clusterServiceLocator.WorldCluster.GetWorldDatabase().Query("SELECT * FROM battleground_template", ref mySqlQuery);
        foreach (DataRow row in mySqlQuery.Rows)
        {
            entry = row.As<byte>("id");
            Battlegrounds.Add(entry, new Battleground());

            // TODO: the MAPId needs to be located from somewhere other than the template file
            // BUG: THIS IS AN UGLY HACK UNTIL THE ABOVE IS FIXED
            // Battlegrounds(Entry).Map = row.Item("Map")
            Battlegrounds[entry].MinPlayersPerTeam = row.As<byte>("MinPlayersPerTeam");
            Battlegrounds[entry].MaxPlayersPerTeam = row.As<byte>("MaxPlayersPerTeam");
            Battlegrounds[entry].MinLevel = row.As<byte>("MinLvl");
            Battlegrounds[entry].MaxLevel = row.As<byte>("MaxLvl");
            Battlegrounds[entry].AllianceStartLoc = row.As<int>("AllianceStartLoc");
            Battlegrounds[entry].AllianceStartO = row.As<float>("AllianceStartO");
            Battlegrounds[entry].HordeStartLoc = row.As<int>("HordeStartLoc");
            Battlegrounds[entry].HordeStartO = row.As<float>("HordeStartO");
        }

        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "World: {0} Battlegrounds Initialized.", mySqlQuery.Rows.Count);
    }

    public class Battleground
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

    private readonly string _chatChannelsDbc = "ChatChannels.dbc";
    public Dictionary<int, ChatChannelInfo> ChatChannelsInfo = new();

    public async Task InitializeChatChannelsAsync()
    {
        try
        {
            var data = await _dataStoreProvider.GetDataStoreAsync(_chatChannelsDbc);
            for (int i = 0, loopTo = data.Rows - 1; i <= loopTo; i++)
            {
                ChatChannelInfo chatChannels = new()
                {
                    Index = data.ReadInt(i, 0),
                    Flags = data.ReadInt(i, 1),
                    Name = data.ReadString(i, 3),
                };
                ChatChannelsInfo.Add(chatChannels.Index, chatChannels);
            }

            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ChatChannels Initialized.", data.Rows - 1);
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

    private readonly string _chrRacesDbc = "ChrRaces.dbc";

    public async Task InitializeCharRacesAsync()
    {
        try
        {
            // Loading from DBC
            int raceId;
            int factionId;
            int modelM;
            int modelF;
            int teamId; // 1 = Horde / 7 = Alliance
            int cinematicId;
            var data = await _dataStoreProvider.GetDataStoreAsync(_chrRacesDbc);
            for (int i = 0, loopTo = data.Rows - 1; i <= loopTo; i++)
            {
                raceId = data.ReadInt(i, 0);
                factionId = data.ReadInt(i, 2);
                modelM = data.ReadInt(i, 4);
                modelF = data.ReadInt(i, 5);
                teamId = data.ReadInt(i, 8);
                cinematicId = data.ReadInt(i, 16);
                CharRaces[(byte)raceId] = new CharRace((short)factionId, modelM, modelF, (byte)teamId, cinematicId);
            }

            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ChrRace Loaded.", data.Rows - 1);
        }
        catch (DirectoryNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("DBC File : ChrRaces.dbc missing.");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    private readonly string _chrClassesDbc = "ChrClasses.dbc";

    public async Task InitializeCharClassesAsync()
    {
        try
        {
            // Loading from DBC
            int classId;
            int cinematicId;
            var dataStore = await _dataStoreProvider.GetDataStoreAsync(_chrClassesDbc);
            for (int i = 0, loopTo = dataStore.Rows - 1; i <= loopTo; i++)
            {
                classId = dataStore.ReadInt(i, 0);
                cinematicId = dataStore.ReadInt(i, 5); // or 14 or 15?
                CharClasses[(byte)classId] = new CharClass(cinematicId);
            }

            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ChrClasses Loaded.", dataStore.Rows - 1);
        }
        catch (DirectoryNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("DBC File : ChrClasses.dbc missing.");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    public Dictionary<int, CharRace> CharRaces = new();

    public class CharRace
    {
        public short FactionId;
        public int ModelMale;
        public int ModelFemale;
        public byte TeamId;
        public int CinematicId;

        public CharRace(short faction, int modelM, int modelF, byte team, int cinematic)
        {
            FactionId = faction;
            ModelMale = modelM;
            ModelFemale = modelF;
            TeamId = team;
            CinematicId = cinematic;
        }
    }

    public Dictionary<int, CharClass> CharClasses = new();

    public class CharClass
    {
        public int CinematicId;

        public CharClass(int cinematic)
        {
            CinematicId = cinematic;
        }
    }
}
