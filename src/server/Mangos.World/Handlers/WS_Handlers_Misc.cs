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
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Mangos.Common;
using Mangos.MySql;
using Mangos.World.AI;
using Mangos.World.Globals;
using Mangos.World.Loots;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Quests;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;

namespace Mangos.World.Handlers;

public class WS_Handlers_Misc
{
    public void On_CMSG_NAME_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 13)
            {
                return;
            }
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NAME_QUERY [GUID={2:X}]", client.IP, client.Port, GUID);
            Packets.PacketClass SMSG_NAME_QUERY_RESPONSE = new(Opcodes.SMSG_NAME_QUERY_RESPONSE);
            switch (GUID)
            {
                case int.MaxValue:
                    try
                    {
                        SMSG_NAME_QUERY_RESPONSE.AddUInt64(GUID);
                        SMSG_NAME_QUERY_RESPONSE.AddString("System");
                        SMSG_NAME_QUERY_RESPONSE.AddInt32(1);
                        SMSG_NAME_QUERY_RESPONSE.AddInt32(1);
                        SMSG_NAME_QUERY_RESPONSE.AddInt32(1);
                        client.Send(ref SMSG_NAME_QUERY_RESPONSE);
                    }
                    finally
                    {
                        SMSG_NAME_QUERY_RESPONSE.Dispose();
                    }
                    break;
                default:
                    if (WorldServiceLocator.CommonGlobalFunctions.GuidIsPlayer(GUID))
                    {
                        if (WorldServiceLocator.WorldServer.CHARACTERs.ContainsKey(GUID))
                        {
                            try
                            {
                                SMSG_NAME_QUERY_RESPONSE.AddUInt64(GUID);
                                SMSG_NAME_QUERY_RESPONSE.AddString(WorldServiceLocator.WorldServer.CHARACTERs[GUID].Name);
                                SMSG_NAME_QUERY_RESPONSE.AddInt32((int)WorldServiceLocator.WorldServer.CHARACTERs[GUID].Race);
                                SMSG_NAME_QUERY_RESPONSE.AddInt32((int)WorldServiceLocator.WorldServer.CHARACTERs[GUID].Gender);
                                SMSG_NAME_QUERY_RESPONSE.AddInt32((int)WorldServiceLocator.WorldServer.CHARACTERs[GUID].Classe);
                                client.Send(ref SMSG_NAME_QUERY_RESPONSE);
                            }
                            finally
                            {
                                SMSG_NAME_QUERY_RESPONSE.Dispose();
                            }
                            return;
                        }
                        DataTable MySQLQuery = new();
                        WorldServiceLocator.WorldServer.CharacterDatabase.Query($"SELECT char_name, char_race, char_class, char_gender FROM characters WHERE char_guid = \"{GUID}\";", ref MySQLQuery);
                        switch (MySQLQuery.Rows.Count)
                        {
                            case > 0:
                                try
                                {
                                    SMSG_NAME_QUERY_RESPONSE.AddUInt64(GUID);
                                    SMSG_NAME_QUERY_RESPONSE.AddString(MySQLQuery.Rows[0].As<string>("char_name"));
                                    SMSG_NAME_QUERY_RESPONSE.AddInt32(MySQLQuery.Rows[0].As<int>("char_race"));
                                    SMSG_NAME_QUERY_RESPONSE.AddInt32(MySQLQuery.Rows[0].As<int>("char_gender"));
                                    SMSG_NAME_QUERY_RESPONSE.AddInt32(MySQLQuery.Rows[0].As<int>("char_class"));
                                    client.Send(ref SMSG_NAME_QUERY_RESPONSE);
                                }
                                finally
                                {
                                    SMSG_NAME_QUERY_RESPONSE.Dispose();
                                }
                                break;
                            default:
                                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_NAME_QUERY_RESPONSE [Character GUID={2:X} not found]", client.IP, client.Port, GUID);
                                break;
                        }
                        MySQLQuery.Dispose();
                    }
                    else
                    {
                        if (!WorldServiceLocator.CommonGlobalFunctions.GuidIsCreature(GUID))
                        {
                            return;
                        }
                        if (WorldServiceLocator.WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
                        {
                            try
                            {
                                SMSG_NAME_QUERY_RESPONSE.AddUInt64(GUID);
                                SMSG_NAME_QUERY_RESPONSE.AddString(WorldServiceLocator.WorldServer.WORLD_CREATUREs[GUID].Name);
                                SMSG_NAME_QUERY_RESPONSE.AddInt32(0);
                                SMSG_NAME_QUERY_RESPONSE.AddInt32(0);
                                SMSG_NAME_QUERY_RESPONSE.AddInt32(0);
                                client.Send(ref SMSG_NAME_QUERY_RESPONSE);
                            }
                            finally
                            {
                                SMSG_NAME_QUERY_RESPONSE.Dispose();
                            }
                        }
                        else
                        {
                            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_NAME_QUERY_RESPONSE [Creature GUID={2:X} not found]", client.IP, client.Port, GUID);
                        }
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at name query.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_TUTORIAL_FLAG(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 >= 9)
            {
                packet.GetInt16();
                var Flag = packet.GetInt32();
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TUTORIAL_FLAG [flag={2}]", client.IP, client.Port, Flag);
                client.Character.TutorialFlags[Flag / 8] = (byte)(client.Character.TutorialFlags[Flag / 8] + (1 << (7 - (Flag % 8))));
                client.Character.SaveCharacter();
            }
        }
    }

    public void On_CMSG_TUTORIAL_CLEAR(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TUTORIAL_CLEAR", client.IP, client.Port);
        var i = 0;
        do
        {
            client.Character.TutorialFlags[i] = byte.MaxValue;
            i = checked(i + 1);
        }
        while (i <= 31);
        client.Character.SaveCharacter();
    }

    public void On_CMSG_TUTORIAL_RESET(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TUTORIAL_RESET", client.IP, client.Port);
        var i = 0;
        do
        {
            client.Character.TutorialFlags[i] = 0;
            i = checked(i + 1);
        }
        while (i <= 31);
        client.Character.SaveCharacter();
    }

    public void On_CMSG_TOGGLE_HELM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TOGGLE_HELM", client.IP, client.Port);
        if ((client.Character.cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_HELM) != 0)
        {
            client.Character.cPlayerFlags &= ~PlayerFlags.PLAYER_FLAGS_HIDE_HELM;
        }
        else
        {
            client.Character.cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_HIDE_HELM;
        }
        client.Character.SetUpdateFlag(190, (int)client.Character.cPlayerFlags);
        client.Character.SendCharacterUpdate();
    }

    public void On_CMSG_TOGGLE_CLOAK(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TOGGLE_CLOAK", client.IP, client.Port);
        if ((client.Character.cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK) != 0)
        {
            client.Character.cPlayerFlags &= ~PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK;
        }
        else
        {
            client.Character.cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK;
        }
        client.Character.SetUpdateFlag(190, (int)client.Character.cPlayerFlags);
        client.Character.SendCharacterUpdate();
    }

    public void On_CMSG_SET_ACTIONBAR_TOGGLES(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var ActionBar = packet.GetInt8();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTIONBAR_TOGGLES [{2:X}]", client.IP, client.Port, ActionBar);
        client.Character.cPlayerFieldBytes = (client.Character.cPlayerFieldBytes & -983041) | (byte)(ActionBar << (0x10 & 7));
        client.Character.SetUpdateFlag(1222, client.Character.cPlayerFieldBytes);
        client.Character.SendCharacterUpdate();
    }

    public void On_CMSG_MOUNTSPECIAL_ANIM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MOUNTSPECIAL_ANIM", client.IP, client.Port);
        Packets.PacketClass response = new(Opcodes.SMSG_MOUNTSPECIAL_ANIM);
        try
        {
            response.AddPackGUID(client.Character.GUID);
            client.Character.SendToNearPlayers(ref response);
        }
        finally
        {
            response.Dispose();
        }
    }

    public void On_CMSG_EMOTE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 9)
        {
            packet.GetInt16();
            var emoteID = packet.GetInt32();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_EMOTE [{2}]", client.IP, client.Port, emoteID);
            Packets.PacketClass response = new(Opcodes.SMSG_EMOTE);
            try
            {
                response.AddInt32(emoteID);
                response.AddUInt64(client.Character.GUID);
                client.Character.SendToNearPlayers(ref response);
            }
            finally
            {
                response.Dispose();
            }
        }
    }

    public void On_CMSG_TEXT_EMOTE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 21 && client.Character != null)
            {
                return;
            }
            packet.GetInt16();
            var TextEmote = packet.GetInt32();
            var Unk = packet.GetInt32();
            var GUID = packet.GetUInt64();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TEXT_EMOTE [TextEmote={2} Unk={3}]", client.IP, client.Port, TextEmote, Unk);
            if (WorldServiceLocator.CommonGlobalFunctions.GuidIsCreature(GUID) && WorldServiceLocator.WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
            {
                ref var character = ref client.Character;
                ulong key;
                Dictionary<ulong, WS_Creatures.CreatureObject> WORLD_CREATUREs;
                var creature = (WORLD_CREATUREs = WorldServiceLocator.WorldServer.WORLD_CREATUREs)[key = GUID];
                WorldServiceLocator.WorldServer.ALLQUESTS.OnQuestDoEmote(ref character, ref creature, TextEmote);
                WORLD_CREATUREs[key] = creature;
                if (WorldServiceLocator.WorldServer.WORLD_CREATUREs[GUID].aiScript is not null and WS_Creatures_AI.GuardAI)
                {
                    ((WS_Creatures_AI.GuardAI)WorldServiceLocator.WorldServer.WORLD_CREATUREs[GUID].aiScript).OnEmote(TextEmote);
                }
            }
            if (WorldServiceLocator.WSDBCDatabase.EmotesText.ContainsKey(TextEmote))
            {
                switch (WorldServiceLocator.WSDBCDatabase.EmotesState[WorldServiceLocator.WSDBCDatabase.EmotesText[TextEmote]])
                {
                    case 0:
                        client.Character.DoEmote(WorldServiceLocator.WSDBCDatabase.EmotesText[TextEmote]);
                        break;
                    case 2:
                        client.Character.cEmoteState = WorldServiceLocator.WSDBCDatabase.EmotesText[TextEmote];
                        client.Character.SetUpdateFlag(148, client.Character.cEmoteState);
                        client.Character.SendCharacterUpdate();
                        break;
                    default:
                        break;
                }
            }
            var secondName = "";
            if (decimal.Compare(new decimal(GUID), 0m) > 0)
            {
                if (WorldServiceLocator.WorldServer.CHARACTERs.ContainsKey(GUID))
                {
                    secondName = WorldServiceLocator.WorldServer.CHARACTERs[GUID].Name;
                }
                else if (WorldServiceLocator.WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
                {
                    secondName = WorldServiceLocator.WorldServer.WORLD_CREATUREs[GUID].Name;
                }
            }
            Packets.PacketClass SMSG_TEXT_EMOTE = new(Opcodes.SMSG_TEXT_EMOTE);
            try
            {
                SMSG_TEXT_EMOTE.AddUInt64(client.Character.GUID);
                SMSG_TEXT_EMOTE.AddInt32(TextEmote);
                SMSG_TEXT_EMOTE.AddInt32(255);
                SMSG_TEXT_EMOTE.AddInt32(secondName.Length + 1);
                SMSG_TEXT_EMOTE.AddString(secondName);
                client.Character.SendToNearPlayers(ref SMSG_TEXT_EMOTE);
            }
            finally
            {
                SMSG_TEXT_EMOTE.Dispose();
            }
        }
    }

    public void On_MSG_CORPSE_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (decimal.Compare(new decimal(client.Character.corpseGUID), 0m) != 0)
        {
            Packets.PacketClass MSG_CORPSE_QUERY = new(Opcodes.MSG_CORPSE_QUERY);
            try
            {
                MSG_CORPSE_QUERY.AddInt8(1);
                MSG_CORPSE_QUERY.AddInt32(checked((int)client.Character.MapID));
                MSG_CORPSE_QUERY.AddSingle(client.Character.corpsePositionX);
                MSG_CORPSE_QUERY.AddSingle(client.Character.corpsePositionY);
                MSG_CORPSE_QUERY.AddSingle(client.Character.corpsePositionZ);
                MSG_CORPSE_QUERY.AddInt32(client.Character.corpseMapID);
                client.Send(ref MSG_CORPSE_QUERY);
            }
            finally
            {
                MSG_CORPSE_QUERY.Dispose();
            }
            Packets.PacketClass MSG_MINIMAP_PING = new(Opcodes.MSG_MINIMAP_PING);
            try
            {
                MSG_MINIMAP_PING.AddUInt64(client.Character.corpseGUID);
                MSG_MINIMAP_PING.AddSingle(client.Character.corpsePositionX);
                MSG_MINIMAP_PING.AddSingle(client.Character.corpsePositionY);
                client.Send(ref MSG_MINIMAP_PING);
            }
            finally
            {
                MSG_MINIMAP_PING.Dispose();
            }
        }
    }

    public void On_CMSG_REPOP_REQUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_REPOP_REQUEST [GUID={2:X}]", client.IP, client.Port, client.Character.GUID);
        if (client.Character.repopTimer != null)
        {
            client.Character.repopTimer.Dispose();
            client.Character.repopTimer = null;
        }
        CharacterRepop(ref client);
    }

    public void On_CMSG_RECLAIM_CORPSE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_RECLAIM_CORPSE [GUID={2:X}]", client.IP, client.Port, GUID);
            CharacterResurrect(ref client.Character);
        }
    }

    public void CharacterRepop(ref WS_Network.ClientClass client)
    {
        try
        {
            if (client.Character is null)
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1} Account:{2} CharName:{3} CharGUID:{4}] Client is Null!", client.IP, client.Port, client.Account, client.Character.UnitName, client.Character.GUID);
                return;
            }
            client.Character.Mana.Current = 0;
            client.Character.Rage.Current = 0;
            client.Character.Energy.Current = 0;
            client.Character.Life.Current = 1;
            client.Character.DEAD = true;
            client.Character.cUnitFlags = 8;
            client.Character.cDynamicFlags = 0;
            client.Character.cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_DEAD;
            WorldServiceLocator.Functions.SendCorpseReclaimDelay(ref client, ref client.Character);
            client.Character.StopMirrorTimer(MirrorTimer.FATIGUE);
            client.Character.StopMirrorTimer(MirrorTimer.DROWNING);
            if (client.Character.underWaterTimer != null)
            {
                client.Character.underWaterTimer.Dispose();
                client.Character.underWaterTimer = null;
            }
            WS_Corpses.CorpseObject myCorpse = new(ref client.Character);
            myCorpse.Save();
            myCorpse.AddToWorld();
            client.Character.Invisibility = InvisibilityLevel.DEAD;
            client.Character.CanSeeInvisibility = InvisibilityLevel.DEAD;
            WorldServiceLocator.WSCharMovement.UpdateCell(ref client.Character);
            checked
            {
                for (var i = 0; i <= WorldServiceLocator.GlobalConstants.MAX_AURA_EFFECTs - 1; i++)
                {
                    if (client.Character.ActiveSpells[i] != null)
                    {
                        client.Character.RemoveAura(i, ref client.Character.ActiveSpells[i].SpellCaster);
                    }
                }
                client.Character.SetWaterWalk();
                client.Character.SetMoveUnroot();
                if (client.Character.Race == Races.RACE_NIGHT_ELF)
                {
                    client.Character.ApplySpell(20584);
                }
                else
                {
                    client.Character.ApplySpell(8326);
                }
                client.Character.SetUpdateFlag(22, 1);
            }
            client.Character.SetUpdateFlag((int)checked(23 + client.Character.ManaType), 0);
            client.Character.SetUpdateFlag(190, (int)client.Character.cPlayerFlags);
            client.Character.SetUpdateFlag(46, client.Character.cUnitFlags);
            client.Character.SetUpdateFlag(143, client.Character.cDynamicFlags);
            client.Character.SetUpdateFlag(138, 16777216);
            client.Character.SendCharacterUpdate();
            WorldServiceLocator.WorldServer.AllGraveYards.GoToNearestGraveyard(ref client.Character, Alive: false, Teleport: true);
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.FAILED, "Error on repop: {0}", e.ToString());
        }
    }

    public void CharacterResurrect(ref WS_PlayerData.CharacterObject Character)
    {
        if (Character.repopTimer != null)
        {
            Character.repopTimer.Dispose();
            Character.repopTimer = null;
        }
        Character.Mana.Current = 0;
        Character.Rage.Current = 0;
        Character.Energy.Current = 0;
        Character.Life.Current = checked((int)Math.Round(Character.Life.Maximum / 2.0));
        Character.DEAD = false;
        Character.cPlayerFlags &= ~PlayerFlags.PLAYER_FLAGS_DEAD;
        Character.cUnitFlags = 8;
        Character.cDynamicFlags = 0;
        Character.InvisibilityReset();
        WorldServiceLocator.WSCharMovement.UpdateCell(ref Character);
        Character.SetLandWalk();
        if (Character.Race == Races.RACE_NIGHT_ELF)
        {
            Character.RemoveAuraBySpell(20584);
        }
        else
        {
            Character.RemoveAuraBySpell(8326);
        }
        Character.SetUpdateFlag(22, Character.Life.Current);
        Character.SetUpdateFlag(190, (int)Character.cPlayerFlags);
        Character.SetUpdateFlag(46, Character.cUnitFlags);
        Character.SetUpdateFlag(143, Character.cDynamicFlags);
        Character.SendCharacterUpdate();
        if (decimal.Compare(new decimal(Character.corpseGUID), 0m) != 0)
        {
            if (WorldServiceLocator.WorldServer.WORLD_CORPSEOBJECTs.ContainsKey(Character.corpseGUID))
            {
                WorldServiceLocator.WorldServer.WORLD_CORPSEOBJECTs[Character.corpseGUID].ConvertToBones();
            }
            else
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "Corpse wasn't found [{0}]!", checked(Character.corpseGUID - WorldServiceLocator.GlobalConstants.GUID_CORPSE));
                WorldServiceLocator.WorldServer.CharacterDatabase.Update($"DELETE FROM corpse WHERE player = \"{Character.GUID}\";");
            }
            Character.corpseGUID = 0uL;
            Character.corpseMapID = 0;
            Character.corpsePositionX = 0f;
            Character.corpsePositionY = 0f;
            Character.corpsePositionZ = 0f;
        }
    }

    public void On_CMSG_TOGGLE_PVP(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TOGGLE_PVP", client.IP, client.Port);
        client.Character.IsPvP = !client.Character.IsPvP;
        client.Character.SetUpdateFlag(46, client.Character.cUnitFlags);
        client.Character.SendCharacterUpdate();
    }

    public void On_MSG_INSPECT_HONOR_STATS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 13)
        {
            return;
        }
        packet.GetInt16();
        var GUID = packet.GetUInt64();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_INSPECT_HONOR_STATS [{2:X}]", client.IP, client.Port, GUID);
        if (WorldServiceLocator.WorldServer.CHARACTERs.ContainsKey(GUID))
        {
            Packets.PacketClass response = new(Opcodes.MSG_INSPECT_HONOR_STATS);
            try
            {
                response.AddUInt64(GUID);
                WorldServiceLocator.WorldServer.CHARACTERs_Lock.EnterReadLock();
                response.AddInt8((byte)WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorRank);
                response.AddInt32(checked(WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorKillsToday + WorldServiceLocator.WorldServer.CHARACTERs[GUID].DishonorKillsToday) << 16);
                response.AddInt32(WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorKillsYesterday);
                response.AddInt32(WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorKillsLastWeek);
                response.AddInt32(WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorKillsThisWeek);
                response.AddInt32(WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorKillsLifeTime);
                response.AddInt32(WorldServiceLocator.WorldServer.CHARACTERs[GUID].DishonorKillsLifeTime);
                response.AddInt32(WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorPointsYesterday);
                response.AddInt32(WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorPointsLastWeek);
                response.AddInt32(WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorPointsThisWeek);
                response.AddInt32(WorldServiceLocator.WorldServer.CHARACTERs[GUID].StandingLastWeek);
                response.AddInt8((byte)WorldServiceLocator.WorldServer.CHARACTERs[GUID].HonorHighestRank);
                WorldServiceLocator.WorldServer.CHARACTERs_Lock.ExitReadLock();
                client.Send(ref response);
            }
            finally
            {
                response.Dispose();
            }
        }
    }

    public void On_CMSG_MOVE_FALL_RESET(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MOVE_FALL_RESET", client.IP, client.Port);
        WS_Network.ClientClass client2 = null;
        WorldServiceLocator.Packets.DumpPacket(packet.Data, client2);
    }

    public void On_CMSG_BATTLEFIELD_STATUS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BATTLEFIELD_STATUS", client.IP, client.Port);
    }

    public void On_CMSG_SET_ACTIVE_MOVER(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var GUID = packet.GetUInt64();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTIVE_MOVER [GUID={2:X}]", client.IP, client.Port, GUID);
    }

    public void On_CMSG_MEETINGSTONE_INFO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MEETINGSTONE_INFO", client.IP, client.Port);
    }

    public void On_CMSG_SET_FACTION_ATWAR(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var faction = packet.GetInt32();
        var enabled = packet.GetInt8();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_FACTION_ATWAR [faction={2:X} enabled={3}]", client.IP, client.Port, faction, enabled);
        if (enabled <= 1)
        {
            client.Character.Reputation[faction].Flags = enabled == 1 ? client.Character.Reputation[faction].Flags | 2 : client.Character.Reputation[faction].Flags & -3;
            Packets.PacketClass response = new(Opcodes.SMSG_SET_FACTION_STANDING);
            try
            {
                response.AddInt32(client.Character.Reputation[faction].Flags);
                response.AddInt32(faction);
                response.AddInt32(client.Character.Reputation[faction].Value);
                client.Send(ref response);
            }
            finally
            {
                response.Dispose();
            }
        }
    }

    public void On_CMSG_SET_FACTION_INACTIVE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var faction = packet.GetInt32();
        var enabled = packet.GetInt8();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_FACTION_INACTIVE [faction={2:X} enabled={3}]", client.IP, client.Port, faction, enabled);
        if (enabled <= 1)
        {
        }
    }

    public void On_CMSG_SET_WATCHED_FACTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var faction = packet.GetInt32();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_WATCHED_FACTION [faction={2:X}]", client.IP, client.Port, faction);
        if (faction == -1)
        {
            faction = 255;
        }
        if (faction is >= 0 and <= 255)
        {
            client.Character.WatchedFactionIndex = checked((byte)faction);
            client.Character.SetUpdateFlag(1261, faction);
            client.Character.SendCharacterUpdate(toNear: false);
        }
    }

    public void On_MSG_PVP_LOG_DATA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (WorldServiceLocator.WSMaps.Maps[client.Character.MapID].IsBattleGround)
        {
        }
    }

    public void On_CMSG_COMPLETE_CINEMATIC(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_COMPLETE_CINEMATIC", client.IP, client.Port);
    }

    public void On_CMSG_NEXT_CINEMATIC_CAMERA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NEXT_CINEMATIC_CAMERA", client.IP, client.Port);
    }

    public void On_CMSG_OPENING_CINEMATIC(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_OPENING_CINEMATIC", client.IP, client.Port);
        if (client.Character == null) return;

        byte cinematicId = client.Character.Race switch
        {
            Races.RACE_HUMAN => 81,
            Races.RACE_ORC => 21,
            Races.RACE_DWARF => 41,
            Races.RACE_NIGHT_ELF => 61,
            Races.RACE_UNDEAD => 2,
            Races.RACE_TAUREN => 141,
            Races.RACE_GNOME => 101,
            Races.RACE_TROLL => 121,
            _ => 0,
        };

        if (cinematicId > 0)
        {
            Packets.PacketClass response = new(Opcodes.SMSG_TRIGGER_CINEMATIC);
            try
            {
                response.AddInt32(cinematicId);
                client.Send(ref response);
            }
            finally
            {
                response.Dispose();
            }
        }
    }

    public void On_CMSG_FAR_SIGHT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var enable = packet.GetInt8();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_FAR_SIGHT [enable={2}]", client.IP, client.Port, enable);
    }

    public void On_CMSG_SELF_RES(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SELF_RES [GUID={2:X}]", client.IP, client.Port, client.Character.GUID);
        try
        {
            if (client.Character == null || !client.Character.DEAD)
            {
                return;
            }
            CharacterResurrect(ref client.Character);
            client.Character.Life.Current = checked((int)Math.Round(client.Character.Life.Maximum * 0.20));
            client.Character.Mana.Current = checked((int)Math.Round(client.Character.Mana.Maximum * 0.20));
            client.Character.SetUpdateFlag(22, client.Character.Life.Current);
            client.Character.SetUpdateFlag(23, client.Character.Mana.Current);
            client.Character.SendCharacterUpdate();
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at self res.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_UNLEARN_SKILL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 9)
            {
                return;
            }
            packet.GetInt16();
            var skillId = packet.GetInt32();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_UNLEARN_SKILL [skill={2}]", client.IP, client.Port, skillId);
            if (client.Character == null)
            {
                return;
            }
            if (!client.Character.Skills.ContainsKey(skillId))
            {
                return;
            }
            client.Character.Skills.Remove(skillId);
            if (client.Character.SkillsPositions.ContainsKey(skillId))
            {
                var pos = client.Character.SkillsPositions[skillId];
                client.Character.SkillsPositions.Remove(skillId);
                checked
                {
                    client.Character.SetUpdateFlag(718 + (pos * 3), 0);
                    client.Character.SetUpdateFlag(718 + (pos * 3) + 1, 0);
                    client.Character.SetUpdateFlag(718 + (pos * 3) + 2, 0);
                }
            }
            client.Character.SendCharacterUpdate();
            client.Character.SaveCharacter();
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at unlearn skill.{0}", Environment.NewLine + e);
        }
    }

    public void On_MSG_RANDOM_ROLL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 13)
            {
                return;
            }
            packet.GetInt16();
            var minimum = packet.GetInt32();
            var maximum = packet.GetInt32();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_RANDOM_ROLL [{2}-{3}]", client.IP, client.Port, minimum, maximum);
            if (client.Character == null)
            {
                return;
            }
            if (minimum < 0)
            {
                minimum = 0;
            }
            if (maximum < 1)
            {
                maximum = 1;
            }
            if (minimum > maximum)
            {
                minimum = 0;
            }
            var rollResult = WorldServiceLocator.WorldServer.Rnd.Next(minimum, checked(maximum + 1));
            Packets.PacketClass response = new(Opcodes.MSG_RANDOM_ROLL);
            try
            {
                response.AddInt32(minimum);
                response.AddInt32(maximum);
                response.AddInt32(rollResult);
                response.AddUInt64(client.Character.GUID);
                if (client.Character.IsInGroup)
                {
                    client.Character.Group.Broadcast(response);
                }
                else
                {
                    client.Send(ref response);
                }
            }
            finally
            {
                response.Dispose();
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at random roll.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_CANCEL_GROWTH_AURA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_GROWTH_AURA", client.IP, client.Port);

        if (client.Character == null) return;

        checked
        {
            for (var i = 0; i <= WorldServiceLocator.GlobalConstants.MAX_AURA_EFFECTs - 1; i++)
            {
                if (client.Character.ActiveSpells[i] != null &&
                    WorldServiceLocator.WSSpells.SPELLs.ContainsKey(client.Character.ActiveSpells[i].SpellID))
                {
                    var spellInfo = WorldServiceLocator.WSSpells.SPELLs[client.Character.ActiveSpells[i].SpellID];
                    for (var j = 0; j < 3; j++)
                    {
                        if (spellInfo.SpellEffects[j] != null &&
                            spellInfo.SpellEffects[j].ApplyAuraIndex == 61)
                        {
                            client.Character.RemoveAura(i, ref client.Character.ActiveSpells[i].SpellCaster);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void On_CMSG_REQUEST_RAID_INFO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_REQUEST_RAID_INFO", client.IP, client.Port);

        Packets.PacketClass response = new(Opcodes.SMSG_RAID_INSTANCE_INFO);
        try
        {
            response.AddInt32(0);
            client.Send(ref response);
        }
        finally
        {
            response.Dispose();
        }
    }

    public void On_CMSG_RESET_INSTANCES(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_RESET_INSTANCES", client.IP, client.Port);
    }

    public void On_MSG_RAID_ICON_TARGET(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 6)
            {
                return;
            }
            packet.GetInt16();
            var iconSlot = packet.GetInt8();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_RAID_ICON_TARGET [slot={2}]", client.IP, client.Port, iconSlot);
            if (!client.Character.IsInGroup)
            {
                return;
            }
            if (iconSlot == 0xFF)
            {
                Packets.PacketClass response = new(Opcodes.MSG_RAID_ICON_TARGET);
                try
                {
                    response.AddInt8(1);
                    for (var i = 0; i < 8; i++)
                    {
                        response.AddUInt64(client.Character.Group.GetTargetIcon(i));
                    }
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }
            }
            else if (iconSlot < 8 && checked(packet.Data.Length - 1) >= 14)
            {
                var targetGuid = packet.GetUInt64();
                client.Character.Group.SetTargetIcon(iconSlot, targetGuid);
                Packets.PacketClass response = new(Opcodes.MSG_RAID_ICON_TARGET);
                try
                {
                    response.AddInt8(0);
                    response.AddInt8(iconSlot);
                    response.AddUInt64(targetGuid);
                    client.Character.Group.Broadcast(response);
                }
                finally
                {
                    response.Dispose();
                }
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at raid icon target.{0}", Environment.NewLine + e);
        }
    }

    public void On_MSG_RAID_READY_CHECK(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_RAID_READY_CHECK", client.IP, client.Port);
            if (!client.Character.IsInGroup)
            {
                return;
            }
            if (client.Character.IsGroupLeader)
            {
                Packets.PacketClass readyCheck = new(Opcodes.MSG_RAID_READY_CHECK);
                try
                {
                    readyCheck.AddUInt64(client.Character.GUID);
                    client.Character.Group.Broadcast(readyCheck);
                }
                finally
                {
                    readyCheck.Dispose();
                }
            }
            else
            {
                if (checked(packet.Data.Length - 1) < 6)
                {
                    return;
                }
                packet.GetInt16();
                var isReady = packet.GetInt8();
                Packets.PacketClass response = new(Opcodes.MSG_RAID_READY_CHECK);
                try
                {
                    response.AddUInt64(client.Character.GUID);
                    response.AddInt8(isReady);
                    client.Character.Group.Broadcast(response);
                }
                finally
                {
                    response.Dispose();
                }
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at raid ready check.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_PLAYED_TIME(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PLAYED_TIME", client.IP, client.Port);
        Packets.PacketClass response = new(Opcodes.SMSG_PLAYED_TIME);
        try
        {
            response.AddInt32(0);
            response.AddInt32(0);
            client.Send(ref response);
        }
        finally
        {
            response.Dispose();
        }
    }

    public void On_CMSG_INSPECT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 13)
        {
            return;
        }
        packet.GetInt16();
        var guid = packet.GetUInt64();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_INSPECT [GUID={2:X}]", client.IP, client.Port, guid);

        if (!WorldServiceLocator.WorldServer.CHARACTERs.ContainsKey(guid)) return;

        Packets.PacketClass response = new(Opcodes.SMSG_INSPECT);
        try
        {
            response.AddUInt64(guid);
            client.Send(ref response);
        }
        finally
        {
            response.Dispose();
        }
    }

    public void On_CMSG_SUMMON_RESPONSE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 14)
            {
                return;
            }
            packet.GetInt16();
            var summonerGuid = packet.GetUInt64();
            var accept = packet.GetInt8();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SUMMON_RESPONSE [summoner={2:X} accept={3}]", client.IP, client.Port, summonerGuid, accept);
            if (accept == 0)
            {
                return;
            }
            if (!WorldServiceLocator.WorldServer.CHARACTERs.ContainsKey(summonerGuid))
            {
                return;
            }
            var summoner = WorldServiceLocator.WorldServer.CHARACTERs[summonerGuid];
            client.Character.Teleport(summoner.positionX, summoner.positionY, summoner.positionZ, summoner.orientation, checked((int)summoner.MapID));
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at summon response.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_LOOT_MASTER_GIVE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 22)
            {
                return;
            }
            packet.GetInt16();
            var lootGuid = packet.GetUInt64();
            var slotId = packet.GetInt8();
            var playerGuid = packet.GetUInt64();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_MASTER_GIVE [loot={2:X} slot={3} player={4:X}]", client.IP, client.Port, lootGuid, slotId, playerGuid);
            if (!client.Character.IsInGroup)
            {
                return;
            }
            if (client.Character.Group.LootMethod != GroupLootMethod.LOOT_MASTER)
            {
                return;
            }
            if (client.Character.Group.LocalLootMaster != client.Character)
            {
                return;
            }
            if (!WorldServiceLocator.WSLoot.LootTable.ContainsKey(lootGuid))
            {
                return;
            }
            if (!WorldServiceLocator.WorldServer.CHARACTERs.ContainsKey(playerGuid))
            {
                return;
            }
            var lootObject = WorldServiceLocator.WSLoot.LootTable[lootGuid];
            if (slotId >= lootObject.Items.Count || lootObject.Items[slotId] == null)
            {
                return;
            }
            var targetCharacter = WorldServiceLocator.WorldServer.CHARACTERs[playerGuid];
            ItemObject tmpItem = new(lootObject.Items[slotId].ItemID, targetCharacter.GUID)
            {
                StackCount = lootObject.Items[slotId].ItemCount
            };
            if (targetCharacter.ItemADD(ref tmpItem))
            {
                if (tmpItem.ItemInfo.Bonding == 1)
                {
                    WS_Network.ClientClass client2 = null;
                    tmpItem.SoulbindItem(client2);
                }
                targetCharacter.LogLootItem(tmpItem, lootObject.Items[slotId].ItemCount, Recieved: false, Created: false);
                Packets.PacketClass removed = new(Opcodes.SMSG_LOOT_REMOVED);
                try
                {
                    removed.AddInt8(slotId);
                    client.Send(ref removed);
                }
                finally
                {
                    removed.Dispose();
                }
                lootObject.Items[slotId].Dispose();
                lootObject.Items[slotId] = null;
            }
            else
            {
                tmpItem.Delete();
                Packets.PacketClass response = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                try
                {
                    response.AddInt8(50);
                    response.AddUInt64(0uL);
                    response.AddUInt64(0uL);
                    response.AddInt8(0);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at loot master give.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_SET_EXPLORATION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 10)
        {
            return;
        }
        packet.GetInt16();
        var areaFlag = packet.GetInt32();
        var explored = packet.GetInt8();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_EXPLORATION [flag={2} explored={3}]", client.IP, client.Port, areaFlag, explored);
    }

    public void On_CMSG_CHAT_IGNORED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 13)
        {
            return;
        }
        packet.GetInt16();
        var guid = packet.GetUInt64();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAT_IGNORED [GUID={2:X}]", client.IP, client.Port, guid);

        if (WorldServiceLocator.WorldServer.CHARACTERs.ContainsKey(guid) &&
            WorldServiceLocator.WorldServer.CHARACTERs[guid].client != null)
        {
            Packets.PacketClass response = new(Opcodes.SMSG_CHAT_PLAYER_NOT_FOUND);
            try
            {
                response.AddString(client.Character.Name);
                var targetClient = WorldServiceLocator.WorldServer.CHARACTERs[guid].client;
                targetClient.Send(ref response);
            }
            finally
            {
                response.Dispose();
            }
        }
    }

}
