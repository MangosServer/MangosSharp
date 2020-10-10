// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mangos.Common.DataStores;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Map;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Maps
{
    public class WS_Maps
    {
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<int, TArea> AreaTable = new Dictionary<int, TArea>();

        public int GetAreaIDByMapandParent(int mapId, int parentID)
        {
            foreach (KeyValuePair<int, TArea> thisArea in AreaTable)
            {
                int thisMap = thisArea.Value.mapId;
                int thisParent = thisArea.Value.Zone;
                if (thisMap == mapId & thisParent == parentID)
                {
                    return thisArea.Key;
                }
            }

            return -999; // Lol?
        }

        public class TArea
        {
            public int ID;
            public int mapId;
            public byte Level;
            public int Zone;
            public int ZoneType;
            public AreaTeam Team;
            public string Name;

            public bool IsMyLand(ref WS_PlayerData.CharacterObject objCharacter)
            {
                if (Team == AreaTeam.AREATEAM_NONE)
                    return false;
                if (objCharacter.IsHorde == false)
                    return Team == AreaTeam.AREATEAM_ALLY;
                if (objCharacter.IsHorde == true)
                    return Team == AreaTeam.AREATEAM_HORDE;
                return false;
            }

            public bool IsCity()
            {
                return ZoneType == 312;
            }
            // TODO: REMOVE
            public bool NeedFlyingMount()
            {
                return ZoneType & AreaFlag.AREA_FLAG_NEED_FLY;
            }
            // TODO: REMOVE
            public bool IsSanctuary()
            {
                return ZoneType & AreaFlag.AREA_FLAG_SANCTUARY;
            }
            // TODO: REMOVE
            public bool IsArena()
            {
                return ZoneType & AreaFlag.AREA_FLAG_ARENA;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        public int RESOLUTION_ZMAP = 0;


        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        // NOTE: Map resolution. The resolution of your map files in your maps folder.
        // Public Const RESOLUTION_ZMAP As Integer = 256 - 1

        public class TMapTile : IDisposable
        {

            // TMap contains 64x64 TMapTile(s)
            public ushort[,] AreaFlag = new ushort[WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS + 1, WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS + 1];
            public byte[,] AreaTerrain = new byte[WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN + 1, WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN + 1];
            public float[,] WaterLevel = new float[WorldServiceLocator._Global_Constants.RESOLUTION_WATER + 1, WorldServiceLocator._Global_Constants.RESOLUTION_WATER + 1];
            // Public ZCoord(RESOLUTION_ZMAP, RESOLUTION_ZMAP) As Single
            public float[,] ZCoord;

            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            public List<ulong> PlayersHere = new List<ulong>();
            public List<ulong> CreaturesHere = new List<ulong>();
            public List<ulong> GameObjectsHere = new List<ulong>();
            public List<ulong> CorpseObjectsHere = new List<ulong>();
            public List<ulong> DynamicObjectsHere = new List<ulong>();
            private readonly byte CellX;
            private readonly byte CellY;
            private readonly uint CellMap;

            public TMapTile(byte tileX, byte tileY, uint tileMap)
            {
                // DONE: Don't load maptiles we don't handle
                if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey(tileMap))
                    return;
                ZCoord = new float[WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP + 1, WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP + 1];
                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                CellX = tileX;
                CellY = tileY;
                CellMap = tileMap;
                string fileName;
                string fileVersion;
                FileStream f;
                BinaryReader b;
                int x, y;

                // DONE: Loading MAP file
                fileName = string.Format("{0}{1}{2}.map", Strings.Format(tileMap, "000"), Strings.Format(tileX, "00"), Strings.Format(tileY, "00"));
                if (!File.Exists(@"maps\" + fileName))
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Map file [{0}] not found", fileName);
                }
                else
                {
                    f = new FileStream(@"maps\" + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 82704, FileOptions.SequentialScan);
                    b = new BinaryReader(f);
                    fileVersion = System.Text.Encoding.ASCII.GetString(b.ReadBytes(8), 0, 8);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading map file [{0}] version [{1}]", fileName, fileVersion);
                    var loopTo = WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS;
                    for (x = 0; x <= loopTo; x++)
                    {
                        var loopTo1 = WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS;
                        for (y = 0; y <= loopTo1; y++)
                            AreaFlag[x, y] = b.ReadUInt16();
                    }

                    var loopTo2 = WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN;
                    for (x = 0; x <= loopTo2; x++)
                    {
                        var loopTo3 = WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN;
                        for (y = 0; y <= loopTo3; y++)
                            AreaTerrain[x, y] = b.ReadByte();
                    }

                    var loopTo4 = WorldServiceLocator._Global_Constants.RESOLUTION_WATER;
                    for (x = 0; x <= loopTo4; x++)
                    {
                        var loopTo5 = WorldServiceLocator._Global_Constants.RESOLUTION_WATER;
                        for (y = 0; y <= loopTo5; y++)
                            WaterLevel[x, y] = b.ReadSingle();
                    }

                    var loopTo6 = WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP;
                    for (x = 0; x <= loopTo6; x++)
                    {
                        var loopTo7 = WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP;
                        for (y = 0; y <= loopTo7; y++)
                            ZCoord[x, y] = b.ReadSingle();
                    }

                    b.Close();
                    // f.Close()
                    // f.Dispose()
                }

                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            }

            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    WorldServiceLocator._WS_Maps.UnloadSpawns(CellX, CellY, CellMap);
                }

                _disposedValue = true;
            }

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        }

        public class TMap : IDisposable
        {
            public int ID;
            public MapTypes Type = MapTypes.MAP_COMMON;
            public string Name = "";
            public bool[,] TileUsed = new bool[64, 64]; // The same maptile should no longer be loaded twice
            public TMapTile[,] Tiles = new TMapTile[64, 64];

            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
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

            public int ResetTime
            {
                get
                {
                    switch (Type)
                    {
                        case var @case when @case == MapTypes.MAP_BATTLEGROUND:
                            {
                                return WorldServiceLocator._Global_Constants.DEFAULT_BATTLEFIELD_EXPIRE_TIME;
                            }

                        case var case1 when case1 == MapTypes.MAP_RAID:
                        case var case2 when case2 == MapTypes.MAP_INSTANCE:
                            {
                                // * Molten Core: Every Tuesday at 3:00AM or during weekly maintenance
                                // * Blackwing Lair: Every Tuesday at 3:00AM or during weekly maintenance
                                // * Onyxia's Lair: Every 5 days at 3:00AM
                                // * Zul'Gurub: Every 3 days at 3:00AM
                                // * Ruins of Ahn'Qiraj: Every 3 days at 3:00AM
                                // * Temple of Ahn'Qiraj: Every Tuesday at 3:00AM or during weekly maintenance
                                // * Naxxramas: Every Tuesday at 3:00AM or during weekly maintenance
                                switch (ID)
                                {
                                    case 249: // Onyxia's Lair
                                        {
                                            return (int)WorldServiceLocator._Functions.GetNextDate(5, 3).Subtract(DateAndTime.Now).TotalSeconds;
                                        }

                                    case 309:
                                    case 509: // Zul'Gurub and Ruins of Ahn'Qiraj
                                        {
                                            return (int)WorldServiceLocator._Functions.GetNextDate(3, 3).Subtract(DateAndTime.Now).TotalSeconds;
                                        }

                                    case 409:
                                    case 469:
                                    case 531:
                                    case 533: // Molten Core, Blackwing Lair, Temple of Ahn'Qiraj and Naxxramas
                                        {
                                            return (int)WorldServiceLocator._Functions.GetNextDay(DayOfWeek.Tuesday, 3).Subtract(DateAndTime.Now).TotalSeconds;
                                        }
                                }

                                break;
                            }
                    }

                    return WorldServiceLocator._Global_Constants.DEFAULT_INSTANCE_EXPIRE_TIME;
                }
            }

            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            public TMap(int Map)
            {
                if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey((uint)Map))
                {
                    WorldServiceLocator._WS_Maps.Maps.Add((uint)Map, this);
                    for (int x = 0; x <= 63; x++)
                    {
                        for (int y = 0; y <= 63; y++)
                            TileUsed[x, y] = false;
                    }

                    try
                    {
                        var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "Map.dbc");
                        int tmpMap;
                        for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                        {
                            tmpMap = (int)tmpDBC.Item(i, 0);
                            if (tmpMap == Map)
                            {
                                ID = Map;
                                Type = (MapTypes)tmpDBC.Item(i, 2, DBCValueType.DBC_INTEGER);
                                Name = (string)tmpDBC.Item(i, 4, DBCValueType.DBC_STRING);
                                break;
                            }
                        }

                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: 1 Map initialized.", tmpDBC.Rows - 1);
                        tmpDBC.Dispose();
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("DBC File : Map missing.");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    for (int i = 0; i <= 63; i++)
                    {
                        for (int j = 0; j <= 63; j++)
                        {
                            if (Tiles[i, j] is object)
                                Tiles[i, j].Dispose();
                        }
                    }

                    WorldServiceLocator._WS_Maps.Maps.Remove((uint)ID);
                    // iTree.Dispose()
                }

                _disposedValue = true;
            }

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        }

        public Dictionary<uint, TMap> Maps = new Dictionary<uint, TMap>();
        public string MapList;

        public void InitializeMaps()
        {
            // DONE: Creating map list for queries
            IEnumerator e = WorldServiceLocator._WorldServer.Config.Maps.GetEnumerator();
            e.Reset();
            if (e.MoveNext())
            {
                MapList = Conversions.ToString(e.Current);
                while (e.MoveNext())
                    MapList = Conversions.ToString(MapList + Operators.ConcatenateObject(", ", e.Current));
            }

            // 'DONE: Loading maps
            foreach (uint id in WorldServiceLocator._WorldServer.Config.Maps)
                var map = new TMap((int)id);
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Initalizing: {0} Maps initialized.", Maps.Count);
        }

        /// <summary>
        /// Ensures that the Coordinate is within valid boundaries and if not, brings is back into boundary
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public float ValidateMapCoord(float coord)
        {
            if (coord > 32 * WorldServiceLocator._Global_Constants.SIZE)
            {
                // Invalid Value for X provided, so clamp it to x
                coord = 32 * WorldServiceLocator._Global_Constants.SIZE;
            }
            else if (coord < -32 * WorldServiceLocator._Global_Constants.SIZE)
            {
                // Invalid Value for X provided, so clamp it to -x
                coord = -32 * WorldServiceLocator._Global_Constants.SIZE;
            }

            return coord;
        }

        public void GetMapTile(float x, float y, ref byte MapTileX, ref byte MapTileY)
        {
            // How to calculate where is X,Y:
            MapTileX = Fix(32 - ValidateMapCoord(x) / WorldServiceLocator._Global_Constants.SIZE);
            MapTileY = Fix(32 - ValidateMapCoord(y) / WorldServiceLocator._Global_Constants.SIZE);
        }

        public byte GetMapTileX(float x)
        {
            return Fix(32 - ValidateMapCoord(x) / WorldServiceLocator._Global_Constants.SIZE);
        }

        public byte GetMapTileY(float y)
        {
            return Fix(32 - ValidateMapCoord(y) / WorldServiceLocator._Global_Constants.SIZE);
        }

        public byte GetSubMapTileX(float x)
        {
            return Fix(RESOLUTION_ZMAP * (32 - ValidateMapCoord(x) / WorldServiceLocator._Global_Constants.SIZE - Fix(32 - ValidateMapCoord(x) / WorldServiceLocator._Global_Constants.SIZE)));
        }

        public byte GetSubMapTileY(float y)
        {
            return Fix(RESOLUTION_ZMAP * (32 - ValidateMapCoord(y) / WorldServiceLocator._Global_Constants.SIZE - Fix(32 - ValidateMapCoord(y) / WorldServiceLocator._Global_Constants.SIZE)));
        }

        public float GetZCoord(float x, float y, uint Map)
        {
            try
            {
                x = ValidateMapCoord(x);
                y = ValidateMapCoord(y);
                byte MapTileX = Fix(32 - x / WorldServiceLocator._Global_Constants.SIZE);
                byte MapTileY = Fix(32 - y / WorldServiceLocator._Global_Constants.SIZE);
                byte MapTile_LocalX = RESOLUTION_ZMAP * (32 - x / WorldServiceLocator._Global_Constants.SIZE - MapTileX);
                byte MapTile_LocalY = RESOLUTION_ZMAP * (32 - y / WorldServiceLocator._Global_Constants.SIZE - MapTileY);
                float xNormalized = RESOLUTION_ZMAP * (32 - x / WorldServiceLocator._Global_Constants.SIZE - MapTileX) - MapTile_LocalX;
                float yNormalized = RESOLUTION_ZMAP * (32 - y / WorldServiceLocator._Global_Constants.SIZE - MapTileY) - MapTile_LocalY;
                if (Maps[Map].Tiles[MapTileX, MapTileY] is null)
                    return 0.0f;
                try
                {
                    float topHeight = WorldServiceLocator._Functions.MathLerp(GetHeight(Map, MapTileX, MapTileY, MapTile_LocalX, MapTile_LocalY), GetHeight(Map, MapTileX, MapTileY, (byte)(MapTile_LocalX + 1), MapTile_LocalY), xNormalized);
                    float bottomHeight = WorldServiceLocator._Functions.MathLerp(GetHeight(Map, MapTileX, MapTileY, MapTile_LocalX, (byte)(MapTile_LocalY + 1)), GetHeight(Map, MapTileX, MapTileY, (byte)(MapTile_LocalX + 1), (byte)(MapTile_LocalY + 1)), xNormalized);
                    return WorldServiceLocator._Functions.MathLerp(topHeight, bottomHeight, yNormalized);
                }
                catch
                {
                    return Maps[Map].Tiles[MapTileX, MapTileY].ZCoord[MapTile_LocalX, MapTile_LocalY];
                }
            }
            catch (Exception)
            {
                return 0.0f;
            }
        }

        public float GetWaterLevel(float x, float y, int Map)
        {
            x = ValidateMapCoord(x);
            y = ValidateMapCoord(y);
            byte MapTileX = Fix(32 - x / WorldServiceLocator._Global_Constants.SIZE);
            byte MapTileY = Fix(32 - y / WorldServiceLocator._Global_Constants.SIZE);
            byte MapTile_LocalX = WorldServiceLocator._Global_Constants.RESOLUTION_WATER * (32 - x / WorldServiceLocator._Global_Constants.SIZE - MapTileX);
            byte MapTile_LocalY = WorldServiceLocator._Global_Constants.RESOLUTION_WATER * (32 - y / WorldServiceLocator._Global_Constants.SIZE - MapTileY);
            if (Maps[(uint)Map].Tiles[MapTileX, MapTileY] is null)
                return 0f;
            return Maps[(uint)Map].Tiles[MapTileX, MapTileY].WaterLevel[MapTile_LocalX, MapTile_LocalY];
        }

        public byte GetTerrainType(float x, float y, int Map)
        {
            x = ValidateMapCoord(x);
            y = ValidateMapCoord(y);
            byte MapTileX = Fix(32 - x / WorldServiceLocator._Global_Constants.SIZE);
            byte MapTileY = Fix(32 - y / WorldServiceLocator._Global_Constants.SIZE);
            byte MapTile_LocalX = WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN * (32 - x / WorldServiceLocator._Global_Constants.SIZE - MapTileX);
            byte MapTile_LocalY = WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN * (32 - y / WorldServiceLocator._Global_Constants.SIZE - MapTileY);
            if (Maps[(uint)Map].Tiles[MapTileX, MapTileY] is null)
                return 0;
            return Maps[(uint)Map].Tiles[MapTileX, MapTileY].AreaTerrain[MapTile_LocalX, MapTile_LocalY];
        }

        public int GetAreaFlag(float x, float y, int Map)
        {
            x = ValidateMapCoord(x);
            y = ValidateMapCoord(y);
            byte MapTileX = Fix(32 - x / WorldServiceLocator._Global_Constants.SIZE);
            byte MapTileY = Fix(32 - y / WorldServiceLocator._Global_Constants.SIZE);
            byte MapTile_LocalX = WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS * (32 - x / WorldServiceLocator._Global_Constants.SIZE - MapTileX);
            byte MapTile_LocalY = WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS * (32 - y / WorldServiceLocator._Global_Constants.SIZE - MapTileY);
            if (Maps[(uint)Map].Tiles[MapTileX, MapTileY] is null)
                return 0;
            return Maps[(uint)Map].Tiles[MapTileX, MapTileY].AreaFlag[MapTile_LocalX, MapTile_LocalY];
        }

        public bool IsOutsideOfMap(ref WS_Base.BaseObject objCharacter)
        {
            // NOTE: Disabled these checks because DBC data contains too big X/Y coords to be usefull
            return false;

            // Dim x As Single = objCharacter.positionX
            // Dim y As Single = objCharacter.positionY
            // Dim m As UInteger = objCharacter.MapID

            // 'Check transform data
            // For Each i As WorldMapTransformsDimension In WorldMapTransforms
            // If i.Map = m Then
            // With i
            // If x < .X_Maximum And x > .X_Minimum And _
            // y < .Y_Maximum And y > .Y_Minimum Then

            // _WorldServer.Log.WriteLine(LogType.USER, "Applying map transform {0},{1},{2} -> {3},{4},{5}", x, y, m, .Dest_X, .Dest_Y, .Dest_Map)
            // 'x += .Dest_X
            // 'y += .Dest_Y
            // 'm = .Dest_Map
            // 'Exit For
            // Return False
            // End If
            // End With
            // End If
            // Next

            // 'Check Map data
            // If WorldMapContinent.ContainsKey(m) Then
            // With WorldMapContinent(m)
            // If x > .X_Maximum Or x < .X_Minimum Or _
            // y > .Y_Maximum Or y < .Y_Minimum Then
            // _WorldServer.Log.WriteLine(LogType.USER, "Outside map: {0:X}", objCharacter.GUID)
            // Return True
            // Else
            // Return False
            // End If
            // End With
            // End If

            // _WorldServer.Log.WriteLine(LogType.USER, "WorldMapContinent not found for map {0}.", objCharacter.MapID)
            // Return False
        }

        /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped ElseDirectiveTrivia */
        public float GetZCoord(float x, float y, float z, uint Map)
        {
            try
            {
                x = ValidateMapCoord(x);
                y = ValidateMapCoord(y);
                z = ValidateMapCoord(z);
                byte MapTileX = Fix(32 - x / WorldServiceLocator._Global_Constants.SIZE);
                byte MapTileY = Fix(32 - y / WorldServiceLocator._Global_Constants.SIZE);
                byte MapTile_LocalX = RESOLUTION_ZMAP * (32 - x / WorldServiceLocator._Global_Constants.SIZE - MapTileX);
                byte MapTile_LocalY = RESOLUTION_ZMAP * (32 - y / WorldServiceLocator._Global_Constants.SIZE - MapTileY);
                float xNormalized = RESOLUTION_ZMAP * (32 - x / WorldServiceLocator._Global_Constants.SIZE - MapTileX) - MapTile_LocalX;
                float yNormalized = RESOLUTION_ZMAP * (32 - y / WorldServiceLocator._Global_Constants.SIZE - MapTileY) - MapTile_LocalY;
                if (Maps[Map].Tiles[MapTileX, MapTileY] is null)
                {
                    // Return vmap height if one was found
                    float VMapHeight = GetVMapHeight(Map, x, y, z + 5.0f);
                    if (VMapHeight != WorldServiceLocator._Global_Constants.VMAP_INVALID_HEIGHT_VALUE)
                    {
                        return VMapHeight;
                    }

                    return 0.0f;
                }

                if (Math.Abs(Maps[Map].Tiles[MapTileX, MapTileY].ZCoord[MapTile_LocalX, MapTile_LocalY] - z) >= 2.0f)
                {
                    // Return vmap height if one was found
                    float VMapHeight = GetVMapHeight(Map, x, y, z + 5.0f);
                    if (VMapHeight != WorldServiceLocator._Global_Constants.VMAP_INVALID_HEIGHT_VALUE)
                    {
                        return VMapHeight;
                    }
                }

                try
                {
                    float topHeight = WorldServiceLocator._Functions.MathLerp(GetHeight(Map, MapTileX, MapTileY, MapTile_LocalX, MapTile_LocalY), GetHeight(Map, MapTileX, MapTileY, (byte)(MapTile_LocalX + 1), MapTile_LocalY), xNormalized);
                    float bottomHeight = WorldServiceLocator._Functions.MathLerp(GetHeight(Map, MapTileX, MapTileY, MapTile_LocalX, (byte)(MapTile_LocalY + 1)), GetHeight(Map, MapTileX, MapTileY, (byte)(MapTile_LocalX + 1), (byte)(MapTile_LocalY + 1)), xNormalized);
                    return WorldServiceLocator._Functions.MathLerp(topHeight, bottomHeight, yNormalized);
                }
                catch
                {
                    return Maps[Map].Tiles[MapTileX, MapTileY].ZCoord[MapTile_LocalX, MapTile_LocalY];
                }
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex.ToString());
                return z;
            }
        }
        /* TODO ERROR: Skipped EndIfDirectiveTrivia */
        private float GetHeight(uint Map, byte MapTileX, byte MapTileY, byte MapTileLocalX, byte MapTileLocalY)
        {
            if (MapTileLocalX > RESOLUTION_ZMAP)
            {
                MapTileX = (byte)(MapTileX + 1);
                MapTileLocalX = (byte)(MapTileLocalX - (RESOLUTION_ZMAP + 1));
            }
            else if (MapTileLocalX < 0)
            {
                MapTileX = (byte)(MapTileX - 1);
                MapTileLocalX = (byte)(-MapTileLocalX - 1);
            }

            if (MapTileLocalY > RESOLUTION_ZMAP)
            {
                MapTileY = (byte)(MapTileY + 1);
                MapTileLocalY = (byte)(MapTileLocalY - (RESOLUTION_ZMAP + 1));
            }
            else if (MapTileLocalY < 0)
            {
                MapTileY = (byte)(MapTileY - 1);
                MapTileLocalY = (byte)(-MapTileLocalY - 1);
            }

            return Maps[Map].Tiles[MapTileX, MapTileY].ZCoord[MapTileLocalX, MapTileLocalY];
        }

        public bool IsInLineOfSight(ref WS_Base.BaseObject obj, ref WS_Base.BaseObject obj2)
        {
            return IsInLineOfSight(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2.0f, obj2.positionX, obj2.positionY, obj2.positionZ + 2.0f);
        }

        public bool IsInLineOfSight(ref WS_Base.BaseObject obj, float x2, float y2, float z2)
        {
            x2 = ValidateMapCoord(x2);
            y2 = ValidateMapCoord(y2);
            z2 = ValidateMapCoord(z2);
            return IsInLineOfSight(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2.0f, x2, y2, z2);
        }

        public bool IsInLineOfSight(uint MapID, float x1, float y1, float z1, float x2, float y2, float z2)
        {
            x1 = ValidateMapCoord(x1);
            y1 = ValidateMapCoord(y1);
            z1 = ValidateMapCoord(z1);
            x2 = ValidateMapCoord(x2);
            y2 = ValidateMapCoord(y2);
            z2 = ValidateMapCoord(z2);
            bool result = true;
            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            return result;
        }

        public float GetVMapHeight(uint MapID, float x, float y, float z)
        {
            x = ValidateMapCoord(x);
            y = ValidateMapCoord(y);
            z = ValidateMapCoord(z);
            float height = WorldServiceLocator._Global_Constants.VMAP_INVALID_HEIGHT_VALUE;
            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            return height;
        }

        public bool GetObjectHitPos(ref WS_Base.BaseObject obj, ref WS_Base.BaseObject obj2, ref float rx, ref float ry, ref float rz, float pModifyDist)
        {
            rx = ValidateMapCoord(rx);
            ry = ValidateMapCoord(ry);
            rz = ValidateMapCoord(rz);
            return GetObjectHitPos(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2.0f, obj2.positionX, obj2.positionY, obj2.positionZ + 2.0f, ref rx, ref ry, ref rz, pModifyDist);
        }

        public bool GetObjectHitPos(ref WS_Base.BaseObject obj, float x2, float y2, float z2, ref float rx, ref float ry, ref float rz, float pModifyDist)
        {
            rx = ValidateMapCoord(rx);
            ry = ValidateMapCoord(ry);
            rz = ValidateMapCoord(rz);
            x2 = ValidateMapCoord(x2);
            y2 = ValidateMapCoord(y2);
            z2 = ValidateMapCoord(z2);
            return GetObjectHitPos(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2.0f, x2, y2, z2, ref rx, ref ry, ref rz, pModifyDist);
        }

        public bool GetObjectHitPos(uint MapID, float x1, float y1, float z1, float x2, float y2, float z2, ref float rx, ref float ry, ref float rz, float pModifyDist)
        {
            x1 = ValidateMapCoord(x1);
            y1 = ValidateMapCoord(y1);
            z1 = ValidateMapCoord(z1);
            x2 = ValidateMapCoord(x2);
            y2 = ValidateMapCoord(y2);
            z2 = ValidateMapCoord(z2);
            bool result = false;
            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            return result;
        }

        public void LoadSpawns(byte TileX, byte TileY, uint TileMap, uint TileInstance)
        {
            // Caluclate (x1, y1) and (x2, y2)
            float MinX = (32 - TileX) * WorldServiceLocator._Global_Constants.SIZE;
            float MaxX = (32 - (TileX + 1)) * WorldServiceLocator._Global_Constants.SIZE;
            float MinY = (32 - TileY) * WorldServiceLocator._Global_Constants.SIZE;
            float MaxY = (32 - (TileY + 1)) * WorldServiceLocator._Global_Constants.SIZE;
            // We need the maximum value to be the largest value
            if (MinX > MaxX)
            {
                float tmpSng = MinX;
                MinX = MaxX;
                MaxX = tmpSng;
            }

            if (MinY > MaxY)
            {
                float tmpSng = MinY;
                MinY = MaxY;
                MaxY = tmpSng;
            }

            // DONE: Instance units get a new specific GUID
            ulong InstanceGuidAdd = 0UL;
            if (TileInstance > 0L)
            {
                InstanceGuidAdd = 1000000UL + (TileInstance - 1UL) * 100000UL;
            }

            // DONE: Creatures
            var MysqlQuery = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM creature LEFT OUTER JOIN game_event_creature ON creature.guid = game_event_creature.guid WHERE map={0} AND position_X BETWEEN '{1}' AND '{2}' AND position_Y BETWEEN '{3}' AND '{4}';", (object)TileMap, (object)MinX, (object)MaxX, (object)MinY, (object)MaxY), MysqlQuery);
            foreach (DataRow InfoRow in MysqlQuery.Rows)
            {
                if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey((decimal)Conversions.ToLong(InfoRow["guid"]) + (decimal)InstanceGuidAdd + WorldServiceLocator._Global_Constants.GUID_UNIT))
                {
                    try
                    {
                        var tmpCr = new WS_Creatures.CreatureObject((ulong)(Conversions.ToLong(InfoRow["guid"]) + (decimal)InstanceGuidAdd), ref InfoRow);
                        if (tmpCr.GameEvent == 0)
                        {
                            tmpCr.instance = TileInstance;
                            tmpCr.AddToWorld();
                        }
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating creature [{0}].{1}{2}", InfoRow["id"], Environment.NewLine, ex.ToString());
                    }
                }
            }

            // DONE: Gameobjects
            MysqlQuery.Clear();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM gameobject LEFT OUTER JOIN game_event_gameobject ON gameobject.guid = game_event_gameobject.guid WHERE map={0} AND spawntimesecs>=0 AND position_X BETWEEN '{1}' AND '{2}' AND position_Y BETWEEN '{3}' AND '{4}';", (object)TileMap, (object)MinX, (object)MaxX, (object)MinY, (object)MaxY), MysqlQuery);
            foreach (DataRow InfoRow in MysqlQuery.Rows)
            {
                if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(Conversions.ToULong(InfoRow["guid"]) + InstanceGuidAdd + WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT) && !WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(Conversions.ToULong(InfoRow["guid"]) + InstanceGuidAdd + WorldServiceLocator._Global_Constants.GUID_TRANSPORT))
                {
                    try
                    {
                        var tmpGo = new WS_GameObjects.GameObjectObject(Conversions.ToULong(InfoRow["guid"]) + InstanceGuidAdd, ref InfoRow);
                        if (tmpGo.GameEvent == 0)
                        {
                            tmpGo.instance = TileInstance;
                            tmpGo.AddToWorld();
                        }
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating gameobject [{0}].{1}{2}", InfoRow["id"], Environment.NewLine, ex.ToString());
                    }
                }
            }

            // DONE: Corpses
            MysqlQuery.Clear();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM corpse WHERE map={0} AND instance={5} AND position_x BETWEEN '{1}' AND '{2}' AND position_y BETWEEN '{3}' AND '{4}';", (object)TileMap, (object)MinX, (object)MaxX, (object)MinY, (object)MaxY, (object)TileInstance), MysqlQuery);
            foreach (DataRow InfoRow in MysqlQuery.Rows)
            {
                if (!WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs.ContainsKey(Conversions.ToULong(InfoRow["guid"]) + WorldServiceLocator._Global_Constants.GUID_CORPSE))
                {
                    try
                    {
                        var tmpCorpse = new WS_Corpses.CorpseObject(Conversions.ToULong(InfoRow["guid"]), ref InfoRow) { instance = TileInstance };
                        tmpCorpse.AddToWorld();
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating corpse [{0}].{1}{2}", InfoRow["guid"], Environment.NewLine, ex.ToString());
                    }
                }
            }

            // DONE: Transports
            try
            {
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.AcquireReaderLock(1000);
                foreach (KeyValuePair<ulong, WS_Transports.TransportObject> Transport in WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)
                {
                    try
                    {
                        if (Transport.Value.MapID == TileMap && Transport.Value.positionX >= MinX && Transport.Value.positionX <= MaxX && Transport.Value.positionY >= MinY && Transport.Value.positionY <= MaxY)
                        {
                            if (Maps[TileMap].Tiles[TileX, TileY].GameObjectsHere.Contains(Transport.Value.GUID) == false)
                            {
                                Maps[TileMap].Tiles[TileX, TileY].GameObjectsHere.Add(Transport.Value.GUID);
                            }

                            Transport.Value.NotifyEnter();
                        }
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating transport [{0}].{1}{2}", Transport.Key - WorldServiceLocator._Global_Constants.GUID_MO_TRANSPORT, Environment.NewLine, ex.ToString());
                    }
                }
            }
            catch
            {
            }
            finally
            {
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.ReleaseReaderLock();
            }
        }

        public void UnloadSpawns(byte TileX, byte TileY, uint TileMap)
        {
            // Caluclate (x1, y1) and (x2, y2)
            float MinX = (32 - TileX) * WorldServiceLocator._Global_Constants.SIZE;
            float MaxX = (32 - (TileX + 1)) * WorldServiceLocator._Global_Constants.SIZE;
            float MinY = (32 - TileY) * WorldServiceLocator._Global_Constants.SIZE;
            float MaxY = (32 - (TileY + 1)) * WorldServiceLocator._Global_Constants.SIZE;
            // We need the maximum value to be the largest value
            if (MinX > MaxX)
            {
                float tmpSng = MinX;
                MinX = MaxX;
                MaxX = tmpSng;
            }

            if (MinY > MaxY)
            {
                float tmpSng = MinY;
                MinY = MaxY;
                MaxY = tmpSng;
            }

            try
            {
                WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                foreach (KeyValuePair<ulong, WS_Creatures.CreatureObject> Creature in WorldServiceLocator._WorldServer.WORLD_CREATUREs)
                {
                    if (Creature.Value.MapID == TileMap && Creature.Value.SpawnX >= MinX && Creature.Value.SpawnX <= MaxX && Creature.Value.SpawnY >= MinY && Creature.Value.SpawnY <= MaxY)
                    {
                        Creature.Value.Destroy();
                    }
                }
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, ex.ToString(), default);
            }
            finally
            {
                WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseReaderLock();
            }

            foreach (KeyValuePair<ulong, WS_GameObjects.GameObjectObject> Gameobject in WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)
            {
                if (Gameobject.Value.MapID == TileMap && Gameobject.Value.positionX >= MinX && Gameobject.Value.positionX <= MaxX && Gameobject.Value.positionY >= MinY && Gameobject.Value.positionY <= MaxY)
                {
                    Gameobject.Value.Destroy(Gameobject);
                }
            }

            foreach (KeyValuePair<ulong, WS_Corpses.CorpseObject> Corpseobject in WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs)
            {
                if (Corpseobject.Value.MapID == TileMap && Corpseobject.Value.positionX >= MinX && Corpseobject.Value.positionX <= MaxX && Corpseobject.Value.positionY >= MinY && Corpseobject.Value.positionY <= MaxY)
                {
                    Corpseobject.Value.Destroy();
                }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void SendTransferAborted(ref WS_Network.ClientClass client, int Map, TransferAbortReason Reason)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_TRANSFER_ABORTED [{2}:{3}]", client.IP, client.Port, Map, Reason);
            var p = new Packets.PacketClass(OPCODES.SMSG_TRANSFER_ABORTED);
            try
            {
                p.AddInt32(Map);
                p.AddInt16((short)Reason);
                client.Send(ref p);
            }
            finally
            {
                p.Dispose();
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}