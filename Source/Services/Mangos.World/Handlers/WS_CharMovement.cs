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
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.AI;
using Mangos.World.AntiCheat;
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
    public class WS_CharMovement
	{
		private const float PId2 = (float)Math.PI / 2f;

		private const float PIx2 = (float)Math.PI * 2f;

		public void OnMovementPacket(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			packet.GetInt16();
			if (client.Character.MindControl != null)
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
			WS_Anticheat.MovementEvent(ref client, client.Character.RunSpeed, posX, client.Character.positionX, posY, client.Character.positionY, posZ, client.Character.positionZ, checked((int)Time), WorldServiceLocator._WS_Network.MsTime());
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
				float angle = client.Character.orientation - (float)Math.PI / 2f;
				if (angle < 0f)
				{
					angle += (float)Math.PI * 2f;
				}
				client.Character.Pet.SetToRealPosition();
				float tmpX = (float)(client.Character.positionX + Math.Cos(angle) * 2.0);
				float tmpY = (float)(client.Character.positionY + Math.Sin(angle) * 2.0);
				client.Character.Pet.MoveTo(tmpX, tmpY, client.Character.positionZ, client.Character.orientation, Running: true);
			}
			if (((uint)client.Character.charMovementFlags & 0x2000000u) != 0)
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
				if (client.Character.OnTransport == null)
				{
					if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(transportGUID) && WorldServiceLocator._WorldServer.WORLD_TRANSPORTs.ContainsKey(transportGUID))
					{
						client.Character.OnTransport = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[transportGUID];
						WS_PlayerData.CharacterObject character = client.Character;
						int NotSpellID = 0;
						character.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOUNTED, NotSpellID);
						WS_Transports.TransportObject obj = (WS_Transports.TransportObject)client.Character.OnTransport;
						ref WS_PlayerData.CharacterObject character2 = ref client.Character;
						ref WS_PlayerData.CharacterObject reference = ref character2;
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
				if (client.Character.OnTransport is WS_Transports.TransportObject)
				{
					WS_Transports.TransportObject obj2 = (WS_Transports.TransportObject)client.Character.OnTransport;
					ref WS_PlayerData.CharacterObject character3 = ref client.Character;
					ref WS_PlayerData.CharacterObject reference = ref character3;
					WS_Base.BaseUnit Unit = character3;
					obj2.RemovePassenger(ref Unit);
					reference = (WS_PlayerData.CharacterObject)Unit;
				}
				client.Character.OnTransport = null;
			}
			if (((uint)client.Character.charMovementFlags & 0x200000u) != 0)
			{
				float swimAngle = packet.GetFloat();
			}
			packet.GetInt32();
			if (((uint)client.Character.charMovementFlags & 0x2000u) != 0)
			{
				uint airTime = packet.GetUInt32();
				float sinAngle = packet.GetFloat();
				float cosAngle = packet.GetFloat();
				float xySpeed = packet.GetFloat();
			}
			if (((uint)client.Character.charMovementFlags & 0x4000000u) != 0)
			{
				float unk1 = packet.GetFloat();
			}
			checked
			{
				if (client.Character.exploreCheckQueued_ && !client.Character.DEAD)
				{
					int exploreFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(client.Character.positionX, client.Character.positionY, (int)client.Character.MapID);
					if (exploreFlag != 65535)
					{
						int areaFlag = unchecked(exploreFlag % 32);
						byte areaFlagOffset = (byte)unchecked(exploreFlag / 32);
						if (!WorldServiceLocator._Functions.HaveFlag(client.Character.ZonesExplored[areaFlagOffset], (byte)areaFlag))
						{
							WorldServiceLocator._Functions.SetFlag(ref client.Character.ZonesExplored[areaFlagOffset], (byte)areaFlag, flagValue: true);
							int GainedXP = unchecked(WorldServiceLocator._WS_Maps.AreaTable[exploreFlag].Level) * 10;
							GainedXP = unchecked(WorldServiceLocator._WS_Maps.AreaTable[exploreFlag].Level) * 10;
							Packets.PacketClass SMSG_EXPLORATION_EXPERIENCE = new Packets.PacketClass(Opcodes.SMSG_EXPLORATION_EXPERIENCE);
							SMSG_EXPLORATION_EXPERIENCE.AddInt32(WorldServiceLocator._WS_Maps.AreaTable[exploreFlag].ID);
							SMSG_EXPLORATION_EXPERIENCE.AddInt32(GainedXP);
							client.Send(ref SMSG_EXPLORATION_EXPERIENCE);
							SMSG_EXPLORATION_EXPERIENCE.Dispose();
							client.Character.SetUpdateFlag(1111 + unchecked(areaFlagOffset), client.Character.ZonesExplored[areaFlagOffset]);
							client.Character.AddXP(GainedXP, 0, 0uL);
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
						WS_Spells.CastSpellParameters castSpellParameters = client.Character.spellCasted[1];
						if (unchecked((0u - ((!castSpellParameters.Finished) ? 1u : 0u)) & ((uint)WorldServiceLocator._WS_Spells.SPELLs[castSpellParameters.SpellID].interruptFlags & (true ? 1u : 0u))) != 0)
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
				int MsTime = WorldServiceLocator._WS_Network.MsTime();
				int ClientTimeDelay = (int)(unchecked(MsTime) - unchecked(Time));
				int MoveTime = (int)(unchecked(Time) - unchecked(checked(MsTime - ClientTimeDelay)) + 500 + MsTime);
				packet.AddInt32(MoveTime, 10);
				Packets.PacketClass response = new Packets.PacketClass(packet.OpCode);
				response.AddPackGUID(client.Character.GUID);
				byte[] tempArray = new byte[packet.Data.Length - 6 + 1];
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
			int MovementFlags = packet.GetInt32();
			uint Time = packet.GetUInt32();
			float PositionX = packet.GetFloat();
			float PositionY = packet.GetFloat();
			float PositionZ = packet.GetFloat();
			float Orientation = packet.GetFloat();
			if (Controlled is WS_PlayerData.CharacterObject)
			{
				WS_PlayerData.CharacterObject characterObject = (WS_PlayerData.CharacterObject)Controlled;
				characterObject.charMovementFlags = MovementFlags;
				characterObject.positionX = PositionX;
				characterObject.positionY = PositionY;
				characterObject.positionZ = PositionZ;
				characterObject.orientation = Orientation;
            }
            else if (Controlled is WS_Creatures.CreatureObject)
			{
				WS_Creatures.CreatureObject creatureObject = (WS_Creatures.CreatureObject)Controlled;
				creatureObject.positionX = PositionX;
				creatureObject.positionY = PositionY;
				creatureObject.positionZ = PositionZ;
				creatureObject.orientation = Orientation;
            }
            int MsTime = WorldServiceLocator._WS_Network.MsTime();
			checked
			{
				int ClientTimeDelay = (int)(unchecked(MsTime) - unchecked(Time));
				int MoveTime = (int)(unchecked(Time) - unchecked(checked(MsTime - ClientTimeDelay)) + 500 + MsTime);
				packet.AddInt32(MoveTime, 10);
				Packets.PacketClass response = new Packets.PacketClass(packet.OpCode);
				response.AddPackGUID(Controlled.GUID);
				byte[] tempArray = new byte[packet.Data.Length - 6 + 1];
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
			ulong GUID = packet.GetUInt64();
			if (GUID == client.Character.GUID)
			{
				packet.GetInt32();
				int flags = packet.GetInt32();
				int time = packet.GetInt32();
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
				float newSpeed = packet.GetFloat();
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
			Packets.PacketClass p = new Packets.PacketClass(Opcodes.SMSG_AREA_TRIGGER_MESSAGE);
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
				int triggerID = packet.GetInt32();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AREATRIGGER [triggerID={2}]", client.IP, client.Port, triggerID);
				DataTable q = new DataTable();
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
					client.Character.cPlayerFlags = client.Character.cPlayerFlags | PlayerFlags.PLAYER_FLAGS_RESTING;
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
					posX = Conversions.ToSingle(q.Rows[0]["target_position_x"]);
					posY = Conversions.ToSingle(q.Rows[0]["target_position_y"]);
					posZ = Conversions.ToSingle(q.Rows[0]["target_position_z"]);
					ori = Conversions.ToSingle(q.Rows[0]["target_orientation"]);
					tMap = Conversions.ToInteger(q.Rows[0]["target_map"]);
					reqLevel = Conversions.ToByte(q.Rows[0]["required_level"]);
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
				end_IL_0001:;
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when entering areatrigger.{0}", Environment.NewLine + e.ToString());
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
				int movFlags = packet.GetInt32();
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
				int FallTime = packet.GetInt32();
				checked
				{
					if (FallTime > 1100 && !client.Character.DEAD && client.Character.positionZ > WorldServiceLocator._WS_Maps.GetWaterLevel(client.Character.positionX, client.Character.positionY, (int)client.Character.MapID) && !client.Character.HaveAuraType(AuraEffects_Names.SPELL_AURA_FEATHER_FALL))
					{
						int safe_fall = client.Character.GetAuraModifier(AuraEffects_Names.SPELL_AURA_SAFE_FALL);
						if (safe_fall > 0)
						{
							FallTime = ((FallTime > safe_fall * 10) ? (FallTime - safe_fall * 10) : 0);
						}
						if (FallTime > 1100)
						{
							float FallPerc = (float)(FallTime / 1100.0);
							int FallDamage = (int)Math.Round((FallPerc * FallPerc - 1f) / 9f * client.Character.Life.Maximum);
							if (FallDamage > 0)
							{
								if (FallDamage > client.Character.Life.Maximum)
								{
									FallDamage = client.Character.Life.Maximum;
								}
								client.Character.LogEnvironmentalDamage(DamageTypes.DMG_FIRE, FallDamage);
								WS_PlayerData.CharacterObject character = client.Character;
								int damage = FallDamage;
								WS_Base.BaseUnit Attacker = null;
								character.DealDamage(damage, Attacker);
								WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[{0}:{1}] Client fall time: {2}  Damage: {3}", client.IP, client.Port, FallTime, FallDamage);
							}
						}
					}
					if (client.Character.underWaterTimer != null)
					{
						client.Character.underWaterTimer.Dispose();
						client.Character.underWaterTimer = null;
					}
					if (client.Character.LogoutTimer != null)
					{
						Packets.UpdateClass UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
						Packets.PacketClass SMSG_UPDATE_OBJECT = new Packets.PacketClass(Opcodes.SMSG_UPDATE_OBJECT);
						try
						{
							SMSG_UPDATE_OBJECT.AddInt32(1);
							SMSG_UPDATE_OBJECT.AddInt8(0);
							client.Character.cUnitFlags = client.Character.cUnitFlags | 0x40000;
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
						Packets.PacketClass packetACK = new Packets.PacketClass(Opcodes.SMSG_STANDSTATE_CHANGE_ACK);
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
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error when falling.{0}", Environment.NewLine + e.ToString());
				ProjectData.ClearProjectError();
			}
		}

		public void On_CMSG_ZONEUPDATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			checked
			{
				if (packet.Data.Length - 1 >= 9)
				{
					packet.GetInt16();
					int newZone = packet.GetInt32();
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
			if ((client.Character.CellX != WorldServiceLocator._WS_Maps.GetMapTileX(client.Character.positionX)) | (client.Character.CellY != WorldServiceLocator._WS_Maps.GetMapTileY(client.Character.positionY)))
			{
				MoveCell(ref client.Character);
			}
			UpdateCell(ref client.Character);
			client.Character.GroupUpdateFlag = client.Character.GroupUpdateFlag | 0x100u;
			client.Character.ZoneCheck();
			WS_Maps wS_Maps = WorldServiceLocator._WS_Maps;
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
			ulong[] array = client.Character.creaturesNear.ToArray();
			foreach (ulong cGUID in array)
			{
				if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript != null && (WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript is WS_Creatures_AI.DefaultAI || WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript is WS_Creatures_AI.GuardAI) && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.InCombat() && !client.Character.inCombatWith.Contains(cGUID) && client.Character.GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) == TReaction.HOSTILE && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], client.Character) <= WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].AggroRange(client.Character))
				{
					WS_Creatures_AI.TBaseAI aiScript = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript;
					ref WS_PlayerData.CharacterObject character = ref client.Character;
					WS_Base.BaseUnit Attacker = character;
					aiScript.OnGenerateHate(ref Attacker, 1);
					character = (WS_PlayerData.CharacterObject)Attacker;
					client.Character.AddToCombat(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
					WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.State = AIState.AI_ATTACKING;
					WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.DoThink();
				}
			}
			ulong[] array2 = client.Character.inCombatWith.ToArray();
			foreach (ulong CombatUnit in array2)
			{
				if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(CombatUnit) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(CombatUnit) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[CombatUnit].aiScript != null)
				{
					WS_Creatures.CreatureObject creatureObject = WorldServiceLocator._WorldServer.WORLD_CREATUREs[CombatUnit];
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
				GC.Collect();
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
			ulong[] list = Character.SeenBy.ToArray();
			ulong[] array = list;
			foreach (ulong GUID in array)
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
			ulong[] array2 = list;
			foreach (ulong GUID2 in array2)
			{
				if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID2].SeenBy.Contains(Character.GUID))
				{
					WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID2].SeenBy.Remove(Character.GUID);
				}
			}
			Character.creaturesNear.Clear();
			list = Character.gameObjectsNear.ToArray();
			ulong[] array3 = list;
			foreach (ulong GUID3 in array3)
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
			ulong[] array4 = list;
			foreach (ulong GUID4 in array4)
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
			byte oldX = Character.CellX;
			byte oldY = Character.CellY;
			WorldServiceLocator._WS_Maps.GetMapTile(Character.positionX, Character.positionY, ref Character.CellX, ref Character.CellY);
			if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY] == null)
			{
				MAP_Load(Character.CellX, Character.CellY, Character.MapID);
			}
			if ((Character.CellX != oldX) | (Character.CellY != oldY))
			{
				WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[oldX, oldY].PlayersHere.Remove(Character.GUID);
				WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, Character.CellY].PlayersHere.Add(Character.GUID);
			}
		}

		public void UpdateCell(ref WS_PlayerData.CharacterObject Character)
		{
			ulong[] list = Character.playersNear.ToArray();
			ulong[] array = list;
			foreach (ulong GUID in array)
			{
				WS_PlayerData.CharacterObject obj = Character;
				Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
				ulong key;
				WS_Base.BaseObject objCharacter = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
				bool flag = obj.CanSee(ref objCharacter);
				cHARACTERs[key] = (WS_PlayerData.CharacterObject)objCharacter;
				if (!flag)
				{
					Character.guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					Character.guidsForRemoving.Add(GUID);
					Character.guidsForRemoving_Lock.ReleaseWriterLock();
					WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Remove(Character.GUID);
					Character.playersNear.Remove(GUID);
				}
				WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[GUID];
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
			ulong[] array2 = list;
			foreach (ulong GUID2 in array2)
			{
				int num;
				if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID2))
				{
					WS_PlayerData.CharacterObject obj2 = Character;
					Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
					ulong key;
					WS_Base.BaseObject objCharacter = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID2];
					bool flag = obj2.CanSee(ref objCharacter);
					wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)objCharacter;
					num = ((!flag) ? 1 : 0);
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
			ulong[] array3 = list;
			foreach (ulong GUID3 in array3)
			{
				if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(GUID3))
				{
					WS_PlayerData.CharacterObject obj3 = Character;
					Dictionary<ulong, WS_Transports.TransportObject> wORLD_TRANSPORTs;
					ulong key;
					WS_Base.BaseObject objCharacter = (wORLD_TRANSPORTs = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)[key = GUID3];
					bool flag = obj3.CanSee(ref objCharacter);
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
					WS_PlayerData.CharacterObject obj4 = Character;
					Dictionary<ulong, WS_GameObjects.GameObjectObject> wORLD_GAMEOBJECTs;
					ulong key;
					WS_Base.BaseObject objCharacter = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID3];
					bool flag = obj4.CanSee(ref objCharacter);
					wORLD_GAMEOBJECTs[key] = (WS_GameObjects.GameObjectObject)objCharacter;
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
			ulong[] array4 = list;
			foreach (ulong GUID4 in array4)
			{
				WS_PlayerData.CharacterObject obj5 = Character;
				Dictionary<ulong, WS_DynamicObjects.DynamicObjectObject> wORLD_DYNAMICOBJECTs;
				ulong key;
				WS_Base.BaseObject objCharacter = (wORLD_DYNAMICOBJECTs = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs)[key = GUID4];
				bool flag = obj5.CanSee(ref objCharacter);
				wORLD_DYNAMICOBJECTs[key] = (WS_DynamicObjects.DynamicObjectObject)objCharacter;
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
			ulong[] array5 = list;
			foreach (ulong GUID5 in array5)
			{
				WS_PlayerData.CharacterObject obj6 = Character;
				Dictionary<ulong, WS_Corpses.CorpseObject> wORLD_CORPSEOBJECTs;
				ulong key;
				WS_Base.BaseObject objCharacter = (wORLD_CORPSEOBJECTs = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs)[key = GUID5];
				bool flag = obj6.CanSee(ref objCharacter);
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
				if (((short)unchecked(Character.CellX + CellXAdd) > 63) | ((short)unchecked(Character.CellX + CellXAdd) < 0))
				{
					CellXAdd = 0;
				}
				if (((short)unchecked(Character.CellY + CellYAdd) > 63) | ((short)unchecked(Character.CellY + CellYAdd) < 0))
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
						MAP_Load((byte)(short)unchecked(Character.CellX + CellXAdd), Character.CellY, Character.MapID);
					}
					if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), Character.CellY].CreaturesHere.Count > 0 || WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), Character.CellY].GameObjectsHere.Count > 0)
					{
						UpdateCreaturesAndGameObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), Character.CellY], ref Character);
					}
					if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), Character.CellY].PlayersHere.Count > 0)
					{
						UpdatePlayersInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), Character.CellY], ref Character);
					}
					if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), Character.CellY].CorpseObjectsHere.Count > 0)
					{
						UpdateCorpseObjectsInCell(ref WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[(short)unchecked(Character.CellX + CellXAdd), Character.CellY], ref Character);
					}
				}
				if (CellYAdd != 0)
				{
					if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].Tiles[Character.CellX, (short)unchecked(Character.CellY + CellYAdd)] == null)
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
			WS_Maps.TMapTile tMapTile = MapTile;
			ulong[] list = tMapTile.PlayersHere.ToArray();
			ulong[] array = list;
			foreach (ulong GUID in array)
			{
				if (!WorldServiceLocator._WorldServer.CHARACTERs[GUID].SeenBy.Contains(Character.GUID))
				{
					WS_PlayerData.CharacterObject obj = Character;
					Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
					ulong key;
					WS_Base.BaseObject objCharacter = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
					bool flag = obj.CanSee(ref objCharacter);
					cHARACTERs[key] = (WS_PlayerData.CharacterObject)objCharacter;
					if (flag)
					{
						Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_UPDATE_OBJECT);
						packet.AddInt32(1);
						packet.AddInt8(0);
						Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
						WorldServiceLocator._WorldServer.CHARACTERs[GUID].FillAllUpdateFlags(ref tmpUpdate);
						Packets.UpdateClass updateClass = tmpUpdate;
						WS_PlayerData.CharacterObject updateObject = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
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
					WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[GUID];
					WS_Base.BaseObject objCharacter = Character;
					bool flag = characterObject.CanSee(ref objCharacter);
					Character = (WS_PlayerData.CharacterObject)objCharacter;
					if (flag)
					{
						Packets.PacketClass myPacket = new Packets.PacketClass(Opcodes.SMSG_UPDATE_OBJECT);
						myPacket.AddInt32(1);
						myPacket.AddInt8(0);
						Packets.UpdateClass myTmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
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
			Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
			WS_Maps.TMapTile tMapTile = MapTile;
			ulong[] list = tMapTile.CreaturesHere.ToArray();
			ulong[] array = list;
			foreach (ulong GUID in array)
			{
				if (!Character.creaturesNear.Contains(GUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
				{
					WS_PlayerData.CharacterObject obj = Character;
					Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
					ulong key;
					WS_Base.BaseObject objCharacter = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
					bool flag = obj.CanSee(ref objCharacter);
					wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)objCharacter;
					if (flag)
					{
						Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
						WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].FillAllUpdateFlags(ref tmpUpdate);
						Packets.UpdateClass updateClass = tmpUpdate;
						Packets.PacketClass packet2 = packet;
						WS_Creatures.CreatureObject updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
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
			ulong[] array2 = list;
			foreach (ulong GUID2 in array2)
			{
				if (Character.gameObjectsNear.Contains(GUID2))
				{
					continue;
				}
				if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(GUID2))
				{
					WS_PlayerData.CharacterObject obj2 = Character;
					Dictionary<ulong, WS_Transports.TransportObject> wORLD_TRANSPORTs;
					ulong key;
					WS_Base.BaseObject objCharacter = (wORLD_TRANSPORTs = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)[key = GUID2];
					bool flag = obj2.CanSee(ref objCharacter);
					wORLD_TRANSPORTs[key] = (WS_Transports.TransportObject)objCharacter;
					if (flag)
					{
						Packets.UpdateClass tmpUpdate3 = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
						WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[GUID2].FillAllUpdateFlags(ref tmpUpdate3, ref Character);
						Packets.UpdateClass updateClass2 = tmpUpdate3;
						Packets.PacketClass packet2 = packet;
						WS_GameObjects.GameObjectObject updateObject2 = (wORLD_TRANSPORTs = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)[key = GUID2];
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
					WS_PlayerData.CharacterObject obj3 = Character;
					Dictionary<ulong, WS_GameObjects.GameObjectObject> wORLD_GAMEOBJECTs;
					ulong key;
					WS_Base.BaseObject objCharacter = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID2];
					bool flag = obj3.CanSee(ref objCharacter);
					wORLD_GAMEOBJECTs[key] = (WS_GameObjects.GameObjectObject)objCharacter;
					if (flag)
					{
						Packets.UpdateClass tmpUpdate2 = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
						WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID2].FillAllUpdateFlags(ref tmpUpdate2, ref Character);
						Packets.UpdateClass updateClass3 = tmpUpdate2;
						Packets.PacketClass packet2 = packet;
						WS_GameObjects.GameObjectObject updateObject2 = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID2];
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
			ulong[] array3 = list;
			foreach (ulong GUID3 in array3)
			{
				if (!Character.dynamicObjectsNear.Contains(GUID3))
				{
					WS_PlayerData.CharacterObject obj4 = Character;
					Dictionary<ulong, WS_DynamicObjects.DynamicObjectObject> wORLD_DYNAMICOBJECTs;
					ulong key;
					WS_Base.BaseObject objCharacter = (wORLD_DYNAMICOBJECTs = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs)[key = GUID3];
					bool flag = obj4.CanSee(ref objCharacter);
					wORLD_DYNAMICOBJECTs[key] = (WS_DynamicObjects.DynamicObjectObject)objCharacter;
					if (flag)
					{
						Packets.UpdateClass tmpUpdate4 = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_DYNAMICOBJECT);
						WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID3].FillAllUpdateFlags(ref tmpUpdate4);
						Packets.UpdateClass updateClass4 = tmpUpdate4;
						Packets.PacketClass packet2 = packet;
						WS_DynamicObjects.DynamicObjectObject updateObject3 = (wORLD_DYNAMICOBJECTs = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs)[key = GUID3];
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
				WS_Network.ClientClass client = Character.client;
				Packets.PacketClass packet2 = packet;
				client.Send(ref packet2);
				packet = (Packets.UpdatePacketClass)packet2;
			}
			packet.Dispose();
		}

		public void UpdateCreaturesInCell(ref WS_Maps.TMapTile MapTile, ref WS_PlayerData.CharacterObject Character)
		{
			WS_Maps.TMapTile tMapTile = MapTile;
			ulong[] list = tMapTile.CreaturesHere.ToArray();
			ulong[] array = list;
			foreach (ulong GUID in array)
			{
				if (!Character.creaturesNear.Contains(GUID))
				{
					WS_PlayerData.CharacterObject obj = Character;
					Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
					ulong key;
					WS_Base.BaseObject objCharacter = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
					bool flag = obj.CanSee(ref objCharacter);
					wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)objCharacter;
					if (flag)
					{
						Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_UPDATE_OBJECT);
						packet.AddInt32(1);
						packet.AddInt8(0);
						Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
						WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].FillAllUpdateFlags(ref tmpUpdate);
						Packets.UpdateClass updateClass = tmpUpdate;
						WS_Creatures.CreatureObject updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
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
			WS_Maps.TMapTile tMapTile = MapTile;
			ulong[] list = tMapTile.GameObjectsHere.ToArray();
			ulong[] array = list;
			foreach (ulong GUID in array)
			{
				if (!Character.gameObjectsNear.Contains(GUID))
				{
					int num;
					if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID))
					{
						WS_PlayerData.CharacterObject obj = Character;
						Dictionary<ulong, WS_GameObjects.GameObjectObject> wORLD_GAMEOBJECTs;
						ulong key;
						WS_Base.BaseObject objCharacter = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID];
						bool flag = obj.CanSee(ref objCharacter);
						wORLD_GAMEOBJECTs[key] = (WS_GameObjects.GameObjectObject)objCharacter;
						num = (flag ? 1 : 0);
					}
					else
					{
						num = 0;
					}
					if (num != 0)
					{
						Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_UPDATE_OBJECT);
						packet.AddInt32(1);
						packet.AddInt8(0);
						Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
						WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].FillAllUpdateFlags(ref tmpUpdate, ref Character);
						Packets.UpdateClass updateClass = tmpUpdate;
						Dictionary<ulong, WS_GameObjects.GameObjectObject> wORLD_GAMEOBJECTs;
						ulong key;
						WS_GameObjects.GameObjectObject updateObject = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID];
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
			WS_Maps.TMapTile tMapTile = MapTile;
			ulong[] list = tMapTile.CorpseObjectsHere.ToArray();
			ulong[] array = list;
			foreach (ulong GUID in array)
			{
				if (!Character.corpseObjectsNear.Contains(GUID))
				{
					WS_PlayerData.CharacterObject obj = Character;
					Dictionary<ulong, WS_Corpses.CorpseObject> wORLD_CORPSEOBJECTs;
					ulong key;
					WS_Base.BaseObject objCharacter = (wORLD_CORPSEOBJECTs = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs)[key = GUID];
					bool flag = obj.CanSee(ref objCharacter);
					wORLD_CORPSEOBJECTs[key] = (WS_Corpses.CorpseObject)objCharacter;
					if (flag)
					{
						Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_UPDATE_OBJECT);
						packet.AddInt32(1);
						packet.AddInt8(0);
						Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_CORPSE);
						WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].FillAllUpdateFlags(ref tmpUpdate);
						Packets.UpdateClass updateClass = tmpUpdate;
						WS_Corpses.CorpseObject updateObject = (wORLD_CORPSEOBJECTs = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs)[key = GUID];
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
}
