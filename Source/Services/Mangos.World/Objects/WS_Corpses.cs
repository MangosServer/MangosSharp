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
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
    public class WS_Corpses
    {
        // WARNING: Use only with _WorldServer.WORLD_GAMEOBJECTs()
        public class CorpseObject : WS_Base.BaseObject, IDisposable
        {
            public int DynFlags = 0;
            public int Flags = 0;
            public ulong Owner = 0UL;
            public int Bytes1 = 0;
            public int Bytes2 = 0;
            public int Model = 0;
            public int Guild = 0;
            public int[] Items = new int[EquipmentSlots.EQUIPMENT_SLOT_END];

            public void FillAllUpdateFlags(ref Packets.UpdateClass Update)
            {
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, ObjectType.TYPE_CORPSE + ObjectType.TYPE_OBJECT);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_ENTRY, 0);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, 1.0f);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_OWNER, Owner);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_FACING, orientation);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_POS_X, positionX);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_POS_Y, positionY);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_POS_Z, positionZ);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_DISPLAY_ID, Model);
                for (int i = 0, loopTo = EquipmentSlots.EQUIPMENT_SLOT_END - 1; i <= loopTo; i++)
                    Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_ITEM + i, Items[i]);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_BYTES_1, Bytes1);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_BYTES_2, Bytes2);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_GUILD, Guild);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_FLAGS, Flags);
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_DYNAMIC_FLAGS, DynFlags);
            }

            public void ConvertToBones()
            {
                // DONE: Delete from database
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM corpse WHERE player = \"{0}\";", Owner));
                Flags = 5;
                Owner = 0UL;
                for (int i = 0, loopTo = EquipmentSlots.EQUIPMENT_SLOT_END - 1; i <= loopTo; i++)
                    Items[i] = 0;
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                try
                {
                    packet.AddInt32(1);
                    packet.AddInt8(0);
                    var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_CORPSE);
                    try
                    {
                        tmpUpdate.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_OWNER, 0);
                        tmpUpdate.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_FLAGS, 5);
                        for (int i = 0, loopTo1 = EquipmentSlots.EQUIPMENT_SLOT_END - 1; i <= loopTo1; i++)
                            tmpUpdate.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_ITEM + i, 0);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                        SendToNearPlayers(ref packet);
                    }
                    finally
                    {
                        tmpUpdate.Dispose();
                    }
                }
                finally
                {
                    packet.Dispose();
                }
            }

            public void Save()
            {
                // Only for creating New Character
                string tmpCmd = "INSERT INTO corpse (guid";
                string tmpValues = " VALUES (" + (GUID - WorldServiceLocator._Global_Constants.GUID_CORPSE);
                tmpCmd += ", player";
                tmpValues = tmpValues + ", " + Owner;
                tmpCmd += ", position_x";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(positionX));
                tmpCmd += ", position_y";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(positionY));
                tmpCmd += ", position_z";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(positionZ));
                tmpCmd += ", map";
                tmpValues = tmpValues + ", " + MapID;
                tmpCmd += ", instance";
                tmpValues = tmpValues + ", " + instance;
                tmpCmd += ", orientation";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(orientation));
                tmpCmd += ", time";
                tmpValues += ", UNIX_TIMESTAMP()";
                tmpCmd += ", corpse_type";
                tmpValues = tmpValues + ", " + CorpseType;

                // tmpCmd = tmpCmd & ", corpse_bytes1"
                // tmpValues = tmpValues & ", " & Bytes1
                // tmpCmd = tmpCmd & ", corpse_bytes2"
                // tmpValues = tmpValues & ", " & Bytes2
                // tmpCmd = tmpCmd & ", corpse_model"
                // tmpValues = tmpValues & ", " & Model
                // tmpCmd = tmpCmd & ", corpse_guild"
                // tmpValues = tmpValues & ", " & Guild

                // Dim temp(EquipmentSlots.EQUIPMENT_SLOT_END - 1) As String
                // For i As Byte = 0 To EquipmentSlots.EQUIPMENT_SLOT_END - 1
                // temp(i) = Items(i)
                // Next
                // tmpCmd = tmpCmd & ", corpse_items"
                // tmpValues = tmpValues & ", """ & Join(temp, " ") & """"

                tmpCmd = tmpCmd + ") " + tmpValues + ");";
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmpCmd);
            }

            public void Destroy()
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_DESTROY_OBJECT);
                try
                {
                    packet.AddUInt64(GUID);
                    SendToNearPlayers(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }

                Dispose();
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
                    RemoveFromWorld();
                    WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs.Remove(GUID);
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
            public CorpseObject(ref WS_PlayerData.CharacterObject Character)
            {
                // WARNING: Use only for spawning new object
                GUID = WorldServiceLocator._WS_Corpses.GetNewGUID();
                Bytes1 = (Conversions.ToInteger(Character.Race) << 8) + (Conversions.ToInteger(Character.Gender) << 16) + (Conversions.ToInteger(Character.Skin) << 24);
                Bytes2 = Character.Face + (Conversions.ToInteger(Character.HairStyle) << 8) + (Conversions.ToInteger(Character.HairColor) << 16) + (Conversions.ToInteger(Character.FacialHair) << 24);
                Model = Character.Model;
                positionX = Character.positionX;
                positionY = Character.positionY;
                positionZ = Character.positionZ;
                orientation = Character.orientation;
                MapID = Character.MapID;
                Owner = Character.GUID;
                Character.corpseGUID = GUID;
                Character.corpsePositionX = positionX;
                Character.corpsePositionY = positionY;
                Character.corpsePositionZ = positionZ;
                Character.corpseMapID = (int)MapID;

                // TODO: The Corpse Type May Need to be Set Differently (Perhaps using Player Extra Flags)?
                if (Character.isPvP)
                {
                    Character.corpseCorpseType = this.CorpseType.CORPSE_RESURRECTABLE_PVP;
                }
                else
                {
                    Character.corpseCorpseType = this.CorpseType.CORPSE_RESURRECTABLE_PVE;
                }

                Character.corpseCorpseType = CorpseType;
                for (byte i = 0, loopTo = EquipmentSlots.EQUIPMENT_SLOT_END - 1; i <= loopTo; i++)
                {
                    if (Character.Items.ContainsKey(i))
                    {
                        Items[i] = Character.Items[i].ItemInfo.Model + (Conversions.ToInteger(Character.Items[i].ItemInfo.InventoryType) << 24);
                    }
                    else
                    {
                        Items[i] = 0;
                    }
                }

                Flags = 4;
                WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs.Add(GUID, this);
            }

            public CorpseObject(ulong cGUID, [Optional, DefaultParameterValue(null)] ref DataRow Info)
            {
                // WARNING: Use only for loading from DB
                if (Info is null)
                {
                    var MySQLQuery = new DataTable();
                    WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM corpse WHERE guid = {0};", (object)cGUID), MySQLQuery);
                    if (MySQLQuery.Rows.Count > 0)
                    {
                        Info = MySQLQuery.Rows[0];
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Corpse not found in database. [corpseGUID={0:X}]", cGUID);
                        return;
                    }
                }

                positionX = Conversions.ToSingle(Info["position_x"]);
                positionY = Conversions.ToSingle(Info["position_y"]);
                positionZ = Conversions.ToSingle(Info["position_z"]);
                orientation = Conversions.ToSingle(Info["orientation"]);
                MapID = Conversions.ToUInteger(Info["map"]);
                instance = Conversions.ToUInteger(Info["instance"]);
                Owner = Conversions.ToULong(Info["player"]);
                CorpseType = Info["corpse_type"];
                // Bytes1 = Info.Item("corpse_bytes1")
                // Bytes2 = Info.Item("corpse_bytes2")
                // Model = Info.Item("corpse_model")
                // Guild = Info.Item("corpse_guild")

                // Dim tmp() As String
                // tmp = Split(CType(Info.Item("corpse_items"), String), " ")
                // For i As Integer = 0 To tmp.Length - 1
                // Items(i) = tmp(i)
                // Next i

                Flags = 4;
                GUID = cGUID + WorldServiceLocator._Global_Constants.GUID_CORPSE;
                WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs.Add(GUID, this);
            }

            public void AddToWorld()
            {
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
                if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] is null)
                    WorldServiceLocator._WS_CharMovement.MAP_Load(CellX, CellY, MapID);
                WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CorpseObjectsHere.Add(GUID);
                ulong[] list;
                // DONE: Sending to players in nearby cells
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                try
                {
                    var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_CORPSE);
                    try
                    {
                        packet.AddInt32(1);
                        packet.AddInt8(0);
                        FillAllUpdateFlags(ref tmpUpdate);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, this);
                    }
                    finally
                    {
                        tmpUpdate.Dispose();
                    }

                    for (short i = -1; i <= 1; i++)
                    {
                        for (short j = -1; j <= 1; j++)
                        {
                            if (CellX + i >= 0 && CellX + i <= 63 && CellY + j >= 0 && CellY + j <= 63 && WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX + i, CellY + j] is object && WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX + i, CellY + j].PlayersHere.Count > 0)
                            {
                                {
                                    var withBlock = WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX + i, CellY + j];
                                    list = withBlock.PlayersHere.ToArray();
                                    foreach (ulong plGUID in list)
                                    {
                                        WS_Base.BaseObject argobjCharacter = this;
                                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(plGUID) && WorldServiceLocator._WorldServer.CHARACTERs[plGUID].CanSee(ref argobjCharacter))
                                        {
                                            WorldServiceLocator._WorldServer.CHARACTERs[plGUID].client.SendMultiplyPackets(ref packet);
                                            WorldServiceLocator._WorldServer.CHARACTERs[plGUID].corpseObjectsNear.Add(GUID);
                                            SeenBy.Add(plGUID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    packet.Dispose();
                }
            }

            public void RemoveFromWorld()
            {
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
                WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CorpseObjectsHere.Remove(GUID);
                ulong[] list;

                // DONE: Removing from players in <CENTER> Cell wich can see it
                if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].PlayersHere.Count > 0)
                {
                    {
                        var withBlock = WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY];
                        list = withBlock.PlayersHere.ToArray();
                        foreach (ulong plGUID in list)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs[plGUID].corpseObjectsNear.Contains(GUID))
                            {
                                WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                                WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving.Add(GUID);
                                WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.ReleaseWriterLock();
                                WorldServiceLocator._WorldServer.CHARACTERs[plGUID].corpseObjectsNear.Remove(GUID);
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private ulong GetNewGUID()
        {
            ulong GetNewGUIDRet = default;
            WorldServiceLocator._WorldServer.CorpseGUIDCounter = (ulong)(WorldServiceLocator._WorldServer.CorpseGUIDCounter + 1m);
            GetNewGUIDRet = WorldServiceLocator._WorldServer.CorpseGUIDCounter;
            return GetNewGUIDRet;
        }
    }
}