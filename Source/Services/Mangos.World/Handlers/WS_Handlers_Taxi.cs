using System;
using System.Collections.Generic;
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Handlers
{
	public class WS_Handlers_Taxi
	{
		private void SendActivateTaxiReply(ref WS_Network.ClientClass client, ActivateTaxiReplies reply)
		{
			Packets.PacketClass taxiFailed = new Packets.PacketClass(OPCODES.SMSG_ACTIVATETAXIREPLY);
			try
			{
				taxiFailed.AddInt32((int)reply);
				client.Send(ref taxiFailed);
			}
			finally
			{
				taxiFailed.Dispose();
			}
		}

		private void SendTaxiStatus(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
		{
			if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGuid))
			{
				return;
			}
			int currentTaxi = WorldServiceLocator._WS_DBCDatabase.GetNearestTaxi(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionX, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionY, checked((int)WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].MapID));
			Packets.PacketClass SMSG_TAXINODE_STATUS = new Packets.PacketClass(OPCODES.SMSG_TAXINODE_STATUS);
			try
			{
				SMSG_TAXINODE_STATUS.AddUInt64(cGuid);
				if (!objCharacter.TaxiZones[currentTaxi])
				{
					SMSG_TAXINODE_STATUS.AddInt8(0);
				}
				else
				{
					SMSG_TAXINODE_STATUS.AddInt8(1);
				}
				objCharacter.client.Send(ref SMSG_TAXINODE_STATUS);
			}
			finally
			{
				SMSG_TAXINODE_STATUS.Dispose();
			}
		}

		public void SendTaxiMenu(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
		{
			if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGuid))
			{
				return;
			}
			int currentTaxi = WorldServiceLocator._WS_DBCDatabase.GetNearestTaxi(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionX, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionY, checked((int)WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].MapID));
			if (!objCharacter.TaxiZones[currentTaxi])
			{
				objCharacter.TaxiZones.Set(currentTaxi, value: true);
				Packets.PacketClass SMSG_NEW_TAXI_PATH = new Packets.PacketClass(OPCODES.SMSG_NEW_TAXI_PATH);
				try
				{
					objCharacter.client.Send(ref SMSG_NEW_TAXI_PATH);
				}
				finally
				{
					SMSG_NEW_TAXI_PATH.Dispose();
				}
				Packets.PacketClass SMSG_TAXINODE_STATUS = new Packets.PacketClass(OPCODES.SMSG_TAXINODE_STATUS);
				try
				{
					SMSG_TAXINODE_STATUS.AddUInt64(cGuid);
					SMSG_TAXINODE_STATUS.AddInt8(1);
					objCharacter.client.Send(ref SMSG_TAXINODE_STATUS);
				}
				finally
				{
					SMSG_TAXINODE_STATUS.Dispose();
				}
			}
			else
			{
				Packets.PacketClass SMSG_SHOWTAXINODES = new Packets.PacketClass(OPCODES.SMSG_SHOWTAXINODES);
				try
				{
					SMSG_SHOWTAXINODES.AddInt32(1);
					SMSG_SHOWTAXINODES.AddUInt64(cGuid);
					SMSG_SHOWTAXINODES.AddInt32(currentTaxi);
					SMSG_SHOWTAXINODES.AddBitArray(objCharacter.TaxiZones, 32);
					objCharacter.client.Send(ref SMSG_SHOWTAXINODES);
				}
				finally
				{
					SMSG_SHOWTAXINODES.Dispose();
				}
			}
		}

		public void On_CMSG_TAXINODE_STATUS_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) >= 13)
			{
				packet.GetInt16();
				ulong guid = packet.GetUInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TAXINODE_STATUS_QUERY [taxiGUID={2:X}]", client.IP, client.Port, guid);
				if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid))
				{
					SendTaxiStatus(ref client.Character, guid);
				}
			}
		}

		public void On_CMSG_TAXIQUERYAVAILABLENODES(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) >= 13)
			{
				packet.GetInt16();
				ulong guid = packet.GetUInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TAXIQUERYAVAILABLENODES [taxiGUID={2:X}]", client.IP, client.Port, guid);
				if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) && ((uint)WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].CreatureInfo.cNpcFlags & 8u) != 0)
				{
					SendTaxiMenu(ref client.Character, guid);
				}
			}
		}

		public void On_CMSG_ACTIVATETAXI(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) < 21)
			{
				return;
			}
			packet.GetInt16();
			ulong guid = packet.GetUInt64();
			int srcNode = packet.GetInt32();
			int dstNode = packet.GetInt32();
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ACTIVATETAXI [taxiGUID={2:X} srcNode={3} dstNode={4}]", client.IP, client.Port, guid, srcNode, dstNode);
			if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].CreatureInfo.cNpcFlags & 8) == 0)
			{
				SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOVENDORNEARBY);
				return;
			}
			if (client.Character.LogoutTimer != null)
			{
				SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERBUSY);
				return;
			}
			if (((uint)client.Character.cUnitFlags & 4u) != 0)
			{
				SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTSTANDING);
				return;
			}
			if ((int)client.Character.ShapeshiftForm > 0 && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_BERSERKERSTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_BATTLESTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_DEFENSIVESTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_SHADOW)
			{
				SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERSHAPESHIFTED);
				return;
			}
			if (client.Character.Mount != 0)
			{
				SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERALREADYMOUNTED);
				return;
			}
			if (!WorldServiceLocator._WS_DBCDatabase.TaxiNodes.ContainsKey(srcNode) || !WorldServiceLocator._WS_DBCDatabase.TaxiNodes.ContainsKey(dstNode))
			{
				SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOSUCHPATH);
				return;
			}
			int mount;
			if (client.Character.IsHorde)
			{
				if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount))
				{
					CreatureInfo tmpCr2 = new CreatureInfo(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount);
					mount = tmpCr2.GetFirstModel;
				}
				else
				{
					mount = WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount].ModelA1;
				}
			}
			else if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount))
			{
				CreatureInfo tmpCr = new CreatureInfo(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount);
				mount = tmpCr.GetFirstModel;
			}
			else
			{
				mount = WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount].ModelA2;
			}
			if (mount == 0)
			{
				SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIUNSPECIFIEDSERVERERROR);
				return;
			}
			float discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Faction);
			checked
			{
				int totalCost = default(int);
				foreach (KeyValuePair<int, WS_DBCDatabase.TTaxiPath> taxiPath in WorldServiceLocator._WS_DBCDatabase.TaxiPaths)
				{
					if (taxiPath.Value.TFrom == srcNode && taxiPath.Value.TTo == dstNode)
					{
						totalCost = (int)Math.Round((float)totalCost + (float)taxiPath.Value.Price * discountMod);
						break;
					}
				}
				if (client.Character.Copper < totalCost)
				{
					SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTENOUGHMONEY);
					return;
				}
				ref uint copper = ref client.Character.Copper;
				copper = (uint)(unchecked((long)copper) - unchecked((long)totalCost));
				client.Character.TaxiNodes.Clear();
				client.Character.TaxiNodes.Enqueue(srcNode);
				client.Character.TaxiNodes.Enqueue(dstNode);
				SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIOK);
				TaxiTake(client.Character, mount);
				TaxiMove(client.Character, discountMod);
			}
		}

		public void On_CMSG_ACTIVATETAXI_FAR(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			checked
			{
				if (packet.Data.Length - 1 < 21)
				{
					return;
				}
				packet.GetInt16();
				try
				{
					ulong guid = packet.GetUInt64();
					int totalCost = packet.GetInt32();
					int nodeCount = packet.GetInt32();
					if (nodeCount <= 0 || packet.Data.Length - 1 < 21 + 4 * nodeCount)
					{
						return;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ACTIVATETAXI_FAR [taxiGUID={2:X} TotalCost={3} NodeCount={4}]", client.IP, client.Port, guid, totalCost, nodeCount);
					if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].CreatureInfo.cNpcFlags & 8) == 0)
					{
						SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOVENDORNEARBY);
						return;
					}
					if (client.Character.LogoutTimer != null)
					{
						SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERBUSY);
						return;
					}
					List<int> nodes;
					unchecked
					{
						if (((uint)client.Character.cUnitFlags & 4u) != 0)
						{
							SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTSTANDING);
							return;
						}
						if ((int)client.Character.ShapeshiftForm > 0 && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_BERSERKERSTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_BATTLESTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_DEFENSIVESTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_SHADOW)
						{
							SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERSHAPESHIFTED);
							return;
						}
						if (client.Character.Mount != 0)
						{
							SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERALREADYMOUNTED);
							return;
						}
						if (nodeCount < 1)
						{
							SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOSUCHPATH);
							return;
						}
						if (client.Character.Copper < totalCost)
						{
							SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTENOUGHMONEY);
							return;
						}
						nodes = new List<int>();
					}
					int num = nodeCount - 1;
					for (int i = 0; i <= num; i++)
					{
						nodes.Add(packet.GetInt32());
					}
					int srcNode = nodes[0];
					int dstNode = nodes[1];
					foreach (int node2 in client.Character.TaxiNodes)
					{
						if (!WorldServiceLocator._WS_DBCDatabase.TaxiNodes.ContainsKey(node2))
						{
							SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOSUCHPATH);
							return;
						}
					}
					int mount;
					if (client.Character.IsHorde)
					{
						if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount))
						{
							CreatureInfo tmpCr2 = new CreatureInfo(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount);
							mount = tmpCr2.GetFirstModel;
						}
						else
						{
							mount = WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount].GetFirstModel;
						}
					}
					else if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount))
					{
						CreatureInfo tmpCr = new CreatureInfo(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount);
						mount = tmpCr.GetFirstModel;
					}
					else
					{
						mount = WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount].GetFirstModel;
					}
					if (mount == 0)
					{
						SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIUNSPECIFIEDSERVERERROR);
						return;
					}
					float discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Faction);
					totalCost = 0;
					foreach (KeyValuePair<int, WS_DBCDatabase.TTaxiPath> taxiPath in WorldServiceLocator._WS_DBCDatabase.TaxiPaths)
					{
						if (taxiPath.Value.TFrom == srcNode && taxiPath.Value.TTo == dstNode)
						{
							totalCost = (int)Math.Round((float)totalCost + (float)taxiPath.Value.Price * discountMod);
							break;
						}
					}
					if (client.Character.Copper < totalCost)
					{
						SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTENOUGHMONEY);
						return;
					}
					ref uint copper = ref client.Character.Copper;
					copper = (uint)(unchecked((long)copper) - unchecked((long)totalCost));
					client.Character.TaxiNodes.Clear();
					foreach (int node in nodes)
					{
						client.Character.TaxiNodes.Enqueue(node);
					}
					SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIOK);
					TaxiTake(client.Character, mount);
					TaxiMove(client.Character, discountMod);
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when taking a long taxi.{0}", Environment.NewLine + e.ToString());
					ProjectData.ClearProjectError();
				}
			}
		}

		public void On_CMSG_MOVE_SPLINE_DONE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MOVE_SPLINE_DONE", client.IP, client.Port);
		}

		private void TaxiLand(WS_PlayerData.CharacterObject character)
		{
			character.TaxiNodes.Clear();
			character.Mount = 0;
			character.cUnitFlags &= -5;
			character.cUnitFlags &= -1048577;
			character.SetUpdateFlag(133, character.Mount);
			character.SetUpdateFlag(46, character.cUnitFlags);
			character.SendCharacterUpdate();
		}

		private void TaxiTake(WS_PlayerData.CharacterObject character, int mount)
		{
			character.Mount = mount;
			character.cUnitFlags |= 4;
			character.cUnitFlags |= 1048576;
			character.SetUpdateFlag(133, character.Mount);
			character.SetUpdateFlag(46, character.cUnitFlags);
			character.SetUpdateFlag(1176, character.Copper);
			character.SendCharacterUpdate();
		}

		private void TaxiMove(WS_PlayerData.CharacterObject character, float discountMod)
		{
			bool flagFirstNode = true;
			List<int> waypointPaths = new List<int>();
			Dictionary<int, WS_DBCDatabase.TTaxiPathNode> waypointNodes = new Dictionary<int, WS_DBCDatabase.TTaxiPathNode>();
			checked
			{
				try
				{
					int dstNode = character.TaxiNodes.Dequeue();
					while (character.TaxiNodes.Count > 0)
					{
						int srcNode = dstNode;
						dstNode = character.TaxiNodes.Dequeue();
						foreach (KeyValuePair<int, WS_DBCDatabase.TTaxiPath> taxiPath in WorldServiceLocator._WS_DBCDatabase.TaxiPaths)
						{
							if (taxiPath.Value.TFrom == srcNode && taxiPath.Value.TTo == dstNode)
							{
								waypointPaths.Add(taxiPath.Key);
								break;
							}
						}
					}
					foreach (int path in waypointPaths)
					{
						if (flagFirstNode)
						{
							flagFirstNode = false;
						}
						else
						{
							int price = (int)Math.Round((float)WorldServiceLocator._WS_DBCDatabase.TaxiPaths[path].Price * discountMod);
							if (character.Copper < price)
							{
								break;
							}
							ref uint copper = ref character.Copper;
							copper = (uint)(unchecked((long)copper) - unchecked((long)price));
							character.SetUpdateFlag(1176, character.Copper);
							character.SendCharacterUpdate(toNear: false);
							Console.WriteLine("Paying {0}", price);
						}
						float lastX = character.positionX;
						float lastY = character.positionY;
						float lastZ = character.positionZ;
						float totalDistance = 0f;
						waypointNodes.Clear();
						foreach (KeyValuePair<int, WS_DBCDatabase.TTaxiPathNode> taxiPathNode in WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[path])
						{
							waypointNodes.Add(taxiPathNode.Value.Seq, taxiPathNode.Value);
							totalDistance += WorldServiceLocator._WS_Combat.GetDistance(lastX, taxiPathNode.Value.x, lastY, taxiPathNode.Value.y, lastZ, taxiPathNode.Value.z);
							lastX = taxiPathNode.Value.x;
							lastY = taxiPathNode.Value.y;
							lastZ = taxiPathNode.Value.z;
						}
						lastX = character.positionX;
						lastY = character.positionY;
						lastZ = character.positionZ;
						Packets.PacketClass SMSG_MONSTER_MOVE = new Packets.PacketClass(OPCODES.SMSG_MONSTER_MOVE);
						try
						{
							SMSG_MONSTER_MOVE.AddPackGUID(character.GUID);
							SMSG_MONSTER_MOVE.AddSingle(character.positionX);
							SMSG_MONSTER_MOVE.AddSingle(character.positionY);
							SMSG_MONSTER_MOVE.AddSingle(character.positionZ);
							SMSG_MONSTER_MOVE.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
							SMSG_MONSTER_MOVE.AddInt8(0);
							SMSG_MONSTER_MOVE.AddInt32(768);
							SMSG_MONSTER_MOVE.AddInt32((int)(totalDistance / WorldServiceLocator._Global_Constants.UNIT_NORMAL_TAXI_SPEED * 1000f));
							SMSG_MONSTER_MOVE.AddInt32(waypointNodes.Count);
							int num = waypointNodes.Count - 1;
							for (int k = 0; k <= num; k++)
							{
								SMSG_MONSTER_MOVE.AddSingle(waypointNodes[k].x);
								SMSG_MONSTER_MOVE.AddSingle(waypointNodes[k].y);
								SMSG_MONSTER_MOVE.AddSingle(waypointNodes[k].z);
							}
							character.client.Send(ref SMSG_MONSTER_MOVE);
						}
						finally
						{
							SMSG_MONSTER_MOVE.Dispose();
						}
						int num2 = waypointNodes.Count - 1;
						for (int i = 0; i <= num2; i++)
						{
							float moveDistance = WorldServiceLocator._WS_Combat.GetDistance(lastX, waypointNodes[i].x, lastY, waypointNodes[i].y, lastZ, waypointNodes[i].z);
							lastX = waypointNodes[i].x;
							lastY = waypointNodes[i].y;
							lastZ = waypointNodes[i].z;
							Packets.PacketClass p = new Packets.PacketClass(OPCODES.SMSG_MONSTER_MOVE);
							try
							{
								p.AddPackGUID(character.GUID);
								p.AddSingle(character.positionX);
								p.AddSingle(character.positionY);
								p.AddSingle(character.positionZ);
								p.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
								p.AddInt8(0);
								p.AddInt32(768);
								p.AddInt32((int)(totalDistance / WorldServiceLocator._Global_Constants.UNIT_NORMAL_TAXI_SPEED * 1000f));
								p.AddInt32(waypointNodes.Count);
								int num3 = i;
								int num4 = waypointNodes.Count - 1;
								for (int j = num3; j <= num4; j++)
								{
									p.AddSingle(waypointNodes[j].x);
									p.AddSingle(waypointNodes[j].y);
									p.AddSingle(waypointNodes[j].z);
								}
								character.SendToNearPlayers(ref p, 0uL, ToSelf: false);
							}
							finally
							{
								p.Dispose();
							}
							Thread.Sleep((int)(moveDistance / WorldServiceLocator._Global_Constants.UNIT_NORMAL_TAXI_SPEED * 1000f));
							totalDistance -= moveDistance;
							character.positionX = lastX;
							character.positionY = lastY;
							character.positionZ = lastZ;
							WorldServiceLocator._WS_CharMovement.MoveCell(ref character);
							WorldServiceLocator._WS_CharMovement.UpdateCell(ref character);
						}
					}
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error on flight: {0}", ex.ToString());
					ProjectData.ClearProjectError();
				}
				character.Save();
				TaxiLand(character);
			}
		}
	}
}
