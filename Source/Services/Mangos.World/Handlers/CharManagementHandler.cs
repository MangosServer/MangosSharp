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
using System.Threading;
using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Server;

namespace Mangos.World.Handlers
{

    /* TODO ERROR: Skipped RegionDirectiveTrivia */
    public class CharManagementHandler
    {
        public void On_CMSG_SET_ACTION_BUTTON(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 10)
                return;
            packet.GetInt16();
            byte button = packet.GetInt8(); // (6)
            ushort action = packet.GetUInt16(); // (7)
            byte actionMisc = packet.GetInt8(); // (9)
            byte actionType = packet.GetInt8(); // (10)
            if (action == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_SET_ACTION_BUTTON [Remove action from button {2}]", client.IP, client.Port, button);
                client.Character.ActionButtons.Remove(button);
            }
            else if (actionType == 64)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTION_BUTTON [Added Macro {2} into button {3}]", client.IP, client.Port, action, button);
            }
            else if (actionType == 128)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTION_BUTTON [Added Item {2} into button {3}]", client.IP, client.Port, action, button);
            }
            else
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTION_BUTTON [Added Action {2}:{4}:{5} into button {3}]", client.IP, client.Port, action, button, actionType, actionMisc);
            }

            client.Character.ActionButtons[button] = new WS_PlayerHelper.TActionButton(action, actionType, actionMisc);
        }

        public void On_CMSG_LOGOUT_REQUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOGOUT_REQUEST", client.IP, client.Port);
            client.Character.Save();

            // TODO: Lose Invisibility

            // DONE: Can't log out in combat
            if (client.Character.IsInCombat)
            {
                var LOGOUT_RESPONSE_DENIED = new Packets.PacketClass(OPCODES.SMSG_LOGOUT_RESPONSE);
                try
                {
                    LOGOUT_RESPONSE_DENIED.AddInt32(0);
                    LOGOUT_RESPONSE_DENIED.AddInt8(LogoutResponseCode.LOGOUT_RESPONSE_DENIED);
                    client.Send(ref LOGOUT_RESPONSE_DENIED);
                }
                finally
                {
                    LOGOUT_RESPONSE_DENIED.Dispose();
                }

                return;
            }

            if (!(client.Character.positionZ > WorldServiceLocator._WS_Maps.GetZCoord(client.Character.positionX, client.Character.positionY, client.Character.positionZ, client.Character.MapID) + 10f))
            {
                // DONE: Initialize packet
                var UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                var SMSG_UPDATE_OBJECT = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                try
                {
                    SMSG_UPDATE_OBJECT.AddInt32(1);      // Operations.Count
                    SMSG_UPDATE_OBJECT.AddInt8(0);

                    // DONE: Disable Turn
                    client.Character.cUnitFlags = client.Character.cUnitFlags | UnitFlags.UNIT_FLAG_STUNTED;
                    UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, client.Character.cUnitFlags);

                    // DONE: StandState -> Sit
                    client.Character.StandState = StandStates.STANDSTATE_SIT;
                    UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, client.Character.cBytes1);

                    // DONE: Send packet
                    UpdateData.AddToPacket(SMSG_UPDATE_OBJECT, ObjectUpdateType.UPDATETYPE_VALUES, client.Character);
                    client.Character.SendToNearPlayers(ref SMSG_UPDATE_OBJECT);
                }
                finally
                {
                    SMSG_UPDATE_OBJECT.Dispose();
                }

                var packetACK = new Packets.PacketClass(OPCODES.SMSG_STANDSTATE_CHANGE_ACK);
                try
                {
                    packetACK.AddInt8(StandStates.STANDSTATE_SIT);
                    client.Send(ref packetACK);
                }
                finally
                {
                    packetACK.Dispose();
                }
            }

            // DONE: Let the client to exit
            var SMSG_LOGOUT_RESPONSE = new Packets.PacketClass(OPCODES.SMSG_LOGOUT_RESPONSE);
            try
            {
                SMSG_LOGOUT_RESPONSE.AddInt32(0);
                SMSG_LOGOUT_RESPONSE.AddInt8(LogoutResponseCode.LOGOUT_RESPONSE_ACCEPTED);     // Logout Accepted
                client.Send(ref SMSG_LOGOUT_RESPONSE);
            }
            finally
            {
                SMSG_LOGOUT_RESPONSE.Dispose();
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_LOGOUT_RESPONSE", client.IP, client.Port);

            // DONE: While logout, the player can't move
            client.Character.SetMoveRoot();

            // DONE: If the player is resting, then it's instant logout
            client.Character.ZoneCheck();
            if (client.Character.isResting)
            {
                client.Character.Logout();
            }
            else
            {
                client.Character.LogoutTimer = new Timer(client.Character.Logout, null, 20000, Timeout.Infinite);
            }
        }

        public void On_CMSG_LOGOUT_CANCEL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOGOUT_CANCEL", client.IP, client.Port);
                if (client is null)
                    return;
                if (client.Character is null)
                    return;
                if (client.Character.LogoutTimer is null)
                    return;
                try
                {
                    client.Character.LogoutTimer.Dispose();
                    client.Character.LogoutTimer = null;
                }
                catch
                {
                }

                // DONE: Initialize packet
                var UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                var SMSG_UPDATE_OBJECT = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                try
                {
                    SMSG_UPDATE_OBJECT.AddInt32(1);      // Operations.Count
                    SMSG_UPDATE_OBJECT.AddInt8(0);

                    // DONE: Enable turn
                    client.Character.cUnitFlags = client.Character.cUnitFlags & !UnitFlags.UNIT_FLAG_STUNTED;
                    UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, client.Character.cUnitFlags);

                    // DONE: StandState -> Stand
                    client.Character.StandState = StandStates.STANDSTATE_STAND;
                    UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, client.Character.cBytes1);

                    // DONE: Send packet
                    UpdateData.AddToPacket(SMSG_UPDATE_OBJECT, ObjectUpdateType.UPDATETYPE_VALUES, client.Character);
                    client.Send(ref SMSG_UPDATE_OBJECT);
                }
                finally
                {
                    SMSG_UPDATE_OBJECT.Dispose();
                }

                var packetACK = new Packets.PacketClass(OPCODES.SMSG_STANDSTATE_CHANGE_ACK);
                try
                {
                    packetACK.AddInt8(StandStates.STANDSTATE_STAND);
                    client.Send(ref packetACK);
                }
                finally
                {
                    packetACK.Dispose();
                }

                // DONE: Stop client logout
                var SMSG_LOGOUT_CANCEL_ACK = new Packets.PacketClass(OPCODES.SMSG_LOGOUT_CANCEL_ACK);
                try
                {
                    client.Send(ref SMSG_LOGOUT_CANCEL_ACK);
                }
                finally
                {
                    SMSG_LOGOUT_CANCEL_ACK.Dispose();
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_LOGOUT_CANCEL_ACK", client.IP, client.Port);

                // DONE: Enable moving
                client.Character.SetMoveUnroot();
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error while trying to cancel logout.{0}", Environment.NewLine + e.ToString());
            }
        }

        public void On_CMSG_STANDSTATECHANGE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            byte StandState = packet.GetInt8();
            if (StandState == StandStates.STANDSTATE_STAND)
            {
                client.Character.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_NOT_SEATED);
            }

            client.Character.StandState = StandState;
            client.Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, client.Character.cBytes1);
            client.Character.SendCharacterUpdate();
            var packetACK = new Packets.PacketClass(OPCODES.SMSG_STANDSTATE_CHANGE_ACK);
            try
            {
                packetACK.AddInt8(StandState);
                client.Send(ref packetACK);
            }
            finally
            {
                packetACK.Dispose();
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_STANDSTATECHANGE [{2}]", client.IP, client.Port, client.Character.StandState);
        }

        public InventoryChangeFailure CanUseAmmo(ref WS_PlayerData.CharacterObject objCharacter, int AmmoID)
        {
            if (objCharacter.DEAD)
                return InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD;
            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(AmmoID) == false)
                return InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND;
            if (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].InventoryType != INVENTORY_TYPES.INVTYPE_AMMO)
                return InventoryChangeFailure.EQUIP_ERR_ONLY_AMMO_CAN_GO_HERE;
            if (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].AvailableClasses != 0L && (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].AvailableClasses & objCharacter.ClassMask) == 0L)
                return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM;
            if (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].AvailableRaces != 0L && (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].AvailableRaces & objCharacter.RaceMask) == 0L)
                return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM;
            if (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].ReqSkill != 0)
            {
                if (objCharacter.HaveSkill(WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].ReqSkill) == false)
                    return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
                if (objCharacter.HaveSkill(WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].ReqSkill, WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].ReqSkillRank) == false)
                    return InventoryChangeFailure.EQUIP_ERR_SKILL_ISNT_HIGH_ENOUGH;
            }

            if (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].ReqSpell != 0)
            {
                if (objCharacter.HaveSpell(WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].ReqSpell) == false)
                    return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
            }

            if (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].ReqLevel > objCharacter.Level)
                return InventoryChangeFailure.EQUIP_ERR_YOU_MUST_REACH_LEVEL_N;
            if (objCharacter.HavePassiveAura(46699))
                return InventoryChangeFailure.EQUIP_ERR_BAG_FULL6; // Required no ammoe
            return InventoryChangeFailure.EQUIP_ERR_OK;
        }

        public bool CheckAmmoCompatibility(ref WS_PlayerData.CharacterObject objCharacter, int AmmoID)
        {
            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(AmmoID) == false)
                return false;
            if (objCharacter.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED) == false || objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).IsBroken)
                return false;
            if (objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_WEAPON)
                return false;
            switch (objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.SubClass)
            {
                case var @case when @case == ITEM_SUBCLASS.ITEM_SUBCLASS_BOW:
                case var case1 when case1 == ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW:
                    {
                        if (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].SubClass != ITEM_SUBCLASS.ITEM_SUBCLASS_ARROW)
                            return false;
                        break;
                    }

                case var case2 when case2 == ITEM_SUBCLASS.ITEM_SUBCLASS_GUN:
                    {
                        if (WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].SubClass != ITEM_SUBCLASS.ITEM_SUBCLASS_BULLET)
                            return false;
                        break;
                    }

                default:
                    {
                        return false;
                    }
            }

            return true;
        }
    }
    /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
}