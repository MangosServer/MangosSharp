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
using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.AI;
using Mangos.World.Globals;
using Mangos.World.Maps;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Handlers
{
    public class WS_CharMovement
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private const double PId2 = Math.PI / 2d;
        private const double PIx2 = 2d * Math.PI;

        public void OnMovementPacket(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            if (client.Character.MindControl is object)
            {
                OnControlledMovementPacket(ref packet, ref client.Character.MindControl, ref client.Character);
                return;
            }

            client.Character.charMovementFlags = packet.GetInt32();
            uint Time = packet.GetUInt32();
            float posX = packet.GetFloat();
            float posY = packet.GetFloat();
            float posZ = packet.GetFloat();
            client.Character.orientation = packet.GetFloat();

            // Anticheat injection
            WS_Anticheat.MovementEvent(ref client, client.Character.RunSpeed, posX, client.Character.positionX, posY, client.Character.positionY, posZ, client.Character.positionZ, (int)Time, WorldServiceLocator._WS_Network.MsTime());
            if (client.Character is null)
            {
                return;
            }

            client.Character.positionX = posX;
            client.Character.positionY = posY;
            client.Character.positionZ = posZ;


            // DONE: If character is falling below the world
            if (client.Character.positionZ < -500.0f)
            {
                WorldServiceLocator._WorldServer.AllGraveYards.GoToNearestGraveyard(ref client.Character, false, true);
                return;
            }

            if (client.Character.Pet is object)
            {
                if (client.Character.Pet.FollowOwner)
                {
                    float angle = (float)(client.Character.orientation - PId2);
                    if (angle < 0f)
                        angle = (float)PIx2;
                    client.Character.Pet.SetToRealPosition();
                    float tmpX = (float)(client.Character.positionX + Math.Cos(angle) * 2.0d);
                    float tmpY = (float)(client.Character.positionY + Math.Sin(angle) * 2.0d);
                    client.Character.Pet.MoveTo(tmpX, tmpY, client.Character.positionZ, client.Character.orientation, true);
                }
            }

            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            if (client.Character.charMovementFlags && MovementFlags.MOVEMENTFLAG_ONTRANSPORT)
                {
                ulong transportGUID = packet.GetUInt64();
                float transportX = packet.GetFloat();
                float transportY = packet.GetFloat();
                float transportZ = packet.GetFloat();
                float transportO = packet.GetFloat();
                client.Character.transportX = transportX;
                client.Character.transportY = transportY;
                client.Character.transportZ = transportZ;
                client.Character.transportO = transportO;

                // DONE: Boarding transport
                if (client.Character.OnTransport is null)
                {
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(transportGUID) && WorldServiceLocator._WorldServer.WORLD_TRANSPORTs.ContainsKey(transportGUID))
                    {
                        client.Character.OnTransport = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[transportGUID];

                        // DONE: Unmount when boarding
                        int argNotSpellID = 0;
                        client.Character.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOUNTED, NotSpellID: ref argNotSpellID);
                        WS_Base.BaseUnit argUnit = client.Character;
                        ((WS_Transports.TransportObject)client.Character.OnTransport).AddPassenger(ref argUnit);
                    }
                    else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsTransport(transportGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(transportGUID))
                    {
                        client.Character.OnTransport = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[transportGUID];
                    }
                }
            }
            else if (client.Character.OnTransport is object)
            {
                // DONE: Unboarding transport
                if (client.Character.OnTransport is WS_Transports.TransportObject)
                {
                    WS_Base.BaseUnit argUnit1 = client.Character;
                    ((WS_Transports.TransportObject)client.Character.OnTransport).RemovePassenger(ref argUnit1);
                }

                client.Character.OnTransport = null;
            }

            if (client.Character.charMovementFlags & MovementFlags.MOVEMENTFLAG_SWIMMING)
            {
                float swimAngle = packet.GetFloat();
                // #If DEBUG Then
                // Console.WriteLine("[{0}] [{1}:{2}] Client swim angle:{3}", Format(TimeOfDay, "hh:mm:ss"), client.IP, client.Port, swimAngle)
                // #End If
            }

            packet.GetInt32(); // Fall time
            if (client.Character.charMovementFlags & MovementFlags.MOVEMENTFLAG_JUMPING)
            {
                uint airTime = packet.GetUInt32();
                float sinAngle = packet.GetFloat();
                float cosAngle = packet.GetFloat();
                float xySpeed = packet.GetFloat();
                // #If DEBUG Then
                // Console.WriteLine("[{0}] [{1}:{2}] Client jump: 0x{3:X} {4} {5} {6}", Format(TimeOfDay, "hh:mm:ss"), client.IP, client.Port, unk, sinAngle, cosAngle, xySpeed)
                // #End If
            }

            if (client.Character.charMovementFlags & MovementFlags.MOVEMENTFLAG_SPLINE)
            {
                float unk1 = packet.GetFloat();
            }

            if (client.Character.exploreCheckQueued_ && !client.Character.DEAD)
            {
                int exploreFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(client.Character.positionX, client.Character.positionY, (int)client.Character.MapID);

                // DONE: Checking Explore System
                if (exploreFlag != 0xFFFF)
                {
                    int areaFlag = exploreFlag % 32;
                    byte areaFlagOffset = (byte)(exploreFlag / 32);
                    if (!WorldServiceLocator._Functions.HaveFlag(client.Character.ZonesExplored[areaFlagOffset], (byte)areaFlag))
                    {
                        WorldServiceLocator._Functions.SetFlag(ref client.Character.ZonesExplored[areaFlagOffset], (byte)areaFlag, true);
                        int GainedXP = WorldServiceLocator._WS_Maps.AreaTable[exploreFlag].Level * 10;
                        GainedXP = WorldServiceLocator._WS_Maps.AreaTable[exploreFlag].Level * 10;
                        var SMSG_EXPLORATION_EXPERIENCE = new Packets.PacketClass(OPCODES.SMSG_EXPLORATION_EXPERIENCE);
                        SMSG_EXPLORATION_EXPERIENCE.AddInt32(WorldServiceLocator._WS_Maps.AreaTable[exploreFlag].ID);
                        SMSG_EXPLORATION_EXPERIENCE.AddInt32(GainedXP);
                        client.Send(SMSG_EXPLORATION_EXPERIENCE);
                        SMSG_EXPLORATION_EXPERIENCE.Dispose();
                        client.Character.SetUpdateFlag((int)(EPlayerFields.PLAYER_EXPLORED_ZONES_1 + areaFlagOffset), client.Character.ZonesExplored[(int)areaFlagOffset]);
                        client.Character.AddXP(GainedXP, 0, 0UL, true);

                        // DONE: Fire quest event to check for if this area is used in explore area quest
                        WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestExplore(ref client.Character, exploreFlag);
                    }
                }
            }

            // If character is moving
            if (client.Character.isMoving)
            {
                // DONE: Stop emotes if moving
                if (client.Character.cEmoteState > 0)
                {
                    client.Character.cEmoteState = 0;
                    client.Character.SetUpdateFlag((int)EUnitFields.UNIT_NPC_EMOTESTATE, client.Character.cEmoteState);
                    client.Character.SendCharacterUpdate(true);
                }

                // DONE: Stop casting
                if (client.Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL] is object)
                {
                    {
                        var withBlock = client.Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL];
                        if (withBlock.Finished == false & (WorldServiceLocator._WS_Spells.SPELLs[withBlock.SpellID].interruptFlags & SpellInterruptFlags.SPELL_INTERRUPT_FLAG_MOVEMENT))
                        {
                            client.Character.FinishSpell(CurrentSpellTypes.CURRENT_GENERIC_SPELL);
                        }
                    }
                }

                client.Character.RemoveAurasByInterruptFlag((int)SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_MOVE);
            }

            // If character is turning
            if (client.Character.isTurning)
            {
                client.Character.RemoveAurasByInterruptFlag((int)SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_TURNING);
            }

            // DONE: Movement time calculation
            // TODO: PROPERLY MOVE THIS OVER TO THE CMSG_MOVE_TIME_SKIPPED OPCODE, Reference @ LN 406
            int MsTime = WorldServiceLocator._WS_Network.MsTime();
            int ClientTimeDelay = (int)(MsTime - Time);
            int MoveTime = (int)(Time - (MsTime - ClientTimeDelay) + 500L + MsTime);
            packet.AddInt32(MoveTime, 10);

            // DONE: Send to nearby players
            var response = new Packets.PacketClass(packet.OpCode);
            response.AddPackGUID(client.Character.GUID);
            var tempArray = new byte[packet.Data.Length - 6 + 1];
            Array.Copy(packet.Data, 6, tempArray, 0, packet.Data.Length - 6);
            response.AddByteArray(tempArray);
            client.Character.SendToNearPlayers(ref response, ToSelf: false);
            response.Dispose();

            // NOTE: They may slow the movement down so let's do them after the packet is sent
            // DONE: Remove auras that requires you to not move
            if (client.Character.isMoving)
            {
                client.Character.RemoveAurasByInterruptFlag((int)SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_MOVE);
            }
            // DONE: Remove auras that requires you to not turn
            if (client.Character.isTurning)
            {
                client.Character.RemoveAurasByInterruptFlag((int)SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_TURNING);
            }
        }

        public void OnControlledMovementPacket(ref Packets.PacketClass packet, ref WS_Base.BaseUnit Controlled, ref WS_PlayerData.CharacterObject Controller)
        {
            int MovementFlags = packet.GetInt32();
            uint Time = packet.GetUInt32();
            float PositionX = packet.GetFloat();
            float PositionY = packet.GetFloat();
            float PositionZ = packet.GetFloat();
            float Orientation = packet.GetFloat();
            if (Controlled is WS_PlayerData.CharacterObject)
            {
                {
                    var withBlock = (WS_PlayerData.CharacterObject)Controlled;
                    withBlock.charMovementFlags = MovementFlags;
                    withBlock.positionX = PositionX;
                    withBlock.positionY = PositionY;
                    withBlock.positionZ = PositionZ;
                    withBlock.orientation = Orientation;
                }
            }
            else if (Controlled is WS_Creatures.CreatureObject)
            {
                {
                    var withBlock1 = (WS_Creatures.CreatureObject)Controlled;
                    withBlock1.positionX = PositionX;
                    withBlock1.positionY = PositionY;
                    withBlock1.positionZ = PositionZ;
                    withBlock1.orientation = Orientation;
                }
            }

            // DONE: Movement time calculation
            int MsTime = WorldServiceLocator._WS_Network.MsTime();
            int ClientTimeDelay = (int)(MsTime - Time);
            int MoveTime = (int)(Time - (MsTime - ClientTimeDelay) + 500L + MsTime);
            packet.AddInt32(MoveTime, 10);

            // DONE: Send to nearby players
            var response = new Packets.PacketClass(packet.OpCode);
            response.AddPackGUID(Controlled.GUID);
            var tempArray = new byte[packet.Data.Length - 6 + 1];
            Array.Copy(packet.Data, 6, tempArray, 0, packet.Data.Length - 6);
            response.AddByteArray(tempArray);
            Controlled.SendToNearPlayers(ref response, Controller.GUID);
            response.Dispose();
        }

        public void OnStartSwim(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            OnMovementPacket(ref packet, ref client);
            if (client.Character.positionZ < WorldServiceLocator._WS_Maps.GetWaterLevel(client.Character.positionX, client.Character.positionY, (int)client.Character.MapID))
            {
                if (client.Character.underWaterTimer is null && !client.Character.underWaterBreathing && !client.Character.DEAD)
                {
                    client.Character.underWaterTimer = new WS_PlayerHelper.TDrowningTimer(ref client.Character);
                }
            }
            else if (client.Character.underWaterTimer is object)
            {
                client.Character.underWaterTimer.Dispose();
                client.Character.underWaterTimer = null;
            }
        }

        public void OnStopSwim(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (client.Character.underWaterTimer is object)
            {
                client.Character.underWaterTimer.Dispose();
                client.Character.underWaterTimer = null;
            }

            OnMovementPacket(ref packet, ref client);
        }

        public void OnChangeSpeed(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            if (GUID != client.Character.GUID)
                return; // Skip it, it's not our packet
            packet.GetInt32();
            int flags = packet.GetInt32();
            int time = packet.GetInt32();
            client.Character.positionX = packet.GetFloat();
            client.Character.positionY = packet.GetFloat();
            client.Character.positionZ = packet.GetFloat();
            client.Character.orientation = packet.GetFloat();
            if (flags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT)
            {
                packet.GetInt64(); // GUID
                packet.GetFloat(); // X
                packet.GetFloat(); // Y
                packet.GetFloat(); // Z
                packet.GetFloat(); // O
            }

            if (flags & MovementFlags.MOVEMENTFLAG_SWIMMING)
            {
                packet.GetFloat(); // angle
            }

            float falltime = packet.GetInt32();
            if (flags & MovementFlags.MOVEMENTFLAG_JUMPING)
            {
                packet.GetFloat(); // unk
                packet.GetFloat(); // sin angle
                packet.GetFloat(); // cos angle
                packet.GetFloat(); // xyz speed
            }

            float newSpeed = packet.GetFloat();

            // DONE: Update speed value and create packet
            client.Character.antiHackSpeedChanged_ -= 1;
            switch (packet.OpCode)
            {
                case var @case when @case == OPCODES.CMSG_FORCE_RUN_SPEED_CHANGE_ACK:
                    {
                        client.Character.RunSpeed = newSpeed;
                        break;
                    }

                case var case1 when case1 == OPCODES.CMSG_FORCE_RUN_BACK_SPEED_CHANGE_ACK:
                    {
                        client.Character.RunBackSpeed = newSpeed;
                        break;
                    }

                case var case2 when case2 == OPCODES.CMSG_FORCE_SWIM_BACK_SPEED_CHANGE_ACK:
                    {
                        client.Character.SwimBackSpeed = newSpeed;
                        break;
                    }

                case var case3 when case3 == OPCODES.CMSG_FORCE_SWIM_SPEED_CHANGE_ACK:
                    {
                        client.Character.SwimSpeed = newSpeed;
                        break;
                    }

                case var case4 when case4 == OPCODES.CMSG_FORCE_TURN_RATE_CHANGE_ACK:
                    {
                        client.Character.TurnRate = newSpeed;
                        break;
                    }
            }
        }

        public void SendAreaTriggerMessage(ref WS_Network.ClientClass client, string Text)
        {
            var p = new Packets.PacketClass(OPCODES.SMSG_AREA_TRIGGER_MESSAGE);
            p.AddInt32(Text.Length);
            p.AddString(Text);
            client.Send(p);
            p.Dispose();
        }

        public void On_CMSG_AREATRIGGER(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                if (packet.Data.Length - 1 < 9)
                    return;
                packet.GetInt16();
                int triggerID = packet.GetInt32();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AREATRIGGER [triggerID={2}]", client.IP, client.Port, triggerID);

                // TODO: Check if in combat?

                var q = new DataTable();

                // DONE: Handling quest triggers
                q.Clear();
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT entry, quest FROM quest_relations WHERE actor=2 and role=0 and entry = {0};", (object)triggerID), ref q);
                if (q.Rows.Count > 0)
                {
                    WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestExplore(ref client.Character, triggerID);
                    return;
                }

                // TODO: Handling tavern triggers
                q.Clear();
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM areatrigger_tavern WHERE id = {0};", (object)triggerID), ref q);
                if (q.Rows.Count > 0)
                {
                    client.Character.cPlayerFlags = client.Character.cPlayerFlags | PlayerFlags.PLAYER_FLAGS_RESTING;
                    client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FLAGS, (int)client.Character.cPlayerFlags);
                    client.Character.SendCharacterUpdate(true);
                    return;
                }

                // DONE: Handling teleport triggers
                q.Clear();
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM areatrigger_teleport WHERE id = {0};", (object)triggerID), ref q);
                if (q.Rows.Count > 0)
                {
                    float posX = Conversions.ToSingle(q.Rows[0]["target_position_x"]);
                    float posY = Conversions.ToSingle(q.Rows[0]["target_position_y"]);
                    float posZ = Conversions.ToSingle(q.Rows[0]["target_position_z"]);
                    float ori = Conversions.ToSingle(q.Rows[0]["target_orientation"]);
                    int tMap = Conversions.ToInteger(q.Rows[0]["target_map"]);
                    byte reqLevel = Conversions.ToByte(q.Rows[0]["required_level"]);
                    if (client.Character.DEAD)
                    {
                        if (client.Character.corpseMapID == tMap)
                        {
                            WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref client.Character);
                        }
                        else
                        {
                            WorldServiceLocator._WorldServer.AllGraveYards.GoToNearestGraveyard(ref client.Character, false, true);
                            return;
                        }
                    }

                    if (reqLevel != 0 && client.Character.Level < reqLevel)
                    {
                        SendAreaTriggerMessage(ref client, "Your level is too low");
                        return;
                    }

                    if (posX != 0f & posY != 0f & posZ != 0f)
                    {
                        client.Character.Teleport(posX, posY, posZ, ori, tMap);
                    }

                    return;
                }

                // DONE: Handling all other scripted triggers
                if (!Information.IsNothing(WorldServiceLocator._WorldServer.AreaTriggers))
                {
                    if (WorldServiceLocator._WorldServer.AreaTriggers.ContainsMethod("AreaTriggers", string.Format("HandleAreaTrigger_{0}", triggerID)))
                    {
                        WorldServiceLocator._WorldServer.AreaTriggers.InvokeFunction("AreaTriggers", string.Format("HandleAreaTrigger_{0}", triggerID), new object[] { client.Character.GUID });
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] AreaTrigger [{2}] not found!", client.IP, client.Port, triggerID);
                    }
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when entering areatrigger.{0}", Environment.NewLine + e.ToString());
            }
        }

        public void On_CMSG_MOVE_TIME_SKIPPED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            // TODO: Figure out why this is causing a freeze everytime the packet is called, Reference @ LN 180

            // packet.GetUInt64()
            // packet.GetUInt32()
            // Dim MsTime As Integer = WS_Network.msTime()
            // Dim ClientTimeDelay As Integer = MsTime - MsTime
            // Dim MoveTime As Integer = (MsTime - (MsTime - ClientTimeDelay)) + 500 + MsTime
            // packet.AddInt32(MoveTime, 10)
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MOVE_TIME_SKIPPED", client.IP, client.Port);
        }

        public void On_MSG_MOVE_FALL_LAND(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                OnMovementPacket(ref packet, ref client);
                packet.Offset = 6;
                int movFlags = packet.GetInt32();
                packet.GetUInt32();
                packet.GetFloat();
                packet.GetFloat();
                packet.GetFloat();
                packet.GetFloat();
                if (movFlags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT)
                {
                    packet.GetUInt64();
                    packet.GetFloat();
                    packet.GetFloat();
                    packet.GetFloat();
                    packet.GetFloat();
                }

                if (movFlags & MovementFlags.MOVEMENTFLAG_SWIMMING)
                {
                    packet.GetFloat();
                }

                int FallTime = packet.GetInt32();

                // DONE: If FallTime > 1100 and not Dead
                if (FallTime > 1100 && !client.Character.DEAD && client.Character.positionZ > WorldServiceLocator._WS_Maps.GetWaterLevel(client.Character.positionX, client.Character.positionY, (int)client.Character.MapID))
                {
                    if (client.Character.HaveAuraType(AuraEffects_Names.SPELL_AURA_FEATHER_FALL) == false)
                    {
                        int safe_fall = client.Character.GetAuraModifier(AuraEffects_Names.SPELL_AURA_SAFE_FALL);
                        if (safe_fall > 0)
                        {
                            if (FallTime > safe_fall * 10)
                            {
                                FallTime -= safe_fall * 10;
                            }
                            else
                            {
                                FallTime = 0;
                            }
                        }

                        if (FallTime > 1100)
                        {
                            // DONE: Caluclate fall damage
                            float FallPerc = (float)(FallTime / 1100d);
                            int FallDamage = (int)((FallPerc * FallPerc - 1f) / 9f * client.Character.Life.Maximum);
                            if (FallDamage > 0)
                            {
                                // Prevent the fall damage to be more than your maximum health
                                if (FallDamage > client.Character.Life.Maximum)
                                    FallDamage = client.Character.Life.Maximum;
                                // Deal the damage
                                client.Character.LogEnvironmentalDamage((DamageTypes)EnvironmentalDamage.DAMAGE_FALL, FallDamage);
                                WS_Base.BaseUnit argAttacker = null;
                                client.Character.DealDamage(FallDamage, Attacker: ref argAttacker);

                                /* TODO ERROR: Skipped IfDirectiveTrivia */
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[{0}:{1}] Client fall time: {2}  Damage: {3}", client.IP, client.Port, FallTime, FallDamage);
                                /* TODO ERROR: Skipped EndIfDirectiveTrivia */
                            }
                        }
                    }
                }

                if (client.Character.underWaterTimer is object)
                {
                    client.Character.underWaterTimer.Dispose();
                    client.Character.underWaterTimer = null;
                }

                if (client.Character.LogoutTimer is object)
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
                        UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, client.Character.cUnitFlags);
                        // DONE: StandState -> Sit
                        client.Character.StandState = (byte)StandStates.STANDSTATE_SIT;
                        UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_BYTES_1, client.Character.cBytes1);

                        // DONE: Send packet
                        UpdateData.AddToPacket(SMSG_UPDATE_OBJECT, ObjectUpdateType.UPDATETYPE_VALUES, ref client.Character);
                        client.Send(SMSG_UPDATE_OBJECT);
                    }
                    finally
                    {
                        SMSG_UPDATE_OBJECT.Dispose();
                    }

                    var packetACK = new Packets.PacketClass(OPCODES.SMSG_STANDSTATE_CHANGE_ACK);
                    try
                    {
                        packetACK.AddInt8((byte)StandStates.STANDSTATE_SIT);
                        client.Send(packetACK);
                    }
                    finally
                    {
                        packetACK.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error when falling.{0}", Environment.NewLine + e.ToString());
            }
        }

        public void On_CMSG_ZONEUPDATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int newZone = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ZONEUPDATE [newZone={2}]", client.IP, client.Port, newZone);
            client.Character.ZoneID = newZone;
            client.Character.exploreCheckQueued_ = true;
            client.Character.ZoneCheck();

            // DONE: Update zone on cluster
            WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientUpdate(client.Index, (uint)client.Character.ZoneID, client.Character.Level);

            // DONE: Send weather
            if (WorldServiceLocator._WS_Weather.WeatherZones.ContainsKey(newZone))
            {
                WorldServiceLocator._WS_Weather.SendWeather(newZone, ref client);
            }
        }

        public void On_MSG_MOVE_HEARTBEAT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            OnMovementPacket(ref packet, ref client);
            if (client.Character is null)
            {
                return;
            }

            if (client.Character.CellX != WorldServiceLocator._WS_Maps.GetMapTileX(client.Character.positionX) | client.Character.CellY != WorldServiceLocator._WS_Maps.GetMapTileY(client.Character.positionY))
            {
                MoveCell(ref client.Character);
            }

            UpdateCell(ref client.Character);
            client.Character.GroupUpdateFlag = client.Character.GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION;
            client.Character.ZoneCheck();

            // DONE: Check for out of continent - coordinates from WorldMapContinent.dbc
            if (WorldServiceLocator._WS_Maps.IsOutsideOfMap(client.Character))
            {
                if (client.Character.outsideMapID_ == false)
                {
                    client.Character.outsideMapID_ = true;
                    client.Character.StartMirrorTimer(MirrorTimer.FATIGUE, 30000);
                }
            }
            else if (client.Character.outsideMapID_ == true)
            {
                client.Character.outsideMapID_ = false;
                client.Character.StopMirrorTimer(MirrorTimer.FATIGUE);
            }

            // DONE: Duel check
            if (client.Character.IsInDuel)
                WorldServiceLocator._WS_Spells.CheckDuelDistance(ref client.Character);

            // DONE: Aggro range
            foreach (ulong cGUID in client.Character.creaturesNear.ToArray())
            {
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript is object && (WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript is WS_Creatures_AI.DefaultAI || WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript is WS_Creatures_AI.GuardAI))
                {
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead == false && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.InCombat() == false)
                    {
                        if (client.Character.inCombatWith.Contains(cGUID))
                            continue;
                        if (client.Character.GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) == TReaction.HOSTILE && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], client.Character) <= WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].AggroRange(client.Character))
                        {
                            WS_Base.BaseUnit argAttacker = client.Character;
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.OnGenerateHate(ref argAttacker, 1);
                            client.Character.AddToCombat(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.State = AIState.AI_ATTACKING;
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.DoThink();
                        }
                    }
                }
            }

            // DONE: Creatures that are following you will have a more smooth movement
            foreach (ulong CombatUnit in client.Character.inCombatWith.ToArray())
            {
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(CombatUnit) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(CombatUnit) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[CombatUnit].aiScript is object)
                {
                    {
                        var withBlock = WorldServiceLocator._WorldServer.WORLD_CREATUREs[CombatUnit];
                        if (withBlock.aiScript.aiTarget is object && ReferenceEquals(withBlock.aiScript.aiTarget, client.Character))
                        {
                            withBlock.SetToRealPosition(); // Make sure it moves from it's location and not from where it was already heading before this
                            withBlock.aiScript.State = AIState.AI_MOVE_FOR_ATTACK;
                            withBlock.aiScript.DoMove();
                        }
                    }
                }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void MAP_Load(byte x, byte y, uint Map)
        {
            for (short i = -1; i <= 1; i++)
            {
                for (short j = -1; j <= 1; j++)
                {
                    if (x + i > -1 && x + i < 64 && y + j > -1 && y + j < 64)
                    {
                        if (WorldServiceLocator._WS_Maps.Maps[Map].TileUsed[x + i, y + j] == false)
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading map [{2}: {0},{1}]...", (short)(x + i), (short)(y + j), Map);
                            WorldServiceLocator._WS_Maps.Maps[Map].TileUsed[x + i, y + j] = true;
                            WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x + i, y + j] = new WS_Maps.TMapTile((byte)(x + i), (byte)(y + j), Map);
                            // DONE: Load spawns
                            WorldServiceLocator._WS_Maps.LoadSpawns((byte)(x + i), (byte)(y + j), Map, 0U);
                        }
                    }
                }
            }

            GC.Collect();
        }

        public void MAP_UnLoad(byte x, byte y, int Map)
        {
            if (WorldServiceLocator._WS_Maps.Maps[(uint)Map].Tiles[x, y].PlayersHere.Count == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Unloading map [{2}: {0},{1}]...", x, y, Map);
                WorldServiceLocator._WS_Maps.Maps[(uint)Map].Tiles[x, y].Dispose();
                WorldServiceLocator._WS_Maps.Maps[(uint)Map].Tiles[x, y] = null;
            }
        }

        public void AddToWorld(ref WS_PlayerData.CharacterObject Character)
        {
            WorldServiceLocator._WS_Maps.GetMapTile(Character.positionX, Character.positionY, ref Character.CellX, ref Character.CellY);

            // DONE: Dynamic map loading
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY] is null)
                MAP_Load(Character.CellX, Character.CellY, Character.MapID);

            // DONE: Cleanig
            // myPacket.Dispose()
            WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].PlayersHere.Add(Character.GUID);

            // DONE: Send all creatures and gameobjects to the client
            UpdateCell(ref Character);

            // DONE: Add the pet to the world as well
            if (Character.Pet is object)
            {
                Character.Pet.Spawn();
            }
        }

        public void RemoveFromWorld(ref WS_PlayerData.CharacterObject Character)
        {
            if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey(Character.MapID))
                return;
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY] is object)
            {
                // DONE: Remove character from map
                try
                {
                    WorldServiceLocator._WS_Maps.GetMapTile(Character.positionX, Character.positionY, ref Character.CellX, ref Character.CellY);
                    WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].PlayersHere.Remove(Character.GUID);
                }
                catch (Exception)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error removing character {0} from map", Character.Name);
                }
            }

            ulong[] list;

            // DONE: Removing from players wich can see it
            list = Character.SeenBy.ToArray();
            foreach (ulong GUID in list)
            {
                if (WorldServiceLocator._WorldServer.CHARACTERs[GUID].playersNear.Contains(Character.GUID))
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving.Add(Character.GUID);
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving_Lock.ReleaseWriterLock();
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].playersNear.Remove(Character.GUID);
                }
                // DONE: Fully clean
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Remove(Character.GUID);
            }

            Character.playersNear.Clear();
            Character.SeenBy.Clear();

            // DONE: Removing from creatures wich can see it
            list = Character.creaturesNear.ToArray();
            foreach (ulong GUID in list)
            {
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SeenBy.Contains(Character.GUID))
                {
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SeenBy.Remove(Character.GUID);
                }
            }

            Character.creaturesNear.Clear();

            // DONE: Removing from gameObjects wich can see it
            list = Character.gameObjectsNear.ToArray();
            foreach (ulong GUID in list)
            {
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(GUID))
                {
                    if (WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID].SeenBy.Contains(Character.GUID))
                    {
                        WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID].SeenBy.Remove(Character.GUID);
                    }
                }
                else if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].SeenBy.Contains(Character.GUID))
                {
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].SeenBy.Remove(Character.GUID);
                }
            }

            Character.gameObjectsNear.Clear();

            // DONE: Removing from corpseObjects wich can see it
            list = Character.corpseObjectsNear.ToArray();
            foreach (ulong GUID in list)
            {
                if (WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].SeenBy.Contains(Character.GUID))
                {
                    WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].SeenBy.Remove(Character.GUID);
                }
            }

            Character.corpseObjectsNear.Clear();

            // DONE: Remove the pet from the world as well
            if (Character.Pet is object)
            {
                Character.Pet.Hide();
            }
        }

        public void MoveCell(ref WS_PlayerData.CharacterObject Character)
        {
            byte oldX = Character.CellX;
            byte oldY = Character.CellY;
            WorldServiceLocator._WS_Maps.GetMapTile(Character.positionX, Character.positionY, ref Character.CellX, ref Character.CellY);

            // Map Loading
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY] is null)
                MAP_Load(Character.CellX, Character.CellY, Character.MapID);

            // TODO: Fix map unloading

            if (Character.CellX != oldX | Character.CellY != oldY)
            {
                WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[oldX, oldY].PlayersHere.Remove(Character.GUID);
                // MAP_UnLoad(oldX, oldY)
                WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].PlayersHere.Add(Character.GUID);
            }
        }

        public void UpdateCell(ref WS_PlayerData.CharacterObject Character)
        {
            // Dim start As Integer = _NativeMethods.timeGetTime("")
            ulong[] list;

            // DONE: Remove players,creatures,objects if dist is >
            list = Character.playersNear.ToArray();
            foreach (ulong GUID in list)
            {
                var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
                WS_Base.BaseObject argobjCharacter = tmp[GUID];
                if (!Character.CanSee(ref argobjCharacter))
                {
                    Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    Character.guidsForRemoving.Add(GUID);
                    Character.guidsForRemoving_Lock.ReleaseWriterLock();
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Remove(Character.GUID);
                    Character.playersNear.Remove(GUID);
                }

                tmp[GUID] = (WS_PlayerData.CharacterObject)argobjCharacter;
                // Remove me for him
                WS_Base.BaseObject argobjCharacter1 = Character;
                if (!WorldServiceLocator._WorldServer.CHARACTERs[GUID].CanSee(ref argobjCharacter1) && Character.SeenBy.Contains(GUID))
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving.Add(Character.GUID);
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving_Lock.ReleaseWriterLock();
                    Character.SeenBy.Remove(GUID);
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].playersNear.Remove(Character.GUID);
                }
            }

            list = Character.creaturesNear.ToArray();
            foreach (ulong GUID in list)
            {
                bool localCanSee() { var tmp = WorldServiceLocator._WorldServer.WORLD_CREATUREs; WS_Base.BaseObject argobjCharacter = tmp[GUID]; var ret = Character.CanSee(ref argobjCharacter); tmp[GUID] = (WS_Creatures.CreatureObject)argobjCharacter; return ret; }

                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID) == false || localCanSee() == false)
                {
                    Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    Character.guidsForRemoving.Add(GUID);
                    Character.guidsForRemoving_Lock.ReleaseWriterLock();
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SeenBy.Remove(Character.GUID);
                    Character.creaturesNear.Remove(GUID);
                }
            }

            list = Character.gameObjectsNear.ToArray();
            foreach (ulong GUID in list)
            {
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(GUID))
                {
                    var tmp1 = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs;
                    WS_Base.BaseObject argobjCharacter2 = tmp1[GUID];
                    if (!Character.CanSee(ref argobjCharacter2))
                    {
                        Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        Character.guidsForRemoving.Add(GUID);
                        Character.guidsForRemoving_Lock.ReleaseWriterLock();
                        WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID].SeenBy.Remove(Character.GUID);
                        Character.gameObjectsNear.Remove(GUID);
                    }

                    tmp1[GUID] = (WS_Transports.TransportObject)argobjCharacter2;
                }
                else
                {
                    var tmp2 = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs;
                    WS_Base.BaseObject argobjCharacter3 = tmp2[GUID];
                    if (!Character.CanSee(ref argobjCharacter3))
                    {
                        Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        Character.guidsForRemoving.Add(GUID);
                        Character.guidsForRemoving_Lock.ReleaseWriterLock();
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].SeenBy.Remove(Character.GUID);
                        Character.gameObjectsNear.Remove(GUID);
                    }

                    tmp2[GUID] = (WS_GameObjects.GameObjectObject)argobjCharacter3;
                }
            }

            list = Character.dynamicObjectsNear.ToArray();
            foreach (ulong GUID in list)
            {
                var tmp3 = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs;
                WS_Base.BaseObject argobjCharacter4 = tmp3[GUID];
                if (!Character.CanSee(ref argobjCharacter4))
                {
                    Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    Character.guidsForRemoving.Add(GUID);
                    Character.guidsForRemoving_Lock.ReleaseWriterLock();
                    WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID].SeenBy.Remove(Character.GUID);
                    Character.dynamicObjectsNear.Remove(GUID);
                }

                tmp3[GUID] = (WS_DynamicObjects.DynamicObjectObject)argobjCharacter4;
            }

            list = Character.corpseObjectsNear.ToArray();
            foreach (ulong GUID in list)
            {
                var tmp4 = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs;
                WS_Base.BaseObject argobjCharacter5 = tmp4[GUID];
                if (!Character.CanSee(ref argobjCharacter5))
                {
                    Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    Character.guidsForRemoving.Add(GUID);
                    Character.guidsForRemoving_Lock.ReleaseWriterLock();
                    WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].SeenBy.Remove(Character.GUID);
                    Character.corpseObjectsNear.Remove(GUID);
                }

                tmp4[GUID] = (WS_Corpses.CorpseObject)argobjCharacter5;
            }

            // DONE: Add new if dist is <
            short CellXAdd = -1;
            short CellYAdd = -1;
            if (WorldServiceLocator._WS_Maps.GetSubMapTileX(Character.positionX) > 32)
                CellXAdd = 1;
            if (WorldServiceLocator._WS_Maps.GetSubMapTileX(Character.positionY) > 32)
                CellYAdd = 1;
            if (Character.CellX + CellXAdd > 63 | Character.CellX + CellXAdd < 0)
                CellXAdd = 0;
            if (Character.CellY + CellYAdd > 63 | Character.CellY + CellYAdd < 0)
                CellYAdd = 0;

            // DONE: Load cell if needed
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY] is null)
            {
                MAP_Load(Character.CellX, Character.CellY, Character.MapID);
            }
            // DONE: Sending near creatures and gameobjects in <CENTER CELL>
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].CreaturesHere.Count > 0 || WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].GameObjectsHere.Count > 0)
            {
                UpdateCreaturesAndGameObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY], ref Character);
            }
            // DONE: Sending near players in <CENTER CELL>
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].PlayersHere.Count > 0)
            {
                UpdatePlayersInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY], ref Character);
            }
            // DONE: Sending near corpseobjects in <CENTER CELL>
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].CorpseObjectsHere.Count > 0)
            {
                UpdateCorpseObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY], ref Character);
            }

            if (CellXAdd != 0)
            {
                // DONE: Load cell if needed
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY] is null)
                {
                    MAP_Load((byte)(Character.CellX + CellXAdd), Character.CellY, Character.MapID);
                }
                // DONE: Sending near creatures and gameobjects in <LEFT/RIGHT CELL>
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY].CreaturesHere.Count > 0 || WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY].GameObjectsHere.Count > 0)
                {
                    UpdateCreaturesAndGameObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY], ref Character);
                }
                // DONE: Sending near players in <LEFT/RIGHT CELL>
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY].PlayersHere.Count > 0)
                {
                    UpdatePlayersInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY], ref Character);
                }
                // DONE: Sending near corpseobjects in <LEFT/RIGHT CELL>
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY].CorpseObjectsHere.Count > 0)
                {
                    UpdateCorpseObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY], ref Character);
                }
            }

            if (CellYAdd != 0)
            {
                // DONE: Load cell if needed
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY + CellYAdd] is null)
                {
                    MAP_Load(Character.CellX, (byte)(Character.CellY + CellYAdd), Character.MapID);
                }
                // DONE: Sending near creatures and gameobjects in <TOP/BOTTOM CELL>
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY + CellYAdd].CreaturesHere.Count > 0 || WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY + CellYAdd].GameObjectsHere.Count > 0)
                {
                    UpdateCreaturesAndGameObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY + CellYAdd], ref Character);
                }
                // DONE: Sending near players in <TOP/BOTTOM CELL>
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY + CellYAdd].PlayersHere.Count > 0)
                {
                    UpdatePlayersInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY + CellYAdd], ref Character);
                }
                // DONE: Sending near corpseobjects in <TOP/BOTTOM CELL>
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY + CellYAdd].CorpseObjectsHere.Count > 0)
                {
                    UpdateCorpseObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY + CellYAdd], ref Character);
                }
            }

            if (CellYAdd != 0 && CellXAdd != 0)
            {
                // DONE: Load cell if needed
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY + CellYAdd] is null)
                {
                    MAP_Load((byte)(Character.CellX + CellXAdd), (byte)(Character.CellY + CellYAdd), Character.MapID);
                }
                // DONE: Sending near creatures and gameobjects in <CORNER CELL>
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY + CellYAdd].CreaturesHere.Count > 0 || WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY + CellYAdd].GameObjectsHere.Count > 0)
                {
                    UpdateCreaturesAndGameObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY + CellYAdd], ref Character);
                }
                // DONE: Sending near players in <LEFT/RIGHT CELL>
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY + CellYAdd].PlayersHere.Count > 0)
                {
                    UpdatePlayersInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY + CellYAdd], ref Character);
                }
                // DONE: Sending near corpseobjects in <LEFT/RIGHT CELL>
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY + CellYAdd].CorpseObjectsHere.Count > 0)
                {
                    UpdateCorpseObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX + CellXAdd, Character.CellY + CellYAdd], ref Character);
                }
            }

            Character.SendOutOfRangeUpdate();
            // _WorldServer.Log.WriteLine(LogType.DEBUG, "Update: {0}ms", _NativeMethods.timeGetTime("") - start)
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdatePlayersInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
        {
            ulong[] list;
            list = MapTile.PlayersHere.ToArray();
            foreach (ulong GUID in list)
            {

                // DONE: Send to me
                if (!WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Contains(Character.GUID))
                {
                    var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
                    WS_Base.BaseObject argobjCharacter = tmp[GUID];
                    if (Character.CanSee(ref argobjCharacter))
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                        packet.AddInt32(1);
                        packet.AddInt8(0);
                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                        WorldServiceLocator._WorldServer.CHARACTERs[GUID].FillAllUpdateFlags(ref tmpUpdate);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
                        tmpUpdate.Dispose();
                        Character.client.Send(packet);
                        packet.Dispose();
                        WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Add(Character.GUID);
                        Character.playersNear.Add(GUID);
                    }

                    tmp[GUID] = (WS_PlayerData.CharacterObject)argobjCharacter;
                }
                // DONE: Send to him
                if (!Character.SeenBy.Contains(GUID))
                {
                    WS_Base.BaseObject argobjCharacter1 = Character;
                    if (WorldServiceLocator._WorldServer.CHARACTERs[GUID].CanSee(ref argobjCharacter1))
                    {
                        var myPacket = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                        myPacket.AddInt32(1);
                        myPacket.AddInt8(0);
                        var myTmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                        Character.FillAllUpdateFlags(ref myTmpUpdate);
                        myTmpUpdate.AddToPacket(myPacket, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref Character);
                        myTmpUpdate.Dispose();
                        WorldServiceLocator._WorldServer.CHARACTERs[GUID].client.Send(myPacket);
                        myPacket.Dispose();
                        Character.SeenBy.Add(GUID);
                        WorldServiceLocator._WorldServer.CHARACTERs[GUID].playersNear.Add(Character.GUID);
                    }
                }
            }
        }

        public void UpdateCreaturesAndGameObjectsInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
        {
            ulong[] list;
            var packet = new Packets.UpdatePacketClass();
            list = MapTile.CreaturesHere.ToArray();
            foreach (ulong GUID in list)
            {
                if (!Character.creaturesNear.Contains(GUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
                {
                    var tmp = WorldServiceLocator._WorldServer.WORLD_CREATUREs;
                    WS_Base.BaseObject argobjCharacter = tmp[GUID];
                    if (Character.CanSee(ref argobjCharacter))
                    {
                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].FillAllUpdateFlags(ref tmpUpdate);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID]);
                        tmpUpdate.Dispose();
                        Character.creaturesNear.Add(GUID);
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SeenBy.Add(Character.GUID);
                    }

                    tmp[GUID] = (WS_Creatures.CreatureObject)argobjCharacter;
                }
            }

            list = MapTile.GameObjectsHere.ToArray();
            foreach (ulong GUID in list)
            {
                if (!Character.gameObjectsNear.Contains(GUID))
                {
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(GUID))
                    {
                        var tmp1 = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs;
                        WS_Base.BaseObject argobjCharacter1 = tmp1[GUID];
                        if (Character.CanSee(ref argobjCharacter1))
                        {
                            var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                            WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID].FillAllUpdateFlags(ref tmpUpdate, ref Character);
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID]);
                            tmpUpdate.Dispose();
                            Character.gameObjectsNear.Add(GUID);
                            WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID].SeenBy.Add(Character.GUID);
                        }

                        tmp1[GUID] = (WS_Transports.TransportObject)argobjCharacter1;
                    }
                    else
                    {
                        var tmp2 = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs;
                        WS_Base.BaseObject argobjCharacter2 = tmp2[GUID];
                        if (Character.CanSee(ref argobjCharacter2))
                        {
                            var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].FillAllUpdateFlags(ref tmpUpdate, ref Character);
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID]);
                            tmpUpdate.Dispose();
                            Character.gameObjectsNear.Add(GUID);
                            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].SeenBy.Add(Character.GUID);
                        }

                        tmp2[GUID] = (WS_GameObjects.GameObjectObject)argobjCharacter2;
                    }
                }
            }

            list = MapTile.DynamicObjectsHere.ToArray();
            foreach (ulong GUID in list)
            {
                if (!Character.dynamicObjectsNear.Contains(GUID))
                {
                    var tmp3 = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs;
                    WS_Base.BaseObject argobjCharacter3 = tmp3[GUID];
                    if (Character.CanSee(ref argobjCharacter3))
                    {
                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_DYNAMICOBJECT);
                        WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID].FillAllUpdateFlags(ref tmpUpdate);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF, WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID]);
                        tmpUpdate.Dispose();
                        Character.dynamicObjectsNear.Add(GUID);
                        WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID].SeenBy.Add(Character.GUID);
                    }

                    tmp3[GUID] = (WS_DynamicObjects.DynamicObjectObject)argobjCharacter3;
                }
            }

            // DONE: Send creatures, game objects and dynamic objects in the same packet
            if (packet.UpdatesCount > 0)
            {
                packet.CompressUpdatePacket();
                Packets.PacketClass argpacket = packet;
                Character.client.Send(argpacket);
            }

            packet.Dispose();
        }

        public void UpdateCreaturesInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
        {
            ulong[] list;
            list = MapTile.CreaturesHere.ToArray();
            foreach (ulong GUID in list)
            {
                if (!Character.creaturesNear.Contains(GUID))
                {
                    var tmp = WorldServiceLocator._WorldServer.WORLD_CREATUREs;
                    WS_Base.BaseObject argobjCharacter = tmp[GUID];
                    if (Character.CanSee(ref argobjCharacter))
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                        packet.AddInt32(1);
                        packet.AddInt8(0);
                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].FillAllUpdateFlags(ref tmpUpdate);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID]);
                        tmpUpdate.Dispose();
                        Character.client.Send(packet);
                        packet.Dispose();
                        Character.creaturesNear.Add(GUID);
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SeenBy.Add(Character.GUID);
                    }

                    tmp[GUID] = (WS_Creatures.CreatureObject)argobjCharacter;
                }
            }
        }

        public void UpdateGameObjectsInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
        {
            ulong[] list;
            list = MapTile.GameObjectsHere.ToArray();
            foreach (ulong GUID in list)
            {
                if (!Character.gameObjectsNear.Contains(GUID))
                {
                    bool localCanSee() { var tmp = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs; WS_Base.BaseObject argobjCharacter = tmp[GUID]; var ret = Character.CanSee(ref argobjCharacter); tmp[GUID] = (WS_GameObjects.GameObjectObject)argobjCharacter; return ret; }

                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID) && localCanSee())
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                        packet.AddInt32(1);
                        packet.AddInt8(0);
                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].FillAllUpdateFlags(ref tmpUpdate, ref Character);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID]);
                        tmpUpdate.Dispose();
                        Character.client.Send(packet);
                        packet.Dispose();
                        Character.gameObjectsNear.Add(GUID);
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].SeenBy.Add(Character.GUID);
                    }
                }
            }
        }

        public void UpdateCorpseObjectsInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
        {
            ulong[] list;
            list = MapTile.CorpseObjectsHere.ToArray();
            foreach (ulong GUID in list)
            {
                if (!Character.corpseObjectsNear.Contains(GUID))
                {
                    var tmp = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs;
                    WS_Base.BaseObject argobjCharacter = tmp[GUID];
                    if (Character.CanSee(ref argobjCharacter))
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                        packet.AddInt32(1);
                        packet.AddInt8(0);
                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_CORPSE);
                        WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].FillAllUpdateFlags(ref tmpUpdate);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID]);
                        tmpUpdate.Dispose();
                        Character.client.Send(packet);
                        packet.Dispose();
                        Character.corpseObjectsNear.Add(GUID);
                        WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].SeenBy.Add(Character.GUID);
                    }

                    tmp[GUID] = (WS_Corpses.CorpseObject)argobjCharacter;
                }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}