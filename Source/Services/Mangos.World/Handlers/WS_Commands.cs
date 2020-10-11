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
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Handlers
{
    public class WS_Commands
    {
        public const ulong SystemGUID = int.MaxValue;
        public const string SystemNAME = "System";
        public Dictionary<string, ChatCommand> ChatCommands = new Dictionary<string, ChatCommand>();

        public class ChatCommand
        {
            public string CommandHelp;
            public AccessLevel CommandAccess = AccessLevel.GameMaster;
            public ChatCommandDelegate CommandDelegate;
        }

        public delegate bool ChatCommandDelegate(ref WS_PlayerData.CharacterObject objCharacter, string Message);

        public void RegisterChatCommands()
        {
            foreach (Type tmpModule in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (MethodInfo tmpMethod in tmpModule.GetMethods())
                {
                    ChatCommandAttribute[] infos = (ChatCommandAttribute[])tmpMethod.GetCustomAttributes(typeof(ChatCommandAttribute), true);
                    if (infos.Length != 0)
                    {
                        foreach (ChatCommandAttribute info in infos)
                        {
                            var cmd = new ChatCommand()
                            {
                                CommandHelp = info.cmdHelp,
                                CommandAccess = info.cmdAccess,
                                CommandDelegate = (ChatCommandDelegate)Delegate.CreateDelegate(typeof(ChatCommandDelegate), WorldServiceLocator._WS_Commands, tmpMethod)
                            };
                            ChatCommands.Add(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(info.cmdName), cmd);
                        }
                    }
                }
            }
        }

        public void OnCommand(ref WS_Network.ClientClass client, string Message)
        {
            try
            {
                // DONE: Find the command
                var tmp = Strings.Split(Message, " ", 2);
                ChatCommand Command = null;
                if (ChatCommands.ContainsKey(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(tmp[0])))
                {
                    Command = this.ChatCommands(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(tmp[0]));
                }

                // DONE: Build argument string
                string Arguments = "";
                if (tmp.Length == 2)
                    Arguments = Strings.Trim(tmp[1]);

                // DONE: Get character name (there can be no character after the command)
                string Name = client.Character.Name;
                if (Command is null)
                {
                    client.Character.CommandResponse("Unknown command.");
                }
                else if (Command.CommandAccess > client.Character.Access)
                {
                    client.Character.CommandResponse("This command is not available for your access level.");
                }
                else if (!Command.CommandDelegate(ref client.Character, Arguments))
                {
                    client.Character.CommandResponse(Command.CommandHelp);
                }
                else
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[{0}:{1}] {2} used command: {3}", client.IP, client.Port, Name, Message);
                }
            }
            catch (Exception err)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Client command caused error! {3}{2}", client.IP, client.Port, err.ToString(), Environment.NewLine);
                client.Character.CommandResponse(string.Format("Your command caused error:" + Environment.NewLine + " [{0}]", err.Message));
            }
        }

        // Help Command
        [ChatCommand("help", @"help #command\r\nDisplays usage information about command, if no command specified - displays list of available commands.", AccessLevel.GameMaster)]
        public bool Help(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (!string.IsNullOrEmpty(Strings.Trim(Message)))
            {
                var Command = this.ChatCommands(Strings.Trim(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Message)));
                if (Command is null)
                {
                    objCharacter.CommandResponse("Unknown command.");
                }
                else if (Command.CommandAccess > objCharacter.Access)
                {
                    objCharacter.CommandResponse("This command is not available for your access level.");
                }
                else
                {
                    objCharacter.CommandResponse(Command.CommandHelp);
                }
            }
            else
            {
                string cmdList = "Listing available commands:" + Environment.NewLine;
                foreach (KeyValuePair<string, ChatCommand> Command in ChatCommands)
                {
                    if (Command.Value.CommandAccess <= objCharacter.Access)
                        cmdList += WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Command.Key) + Environment.NewLine; // ", "
                }

                cmdList += Environment.NewLine + "Use help #command for usage information about particular command.";
                objCharacter.CommandResponse(cmdList);
            }

            return true;
        }

        // CastSpell Command
        [ChatCommand("castspell", "castspell #spellid #target - Selected unit will start casting spell. Target can be ME or SELF.", AccessLevel.Developer)]
        public bool cmdCastSpellMe(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            var tmp = Strings.Split(Message, " ", 2);
            if (tmp.Length < 2)
                return false;
            int SpellID = Conversions.ToInteger(tmp[0]);
            string Target = WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(tmp[1]);
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
            {
                switch (Target ?? "")
                {
                    case "ME":
                        {
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].CastSpell(SpellID, objCharacter);
                            break;
                        }

                    case "SELF":
                        {
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].CastSpell(SpellID, WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID]);
                            break;
                        }
                }
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                switch (Target ?? "")
                {
                    case "ME":
                        {
                            var Targets = new WS_Spells.SpellTargets();
                            WS_Base.BaseUnit argobjCharacter = objCharacter;
                            Targets.SetTarget_UNIT(ref argobjCharacter);
                            var tmp1 = WorldServiceLocator._WorldServer.CHARACTERs;
                            WS_Base.BaseObject argCaster = tmp1[objCharacter.TargetGUID];
                            var castParams = new WS_Spells.CastSpellParameters(ref Targets, ref argCaster, SpellID);
                            tmp1[objCharacter.TargetGUID] = (WS_PlayerData.CharacterObject)argCaster;
                            ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
                            break;
                        }

                    case "SELF":
                        {
                            WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].CastOnSelf(SpellID);
                            break;
                        }
                }
            }
            else
            {
                objCharacter.CommandResponse(string.Format("GUID=[{0:X}] not found or unsupported.", objCharacter.TargetGUID));
            }

            return true;
        }

        // Control Command
        [ChatCommand("control", "control - Takes or removes control over the selected unit.", AccessLevel.Admin)]
        public bool cmdControl(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.MindControl is object)
            {
                if (objCharacter.MindControl is WS_PlayerData.CharacterObject)
                {
                    var packet1 = new Packets.PacketClass(OPCODES.SMSG_DEATH_NOTIFY_OBSOLETE);
                    packet1.AddPackGUID(objCharacter.MindControl.GUID);
                    packet1.AddInt8(1);
                    ((WS_PlayerData.CharacterObject)objCharacter.MindControl).client.Send(packet1);
                    packet1.Dispose();
                }

                var packet3 = new Packets.PacketClass(OPCODES.SMSG_DEATH_NOTIFY_OBSOLETE);
                packet3.AddPackGUID(objCharacter.MindControl.GUID);
                packet3.AddInt8(0);
                objCharacter.client.Send(packet3);
                packet3.Dispose();
                objCharacter.cUnitFlags &= !UnitFlags.UNIT_FLAG_UNK21;
                objCharacter.SetUpdateFlag((int)EPlayerFields.PLAYER_FARSIGHT, 0);
                objCharacter.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, objCharacter.cUnitFlags);
                objCharacter.SendCharacterUpdate(false);
                objCharacter.MindControl = null;
                objCharacter.CommandResponse("Removed control over the unit.");
                return true;
            }

            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                var packet1 = new Packets.PacketClass(OPCODES.SMSG_DEATH_NOTIFY_OBSOLETE);
                packet1.AddPackGUID(objCharacter.TargetGUID);
                packet1.AddInt8(0);
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].client.Send(packet1);
                packet1.Dispose();
                objCharacter.MindControl = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID];
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
            {
                objCharacter.MindControl = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
            }
            else
            {
                objCharacter.CommandResponse("You need a target.");
                return true;
            }

            var packet2 = new Packets.PacketClass(OPCODES.SMSG_DEATH_NOTIFY_OBSOLETE);
            packet2.AddPackGUID(objCharacter.TargetGUID);
            packet2.AddInt8(1);
            objCharacter.client.Send(packet2);
            packet2.Dispose();
            objCharacter.cUnitFlags |= UnitFlags.UNIT_FLAG_UNK21;
            objCharacter.SetUpdateFlag((int)EPlayerFields.PLAYER_FARSIGHT, objCharacter.TargetGUID);
            objCharacter.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, objCharacter.cUnitFlags);
            objCharacter.SendCharacterUpdate(false);
            objCharacter.CommandResponse("Taken control over a unit.");
            return true;
        }

        // CreateGuild Command - Needs to be implemented
        [ChatCommand("createguild", "createguild #guildname - Creates a guild.", AccessLevel.Developer)]
        public bool cmdCreateGuild(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            // TODO: Creating guilds must be done in the cluster

            // Dim GuildName As String = Message

            // Dim MySQLQuery As New DataTable
            // _WorldServer.CharacterDatabase.Query(String.Format("INSERT INTO guilds (guild_name, guild_leader, guild_cYear, guild_cMonth, guild_cDay) VALUES ('{0}', {1}, {2}, {3}, {4}); SELECT guild_id FROM guilds WHERE guild_name = '{0}';", GuildName, objCharacter.GUID, Now.Year, Now.Month, Now.Day), MySQLQuery)

            // AddCharacterToGuild(objCharacter, MySQLQuery.Rows(0).Item("guild_id"), 0)
            return true;
        }

        // Cast Command
        [ChatCommand("cast", "cast #spellid - You will start casting spell on selected target.", AccessLevel.Developer)]
        public bool cmdCastSpell(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            var tmp = Strings.Split(Message, " ", 2);
            int SpellID = Conversions.ToInteger(tmp[0]);
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
            {
                var Targets = new WS_Spells.SpellTargets();
                var tmp1 = WorldServiceLocator._WorldServer.WORLD_CREATUREs;
                WS_Base.BaseUnit argobjCharacter = tmp1[objCharacter.TargetGUID];
                Targets.SetTarget_UNIT(ref argobjCharacter);
                tmp1[objCharacter.TargetGUID] = (WS_Creatures.CreatureObject)argobjCharacter;
                WS_Base.BaseObject argCaster = objCharacter;
                var castParams = new WS_Spells.CastSpellParameters(ref Targets, ref argCaster, SpellID);
                ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
                objCharacter.CommandResponse("You are now casting [" + SpellID + "] at [" + WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].Name + "].");
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                var Targets = new WS_Spells.SpellTargets();
                var tmp2 = WorldServiceLocator._WorldServer.CHARACTERs;
                WS_Base.BaseUnit argobjCharacter1 = tmp2[objCharacter.TargetGUID];
                Targets.SetTarget_UNIT(ref argobjCharacter1);
                tmp2[objCharacter.TargetGUID] = (WS_PlayerData.CharacterObject)argobjCharacter1;
                WS_Base.BaseObject argCaster1 = objCharacter;
                var castParams = new WS_Spells.CastSpellParameters(ref Targets, ref argCaster1, SpellID);
                ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
                objCharacter.CommandResponse("You are now casting [" + SpellID + "] at [" + WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name + "].");
            }
            else
            {
                objCharacter.CommandResponse(string.Format("GUID=[{0:X}] not found or unsupported.", objCharacter.TargetGUID));
            }

            return true;
        }

        // Save Command
        [ChatCommand("save", "save - Saves selected character.", AccessLevel.Developer)]
        public bool cmdSave(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID != 0m && WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Save();
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].CommandResponse(string.Format("Character {0} saved.", WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name));
            }
            else
            {
                objCharacter.Save();
                objCharacter.CommandResponse(string.Format("Character {0} saved.", objCharacter.Name));
            }

            return true;
        }

        // SpawnData Command
        [ChatCommand("spawndata", "spawndata - Tells you the spawn in memory information.", AccessLevel.Developer)]
        public bool cmdSpawns(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            objCharacter.CommandResponse("Spawns loaded in server memory:");
            objCharacter.CommandResponse("-------------------------------");
            objCharacter.CommandResponse("Creatures: " + WorldServiceLocator._WorldServer.WORLD_CREATUREs.Count);
            objCharacter.CommandResponse("GameObjects: " + WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.Count);
            return true;
        }

        // GobjectNear Command
        [ChatCommand("gobjectnear", "gobjectnear - Tells you the near objects count.", AccessLevel.Developer)]
        public bool cmdNear(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            objCharacter.CommandResponse("Near objects:");
            objCharacter.CommandResponse("-------------------------------");
            objCharacter.CommandResponse("Players: " + objCharacter.playersNear.Count);
            objCharacter.CommandResponse("Creatures: " + objCharacter.creaturesNear.Count);
            objCharacter.CommandResponse("GameObjects: " + objCharacter.gameObjectsNear.Count);
            objCharacter.CommandResponse("Corpses: " + objCharacter.corpseObjectsNear.Count);
            objCharacter.CommandResponse("-------------------------------");
            objCharacter.CommandResponse("You are seen by: " + objCharacter.SeenBy.Count);
            return true;
        }

        // NpcAI Command
        [ChatCommand("npcai", "npcai #enable/disable - Enables/Disables  Creature AI updating.", AccessLevel.Developer)]
        public bool cmdAI(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Message) == "ENABLE")
            {
                WorldServiceLocator._WS_TimerBasedEvents.AIManager.AIManagerTimer.Change(WS_TimerBasedEvents.TAIManager.UPDATE_TIMER, WS_TimerBasedEvents.TAIManager.UPDATE_TIMER);
                objCharacter.CommandResponse("AI is enabled.");
            }
            else if (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Message) == "DISABLE")
            {
                WorldServiceLocator._WS_TimerBasedEvents.AIManager.AIManagerTimer.Change(Timeout.Infinite, Timeout.Infinite);
                objCharacter.CommandResponse("AI is disabled.");
            }
            else
            {
                return false;
            }

            return true;
        }

        // NpcAIState Command
        [ChatCommand("npcaistate", "npcaistate - Shows debug information about AI state of selected creature.", AccessLevel.Developer)]
        public bool cmdAIState(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return false;
            }

            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
            {
                objCharacter.CommandResponse("Selected target is not creature!");
                return false;
            }

            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].aiScript is null)
            {
                objCharacter.CommandResponse("This creature doesn't have AI");
                return false;
            }
            else
            {
                {
                    var withBlock = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
                    objCharacter.CommandResponse(string.Format("Information for creature [{0}]:{1}ai = {2}{1}state = {3}{1}maxdist = {4}", withBlock.Name, Environment.NewLine, withBlock.aiScript.ToString(), withBlock.aiScript.State.ToString, withBlock.MaxDistance));
                    objCharacter.CommandResponse("Hate table:");
                    foreach (KeyValuePair<WS_Base.BaseUnit, int> u in withBlock.aiScript.aiHateTable)
                        objCharacter.CommandResponse(string.Format("{0:X} = {1} hate", u.Key.GUID, u.Value));
                }
            }

            return true;
        }

        // ServerMessage Command
        [ChatCommand("servermessage", "servermessage #type #text - Send text message to all players on the server.", AccessLevel.GameMaster)]
        public bool cmdServerMessage(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            // 1,"[SERVER] Shutdown in %s"
            // 2,"[SERVER] Restart in %s"
            // 3,"%s"
            // 4,"[SERVER] Shutdown cancelled"
            // 5,"[SERVER] Restart cancelled"

            var tmp = Strings.Split(Message, " ", 2);
            if (tmp.Length != 2)
                return false;
            int Type = Conversions.ToInteger(tmp[0]);
            string Text = tmp[1];
            var packet = new Packets.PacketClass(OPCODES.SMSG_SERVER_MESSAGE);
            packet.AddInt32(Type);
            packet.AddString(Text);
            packet.UpdateLength();
            WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.Broadcast(packet.Data);
            packet.Dispose();
            return true;
        }

        // NotifyMessage Command
        [ChatCommand("notifymessage", "notify #message - Send text message to all players on the server.", AccessLevel.GameMaster)]
        public bool cmdNotificationMessage(ref WS_PlayerData.CharacterObject objCharacter, string Text)
        {
            if (string.IsNullOrEmpty(Text))
                return false;
            var packet = new Packets.PacketClass(OPCODES.SMSG_NOTIFICATION);
            packet.AddString(Text);
            packet.UpdateLength();
            WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.Broadcast(packet.Data);
            packet.Dispose();
            return true;
        }

        // Say Command
        [ChatCommand("say", "say #text - Target NPC will say this.", AccessLevel.GameMaster)]
        public bool cmdSay(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            if (objCharacter.TargetGUID == 0m)
                return false;
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].SendChatMessage(Message, ChatMsg.CHAT_MSG_MONSTER_SAY, LANGUAGES.LANG_UNIVERSAL, objCharacter.GUID);
            }
            else
            {
                return false;
            }

            return true;
        }

        // ResetFactions Command
        [ChatCommand("resetfactions", "resetfactions - Resets character reputation standings.", AccessLevel.Admin)]
        public bool cmdResetFactions(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
                var argobjCharacter = tmp[objCharacter.TargetGUID];
                WorldServiceLocator._WS_Player_Initializator.InitializeReputations(ref argobjCharacter);
                tmp[objCharacter.TargetGUID] = argobjCharacter;
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SaveCharacter();
            }
            else
            {
                WorldServiceLocator._WS_Player_Initializator.InitializeReputations(ref objCharacter);
                objCharacter.SaveCharacter();
            }

            return true;
        }

        // SkillMaster Command
        [ChatCommand("skillmaster", "skillmaster - Get all spells and skills maxed out for your level.", AccessLevel.Developer)]
        public bool cmdGetMax(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            // DONE: Max out all skills you know
            foreach (KeyValuePair<int, WS_PlayerHelper.TSkill> skill in objCharacter.Skills)
            {
                skill.Value.Current = (short)skill.Value.Maximum;
                objCharacter.SetUpdateFlag((int)(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)objCharacter.SkillsPositions[skill.Key] * 3 + 1), objCharacter.Skills[skill.Key].GetSkill);
            }

            objCharacter.SendCharacterUpdate(false);

            // TODO: Add all spells

            return true;
        }

        // SetLevel Command
        [ChatCommand("setlevel", "setlevel #level - Set the level of selected character.", AccessLevel.Developer)]
        public bool cmdSetLevel(ref WS_PlayerData.CharacterObject objCharacter, string tLevel)
        {
            if (Information.IsNumeric(tLevel) == false)
                return false;
            int Level = Conversions.ToInteger(tLevel);
            if (Level > WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                Level = WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL;
            if (Level > 60)
                Level = 60;
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID) == false)
            {
                objCharacter.CommandResponse("Target not found or not character.");
                return true;
            }

            WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetLevel((byte)Level);
            return true;
        }

        // AddXp Command
        [ChatCommand("addxp", "addxp #amount - Add X experience points to selected character.", AccessLevel.Developer)]
        public bool cmdAddXP(ref WS_PlayerData.CharacterObject objCharacter, string tXP)
        {
            if (Information.IsNumeric(tXP) == false)
                return false;
            int XP = Conversions.ToInteger(tXP);
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].AddXP(XP, 0, 0UL, true);
            }
            else
            {
                objCharacter.CommandResponse("Target not found or not character.");
            }

            return true;
        }

        // AddRestedXp Command
        [ChatCommand("addrestedxp", "addrestedxp #amount - Add X rested bonus experience points to selected character.", AccessLevel.Developer)]
        public bool cmdAddRestedXP(ref WS_PlayerData.CharacterObject objCharacter, string tXP)
        {
            if (Information.IsNumeric(tXP) == false)
                return false;
            int XP = Conversions.ToInteger(tXP);
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].RestBonus += XP;
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].RestState = XPSTATE.Rested;
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetUpdateFlag((int)EPlayerFields.PLAYER_REST_STATE_EXPERIENCE, WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].RestBonus);
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetUpdateFlag((int)EPlayerFields.PLAYER_BYTES_2, WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].cPlayerBytes2);
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SendCharacterUpdate();
            }
            else
            {
                objCharacter.CommandResponse("Target not found or not character.");
            }

            return true;
        }

        // AddHonor Command - Disabled: missing packet data
        // <ChatCommand("addhonor", "addhonor #amount - Add select amount of honor points to selected character.", AccessLevel.Admin)>
        // Public Function cmdAddHonor(ByRef objCharacter As CharacterObject, ByVal tHONOR As String) As Boolean
        // If IsNumeric(tHONOR) = False Then Return False

        // Dim Honor As Integer = tHONOR

        // If _WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID) Then
        // _WorldServer.CHARACTERs(objCharacter.TargetGUID).HonorPoints += Honor
        // '_WorldServer.CHARACTERs(objCharacter.TargetGUID).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_HONOR_CURRENCY, _WorldServer.CHARACTERs(objCharacter.TargetGUID).HonorCurrency)
        // _WorldServer.CHARACTERs(objCharacter.TargetGUID).SendCharacterUpdate(False)
        // Else
        // objCharacter.CommandResponse("Target not found or not character.")
        // End If

        // Return True
        // End Function

        // PlaySound Command
        [ChatCommand("playsound", "playsound - Plays a specific sound for every player around you.", AccessLevel.Developer)]
        public bool cmdPlaySound(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            int soundID;
            if (int.TryParse(Message, out soundID) == false)
                return false;
            objCharacter.SendPlaySound(soundID);
            return true;
        }

        // CombatList Command
        [ChatCommand("combatlist", "combatlist - Lists everyone in your targets combatlist.", AccessLevel.Developer)]
        public bool cmdCombatList(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            var combatList = Array.Empty<ulong>();
            if (objCharacter.TargetGUID != 0m && WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID))
            {
                combatList = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].inCombatWith.ToArray();
            }
            else
            {
                combatList = objCharacter.inCombatWith.ToArray();
            }

            objCharacter.CommandResponse("Combat List (" + combatList.Length + "):");
            foreach (ulong Guid in combatList)
                objCharacter.CommandResponse(string.Format("* {0:X}", Guid));
            return true;
        }

        // CoolDownList Command
        [ChatCommand("cooldownlist", "cooldownlist - Lists all cooldowns of your target.", AccessLevel.GameMaster)]
        public bool cmdCooldownList(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            WS_Base.BaseUnit targetUnit = null;
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID))
            {
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
                    targetUnit = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID];
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID))
            {
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
                    targetUnit = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
            }

            if (targetUnit is null)
            {
                targetUnit = objCharacter;
            }

            if (ReferenceEquals(targetUnit, objCharacter))
            {
                objCharacter.CommandResponse("Listing your cooldowns:");
            }
            else
            {
                objCharacter.CommandResponse("Listing cooldowns for [" + targetUnit.UnitName + "]:");
            }

            if (targetUnit is WS_PlayerData.CharacterObject)
            {
                string sCooldowns = "";
                uint timeNow = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                foreach (KeyValuePair<int, WS_Spells.CharacterSpell> Spell in ((WS_PlayerData.CharacterObject)targetUnit).Spells)
                {
                    if (Spell.Value.Cooldown > 0U)
                    {
                        uint timeLeft = 0U;
                        if (timeNow < Spell.Value.Cooldown)
                            timeLeft = Spell.Value.Cooldown - timeNow;
                        if (timeLeft > 0L)
                        {
                            sCooldowns += "* Spell: " + Spell.Key + " - TimeLeft: " + WorldServiceLocator._Functions.GetTimeLeftString(timeLeft) + " sec" + " - Item: " + Spell.Value.CooldownItem + Environment.NewLine;
                        }
                    }
                }

                objCharacter.CommandResponse(sCooldowns);
            }
            else
            {
                objCharacter.CommandResponse("*Cooldowns not supported for creatures yet*");
            }

            return true;
        }

        // ClearCoolDowns Command
        [ChatCommand("clearcooldowns", "clearcooldowns - Clears all cooldowns of your target.", AccessLevel.Developer)]
        public bool cmdClearCooldowns(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            WS_Base.BaseUnit targetUnit = null;
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID))
            {
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
                    targetUnit = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID];
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID))
            {
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
                    targetUnit = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
            }

            if (targetUnit is null)
            {
                targetUnit = objCharacter;
            }

            if (targetUnit is WS_PlayerData.CharacterObject)
            {
                uint timeNow = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                var cooldownSpells = new List<int>();
                foreach (KeyValuePair<int, WS_Spells.CharacterSpell> Spell in ((WS_PlayerData.CharacterObject)targetUnit).Spells)
                {
                    if (Spell.Value.Cooldown > 0U)
                    {
                        Spell.Value.Cooldown = 0U;
                        Spell.Value.CooldownItem = (int)0U;
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_spells SET cooldown={2}, cooldownitem={3} WHERE guid = {0} AND spellid = {1};", objCharacter.GUID, Spell.Key, 0, 0));
                        cooldownSpells.Add(Spell.Key);
                    }
                }

                foreach (int SpellID in cooldownSpells)
                {
                    var packet = new Packets.PacketClass(OPCODES.SMSG_CLEAR_COOLDOWN);
                    packet.AddInt32(SpellID);
                    packet.AddUInt64(targetUnit.GUID);
                    ((WS_PlayerData.CharacterObject)targetUnit).client.Send(packet);
                    packet.Dispose();
                }
            }
            else
            {
                objCharacter.CommandResponse("Cooldowns are not supported for creatures yet.");
            }

            return true;
        }

        // Disabled till warden is finished
        // <ChatCommand("StartCheck", "STARTCHECK - Initialize Warden anti-cheat engine for selected character.", AccessLevel.Developer)>
        // Public Function cmdStartCheck(ByRef objCharacter As CharacterObject, ByVal Message As String) As Boolean
        // #If WARDEN Then
        // If objCharacter.TargetGUID <> 0 AndAlso _CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) AndAlso _WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID) Then
        // MaievInit(_WorldServer.CHARACTERs(objCharacter.TargetGUID))
        // Else
        // objCharacter.CommandResponse("No player target selected.")
        // End If
        // #Else
        // objCharacter.CommandResponse("Warden is not active.")
        // #End If

        // Return True
        // End Function

        // <ChatCommand("SendCheck", "SENDCHECK - Sends a Warden anti-cheat check packet to the selected character.", AccessLevel.Developer)>
        // Public Function cmdSendCheck(ByRef objCharacter As CharacterObject, ByVal Message As String) As Boolean
        // #If WARDEN Then
        // If objCharacter.TargetGUID <> 0 AndAlso _CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) AndAlso _WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID) Then
        // MaievSendCheck(_WorldServer.CHARACTERs(objCharacter.TargetGUID))
        // Else
        // objCharacter.CommandResponse("No player target selected.")
        // End If
        // #Else
        // objCharacter.CommandResponse("Warden is not active.")
        // #End If

        // Return True
        // End Function

        // Additem Command
        [ChatCommand("additem", "additem #itemid #count (optional) - Add chosen items with item amount to selected character.", AccessLevel.GameMaster)]
        public bool cmdAddItem(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            var tmp = Strings.Split(Message, " ", 2);
            if (tmp.Length < 1)
                return false;
            int id = Conversions.ToInteger(tmp[0]);
            int Count = 1;
            if (tmp.Length == 2)
                Count = Conversions.ToInteger(tmp[1]);
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                var newItem = new ItemObject(id, objCharacter.TargetGUID) { StackCount = Count };
                if (WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].ItemADD(ref newItem))
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].LogLootItem(newItem, (byte)Count, true, false);
                }
                else
                {
                    newItem.Delete();
                }
            }
            else
            {
                var newItem = new ItemObject(id, objCharacter.GUID) { StackCount = Count };
                if (objCharacter.ItemADD(ref newItem))
                {
                    objCharacter.LogLootItem(newItem, (byte)Count, false, true);
                }
                else
                {
                    newItem.Delete();
                }
            }

            return true;
        }

        // AddItemSet
        [ChatCommand("additemset", "additemset #item - Add the items in the item set with id X to selected character.", AccessLevel.GameMaster)]
        public bool cmdAddItemSet(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            var tmp = Strings.Split(Message, " ", 2);
            if (tmp.Length < 1)
                return false;
            int id = Conversions.ToInteger(tmp[0]);
            if (WorldServiceLocator._WS_DBCDatabase.ItemSet.ContainsKey(id))
            {
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
                {
                    foreach (int item in WorldServiceLocator._WS_DBCDatabase.ItemSet[id].ItemID)
                    {
                        var newItem = new ItemObject(item, objCharacter.TargetGUID) { StackCount = 1 };
                        if (WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].ItemADD(ref newItem))
                        {
                            WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].LogLootItem(newItem, 1, false, true);
                        }
                        else
                        {
                            newItem.Delete();
                        }
                    }
                }
                else
                {
                    foreach (int item in WorldServiceLocator._WS_DBCDatabase.ItemSet[id].ItemID)
                    {
                        var newItem = new ItemObject(item, objCharacter.GUID) { StackCount = 1 };
                        if (objCharacter.ItemADD(ref newItem))
                        {
                            objCharacter.LogLootItem(newItem, 1, false, true);
                        }
                        else
                        {
                            newItem.Delete();
                        }
                    }
                }
            }

            return true;
        }

        // Addmoney Command
        // ToDo: Add method of Copper, Silver or Gold in the command.
        // Max Gold in Vanilla?
        [ChatCommand("addmoney", "addmoney #amount - Add chosen copper to your character or selected character.", AccessLevel.GameMaster)]
        public bool cmdAddMoney(ref WS_PlayerData.CharacterObject objCharacter, string tCopper)
        {
            if (string.IsNullOrEmpty(tCopper))
                return false;
            ulong Copper = Conversions.ToULong(tCopper);
            if (objCharacter.Copper + Copper > uint.MaxValue)
            {
                objCharacter.Copper = uint.MaxValue;
            }
            else
            {
                objCharacter.Copper = (uint)(objCharacter.Copper + Copper);
            }

            objCharacter.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, objCharacter.Copper);
            objCharacter.SendCharacterUpdate(false);
            return true;
        }

        // LearnSkill Command
        [ChatCommand("learnskill", "learnskill #id #current #max - Add skill id X with value Y of Z to selected character.", AccessLevel.Developer)]
        public bool cmdLearnSkill(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                string[] tmp;
                tmp = Strings.Split(Strings.Trim(Message), " ");
                int SkillID = Conversions.ToInteger(tmp[0]);
                short Current = Conversions.ToShort(tmp[1]);
                short Maximum = Conversions.ToShort(tmp[2]);
                if (WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Skills.ContainsKey(SkillID))
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Skills[SkillID].Base = Maximum;
                    WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Skills[SkillID].Current = Current;
                }
                else
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].LearnSkill(SkillID, Current, Maximum);
                }

                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].FillAllUpdateFlags();
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SendUpdate();
            }
            else
            {
                objCharacter.CommandResponse("Target not found or not character.");
            }

            return true;
        }

        // LearnSpell Command
        [ChatCommand("learnSpell", "learnSpell #id - Add chosen spell to selected character.", AccessLevel.Developer)]
        public bool cmdLearnSpell(ref WS_PlayerData.CharacterObject objCharacter, string tID)
        {
            if (string.IsNullOrEmpty(tID))
                return false;
            int ID;
            if (int.TryParse(tID, out ID) == false || ID < 0)
                return false;
            if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(ID) == false)
            {
                objCharacter.CommandResponse("You tried learning a spell that did not exist.");
                return false;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].LearnSpell(ID);
                if (objCharacter.TargetGUID == objCharacter.GUID)
                {
                    objCharacter.CommandResponse("You learned spell: " + ID);
                }
                else
                {
                    objCharacter.CommandResponse(WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name + " has learned spell: " + ID);
                }
            }
            else
            {
                objCharacter.CommandResponse("Target not found or not character.");
            }

            return true;
        }

        // UnlearnSpell Command
        [ChatCommand("unlearnspell", "unlearnspell #id - Remove chosen spell from selected character.", AccessLevel.Developer)]
        public bool cmdUnlearnSpell(ref WS_PlayerData.CharacterObject objCharacter, string tID)
        {
            if (string.IsNullOrEmpty(tID))
                return false;
            int ID = Conversions.ToInteger(tID);
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].UnLearnSpell(ID);
                if (objCharacter.TargetGUID == objCharacter.GUID)
                {
                    objCharacter.CommandResponse("You unlearned spell: " + ID);
                }
                else
                {
                    objCharacter.CommandResponse(WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name + " has unlearned spell: " + ID);
                }
            }
            else
            {
                objCharacter.CommandResponse("Target not found or not character.");
            }

            return true;
        }

        // ShowTaxi Command
        [ChatCommand("showtaxi", "showtaxi - Unlock all taxi locations.", AccessLevel.Developer)]
        public bool cmdShowTaxi(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            objCharacter.TaxiZones.SetAll(true);
            return true;
        }

        // SetCharacterSpeed Command
        [ChatCommand("setcharacterspeed", "setcharacterspeed #value - Change your character travel speed.", AccessLevel.GameMaster)]
        public bool cmdSetCharacterSpeed(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            objCharacter.ChangeSpeedForced(ChangeSpeedType.RUN, Message);
            objCharacter.CommandResponse("Your RunSpeed is changed to " + Message);
            objCharacter.ChangeSpeedForced(ChangeSpeedType.SWIM, Message);
            objCharacter.CommandResponse("Your SwimSpeed is changed to " + Message);
            objCharacter.ChangeSpeedForced(ChangeSpeedType.SWIMBACK, Message);
            objCharacter.CommandResponse("Your RunBackSpeed is changed to " + Message);
            return true;
        }

        // SetReputation Command
        [ChatCommand("setreputation", "setreputation #faction #value - Change your reputation standings.", AccessLevel.GameMaster)]
        public bool cmdSetReputation(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            var tmp = Strings.Split(Message, " ", 2);
            objCharacter.SetReputation(Conversions.ToInteger(tmp[0]), Conversions.ToInteger(tmp[1]));
            objCharacter.CommandResponse("You have set your reputation with [" + tmp[0] + "] to [" + tmp[1] + "]");
            return true;
        }

        // ChangeModel Command
        [ChatCommand("changemodel", "changemodel #id - Will morph you into specified model ID.", AccessLevel.GameMaster)]
        public bool cmdModel(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            int value;
            if (int.TryParse(Message, out value) == false || value < 0)
                return false;
            if (WorldServiceLocator._WS_DBCDatabase.CreatureModel.ContainsKey(value))
            {
                objCharacter.BoundingRadius = WorldServiceLocator._WS_DBCDatabase.CreatureModel[value].BoundingRadius;
                objCharacter.CombatReach = WorldServiceLocator._WS_DBCDatabase.CreatureModel[value].CombatReach;
            }

            objCharacter.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_BOUNDINGRADIUS, objCharacter.BoundingRadius);
            objCharacter.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_COMBATREACH, objCharacter.CombatReach);
            objCharacter.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_DISPLAYID, value);
            objCharacter.SendCharacterUpdate();
            return true;
        }

        // Mount Command
        [ChatCommand("mount", "mount #id - Will mount you to specified model ID.", AccessLevel.GameMaster)]
        public bool cmdMount(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            int value;
            if (int.TryParse(Message, out value) == false || value < 0)
                return false;
            objCharacter.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, value);
            objCharacter.SendCharacterUpdate();
            return true;
        }

        // Hurt Command - Wait what?
        [ChatCommand("hurt", "hurt - Hurts a selected character.", AccessLevel.GameMaster)]
        public bool cmdHurt(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Life.Current = (int)(WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Life.Current - WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Life.Maximum * 0.1d);
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Life.Current);
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SendCharacterUpdate();
                return true;
            }

            return true;
        }

        // Root Command
        [ChatCommand("root", "root - Instantly root selected character.", AccessLevel.GameMaster)]
        public bool cmdRoot(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetMoveRoot();
                return true;
            }

            return true;
        }

        // Unroot Command
        [ChatCommand("unroot", "unroot - Instantly unroot selected character.", AccessLevel.GameMaster)]
        public bool cmdUnRoot(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetMoveUnroot();
                return true;
            }

            return true;
        }

        // Revive Command
        [ChatCommand("revive", "revive - Instantly revive selected character.", AccessLevel.GameMaster)]
        public bool cmdRevive(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
                var argCharacter = tmp[objCharacter.TargetGUID];
                WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref argCharacter);
                tmp[objCharacter.TargetGUID] = argCharacter;
                return true;
            }

            return true;
        }

        // GoToGY Command
        [ChatCommand("gotogy", "gotogy - Instantly teleports selected character to nearest graveyard.", AccessLevel.GameMaster)]
        public bool cmdGoToGraveyard(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
                var argCharacter = tmp[objCharacter.TargetGUID];
                WorldServiceLocator._WorldServer.AllGraveYards.GoToNearestGraveyard(ref argCharacter, false, true);
                tmp[objCharacter.TargetGUID] = argCharacter;
                return true;
            }

            return true;
        }

        // ToStart Command
        [ChatCommand("tostart", "tostart #race - Instantly teleports selected character to specified race start location.", AccessLevel.GameMaster)]
        public bool cmdGoToStart(ref WS_PlayerData.CharacterObject objCharacter, string StringRace)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                var Info = new DataTable();
                var Character = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID];
                Races Race;
                switch (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(StringRace))
                {
                    case "DWARF":
                    case "DW":
                        {
                            Race = Races.RACE_DWARF;
                            break;
                        }

                    case "GNOME":
                    case "GN":
                        {
                            Race = Races.RACE_GNOME;
                            break;
                        }

                    case "HUMAN":
                    case "HU":
                        {
                            Race = Races.RACE_HUMAN;
                            break;
                        }

                    case "NIGHTELF":
                    case "NE":
                        {
                            Race = Races.RACE_NIGHT_ELF;
                            break;
                        }

                    case "ORC":
                    case "OR":
                        {
                            Race = Races.RACE_ORC;
                            break;
                        }

                    case "TAUREN":
                    case "TA":
                        {
                            Race = Races.RACE_TAUREN;
                            break;
                        }

                    case "TROLL":
                    case "TR":
                        {
                            Race = Races.RACE_TROLL;
                            break;
                        }

                    case "UNDEAD":
                    case "UN":
                        {
                            Race = Races.RACE_UNDEAD;
                            break;
                        }

                    default:
                        {
                            objCharacter.CommandResponse("Unknown race. Use DW, GN, HU, NE, OR, TA, TR, UN for race.");
                            return true;
                        }
                }

                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM playercreateinfo WHERE race = {0};", Conversions.ToInteger(Race)), ref Info);
                Character.Teleport(Conversions.ToSingle(Info.Rows[0]["position_x"]), Conversions.ToSingle(Info.Rows[0]["position_y"]), Conversions.ToSingle(Info.Rows[0]["position_z"]), Conversions.ToSingle(Info.Rows[0]["orientation"]), Conversions.ToInteger(Info.Rows[0]["map"]));
                return true;
            }

            return true;
        }

        // Summon Command
        [ChatCommand("summon", "summon #name - Instantly teleports the player to you.", AccessLevel.GameMaster)]
        public bool cmdSummon(ref WS_PlayerData.CharacterObject objCharacter, string Name)
        {
            ulong GUID = GetGUID(WorldServiceLocator._Functions.CapitalizeName(ref Name));
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
            {
                if (objCharacter.OnTransport is object)
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].OnTransport = objCharacter.OnTransport;
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].Transfer(objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ, objCharacter.orientation, (int)objCharacter.MapID);
                }
                else
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[GUID].Teleport(objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ, objCharacter.orientation, (int)objCharacter.MapID);
                }

                return true;
            }
            else
            {
                objCharacter.CommandResponse("Player not found.");
                return true;
            }
        }

        // Appear Command
        [ChatCommand("appear", "appear #name - Instantly teleports you to the player.", AccessLevel.GameMaster)]
        public bool cmdAppear(ref WS_PlayerData.CharacterObject objCharacter, string Name)
        {
            ulong GUID = GetGUID(WorldServiceLocator._Functions.CapitalizeName(ref Name));
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
            {
                {
                    var withBlock = WorldServiceLocator._WorldServer.CHARACTERs[GUID];
                    if (withBlock.OnTransport is object)
                    {
                        objCharacter.OnTransport = withBlock.OnTransport;
                        objCharacter.Transfer(withBlock.positionX, withBlock.positionY, withBlock.positionZ, withBlock.orientation, (int)withBlock.MapID);
                    }
                    else
                    {
                        objCharacter.Teleport(withBlock.positionX, withBlock.positionY, withBlock.positionZ, withBlock.orientation, (int)withBlock.MapID);
                    }
                }

                return true;
            }
            else
            {
                objCharacter.CommandResponse("Player not found.");
                return true;
            }
        }

        // <ChatCommand("VmapTest", "VMAPTEST - Tests VMAP functionality.", AccessLevel.Developer)>
        // Public Function cmdVmapTest(ByRef objCharacter As CharacterObject, ByVal Message As String) As Boolean
        // #If VMAPS Then
        // If _WorldServer.Config.VMapsEnabled Then
        // Dim target As BaseUnit = Nothing
        // If objCharacter.TargetGUID > 0 Then
        // If _CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) AndAlso _WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID) Then
        // target = _WorldServer.CHARACTERs(objCharacter.TargetGUID)
        // ElseIf _CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) AndAlso _WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID) Then
        // target = _WorldServer.WORLD_CREATUREs(objCharacter.TargetGUID)
        // _WorldServer.WORLD_CREATUREs(objCharacter.TargetGUID).SetToRealPosition()
        // End If
        // End If

        // Dim timeStart As Integer = _NativeMethods.timeGetTime("")

        // Dim height As Single = GetVMapHeight(objCharacter.MapID, objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ + 2.0F)

        // Dim isInLOS As Boolean = False
        // If target IsNot Nothing Then
        // isInLOS = IsInLineOfSight(objCharacter, target)
        // End If

        // Dim timeTaken As Integer = _NativeMethods.timeGetTime("") - timeStart

        // If height = _Global_Constants.VMAP_INVALID_HEIGHT_VALUE Then
        // objCharacter.CommandResponse(String.Format("Unable to retrieve VMap height for your location."))
        // Else
        // objCharacter.CommandResponse(String.Format("Your Z: {0}  VMap Z: {1}", objCharacter.positionZ, height))
        // End If

        // If target IsNot Nothing Then
        // objCharacter.CommandResponse(String.Format("Target in line of sight: {0}", isInLOS))
        // End If

        // objCharacter.CommandResponse(String.Format("Vmap functionality ran under [{0} ms].", timeTaken))
        // Else
        // objCharacter.CommandResponse("Vmaps is not enabled.")
        // End If
        // #Else
        // objCharacter.CommandResponse("Vmaps is not enabled.")
        // #End If
        // Return True
        // End Function

        // <ChatCommand("VmapTest2", "VMAPTEST2 - Tests VMAP functionality.", AccessLevel.Developer)>
        // Public Function cmdVmapTest2(ByRef objCharacter As CharacterObject, ByVal Message As String) As Boolean
        // #If VMAPS Then
        // If _WorldServer.Config.VMapsEnabled Then
        // If objCharacter.TargetGUID = 0UL OrElse _CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) = False OrElse _WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID) = False Then
        // objCharacter.CommandResponse("You must target a creature first.")
        // Else
        // _WorldServer.WORLD_CREATUREs(objCharacter.TargetGUID).SetToRealPosition()

        // Dim resX As Single = 0.0F
        // Dim resY As Single = 0.0F
        // Dim resZ As Single = 0.0F
        // Dim result As Boolean = GetObjectHitPos(objCharacter, _WorldServer.WORLD_CREATUREs(objCharacter.TargetGUID), resX, resY, resZ, -1.0F)

        // If result = False Then
        // objCharacter.CommandResponse("You teleported without any problems.")
        // Else
        // objCharacter.CommandResponse("You teleported by hitting something.")
        // End If

        // objCharacter.orientation = GetOrientation(objCharacter.positionX, _WorldServer.WORLD_CREATUREs(objCharacter.TargetGUID).positionX, objCharacter.positionY, _WorldServer.WORLD_CREATUREs(objCharacter.TargetGUID).positionY)
        // resZ = GetVMapHeight(objCharacter.MapID, resX, resY, resZ + 2.0F)
        // objCharacter.Teleport(resX, resY, resZ, objCharacter.orientation, objCharacter.MapID)
        // End If
        // Else
        // objCharacter.CommandResponse("Vmaps is not enabled.")
        // End If
        // #Else
        // objCharacter.CommandResponse("Vmaps is not enabled.")
        // #End If
        // Return True
        // End Function

        // <ChatCommand("VmapTest3", "VMAPTEST3 - Tests VMAP functionality.", AccessLevel.Developer)>
        // Public Function cmdVmapTest3(ByRef objCharacter As CharacterObject, ByVal Message As String) As Boolean
        // #If VMAPS Then
        // Dim CellMap As UInteger = objCharacter.MapID
        // Dim CellX As Byte = GetMapTileX(objCharacter.positionX)
        // Dim CellY As Byte = GetMapTileY(objCharacter.positionY)

        // Dim fileName As String = String.Format("{0}_{1}_{2}.vmdir", Format(CellMap, "000"), Format(CellX, "00"), Format(CellY, "00"))
        // If Not IO.File.Exists("vmaps\" & fileName) Then
        // objCharacter.CommandResponse(String.Format("VMap file [{0}] not found", fileName))
        // fileName = String.Format("{0}.vmdir", Format(CellMap, "000"))
        // End If

        // If Not IO.File.Exists("vmaps\" & fileName) Then
        // objCharacter.CommandResponse(String.Format("VMap file [{0}] not found", fileName))
        // Else
        // objCharacter.CommandResponse(String.Format("VMap file [{0}] found!", fileName))
        // Dim map As TMap = Maps(CellMap)
        // fileName = Trim(IO.File.ReadAllText("vmaps\" & fileName))

        // objCharacter.CommandResponse(String.Format("Full file: '{0}'", fileName))
        // If fileName.Contains(vbLf) Then
        // fileName = fileName.Substring(0, fileName.IndexOf(vbLf))
        // End If

        // objCharacter.CommandResponse(String.Format("First line: '{0}'", fileName))
        // Dim newModelLoaded As Boolean = False
        // If fileName.Length > 0 AndAlso IO.File.Exists("vmaps\" & fileName) Then
        // objCharacter.CommandResponse(String.Format("VMap file [{0}] found!", fileName))

        // If Maps(CellMap).ContainsModelContainer(fileName) Then
        // objCharacter.CommandResponse(String.Format("VMap ModelContainer is loaded!"))
        // Else
        // objCharacter.CommandResponse(String.Format("VMap ModelContainer is NOT loaded!"))
        // End If
        // Else
        // objCharacter.CommandResponse(String.Format("VMap file [{0}] not found!", fileName))
        // End If
        // End If
        // #Else
        // objCharacter.CommandResponse("Vmaps is not enabled.")
        // #End If
        // Return True
        // End Function

        // LOS Command
        [ChatCommand("los", "los #on/off - Enables/Disables line of sight calculation.", AccessLevel.Developer)]
        public bool cmdLineOfSight(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (Message.ToUpper() == "on")
            {
                WorldServiceLocator._WorldServer.Config.LineOfSightEnabled = true;
                objCharacter.CommandResponse("Line of Sight Calculation is now Enabled.");
            }
            else if (Message.ToUpper() == "on")
            {
                WorldServiceLocator._WorldServer.Config.LineOfSightEnabled = false;
                objCharacter.CommandResponse("Line of Sight Calculation is now Disabled.");
            }
            else
            {
                return false;
            }

            return true;
        }

        // GPS Command
        [ChatCommand("gps", "gps - Tells you where you are located.", AccessLevel.GameMaster)]
        public bool cmdGPS(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            objCharacter.CommandResponse("X: " + objCharacter.positionX);
            objCharacter.CommandResponse("Y: " + objCharacter.positionY);
            objCharacter.CommandResponse("Z: " + objCharacter.positionZ);
            objCharacter.CommandResponse("Orientation: " + objCharacter.orientation);
            objCharacter.CommandResponse("Map: " + objCharacter.MapID);
            return true;
        }

        // SetInstance Command
        [ChatCommand("SetInstance", "SETINSTANCE <ID> - Sets you into another instance.", AccessLevel.Admin)]
        public bool cmdSetInstance(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            int instanceID;
            if (int.TryParse(Message, out instanceID) == false)
                return false;
            if (instanceID < 0 || instanceID > 400000)
                return false;
            objCharacter.instance = (uint)instanceID;
            return true;
        }

        // Port Command
        [ChatCommand("port", "port #x #y #z #orientation #map - Teleports Character To Given Coordinates.", AccessLevel.GameMaster)]
        public bool cmdPort(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            string[] tmp;
            tmp = Message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (tmp.Length != 5)
                return false;
            float posX = Conversions.ToSingle(tmp[0]);
            float posY = Conversions.ToSingle(tmp[1]);
            float posZ = Conversions.ToSingle(tmp[2]);
            float posO = Conversions.ToSingle(tmp[3]);
            int posMap = (int)Conversions.ToSingle(tmp[4]);
            objCharacter.Teleport(posX, posY, posZ, posO, posMap);
            return true;
        }

        // Teleport Command
        [ChatCommand("teleport", "teleport #locationname - Teleports character to given location name.", AccessLevel.GameMaster)]
        public bool CmdPortByName(ref WS_PlayerData.CharacterObject objCharacter, string location)
        {
            if (string.IsNullOrEmpty(location))
                return false;
            float posX; // = 0
            float posY; // = 0
            float posZ; // = 0
            float posO; // = 0
            int posMap; // = 0
            if (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(location) == "LIST")
            {
                string cmdList = "Listing of available locations:" + Environment.NewLine;
                var listSqlQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM game_tele order by name", ref listSqlQuery);
                foreach (DataRow locationRow in listSqlQuery.Rows)
                    cmdList = Conversions.ToString(cmdList + Operators.ConcatenateObject(locationRow["name"], ", "));
                objCharacter.CommandResponse(cmdList);
                return true;
            }

            location = location.Replace("'", "").Replace(" ", "");
            location = location.Replace(";", ""); // Some SQL Safety added
            var mySqlQuery = new DataTable();
            if (location.Contains("*"))
            {
                location = location.Replace("*", "");
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM game_tele WHERE name like '{0}%' order by name;", location), ref mySqlQuery);
            }
            else
            {
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM game_tele WHERE name = '{0}' order by name LIMIT 1;", location), ref mySqlQuery);
            }

            if (mySqlQuery.Rows.Count > 0)
            {
                if (mySqlQuery.Rows.Count == 1)
                {
                    posX = Conversions.ToSingle(mySqlQuery.Rows[0]["position_x"]);
                    posY = Conversions.ToSingle(mySqlQuery.Rows[0]["position_y"]);
                    posZ = Conversions.ToSingle(mySqlQuery.Rows[0]["position_z"]);
                    posO = Conversions.ToSingle(mySqlQuery.Rows[0]["orientation"]);
                    posMap = Conversions.ToInteger(mySqlQuery.Rows[0]["map"]);
                    objCharacter.Teleport(posX, posY, posZ, posO, posMap);
                }
                else
                {
                    string cmdList = "Listing of matching locations:" + Environment.NewLine;
                    foreach (DataRow locationRow in mySqlQuery.Rows)
                        cmdList = Conversions.ToString(cmdList + Operators.ConcatenateObject(locationRow["name"], ", "));
                    objCharacter.CommandResponse(cmdList);
                    return true;
                }
            }
            else
            {
                objCharacter.CommandResponse(string.Format("Location {0} NOT found in Database", location));
            }

            return true;
        }

        // Kick Command
        [ChatCommand("kick", "kick #name (optional) - Kick selected player or character with name specified if found.", AccessLevel.GameMaster)]
        public bool cmdKick(ref WS_PlayerData.CharacterObject objCharacter, string Name)
        {
            if (string.IsNullOrEmpty(Name))
            {

                // DONE: Kick by selection
                if (objCharacter.TargetGUID == 0m)
                {
                    objCharacter.CommandResponse("No target selected.");
                }
                else if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
                {
                    // DONE: Kick gracefully
                    objCharacter.CommandResponse(string.Format("Character [{0}] kicked form server.", WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name));
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[{0}:{1}] Character [{3}] kicked by [{2}].", objCharacter.client.IP.ToString, objCharacter.client.Port, objCharacter.client.Character.Name, WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name);
                    WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Logout();
                }
                else
                {
                    objCharacter.CommandResponse(string.Format("Character GUID=[{0}] not found.", objCharacter.TargetGUID));
                }
            }
            else
            {

                // DONE: Kick by name
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                foreach (KeyValuePair<ulong, WS_PlayerData.CharacterObject> Character in WorldServiceLocator._WorldServer.CHARACTERs)
                {
                    if (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Character.Value.Name) == Name)
                    {
                        WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
                        // DONE: Kick gracefully
                        Character.Value.Logout();
                        objCharacter.CommandResponse(string.Format("Character [{0}] kicked form server.", Character.Value.Name));
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[{0}:{1}] Character [{3}] kicked by [{2}].", objCharacter.client.IP.ToString, objCharacter.client.Port, objCharacter.client.Character.Name, Name);
                        return true;
                    }
                }

                WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
                objCharacter.CommandResponse(string.Format("Character [{0:X}] not found.", Name));
            }

            return true;
        }

        // ForceRename
        // ToDo: Add option to use a player name as well
        [ChatCommand("forcerename", "forcerename - Force selected player to change his name next time on char enum.", AccessLevel.GameMaster)]
        public bool cmdForceRename(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("No target selected.");
            }
            else if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters SET force_restrictions = 1 WHERE char_guid = {0};", objCharacter.TargetGUID));
                objCharacter.CommandResponse("Player will be asked to change his name on next logon.");
            }
            else
            {
                objCharacter.CommandResponse(string.Format("Character GUID=[{0:X}] not found.", objCharacter.TargetGUID));
            }

            return true;
        }

        // BanCharacter Command
        // ToDo: Add option to use a player name as well
        [ChatCommand("bancharacter", "bancharacter - Selected player won't be able to login next time with this character.", AccessLevel.GameMaster)]
        public bool cmdBanChar(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("No target selected.");
            }
            else if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters SET force_restrictions = 2 WHERE char_guid = {0};", objCharacter.TargetGUID));
                objCharacter.CommandResponse("Character disabled.");
            }
            else
            {
                objCharacter.CommandResponse(string.Format("Character GUID=[{0:X}] not found.", objCharacter.TargetGUID));
            }

            return true;
        }

        // BanAccount Comand
        [ChatCommand("banaccount", "banaccount #account - Ban specified account from server.", AccessLevel.GameMaster)]
        public bool cmdBan(ref WS_PlayerData.CharacterObject objCharacter, string Name)
        {
            // TODO: Allow Reason For BAN to be Specified, and Inserted.
            if (string.IsNullOrEmpty(Name))
                return false;
            var account = new DataTable();
            WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT id, last_ip FROM account WHERE username = \"" + Name + "\";", ref account);
            ulong accountID = Conversions.ToULong(account.Rows[0]["id"]);
            int IP = Conversions.ToInteger(account.Rows[0]["last_ip"]);
            var result = new DataTable();
            WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT active FROM account_banned WHERE id = " + accountID + ";", ref result);
            if (result.Rows.Count > 0)
            {
                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(result.Rows[0]["active"], 1, false)))
                {
                    objCharacter.CommandResponse(string.Format("Account [{0}] already banned.", Name));
                }
                else
                {
                    // TODO: We May Want To Allow Account and IP to be Banned Separately
                    WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("INSERT INTO `account_banned` VALUES ('{0}', UNIX_TIMESTAMP({1}), UNIX_TIMESTAMP({2}), '{3}', '{4}', active = 1);", accountID, Strings.Format(DateAndTime.Now, "yyyy-MM-dd hh:mm:ss"), "0000-00-00 00:00:00", objCharacter.Name, "No Reason Specified."));
                    WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("INSERT INTO `ip_banned` VALUES ('{0}', UNIX_TIMESTAMP({1}), UNIX_TIMESTAMP({2}), '{3}', '{4}');", IP, Strings.Format(DateAndTime.Now, "yyyy-MM-dd hh:mm:ss"), "0000-00-00 00:00:00", objCharacter.Name, "No Reason Specified."));
                    objCharacter.CommandResponse(string.Format("Account [{0}] banned.", Name));
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[{0}:{1}] Account [{3}] banned by [{2}].", objCharacter.client.IP.ToString, objCharacter.client.Port, objCharacter.Name, Name);
                }
            }
            else
            {
                objCharacter.CommandResponse(string.Format("Account [{0}] not found.", Name));
            }

            return true;
        }

        // UnBan Command
        [ChatCommand("unban", "unban #account - Remove ban of specified account from server.", AccessLevel.Admin)]
        public bool cmdUnBan(ref WS_PlayerData.CharacterObject objCharacter, string Name)
        {
            if (string.IsNullOrEmpty(Name))
                return false;
            var account = new DataTable();
            WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT id, last_ip FROM account WHERE username = \"" + Name + "\";", ref  account);
            ulong accountID = Conversions.ToULong(account.Rows[0]["id"]);
            int IP = Conversions.ToInteger(account.Rows[0]["last_ip"]);
            var result = new DataTable();
            WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT active FROM account_banned WHERE id = '" + accountID + "';", ref result);
            if (result.Rows.Count > 0)
            {
                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(result.Rows[0]["active"], 0, false)))
                {
                    objCharacter.CommandResponse(string.Format("Account [{0}] is not banned.", Name));
                }
                else
                {
                    // TODO: Do we want to update the account_banned, ip_banned tables or DELETE the records?
                    WorldServiceLocator._WorldServer.AccountDatabase.Update("UPDATE account_banned SET active = 0 WHERE id = '" + accountID + "';");
                    WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("DELETE FROM `ip_banned` WHERE `ip` = '{0}';", IP));
                    objCharacter.CommandResponse(string.Format("Account [{0}] unbanned.", Name));
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[{0}:{1}] Account [{3}] unbanned by [{2}].", objCharacter.client.IP.ToString, objCharacter.client.Port, objCharacter.Name, Name);
                }
            }
            else
            {
                objCharacter.CommandResponse(string.Format("Account [{0}] not found.", Name));
            }

            return true;
        }

        // SetGM Command - not really working as it should right now
        [ChatCommand("setgm", "set gm #flag #invisibility - Toggles gameMaster status. You can use values like On/Off.", AccessLevel.GameMaster)]
        public bool cmdSetGM(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            var tmp = Strings.Split(Message, " ", 2);
            string value1 = tmp[0];
            string value2 = tmp[1];

            // setFaction(35);
            // _Functions.SetFlag(PLAYER_BYTES_2, 0x8);

            // Commnad: .setgm <gmflag:off/on> <invisibility:off/on>
            if (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(value1) == "off")
            {
                objCharacter.GM = false;
                objCharacter.CommandResponse("GameMaster Flag turned off.");
            }
            else if (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(value1) == "on")
            {
                objCharacter.GM = true;
                objCharacter.CommandResponse("GameMaster Flag turned on.");
            }

            if (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(value2) == "off")
            {
                objCharacter.Invisibility = InvisibilityLevel.VISIBLE;
                objCharacter.CanSeeInvisibility = InvisibilityLevel.VISIBLE;
                objCharacter.CommandResponse("GameMaster Invisibility turned off.");
            }
            else if (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(value1) == "on")
            {
                objCharacter.Invisibility = InvisibilityLevel.GM;
                objCharacter.CanSeeInvisibility = InvisibilityLevel.GM;
                objCharacter.CommandResponse("GameMaster Invisibility turned on.");
            }

            objCharacter.SetUpdateFlag((int)EPlayerFields.PLAYER_FLAGS, (int)objCharacter.cPlayerFlags);
            objCharacter.SendCharacterUpdate();
            WorldServiceLocator._WS_CharMovement.UpdateCell(ref objCharacter);
            return true;
        }

        // SetWeather Command
        [ChatCommand("setweather", "setweather #type #intensity - Change weather in current zone. Intensity is float value!", AccessLevel.Developer)]
        public bool cmdSetWeather(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            var tmp = Strings.Split(Message, " ", 2);
            int Type = Conversions.ToInteger(tmp[0]);
            float Intensity = Conversions.ToSingle(tmp[1]);
            if (WorldServiceLocator._WS_Weather.WeatherZones.ContainsKey(objCharacter.ZoneID) == false)
            {
                objCharacter.CommandResponse("No weather for this zone is found!");
            }
            else
            {
                WorldServiceLocator._WS_Weather.WeatherZones[objCharacter.ZoneID].CurrentWeather = (WeatherType)Type;
                WorldServiceLocator._WS_Weather.WeatherZones[objCharacter.ZoneID].Intensity = Intensity;
                WorldServiceLocator._WS_Weather.SendWeather(objCharacter.ZoneID, ref objCharacter.client);
            }

            return true;
        }

        // Remove Command
        // ToDo: Needs to be split in two commands
        [ChatCommand("remove", "remove #id - Delete selected creature or gameobject.", AccessLevel.Developer)]
        public bool cmdDeleteObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID))
            {
                // DONE: Delete creature
                if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
                {
                    objCharacter.CommandResponse("Selected target is not creature!");
                    return true;
                }

                WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].Destroy();
                objCharacter.CommandResponse("Creature deleted.");
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(objCharacter.TargetGUID))
            {
                // DONE: Delete GO
                if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(objCharacter.TargetGUID))
                {
                    objCharacter.CommandResponse("Selected target is not game object!");
                    return true;
                }

                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].Destroy(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID]);
                objCharacter.CommandResponse("Game object deleted.");
            }

            return true;
        }

        // Turn Command
        // ToDo: Needs to be split in two commands
        [ChatCommand("turn", "turn - Selected creature or game object will turn to your position.", AccessLevel.Developer)]
        public bool cmdTurnObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID))
            {
                // DONE: Turn creature
                if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
                {
                    objCharacter.CommandResponse("Selected target is not creature!");
                    return true;
                }

                WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].TurnTo(objCharacter.positionX, objCharacter.positionY);
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(objCharacter.TargetGUID))
            {
                // DONE: Turn GO
                if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(objCharacter.TargetGUID))
                {
                    objCharacter.CommandResponse("Selected target is not game object!");
                    return true;
                }

                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].TurnTo(objCharacter.positionX, objCharacter.positionY);
                var q = new DataTable();
                ulong GUID = objCharacter.TargetGUID - WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT;
                objCharacter.CommandResponse("Object rotation will be visible when the object is reloaded!");
            }

            return true;
        }

        // AddNpc Command
        [ChatCommand("npcadd", "npcadd #id - Spawn creature at your position.", AccessLevel.Developer)]
        public bool cmdAddCreature(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            var tmpCr = new WS_Creatures.CreatureObject(Conversions.ToInteger(Message), objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ, objCharacter.orientation, (int)objCharacter.MapID);
            tmpCr.AddToWorld();
            objCharacter.CommandResponse("Creature [" + tmpCr.Name + "] spawned.");
            return true;
        }

        // NpcCome Command
        [ChatCommand("npccome", "npccome - Selected creature will come to your position.", AccessLevel.Developer)]
        public bool cmdComeCreature(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
            {
                objCharacter.CommandResponse("Selected target is not creature!");
                return true;
            }

            var creature = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
            if (creature.aiScript is object && creature.aiScript.InCombat())
            {
                objCharacter.CommandResponse("Creature is in combat. It has to be out of combat first.");
                return true;
            }

            creature.SetToRealPosition(true);
            int MoveTime = creature.MoveTo(objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ, objCharacter.orientation);
            if (creature.aiScript is object)
            {
                creature.aiScript.Pause(MoveTime); // Make sure it doesn't do anything in this period
            }

            return true;
        }

        // Kill Command
        [ChatCommand("kill", "kill - Selected creature or character will die.", AccessLevel.GameMaster)]
        public bool cmdKillCreature(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (objCharacter.TargetGUID == 0m)
            {
                objCharacter.CommandResponse("Select target first!");
                return true;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
            {
                WS_Base.BaseUnit argAttacker = objCharacter;
                WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Die(ref argAttacker);
                return true;
            }
            else if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
            {
                WS_Base.BaseUnit argAttacker1 = null;
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].DealDamage(WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].Life.Maximum, Attacker: ref argAttacker1);
                return true;
            }

            return false;
        }

        // ObjectTarget Command
        [ChatCommand("gobjecttarget", "gobjecttarget - Nearest game object will be selected.", AccessLevel.Developer)]
        public bool cmdTargetGameObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            WS_Base.BaseUnit argunit = objCharacter;
            var targetGO = WorldServiceLocator._WS_GameObjects.GetClosestGameobject(ref argunit);
            if (targetGO is null)
            {
                objCharacter.CommandResponse("Could not find any near objects.");
            }
            else
            {
                float distance = WorldServiceLocator._WS_Combat.GetDistance(targetGO, objCharacter);
                objCharacter.CommandResponse(string.Format("Selected [{0}][{1}] game object at distance {2}.", targetGO.ID, targetGO.Name, distance));
            }

            return true;
        }

        // ActiveGameObject Command
        [ChatCommand("activatego", "activatego - Activates your targetted game object.", AccessLevel.Developer)]
        public bool cmdActivateGameObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(objCharacter.TargetGUID) == false)
                return false;
            if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].State == GameObjectLootState.DOOR_CLOSED)
            {
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].State = GameObjectLootState.DOOR_OPEN;
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].SetState(GameObjectLootState.DOOR_OPEN);
            }
            else
            {
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].State = GameObjectLootState.DOOR_CLOSED;
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].SetState(GameObjectLootState.DOOR_CLOSED);
            }

            objCharacter.CommandResponse(string.Format("Activated game object [{0}] to state [{1}].", WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].Name, WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].State));
            return true;
        }

        // GameObjectAdd Command
        [ChatCommand("gobjectadd", "gobjectadd #id - Spawn game object at your position.", AccessLevel.Developer)]
        public bool cmdAddGameObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            var tmpGO = new WS_GameObjects.GameObjectObject(Conversions.ToInteger(Message), objCharacter.MapID, objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ, objCharacter.orientation);
            tmpGO.Rotations[2] = (float)Math.Sin(tmpGO.orientation / 2f);
            tmpGO.Rotations[3] = (float)Math.Cos(tmpGO.orientation / 2f);
            tmpGO.AddToWorld();
            objCharacter.CommandResponse(string.Format("GameObject [{0}][{1:X}] spawned.", tmpGO.Name, tmpGO.GUID));
            return true;
        }

        // CreateAccount Command
        [ChatCommand("createaccount", "createaccount #account #password #email - Add a New account using Name, Password, And Email.", AccessLevel.Admin)]
        public bool cmdCreateAccount(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            var result = new DataTable();
            string[] acct;
            acct = Strings.Split(Strings.Trim(Message), " ");
            if (acct.Length != 3)
                return false;
            string aName = acct[0];
            string aPassword = acct[1];
            string aEmail = acct[2];
            WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT username FROM account WHERE username = \"" + aName + "\";", ref result);
            if (result.Rows.Count > 0)
            {
                objCharacter.CommandResponse(string.Format("Account [{0}] already exists.", aName));
            }
            else
            {
                var passwordStr = System.Text.Encoding.ASCII.GetBytes(aName.ToUpper() + ":" + aPassword.ToUpper());
                var passwordHash = new System.Security.Cryptography.SHA1Managed().ComputeHash(passwordStr);
                string hashStr = BitConverter.ToString(passwordHash).Replace("-", "");
                WorldServiceLocator._WorldServer.AccountDatabase.Insert(string.Format("INSERT INTO account (username, sha_pass_hash, email, joindate, last_ip) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", aName, hashStr, aEmail, Strings.Format(DateAndTime.Now, "yyyy-MM-dd"), "0.0.0.0"));
                objCharacter.CommandResponse(string.Format("Account [{0}] has been created.", aName));
            }

            return true;
        }

        // ChangePassword Command
        [ChatCommand("changepassword", "changepassword #account #password - Changes the password of an account.", AccessLevel.Admin)]
        public bool cmdChangePassword(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            var result = new DataTable();
            string[] acct;
            acct = Strings.Split(Strings.Trim(Message), " ");
            if (acct.Length != 2)
                return false;
            string aName = acct[0];
            string aPassword = acct[1];
            WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT id, gmlevel FROM account WHERE username = \"" + aName + "\";", ref result);
            if (result.Rows.Count == 0)
            {
                objCharacter.CommandResponse(string.Format("Account [{0}] does not exist.", aName));
            }
            else
            {
                AccessLevel targetLevel = (AccessLevel)result.Rows[0]["gmlevel"];
                if (targetLevel >= objCharacter.Access)
                {
                    objCharacter.CommandResponse("You cannot change password for accounts with the same or a higher access level than yourself.");
                }
                else
                {
                    var passwordStr = System.Text.Encoding.ASCII.GetBytes(aName.ToUpper() + ":" + aPassword.ToUpper());
                    var passwordHash = new System.Security.Cryptography.SHA1Managed().ComputeHash(passwordStr);
                    string hashStr = BitConverter.ToString(passwordHash).Replace("-", "");
                    WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("UPDATE account SET password='{0}' WHERE id={1}", hashStr, result.Rows[0]["id"]));
                    objCharacter.CommandResponse(string.Format("Account [{0}] now has a new password [{1}].", aName, aPassword));
                }
            }

            return true;
        }

        // SetAccess Command
        [ChatCommand("setaccess", "setaccess #account #level - Sets the account to a specific access level.", AccessLevel.Admin)]
        public bool cmdSetAccess(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            var result = new DataTable();
            string[] acct;
            acct = Strings.Split(Strings.Trim(Message), " ");
            if (acct.Length != 2)
                return false;
            string aName = acct[0];
            byte aLevel;
            if (byte.TryParse(acct[1], out aLevel) == false)
                return false;
            if (aLevel < AccessLevel.Trial || aLevel > AccessLevel.Developer)
            {
                objCharacter.CommandResponse(string.Format("Not a valid access level. Must be in the range {0}-{1}.", AccessLevel.Trial, AccessLevel.Developer));
                return true;
            }

            AccessLevel newLevel = (AccessLevel)aLevel;
            if (newLevel >= objCharacter.Access)
            {
                objCharacter.CommandResponse("You cannot set access levels to your own or above your own access level.");
                return true;
            }

            WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT id, gmlevel FROM account WHERE username = \"" + aName + "\";", ref result);
            if (result.Rows.Count == 0)
            {
                objCharacter.CommandResponse(string.Format("Account [{0}] does not exist.", aName));
            }
            else
            {
                AccessLevel targetLevel = (AccessLevel)result.Rows[0]["gmlevel"];
                if (targetLevel >= objCharacter.Access)
                {
                    objCharacter.CommandResponse("You cannot set access levels to accounts with the same or a higher access level than yourself.");
                }
                else
                {
                    WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("UPDATE account SET gmlevel={0} WHERE id={1}", newLevel, result.Rows[0]["id"]));
                    objCharacter.CommandResponse(string.Format("Account [{0}] now has access level [{1}].", aName, newLevel));
                }
            }

            return true;
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public ulong GetGUID(string Name)
        {
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT char_guid FROM characters WHERE char_name = \"{0}\";", Name), ref MySQLQuery);
            if (MySQLQuery.Rows.Count > 0)
            {
                return Conversions.ToULong(MySQLQuery.Rows[0]["char_guid"]);
            }
            else
            {
                return 0UL;
            }
        }

        public void SystemMessage(string Message)
        {
            var packet = WorldServiceLocator._Functions.BuildChatMessage(0, "System Message: " + Message, ChatMsg.CHAT_MSG_SYSTEM, LANGUAGES.LANG_UNIVERSAL, 0, "");
            packet.UpdateLength();
            WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.Broadcast(packet.Data);
            packet.Dispose();
        }

        public bool SetUpdateValue(ulong GUID, int Index, int Value, WS_Network.ClientClass client)
        {
            bool noErrors = true;
            try
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);      // Operations.Count
                packet.AddInt8(0);
                var UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                UpdateData.SetUpdateFlag(Index, Value);
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
                {
                    UpdateData.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID]);
                }
                else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(GUID))
                {
                    if (GUID == client.Character.GUID)
                    {
                        UpdateData.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
                    }
                    else
                    {
                        UpdateData.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
                    }
                }

                client.Send(packet);
                packet.Dispose();
                UpdateData.Dispose();
            }
            catch (DataException)
            {
                noErrors = false;
            }

            return noErrors;
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}