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

using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.World.Loots;

public partial class WS_Loot
{
    public LootStore LootTemplates_Creature;

    public LootStore LootTemplates_Disenchant;

    public LootStore LootTemplates_Fishing;

    public LootStore LootTemplates_Gameobject;

    public LootStore LootTemplates_Item;

    public LootStore LootTemplates_Pickpocketing;

    public LootStore LootTemplates_QuestMail;

    public LootStore LootTemplates_Reference;

    public LootStore LootTemplates_Skinning;

    public Dictionary<ulong, LootObject> LootTable;

    public Dictionary<int, TLock> Locks;

    public WS_Loot()
    {
        LootTable = new Dictionary<ulong, LootObject>();
        Locks = new Dictionary<int, TLock>();
    }

    public void On_CMSG_AUTOSTORE_LOOT_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 6)
        {
            return;
        }
        try
        {
            packet.GetInt16();
            var slot = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOSTORE_LOOT_ITEM [slot={2}]", client.IP, client.Port, slot);
            if (LootTable.ContainsKey(client.Character.lootGUID))
            {
                LootTable[client.Character.lootGUID].GetLoot(ref client, slot);
                return;
            }
            Packets.PacketClass response = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
            response.AddInt8(49);
            response.AddUInt64(0uL);
            response.AddUInt64(0uL);
            response.AddInt8(0);
            client.Send(ref response);
            response.Dispose();
        }
        catch (Exception e)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error looting item.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_LOOT_MONEY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_MONEY", client.IP, client.Port);
        if (!LootTable.ContainsKey(client.Character.lootGUID))
        {
            return;
        }
        checked
        {
            if (client.Character.IsInGroup)
            {
                var members = WorldServiceLocator._WS_Spells.GetPartyMembersAroundMe(ref client.Character, 100f);
                LootTable[client.Character.lootGUID].Money = 0;
                Packets.PacketClass sharePcket = new(Opcodes.SMSG_LOOT_MONEY_NOTIFY);
                var copper2 = (LootTable[client.Character.lootGUID].Money / members.Count) + 1;
                sharePcket.AddInt32(copper2);
                foreach (WS_PlayerData.CharacterObject character in members)
                {
                    character.client.SendMultiplyPackets(ref sharePcket);
                    ref var copper3 = ref character.Copper;
                    copper3 = (uint)(copper3 + copper2);
                    character.SetUpdateFlag(1176, character.Copper);
                    character.SaveCharacter();
                }
                client.SendMultiplyPackets(ref sharePcket);
                ref var copper4 = ref client.Character.Copper;
                copper4 = (uint)(copper4 + copper2);
                sharePcket.Dispose();
            }
            else
            {
                var copper = LootTable[client.Character.lootGUID].Money;
                ref var copper5 = ref client.Character.Copper;
                copper5 = (uint)(copper5 + copper);
                LootTable[client.Character.lootGUID].Money = 0;
                Packets.PacketClass lootPacket = new(Opcodes.SMSG_LOOT_MONEY_NOTIFY);
                lootPacket.AddInt32(copper);
                client.Send(ref lootPacket);
                lootPacket.Dispose();
            }
            client.Character.SetUpdateFlag(1176, client.Character.Copper);
            client.Character.SendCharacterUpdate(toNear: false);
            client.Character.SaveCharacter();
            Packets.PacketClass response2 = new(Opcodes.SMSG_LOOT_CLEAR_MONEY);
            client.SendMultiplyPackets(ref response2);
            response2.Dispose();
        }
    }

    public void On_CMSG_LOOT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT [GUID={2:X}]", client.IP, client.Port, GUID);
            client.Character.cUnitFlags |= 0x400;
            client.Character.SetUpdateFlag(46, client.Character.cUnitFlags);
            client.Character.SendCharacterUpdate();
            if (LootTable.ContainsKey(GUID))
            {
                LootTable[GUID].SendLoot(ref client);
            }
            else
            {
                SendEmptyLoot(GUID, LootType.LOOTTYPE_CORPSE, ref client);
            }
        }
    }

    public void On_CMSG_LOOT_RELEASE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 13)
        {
            return;
        }
        packet.GetInt16();
        var GUID = packet.GetUInt64();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_RELEASE [lootGUID={2:X}]", client.IP, client.Port, GUID);
        if (client.Character.spellCasted[1] != null)
        {
            client.Character.spellCasted[1].State = SpellCastState.SPELL_STATE_IDLE;
        }
        client.Character.cUnitFlags &= -1025;
        client.Character.SetUpdateFlag(46, client.Character.cUnitFlags);
        client.Character.SendCharacterUpdate();
        if (LootTable.ContainsKey(GUID))
        {
            LootTable[GUID].SendRelease(ref client);
            LootTable[GUID].LootOwner = 0uL;
            if (LootTable[GUID].IsEmpty)
            {
                LootTable[GUID].Dispose();
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
                {
                    switch (LootTable[GUID].LootType)
                    {
                        case LootType.LOOTTYPE_CORPSE:
                            {
                                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.SkinLootID > 0)
                                {
                                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags = WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags | 0x4000000;
                                }
                                WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags = 0;
                                Packets.PacketClass response3 = new(Opcodes.SMSG_UPDATE_OBJECT);
                                response3.AddInt32(1);
                                response3.AddInt8(0);
                                Packets.UpdateClass UpdateData4 = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                                UpdateData4.SetUpdateFlag(143, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags);
                                UpdateData4.SetUpdateFlag(46, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags);
                                ulong key;
                                Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                                var updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
                                UpdateData4.AddToPacket(ref response3, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                wORLD_CREATUREs[key] = updateObject;
                                WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SendToNearPlayers(ref response3);
                                response3.Dispose();
                                UpdateData4.Dispose();
                                break;
                            }
                        case LootType.LOOTTYPE_SKINNING:
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Despawn();
                            break;
                        default:
                            break;
                    }
                }
                else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID))
                {
                    if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].IsConsumeable)
                    {
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State = GameObjectLootState.LOOT_LOOTED;
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].Despawn();
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State = GameObjectLootState.DOOR_CLOSED;
                    }
                }
                else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsItem(GUID))
                {
                    client.Character.ItemREMOVE(GUID, Destroy: true, Update: true);
                }
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
            {
                switch (LootTable[GUID].LootType)
                {
                    case LootType.LOOTTYPE_CORPSE:
                        {
                            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
                            {
                                LootTable[GUID].Dispose();
                                break;
                            }
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags = 1;
                            Packets.PacketClass response4 = new(Opcodes.SMSG_UPDATE_OBJECT);
                            response4.AddInt32(1);
                            response4.AddInt8(0);
                            Packets.UpdateClass UpdateData3 = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                            UpdateData3.SetUpdateFlag(143, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags);
                            Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                            ulong key;
                            var updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
                            UpdateData3.AddToPacket(ref response4, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                            wORLD_CREATUREs[key] = updateObject;
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SendToNearPlayers(ref response4);
                            response4.Dispose();
                            UpdateData3.Dispose();
                            break;
                        }
                    case LootType.LOOTTYPE_SKINNING:
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Despawn();
                        break;
                    default:
                        break;
                }
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID))
            {
                if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID) || LootTable[GUID].LootType == LootType.LOOTTYPE_FISHING)
                {
                    LootTable[GUID].Dispose();
                }
                else
                {
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State = GameObjectLootState.DOOR_CLOSED;
                    Packets.PacketClass response2 = new(Opcodes.SMSG_UPDATE_OBJECT);
                    response2.AddInt32(1);
                    response2.AddInt8(0);
                    Packets.UpdateClass UpdateData2 = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                    UpdateData2.SetUpdateFlag(14, 0, (byte)WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State);
                    ulong key;
                    Dictionary<ulong, WS_GameObjects.GameObject> wORLD_GAMEOBJECTs;
                    var updateObject2 = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID];
                    UpdateData2.AddToPacket(ref response2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                    wORLD_GAMEOBJECTs[key] = updateObject2;
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].SendToNearPlayers(ref response2);
                    response2.Dispose();
                    UpdateData2.Dispose();
                }
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsItem(GUID))
            {
                LootTable[GUID].Dispose();
                client.Character.ItemREMOVE(GUID, Destroy: true, Update: true);
            }
            else
            {
                LootTable[GUID].Dispose();
            }
        }
        else
        {
            Packets.PacketClass responseRelease = new(Opcodes.SMSG_LOOT_RELEASE_RESPONSE);
            responseRelease.AddUInt64(GUID);
            responseRelease.AddInt8(1);
            client.Send(ref responseRelease);
            responseRelease.Dispose();
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
            {
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.SkinLootID > 0)
                {
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags = WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags | 0x4000000;
                }
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags = 0;
                Packets.PacketClass response = new(Opcodes.SMSG_UPDATE_OBJECT);
                response.AddInt32(1);
                response.AddInt8(0);
                Packets.UpdateClass UpdateData = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                UpdateData.SetUpdateFlag(143, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags);
                UpdateData.SetUpdateFlag(46, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags);
                ulong key;
                Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                var updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
                UpdateData.AddToPacket(ref response, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                wORLD_CREATUREs[key] = updateObject;
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SendToNearPlayers(ref response);
                response.Dispose();
                UpdateData.Dispose();
            }
        }
        client.Character.lootGUID = 0uL;
    }

    public void SendEmptyLoot(ulong GUID, LootType LootType, ref WS_Network.ClientClass client)
    {
        Packets.PacketClass response = new(Opcodes.SMSG_LOOT_RESPONSE);
        response.AddUInt64(GUID);
        response.AddInt8((byte)LootType);
        response.AddInt32(0);
        response.AddInt8(0);
        client.Send(ref response);
        response.Dispose();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Empty loot for GUID [{2:X}].", client.IP, client.Port, GUID);
    }

    public void StartRoll(ulong LootGUID, byte Slot, ref WS_PlayerData.CharacterObject Character)
    {
        List<WS_PlayerData.CharacterObject> rollCharacters = new()
        {
            Character
        };
        foreach (var GUID in Character.Group.LocalMembers)
        {
            if (Character.playersNear.Contains(GUID))
            {
                rollCharacters.Add(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
            }
        }
        Packets.PacketClass startRoll = new(Opcodes.SMSG_LOOT_START_ROLL);
        startRoll.AddUInt64(LootGUID);
        startRoll.AddInt32(Slot);
        startRoll.AddInt32(LootTable[LootGUID].GroupLootInfo[Slot].Item.ItemID);
        startRoll.AddInt32(0);
        startRoll.AddInt32(0);
        startRoll.AddInt32(60000);
        foreach (var objCharacter in rollCharacters)
        {
            objCharacter.client.SendMultiplyPackets(ref startRoll);
        }
        startRoll.Dispose();
        LootTable[LootGUID].GroupLootInfo[Slot].Rolls = rollCharacters;
        LootTable[LootGUID].GroupLootInfo[Slot].RollTimeoutTimer = new Timer(LootTable[LootGUID].GroupLootInfo[Slot].EndRoll, 0, 60000, -1);
    }

    public void On_CMSG_LOOT_ROLL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 >= 18)
            {
                packet.GetInt16();
                var GUID = packet.GetUInt64();
                var Slot = (byte)packet.GetInt32();
                var rollType = packet.GetInt8();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_ROLL [loot={2} roll={3}]", client.IP, client.Port, GUID, rollType);
                Packets.PacketClass response = new(Opcodes.SMSG_LOOT_ROLL);
                response.AddUInt64(GUID);
                response.AddInt32(Slot);
                response.AddUInt64(client.Character.GUID);
                response.AddInt32(LootTable[GUID].GroupLootInfo[Slot].Item.ItemID);
                response.AddInt32(0);
                response.AddInt32(0);
                switch (rollType)
                {
                    case 0:
                        response.AddInt8(249);
                        response.AddInt8(0);
                        break;

                    case 1:
                        response.AddInt8(0);
                        response.AddInt8(0);
                        break;

                    case 2:
                        response.AddInt8(249);
                        response.AddInt8(2);
                        break;
                    default:
                        break;
                }
                LootTable[GUID].GroupLootInfo[Slot].Broadcast(ref response);
                response.Dispose();
                LootTable[GUID].GroupLootInfo[Slot].Looters[client.Character] = rollType;
                LootTable[GUID].GroupLootInfo[Slot].Check();
            }
        }
    }
}
