//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Mangos.Common;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Maps;
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
		public class ChatCommand
		{
			public string CommandHelp;

			public AccessLevel CommandAccess;

			public ChatCommandDelegate CommandDelegate;

			public ChatCommand()
			{
				CommandAccess = AccessLevel.GameMaster;
			}
		}

		public delegate bool ChatCommandDelegate(ref WS_PlayerData.CharacterObject objCharacter, string Message);

		public const ulong SystemGUID = 2147483647uL;

		public const string SystemNAME = "System";

		public Dictionary<string, ChatCommand> ChatCommands;

		public WS_Commands()
		{
			ChatCommands = new Dictionary<string, ChatCommand>();
		}

		public void RegisterChatCommands()
		{
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (Type tmpModule in types)
			{
				MethodInfo[] methods = tmpModule.GetMethods();
				foreach (MethodInfo tmpMethod in methods)
				{
					ChatCommandAttribute[] infos = (ChatCommandAttribute[])tmpMethod.GetCustomAttributes(typeof(ChatCommandAttribute), inherit: true);
					if (infos.Length != 0)
					{
						ChatCommandAttribute[] array = infos;
						foreach (ChatCommandAttribute info in array)
						{
                            ChatCommand chatCommand = new ChatCommand
                            {
                                CommandHelp = info.cmdHelp,
                                CommandAccess = info.cmdAccess,
                                CommandDelegate = (ChatCommandDelegate)Delegate.CreateDelegate(typeof(ChatCommandDelegate), WorldServiceLocator._WS_Commands, tmpMethod)
                            };
                            ChatCommand cmd = chatCommand;
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
				string[] tmp = Strings.Split(Message, " ", 2);
				ChatCommand Command = null;
				if (ChatCommands.ContainsKey(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(tmp[0])))
				{
					Command = ChatCommands[WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(tmp[0])];
				}
				string Arguments = "";
				if (tmp.Length == 2)
				{
					Arguments = Strings.Trim(tmp[1]);
				}
				string Name = client.Character.Name;
				if (Command == null)
				{
					client.Character.CommandResponse("Unknown command.");
					return;
				}
				if (Command.CommandAccess > client.Character.Access)
				{
					client.Character.CommandResponse("This command is not available for your access level.");
					return;
				}
				if (!Command.CommandDelegate(ref client.Character, Arguments))
				{
					client.Character.CommandResponse(Command.CommandHelp);
					return;
				}
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[{0}:{1}] {2} used command: {3}", client.IP, client.Port, Name, Message);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception err = ex;
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Client command caused error! {3}{2}", client.IP, client.Port, err.ToString(), Environment.NewLine);
				client.Character.CommandResponse(string.Format("Your command caused error:" + Environment.NewLine + " [{0}]", err.Message));
				ProjectData.ClearProjectError();
			}
		}

		[ChatCommand("help", "help #command\\r\\nDisplays usage information about command, if no command specified - displays list of available commands.", AccessLevel.GameMaster)]
		public bool Help(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Strings.Trim(Message), "", TextCompare: false) != 0)
			{
				ChatCommand Command2 = ChatCommands[Strings.Trim(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Message))];
				if (Command2 == null)
				{
					objCharacter.CommandResponse("Unknown command.");
				}
				else if (Command2.CommandAccess > objCharacter.Access)
				{
					objCharacter.CommandResponse("This command is not available for your access level.");
				}
				else
				{
					objCharacter.CommandResponse(Command2.CommandHelp);
				}
			}
			else
			{
				string cmdList = "Listing available commands:" + Environment.NewLine;
				foreach (KeyValuePair<string, ChatCommand> Command in ChatCommands)
				{
					if (Command.Value.CommandAccess <= objCharacter.Access)
					{
						cmdList = cmdList + WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Command.Key) + Environment.NewLine;
					}
				}
				cmdList = cmdList + Environment.NewLine + "Use help #command for usage information about particular command.";
				objCharacter.CommandResponse(cmdList);
			}
			return true;
		}

		[ChatCommand("castspell", "castspell #spellid #target - Selected unit will start casting spell. Target can be ME or SELF.", AccessLevel.Developer)]
		public bool cmdCastSpellMe(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			string[] tmp = Strings.Split(Message, " ", 2);
			if (tmp.Length < 2)
			{
				return false;
			}
			int SpellID = Conversions.ToInteger(tmp[0]);
			string Target = WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(tmp[1]);
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
			{
				if (Operators.CompareString(Target, "ME", TextCompare: false) != 0)
				{
					if (Operators.CompareString(Target, "SELF", TextCompare: false) == 0)
					{
						WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].CastSpell(SpellID, WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID]);
					}
				}
				else
				{
					WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].CastSpell(SpellID, objCharacter);
				}
			}
			else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				if (Operators.CompareString(Target, "ME", TextCompare: false) != 0)
				{
					if (Operators.CompareString(Target, "SELF", TextCompare: false) == 0)
					{
						WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].CastOnSelf(SpellID);
					}
				}
				else
				{
					WS_Spells.SpellTargets Targets = new WS_Spells.SpellTargets();
					WS_Spells.SpellTargets spellTargets = Targets;
					WS_Base.BaseUnit objCharacter2 = objCharacter;
					spellTargets.SetTarget_UNIT(ref objCharacter2);
					objCharacter = (WS_PlayerData.CharacterObject)objCharacter2;
					Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
					ulong targetGUID;
					WS_Base.BaseObject Caster = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[targetGUID = objCharacter.TargetGUID];
					WS_Spells.CastSpellParameters castSpellParameters = new WS_Spells.CastSpellParameters(ref Targets, ref Caster, SpellID);
					cHARACTERs[targetGUID] = (WS_PlayerData.CharacterObject)Caster;
					WS_Spells.CastSpellParameters castParams = castSpellParameters;
					ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
				}
			}
			else
			{
				objCharacter.CommandResponse($"GUID=[{objCharacter.TargetGUID:X}] not found or unsupported.");
			}
			return true;
		}

		[ChatCommand("control", "control - Takes or removes control over the selected unit.", AccessLevel.Admin)]
		public bool cmdControl(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (objCharacter.MindControl != null)
			{
				if (objCharacter.MindControl is WS_PlayerData.CharacterObject)
				{
					Packets.PacketClass packet1 = new Packets.PacketClass(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
					packet1.AddPackGUID(objCharacter.MindControl.GUID);
					packet1.AddInt8(1);
					((WS_PlayerData.CharacterObject)objCharacter.MindControl).client.Send(ref packet1);
					packet1.Dispose();
				}
				Packets.PacketClass packet4 = new Packets.PacketClass(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
				packet4.AddPackGUID(objCharacter.MindControl.GUID);
				packet4.AddInt8(0);
				objCharacter.client.Send(ref packet4);
				packet4.Dispose();
				objCharacter.cUnitFlags &= -16777217;
				objCharacter.SetUpdateFlag(712, 0);
				objCharacter.SetUpdateFlag(46, objCharacter.cUnitFlags);
				objCharacter.SendCharacterUpdate(toNear: false);
				objCharacter.MindControl = null;
				objCharacter.CommandResponse("Removed control over the unit.");
				return true;
			}
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				Packets.PacketClass packet2 = new Packets.PacketClass(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
				packet2.AddPackGUID(objCharacter.TargetGUID);
				packet2.AddInt8(0);
				WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].client.Send(ref packet2);
				packet2.Dispose();
				objCharacter.MindControl = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID];
			}
			else
			{
				if (!WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) || !WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
				{
					objCharacter.CommandResponse("You need a target.");
					return true;
				}
				objCharacter.MindControl = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
			}
			Packets.PacketClass packet3 = new Packets.PacketClass(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
			packet3.AddPackGUID(objCharacter.TargetGUID);
			packet3.AddInt8(1);
			objCharacter.client.Send(ref packet3);
			packet3.Dispose();
			objCharacter.cUnitFlags |= 0x1000000;
			objCharacter.SetUpdateFlag(712, objCharacter.TargetGUID);
			objCharacter.SetUpdateFlag(46, objCharacter.cUnitFlags);
			objCharacter.SendCharacterUpdate(toNear: false);
			objCharacter.CommandResponse("Taken control over a unit.");
			return true;
		}

		[ChatCommand("createguild", "createguild #guildname - Creates a guild.", AccessLevel.Developer)]
		public bool cmdCreateGuild(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			return true;
		}

		[ChatCommand("cast", "cast #spellid - You will start casting spell on selected target.", AccessLevel.Developer)]
		public bool cmdCastSpell(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			string[] tmp = Strings.Split(Message, " ", 2);
			int SpellID = Conversions.ToInteger(tmp[0]);
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
			{
				WS_Spells.SpellTargets Targets2 = new WS_Spells.SpellTargets();
				WS_Spells.SpellTargets spellTargets = Targets2;
				Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
				ulong targetGUID;
				WS_Base.BaseUnit objCharacter2 = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[targetGUID = objCharacter.TargetGUID];
				spellTargets.SetTarget_UNIT(ref objCharacter2);
				wORLD_CREATUREs[targetGUID] = (WS_Creatures.CreatureObject)objCharacter2;
				WS_Base.BaseObject Caster = objCharacter;
				WS_Spells.CastSpellParameters castSpellParameters = new WS_Spells.CastSpellParameters(ref Targets2, ref Caster, SpellID);
				objCharacter = (WS_PlayerData.CharacterObject)Caster;
				WS_Spells.CastSpellParameters castParams2 = castSpellParameters;
				ThreadPool.QueueUserWorkItem(new WaitCallback(castParams2.Cast));
				objCharacter.CommandResponse("You are now casting [" + Conversions.ToString(SpellID) + "] at [" + WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].Name + "].");
			}
			else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WS_Spells.SpellTargets Targets = new WS_Spells.SpellTargets();
				WS_Spells.SpellTargets spellTargets2 = Targets;
				Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
				ulong targetGUID;
				WS_Base.BaseUnit objCharacter2 = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[targetGUID = objCharacter.TargetGUID];
				spellTargets2.SetTarget_UNIT(ref objCharacter2);
				cHARACTERs[targetGUID] = (WS_PlayerData.CharacterObject)objCharacter2;
				WS_Base.BaseObject Caster = objCharacter;
				WS_Spells.CastSpellParameters castSpellParameters = new WS_Spells.CastSpellParameters(ref Targets, ref Caster, SpellID);
				objCharacter = (WS_PlayerData.CharacterObject)Caster;
				WS_Spells.CastSpellParameters castParams = castSpellParameters;
				ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
				objCharacter.CommandResponse("You are now casting [" + Conversions.ToString(SpellID) + "] at [" + WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name + "].");
			}
			else
			{
				objCharacter.CommandResponse($"GUID=[{objCharacter.TargetGUID:X}] not found or unsupported.");
			}
			return true;
		}

		[ChatCommand("save", "save - Saves selected character.", AccessLevel.Developer)]
		public bool cmdSave(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) != 0 && WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID))
			{
				WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Save();
				WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].CommandResponse($"Character {WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name} saved.");
			}
			else
			{
				objCharacter.Save();
				objCharacter.CommandResponse($"Character {objCharacter.Name} saved.");
			}
			return true;
		}

		[ChatCommand("spawndata", "spawndata - Tells you the spawn in memory information.", AccessLevel.Developer)]
		public bool cmdSpawns(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			objCharacter.CommandResponse("Spawns loaded in server memory:");
			objCharacter.CommandResponse("-------------------------------");
			objCharacter.CommandResponse("Creatures: " + Conversions.ToString(WorldServiceLocator._WorldServer.WORLD_CREATUREs.Count));
			objCharacter.CommandResponse("GameObjects: " + Conversions.ToString(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.Count));
			return true;
		}

		[ChatCommand("gobjectnear", "gobjectnear - Tells you the near objects count.", AccessLevel.Developer)]
		public bool cmdNear(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			objCharacter.CommandResponse("Near objects:");
			objCharacter.CommandResponse("-------------------------------");
			objCharacter.CommandResponse("Players: " + Conversions.ToString(objCharacter.playersNear.Count));
			objCharacter.CommandResponse("Creatures: " + Conversions.ToString(objCharacter.creaturesNear.Count));
			objCharacter.CommandResponse("GameObjects: " + Conversions.ToString(objCharacter.gameObjectsNear.Count));
			objCharacter.CommandResponse("Corpses: " + Conversions.ToString(objCharacter.corpseObjectsNear.Count));
			objCharacter.CommandResponse("-------------------------------");
			objCharacter.CommandResponse("You are seen by: " + Conversions.ToString(objCharacter.SeenBy.Count));
			return true;
		}

		[ChatCommand("npcai", "npcai #enable/disable - Enables/Disables  Creature AI updating.", AccessLevel.Developer)]
		public bool cmdAI(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Message), "ENABLE", TextCompare: false) == 0)
			{
				WorldServiceLocator._WS_TimerBasedEvents.AIManager.AIManagerTimer.Change(1000, 1000);
				objCharacter.CommandResponse("AI is enabled.");
			}
			else
			{
				if (Operators.CompareString(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Message), "DISABLE", TextCompare: false) != 0)
				{
					return false;
				}
				WorldServiceLocator._WS_TimerBasedEvents.AIManager.AIManagerTimer.Change(-1, -1);
				objCharacter.CommandResponse("AI is disabled.");
			}
			return true;
		}

		[ChatCommand("npcaistate", "npcaistate - Shows debug information about AI state of selected creature.", AccessLevel.Developer)]
		public bool cmdAIState(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("Select target first!");
				return false;
			}
			if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
			{
				objCharacter.CommandResponse("Selected target is not creature!");
				return false;
			}
			if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].aiScript == null)
			{
				objCharacter.CommandResponse("This creature doesn't have AI");
				return false;
			}
			WS_Creatures.CreatureObject creatureObject = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
			objCharacter.CommandResponse(string.Format("Information for creature [{0}]:{1}ai = {2}{1}state = {3}{1}maxdist = {4}", creatureObject.Name, Environment.NewLine, creatureObject.aiScript.ToString(), creatureObject.aiScript.State.ToString(), creatureObject.MaxDistance));
			objCharacter.CommandResponse("Hate table:");
			foreach (KeyValuePair<WS_Base.BaseUnit, int> u in creatureObject.aiScript.aiHateTable)
			{
				objCharacter.CommandResponse($"{u.Key.GUID:X} = {u.Value} hate");
			}

            return true;
		}

		[ChatCommand("servermessage", "servermessage #type #text - Send text message to all players on the server.", AccessLevel.GameMaster)]
		public bool cmdServerMessage(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			string[] tmp = Strings.Split(Message, " ", 2);
			if (tmp.Length != 2)
			{
				return false;
			}
			int Type = Conversions.ToInteger(tmp[0]);
			string Text = tmp[1];
			Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_SERVER_MESSAGE);
			packet.AddInt32(Type);
			packet.AddString(Text);
			packet.UpdateLength();
			WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.Broadcast(packet.Data);
			packet.Dispose();
			return true;
		}

		[ChatCommand("notifymessage", "notify #message - Send text message to all players on the server.", AccessLevel.GameMaster)]
		public bool cmdNotificationMessage(ref WS_PlayerData.CharacterObject objCharacter, string Text)
		{
			if (Operators.CompareString(Text, "", TextCompare: false) == 0)
			{
				return false;
			}
			Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_NOTIFICATION);
			packet.AddString(Text);
			packet.UpdateLength();
			WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.Broadcast(packet.Data);
			packet.Dispose();
			return true;
		}

		[ChatCommand("say", "say #text - Target NPC will say this.", AccessLevel.GameMaster)]
		public bool cmdSay(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Message, "", TextCompare: false) == 0)
			{
				return false;
			}
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				return false;
			}
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID))
			{
				WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].SendChatMessage(Message, ChatMsg.CHAT_MSG_MONSTER_SAY, LANGUAGES.LANG_GLOBAL, objCharacter.GUID);
				return true;
			}
			return false;
		}

		[ChatCommand("resetfactions", "resetfactions - Resets character reputation standings.", AccessLevel.Admin)]
		public bool cmdResetFactions(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WS_Player_Initializator wS_Player_Initializator = WorldServiceLocator._WS_Player_Initializator;
				Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
				ulong targetGUID;
				WS_PlayerData.CharacterObject objCharacter2 = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[targetGUID = objCharacter.TargetGUID];
				wS_Player_Initializator.InitializeReputations(ref objCharacter2);
				cHARACTERs[targetGUID] = objCharacter2;
				WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SaveCharacter();
			}
			else
			{
				WorldServiceLocator._WS_Player_Initializator.InitializeReputations(ref objCharacter);
				objCharacter.SaveCharacter();
			}
			return true;
		}

		[ChatCommand("skillmaster", "skillmaster - Get all spells and skills maxed out for your level.", AccessLevel.Developer)]
		public bool cmdGetMax(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			checked
			{
				foreach (KeyValuePair<int, WS_PlayerHelper.TSkill> skill in objCharacter.Skills)
				{
					skill.Value.Current = (short)skill.Value.Maximum;
					objCharacter.SetUpdateFlag(718 + objCharacter.SkillsPositions[skill.Key] * 3 + 1, objCharacter.Skills[skill.Key].GetSkill);
				}
				objCharacter.SendCharacterUpdate(toNear: false);
				return true;
			}
		}

		[ChatCommand("setlevel", "setlevel #level - Set the level of selected character.", AccessLevel.Developer)]
		public bool cmdSetLevel(ref WS_PlayerData.CharacterObject objCharacter, string tLevel)
		{
			if (!Versioned.IsNumeric(tLevel))
			{
				return false;
			}
			int Level = Conversions.ToInteger(tLevel);
			if (Level > WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
			{
				Level = WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL;
			}
			if (Level > 60)
			{
				Level = 60;
			}
			if (!WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				objCharacter.CommandResponse("Target not found or not character.");
				return true;
			}
			WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetLevel(checked((byte)Level));
			return true;
		}

		[ChatCommand("addxp", "addxp #amount - Add X experience points to selected character.", AccessLevel.Developer)]
		public bool cmdAddXP(ref WS_PlayerData.CharacterObject objCharacter, string tXP)
		{
			if (!Versioned.IsNumeric(tXP))
			{
				return false;
			}
			int XP = Conversions.ToInteger(tXP);
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].AddXP(XP, 0, 0uL);
			}
			else
			{
				objCharacter.CommandResponse("Target not found or not character.");
			}
			return true;
		}

		[ChatCommand("addrestedxp", "addrestedxp #amount - Add X rested bonus experience points to selected character.", AccessLevel.Developer)]
		public bool cmdAddRestedXP(ref WS_PlayerData.CharacterObject objCharacter, string tXP)
		{
			if (!Versioned.IsNumeric(tXP))
			{
				return false;
			}
			int XP = Conversions.ToInteger(tXP);
			checked
			{
				if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
				{
					WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].RestBonus += XP;
					WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].RestState = XPSTATE.Rested;
					WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetUpdateFlag(1175, WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].RestBonus);
					WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetUpdateFlag(194, WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].cPlayerBytes2);
					WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SendCharacterUpdate();
				}
				else
				{
					objCharacter.CommandResponse("Target not found or not character.");
				}
				return true;
			}
		}

		[ChatCommand("playsound", "playsound - Plays a specific sound for every player around you.", AccessLevel.Developer)]
		public bool cmdPlaySound(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
            if (!int.TryParse(Message, out int soundID))
            {
                return false;
            }
            objCharacter.SendPlaySound(soundID);
			return true;
		}

		[ChatCommand("combatlist", "combatlist - Lists everyone in your targets combatlist.", AccessLevel.Developer)]
		public bool cmdCombatList(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			ulong[] combatList = new ulong[0];
			combatList = ((decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0 || !WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID)) ? objCharacter.inCombatWith.ToArray() : WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].inCombatWith.ToArray());
			objCharacter.CommandResponse("Combat List (" + Conversions.ToString(combatList.Length) + "):");
			ulong[] array = combatList;
			foreach (ulong Guid in array)
			{
				objCharacter.CommandResponse($"* {Guid:X}");
			}
			return true;
		}

		[ChatCommand("cooldownlist", "cooldownlist - Lists all cooldowns of your target.", AccessLevel.GameMaster)]
		public bool cmdCooldownList(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			WS_Base.BaseUnit targetUnit = null;
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID))
			{
				if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
				{
					targetUnit = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID];
				}
			}
			else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
			{
				targetUnit = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
			}
			if (targetUnit == null)
			{
				targetUnit = objCharacter;
			}
			if (targetUnit == objCharacter)
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
					if (Spell.Value.Cooldown != 0)
					{
						uint timeLeft = 0u;
						if (timeNow < Spell.Value.Cooldown)
						{
							timeLeft = checked(Spell.Value.Cooldown - timeNow);
						}
						if (timeLeft > 0L)
						{
							sCooldowns = sCooldowns + "* Spell: " + Conversions.ToString(Spell.Key) + " - TimeLeft: " + WorldServiceLocator._Functions.GetTimeLeftString(timeLeft) + " sec - Item: " + Conversions.ToString(Spell.Value.CooldownItem) + Environment.NewLine;
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

		[ChatCommand("clearcooldowns", "clearcooldowns - Clears all cooldowns of your target.", AccessLevel.Developer)]
		public bool cmdClearCooldowns(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			WS_Base.BaseUnit targetUnit = null;
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID))
			{
				if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
				{
					targetUnit = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID];
				}
			}
			else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
			{
				targetUnit = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
			}
			if (targetUnit == null)
			{
				targetUnit = objCharacter;
			}
			if (targetUnit is WS_PlayerData.CharacterObject)
			{
				uint timeNow = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
				List<int> cooldownSpells = new List<int>();
				foreach (KeyValuePair<int, WS_Spells.CharacterSpell> Spell in ((WS_PlayerData.CharacterObject)targetUnit).Spells)
				{
					if (Spell.Value.Cooldown != 0)
					{
						Spell.Value.Cooldown = 0u;
						Spell.Value.CooldownItem = 0;
						WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_spells SET cooldown={2}, cooldownitem={3} WHERE guid = {0} AND spellid = {1};", objCharacter.GUID, Spell.Key, 0, 0));
						cooldownSpells.Add(Spell.Key);
					}
				}
				foreach (int SpellID in cooldownSpells)
				{
					Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_CLEAR_COOLDOWN);
					packet.AddInt32(SpellID);
					packet.AddUInt64(targetUnit.GUID);
					((WS_PlayerData.CharacterObject)targetUnit).client.Send(ref packet);
					packet.Dispose();
				}
			}
			else
			{
				objCharacter.CommandResponse("Cooldowns are not supported for creatures yet.");
			}
			return true;
		}

		[ChatCommand("additem", "additem #itemid #count (optional) - Add chosen items with item amount to selected character.", AccessLevel.GameMaster)]
		public bool cmdAddItem(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			string[] tmp = Strings.Split(Message, " ", 2);
			if (tmp.Length < 1)
			{
				return false;
			}
			int id = Conversions.ToInteger(tmp[0]);
			int Count = 1;
			if (tmp.Length == 2)
			{
				Count = Conversions.ToInteger(tmp[1]);
			}
			checked
			{
				if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
				{
					ItemObject newItem2 = new ItemObject(id, objCharacter.TargetGUID)
					{
						StackCount = Count
					};
					if (WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].ItemADD(ref newItem2))
					{
						WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].LogLootItem(newItem2, (byte)Count, Recieved: true, Created: false);
					}
					else
					{
						newItem2.Delete();
					}
				}
				else
				{
					ItemObject newItem = new ItemObject(id, objCharacter.GUID)
					{
						StackCount = Count
					};
					if (objCharacter.ItemADD(ref newItem))
					{
						objCharacter.LogLootItem(newItem, (byte)Count, Recieved: false, Created: true);
					}
					else
					{
						newItem.Delete();
					}
				}
				return true;
			}
		}

		[ChatCommand("additemset", "additemset #item - Add the items in the item set with id X to selected character.", AccessLevel.GameMaster)]
		public bool cmdAddItemSet(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			string[] tmp = Strings.Split(Message, " ", 2);
			if (tmp.Length < 1)
			{
				return false;
			}
			int id = Conversions.ToInteger(tmp[0]);
			if (WorldServiceLocator._WS_DBCDatabase.ItemSet.ContainsKey(id))
			{
				if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(objCharacter.TargetGUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
				{
					int[] itemID = WorldServiceLocator._WS_DBCDatabase.ItemSet[id].ItemID;
					foreach (int item2 in itemID)
					{
						ItemObject newItem2 = new ItemObject(item2, objCharacter.TargetGUID)
						{
							StackCount = 1
						};
						if (WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].ItemADD(ref newItem2))
						{
							WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].LogLootItem(newItem2, 1, Recieved: false, Created: true);
						}
						else
						{
							newItem2.Delete();
						}
					}
				}
				else
				{
					int[] itemID2 = WorldServiceLocator._WS_DBCDatabase.ItemSet[id].ItemID;
					foreach (int item in itemID2)
					{
						ItemObject newItem = new ItemObject(item, objCharacter.GUID)
						{
							StackCount = 1
						};
						if (objCharacter.ItemADD(ref newItem))
						{
							objCharacter.LogLootItem(newItem, 1, Recieved: false, Created: true);
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

		[ChatCommand("addmoney", "addmoney #amount - Add chosen copper to your character or selected character.", AccessLevel.GameMaster)]
		public bool cmdAddMoney(ref WS_PlayerData.CharacterObject objCharacter, string tCopper)
		{
			if (Operators.CompareString(tCopper, "", TextCompare: false) == 0)
			{
				return false;
			}
			ulong Copper = Conversions.ToULong(tCopper);
			checked
			{
				if (objCharacter.Copper + Copper > uint.MaxValue)
				{
					objCharacter.Copper = uint.MaxValue;
				}
				else
				{
					ref uint copper = ref objCharacter.Copper;
					copper = (uint)(copper + Copper);
				}
				objCharacter.SetUpdateFlag(1176, objCharacter.Copper);
				objCharacter.SendCharacterUpdate(toNear: false);
				return true;
			}
		}

		[ChatCommand("learnskill", "learnskill #id #current #max - Add skill id X with value Y of Z to selected character.", AccessLevel.Developer)]
		public bool cmdLearnSkill(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Message, "", TextCompare: false) == 0)
			{
				return false;
			}
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				string[] tmp = Strings.Split(Strings.Trim(Message));
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

		[ChatCommand("learnSpell", "learnSpell #id - Add chosen spell to selected character.", AccessLevel.Developer)]
		public bool cmdLearnSpell(ref WS_PlayerData.CharacterObject objCharacter, string tID)
		{
			if (Operators.CompareString(tID, "", TextCompare: false) == 0)
			{
				return false;
			}
			if (!int.TryParse(tID, out var ID) || ID < 0)
			{
				return false;
			}
			if (!WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(ID))
			{
				objCharacter.CommandResponse("You tried learning a spell that did not exist.");
				return false;
			}
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].LearnSpell(ID);
				if (objCharacter.TargetGUID == objCharacter.GUID)
				{
					objCharacter.CommandResponse("You learned spell: " + Conversions.ToString(ID));
				}
				else
				{
					objCharacter.CommandResponse(WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name + " has learned spell: " + Conversions.ToString(ID));
				}
			}
			else
			{
				objCharacter.CommandResponse("Target not found or not character.");
			}
			return true;
		}

		[ChatCommand("unlearnspell", "unlearnspell #id - Remove chosen spell from selected character.", AccessLevel.Developer)]
		public bool cmdUnlearnSpell(ref WS_PlayerData.CharacterObject objCharacter, string tID)
		{
			if (Operators.CompareString(tID, "", TextCompare: false) == 0)
			{
				return false;
			}
			int ID = Conversions.ToInteger(tID);
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].UnLearnSpell(ID);
				if (objCharacter.TargetGUID == objCharacter.GUID)
				{
					objCharacter.CommandResponse("You unlearned spell: " + Conversions.ToString(ID));
				}
				else
				{
					objCharacter.CommandResponse(WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name + " has unlearned spell: " + Conversions.ToString(ID));
				}
			}
			else
			{
				objCharacter.CommandResponse("Target not found or not character.");
			}
			return true;
		}

		[ChatCommand("showtaxi", "showtaxi - Unlock all taxi locations.", AccessLevel.Developer)]
		public bool cmdShowTaxi(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			objCharacter.TaxiZones.SetAll(value: true);
			return true;
		}

		[ChatCommand("setcharacterspeed", "setcharacterspeed #value - Change your character travel speed.", AccessLevel.GameMaster)]
		public bool cmdSetCharacterSpeed(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Message, "", TextCompare: false) == 0)
			{
				return false;
			}
			objCharacter.ChangeSpeedForced(ChangeSpeedType.RUN, Conversions.ToSingle(Message));
			objCharacter.CommandResponse("Your RunSpeed is changed to " + Message);
			objCharacter.ChangeSpeedForced(ChangeSpeedType.SWIM, Conversions.ToSingle(Message));
			objCharacter.CommandResponse("Your SwimSpeed is changed to " + Message);
			objCharacter.ChangeSpeedForced(ChangeSpeedType.SWIMBACK, Conversions.ToSingle(Message));
			objCharacter.CommandResponse("Your RunBackSpeed is changed to " + Message);
			return true;
		}

		[ChatCommand("setreputation", "setreputation #faction #value - Change your reputation standings.", AccessLevel.GameMaster)]
		public bool cmdSetReputation(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Message, "", TextCompare: false) == 0)
			{
				return false;
			}
			string[] tmp = Strings.Split(Message, " ", 2);
			objCharacter.SetReputation(Conversions.ToInteger(tmp[0]), Conversions.ToInteger(tmp[1]));
			objCharacter.CommandResponse("You have set your reputation with [" + tmp[0] + "] to [" + tmp[1] + "]");
			return true;
		}

		[ChatCommand("changemodel", "changemodel #id - Will morph you into specified model ID.", AccessLevel.GameMaster)]
		public bool cmdModel(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
            if (!int.TryParse(Message, out int value) || value < 0)
            {
                return false;
            }
            if (WorldServiceLocator._WS_DBCDatabase.CreatureModel.ContainsKey(value))
			{
				objCharacter.BoundingRadius = WorldServiceLocator._WS_DBCDatabase.CreatureModel[value].BoundingRadius;
				objCharacter.CombatReach = WorldServiceLocator._WS_DBCDatabase.CreatureModel[value].CombatReach;
			}
			objCharacter.SetUpdateFlag(129, objCharacter.BoundingRadius);
			objCharacter.SetUpdateFlag(130, objCharacter.CombatReach);
			objCharacter.SetUpdateFlag(131, value);
			objCharacter.SendCharacterUpdate();
			return true;
		}

		[ChatCommand("mount", "mount #id - Will mount you to specified model ID.", AccessLevel.GameMaster)]
		public bool cmdMount(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
            if (!int.TryParse(Message, out int value) || value < 0)
            {
                return false;
            }
            objCharacter.SetUpdateFlag(133, value);
			objCharacter.SendCharacterUpdate();
			return true;
		}

		[ChatCommand("hurt", "hurt - Hurts a selected character.", AccessLevel.GameMaster)]
		public bool cmdHurt(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("Select target first!");
				return true;
			}
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WS_PlayerHelper.TStatBar life;
				(life = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Life).Current = checked((int)Math.Round(life.Current - WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Life.Maximum * 0.1));
				WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SetUpdateFlag(22, WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Life.Current);
				WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].SendCharacterUpdate();
				return true;
			}
			return true;
		}

		[ChatCommand("root", "root - Instantly root selected character.", AccessLevel.GameMaster)]
		public bool cmdRoot(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
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

		[ChatCommand("unroot", "unroot - Instantly unroot selected character.", AccessLevel.GameMaster)]
		public bool cmdUnRoot(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
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

		[ChatCommand("revive", "revive - Instantly revive selected character.", AccessLevel.GameMaster)]
		public bool cmdRevive(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("Select target first!");
				return true;
			}
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WS_Handlers_Misc wS_Handlers_Misc = WorldServiceLocator._WS_Handlers_Misc;
				Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
				ulong targetGUID;
				WS_PlayerData.CharacterObject Character = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[targetGUID = objCharacter.TargetGUID];
				wS_Handlers_Misc.CharacterResurrect(ref Character);
				cHARACTERs[targetGUID] = Character;
				return true;
			}
			return true;
		}

		[ChatCommand("gotogy", "gotogy - Instantly teleports selected character to nearest graveyard.", AccessLevel.GameMaster)]
		public bool cmdGoToGraveyard(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("Select target first!");
				return true;
			}
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WS_GraveYards allGraveYards = WorldServiceLocator._WorldServer.AllGraveYards;
				Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
				ulong targetGUID;
				WS_PlayerData.CharacterObject Character = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[targetGUID = objCharacter.TargetGUID];
				allGraveYards.GoToNearestGraveyard(ref Character, Alive: false, Teleport: true);
				cHARACTERs[targetGUID] = Character;
				return true;
			}
			return true;
		}

		[ChatCommand("tostart", "tostart #race - Instantly teleports selected character to specified race start location.", AccessLevel.GameMaster)]
		public bool cmdGoToStart(ref WS_PlayerData.CharacterObject objCharacter, string StringRace)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("Select target first!");
				return true;
			}
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				DataTable Info = new DataTable();
				WS_PlayerData.CharacterObject Character = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID];
				Races Race;
				switch (WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(StringRace))
				{
					case "DWARF":
					case "DW":
						Race = Races.RACE_DWARF;
						break;
					case "GNOME":
					case "GN":
						Race = Races.RACE_GNOME;
						break;
					case "HUMAN":
					case "HU":
						Race = Races.RACE_HUMAN;
						break;
					case "NIGHTELF":
					case "NE":
						Race = Races.RACE_NIGHT_ELF;
						break;
					case "ORC":
					case "OR":
						Race = Races.RACE_ORC;
						break;
					case "TAUREN":
					case "TA":
						Race = Races.RACE_TAUREN;
						break;
					case "TROLL":
					case "TR":
						Race = Races.RACE_TROLL;
						break;
					case "UNDEAD":
					case "UN":
						Race = Races.RACE_UNDEAD;
						break;
					default:
						objCharacter.CommandResponse("Unknown race. Use DW, GN, HU, NE, OR, TA, TR, UN for race.");
						return true;
				}
				WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM playercreateinfo WHERE race = {(int)Race};", ref Info);
				Character.Teleport(Conversions.ToSingle(Info.Rows[0]["position_x"]), Conversions.ToSingle(Info.Rows[0]["position_y"]), Conversions.ToSingle(Info.Rows[0]["position_z"]), Conversions.ToSingle(Info.Rows[0]["orientation"]), Conversions.ToInteger(Info.Rows[0]["map"]));
				return true;
			}
			return true;
		}

		[ChatCommand("summon", "summon #name - Instantly teleports the player to you.", AccessLevel.GameMaster)]
		public bool cmdSummon(ref WS_PlayerData.CharacterObject objCharacter, string Name)
		{
			ulong GUID = GetGUID(WorldServiceLocator._Functions.CapitalizeName(ref Name));
			checked
			{
				if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
				{
					if (objCharacter.OnTransport != null)
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
				objCharacter.CommandResponse("Player not found.");
				return true;
			}
		}

		[ChatCommand("appear", "appear #name - Instantly teleports you to the player.", AccessLevel.GameMaster)]
		public bool cmdAppear(ref WS_PlayerData.CharacterObject objCharacter, string Name)
		{
			ulong GUID = GetGUID(WorldServiceLocator._Functions.CapitalizeName(ref Name));
			checked
			{
				if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
				{
					WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[GUID];
					if (characterObject.OnTransport != null)
					{
						objCharacter.OnTransport = characterObject.OnTransport;
						objCharacter.Transfer(characterObject.positionX, characterObject.positionY, characterObject.positionZ, characterObject.orientation, (int)characterObject.MapID);
					}
					else
					{
						objCharacter.Teleport(characterObject.positionX, characterObject.positionY, characterObject.positionZ, characterObject.orientation, (int)characterObject.MapID);
					}

                    return true;
				}
				objCharacter.CommandResponse("Player not found.");
				return true;
			}
		}

		[ChatCommand("los", "los #on/off - Enables/Disables line of sight calculation.", AccessLevel.Developer)]
		public bool cmdLineOfSight(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Message.ToUpper(), "on", TextCompare: false) == 0)
			{
				WorldServiceLocator._ConfigurationProvider.GetConfiguration().LineOfSightEnabled = true;
				objCharacter.CommandResponse("Line of Sight Calculation is now Enabled.");
			}
			else
			{
				if (Operators.CompareString(Message.ToUpper(), "on", TextCompare: false) != 0)
				{
					return false;
				}
				WorldServiceLocator._ConfigurationProvider.GetConfiguration().LineOfSightEnabled = false;
				objCharacter.CommandResponse("Line of Sight Calculation is now Disabled.");
			}
			return true;
		}

		[ChatCommand("gps", "gps - Tells you where you are located.", AccessLevel.GameMaster)]
		public bool cmdGPS(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			objCharacter.CommandResponse("X: " + Conversions.ToString(objCharacter.positionX));
			objCharacter.CommandResponse("Y: " + Conversions.ToString(objCharacter.positionY));
			objCharacter.CommandResponse("Z: " + Conversions.ToString(objCharacter.positionZ));
			objCharacter.CommandResponse("Orientation: " + Conversions.ToString(objCharacter.orientation));
			objCharacter.CommandResponse("Map: " + Conversions.ToString(objCharacter.MapID));
			return true;
		}

		[ChatCommand("SetInstance", "SETINSTANCE <ID> - Sets you into another instance.", AccessLevel.Admin)]
		public bool cmdSetInstance(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
            if (!int.TryParse(Message, out int instanceID))
            {
                return false;
            }
            if (instanceID < 0 || instanceID > 400000)
			{
				return false;
			}
			objCharacter.instance = checked((uint)instanceID);
			return true;
		}

		[ChatCommand("port", "port #x #y #z #orientation #map - Teleports Character To Given Coordinates.", AccessLevel.GameMaster)]
		public bool cmdPort(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Message, "", TextCompare: false) == 0)
			{
				return false;
			}
			string[] tmp = Message.Split(new string[1]
			{
				" "
			}, StringSplitOptions.RemoveEmptyEntries);
			if (tmp.Length != 5)
			{
				return false;
			}
			float posX = Conversions.ToSingle(tmp[0]);
			float posY = Conversions.ToSingle(tmp[1]);
			float posZ = Conversions.ToSingle(tmp[2]);
			float posO = Conversions.ToSingle(tmp[3]);
			int posMap = checked((int)Math.Round(Conversions.ToSingle(tmp[4])));
			objCharacter.Teleport(posX, posY, posZ, posO, posMap);
			return true;
		}

		[ChatCommand("teleport", "teleport #locationname - Teleports character to given location name.", AccessLevel.GameMaster)]
		public bool CmdPortByName(ref WS_PlayerData.CharacterObject objCharacter, string location)
		{
			if (Operators.CompareString(location, "", TextCompare: false) == 0)
			{
				return false;
			}
			if (Operators.CompareString(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(location), "LIST", TextCompare: false) == 0)
			{
				string cmdList = "Listing of available locations:" + Environment.NewLine;
				DataTable listSqlQuery = new DataTable();
				WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM game_tele order by name", ref listSqlQuery);
				IEnumerator enumerator = default;
				try
				{
					enumerator = listSqlQuery.Rows.GetEnumerator();
					while (enumerator.MoveNext())
					{
						DataRow row = (DataRow)enumerator.Current;
						cmdList = Conversions.ToString(Operators.AddObject(cmdList, Operators.ConcatenateObject(row["name"], ", ")));
					}
				}
				finally
				{
					if (enumerator is IDisposable)
					{
						(enumerator as IDisposable).Dispose();
					}
				}
				objCharacter.CommandResponse(cmdList);
				return true;
			}
			location = location.Replace("'", "").Replace(" ", "");
			location = location.Replace(";", "");
			DataTable mySqlQuery = new DataTable();
			if (location.Contains("*"))
			{
				location = location.Replace("*", "");
				WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM game_tele WHERE name like '{location}%' order by name;", ref mySqlQuery);
			}
			else
			{
				WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM game_tele WHERE name = '{location}' order by name LIMIT 1;", ref mySqlQuery);
			}
			if (mySqlQuery.Rows.Count > 0)
			{
				if (mySqlQuery.Rows.Count != 1)
				{
					string cmdList2 = "Listing of matching locations:" + Environment.NewLine;
					IEnumerator enumerator2 = default;
					try
					{
						enumerator2 = mySqlQuery.Rows.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							DataRow row = (DataRow)enumerator2.Current;
							cmdList2 = Conversions.ToString(Operators.AddObject(cmdList2, Operators.ConcatenateObject(row["name"], ", ")));
						}
					}
					finally
					{
						if (enumerator2 is IDisposable)
						{
							(enumerator2 as IDisposable).Dispose();
						}
					}
					objCharacter.CommandResponse(cmdList2);
					return true;
				}
				float posX = mySqlQuery.Rows[0].As<float>("position_x");
				float posY = mySqlQuery.Rows[0].As<float>("position_y");
				float posZ = mySqlQuery.Rows[0].As<float>("position_z");
				float posO = mySqlQuery.Rows[0].As<float>("orientation");
				int posMap = mySqlQuery.Rows[0].As<int>("map");
				objCharacter.Teleport(posX, posY, posZ, posO, posMap);
			}
			else
			{
				objCharacter.CommandResponse($"Location {location} NOT found in Database");
			}
			return true;
		}

		[ChatCommand("kick", "kick #name (optional) - Kick selected player or character with name specified if found.", AccessLevel.GameMaster)]
		public bool cmdKick(ref WS_PlayerData.CharacterObject objCharacter, string Name)
		{
			if (Operators.CompareString(Name, "", TextCompare: false) == 0)
			{
				if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
				{
					objCharacter.CommandResponse("No target selected.");
				}
				else if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
				{
					objCharacter.CommandResponse($"Character [{WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name}] kicked form server.");
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[{0}:{1}] Character [{3}] kicked by [{2}].", objCharacter.client.IP.ToString(), objCharacter.client.Port, objCharacter.client.Character.Name, WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Name);
					WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID].Logout();
				}
				else
				{
					objCharacter.CommandResponse($"Character GUID=[{objCharacter.TargetGUID}] not found.");
				}
			}
			else
			{
				WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
				foreach (KeyValuePair<ulong, WS_PlayerData.CharacterObject> Character in WorldServiceLocator._WorldServer.CHARACTERs)
				{
					if (Operators.CompareString(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Character.Value.Name), Name, TextCompare: false) == 0)
					{
						WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
						Character.Value.Logout();
						objCharacter.CommandResponse($"Character [{Character.Value.Name}] kicked form server.");
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[{0}:{1}] Character [{3}] kicked by [{2}].", objCharacter.client.IP.ToString(), objCharacter.client.Port, objCharacter.client.Character.Name, Name);
						return true;
					}
				}
				WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
				objCharacter.CommandResponse($"Character [{Name:X}] not found.");
			}
			return true;
		}

		[ChatCommand("forcerename", "forcerename - Force selected player to change his name next time on char enum.", AccessLevel.GameMaster)]
		public bool cmdForceRename(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("No target selected.");
			}
			else if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters SET force_restrictions = 1 WHERE char_guid = {objCharacter.TargetGUID};");
				objCharacter.CommandResponse("Player will be asked to change his name on next logon.");
			}
			else
			{
				objCharacter.CommandResponse($"Character GUID=[{objCharacter.TargetGUID:X}] not found.");
			}
			return true;
		}

		[ChatCommand("bancharacter", "bancharacter - Selected player won't be able to login next time with this character.", AccessLevel.GameMaster)]
		public bool cmdBanChar(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("No target selected.");
			}
			else if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters SET force_restrictions = 2 WHERE char_guid = {objCharacter.TargetGUID};");
				objCharacter.CommandResponse("Character disabled.");
			}
			else
			{
				objCharacter.CommandResponse($"Character GUID=[{objCharacter.TargetGUID:X}] not found.");
			}
			return true;
		}

		[ChatCommand("banaccount", "banaccount #account - Ban specified account from server.", AccessLevel.GameMaster)]
		public bool cmdBan(ref WS_PlayerData.CharacterObject objCharacter, string Name)
		{
			if (Operators.CompareString(Name, "", TextCompare: false) == 0)
			{
				return false;
			}
			DataTable account = new DataTable();
			WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT id, last_ip FROM account WHERE username = \"" + Name + "\";", ref account);
			ulong accountID = Conversions.ToULong(account.Rows[0]["id"]);
			int IP = Conversions.ToInteger(account.Rows[0]["last_ip"]);
			DataTable result = new DataTable();
			WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT active FROM account_banned WHERE id = " + Conversions.ToString(accountID) + ";", ref result);
			if (result.Rows.Count > 0)
			{
				if (Operators.ConditionalCompareObjectEqual(result.Rows[0]["active"], 1, TextCompare: false))
				{
					objCharacter.CommandResponse($"Account [{Name}] already banned.");
				}
				else
				{
					WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("INSERT INTO `account_banned` VALUES ('{0}', UNIX_TIMESTAMP({1}), UNIX_TIMESTAMP({2}), '{3}', '{4}', active = 1);", accountID, Strings.Format(DateAndTime.Now, "yyyy-MM-dd hh:mm:ss"), "0000-00-00 00:00:00", objCharacter.Name, "No Reason Specified."));
					WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("INSERT INTO `ip_banned` VALUES ('{0}', UNIX_TIMESTAMP({1}), UNIX_TIMESTAMP({2}), '{3}', '{4}');", IP, Strings.Format(DateAndTime.Now, "yyyy-MM-dd hh:mm:ss"), "0000-00-00 00:00:00", objCharacter.Name, "No Reason Specified."));
					objCharacter.CommandResponse($"Account [{Name}] banned.");
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[{0}:{1}] Account [{3}] banned by [{2}].", objCharacter.client.IP.ToString(), objCharacter.client.Port, objCharacter.Name, Name);
				}
			}
			else
			{
				objCharacter.CommandResponse($"Account [{Name}] not found.");
			}
			return true;
		}

		[ChatCommand("unban", "unban #account - Remove ban of specified account from server.", AccessLevel.Admin)]
		public bool cmdUnBan(ref WS_PlayerData.CharacterObject objCharacter, string Name)
		{
			if (Operators.CompareString(Name, "", TextCompare: false) == 0)
			{
				return false;
			}
			DataTable account = new DataTable();
			WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT id, last_ip FROM account WHERE username = \"" + Name + "\";", ref account);
			ulong accountID = Conversions.ToULong(account.Rows[0]["id"]);
			int IP = Conversions.ToInteger(account.Rows[0]["last_ip"]);
			DataTable result = new DataTable();
			WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT active FROM account_banned WHERE id = '" + Conversions.ToString(accountID) + "';", ref result);
			if (result.Rows.Count > 0)
			{
				if (Operators.ConditionalCompareObjectEqual(result.Rows[0]["active"], 0, TextCompare: false))
				{
					objCharacter.CommandResponse($"Account [{Name}] is not banned.");
				}
				else
				{
					WorldServiceLocator._WorldServer.AccountDatabase.Update("UPDATE account_banned SET active = 0 WHERE id = '" + Conversions.ToString(accountID) + "';");
					WorldServiceLocator._WorldServer.AccountDatabase.Update($"DELETE FROM `ip_banned` WHERE `ip` = '{IP}';");
					objCharacter.CommandResponse($"Account [{Name}] unbanned.");
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[{0}:{1}] Account [{3}] unbanned by [{2}].", objCharacter.client.IP.ToString(), objCharacter.client.Port, objCharacter.Name, Name);
				}
			}
			else
			{
				objCharacter.CommandResponse($"Account [{Name}] not found.");
			}
			return true;
		}

		[ChatCommand("setgm", "set gm #flag #invisibility - Toggles gameMaster status. You can use values like On/Off.", AccessLevel.GameMaster)]
		public bool cmdSetGM(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			string[] tmp = Strings.Split(Message, " ", 2);
			string value1 = tmp[0];
			string value2 = tmp[1];
			if (Operators.CompareString(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(value1), "off", TextCompare: false) == 0)
			{
				objCharacter.GM = false;
				objCharacter.CommandResponse("GameMaster Flag turned off.");
			}
			else if (Operators.CompareString(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(value1), "on", TextCompare: false) == 0)
			{
				objCharacter.GM = true;
				objCharacter.CommandResponse("GameMaster Flag turned on.");
			}
			if (Operators.CompareString(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(value2), "off", TextCompare: false) == 0)
			{
				objCharacter.Invisibility = InvisibilityLevel.VISIBLE;
				objCharacter.CanSeeInvisibility = InvisibilityLevel.VISIBLE;
				objCharacter.CommandResponse("GameMaster Invisibility turned off.");
			}
			else if (Operators.CompareString(WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(value1), "on", TextCompare: false) == 0)
			{
				objCharacter.Invisibility = InvisibilityLevel.GM;
				objCharacter.CanSeeInvisibility = InvisibilityLevel.GM;
				objCharacter.CommandResponse("GameMaster Invisibility turned on.");
			}
			objCharacter.SetUpdateFlag(190, (int)objCharacter.cPlayerFlags);
			objCharacter.SendCharacterUpdate();
			WorldServiceLocator._WS_CharMovement.UpdateCell(ref objCharacter);
			return true;
		}

		[ChatCommand("setweather", "setweather #type #intensity - Change weather in current zone. Intensity is float value!", AccessLevel.Developer)]
		public bool cmdSetWeather(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			string[] tmp = Strings.Split(Message, " ", 2);
			int Type = Conversions.ToInteger(tmp[0]);
			float Intensity = Conversions.ToSingle(tmp[1]);
			if (!WorldServiceLocator._WS_Weather.WeatherZones.ContainsKey(objCharacter.ZoneID))
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

		[ChatCommand("remove", "remove #id - Delete selected creature or gameobject.", AccessLevel.Developer)]
		public bool cmdDeleteObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("Select target first!");
				return true;
			}
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID))
			{
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

		[ChatCommand("turn", "turn - Selected creature or game object will turn to your position.", AccessLevel.Developer)]
		public bool cmdTurnObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("Select target first!");
				return true;
			}
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(objCharacter.TargetGUID))
			{
				if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
				{
					objCharacter.CommandResponse("Selected target is not creature!");
					return true;
				}
				WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].TurnTo(objCharacter.positionX, objCharacter.positionY);
			}
			else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(objCharacter.TargetGUID))
			{
				if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(objCharacter.TargetGUID))
				{
					objCharacter.CommandResponse("Selected target is not game object!");
					return true;
				}
				WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].TurnTo(objCharacter.positionX, objCharacter.positionY);
				DataTable q = new DataTable();
				ulong GUID = checked(objCharacter.TargetGUID - WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT);
				objCharacter.CommandResponse("Object rotation will be visible when the object is reloaded!");
			}
			return true;
		}

		[ChatCommand("npcadd", "npcadd #id - Spawn creature at your position.", AccessLevel.Developer)]
		public bool cmdAddCreature(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			WS_Creatures.CreatureObject tmpCr = new WS_Creatures.CreatureObject(Conversions.ToInteger(Message), objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ, objCharacter.orientation, checked((int)objCharacter.MapID));
			tmpCr.AddToWorld();
			objCharacter.CommandResponse("Creature [" + tmpCr.Name + "] spawned.");
			return true;
		}

		[ChatCommand("npccome", "npccome - Selected creature will come to your position.", AccessLevel.Developer)]
		public bool cmdComeCreature(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("Select target first!");
				return true;
			}
			if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
			{
				objCharacter.CommandResponse("Selected target is not creature!");
				return true;
			}
			WS_Creatures.CreatureObject creature = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
			if (creature.aiScript != null && creature.aiScript.InCombat())
			{
				objCharacter.CommandResponse("Creature is in combat. It has to be out of combat first.");
				return true;
			}
			creature.SetToRealPosition(Forced: true);
			int MoveTime = creature.MoveTo(objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ, objCharacter.orientation);
			if (creature.aiScript != null)
			{
				creature.aiScript.Pause(MoveTime);
			}
			return true;
		}

		[ChatCommand("kill", "kill - Selected creature or character will die.", AccessLevel.GameMaster)]
		public bool cmdKillCreature(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (decimal.Compare(new decimal(objCharacter.TargetGUID), 0m) == 0)
			{
				objCharacter.CommandResponse("Select target first!");
				return true;
			}
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter.TargetGUID))
			{
				WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[objCharacter.TargetGUID];
				WS_Base.BaseUnit Attacker = objCharacter;
				characterObject.Die(ref Attacker);
				objCharacter = (WS_PlayerData.CharacterObject)Attacker;
				return true;
			}
			if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(objCharacter.TargetGUID))
			{
				WS_Creatures.CreatureObject creatureObject = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID];
				int maximum = WorldServiceLocator._WorldServer.WORLD_CREATUREs[objCharacter.TargetGUID].Life.Maximum;
				WS_Base.BaseUnit Attacker = null;
				creatureObject.DealDamage(maximum, Attacker);
				return true;
			}
			return false;
		}

		[ChatCommand("gobjecttarget", "gobjecttarget - Nearest game object will be selected.", AccessLevel.Developer)]
		public bool cmdTargetGameObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			WS_GameObjects wS_GameObjects = WorldServiceLocator._WS_GameObjects;
			WS_Base.BaseUnit unit = objCharacter;
			WS_GameObjects.GameObjectObject closestGameobject = wS_GameObjects.GetClosestGameobject(ref unit);
			objCharacter = (WS_PlayerData.CharacterObject)unit;
			WS_GameObjects.GameObjectObject targetGO = closestGameobject;
			if (targetGO == null)
			{
				objCharacter.CommandResponse("Could not find any near objects.");
			}
			else
			{
				float distance = WorldServiceLocator._WS_Combat.GetDistance(targetGO, objCharacter);
				objCharacter.CommandResponse($"Selected [{targetGO.ID}][{targetGO.Name}] game object at distance {distance}.");
			}
			return true;
		}

		[ChatCommand("activatego", "activatego - Activates your targetted game object.", AccessLevel.Developer)]
		public bool cmdActivateGameObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(objCharacter.TargetGUID))
			{
				return false;
			}
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
			objCharacter.CommandResponse($"Activated game object [{WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].Name}] to state [{WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.TargetGUID].State}].");
			return true;
		}

		[ChatCommand("gobjectadd", "gobjectadd #id - Spawn game object at your position.", AccessLevel.Developer)]
		public bool cmdAddGameObject(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			WS_GameObjects.GameObjectObject tmpGO = new WS_GameObjects.GameObjectObject(Conversions.ToInteger(Message), objCharacter.MapID, objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ, objCharacter.orientation, 0uL);
			tmpGO.Rotations[2] = (float)Math.Sin(tmpGO.orientation / 2f);
			tmpGO.Rotations[3] = (float)Math.Cos(tmpGO.orientation / 2f);
			tmpGO.AddToWorld();
			objCharacter.CommandResponse($"GameObject [{tmpGO.Name}][{tmpGO.GUID:X}] spawned.");
			return true;
		}

		[ChatCommand("createaccount", "createaccount #account #password #email - Add a New account using Name, Password, And Email.", AccessLevel.Admin)]
		public bool cmdCreateAccount(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Message, "", TextCompare: false) == 0)
			{
				return false;
			}
			DataTable result = new DataTable();
			string[] acct = Strings.Split(Strings.Trim(Message));
			if (acct.Length != 3)
			{
				return false;
			}
			string aName = acct[0];
			string aPassword = acct[1];
			string aEmail = acct[2];
			WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT username FROM account WHERE username = \"" + aName + "\";", ref result);
			if (result.Rows.Count > 0)
			{
				objCharacter.CommandResponse($"Account [{aName}] already exists.");
			}
			else
			{
				byte[] passwordStr = Encoding.ASCII.GetBytes(aName.ToUpper() + ":" + aPassword.ToUpper());
				byte[] passwordHash = new SHA1Managed().ComputeHash(passwordStr);
				string hashStr = BitConverter.ToString(passwordHash).Replace("-", "");
				WorldServiceLocator._WorldServer.AccountDatabase.Insert(string.Format("INSERT INTO account (username, sha_pass_hash, email, joindate, last_ip) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", aName, hashStr, aEmail, Strings.Format(DateAndTime.Now, "yyyy-MM-dd"), "0.0.0.0"));
				objCharacter.CommandResponse($"Account [{aName}] has been created.");
			}
			return true;
		}

		[ChatCommand("changepassword", "changepassword #account #password - Changes the password of an account.", AccessLevel.Admin)]
		public bool cmdChangePassword(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Message, "", TextCompare: false) == 0)
			{
				return false;
			}
			DataTable result = new DataTable();
			string[] acct = Strings.Split(Strings.Trim(Message));
			if (acct.Length != 2)
			{
				return false;
			}
			string aName = acct[0];
			string aPassword = acct[1];
			WorldServiceLocator._WorldServer.AccountDatabase.Query("SELECT id, gmlevel FROM account WHERE username = \"" + aName + "\";", ref result);
			if (result.Rows.Count == 0)
			{
				objCharacter.CommandResponse($"Account [{aName}] does not exist.");
			}
			else
			{
				AccessLevel targetLevel = (AccessLevel)Conversions.ToByte(result.Rows[0]["gmlevel"]);
				if (targetLevel >= objCharacter.Access)
				{
					objCharacter.CommandResponse("You cannot change password for accounts with the same or a higher access level than yourself.");
				}
				else
				{
					byte[] passwordStr = Encoding.ASCII.GetBytes(aName.ToUpper() + ":" + aPassword.ToUpper());
					byte[] passwordHash = new SHA1Managed().ComputeHash(passwordStr);
					string hashStr = BitConverter.ToString(passwordHash).Replace("-", "");
					WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("UPDATE account SET password='{0}' WHERE id={1}", hashStr, RuntimeHelpers.GetObjectValue(result.Rows[0]["id"])));
					objCharacter.CommandResponse($"Account [{aName}] now has a new password [{aPassword}].");
				}
			}
			return true;
		}

		[ChatCommand("setaccess", "setaccess #account #level - Sets the account to a specific access level.", AccessLevel.Admin)]
		public bool cmdSetAccess(ref WS_PlayerData.CharacterObject objCharacter, string Message)
		{
			if (Operators.CompareString(Message, "", TextCompare: false) == 0)
			{
				return false;
			}
			DataTable result = new DataTable();
			string[] acct = Strings.Split(Strings.Trim(Message));
			if (acct.Length != 2)
			{
				return false;
			}
			string aName = acct[0];
			if (!byte.TryParse(acct[1], out var aLevel))
			{
				return false;
			}
			if (aLevel < 0u || aLevel > 3u)
			{
				objCharacter.CommandResponse($"Not a valid access level. Must be in the range {0}-{3}.");
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
				objCharacter.CommandResponse($"Account [{aName}] does not exist.");
			}
			else
			{
				AccessLevel targetLevel = (AccessLevel)Conversions.ToByte(result.Rows[0]["gmlevel"]);
				if (targetLevel >= objCharacter.Access)
				{
					objCharacter.CommandResponse("You cannot set access levels to accounts with the same or a higher access level than yourself.");
				}
				else
				{
					WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("UPDATE account SET gmlevel={0} WHERE id={1}", (byte)newLevel, RuntimeHelpers.GetObjectValue(result.Rows[0]["id"])));
					objCharacter.CommandResponse($"Account [{aName}] now has access level [{newLevel}].");
				}
			}
			return true;
		}

		public ulong GetGUID(string Name)
		{
			DataTable MySQLQuery = new DataTable();
			WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT char_guid FROM characters WHERE char_name = \"{Name}\";", ref MySQLQuery);
			if (MySQLQuery.Rows.Count > 0)
			{
				return MySQLQuery.Rows[0].As<ulong>("char_guid");
			}
			return 0uL;
		}

		public void SystemMessage(string Message)
		{
			Packets.PacketClass packet = WorldServiceLocator._Functions.BuildChatMessage(0uL, "System Message: " + Message, ChatMsg.CHAT_MSG_SYSTEM, LANGUAGES.LANG_GLOBAL, 0, "");
			packet.UpdateLength();
			WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.Broadcast(packet.Data);
			packet.Dispose();
		}

		public bool SetUpdateValue(ulong GUID, int Index, int Value, WS_Network.ClientClass client)
		{
			bool noErrors = true;
			try
			{
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_UPDATE_OBJECT);
				packet.AddInt32(1);
				packet.AddInt8(0);
				Packets.UpdateClass UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
				UpdateData.SetUpdateFlag(Index, Value);
				if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
				{
                    Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                    ulong key;
                    WS_Creatures.CreatureObject updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
					UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
					wORLD_CREATUREs[key] = updateObject;
				}
				else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(GUID))
				{
					if (GUID == client.Character.GUID)
					{
                        Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
                        ulong key;
                        WS_PlayerData.CharacterObject updateObject2 = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
						UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
						cHARACTERs[key] = updateObject2;
					}
					else
					{
                        Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
                        ulong key;
                        WS_PlayerData.CharacterObject updateObject2 = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
						UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
						cHARACTERs[key] = updateObject2;
					}
				}
				client.Send(ref packet);
				packet.Dispose();
				UpdateData.Dispose();
			}
			catch (DataException ex2)
			{
				ProjectData.SetProjectError(ex2);
                noErrors = false;
				ProjectData.ClearProjectError();
			}
			return noErrors;
		}
	}
}
