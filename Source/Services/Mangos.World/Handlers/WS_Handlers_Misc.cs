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
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Mangos.World.AI;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Handlers
{
    public class WS_Handlers_Misc
    {

        // Public Function SelectMonsterSay(ByVal MonsterID As Integer) As String
        // ' Select Random Text Field From Monster Say HashTable(s)
        // ' TODO: Allow This To Work With Different Monster Say Events Besides Combat
        // Dim TextCount As Integer = 0
        // Dim RandomText As Integer = 0

        // If Trim((MonsterSayCombat(MonsterID)).Text0) <> "" Then TextCount += 1
        // If Trim((MonsterSayCombat(MonsterID)).Text1) <> "" Then TextCount += 1
        // If Trim((MonsterSayCombat(MonsterID)).Text2) <> "" Then TextCount += 1
        // If Trim((MonsterSayCombat(MonsterID)).Text3) <> "" Then TextCount += 1
        // If Trim((MonsterSayCombat(MonsterID)).Text4) <> "" Then TextCount += 1

        // RandomText = Rnd.Next(1, TextCount + 1)

        // SelectMonsterSay = ""

        // Select Case RandomText
        // Case 1
        // SelectMonsterSay = (MonsterSayCombat(MonsterID)).Text0
        // Case 2
        // SelectMonsterSay = (MonsterSayCombat(MonsterID)).Text1
        // Case 3
        // SelectMonsterSay = (MonsterSayCombat(MonsterID)).Text2
        // Case 4
        // SelectMonsterSay = (MonsterSayCombat(MonsterID)).Text3
        // Case 5
        // SelectMonsterSay = (MonsterSayCombat(MonsterID)).Text4
        // End Select

        // End Function

        public void On_CMSG_NAME_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                if (packet.Data.Length - 1 < 13)
                    return;
                packet.GetInt16();
                ulong GUID = packet.GetUInt64();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NAME_QUERY [GUID={2:X}]", client.IP, client.Port, GUID);
                var SMSG_NAME_QUERY_RESPONSE = new Packets.PacketClass(OPCODES.SMSG_NAME_QUERY_RESPONSE);

                // RESERVED For Warden Bot
                if (GUID == WS_Commands.SystemGUID)
                {
                    try
                    {
                        SMSG_NAME_QUERY_RESPONSE.AddUInt64(GUID);
                        SMSG_NAME_QUERY_RESPONSE.AddString(WS_Commands.SystemNAME);
                        SMSG_NAME_QUERY_RESPONSE.AddInt32(1);
                        SMSG_NAME_QUERY_RESPONSE.AddInt32(1);
                        SMSG_NAME_QUERY_RESPONSE.AddInt32(1);
                        client.Send(SMSG_NAME_QUERY_RESPONSE);
                    }
                    finally
                    {
                        SMSG_NAME_QUERY_RESPONSE.Dispose();
                    }

                    return;
                }

                // Asking for player name
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(GUID))
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID) == true)
                    {
                        try
                        {
                            SMSG_NAME_QUERY_RESPONSE.AddUInt64(GUID);
                            SMSG_NAME_QUERY_RESPONSE.AddString(WorldServiceLocator._WorldServer.CHARACTERs[GUID].Name);
                            SMSG_NAME_QUERY_RESPONSE.AddInt32((int)WorldServiceLocator._WorldServer.CHARACTERs[GUID].Race);
                            SMSG_NAME_QUERY_RESPONSE.AddInt32((int)WorldServiceLocator._WorldServer.CHARACTERs[GUID].Gender);
                            SMSG_NAME_QUERY_RESPONSE.AddInt32((int)WorldServiceLocator._WorldServer.CHARACTERs[GUID].Classe);
                            client.Send(SMSG_NAME_QUERY_RESPONSE);
                        }
                        finally
                        {
                            SMSG_NAME_QUERY_RESPONSE.Dispose();
                        }

                        return;
                    }
                    else
                    {
                        var MySQLQuery = new DataTable();
                        WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT char_name, char_race, char_class, char_gender FROM characters WHERE char_guid = \"{0}\";", (object)GUID), MySQLQuery);
                        if (MySQLQuery.Rows.Count > 0)
                        {
                            try
                            {
                                SMSG_NAME_QUERY_RESPONSE.AddUInt64(GUID);
                                SMSG_NAME_QUERY_RESPONSE.AddString(Conversions.ToString(MySQLQuery.Rows[0]["char_name"]));
                                SMSG_NAME_QUERY_RESPONSE.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[0]["char_race"]));
                                SMSG_NAME_QUERY_RESPONSE.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[0]["char_gender"]));
                                SMSG_NAME_QUERY_RESPONSE.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[0]["char_class"]));
                                client.Send(SMSG_NAME_QUERY_RESPONSE);
                            }
                            finally
                            {
                                SMSG_NAME_QUERY_RESPONSE.Dispose();
                            }
                        }
                        else
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_NAME_QUERY_RESPONSE [Character GUID={2:X} not found]", client.IP, client.Port, GUID);
                        }

                        MySQLQuery.Dispose();
                        return;
                    }
                }

                // Asking for creature name (only used in quests?)
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
                {
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
                    {
                        try
                        {
                            SMSG_NAME_QUERY_RESPONSE.AddUInt64(GUID);
                            SMSG_NAME_QUERY_RESPONSE.AddString(WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Name);
                            SMSG_NAME_QUERY_RESPONSE.AddInt32(0);
                            SMSG_NAME_QUERY_RESPONSE.AddInt32(0);
                            SMSG_NAME_QUERY_RESPONSE.AddInt32(0);
                            client.Send(SMSG_NAME_QUERY_RESPONSE);
                        }
                        finally
                        {
                            SMSG_NAME_QUERY_RESPONSE.Dispose();
                        }
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_NAME_QUERY_RESPONSE [Creature GUID={2:X} not found]", client.IP, client.Port, GUID);
                    }
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at name query.{0}", Environment.NewLine + e.ToString());
            }
        }

        public void On_CMSG_TUTORIAL_FLAG(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int Flag = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TUTORIAL_FLAG [flag={2}]", client.IP, client.Port, Flag);
            client.Character.TutorialFlags[Flag / 8] = (byte)(client.Character.TutorialFlags[Flag / 8] + (1 << 7 - Flag % 8));
            client.Character.SaveCharacter();
        }

        public void On_CMSG_TUTORIAL_CLEAR(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TUTORIAL_CLEAR", client.IP, client.Port);
            for (int i = 0; i <= 31; i++)
                client.Character.TutorialFlags[i] = 255;
            client.Character.SaveCharacter();
        }

        public void On_CMSG_TUTORIAL_RESET(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TUTORIAL_RESET", client.IP, client.Port);
            for (int i = 0; i <= 31; i++)
                client.Character.TutorialFlags[i] = 0;
            client.Character.SaveCharacter();
        }

        public void On_CMSG_TOGGLE_HELM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TOGGLE_HELM", client.IP, client.Port);
            if (client.Character.cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_HELM)
            {
                client.Character.cPlayerFlags = client.Character.cPlayerFlags & !PlayerFlags.PLAYER_FLAGS_HIDE_HELM;
            }
            else
            {
                client.Character.cPlayerFlags = client.Character.cPlayerFlags | PlayerFlags.PLAYER_FLAGS_HIDE_HELM;
            }

            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FLAGS, client.Character.cPlayerFlags);
            client.Character.SendCharacterUpdate(true);
        }

        public void On_CMSG_TOGGLE_CLOAK(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TOGGLE_CLOAK", client.IP, client.Port);
            if (client.Character.cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK)
            {
                client.Character.cPlayerFlags = client.Character.cPlayerFlags & !PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK;
            }
            else
            {
                client.Character.cPlayerFlags = client.Character.cPlayerFlags | PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK;
            }

            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FLAGS, client.Character.cPlayerFlags);
            client.Character.SendCharacterUpdate(true);
        }

        public void On_CMSG_SET_ACTIONBAR_TOGGLES(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            byte ActionBar = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTIONBAR_TOGGLES [{2:X}]", client.IP, client.Port, ActionBar);
            client.Character.cPlayerFieldBytes = (int)(client.Character.cPlayerFieldBytes & 0xFFF0FFFF | ActionBar << 16);
            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_BYTES, client.Character.cPlayerFieldBytes);
            client.Character.SendCharacterUpdate(true);
        }

        public void On_CMSG_MOUNTSPECIAL_ANIM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MOUNTSPECIAL_ANIM", client.IP, client.Port);
            var response = new Packets.PacketClass(OPCODES.SMSG_MOUNTSPECIAL_ANIM);
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
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int emoteID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_EMOTE [{2}]", client.IP, client.Port, emoteID);
            var response = new Packets.PacketClass(OPCODES.SMSG_EMOTE);
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

        public void On_CMSG_TEXT_EMOTE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 21)
                return;
            packet.GetInt16();
            int TextEmote = packet.GetInt32();
            int Unk = packet.GetInt32();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TEXT_EMOTE [TextEmote={2} Unk={3}]", client.IP, client.Port, TextEmote, Unk);
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
            {
                // DONE: Some quests needs emotes being done
                var tmp = WorldServiceLocator._WorldServer.WORLD_CREATUREs;
                var argcreature = tmp[GUID];
                WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestDoEmote(ref client.Character, ref argcreature, TextEmote);
                tmp[GUID] = argcreature;

                // DONE: Doing emotes to guards
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].aiScript is object && WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].aiScript is WS_Creatures_AI.GuardAI)
                {
                    ((WS_Creatures_AI.GuardAI)WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].aiScript).OnEmote(TextEmote);
                }
            }

            // DONE: Send Emote animation
            if (WorldServiceLocator._WS_DBCDatabase.EmotesText.ContainsKey(TextEmote))
            {
                if (WorldServiceLocator._WS_DBCDatabase.EmotesState[WorldServiceLocator._WS_DBCDatabase.EmotesText[TextEmote]] == 0)
                {
                    client.Character.DoEmote(WorldServiceLocator._WS_DBCDatabase.EmotesText[TextEmote]);
                }
                else if (WorldServiceLocator._WS_DBCDatabase.EmotesState[WorldServiceLocator._WS_DBCDatabase.EmotesText[TextEmote]] == 2)
                {
                    client.Character.cEmoteState = WorldServiceLocator._WS_DBCDatabase.EmotesText[TextEmote];
                    client.Character.SetUpdateFlag((int)EUnitFields.UNIT_NPC_EMOTESTATE, client.Character.cEmoteState);
                    client.Character.SendCharacterUpdate(true);
                }
            }

            // DONE: Find Creature/Player with the recv GUID
            string secondName = "";
            if (GUID > 0m)
            {
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
                {
                    secondName = WorldServiceLocator._WorldServer.CHARACTERs[GUID].Name;
                }
                else if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
                {
                    secondName = WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Name;
                }
            }

            var SMSG_TEXT_EMOTE = new Packets.PacketClass(OPCODES.SMSG_TEXT_EMOTE);
            try
            {
                SMSG_TEXT_EMOTE.AddUInt64(client.Character.GUID);
                SMSG_TEXT_EMOTE.AddInt32(TextEmote);
                SMSG_TEXT_EMOTE.AddInt32(0xFF);
                SMSG_TEXT_EMOTE.AddInt32(secondName.Length + 1);
                SMSG_TEXT_EMOTE.AddString(secondName);
                client.Character.SendToNearPlayers(ref SMSG_TEXT_EMOTE);
            }
            finally
            {
                SMSG_TEXT_EMOTE.Dispose();
            }
        }

        public void On_MSG_CORPSE_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (client.Character.corpseGUID == 0m)
                return;

            // TODO: Find out the proper structure of this packet or at least what is wrong with instances!

            // DONE: Send corpse coords
            var MSG_CORPSE_QUERY = new Packets.PacketClass(OPCODES.MSG_CORPSE_QUERY);
            try
            {
                MSG_CORPSE_QUERY.AddInt8(1);
                // 'MSG_CORPSE_QUERY.AddInt32(Client.Character.corpseMapID)
                MSG_CORPSE_QUERY.AddInt32((int)client.Character.MapID); // Without changing this from the above line, the corpse pointer on the minimap did not show
                // when I was on a different map at least.
                MSG_CORPSE_QUERY.AddSingle(client.Character.corpsePositionX);
                MSG_CORPSE_QUERY.AddSingle(client.Character.corpsePositionY);
                MSG_CORPSE_QUERY.AddSingle(client.Character.corpsePositionZ);
                // 'If client.Character.corpseMapID <> client.Character.MapID Then
                // '    MSG_CORPSE_QUERY.AddInt32(0)                '1-Normal 0-Corpse in instance
                // 'Else
                // '    MSG_CORPSE_QUERY.AddInt32(1)                '1-Normal 0-Corpse in instance
                // 'End If
                MSG_CORPSE_QUERY.AddInt32(client.Character.corpseMapID); // This change from the above lines, gets rid of the "You must enter the instance to recover your corpse."
                // message when you did not die in an instance, although I did not see it when I did die in the instance either, but I did rez upon reentrance into the instance.
                client.Send(MSG_CORPSE_QUERY);
            }
            finally
            {
                MSG_CORPSE_QUERY.Dispose();
            }

            // DONE: Send ping on minimap
            var MSG_MINIMAP_PING = new Packets.PacketClass(OPCODES.MSG_MINIMAP_PING);
            try
            {
                MSG_MINIMAP_PING.AddUInt64(client.Character.corpseGUID);
                MSG_MINIMAP_PING.AddSingle(client.Character.corpsePositionX);
                MSG_MINIMAP_PING.AddSingle(client.Character.corpsePositionY);
                client.Send(MSG_MINIMAP_PING);
            }
            finally
            {
                MSG_MINIMAP_PING.Dispose();
            }
        }

        public void On_CMSG_REPOP_REQUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_REPOP_REQUEST [GUID={2:X}]", client.IP, client.Port, client.Character.GUID);
            if (client.Character.repopTimer is object)
            {
                client.Character.repopTimer.Dispose();
                client.Character.repopTimer = null;
            }

            CharacterRepop(ref client);
        }

        public void On_CMSG_RECLAIM_CORPSE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_RECLAIM_CORPSE [GUID={2:X}]", client.IP, client.Port, GUID);
            CharacterResurrect(ref client.Character);
        }

        public void CharacterRepop(ref WS_Network.ClientClass client)
        {
            try
            {
                // DONE: Make really dead
                {
                    var withBlock = client.Character;
                    withBlock.Mana.Current = 0;
                    withBlock.Rage.Current = 0;
                    withBlock.Energy.Current = 0;
                    withBlock.Life.Current = 1;
                    withBlock.DEAD = true;
                    withBlock.cUnitFlags = 0x8;
                    withBlock.cDynamicFlags = 0;
                    withBlock.cPlayerFlags = client.Character.cPlayerFlags | PlayerFlags.PLAYER_FLAGS_DEAD;
                }

                WorldServiceLocator._Functions.SendCorpseReclaimDelay(ref client, ref client.Character, 30);

                // DONE: Clear some things like spells, flags and timers
                client.Character.StopMirrorTimer(MirrorTimer.FATIGUE);
                client.Character.StopMirrorTimer(MirrorTimer.DROWNING);
                if (client.Character.underWaterTimer is object)
                {
                    client.Character.underWaterTimer.Dispose();
                    client.Character.underWaterTimer = null;
                }

                // DONE: Spawn Corpse
                var myCorpse = new WS_Corpses.CorpseObject(ref client.Character);
                myCorpse.Save();
                myCorpse.AddToWorld();

                // DONE: Update to see only dead
                client.Character.Invisibility = InvisibilityLevel.DEAD;
                client.Character.CanSeeInvisibility = InvisibilityLevel.DEAD;
                WorldServiceLocator._WS_CharMovement.UpdateCell(ref client.Character);

                // DONE: Remove all auras
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1; i <= loopTo; i++)
                {
                    if (client.Character.ActiveSpells[i] is object)
                        client.Character.RemoveAura(i, ref client.Character.ActiveSpells[i].SpellCaster);
                }

                // DONE: Ghost aura
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

                client.Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, 1);
                client.Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1 + client.Character.ManaType, 0);
                client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FLAGS, client.Character.cPlayerFlags);
                client.Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, client.Character.cUnitFlags);
                client.Character.SetUpdateFlag((int)EUnitFields.UNIT_DYNAMIC_FLAGS, client.Character.cDynamicFlags);
                client.Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_BYTES_1, 0x1000000);       // Set standing so always be standing
                client.Character.SendCharacterUpdate();

                // DONE: Get closest graveyard
                WorldServiceLocator._WorldServer.AllGraveYards.GoToNearestGraveyard(ref client.Character, false, true);
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error on repop: {0}", e.ToString());
            }
        }

        public void CharacterResurrect(ref WS_PlayerData.CharacterObject Character)
        {
            if (Character.repopTimer is object)
            {
                Character.repopTimer.Dispose();
                Character.repopTimer = null;
            }

            // DONE: Make really alive
            Character.Mana.Current = 0;
            Character.Rage.Current = 0;
            Character.Energy.Current = 0;
            Character.Life.Current = (int)(Character.Life.Maximum / 2d);
            Character.DEAD = false;
            Character.cPlayerFlags = Character.cPlayerFlags & !PlayerFlags.PLAYER_FLAGS_DEAD;
            Character.cUnitFlags = 0x8;
            Character.cDynamicFlags = 0;

            // DONE: Update to see only alive
            Character.InvisibilityReset();
            WorldServiceLocator._WS_CharMovement.UpdateCell(ref Character);
            Character.SetLandWalk();
            if (Character.Race == Races.RACE_NIGHT_ELF)
            {
                Character.RemoveAuraBySpell(20584);
            }
            else
            {
                Character.RemoveAuraBySpell(8326);
            }

            Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, Character.Life.Current);
            Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FLAGS, Character.cPlayerFlags);
            Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, Character.cUnitFlags);
            Character.SetUpdateFlag((int)EUnitFields.UNIT_DYNAMIC_FLAGS, Character.cDynamicFlags);
            Character.SendCharacterUpdate();

            // DONE: Spawn Bones, Delete Corpse
            if (Character.corpseGUID != 0m)
            {
                if (WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs.ContainsKey(Character.corpseGUID))
                {
                    WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[Character.corpseGUID].ConvertToBones();
                }
                else
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Corpse wasn't found [{0}]!", Character.corpseGUID - WorldServiceLocator._Global_Constants.GUID_CORPSE);

                    // DONE: Delete from database
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM corpse WHERE player = \"{0}\";", Character.GUID));

                    // TODO: Turn the corpse into bones on the server it is located at!
                }

                Character.corpseGUID = 0UL;
                Character.corpseMapID = 0;
                Character.corpsePositionX = 0f;
                Character.corpsePositionY = 0f;
                Character.corpsePositionZ = 0f;
            }
        }

        public void On_CMSG_TOGGLE_PVP(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TOGGLE_PVP", client.IP, client.Port);
            client.Character.isPvP = !client.Character.isPvP;
            client.Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, client.Character.cUnitFlags);
            client.Character.SendCharacterUpdate();
        }

        public void On_MSG_INSPECT_HONOR_STATS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_INSPECT_HONOR_STATS [{2:X}]", client.IP, client.Port, GUID);
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID) == false)
                return;
            var response = new Packets.PacketClass(OPCODES.MSG_INSPECT_HONOR_STATS);
            try
            {
                response.AddUInt64(GUID);
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                response.AddInt8((byte)WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorRank);                                                            // Highest Rank
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorKillsToday + WorldServiceLocator._WorldServer.CHARACTERs[GUID].DishonorKillsToday << 16);   // PLAYER_FIELD_SESSION_KILLS                - Today Honorable and Dishonorable Kills
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorKillsYesterday);                                                 // PLAYER_FIELD_YESTERDAY_KILLS              - Yesterday Honorable Kills
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorKillsLastWeek);                                                  // PLAYER_FIELD_LAST_WEEK_KILLS              - Last Week Honorable Kills
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorKillsThisWeek);                                                  // PLAYER_FIELD_THIS_WEEK_KILLS              - This Week Honorable kills
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorKillsLifeTime);                                                  // PLAYER_FIELD_LIFETIME_HONORABLE_KILLS     - Lifetime Honorable Kills
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].DishonorKillsLifeTime);                                               // PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS  - Lifetime Dishonorable Kills
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorPointsYesterday);                                                // PLAYER_FIELD_YESTERDAY_CONTRIBUTION       - Yesterday Honor
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorPointsLastWeek);                                                 // PLAYER_FIELD_LAST_WEEK_CONTRIBUTION       - Last Week Honor
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorPointsThisWeek);                                                 // PLAYER_FIELD_THIS_WEEK_CONTRIBUTION       - This Week Honor
                response.AddInt32(WorldServiceLocator._WorldServer.CHARACTERs[GUID].StandingLastWeek);                                                    // PLAYER_FIELD_LAST_WEEK_RANK               - Last Week Standing
                response.AddInt8((byte)WorldServiceLocator._WorldServer.CHARACTERs[GUID].HonorHighestRank);                                                     // ?!
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
                client.Send(response);
            }
            finally
            {
                response.Dispose();
            }
        }

        public void On_CMSG_MOVE_FALL_RESET(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MOVE_FALL_RESET", client.IP, client.Port);
            WS_Network.ClientClass argclient = null;
            WorldServiceLocator._Packets.DumpPacket(packet.Data, client: ref argclient);
        }

        public void On_CMSG_BATTLEFIELD_STATUS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BATTLEFIELD_STATUS", client.IP, client.Port);
        }

        public void On_CMSG_SET_ACTIVE_MOVER(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTIVE_MOVER [GUID={2:X}]", client.IP, client.Port, GUID);
        }

        public void On_CMSG_MEETINGSTONE_INFO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MEETINGSTONE_INFO", client.IP, client.Port);
        }

        public void On_CMSG_SET_FACTION_ATWAR(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            int faction = packet.GetInt32();
            byte enabled = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_FACTION_ATWAR [faction={2:X} enabled={3}]", client.IP, client.Port, faction, enabled);
            if (enabled > 1)
                return;
            if (enabled == 1)
            {
                client.Character.Reputation[faction].Flags = client.Character.Reputation[faction].Flags | 2;
            }
            else
            {
                client.Character.Reputation[faction].Flags = client.Character.Reputation[faction].Flags & ~2;
            }

            var response = new Packets.PacketClass(OPCODES.SMSG_SET_FACTION_STANDING);
            try
            {
                response.AddInt32(client.Character.Reputation[faction].Flags);
                response.AddInt32(faction);
                response.AddInt32(client.Character.Reputation[faction].Value);
                client.Send(response);
            }
            finally
            {
                response.Dispose();
            }
        }

        public void On_CMSG_SET_FACTION_INACTIVE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            int faction = packet.GetInt32();
            byte enabled = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_FACTION_INACTIVE [faction={2:X} enabled={3}]", client.IP, client.Port, faction, enabled);
            if (enabled > 1)
                return;
        }

        public void On_CMSG_SET_WATCHED_FACTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            int faction = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_WATCHED_FACTION [faction={2:X}]", client.IP, client.Port, faction);
            if (faction == -1)
                faction = 0xFF;
            if (faction < 0 || faction > 255)
                return;
            client.Character.WatchedFactionIndex = (byte)faction;
            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_WATCHED_FACTION_INDEX, faction);
            client.Character.SendCharacterUpdate(false);
        }

        public void On_MSG_PVP_LOG_DATA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            // TODO: Implement this packet - As far as I know, this packet only applys if the character is in a battleground.
            if (!WorldServiceLocator._WS_Maps.Maps[client.Character.MapID].IsBattleGround)
                return;
        }
    }
}