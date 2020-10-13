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
using System.Threading;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Server;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Cluster.Handlers
{
	public class WC_Handlers_Group
	{

		// Used as counter for unique Group.ID
		private long GroupCounter = 1L;
		public Dictionary<long, Group> GROUPs = new Dictionary<long, Group>();

		public class Group : IDisposable
		{
			public long Id;
			public GroupType Type = GroupType.PARTY;
			public GroupDungeonDifficulty DungeonDifficulty = GroupDungeonDifficulty.DIFFICULTY_NORMAL;
			private byte LootMaster;
			public GroupLootMethod LootMethod = GroupLootMethod.LOOT_GROUP;
			public GroupLootThreshold LootThreshold = GroupLootThreshold.Uncommon;
			public byte Leader;
			public WcHandlerCharacter.CharacterObject[] Members = new WcHandlerCharacter.CharacterObject[ClusterServiceLocator._Global_Constants.GROUP_SIZE + 1];
			public ulong[] TargetIcons = new ulong[8];

			public Group(WcHandlerCharacter.CharacterObject objCharacter)
			{
				Id = Interlocked.Increment(ref ClusterServiceLocator._WC_Handlers_Group.GroupCounter);
				ClusterServiceLocator._WC_Handlers_Group.GROUPs.Add(Id, this);
				Members[0] = objCharacter;
				Members[1] = null;
				Members[2] = null;
				Members[3] = null;
				Members[4] = null;
				Leader = 0;
				LootMaster = 255;
				objCharacter.Group = this;
				objCharacter.GroupAssistant = false;
				objCharacter.GetWorld.ClientSetGroup(objCharacter.Client.Index, Id);
			}

			/* TODO ERROR: Skipped RegionDirectiveTrivia */
			private bool _disposedValue; // To detect redundant calls

			// IDisposable
			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					// TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
					// TODO: set large fields to null.
					Packets.PacketClass packet;
					if (Type == GroupType.RAID)
					{
						packet = new Packets.PacketClass(OPCODES.SMSG_GROUP_LIST);
						packet.AddInt16(0);          // GroupType 0:Party 1:Raid
						packet.AddInt32(0);          // GroupCount
					}
					else
					{
						packet = new Packets.PacketClass(OPCODES.SMSG_GROUP_DESTROYED);
					}

					for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
					{
						if (Members[i] is object)
						{
							Members[i].Group = null;
							if (Members[i].Client is object)
							{
								Members[i].Client.SendMultiplyPackets(packet);
								Members[i].GetWorld.ClientSetGroup(Members[i].Client.Index, -1);
							}

							Members[i] = null;
						}
					}

					packet.Dispose();
					ClusterServiceLocator._WC_Network.WorldServer.GroupSendUpdate(Id);
					ClusterServiceLocator._WC_Handlers_Group.GROUPs.Remove(Id);
				}

				_disposedValue = true;
			}

			// This code added by Visual Basic to correctly implement the disposable pattern.
			public void Dispose()
			{
				// Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			/* TODO ERROR: Skipped EndRegionDirectiveTrivia */
			public void Join(WcHandlerCharacter.CharacterObject objCharacter)
			{
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (Members[i] is null)
					{
						Members[i] = objCharacter;
						objCharacter.Group = this;
						objCharacter.GroupAssistant = false;
						break;
					}
				}

				ClusterServiceLocator._WC_Network.WorldServer.GroupSendUpdate(Id);
				objCharacter.GetWorld.ClientSetGroup(objCharacter.Client.Index, Id);
				SendGroupList();
			}

			public void Leave(WcHandlerCharacter.CharacterObject objCharacter)
			{
				if (GetMembersCount() == 2)
				{
					Dispose();
					return;
				}

				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (ReferenceEquals(Members[i], objCharacter))
					{
						objCharacter.Group = null;
						Members[i] = null;

						// DONE: If current is leader then choose new
						if (i == Leader)
						{
							NewLeader();
						}

						// DONE: If current is lootMaster then choose new
						if (i == LootMaster)
							LootMaster = Leader;
						if (objCharacter.Client is object)
						{
							var packet = new Packets.PacketClass(OPCODES.SMSG_GROUP_UNINVITE);
							objCharacter.Client.Send(packet);
							packet.Dispose();
						}

						break;
					}
				}

				ClusterServiceLocator._WC_Network.WorldServer.GroupSendUpdate(Id);
				objCharacter.GetWorld.ClientSetGroup(objCharacter.Client.Index, -1);
				CheckMembers();
			}

			public void CheckMembers()
			{
				if (GetMembersCount() < 2)
					Dispose();
				else
					SendGroupList();
			}

			public void NewLeader(WcHandlerCharacter.CharacterObject Leaver = null)
			{
				byte ChosenMember = 255;
				bool NewLootMaster = false;
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (Members[i] is object && Members[i].Client is object)
					{
						if (Leaver is object && ReferenceEquals(Leaver, Members[i]))
						{
							if (i == LootMaster)
								NewLootMaster = true;
							if (ChosenMember != 255)
								break;
						}
						else if (Members[i].GroupAssistant && ChosenMember == 255)
						{
							ChosenMember = i;
						}
						else if (ChosenMember == 255)
						{
							ChosenMember = i;
						}
					}
				}

				if (ChosenMember != 255)
				{
					Leader = ChosenMember;
					if (NewLootMaster)
						LootMaster = Leader;
					var response = new Packets.PacketClass(OPCODES.SMSG_GROUP_SET_LEADER);
					response.AddString(Members[Leader].Name);
					Broadcast(response);
					response.Dispose();
					ClusterServiceLocator._WC_Network.WorldServer.GroupSendUpdate(Id);
				}
			}

			public bool IsFull
			{
				get
				{
					for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
					{
						if (Members[i] is null)
							return false;
					}

					return true;
				}
			}

			public void ConvertToRaid()
			{
				Array.Resize(ref Members, ClusterServiceLocator._Global_Constants.GROUP_RAIDSIZE + 1);
				for (int i = ClusterServiceLocator._Global_Constants.GROUP_SIZE + 1, loopTo = ClusterServiceLocator._Global_Constants.GROUP_RAIDSIZE; i <= loopTo; i++)
					Members[i] = null;
				Type = GroupType.RAID;
			}

			public void SetLeader(WcHandlerCharacter.CharacterObject objCharacter)
			{
				for (byte i = 0, loopTo = (byte)Members.Length; i <= loopTo; i++)
				{
					if (ReferenceEquals(Members[i], objCharacter))
					{
						Leader = i;
						break;
					}
				}

				var packet = new Packets.PacketClass(OPCODES.SMSG_GROUP_SET_LEADER);
				packet.AddString(objCharacter.Name);
				Broadcast(packet);
				packet.Dispose();
				ClusterServiceLocator._WC_Network.WorldServer.GroupSendUpdate(Id);
				SendGroupList();
			}

			public void SetLootMaster(WcHandlerCharacter.CharacterObject objCharacter)
			{
				LootMaster = Leader;
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (ReferenceEquals(Members[i], objCharacter))
					{
						LootMaster = i;
						break;
					}
				}

				SendGroupList();
			}

			public void SetLootMaster(ulong GUID)
			{
				LootMaster = 255;
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (Members[i] is object && Members[i].Guid == GUID)
					{
						LootMaster = i;
						break;
					}
				}

				SendGroupList();
			}

			public WcHandlerCharacter.CharacterObject GetLeader()
			{
				return Members[Leader];
			}

			public WcHandlerCharacter.CharacterObject GetLootMaster()
			{
				return Members[Leader];
			}

			public byte GetMembersCount()
			{
				byte count = 0;
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (Members[i] is object)
						count = (byte)(count + 1);
				}

				return count;
			}

			public ulong[] GetMembers()
			{
				var list = new List<ulong>();
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (Members[i] is object)
						list.Add(Members[i].Guid);
				}

				return list.ToArray();
			}

			public void Broadcast(Packets.PacketClass packet)
			{
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (Members[i] is object && Members[i].Client is object)
						Members[i].Client.SendMultiplyPackets(packet);
				}
			}

			public void BroadcastToOther(Packets.PacketClass packet, WcHandlerCharacter.CharacterObject objCharacter)
			{
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (Members[i] is object && !ReferenceEquals(Members[i], objCharacter) && Members[i].Client is object)
						Members[i].Client.SendMultiplyPackets(packet);
				}
			}

			public void BroadcastToOutOfRange(Packets.PacketClass packet, WcHandlerCharacter.CharacterObject objCharacter)
			{
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (Members[i] is object && !ReferenceEquals(Members[i], objCharacter) && Members[i].Client is object)
					{
						if (objCharacter.Map != Members[i].Map || Math.Sqrt(Math.Pow(objCharacter.PositionX - Members[i].PositionX, 2d) + Math.Pow(objCharacter.PositionY - Members[i].PositionY, 2d)) > ClusterServiceLocator._Global_Constants.DEFAULT_DISTANCE_VISIBLE)
						{
							Members[i].Client.SendMultiplyPackets(packet);
						}
					}
				}
			}

			public void SendGroupList()
			{
				byte GroupCount = GetMembersCount();
				for (byte i = 0, loopTo = (byte)(Members.Length - 1); i <= loopTo; i++)
				{
					if (Members[i] is object)
					{
						var packet = new Packets.PacketClass(OPCODES.SMSG_GROUP_LIST);
						packet.AddInt8((byte)Type);                                    // GroupType 0:Party 1:Raid
						byte MemberFlags = (byte)(i / ClusterServiceLocator._Global_Constants.GROUP_SUBGROUPSIZE);
						// If Members(i).GroupAssistant Then MemberFlags = MemberFlags Or &H1
						packet.AddInt8(MemberFlags);
						packet.AddInt32(GroupCount - 1);
						for (byte j = 0, loopTo1 = (byte)(Members.Length - 1); j <= loopTo1; j++)
						{
							if (Members[j] is object && !ReferenceEquals(Members[j], Members[i]))
							{
								packet.AddString(Members[j].Name);
								packet.AddUInt64(Members[j].Guid);
								if (Members[j].IsInWorld)
								{
									packet.AddInt8(1);                           // CharOnline?
								}
								else
								{
									packet.AddInt8(0);
								}                           // CharOnline?

								MemberFlags = (byte)(j / ClusterServiceLocator._Global_Constants.GROUP_SUBGROUPSIZE);
								// If Members(j).GroupAssistant Then MemberFlags = MemberFlags Or &H1
								packet.AddInt8(MemberFlags);
							}
						}

						packet.AddUInt64(Members[Leader].Guid);
						packet.AddInt8((byte)LootMethod);
						if (LootMaster != 255)
							packet.AddUInt64(Members[LootMaster].Guid);
						else
							packet.AddUInt64(0UL);
						packet.AddInt8((byte)LootThreshold);
						packet.AddInt16(0);
						if (Members[i].Client is object)
						{
							Members[i].Client.Send(packet);
						}

						packet.Dispose();
					}
				}
			}

			public void SendChatMessage(WcHandlerCharacter.CharacterObject sender, string message, LANGUAGES language, ChatMsg thisType)
			{
				var packet = ClusterServiceLocator._Functions.BuildChatMessage(sender.Guid, message, thisType, language, (byte)sender.ChatFlag);
				Broadcast(packet);
				packet.Dispose();
			}
		}

		public void On_CMSG_REQUEST_RAID_INFO(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_REQUEST_RAID_INFO", client.IP, client.Port);
			var q = new DataTable();
			if (client.Character is object)
			{
				ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT * FROM characters_instances WHERE char_guid = {0};", client.Character.Guid), ref q);
			}

			var response = new Packets.PacketClass(OPCODES.SMSG_RAID_INSTANCE_INFO);
			response.AddInt32(q.Rows.Count);                                 // Instances Counts
			int i = 0;
			foreach (DataRow r in q.Rows)
			{
				response.AddUInt32(Conversions.ToUInteger(r["map"]));                               // MapID
				response.AddUInt32((uint)(Conversions.ToInteger(r["expire"]) - ClusterServiceLocator._Functions.GetTimestamp(DateAndTime.Now)));  // TimeLeft
				response.AddUInt32(Conversions.ToUInteger(r["instance"]));                          // InstanceID
				response.AddUInt32((uint)i);                                           // Counter
				i += 1;
			}

			client.Send(response);
			response.Dispose();
		}

		public void SendPartyResult(WC_Network.ClientClass objCharacter, string Name, PartyCommand operation, PartyCommandResult result)
		{
			var response = new Packets.PacketClass(OPCODES.SMSG_PARTY_COMMAND_RESULT);
			response.AddInt32((byte)operation);
			response.AddString(Name);
			response.AddInt32((byte)result);
			objCharacter.Send(response);
			response.Dispose();
		}

		public void On_CMSG_GROUP_INVITE(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length - 1 < 6)
				return;
			packet.GetInt16();
			string Name = ClusterServiceLocator._Functions.CapitalizeName(packet.GetString());
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_INVITE [{2}]", client.IP, client.Port, Name);
			ulong GUID = 0UL;
			ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.AcquireReaderLock(ClusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
			foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> Character in ClusterServiceLocator._WorldCluster.CHARACTERs)
			{
				if (ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(Character.Value.Name) == ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(Name))
				{
					GUID = Character.Value.Guid;
					break;
				}
			}

			ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.ReleaseReaderLock();
			PartyCommandResult errCode = PartyCommandResult.INVITE_OK;
			// TODO: InBattlegrounds: INVITE_RESTRICTED
			if (GUID == 0m)
			{
				errCode = PartyCommandResult.INVITE_NOT_FOUND;
			}
			else if (ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].IsInWorld == false)
			{
				errCode = PartyCommandResult.INVITE_NOT_FOUND;
			}
			else if (ClusterServiceLocator._Functions.GetCharacterSide((byte)ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Race) != ClusterServiceLocator._Functions.GetCharacterSide((byte)client.Character.Race))
			{
				errCode = PartyCommandResult.INVITE_NOT_SAME_SIDE;
			}
			else if (ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].IsInGroup)
			{
				errCode = PartyCommandResult.INVITE_ALREADY_IN_GROUP;
				var denied = new Packets.PacketClass(OPCODES.SMSG_GROUP_INVITE);
				denied.AddInt8(0);
				denied.AddString(client.Character.Name);
				ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Client.Send(denied);
				denied.Dispose();
			}
			else if (ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].IgnoreList.Contains(client.Character.Guid))
			{
				errCode = PartyCommandResult.INVITE_IGNORED;
			}
			else if (!client.Character.IsInGroup)
			{
				var newGroup = new Group(client.Character);
				// TODO: Need to do fully test this
				ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Group = newGroup;
				ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].GroupInvitedFlag = true;
			}
			else if (client.Character.Group.IsFull)
			{
				errCode = PartyCommandResult.INVITE_PARTY_FULL;
			}
			else if (client.Character.IsGroupLeader == false && client.Character.GroupAssistant == false)
			{
				errCode = PartyCommandResult.INVITE_NOT_LEADER;
			}
			else
			{
				ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Group = client.Character.Group;
				ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].GroupInvitedFlag = true;
			}

			SendPartyResult(client, Name, PartyCommand.PARTY_OP_INVITE, errCode);
			if (errCode == PartyCommandResult.INVITE_OK)
			{
				var invited = new Packets.PacketClass(OPCODES.SMSG_GROUP_INVITE);
				invited.AddInt8(1);
				invited.AddString(client.Character.Name);
				ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Client.Send(invited);
				invited.Dispose();
			}
		}

		public void On_CMSG_GROUP_CANCEL(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_CANCEL", client.IP, client.Port);
		}

		public void On_CMSG_GROUP_ACCEPT(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_ACCEPT", client.IP, client.Port);
			if (client.Character.GroupInvitedFlag && !client.Character.Group.IsFull)
			{
				client.Character.Group.Join(client.Character);
			}
			else
			{
				SendPartyResult(client, client.Character.Name, PartyCommand.PARTY_OP_INVITE, PartyCommandResult.INVITE_PARTY_FULL);
				client.Character.Group = null;
			}

			client.Character.GroupInvitedFlag = false;
		}

		public void On_CMSG_GROUP_DECLINE(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_DECLINE", client.IP, client.Port);
			if (client.Character.GroupInvitedFlag)
			{
				var response = new Packets.PacketClass(OPCODES.SMSG_GROUP_DECLINE);
				response.AddString(client.Character.Name);
				client.Character.Group.GetLeader().Client.Send(response);
				response.Dispose();
				client.Character.Group.CheckMembers();
				client.Character.Group = null;
				client.Character.GroupInvitedFlag = false;
			}
		}

		public void On_CMSG_GROUP_DISBAND(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_DISBAND", client.IP, client.Port);
			if (client.Character.IsInGroup)
			{
				// TODO: InBattlegrounds: INVITE_RESTRICTED
				if (client.Character.Group.GetMembersCount() > 2)
				{
					client.Character.Group.Leave(client.Character);
				}
				else
				{
					client.Character.Group.Dispose();
				}
			}
		}

		public void On_CMSG_GROUP_UNINVITE(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length - 1 < 6)
				return;
			packet.GetInt16();
			string Name = packet.GetString();
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_UNINVITE [{2}]", client.IP, client.Port, Name);
			ulong GUID = 0UL;
			ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.AcquireReaderLock(ClusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
			foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> Character in ClusterServiceLocator._WorldCluster.CHARACTERs)
			{
				if (ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(Character.Value.Name) == ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(Name))
				{
					GUID = Character.Value.Guid;
					break;
				}
			}

			ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.ReleaseReaderLock();

			// TODO: InBattlegrounds: INVITE_RESTRICTED
			if (GUID == 0m)
			{
				SendPartyResult(client, Name, PartyCommand.PARTY_OP_LEAVE, PartyCommandResult.INVITE_NOT_FOUND);
			}
			else if (!client.Character.IsGroupLeader)
			{
				SendPartyResult(client, "", PartyCommand.PARTY_OP_LEAVE, PartyCommandResult.INVITE_NOT_LEADER);
			}
			else
			{
				var tmp = ClusterServiceLocator._WorldCluster.CHARACTERs;
				var argobjCharacter = tmp[GUID];
				client.Character.Group.Leave(argobjCharacter);
				tmp[GUID] = argobjCharacter;
			}
		}

		public void On_CMSG_GROUP_UNINVITE_GUID(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length - 1 < 13)
				return;
			packet.GetInt16();
			ulong GUID = packet.GetUInt64();
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_UNINVITE_GUID [0x{2:X}]", client.IP, client.Port, GUID);

			// TODO: InBattlegrounds: INVITE_RESTRICTED
			if (GUID == 0m)
			{
				SendPartyResult(client, "", PartyCommand.PARTY_OP_LEAVE, PartyCommandResult.INVITE_NOT_FOUND);
			}
			else if (ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(GUID) == false)
			{
				SendPartyResult(client, "", PartyCommand.PARTY_OP_LEAVE, PartyCommandResult.INVITE_NOT_FOUND);
			}
			else if (!client.Character.IsGroupLeader)
			{
				SendPartyResult(client, "", PartyCommand.PARTY_OP_LEAVE, PartyCommandResult.INVITE_NOT_LEADER);
			}
			else
			{
				var tmp = ClusterServiceLocator._WorldCluster.CHARACTERs;
				var argobjCharacter = tmp[GUID];
				client.Character.Group.Leave(argobjCharacter);
				tmp[GUID] = argobjCharacter;
			}
		}

		public void On_CMSG_GROUP_SET_LEADER(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length - 1 < 6)
				return;
			packet.GetInt16();
			string Name = packet.GetString();
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_SET_LEADER [Name={2}]", client.IP, client.Port, Name);
			ulong GUID = ClusterServiceLocator._WcHandlerCharacter.GetCharacterGUIDByName(Name);
			if (GUID == 0m)
			{
				SendPartyResult(client, "", PartyCommand.PARTY_OP_INVITE, PartyCommandResult.INVITE_NOT_FOUND);
			}
			else if (ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(GUID) == false)
			{
				SendPartyResult(client, "", PartyCommand.PARTY_OP_INVITE, PartyCommandResult.INVITE_NOT_FOUND);
			}
			else if (!client.Character.IsGroupLeader)
			{
				SendPartyResult(client, client.Character.Name, PartyCommand.PARTY_OP_INVITE, PartyCommandResult.INVITE_NOT_LEADER);
			}
			else
			{
				var tmp = ClusterServiceLocator._WorldCluster.CHARACTERs;
				var argobjCharacter = tmp[GUID];
				client.Character.Group.SetLeader(argobjCharacter);
				tmp[GUID] = argobjCharacter;
			}
		}

		public void On_CMSG_GROUP_RAID_CONVERT(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_RAID_CONVERT", client.IP, client.Port);
			if (client.Character.IsInGroup)
			{
				SendPartyResult(client, "", PartyCommand.PARTY_OP_INVITE, PartyCommandResult.INVITE_OK);
				client.Character.Group.ConvertToRaid();
				client.Character.Group.SendGroupList();
				ClusterServiceLocator._WC_Network.WorldServer.GroupSendUpdate(client.Character.Group.Id);
			}
		}

		public void On_CMSG_GROUP_CHANGE_SUB_GROUP(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length - 1 < 6)
				return;
			packet.GetInt16();
			string name = packet.GetString();
			if (packet.Data.Length - 1 < 6 + name.Length + 1)
				return;
			byte subGroup = packet.GetInt8();
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_CHANGE_SUB_GROUP [{2}:{3}]", client.IP, client.Port, name, subGroup);
			if (client.Character.IsInGroup)
			{
				int j;
				var loopTo = (subGroup + 1) * ClusterServiceLocator._Global_Constants.GROUP_SUBGROUPSIZE - 1;
				for (j = subGroup * ClusterServiceLocator._Global_Constants.GROUP_SUBGROUPSIZE; j <= loopTo; j++)
				{
					if (client.Character.Group.Members[j] is null)
					{
						break;
					}
				}

				for (int i = 0, loopTo1 = client.Character.Group.Members.Length - 1; i <= loopTo1; i++)
				{
					if (client.Character.Group.Members[i] is object && (client.Character.Group.Members[i].Name ?? "") == (name ?? ""))
					{
						client.Character.Group.Members[j] = client.Character.Group.Members[i];
						client.Character.Group.Members[i] = null;
						if (client.Character.Group.Leader == i)
							client.Character.Group.Leader = (byte)j;
						client.Character.Group.SendGroupList();
						break;
					}
				}
			}
		}

		public void On_CMSG_GROUP_SWAP_SUB_GROUP(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length - 1 < 6)
				return;
			packet.GetInt16();
			string name1 = packet.GetString();
			if (packet.Data.Length - 1 < 6 + name1.Length + 1)
				return;
			string name2 = packet.GetString();
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GROUP_SWAP_SUB_GROUP [{2}:{3}]", client.IP, client.Port, name1, name2);
			if (client.Character.IsInGroup)
			{
				int j;
				var loopTo = client.Character.Group.Members.Length - 1;
				for (j = 0; j <= loopTo; j++)
				{
					if (client.Character.Group.Members[j] is object && (client.Character.Group.Members[j].Name ?? "") == (name2 ?? ""))
					{
						break;
					}
				}

				for (int i = 0, loopTo1 = client.Character.Group.Members.Length - 1; i <= loopTo1; i++)
				{
					if (client.Character.Group.Members[i] is object && (client.Character.Group.Members[i].Name ?? "") == (name1 ?? ""))
					{
						var tmpPlayer = client.Character.Group.Members[j];
						client.Character.Group.Members[j] = client.Character.Group.Members[i];
						client.Character.Group.Members[i] = tmpPlayer;
						if (client.Character.Group.Leader == i)
						{
							client.Character.Group.Leader = (byte)j;
						}
						else if (client.Character.Group.Leader == j)
						{
							client.Character.Group.Leader = (byte)i;
						}

						client.Character.Group.SendGroupList();
						break;
					}
				}
			}
		}

		public void On_CMSG_LOOT_METHOD(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length - 1 < 21)
				return;
			packet.GetInt16();
			int Method = packet.GetInt32();
			ulong Master = packet.GetUInt64();
			int Threshold = packet.GetInt32();
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_METHOD [Method={2}, Master=0x{3:X}, Threshold={4}]", client.IP, client.Port, Method, Master, Threshold);
			if (!client.Character.IsGroupLeader)
			{
				return;
			}

			client.Character.Group.SetLootMaster(Master);
			client.Character.Group.LootMethod = (GroupLootMethod)Method;
			client.Character.Group.LootThreshold = (GroupLootThreshold)Threshold;
			client.Character.Group.SendGroupList();
			ClusterServiceLocator._WC_Network.WorldServer.GroupSendUpdateLoot(client.Character.Group.Id);
		}

		public void On_MSG_MINIMAP_PING(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			packet.GetInt16();
			float x = packet.GetFloat();
			float y = packet.GetFloat();
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MINIMAP_PING [{2}:{3}]", client.IP, client.Port, x, y);
			if (client.Character.IsInGroup)
			{
				var response = new Packets.PacketClass(OPCODES.MSG_MINIMAP_PING);
				response.AddUInt64(client.Character.Guid);
				response.AddSingle(x);
				response.AddSingle(y);
				client.Character.Group.Broadcast(response);
				response.Dispose();
			}
		}

		public void On_MSG_RANDOM_ROLL(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length - 1 < 13)
				return;
			packet.GetInt16();
			int minRoll = packet.GetInt32();
			int maxRoll = packet.GetInt32();
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_RANDOM_ROLL [min={2} max={3}]", client.IP, client.Port, minRoll, maxRoll);
			var response = new Packets.PacketClass(OPCODES.MSG_RANDOM_ROLL);
			response.AddInt32(minRoll);
			response.AddInt32(maxRoll);
			response.AddInt32(ClusterServiceLocator._WorldCluster.Rnd.Next(minRoll, maxRoll));
			response.AddUInt64(client.Character.Guid);
			if (client.Character.IsInGroup)
			{
				client.Character.Group.Broadcast(response);
			}
			else
			{
				client.SendMultiplyPackets(response);
			}

			response.Dispose();
		}

		public void On_MSG_RAID_READY_CHECK(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_RAID_READY_CHECK", client.IP, client.Port);
			if (client.Character.IsGroupLeader)
			{
				client.Character.Group.BroadcastToOther(packet, client.Character);
			}
			else
			{
				if (packet.Data.Length - 1 < 6)
					return;
				packet.GetInt16();
				byte result = packet.GetInt8();
				if (result == 0)
				{
					// DONE: Not ready
					client.Character.Group.GetLeader().Client.Send(packet);
				}
				else
				{
					// DONE: Ready
					var response = new Packets.PacketClass(OPCODES.MSG_RAID_READY_CHECK);
					response.AddUInt64(client.Character.Guid);
					client.Character.Group.GetLeader().Client.Send(response);
					response.Dispose();
				}
			}
		}

		public void On_MSG_RAID_ICON_TARGET(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length < 7)
				return; // Too short packet
			if (client.Character.Group is null)
				return;
			packet.GetInt16();
			byte icon = packet.GetInt8();
			if (icon == 255)
			{
				// DONE: Send icon target list
				var response = new Packets.PacketClass(OPCODES.MSG_RAID_ICON_TARGET);
				response.AddInt8(1); // Target list
				for (byte i = 0; i <= 7; i++)
				{
					if (client.Character.Group.TargetIcons[i] == 0m)
						continue;
					response.AddInt8(i);
					response.AddUInt64(client.Character.Group.TargetIcons[i]);
				}

				client.Send(response);
				response.Dispose();
			}
			else
			{
				if (icon > 7)
					return; // Not a valid icon
				if (packet.Data.Length < 15)
					return; // Too short packet
				ulong GUID = packet.GetUInt64();

				// DONE: Set the raid icon target
				client.Character.Group.TargetIcons[icon] = GUID;
				var response = new Packets.PacketClass(OPCODES.MSG_RAID_ICON_TARGET);
				response.AddInt8(0); // Set target
				response.AddInt8(icon);
				response.AddUInt64(GUID);
				client.Character.Group.Broadcast(response);
				response.Dispose();
			}
		}

		public void On_CMSG_REQUEST_PARTY_MEMBER_STATS(Packets.PacketClass packet, WC_Network.ClientClass client)
		{
			if (packet.Data.Length - 1 < 13)
				return;
			packet.GetInt16();
			ulong GUID = packet.GetUInt64();
			ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_REQUEST_PARTY_MEMBER_STATS [{2:X}]", client.IP, client.Port, GUID);
			if (!ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(GUID))
			{
				// Character is offline
				var response = ClusterServiceLocator._Functions.BuildPartyMemberStatsOffline(GUID);
				client.Send(response);
				response.Dispose();
			}
			else if (ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].IsInWorld == false)
			{
				// Character is offline (not in world)
				var response = ClusterServiceLocator._Functions.BuildPartyMemberStatsOffline(GUID);
				client.Send(response);
				response.Dispose();
			}
			else
			{
				// Request information from WorldServer
				var response = new Packets.PacketClass(0) { Data = ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].GetWorld.GroupMemberStats(GUID, 0) };
				client.Send(response);
				response.Dispose();
			}
		}
	}
}