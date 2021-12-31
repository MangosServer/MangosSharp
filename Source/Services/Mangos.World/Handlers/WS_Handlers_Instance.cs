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
using Mangos.World.Maps;
using Mangos.World.Network;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Data;
using System.IO;

namespace Mangos.World.Handlers;

public class WS_Handlers_Instance
{
    public void InstanceMapUpdate()
    {
        DataTable q = new();
        var TimeStamp = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
        WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_instances WHERE expire < {TimeStamp};", ref q);
        IEnumerator enumerator = default;
        try
        {
            enumerator = q.Rows.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DataRow row = (DataRow)enumerator.Current;
                if (WorldServiceLocator._WS_Maps.Maps.ContainsKey(row.As<uint>("map")))
                {
                    InstanceMapExpire(row.As<uint>("map"), row.As<uint>("instance"));
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
    }

    public uint InstanceMapCreate(uint Map)
    {
        DataTable q = new();
        WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT MAX(instance) FROM characters_instances WHERE map = {Map};", ref q);
        return q.Rows[0][0] != DBNull.Value ? (uint)(q.Rows[0].As<int>(0) + 1) : 0u;
    }

    public void InstanceMapSpawn(uint Map, uint Instance)
    {
        checked
        {
            short x = 0;
            do
            {
                short y = 0;
                do
                {
                    if (!WorldServiceLocator._WS_Maps.Maps[Map].TileUsed[x, y] && File.Exists(string.Format("maps\\{0}{1}{2}.map", Strings.Format(Map, "000"), Strings.Format(x, "00"), Strings.Format(y, "00"))))
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading map [{2}: {0},{1}]...", x, y, Map);
                        WorldServiceLocator._WS_Maps.Maps[Map].TileUsed[x, y] = true;
                        WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y] = new WS_Maps.TMapTile((byte)x, (byte)y, Map);
                    }
                    if (WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y] != null)
                    {
                        WorldServiceLocator._WS_Maps.LoadSpawns((byte)x, (byte)y, Map, Instance);
                    }
                    y = (short)unchecked(y + 1);
                }
                while (y <= 63);
                x = (short)unchecked(x + 1);
            }
            while (x <= 63);
        }
    }

    public void InstanceMapExpire(uint Map, uint Instance)
    {
        checked
        {
            try
            {
                short x = 0;
                var empty = true;
                do
                {
                    short y = 0;
                    do
                    {
                        if (WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y] != null)
                        {
                            foreach (var GUID7 in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y].PlayersHere.ToArray())
                            {
                                if (WorldServiceLocator._WorldServer.CHARACTERs[GUID7].instance == Instance)
                                {
                                    empty = false;
                                    break;
                                }
                            }
                        }
                        if (!empty)
                        {
                            break;
                        }
                        y = (short)unchecked(y + 1);
                    }
                    while (y <= 63);
                    if (!empty)
                    {
                        break;
                    }
                    x = (short)unchecked(x + 1);
                }
                while (x <= 63);
                if (empty)
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM characters_instances WHERE instance = {Instance} AND map = {Map};");
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM characters_instances_group WHERE instance = {Instance} AND map = {Map};");
                    short x3 = 0;
                    do
                    {
                        short y3 = 0;
                        do
                        {
                            if (WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x3, y3] != null)
                            {
                                foreach (var GUID3 in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x3, y3].CreaturesHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID3].instance == Instance)
                                    {
                                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID3].Destroy();
                                    }
                                }
                                foreach (var GUID4 in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x3, y3].GameObjectsHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID4].instance == Instance)
                                    {
                                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID4].Destroy(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID4]);
                                    }
                                }
                                foreach (var GUID5 in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x3, y3].CorpseObjectsHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID5].instance == Instance)
                                    {
                                        WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID5].Destroy();
                                    }
                                }
                                foreach (var GUID6 in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x3, y3].DynamicObjectsHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID6].instance == Instance)
                                    {
                                        WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID6].Delete();
                                    }
                                }
                            }
                            y3 = (short)unchecked(y3 + 1);
                        }
                        while (y3 <= 63);
                        x3 = (short)unchecked(x3 + 1);
                    }
                    while (x3 <= 63);
                    return;
                }
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_instances SET expire = {2} WHERE instance = {0} AND map = {1};", Instance, Map, WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + WorldServiceLocator._WS_Maps.Maps[Map].ResetTime));
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_instances_group SET expire = {2} WHERE instance = {0} AND map = {1};", Instance, Map, WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + WorldServiceLocator._WS_Maps.Maps[Map].ResetTime));
                short x2 = 0;
                do
                {
                    short y2 = 0;
                    do
                    {
                        if (WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x2, y2] != null)
                        {
                            foreach (var GUID in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x2, y2].CreaturesHere.ToArray())
                            {
                                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].instance == Instance)
                                {
                                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Respawn();
                                }
                            }
                            foreach (var GUID2 in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x2, y2].GameObjectsHere.ToArray())
                            {
                                if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID2].instance == Instance)
                                {
                                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID2].Respawn(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID2]);
                                }
                            }
                        }
                        y2 = (short)unchecked(y2 + 1);
                    }
                    while (y2 <= 63);
                    x2 = (short)unchecked(x2 + 1);
                }
                while (x2 <= 63);
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error expiring map instance.{0}{1}", Environment.NewLine, ex.ToString());
            }
        }
    }

    public void InstanceMapEnter(WS_PlayerData.CharacterObject objCharacter)
    {
        if (WorldServiceLocator._WS_Maps.Maps[objCharacter.MapID].Type == MapTypes.MAP_COMMON)
        {
            objCharacter.instance = 0u;
            objCharacter.SystemMessage(WorldServiceLocator._Functions.SetColor("You are not in instance.", 200, 0, byte.MaxValue));
            return;
        }
        InstanceMapUpdate();
        DataTable q = new();
        WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_instances WHERE char_guid = {objCharacter.GUID} AND map = {objCharacter.MapID};", ref q);
        if (q.Rows.Count > 0)
        {
            objCharacter.instance = Conversions.ToUInteger(q.Rows[0]["instance"]);
            objCharacter.SystemMessage(WorldServiceLocator._Functions.SetColor($"You are in instance #{objCharacter.instance}, map {objCharacter.MapID}", 0, 200, byte.MaxValue));
            SendInstanceMessage(ref objCharacter.client, objCharacter.MapID, Conversions.ToInteger(Operators.SubtractObject(q.Rows[0]["expire"], WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now))));
            return;
        }
        if (objCharacter.IsInGroup)
        {
            WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_instances_group WHERE group_id = {objCharacter.Group.ID} AND map = {objCharacter.MapID};", ref q);
            if (q.Rows.Count > 0)
            {
                objCharacter.instance = Conversions.ToUInteger(q.Rows[0]["instance"]);
                objCharacter.SystemMessage(WorldServiceLocator._Functions.SetColor($"You are in instance #{objCharacter.instance}, map {objCharacter.MapID}", 245, 245, byte.MaxValue));
                SendInstanceMessage(ref objCharacter.client, objCharacter.MapID, Conversions.ToInteger(Operators.SubtractObject(q.Rows[0]["expire"], WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now))));
                return;
            }
        }
        checked
        {
            var instanceNewID = (int)InstanceMapCreate(objCharacter.MapID);
            var instanceNewResetTime = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + WorldServiceLocator._WS_Maps.Maps[objCharacter.MapID].ResetTime);
            objCharacter.instance = (uint)instanceNewID;
            if (objCharacter.IsInGroup)
            {
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"INSERT INTO characters_instances_group (group_id, map, instance, expire) VALUES ({objCharacter.Group.ID}, {objCharacter.MapID}, {instanceNewID}, {instanceNewResetTime});");
            }
            InstanceMapSpawn(objCharacter.MapID, (uint)instanceNewID);
            objCharacter.SystemMessage(WorldServiceLocator._Functions.SetColor($"You are in instance #{objCharacter.instance}, map {objCharacter.MapID}", 245, 245, byte.MaxValue));
            SendInstanceMessage(ref objCharacter.client, objCharacter.MapID, (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) - instanceNewResetTime));
        }
    }

    public void InstanceUpdate(uint Map, uint Instance, uint Cleared)
    {
    }

    public void InstanceMapLeave(WS_PlayerData.CharacterObject objChar)
    {
    }

    public void SendResetInstanceSuccess(ref WS_Network.ClientClass client, uint Map)
    {
    }

    public void SendResetInstanceFailed(ref WS_Network.ClientClass client, uint Map, ResetFailedReason Reason)
    {
    }

    public void SendResetInstanceFailedNotify(ref WS_Network.ClientClass client, uint Map)
    {
    }

    private void SendUpdateInstanceOwnership(ref WS_Network.ClientClass client, uint Saved)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_UPDATE_INSTANCE_OWNERSHIP", client.IP, client.Port);
    }

    private void SendUpdateLastInstance(ref WS_Network.ClientClass client, uint Map)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_UPDATE_LAST_INSTANCE", client.IP, client.Port);
    }

    public void SendInstanceSaved(WS_PlayerData.CharacterObject Character)
    {
        DataTable q = new();
        WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_instances WHERE char_guid = {Character.GUID};", ref q);
        SendUpdateInstanceOwnership(ref Character.client, 0u - ((q.Rows.Count > 0) ? 1u : 0u));
        IEnumerator enumerator = default;
        try
        {
            enumerator = q.Rows.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DataRow row = (DataRow)enumerator.Current;
                SendUpdateLastInstance(ref Character.client, row.As<uint>("map"));
            }
        }
        finally
        {
            if (enumerator is IDisposable)
            {
                (enumerator as IDisposable).Dispose();
            }
        }
    }

    public void SendInstanceMessage(ref WS_Network.ClientClass client, uint Map, int Time)
    {
        if (Time < 0)
        {
            _ = checked(-Time); // Time Discard (Unused)
        }
        else if (Time is <= 60 or >= 3600)
        {
            switch (Time)
            {
                default:
                    break;
            }
        }
    }
}
