//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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
using Mangos.Common.Globals;
using Mangos.DataStores;
using Mangos.MySql;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Mangos.World.Maps;

public partial class WS_Maps
{
    private readonly DataStoreProvider dataStoreProvider;

    public Dictionary<int, TArea> AreaTable;

    public Dictionary<uint, TMap> Maps;

    public string MapList;

    /// <summary>
    /// VMap manager for line-of-sight, indoor detection, and model height queries.
    /// </summary>
    public VMapManager VMapManager;

    /// <summary>
    /// MMap manager for server-side pathfinding using Detour navmeshes.
    /// </summary>
    public MMapManager MMapManager;

    private const float SIZE_OF_GRIDS = 533.33333f;
    private const float INVALID_HEIGHT = -200000.0f;

    public WS_Maps(DataStoreProvider dataStoreProvider)
    {
        this.dataStoreProvider = dataStoreProvider;
        AreaTable = new Dictionary<int, TArea>();
        Maps = new Dictionary<uint, TMap>();
    }

    public int GetAreaIDByMapandParent(int mapId, int parentID)
    {
        foreach (var thisArea in AreaTable)
        {
            var thisMap = thisArea.Value.mapId;
            var thisParent = thisArea.Value.Zone;
            if (thisMap == mapId && thisParent == parentID)
            {
                return thisArea.Key;
            }
        }
        return -999;
    }

    public async Task InitializeMapsAsync()
    {
        var e = WorldServiceLocator.MangosConfiguration.World.Maps.GetEnumerator();
        if (e.MoveNext())
        {
            MapList = Conversions.ToString(e.Current);
            while (e.MoveNext())
            {
                MapList = Conversions.ToString(Operators.AddObject(MapList, Operators.ConcatenateObject(", ", e.Current)));
            }
        }
        foreach (var map2 in WorldServiceLocator.MangosConfiguration.World.Maps)
        {
            var id = Conversions.ToUInteger(map2);
            TMap map = new(checked((int)id), await dataStoreProvider.GetDataStoreAsync("Map.dbc"));
        }
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.INFORMATION, "Initalizing: {0} Maps initialized.", Maps.Count);

        // Initialize VMap system
        VMapManager = new VMapManager();
        VMapManager.Initialize("vmaps");
        if (WorldServiceLocator.MangosConfiguration.World.VMapsEnabled)
        {
            foreach (var map in Maps)
            {
                VMapManager.LoadMap(map.Key);
            }
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.INFORMATION, "VMap: Loaded VMap data for {0} maps.", Maps.Count);
        }
        else
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.INFORMATION, "VMap: VMaps disabled in configuration.");
        }

        // Initialize MMap system
        MMapManager = new MMapManager();
        MMapManager.Initialize("mmaps");
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.INFORMATION, "MMap: Pathfinding system initialized.");
    }

    public float ValidateMapCoord(float coord)
    {
        if (coord > 32f * SIZE_OF_GRIDS)
        {
            coord = 32f * SIZE_OF_GRIDS;
        }
        else if (coord < -32f * SIZE_OF_GRIDS)
        {
            coord = -32f * SIZE_OF_GRIDS;
        }
        return coord;
    }

    public void GetMapTile(float x, float y, ref byte MapTileX, ref byte MapTileY)
    {
        checked
        {
            MapTileX = (byte)(32f - (ValidateMapCoord(x) / SIZE_OF_GRIDS));
            MapTileY = (byte)(32f - (ValidateMapCoord(y) / SIZE_OF_GRIDS));
        }
    }

    public byte GetMapTileX(float x)
    {
        return checked((byte)(32f - (ValidateMapCoord(x) / SIZE_OF_GRIDS)));
    }

    public byte GetMapTileY(float y)
    {
        return checked((byte)(32f - (ValidateMapCoord(y) / SIZE_OF_GRIDS)));
    }

    /// <summary>
    /// Gets the fractional position within a map tile (0.0 to 1.0).
    /// Used to determine which adjacent tiles need to be loaded for visibility.
    /// </summary>
    public float GetSubTileFraction(float coord)
    {
        coord = ValidateMapCoord(coord);
        float tilePos = 32f - (coord / SIZE_OF_GRIDS);
        return tilePos - (float)Math.Floor(tilePos);
    }

    /// <summary>
    /// Gets the ground height at the given world coordinates using the GridMap system.
    /// Uses MangosZero's triangle interpolation for accurate height values.
    /// </summary>
    public float GetZCoord(float x, float y, uint Map)
    {
        try
        {
            x = ValidateMapCoord(x);
            y = ValidateMapCoord(y);
            var MapTileX = checked((byte)(32f - (x / SIZE_OF_GRIDS)));
            var MapTileY = checked((byte)(32f - (y / SIZE_OF_GRIDS)));

            if (!Maps.ContainsKey(Map) || Maps[Map].Tiles[MapTileX, MapTileY] == null)
            {
                return 0f;
            }

            var tile = Maps[Map].Tiles[MapTileX, MapTileY];
            if (tile.GridMapData != null)
            {
                return tile.GridMapData.GetHeight(x, y);
            }

            return 0f;
        }
        catch (Exception ex)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "GetZCoord exception: X={0} Y={1} Map={2}: {3}", x, y, Map, ex.Message);
            return 0f;
        }
    }

    /// <summary>
    /// Gets the ground height at the given world coordinates, using VMap as a fallback
    /// when the grid map height deviates significantly from the expected Z position.
    /// </summary>
    public float GetZCoord(float x, float y, float z, uint Map)
    {
        try
        {
            x = ValidateMapCoord(x);
            y = ValidateMapCoord(y);
            var MapTileX = checked((byte)(32f - (x / SIZE_OF_GRIDS)));
            var MapTileY = checked((byte)(32f - (y / SIZE_OF_GRIDS)));

            if (!Maps.ContainsKey(Map) || Maps[Map].Tiles[MapTileX, MapTileY] == null)
            {
                // No grid tile - try VMap
                var vmapHeight = GetVMapHeight(Map, x, y, z + 5f);
                return vmapHeight != INVALID_HEIGHT ? vmapHeight : 0f;
            }

            var tile = Maps[Map].Tiles[MapTileX, MapTileY];
            if (tile.GridMapData != null)
            {
                var mapHeight = tile.GridMapData.GetHeight(x, y);

                // If map height deviates significantly from current Z, try VMap
                if (Math.Abs(mapHeight - z) >= 2f)
                {
                    var vmapHeight = GetVMapHeight(Map, x, y, z + 5f);
                    if (vmapHeight != INVALID_HEIGHT)
                    {
                        return vmapHeight;
                    }
                }
                return mapHeight;
            }

            // Fallback to VMap only
            var fallbackHeight = GetVMapHeight(Map, x, y, z + 5f);
            return fallbackHeight != INVALID_HEIGHT ? fallbackHeight : z;
        }
        catch (Exception ex)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "GetZCoord exception: X={0} Y={1} Z={2} Map={3}: {4}", x, y, z, Map, ex.Message);
            return z;
        }
    }

    /// <summary>
    /// Gets the water/liquid surface level at the given world coordinates.
    /// </summary>
    public float GetWaterLevel(float x, float y, int Map)
    {
        x = ValidateMapCoord(x);
        y = ValidateMapCoord(y);
        var MapTileX = checked((byte)(32f - (x / SIZE_OF_GRIDS)));
        var MapTileY = checked((byte)(32f - (y / SIZE_OF_GRIDS)));

        if (!Maps.ContainsKey((uint)Map) || Maps[(uint)Map].Tiles[MapTileX, MapTileY] == null)
        {
            return 0f;
        }

        var tile = Maps[(uint)Map].Tiles[MapTileX, MapTileY];
        if (tile.GridMapData != null)
        {
            return tile.GridMapData.GetLiquidLevel(x, y);
        }

        return 0f;
    }

    /// <summary>
    /// Gets the terrain type (liquid flags) at the given world coordinates.
    /// </summary>
    public byte GetTerrainType(float x, float y, int Map)
    {
        x = ValidateMapCoord(x);
        y = ValidateMapCoord(y);
        var MapTileX = checked((byte)(32f - (x / SIZE_OF_GRIDS)));
        var MapTileY = checked((byte)(32f - (y / SIZE_OF_GRIDS)));

        if (!Maps.ContainsKey((uint)Map) || Maps[(uint)Map].Tiles[MapTileX, MapTileY] == null)
        {
            return 0;
        }

        var tile = Maps[(uint)Map].Tiles[MapTileX, MapTileY];
        if (tile.GridMapData != null)
        {
            return tile.GridMapData.GetTerrainType(x, y);
        }

        return 0;
    }

    /// <summary>
    /// Gets the area flag at the given world coordinates.
    /// </summary>
    public int GetAreaFlag(float x, float y, int Map)
    {
        x = ValidateMapCoord(x);
        y = ValidateMapCoord(y);
        var MapTileX = checked((byte)(32f - (x / SIZE_OF_GRIDS)));
        var MapTileY = checked((byte)(32f - (y / SIZE_OF_GRIDS)));

        if (!Maps.ContainsKey((uint)Map) || Maps[(uint)Map].Tiles[MapTileX, MapTileY] == null)
        {
            return 0;
        }

        var tile = Maps[(uint)Map].Tiles[MapTileX, MapTileY];
        if (tile.GridMapData != null)
        {
            return tile.GridMapData.GetArea(x, y);
        }

        return 0;
    }

    /// <summary>
    /// Gets the liquid status at the given world coordinates.
    /// Returns detailed liquid information including type, level, and depth.
    /// </summary>
    public uint GetLiquidStatus(float x, float y, float z, int Map, byte reqLiquidType, out GridMap.LiquidData data)
    {
        data = default;
        x = ValidateMapCoord(x);
        y = ValidateMapCoord(y);
        var MapTileX = checked((byte)(32f - (x / SIZE_OF_GRIDS)));
        var MapTileY = checked((byte)(32f - (y / SIZE_OF_GRIDS)));

        if (!Maps.ContainsKey((uint)Map) || Maps[(uint)Map].Tiles[MapTileX, MapTileY] == null)
        {
            return GridMap.LIQUID_MAP_NO_WATER;
        }

        var tile = Maps[(uint)Map].Tiles[MapTileX, MapTileY];
        if (tile.GridMapData != null)
        {
            return tile.GridMapData.GetLiquidStatus(x, y, z, reqLiquidType, out data);
        }

        return GridMap.LIQUID_MAP_NO_WATER;
    }

    public bool IsOutsideOfMap(ref WS_Base.BaseObject objCharacter)
    {
        return false;
    }

    /// <summary>
    /// Checks line-of-sight between two objects using VMap ray-casting.
    /// </summary>
    public bool IsInLineOfSight(ref WS_Base.BaseObject obj, ref WS_Base.BaseObject obj2)
    {
        return IsInLineOfSight(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2f, obj2.positionX, obj2.positionY, obj2.positionZ + 2f);
    }

    /// <summary>
    /// Checks line-of-sight between an object and a point using VMap ray-casting.
    /// </summary>
    public bool IsInLineOfSight(ref WS_Base.BaseObject obj, float x2, float y2, float z2)
    {
        x2 = ValidateMapCoord(x2);
        y2 = ValidateMapCoord(y2);
        z2 = ValidateMapCoord(z2);
        return IsInLineOfSight(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2f, x2, y2, z2);
    }

    /// <summary>
    /// Checks line-of-sight between two points using VMap ray-casting.
    /// Returns true if there is a clear line of sight (no obstruction).
    /// </summary>
    public bool IsInLineOfSight(uint MapID, float x1, float y1, float z1, float x2, float y2, float z2)
    {
        if (!WorldServiceLocator.MangosConfiguration.World.LineOfSightEnabled)
        {
            return true;
        }

        x1 = ValidateMapCoord(x1);
        y1 = ValidateMapCoord(y1);
        z1 = ValidateMapCoord(z1);
        x2 = ValidateMapCoord(x2);
        y2 = ValidateMapCoord(y2);
        z2 = ValidateMapCoord(z2);

        if (VMapManager != null)
        {
            return VMapManager.IsInLineOfSight(MapID, x1, y1, z1, x2, y2, z2);
        }

        return true;
    }

    /// <summary>
    /// Gets the VMap model height at the given world coordinates.
    /// This accounts for indoor areas and multi-floor buildings.
    /// </summary>
    public float GetVMapHeight(uint MapID, float x, float y, float z)
    {
        if (!WorldServiceLocator.MangosConfiguration.World.HeightCalcEnabled)
        {
            return INVALID_HEIGHT;
        }

        x = ValidateMapCoord(x);
        y = ValidateMapCoord(y);
        z = ValidateMapCoord(z);

        if (VMapManager != null)
        {
            return VMapManager.GetHeight(MapID, x, y, z);
        }

        return INVALID_HEIGHT;
    }

    /// <summary>
    /// Finds the hit position along a ray between two objects.
    /// Returns true if there is a collision, with the hit position in rx/ry/rz.
    /// </summary>
    public bool GetObjectHitPos(ref WS_Base.BaseObject obj, ref WS_Base.BaseObject obj2, ref float rx, ref float ry, ref float rz, float pModifyDist)
    {
        return GetObjectHitPos(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2f, obj2.positionX, obj2.positionY, obj2.positionZ + 2f, ref rx, ref ry, ref rz, pModifyDist);
    }

    /// <summary>
    /// Finds the hit position along a ray from an object to a point.
    /// Returns true if there is a collision, with the hit position in rx/ry/rz.
    /// </summary>
    public bool GetObjectHitPos(ref WS_Base.BaseObject obj, float x2, float y2, float z2, ref float rx, ref float ry, ref float rz, float pModifyDist)
    {
        x2 = ValidateMapCoord(x2);
        y2 = ValidateMapCoord(y2);
        z2 = ValidateMapCoord(z2);
        return GetObjectHitPos(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2f, x2, y2, z2, ref rx, ref ry, ref rz, pModifyDist);
    }

    /// <summary>
    /// Finds the hit position along a ray between two points using VMap data.
    /// Returns true if there is a collision, with the hit position in rx/ry/rz.
    /// </summary>
    public bool GetObjectHitPos(uint MapID, float x1, float y1, float z1, float x2, float y2, float z2, ref float rx, ref float ry, ref float rz, float pModifyDist)
    {
        x1 = ValidateMapCoord(x1);
        y1 = ValidateMapCoord(y1);
        z1 = ValidateMapCoord(z1);
        x2 = ValidateMapCoord(x2);
        y2 = ValidateMapCoord(y2);
        z2 = ValidateMapCoord(z2);

        if (VMapManager != null && WorldServiceLocator.MangosConfiguration.World.VMapsEnabled)
        {
            return VMapManager.GetObjectHitPos(MapID, x1, y1, z1, x2, y2, z2, ref rx, ref ry, ref rz, pModifyDist);
        }

        // No collision - ray passes through completely
        rx = x2;
        ry = y2;
        rz = z2;
        return false;
    }

    public void LoadSpawns(byte TileX, byte TileY, uint TileMap, uint TileInstance)
    {
        checked
        {
            var MinX = (32 - TileX) * SIZE_OF_GRIDS;
            var MaxX = (32 - (TileX + 1)) * SIZE_OF_GRIDS;
            var MinY = (32 - TileY) * SIZE_OF_GRIDS;
            var MaxY = (32 - (TileY + 1)) * SIZE_OF_GRIDS;
            if (MinX > MaxX)
            {
                var tmpSng2 = MinX;
                MinX = MaxX;
                MaxX = tmpSng2;
            }
            if (MinY > MaxY)
            {
                var tmpSng = MinY;
                MinY = MaxY;
                MaxY = tmpSng;
            }
            var InstanceGuidAdd = 0uL;
            if (TileInstance > 0L)
            {
                InstanceGuidAdd = (ulong)(1000000L + (TileInstance - 1L) * 100000L);
            }
            DataTable MysqlQuery = new();
            WorldServiceLocator.WorldServer.WorldDatabase.Query($"SELECT * FROM creature LEFT OUTER JOIN game_event_creature ON creature.guid = game_event_creature.guid WHERE map={TileMap} AND position_X BETWEEN '{MinX}' AND '{MaxX}' AND position_Y BETWEEN '{MinY}' AND '{MaxY}';", ref MysqlQuery);
            IEnumerator enumerator = default;
            try
            {
                enumerator = MysqlQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    if (WorldServiceLocator.WorldServer.WORLD_CREATUREs.ContainsKey((ulong)row.As<long>("guid") + InstanceGuidAdd + WorldServiceLocator.GlobalConstants.GUID_UNIT))
                    {
                        continue;
                    }
                    try
                    {
                        WS_Creatures.CreatureObject tmpCr = new((ulong)row.As<long>("guid") + InstanceGuidAdd, row);
                        if (tmpCr.GameEvent == 0)
                        {
                            tmpCr.instance = TileInstance;
                            tmpCr.AddToWorld();
                        }
                    }
                    catch (Exception ex4)
                    {
                        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating creature [{0}].{1}{2}", row["id"], Environment.NewLine, ex4.ToString());
                    }
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            MysqlQuery.Clear();
            WorldServiceLocator.WorldServer.WorldDatabase.Query($"SELECT * FROM gameobject LEFT OUTER JOIN game_event_gameobject ON gameobject.guid = game_event_gameobject.guid WHERE map={TileMap} AND spawntimesecs>=0 AND position_X BETWEEN '{MinX}' AND '{MaxX}' AND position_Y BETWEEN '{MinY}' AND '{MaxY}';", ref MysqlQuery);
            IEnumerator enumerator2 = default;
            try
            {
                enumerator2 = MysqlQuery.Rows.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    DataRow row = (DataRow)enumerator2.Current;
                    if (WorldServiceLocator.WorldServer.WORLD_GAMEOBJECTs.ContainsKey(row.As<ulong>("guid") + InstanceGuidAdd + WorldServiceLocator.GlobalConstants.GUID_GAMEOBJECT) || WorldServiceLocator.WorldServer.WORLD_GAMEOBJECTs.ContainsKey(row.As<ulong>("guid") + InstanceGuidAdd + WorldServiceLocator.GlobalConstants.GUID_TRANSPORT))
                    {
                        continue;
                    }
                    try
                    {
                        WS_GameObjects.GameObject tmpGo = new(row.As<ulong>("guid") + InstanceGuidAdd, row);
                        if (tmpGo.GameEvent == 0)
                        {
                            tmpGo.instance = TileInstance;
                            tmpGo.AddToWorld();
                        }
                    }
                    catch (Exception ex3)
                    {
                        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating gameobject [{0}].{1}{2}", row["id"], Environment.NewLine, ex3.ToString());
                    }
                }
            }
            finally
            {
                if (enumerator2 is IDisposable)
                {
                    (enumerator2 as IDisposable).Dispose();
                }
            }
            MysqlQuery.Clear();
            WorldServiceLocator.WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM corpse WHERE map={0} AND instance={5} AND position_x BETWEEN '{1}' AND '{2}' AND position_y BETWEEN '{3}' AND '{4}';", TileMap, MinX, MaxX, MinY, MaxY, TileInstance), ref MysqlQuery);
            IEnumerator enumerator3 = default;
            try
            {
                enumerator3 = MysqlQuery.Rows.GetEnumerator();
                while (enumerator3.MoveNext())
                {
                    DataRow InfoRow = (DataRow)enumerator3.Current;
                    if (!WorldServiceLocator.WorldServer.WORLD_CORPSEOBJECTs.ContainsKey(Conversions.ToULong(InfoRow["guid"]) + WorldServiceLocator.GlobalConstants.GUID_CORPSE))
                    {
                        try
                        {
                            WS_Corpses.CorpseObject tmpCorpse = new(Conversions.ToULong(InfoRow["guid"]), InfoRow)
                            {
                                instance = TileInstance
                            };
                            tmpCorpse.AddToWorld();
                        }
                        catch (Exception ex7)
                        {
                            ProjectData.SetProjectError(ex7);
                            var ex2 = ex7;
                            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating corpse [{0}].{1}{2}", InfoRow["guid"], Environment.NewLine, ex2.ToString());
                            ProjectData.ClearProjectError();
                        }
                    }
                }
            }
            finally
            {
                if (enumerator3 is IDisposable)
                {
                    (enumerator3 as IDisposable).Dispose();
                }
            }
            try
            {
                WorldServiceLocator.WorldServer.WORLD_TRANSPORTs_Lock.EnterReadLock();
                foreach (var Transport in WorldServiceLocator.WorldServer.WORLD_TRANSPORTs)
                {
                    try
                    {
                        if (Transport.Value.MapID == TileMap && Transport.Value.positionX >= MinX && Transport.Value.positionX <= MaxX && Transport.Value.positionY >= MinY && Transport.Value.positionY <= MaxY)
                        {
                            if (!Maps[TileMap].Tiles[TileX, TileY].GameObjectsHere.Contains(Transport.Value.GUID))
                            {
                                Maps[TileMap].Tiles[TileX, TileY].GameObjectsHere.Add(Transport.Value.GUID);
                            }
                            Transport.Value.NotifyEnter();
                        }
                    }
                    catch (Exception ex8)
                    {
                        ProjectData.SetProjectError(ex8);
                        var ex = ex8;
                        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating transport [{0}].{1}{2}", Transport.Key - WorldServiceLocator.GlobalConstants.GUID_MO_TRANSPORT, Environment.NewLine, ex.ToString());
                        ProjectData.ClearProjectError();
                    }
                }
            }
            catch (Exception projectError)
            {
                ProjectData.SetProjectError(projectError);
                ProjectData.ClearProjectError();
            }
            finally
            {
                WorldServiceLocator.WorldServer.WORLD_TRANSPORTs_Lock.ExitReadLock();
            }
        }
    }

    public void UnloadSpawns(byte TileX, byte TileY, uint TileMap)
    {
        checked
        {
            var MinX = (32 - TileX) * SIZE_OF_GRIDS;
            var MaxX = (32 - (TileX + 1)) * SIZE_OF_GRIDS;
            var MinY = (32 - TileY) * SIZE_OF_GRIDS;
            var MaxY = (32 - (TileY + 1)) * SIZE_OF_GRIDS;
            if (MinX > MaxX)
            {
                var tmpSng2 = MinX;
                MinX = MaxX;
                MaxX = tmpSng2;
            }
            if (MinY > MaxY)
            {
                var tmpSng = MinY;
                MinY = MaxY;
                MaxY = tmpSng;
            }
            try
            {
                WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.EnterReadLock();
                foreach (var Creature in WorldServiceLocator.WorldServer.WORLD_CREATUREs)
                {
                    if (Creature.Value.MapID == TileMap && Creature.Value.SpawnX >= MinX && Creature.Value.SpawnX <= MaxX && Creature.Value.SpawnY >= MinY && Creature.Value.SpawnY <= MaxY)
                    {
                        Creature.Value.Destroy();
                    }
                }
            }
            catch (Exception ex2)
            {
                ProjectData.SetProjectError(ex2);
                var ex = ex2;
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, ex.ToString(), null);
                ProjectData.ClearProjectError();
            }
            finally
            {
                WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.ExitReadLock();
            }
            foreach (var Gameobject in WorldServiceLocator.WorldServer.WORLD_GAMEOBJECTs)
            {
                if (Gameobject.Value.MapID == TileMap && Gameobject.Value.positionX >= MinX && Gameobject.Value.positionX <= MaxX && Gameobject.Value.positionY >= MinY && Gameobject.Value.positionY <= MaxY)
                {
                    Gameobject.Value.Destroy(Gameobject);
                }
            }
            foreach (var Corpseobject in WorldServiceLocator.WorldServer.WORLD_CORPSEOBJECTs)
            {
                if (Corpseobject.Value.MapID == TileMap && Corpseobject.Value.positionX >= MinX && Corpseobject.Value.positionX <= MaxX && Corpseobject.Value.positionY >= MinY && Corpseobject.Value.positionY <= MaxY)
                {
                    Corpseobject.Value.Destroy();
                }
            }
        }
    }

    public void SendTransferAborted(ref WS_Network.ClientClass client, int Map, TransferAbortReason Reason)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_TRANSFER_ABORTED [{2}:{3}]", client.IP, client.Port, Map, Reason);
        Packets.PacketClass p = new(Opcodes.SMSG_TRANSFER_ABORTED);
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
}
