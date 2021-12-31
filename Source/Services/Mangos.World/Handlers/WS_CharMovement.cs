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

using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.AI;
using Mangos.World.AntiCheat;
using Mangos.World.Globals;
using Mangos.World.Maps;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Mangos.World.Handlers;

public class WS_CharMovement
{
    private const float PId2 = (float)Math.PI / 2f;

    private const float PIx2 = (float)Math.PI * 2f;

    public void OnMovementPacket(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        if (client.Character != null && client.Character.MindControl != null)
        {
            OnControlledMovementPacket(ref packet, ref client.Character.MindControl, ref client.Character);
            return;
        }
        if (client.Character != null)
        {
            client.Character.charMovementFlags = packet.GetInt32();
        }
        var Time = packet.GetUInt32();
        var posX = packet.GetFloat();
        var posY = packet.GetFloat();
        var posZ = packet.GetFloat();
        if (client.Character != null)
        {
            client.Character.orientation = packet.GetFloat();
            WS_Anticheat.MovementEvent(ref client, client.Character.RunSpeed, posX, client.Character.positionX, posY, client.Character.positionY, posZ, client.Character.positionZ, checked((int)Time), WorldServiceLocator._WS_Network.MsTime());
        }
        if (client.Character == null)
        {
            return;
        }
        client.Character.positionX = posX;
        client.Character.positionY = posY;
        client.Character.positionZ = posZ;
        if (client.Character.positionZ < -500f)
        {
            WorldServiceLocator._WorldServer.AllGraveYards.GoToNearestGraveyard(ref client.Character, Alive: false, Teleport: true);
            return;
        }
        if (client.Character.Pet != null && client.Character.Pet.FollowOwner)
        {
            var angle = client.Character.orientation - ((float)Math.PI / 2f);
            if (angle < 0f)
            {
                angle += (float)Math.PI * 2f;
            }
            client.Character.Pet.SetToRealPosition();
            var tmpX = (float)(client.Character.positionX + (Math.Cos(angle) * 2.0));
            var tmpY = (float)(client.Character.positionY + (Math.Sin(angle) * 2.0));
            client.Character.Pet.MoveTo(tmpX, tmpY, client.Character.positionZ, client.Character.orientation, Running: true);
        }
        if (((uint)client.Character.charMovementFlags & 0x2000000u) != 0)
        {
            var transportGUID = packet.GetUInt64();
            var transportX = packet.GetFloat();
            var transportY = packet.GetFloat();
            var transportZ = packet.GetFloat();
            var transportO = packet.GetFloat();
            client.Character.transportX = transportX;
            client.Character.transportY = transportY;
            client.Character.transportZ = transportZ;
            client.Character.transportO = transportO;
            if (client.Character.OnTransport == null)
            {
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(transportGUID) && WorldServiceLocator._WorldServer.WORLD_TRANSPORTs.ContainsKey(transportGUID))
                {
                    client.Character.OnTransport = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[transportGUID];
                    var character = client.Character;
                    var NotSpellID = 0;
                    character.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOUNTED, NotSpellID);
                    WS_Transports.TransportObject obj = (WS_Transports.TransportObject)client.Character.OnTransport;
                    ref var character2 = ref client.Character;
                    ref var reference = ref character2;
                    WS_Base.BaseUnit Unit = character2;
                    obj.AddPassenger(ref Unit);
                    reference = (WS_PlayerData.CharacterObject)Unit;
                }
                else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsTransport(transportGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(transportGUID))
                {
                    client.Character.OnTransport = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[transportGUID];
                }
            }
        }
        else if (client.Character.OnTransport != null)
        {
            if (client.Character.OnTransport is WS_Transports.TransportObject obj2)
            {
                ref var character3 = ref client.Character;
                ref var reference = ref character3;
                WS_Base.BaseUnit Unit = character3;
                obj2.RemovePassenger(ref Unit);
                reference = (WS_PlayerData.CharacterObject)Unit;
            }
            client.Character.OnTransport = null;
        }
        if (((uint)client.Character.charMovementFlags & 0x200000u) != 0)
        {
            var swimAngle = packet.GetFloat();
        }
        packet.GetInt32();
        if (((uint)client.Character.charMovementFlags & 0x2000u) != 0)
        {
            var airTime = packet.GetUInt32();
            var sinAngle = packet.GetFloat();
            var cosAngle = packet.GetFloat();
            var xySpeed = packet.GetFloat();
        }
        if (((uint)client.Character.charMovementFlags & 0x4000000u) != 0)
        {
            var unk1 = packet.GetFloat();
        }
        checked
        {
            if (client.Character.exploreCheckQueued_ && !client.Character.DEAD)
            {
                var exploreFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(client.Character.positionX, client.Character.positionY, (int)client.Character.MapID);
                if (exploreFlag != 65535)
                {
                    var areaFlag = exploreFlag % 32;
                    var areaFlagOffset = (byte)(exploreFlag / 32);
                    if (!WorldServiceLocator._Functions.HaveFlag(client.Character.ZonesExplored[areaFlagOffset], (byte)areaFlag))
                    {
                        WorldServiceLocator._Functions.SetFlag(ref client.Character.ZonesExplored[areaFlagOffset], (byte)areaFlag, flagValue: true);
                        var GainedXP = WorldServiceLocator._WS_Maps.AreaTable[exploreFlag].Level * 10;
                        GainedXP = WorldServiceLocator._WS_Maps.AreaTable[exploreFlag].Level * 10;
                        Packets.PacketClass SMSG_EXPLORATION_EXPERIENCE = new(Opcodes.SMSG_EXPLORATION_EXPERIENCE);
                        SMSG_EXPLORATION_EXPERIENCE.AddInt32(WorldServiceLocator._WS_Maps.AreaTable[exploreFlag].ID);
                        SMSG_EXPLORATION_EXPERIENCE.AddInt32(GainedXP);
                        client.Send(ref SMSG_EXPLORATION_EXPERIENCE);
                        SMSG_EXPLORATION_EXPERIENCE.Dispose();
                        client.Character.SetUpdateFlag(1111 + areaFlagOffset, client.Character.ZonesExplored[areaFlagOffset]);
                        client.Character.AddXP(GainedXP, 0);
                        WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestExplore(ref client.Character, exploreFlag);
                    }
                }
            }
            if (client.Character.isMoving)
            {
                if (client.Character.cEmoteState > 0)
                {
                    client.Character.cEmoteState = 0;
                    client.Character.SetUpdateFlag(148, client.Character.cEmoteState);
                    client.Character.SendCharacterUpdate();
                }
                if (client.Character.spellCasted[1] != null)
                {
                    var castSpellParameters = client.Character.spellCasted[1];
                    if (unchecked((0u - ((!castSpellParameters.Finished) ? 1u : 0u)) & (uint)WorldServiceLocator._WS_Spells.SPELLs[castSpellParameters.SpellID].interruptFlags & (true ? 1u : 0u)) != 0)
                    {
                        client.Character.FinishSpell(CurrentSpellTypes.CURRENT_GENERIC_SPELL);
                    }
                }
                client.Character.RemoveAurasByInterruptFlag(8);
            }
            if (client.Character.isTurning)
            {
                client.Character.RemoveAurasByInterruptFlag(16);
            }
            var MsTime = WorldServiceLocator._WS_Network.MsTime();
            var ClientTimeDelay = (int)(MsTime - Time);
            var MoveTime = (int)(Time - checked(MsTime - ClientTimeDelay) + 500 + MsTime);
            packet.AddInt32(MoveTime, 10);
            Packets.PacketClass response = new(packet.OpCode);
            response.AddPackGUID(client.Character.GUID);
            var tempArray = new byte[packet.Data.Length - 6 + 1];
            Array.Copy(packet.Data, 6, tempArray, 0, packet.Data.Length - 6);
            response.AddByteArray(tempArray);
            client.Character.SendToNearPlayers(ref response, 0uL, ToSelf: false);
            response.Dispose();
            if (client.Character.isMoving)
            {
                client.Character.RemoveAurasByInterruptFlag(8);
            }
            if (client.Character.isTurning)
            {
                client.Character.RemoveAurasByInterruptFlag(16);
            }
        }
    }

    public void OnControlledMovementPacket(ref Packets.PacketClass packet, ref WS_Base.BaseUnit Controlled, ref WS_PlayerData.CharacterObject Controller)
    {
        var MovementFlags = packet.GetInt32();
        var Time = packet.GetUInt32();
        var PositionX = packet.GetFloat();
        var PositionY = packet.GetFloat();
        var PositionZ = packet.GetFloat();
        var Orientation = packet.GetFloat();
        if (Controlled is WS_PlayerData.CharacterObject characterObject)
        {
            characterObject.charMovementFlags = MovementFlags;
            characterObject.positionX = PositionX;
            characterObject.positionY = PositionY;
            characterObject.positionZ = PositionZ;
            characterObject.orientation = Orientation;
        }
        else if (Controlled is WS_Creatures.CreatureObject creatureObject)
        {
            creatureObject.positionX = PositionX;
            creatureObject.positionY = PositionY;
            creatureObject.positionZ = PositionZ;
            creatureObject.orientation = Orientation;
        }
        var MsTime = WorldServiceLocator._WS_Network.MsTime();
        checked
        {
            var ClientTimeDelay = (int)(MsTime - Time);
            var MoveTime = (int)(Time - checked(MsTime - ClientTimeDelay) + 500 + MsTime);
            packet.AddInt32(MoveTime, 10);
            Packets.PacketClass response = new(packet.OpCode);
            response.AddPackGUID(Controlled.GUID);
            var tempArray = new byte[packet.Data.Length - 6 + 1];
            Array.Copy(packet.Data, 6, tempArray, 0, packet.Data.Length - 6);
            response.AddByteArray(tempArray);
            Controlled.SendToNearPlayers(ref response, Controller.GUID);
            response.Dispose();
        }
    }

    public void OnStartSwim(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        OnMovementPacket(ref packet, ref client);
        if (client.Character.positionZ < WorldServiceLocator._WS_Maps.GetWaterLevel(client.Character.positionX, client.Character.positionY, checked((int)client.Character.MapID)))
        {
            if (client.Character.underWaterTimer == null && !client.Character.underWaterBreathing && !client.Character.DEAD)
            {
                client.Character.underWaterTimer = new WS_PlayerHelper.TDrowningTimer(ref client.Character);
            }
        }
        else if (client.Character.underWaterTimer != null)
        {
            client.Character.underWaterTimer.Dispose();
            client.Character.underWaterTimer = null;
        }
    }

    public void OnStopSwim(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (client.Character.underWaterTimer != null)
        {
            client.Character.underWaterTimer.Dispose();
            client.Character.underWaterTimer = null;
        }
        OnMovementPacket(ref packet, ref client);
    }

    public void OnChangeSpeed(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var GUID = packet.GetUInt64();
        if (GUID == client.Character.GUID)
        {
            packet.GetInt32();
            var flags = packet.GetInt32();
            var time = packet.GetInt32();
            client.Character.positionX = packet.GetFloat();
            client.Character.positionY = packet.GetFloat();
            client.Character.positionZ = packet.GetFloat();
            client.Character.orientation = packet.GetFloat();
            if (((uint)flags & 0x2000000u) != 0)
            {
                packet.GetInt64();
                packet.GetFloat();
                packet.GetFloat();
                packet.GetFloat();
                packet.GetFloat();
            }
            if (((uint)flags & 0x200000u) != 0)
            {
                packet.GetFloat();
            }
            float falltime = packet.GetInt32();
            if (((uint)flags & 0x2000u) != 0)
            {
                packet.GetFloat();
                packet.GetFloat();
                packet.GetFloat();
                packet.GetFloat();
            }
            var newSpeed = packet.GetFloat();
            checked
            {
                client.Character.antiHackSpeedChanged_--;
                switch (packet.OpCode)
                {
                    case Opcodes.CMSG_FORCE_RUN_SPEED_CHANGE_ACK:
                        client.Character.RunSpeed = newSpeed;
                        break;

                    case Opcodes.CMSG_FORCE_RUN_BACK_SPEED_CHANGE_ACK:
                        client.Character.RunBackSpeed = newSpeed;
                        break;

                    case Opcodes.CMSG_FORCE_SWIM_BACK_SPEED_CHANGE_ACK:
                        client.Character.SwimBackSpeed = newSpeed;
                        break;

                    case Opcodes.CMSG_FORCE_SWIM_SPEED_CHANGE_ACK:
                        client.Character.SwimSpeed = newSpeed;
                        break;

                    case Opcodes.CMSG_FORCE_TURN_RATE_CHANGE_ACK:
                        client.Character.TurnRate = newSpeed;
                        break;
                }
            }
        }
    }

    public void SendAreaTriggerMessage(ref WS_Network.ClientClass client, string Text)
    {
        Packets.PacketClass p = new(Opcodes.SMSG_AREA_TRIGGER_MESSAGE);
        p.AddInt32(Text.Length);
        p.AddString(Text);
        client.Send(ref p);
        p.Dispose();
    }

    public void On_CMSG_AREATRIGGER(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 9)
            {
                return;
            }
            packet.GetInt16();
            var triggerID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AREATRIGGER [triggerID={2}]", client.IP, client.Port, triggerID);
            DataTable q = new();
            q.Clear();
            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT entry, quest FROM quest_relations WHERE actor=2 and role=0 and entry = {triggerID};", ref q);
            if (q.Rows.Count > 0)
            {
                WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestExplore(ref client.Character, triggerID);
                return;
            }
            q.Clear();
            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM areatrigger_tavern WHERE id = {triggerID};", ref q);
            if (q.Rows.Count > 0)
            {
                client.Character.cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_RESTING;
                client.Character.SetUpdateFlag(190, (int)client.Character.cPlayerFlags);
                client.Character.SendCharacterUpdate();
                return;
            }
            q.Clear();
            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM areatrigger_teleport WHERE id = {triggerID};", ref q);
            float posX;
            float posY;
            float posZ;
            float ori;
            int tMap;
            byte reqLevel;
            if (q.Rows.Count > 0)
            {
                posX = q.Rows[0].As<float>("target_position_x");
                posY = q.Rows[0].As<float>("target_position_y");
                posZ = q.Rows[0].As<float>("target_position_z");
                ori = q.Rows[0].As<float>("target_orientation");
                tMap = q.Rows[0].As<int>("target_map");
                reqLevel = q.Rows[0].As<byte>("required_level");
                if (!client.Character.DEAD)
                {
                    goto IL_029d;
                }
                if (client.Character.corpseMapID == tMap)
                {
                    WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref client.Character);
                    goto IL_029d;
                }
                WorldServiceLocator._WorldServer.AllGraveYards.GoToNearestGraveyard(ref client.Character, Alive: false, Teleport: true);
            }
            else if (!Information.IsNothing(WorldServiceLocator._WorldServer.AreaTriggers))
            {
                if (WorldServiceLocator._WorldServer.AreaTriggers.ContainsMethod("AreaTriggers", $"HandleAreaTrigger_{triggerID}"))
                {
                    WorldServiceLocator._WorldServer.AreaTriggers.InvokeFunction("AreaTriggers", $"HandleAreaTrigger_{triggerID}", new object[1]
                    {
                            client.Character.GUID
                    });
                    return;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] AreaTrigger [{2}] not found!", client.IP, client.Port, triggerID);
            }
            goto end_IL_0001;
        IL_029d:
            if (reqLevel != 0 && client.Character.Level < (uint)reqLevel)
            {
                SendAreaTriggerMessage(ref client, "Your level is too low");
            }
            else if (posX != 0f && posY != 0f && posZ != 0f)
            {
                client.Character.Teleport(posX, posY, posZ, ori, tMap);
            }
        end_IL_0001:
            ;
        }
        catch (Exception ex)
        {
            ProjectData.SetProjectError(ex);
            var e = ex;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when entering areatrigger.{0}", Environment.NewLine + e);
            ProjectData.ClearProjectError();
        }
    }

    public void On_CMSG_MOVE_TIME_SKIPPED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
    }

    public void On_MSG_MOVE_FALL_LAND(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            OnMovementPacket(ref packet, ref client);
            packet.Offset = 6;
            var movFlags = packet.GetInt32();
            packet.GetUInt32();
            packet.GetFloat();
            packet.GetFloat();
            packet.GetFloat();
            packet.GetFloat();
            if (((uint)movFlags & 0x2000000u) != 0)
            {
                packet.GetUInt64();
                packet.GetFloat();
                packet.GetFloat();
                packet.GetFloat();
                packet.GetFloat();
            }
            if (((uint)movFlags & 0x200000u) != 0)
            {
                packet.GetFloat();
            }
            var FallTime = packet.GetInt32();
            checked
            {
                if (FallTime > 1100 && !client.Character.DEAD && client.Character.positionZ > WorldServiceLocator._WS_Maps.GetWaterLevel(client.Character.positionX, client.Character.positionY, (int)client.Character.MapID) && !client.Character.HaveAuraType(AuraEffects_Names.SPELL_AURA_FEATHER_FALL))
                {
                    var safe_fall = client.Character.GetAuraModifier(AuraEffects_Names.SPELL_AURA_SAFE_FALL);
                    if (safe_fall > 0)
                    {
                        FallTime = (FallTime > safe_fall * 10) ? (FallTime - (safe_fall * 10)) : 0;
                    }
                    if (FallTime > 1100)
                    {
                        var FallPerc = (float)(FallTime / 1100.0);
                        var FallDamage = (int)Math.Round(((FallPerc * FallPerc) - 1f) / 9f * client.Character.Life.Maximum);
                        if (FallDamage > 0)
                        {
                            if (FallDamage > client.Character.Life.Maximum)
                            {
                                FallDamage = client.Character.Life.Maximum;
                            }
                            client.Character.LogEnvironmentalDamage(DamageTypes.DMG_FIRE, FallDamage);
                            var character = client.Character;
                            var damage = FallDamage;
                            WS_Base.BaseUnit Attacker = null;
                            character.DealDamage(damage, Attacker);
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[{0}:{1}] Client fall time: {2}  Damage: {3}", client.IP, client.Port, FallTime, FallDamage);
                        }
                    }
                    if (client.Character.underWaterTimer != null && client.Character != null)
                    {
                        client.Character.underWaterTimer.Dispose();
                        client.Character.underWaterTimer = null;
                    }
                    if (client.Character.LogoutTimer != null)
                    {
                        Packets.UpdateClass UpdateData = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                        Packets.PacketClass SMSG_UPDATE_OBJECT = new(Opcodes.SMSG_UPDATE_OBJECT);
                        try
                        {
                            SMSG_UPDATE_OBJECT.AddInt32(1);
                            SMSG_UPDATE_OBJECT.AddInt8(0);
                            client.Character.cUnitFlags |= 0x40000;
                            UpdateData.SetUpdateFlag(46, client.Character.cUnitFlags);
                            client.Character.StandState = 1;
                            UpdateData.SetUpdateFlag(138, client.Character.cBytes1);
                            UpdateData.AddToPacket(ref SMSG_UPDATE_OBJECT, ObjectUpdateType.UPDATETYPE_VALUES, ref client.Character);
                            client.Send(ref SMSG_UPDATE_OBJECT);
                        }
                        finally
                        {
                            SMSG_UPDATE_OBJECT.Dispose();
                        }
                        Packets.PacketClass packetACK = new(Opcodes.SMSG_STANDSTATE_CHANGE_ACK);
                        try
                        {
                            packetACK.AddInt8(1);
                            client.Send(ref packetACK);
                        }
                        finally
                        {
                            packetACK.Dispose();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ProjectData.SetProjectError(ex);
            var e = ex;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error when falling.{0}", Environment.NewLine + e);
            ProjectData.ClearProjectError();
        }
    }

    public void On_CMSG_ZONEUPDATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 >= 9 && client.Character != null)
            {
                packet.GetInt16();
                var newZone = packet.GetInt32();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ZONEUPDATE [newZone={2}]", client.IP, client.Port, newZone);
                client.Character.ZoneID = newZone;
                client.Character.exploreCheckQueued_ = true;
                client.Character.ZoneCheck();
                WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientUpdate(client.Index, (uint)client.Character.ZoneID, client.Character.Level);
                if (WorldServiceLocator._WS_Weather.WeatherZones.ContainsKey(newZone))
                {
                    WorldServiceLocator._WS_Weather.SendWeather(newZone, ref client);
                }
            }
        }
    }

    public void On_MSG_MOVE_HEARTBEAT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        OnMovementPacket(ref packet, ref client);
        if (client.Character == null)
        {
            return;
        }
        if ((client.Character.CellX != WorldServiceLocator._WS_Maps.GetMapTileX(client.Character.positionX)) || (client.Character.CellY != WorldServiceLocator._WS_Maps.GetMapTileY(client.Character.positionY)))
        {
            MoveCell(ref client.Character);
        }
        UpdateCell(ref client.Character);
        client.Character.GroupUpdateFlag |= 0x100u;
        client.Character.ZoneCheck();
        var wS_Maps = WorldServiceLocator._WS_Maps;
        WS_Base.BaseObject objCharacter = client.Character;
        if (wS_Maps.IsOutsideOfMap(ref objCharacter))
        {
            if (!client.Character.outsideMapID_)
            {
                client.Character.outsideMapID_ = true;
                client.Character.StartMirrorTimer(MirrorTimer.FATIGUE, 30000);
            }
        }
        else if (client.Character.outsideMapID_)
        {
            client.Character.outsideMapID_ = false;
            client.Character.StopMirrorTimer(MirrorTimer.FATIGUE);
        }
        if (client.Character.IsInDuel)
        {
            WorldServiceLocator._WS_Spells.CheckDuelDistance(ref client.Character);
        }
        var array = client.Character.creaturesNear.ToArray();
        foreach (var cGUID in array)
        {
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript != null && (WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript is WS_Creatures_AI.DefaultAI || WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript is WS_Creatures_AI.GuardAI) && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.InCombat && !client.Character.inCombatWith.Contains(cGUID) && client.Character.GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) == TReaction.HOSTILE && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], client.Character) <= WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].AggroRange(client.Character))
            {
                var aiScript = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript;
                ref var character = ref client.Character;
                WS_Base.BaseUnit Attacker = character;
                aiScript.OnGenerateHate(ref Attacker, 1);
                character = (WS_PlayerData.CharacterObject)Attacker;
                client.Character.AddToCombat(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.State = AIState.AI_ATTACKING;
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.DoThink();
            }
        }
        var array2 = client.Character.inCombatWith.ToArray();
        foreach (var CombatUnit in array2)
        {
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(CombatUnit) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(CombatUnit) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[CombatUnit].aiScript != null)
            {
                var creatureObject = WorldServiceLocator._WorldServer.WORLD_CREATUREs[CombatUnit];
                if (creatureObject.aiScript.aiTarget != null && creatureObject.aiScript.aiTarget == client.Character)
                {
                    creatureObject.SetToRealPosition();
                    creatureObject.aiScript.State = AIState.AI_MOVE_FOR_ATTACK;
                    creatureObject.aiScript.DoMove();
                }
            }
        }
    }

    public void MAP_Load(byte x, byte y, uint Map)
    {
        short i = -1;
        checked
        {
            do
            {
                short j = -1;
                do
                {
                    if ((short)unchecked(x + i) > -1 && (short)unchecked(x + i) < 64 && (short)unchecked(y + j) > -1 && (short)unchecked(y + j) < 64 && !WorldServiceLocator._WS_Maps.Maps[Map].TileUsed[(short)unchecked(x + i), (short)unchecked(y + j)])
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading map [{2}: {0},{1}]...", (short)unchecked(x + i), (short)unchecked(y + j), Map);
                        WorldServiceLocator._WS_Maps.Maps[Map].TileUsed[(short)unchecked(x + i), (short)unchecked(y + j)] = true;
                        WorldServiceLocator._WS_Maps.Maps[Map].Tiles[(short)unchecked(x + i), (short)unchecked(y + j)] = new WS_Maps.TMapTile((byte)(short)unchecked(x + i), (byte)(short)unchecked(y + j), Map);
                        WorldServiceLocator._WS_Maps.LoadSpawns((byte)(short)unchecked(x + i), (byte)(short)unchecked(y + j), Map, 0u);
                    }
                    j = (short)unchecked(j + 1);
                }
                while (j <= 1);
                i = (short)unchecked(i + 1);
            }
            while (i <= 1);
        }
    }

    public void MAP_UnLoad(byte x, byte y, int Map)
    {
        checked
        {
            if (WorldServiceLocator._WS_Maps.Maps[(uint)Map].Tiles[x, y].PlayersHere.Count == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Unloading map [{2}: {0},{1}]...", x, y, Map);
                WorldServiceLocator._WS_Maps.Maps[(uint)Map].Tiles[x, y].Dispose();
                WorldServiceLocator._WS_Maps.Maps[(uint)Map].Tiles[x, y] = null;
            }
        }
    }

    public void AddToWorld(ref WS_PlayerData.CharacterObject Character)
    {
        WorldServiceLocator._WS_Maps.GetMapTile(Character.positionX, Character.positionY, ref Character.CellX, ref Character.CellY);
        if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY] == null)
        {
            MAP_Load(Character.CellX, Character.CellY, Character.MapID);
        }
        WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].PlayersHere.Add(Character.GUID);
        UpdateCell(ref Character);
        if (Character.Pet != null)
        {
            Character.Pet.Spawn();
        }
    }

    public void RemoveFromWorld(ref WS_PlayerData.CharacterObject Character)
    {
        if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey(Character.MapID))
        {
            return;
        }
        if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY] != null)
        {
            try
            {
                WorldServiceLocator._WS_Maps.GetMapTile(Character.positionX, Character.positionY, ref Character.CellX, ref Character.CellY);
                WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].PlayersHere.Remove(Character.GUID);
            }
            catch (Exception ex2)
            {
                ProjectData.SetProjectError(ex2);
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error removing character {0} from map", Character.Name);
                ProjectData.ClearProjectError();
            }
        }
        var list = Character.SeenBy.ToArray();
        var array = list;
        foreach (var GUID in array)
        {
            if (WorldServiceLocator._WorldServer.CHARACTERs[GUID].playersNear.Contains(Character.GUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving.Add(Character.GUID);
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving_Lock.ReleaseWriterLock();
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].playersNear.Remove(Character.GUID);
            }
            WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Remove(Character.GUID);
        }
        Character.playersNear.Clear();
        Character.SeenBy.Clear();
        list = Character.creaturesNear.ToArray();
        var array2 = list;
        foreach (var GUID2 in array2)
        {
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID2].SeenBy.Contains(Character.GUID))
            {
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID2].SeenBy.Remove(Character.GUID);
            }
        }
        Character.creaturesNear.Clear();
        list = Character.gameObjectsNear.ToArray();
        var array3 = list;
        foreach (var GUID3 in array3)
        {
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(GUID3))
            {
                if (WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID3].SeenBy.Contains(Character.GUID))
                {
                    WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID3].SeenBy.Remove(Character.GUID);
                }
            }
            else if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID3].SeenBy.Contains(Character.GUID))
            {
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID3].SeenBy.Remove(Character.GUID);
            }
        }
        Character.gameObjectsNear.Clear();
        list = Character.corpseObjectsNear.ToArray();
        var array4 = list;
        foreach (var GUID4 in array4)
        {
            if (WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID4].SeenBy.Contains(Character.GUID))
            {
                WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID4].SeenBy.Remove(Character.GUID);
            }
        }
        Character.corpseObjectsNear.Clear();
        if (Character.Pet != null)
        {
            Character.Pet.Hide();
        }
    }

    public void MoveCell(ref WS_PlayerData.CharacterObject Character)
    {
        var oldX = Character.CellX;
        var oldY = Character.CellY;
        WorldServiceLocator._WS_Maps.GetMapTile(Character.positionX, Character.positionY, ref Character.CellX, ref Character.CellY);
        if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY] == null)
        {
            MAP_Load(Character.CellX, Character.CellY, Character.MapID);
        }
        if ((Character.CellX != oldX) || (Character.CellY != oldY) && Character != null)
        {
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles != null)
            {
                WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[oldX, oldY].PlayersHere.Remove(Character.GUID);
            }
            WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].PlayersHere.Add(Character.GUID);
        }
    }

    public void UpdateCell(ref WS_PlayerData.CharacterObject Character)
    {
        var list = Character.playersNear.ToArray();
        var array = list;
        foreach (var GUID in array)
        {
            var obj = Character;
            Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
            ulong key;
            WS_Base.BaseObject objCharacter = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
            var flag = obj.CanSee(ref objCharacter);
            cHARACTERs[key] = (WS_PlayerData.CharacterObject)objCharacter;
            if (!flag)
            {
                Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                Character.guidsForRemoving.Add(GUID);
                Character.guidsForRemoving_Lock.ReleaseWriterLock();
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Remove(Character.GUID);
                Character.playersNear.Remove(GUID);
            }
            var characterObject = WorldServiceLocator._WorldServer.CHARACTERs[GUID];
            objCharacter = Character;
            flag = characterObject.CanSee(ref objCharacter);
            Character = (WS_PlayerData.CharacterObject)objCharacter;
            if (!flag && Character.SeenBy.Contains(GUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving.Add(Character.GUID);
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].guidsForRemoving_Lock.ReleaseWriterLock();
                Character.SeenBy.Remove(GUID);
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].playersNear.Remove(Character.GUID);
            }
        }
        list = Character.creaturesNear.ToArray();
        var array2 = list;
        foreach (var GUID2 in array2)
        {
            int num;
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID2))
            {
                var obj2 = Character;
                Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                ulong key;
                WS_Base.BaseObject objCharacter = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID2];
                var flag = obj2.CanSee(ref objCharacter);
                wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)objCharacter;
                num = (!flag) ? 1 : 0;
            }
            else
            {
                num = 1;
            }
            if (num != 0)
            {
                Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                Character.guidsForRemoving.Add(GUID2);
                Character.guidsForRemoving_Lock.ReleaseWriterLock();
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID2].SeenBy.Remove(Character.GUID);
                Character.creaturesNear.Remove(GUID2);
            }
        }
        list = Character.gameObjectsNear.ToArray();
        var array3 = list;
        foreach (var GUID3 in array3)
        {
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(GUID3))
            {
                var obj3 = Character;
                Dictionary<ulong, WS_Transports.TransportObject> wORLD_TRANSPORTs;
                ulong key;
                WS_Base.BaseObject objCharacter = (wORLD_TRANSPORTs = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)[key = GUID3];
                var flag = obj3.CanSee(ref objCharacter);
                wORLD_TRANSPORTs[key] = (WS_Transports.TransportObject)objCharacter;
                if (!flag)
                {
                    Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    Character.guidsForRemoving.Add(GUID3);
                    Character.guidsForRemoving_Lock.ReleaseWriterLock();
                    WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID3].SeenBy.Remove(Character.GUID);
                    Character.gameObjectsNear.Remove(GUID3);
                }
            }
            else
            {
                var obj4 = Character;
                Dictionary<ulong, WS_GameObjects.GameObject> wORLD_GAMEOBJECTs;
                ulong key;
                WS_Base.BaseObject objCharacter = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID3];
                var flag = obj4.CanSee(ref objCharacter);
                wORLD_GAMEOBJECTs[key] = (WS_GameObjects.GameObject)objCharacter;
                if (!flag)
                {
                    Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    Character.guidsForRemoving.Add(GUID3);
                    Character.guidsForRemoving_Lock.ReleaseWriterLock();
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID3].SeenBy.Remove(Character.GUID);
                    Character.gameObjectsNear.Remove(GUID3);
                }
            }
        }
        list = Character.dynamicObjectsNear.ToArray();
        var array4 = list;
        foreach (var GUID4 in array4)
        {
            var obj5 = Character;
            Dictionary<ulong, WS_DynamicObjects.DynamicObject> wORLD_DYNAMICOBJECTs;
            ulong key;
            WS_Base.BaseObject objCharacter = (wORLD_DYNAMICOBJECTs = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs)[key = GUID4];
            var flag = obj5.CanSee(ref objCharacter);
            wORLD_DYNAMICOBJECTs[key] = (WS_DynamicObjects.DynamicObject)objCharacter;
            if (!flag)
            {
                Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                Character.guidsForRemoving.Add(GUID4);
                Character.guidsForRemoving_Lock.ReleaseWriterLock();
                WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID4].SeenBy.Remove(Character.GUID);
                Character.dynamicObjectsNear.Remove(GUID4);
            }
        }
        list = Character.corpseObjectsNear.ToArray();
        var array5 = list;
        foreach (var GUID5 in array5)
        {
            var obj6 = Character;
            Dictionary<ulong, WS_Corpses.CorpseObject> wORLD_CORPSEOBJECTs;
            ulong key;
            WS_Base.BaseObject objCharacter = (wORLD_CORPSEOBJECTs = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs)[key = GUID5];
            var flag = obj6.CanSee(ref objCharacter);
            wORLD_CORPSEOBJECTs[key] = (WS_Corpses.CorpseObject)objCharacter;
            if (!flag)
            {
                Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                Character.guidsForRemoving.Add(GUID5);
                Character.guidsForRemoving_Lock.ReleaseWriterLock();
                WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID5].SeenBy.Remove(Character.GUID);
                Character.corpseObjectsNear.Remove(GUID5);
            }
        }
        short CellXAdd = -1;
        short CellYAdd = -1;
        if (WorldServiceLocator._WS_Maps.GetSubMapTileX(Character.positionX) > 32)
        {
            CellXAdd = 1;
        }
        if (WorldServiceLocator._WS_Maps.GetSubMapTileX(Character.positionY) > 32)
        {
            CellYAdd = 1;
        }
        checked
        {
            if ((short)(Character.CellX + CellXAdd) is > 63 or < 0)
            {
                CellXAdd = 0;
            }
            if ((short)(Character.CellY + CellYAdd) is > 63 or < 0)
            {
                CellYAdd = 0;
            }
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY] == null)
            {
                MAP_Load(Character.CellX, Character.CellY, Character.MapID);
            }
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].CreaturesHere.Count > 0 || WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].GameObjectsHere.Count > 0)
            {
                UpdateCreaturesAndGameObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY], ref Character);
            }
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].PlayersHere.Count > 0)
            {
                UpdatePlayersInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY], ref Character);
            }
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].CorpseObjectsHere.Count > 0)
            {
                UpdateCorpseObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY], ref Character);
            }
            if (CellXAdd != 0)
            {
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), Character.CellY] == null)
                {
                    MAP_Load((byte)(short)(Character.CellX + CellXAdd), Character.CellY, Character.MapID);
                }
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)(Character.CellX + CellXAdd), Character.CellY].CreaturesHere.Count > 0 || WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), Character.CellY].GameObjectsHere.Count > 0)
                {
                    UpdateCreaturesAndGameObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)(Character.CellX + CellXAdd), Character.CellY], ref Character);
                }
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)(Character.CellX + CellXAdd), Character.CellY].PlayersHere.Count > 0)
                {
                    UpdatePlayersInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)(Character.CellX + CellXAdd), Character.CellY], ref Character);
                }
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)(Character.CellX + CellXAdd), Character.CellY].CorpseObjectsHere.Count > 0)
                {
                    UpdateCorpseObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)(Character.CellX + CellXAdd), Character.CellY], ref Character);
                }
            }
            if (CellYAdd != 0)
            {
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, (short)(Character.CellY + CellYAdd)] == null)
                {
                    MAP_Load(Character.CellX, (byte)(short)unchecked(Character.CellY + CellYAdd), Character.MapID);
                }
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, (short)unchecked(Character.CellY + CellYAdd)].CreaturesHere.Count > 0 || WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, (short)unchecked(Character.CellY + CellYAdd)].GameObjectsHere.Count > 0)
                {
                    UpdateCreaturesAndGameObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, (short)unchecked(Character.CellY + CellYAdd)], ref Character);
                }
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, (short)unchecked(Character.CellY + CellYAdd)].PlayersHere.Count > 0)
                {
                    UpdatePlayersInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, (short)unchecked(Character.CellY + CellYAdd)], ref Character);
                }
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, (short)unchecked(Character.CellY + CellYAdd)].CorpseObjectsHere.Count > 0)
                {
                    UpdateCorpseObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, (short)unchecked(Character.CellY + CellYAdd)], ref Character);
                }
            }
            if (CellYAdd != 0 && CellXAdd != 0)
            {
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), (short)unchecked(Character.CellY + CellYAdd)] == null)
                {
                    MAP_Load((byte)(short)unchecked(Character.CellX + CellXAdd), (byte)(short)unchecked(Character.CellY + CellYAdd), Character.MapID);
                }
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), (short)unchecked(Character.CellY + CellYAdd)].CreaturesHere.Count > 0 || WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), (short)unchecked(Character.CellY + CellYAdd)].GameObjectsHere.Count > 0)
                {
                    UpdateCreaturesAndGameObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), (short)unchecked(Character.CellY + CellYAdd)], ref Character);
                }
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), (short)unchecked(Character.CellY + CellYAdd)].PlayersHere.Count > 0)
                {
                    UpdatePlayersInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), (short)unchecked(Character.CellY + CellYAdd)], ref Character);
                }
                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), (short)unchecked(Character.CellY + CellYAdd)].CorpseObjectsHere.Count > 0)
                {
                    UpdateCorpseObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), (short)unchecked(Character.CellY + CellYAdd)], ref Character);
                }
            }
            Character.SendOutOfRangeUpdate();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void UpdatePlayersInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
    {
        var tMapTile = MapTile;
        var list = tMapTile.PlayersHere.ToArray();
        var array = list;
        foreach (var GUID in array)
        {
            if (!WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Contains(Character.GUID))
            {
                var obj = Character;
                Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
                ulong key;
                WS_Base.BaseObject objCharacter = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
                var flag = obj.CanSee(ref objCharacter);
                cHARACTERs[key] = (WS_PlayerData.CharacterObject)objCharacter;
                if (flag)
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
                    packet.AddInt32(1);
                    packet.AddInt8(0);
                    Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].FillAllUpdateFlags(ref tmpUpdate);
                    var updateClass = tmpUpdate;
                    var updateObject = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
                    updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
                    cHARACTERs[key] = updateObject;
                    tmpUpdate.Dispose();
                    Character.client.Send(ref packet);
                    packet.Dispose();
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Add(Character.GUID);
                    Character.playersNear.Add(GUID);
                }
            }
            if (!Character.SeenBy.Contains(GUID))
            {
                var characterObject = WorldServiceLocator._WorldServer.CHARACTERs[GUID];
                WS_Base.BaseObject objCharacter = Character;
                var flag = characterObject.CanSee(ref objCharacter);
                Character = (WS_PlayerData.CharacterObject)objCharacter;
                if (flag)
                {
                    Packets.PacketClass myPacket = new(Opcodes.SMSG_UPDATE_OBJECT);
                    myPacket.AddInt32(1);
                    myPacket.AddInt8(0);
                    Packets.UpdateClass myTmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                    Character.FillAllUpdateFlags(ref myTmpUpdate);
                    myTmpUpdate.AddToPacket(ref myPacket, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref Character);
                    myTmpUpdate.Dispose();
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].client.Send(ref myPacket);
                    myPacket.Dispose();
                    Character.SeenBy.Add(GUID);
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].playersNear.Add(Character.GUID);
                }
            }
        }
    }

    public void UpdateCreaturesAndGameObjectsInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
    {
        Packets.UpdatePacketClass packet = new();
        var tMapTile = MapTile;
        var list = tMapTile.CreaturesHere.ToArray();
        var array = list;
        foreach (var GUID in array)
        {
            if (!Character.creaturesNear.Contains(GUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
            {
                var obj = Character;
                Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                ulong key;
                WS_Base.BaseObject objCharacter = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
                var flag = obj.CanSee(ref objCharacter);
                wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)objCharacter;
                if (flag)
                {
                    Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].FillAllUpdateFlags(ref tmpUpdate);
                    var updateClass = tmpUpdate;
                    Packets.PacketClass packet2 = packet;
                    var updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
                    updateClass.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
                    wORLD_CREATUREs[key] = updateObject;
                    packet = (Packets.UpdatePacketClass)packet2;
                    tmpUpdate.Dispose();
                    Character.creaturesNear.Add(GUID);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SeenBy.Add(Character.GUID);
                }
            }
        }
        list = tMapTile.GameObjectsHere.ToArray();
        var array2 = list;
        foreach (var GUID2 in array2)
        {
            if (Character.gameObjectsNear.Contains(GUID2))
            {
                continue;
            }
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(GUID2))
            {
                var obj2 = Character;
                Dictionary<ulong, WS_Transports.TransportObject> wORLD_TRANSPORTs;
                ulong key;
                WS_Base.BaseObject objCharacter = (wORLD_TRANSPORTs = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)[key = GUID2];
                var flag = obj2.CanSee(ref objCharacter);
                wORLD_TRANSPORTs[key] = (WS_Transports.TransportObject)objCharacter;
                if (flag)
                {
                    Packets.UpdateClass tmpUpdate3 = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                    WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID2].FillAllUpdateFlags(ref tmpUpdate3, ref Character);
                    var updateClass2 = tmpUpdate3;
                    Packets.PacketClass packet2 = packet;
                    WS_GameObjects.GameObject updateObject2 = (wORLD_TRANSPORTs = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)[key = GUID2];
                    updateClass2.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject2);
                    wORLD_TRANSPORTs[key] = (WS_Transports.TransportObject)updateObject2;
                    packet = (Packets.UpdatePacketClass)packet2;
                    tmpUpdate3.Dispose();
                    Character.gameObjectsNear.Add(GUID2);
                    WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID2].SeenBy.Add(Character.GUID);
                }
            }
            else
            {
                var obj3 = Character;
                Dictionary<ulong, WS_GameObjects.GameObject> wORLD_GAMEOBJECTs;
                ulong key;
                WS_Base.BaseObject objCharacter = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID2];
                var flag = obj3.CanSee(ref objCharacter);
                wORLD_GAMEOBJECTs[key] = (WS_GameObjects.GameObject)objCharacter;
                if (flag)
                {
                    Packets.UpdateClass tmpUpdate2 = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID2].FillAllUpdateFlags(ref tmpUpdate2, ref Character);
                    var updateClass3 = tmpUpdate2;
                    Packets.PacketClass packet2 = packet;
                    var updateObject2 = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID2];
                    updateClass3.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject2);
                    wORLD_GAMEOBJECTs[key] = updateObject2;
                    packet = (Packets.UpdatePacketClass)packet2;
                    tmpUpdate2.Dispose();
                    Character.gameObjectsNear.Add(GUID2);
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID2].SeenBy.Add(Character.GUID);
                }
            }
        }
        list = tMapTile.DynamicObjectsHere.ToArray();
        var array3 = list;
        foreach (var GUID3 in array3)
        {
            if (!Character.dynamicObjectsNear.Contains(GUID3))
            {
                var obj4 = Character;
                Dictionary<ulong, WS_DynamicObjects.DynamicObject> wORLD_DYNAMICOBJECTs;
                ulong key;
                WS_Base.BaseObject objCharacter = (wORLD_DYNAMICOBJECTs = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs)[key = GUID3];
                var flag = obj4.CanSee(ref objCharacter);
                wORLD_DYNAMICOBJECTs[key] = (WS_DynamicObjects.DynamicObject)objCharacter;
                if (flag)
                {
                    Packets.UpdateClass tmpUpdate4 = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_DYNAMICOBJECT);
                    WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID3].FillAllUpdateFlags(ref tmpUpdate4);
                    var updateClass4 = tmpUpdate4;
                    Packets.PacketClass packet2 = packet;
                    var updateObject3 = (wORLD_DYNAMICOBJECTs = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs)[key = GUID3];
                    updateClass4.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF, ref updateObject3);
                    wORLD_DYNAMICOBJECTs[key] = updateObject3;
                    packet = (Packets.UpdatePacketClass)packet2;
                    tmpUpdate4.Dispose();
                    Character.dynamicObjectsNear.Add(GUID3);
                    WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID3].SeenBy.Add(Character.GUID);
                }
            }
        }

        if (packet.UpdatesCount > 0)
        {
            packet.CompressUpdatePacket();
            var client = Character.client;
            Packets.PacketClass packet2 = packet;
            client.Send(ref packet2);
            packet = (Packets.UpdatePacketClass)packet2;
        }
        packet.Dispose();
    }

    public void UpdateCreaturesInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
    {
        var tMapTile = MapTile;
        var list = tMapTile.CreaturesHere.ToArray();
        var array = list;
        foreach (var GUID in array)
        {
            if (!Character.creaturesNear.Contains(GUID))
            {
                var obj = Character;
                Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                ulong key;
                WS_Base.BaseObject objCharacter = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
                var flag = obj.CanSee(ref objCharacter);
                wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)objCharacter;
                if (flag)
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
                    packet.AddInt32(1);
                    packet.AddInt8(0);
                    Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].FillAllUpdateFlags(ref tmpUpdate);
                    var updateClass = tmpUpdate;
                    var updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
                    updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
                    wORLD_CREATUREs[key] = updateObject;
                    tmpUpdate.Dispose();
                    Character.client.Send(ref packet);
                    packet.Dispose();
                    Character.creaturesNear.Add(GUID);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SeenBy.Add(Character.GUID);
                }
            }
        }
    }

    public void UpdateGameObjectsInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
    {
        var tMapTile = MapTile;
        var list = tMapTile.GameObjectsHere.ToArray();
        var array = list;
        foreach (var GUID in array)
        {
            if (!Character.gameObjectsNear.Contains(GUID))
            {
                int num;
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID))
                {
                    var obj = Character;
                    Dictionary<ulong, WS_GameObjects.GameObject> wORLD_GAMEOBJECTs;
                    ulong key;
                    WS_Base.BaseObject objCharacter = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID];
                    var flag = obj.CanSee(ref objCharacter);
                    wORLD_GAMEOBJECTs[key] = (WS_GameObjects.GameObject)objCharacter;
                    num = flag ? 1 : 0;
                }
                else
                {
                    num = 0;
                }
                if (num != 0)
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
                    packet.AddInt32(1);
                    packet.AddInt8(0);
                    Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].FillAllUpdateFlags(ref tmpUpdate, ref Character);
                    var updateClass = tmpUpdate;
                    Dictionary<ulong, WS_GameObjects.GameObject> wORLD_GAMEOBJECTs;
                    ulong key;
                    var updateObject = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID];
                    updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
                    wORLD_GAMEOBJECTs[key] = updateObject;
                    tmpUpdate.Dispose();
                    Character.client.Send(ref packet);
                    packet.Dispose();
                    Character.gameObjectsNear.Add(GUID);
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].SeenBy.Add(Character.GUID);
                }
            }
        }
    }

    public void UpdateCorpseObjectsInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
    {
        var tMapTile = MapTile;
        var list = tMapTile.CorpseObjectsHere.ToArray();
        var array = list;
        foreach (var GUID in array)
        {
            if (!Character.corpseObjectsNear.Contains(GUID))
            {
                var obj = Character;
                Dictionary<ulong, WS_Corpses.CorpseObject> wORLD_CORPSEOBJECTs;
                ulong key;
                WS_Base.BaseObject objCharacter = (wORLD_CORPSEOBJECTs = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs)[key = GUID];
                var flag = obj.CanSee(ref objCharacter);
                wORLD_CORPSEOBJECTs[key] = (WS_Corpses.CorpseObject)objCharacter;
                if (flag)
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
                    packet.AddInt32(1);
                    packet.AddInt8(0);
                    Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_CORPSE);
                    WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].FillAllUpdateFlags(ref tmpUpdate);
                    var updateClass = tmpUpdate;
                    var updateObject = (wORLD_CORPSEOBJECTs = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs)[key = GUID];
                    updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
                    wORLD_CORPSEOBJECTs[key] = updateObject;
                    tmpUpdate.Dispose();
                    Character.client.Send(ref packet);
                    packet.Dispose();
                    Character.corpseObjectsNear.Add(GUID);
                    WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].SeenBy.Add(Character.GUID);
                }
            }
        }
    }
}
