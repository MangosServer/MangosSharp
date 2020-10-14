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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Mangos.Common;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Globals;
using Mangos.SignalR;
using Mangos.World.AntiCheat;
using Mangos.World.Globals;
using Mangos.World.Maps;
using Mangos.World.Player;
using Mangos.World.Social;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Server
{
    public class WS_Network
	{
		public class WorldServerClass : Hub, IWorld, IDisposable
		{
			[Serializable]
			[CompilerGenerated]
			internal sealed class _Closure_0024__
			{
				public static readonly _Closure_0024__ _0024I;

				public static Func<string, uint> _0024I13_002D0;

				public static Func<string, uint> _0024I13_002D1;

				public static Func<string, uint> _0024I14_002D0;

				static _Closure_0024__()
				{
					_0024I = new _Closure_0024__();
				}

				internal uint _Lambda_0024__13_002D0(string x)
				{
					return Conversions.ToUInteger(x);
				}

				internal uint _Lambda_0024__13_002D1(string x)
				{
					return Conversions.ToUInteger(x);
				}

				internal uint _Lambda_0024__14_002D0(string x)
				{
					return Conversions.ToUInteger(x);
				}
			}

			public bool _flagStopListen;

			public string LocalURI;

			private readonly string m_RemoteURI;

			private readonly Timer m_Connection;

			private readonly Timer m_TimerCPU;

			private DateTime LastInfo;

			private double LastCPUTime;

			private float UsageCPU;

			public ICluster Cluster;

			private bool _disposedValue;

			public WorldServerClass()
			{
				_flagStopListen = false;
				LastCPUTime = 0.0;
				UsageCPU = 0f;
				Cluster = null;
				WorldServerConfiguration configuration = WorldServiceLocator._ConfigurationProvider.GetConfiguration();
				m_RemoteURI = $"http://{configuration.ClusterConnectHost}:{configuration.ClusterConnectPort}";
				LocalURI = $"http://{configuration.LocalConnectHost}:{configuration.LocalConnectPort}";
				Cluster = null;
				WorldServiceLocator._WS_Network.LastPing = WorldServiceLocator._NativeMethods.timeGetTime("");
				m_Connection = new Timer(new TimerCallback(CheckConnection), null, 10000, 10000);
				m_TimerCPU = new Timer(new TimerCallback(CheckCPU), null, 1000, 1000);
			}

			protected new virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					ClusterDisconnect();
					_flagStopListen = true;
					m_TimerCPU.Dispose();
					m_Connection.Dispose();
				}
				_disposedValue = true;
			}

			public new void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				Dispose();
			}

			public void ClusterConnect()
			{
				while (Cluster == null)
				{
					try
					{
						Cluster = ProxyClient.Create<ICluster>(m_RemoteURI);
						if (!Information.IsNothing(Cluster))
						{
							WorldServerConfiguration configuration = WorldServiceLocator._ConfigurationProvider.GetConfiguration();
							if (Cluster.Connect(LocalURI, configuration.Maps.Select((_Closure_0024__._0024I13_002D0 != null) ? _Closure_0024__._0024I13_002D0 : (_Closure_0024__._0024I13_002D0 = (string x) => Conversions.ToUInteger(x))).ToList()))
							{
								break;
							}
							Cluster.Disconnect(LocalURI, configuration.Maps.Select((_Closure_0024__._0024I13_002D1 != null) ? _Closure_0024__._0024I13_002D1 : (_Closure_0024__._0024I13_002D1 = (string x) => Conversions.ToUInteger(x))).ToList());
						}
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception e = ex;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unable to connect to cluster. [{0}]", e.Message);
						ProjectData.ClearProjectError();
					}
					Cluster = null;
					Thread.Sleep(3000);
				}
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "Contacted cluster [{0}]", m_RemoteURI);
			}

			public void ClusterDisconnect()
			{
				try
				{
					Cluster.Disconnect(LocalURI, WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps.Select((_Closure_0024__._0024I14_002D0 != null) ? _Closure_0024__._0024I14_002D0 : (_Closure_0024__._0024I14_002D0 = (string x) => Conversions.ToUInteger(x))).ToList());
				}
				catch (Exception projectError)
				{
					ProjectData.SetProjectError(projectError);
					ProjectData.ClearProjectError();
				}
				finally
				{
					Cluster = null;
				}
			}

			public void ClientTransfer(uint ID, float posX, float posY, float posZ, float ori, int map)
			{
				checked
				{
					if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey((uint)map))
					{
						WorldServiceLocator._WorldServer.CLIENTs[ID].Character.Dispose();
						WorldServiceLocator._WorldServer.CLIENTs[ID].Delete();
					}
					Cluster.ClientTransfer(ID, posX, posY, posZ, ori, (uint)map);
				}
			}

			public void ClientConnect(uint id, ClientInfo client)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client connected", id);
				if (client == null)
				{
					throw new ApplicationException("Client doesn't exist!");
				}
				ClientClass objCharacter = new ClientClass(client);
				if (WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(id))
				{
					WorldServiceLocator._WorldServer.CLIENTs.Remove(id);
				}
				WorldServiceLocator._WorldServer.CLIENTs.Add(id, objCharacter);
			}

			void IWorld.ClientConnect(uint id, ClientInfo client)
			{
                if (client is null)
                {
                    throw new ArgumentNullException(nameof(client));
                }
                //ILSpy generated this explicit interface implementation from .override directive in ClientConnect
                ClientConnect(id, client);
			}

			public void ClientDisconnect(uint id)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client disconnected", id);
				if (WorldServiceLocator._WorldServer.CLIENTs[id].Character != null)
				{
					WorldServiceLocator._WorldServer.CLIENTs[id].Character.Save();
				}
				WorldServiceLocator._WorldServer.CLIENTs[id].Delete();
				WorldServiceLocator._WorldServer.CLIENTs.Remove(id);
			}

			void IWorld.ClientDisconnect(uint id)
			{
				//ILSpy generated this explicit interface implementation from .override directive in ClientDisconnect
				ClientDisconnect(id);
			}

			public void ClientLogin(uint id, ulong guid)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client login [0x{1:X}]", id, guid);
				try
				{
					ClientClass client = WorldServiceLocator._WorldServer.CLIENTs[id];
					WS_PlayerData.CharacterObject Character = new WS_PlayerData.CharacterObject(ref client, guid);
					WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					WorldServiceLocator._WorldServer.CHARACTERs[guid] = Character;
					WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseWriterLock();
					WorldServiceLocator._Functions.SendCorpseReclaimDelay(ref client, ref Character);
					WorldServiceLocator._WS_PlayerHelper.InitializeTalentSpells(Character);
					Character.Login();
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[{0}:{1}] Player login complete [0x{2:X}]", client.IP, client.Port, guid);
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error on login: {0}", e.ToString());
					ProjectData.ClearProjectError();
				}
			}

			void IWorld.ClientLogin(uint id, ulong guid)
			{
				//ILSpy generated this explicit interface implementation from .override directive in ClientLogin
				ClientLogin(id, guid);
			}

			public void ClientLogout(uint id)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client logout", id);
				WorldServiceLocator._WorldServer.CLIENTs[id].Character.Logout();
			}

			void IWorld.ClientLogout(uint id)
			{
				//ILSpy generated this explicit interface implementation from .override directive in ClientLogout
				ClientLogout(id);
			}

			public void ClientPacket(uint id, byte[] data)
			{
				if (data == null)
				{
					throw new ApplicationException("Packet doesn't contain data!");
				}
				Packets.PacketClass p = new Packets.PacketClass(ref data);
				try
				{
					if (!WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(id))
					{
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Client ID doesn't contain a key!: {0}", ToString());
					}
					WorldServiceLocator._WorldServer.CLIENTs[id].Packets.Enqueue(p);
					ThreadPool.QueueUserWorkItem(new WaitCallback(WorldServiceLocator._WorldServer.CLIENTs[id].OnPacket));
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error on Client OnPacket: {0}", ex.ToString());
					ProjectData.ClearProjectError();
				}
				finally
				{
					p.Dispose();
				}
			}

			void IWorld.ClientPacket(uint id, byte[] data)
			{
				//ILSpy generated this explicit interface implementation from .override directive in ClientPacket
				ClientPacket(id, data);
			}

			public int ClientCreateCharacter(string account, string name, byte race, byte classe, byte gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair, byte outfitId)
			{
                if (string.IsNullOrEmpty(account))
                {
                    throw new ArgumentException($"'{nameof(account)}' cannot be null or empty", nameof(account));
                }

                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Account {0} Created a character with Name {1}, Race {2}, Class {3}, Gender {4}, Skin {5}, Face {6}, HairStyle {7}, HairColor {8}, FacialHair {9}, outfitID {10}", account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
				return WorldServiceLocator._WS_Player_Creation.CreateCharacter(account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
			}

			int IWorld.ClientCreateCharacter(string account, string name, byte race, byte classe, byte gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair, byte outfitId)
			{
                if (string.IsNullOrEmpty(account))
                {
                    throw new ArgumentException($"'{nameof(account)}' cannot be null or empty", nameof(account));
                }

                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
                }
                //ILSpy generated this explicit interface implementation from .override directive in ClientCreateCharacter
                return ClientCreateCharacter(account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
			}

			public int Ping(int timestamp, int latency)
			{
				checked
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Cluster ping: [{0}ms]", WorldServiceLocator._NativeMethods.timeGetTime("") - timestamp);
					WorldServiceLocator._WS_Network.LastPing = WorldServiceLocator._NativeMethods.timeGetTime("");
					WorldServiceLocator._WS_Network.WC_MsTime = timestamp + latency;
					return WorldServiceLocator._NativeMethods.timeGetTime("");
				}
			}

			int IWorld.Ping(int timestamp, int latency)
			{
				//ILSpy generated this explicit interface implementation from .override directive in Ping
				return Ping(timestamp, latency);
			}

			public void CheckConnection(object State)
			{
                if (checked(WorldServiceLocator._NativeMethods.timeGetTime("") - WorldServiceLocator._WS_Network.LastPing) > 40000)
				{
					if (Cluster != null)
					{
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Cluster timed out. Reconnecting");
						ClusterDisconnect();
					}
					ClusterConnect();
					WorldServiceLocator._WS_Network.LastPing = WorldServiceLocator._NativeMethods.timeGetTime("");
				}
			}

			public void CheckCPU(object State)
			{
                TimeSpan TimeSinceLastCheck = DateAndTime.Now.Subtract(LastInfo);
				UsageCPU = (float)((Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds - LastCPUTime) / TimeSinceLastCheck.TotalMilliseconds * 100.0);
				LastInfo = DateAndTime.Now;
				LastCPUTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
			}

			public ServerInfo GetServerInfo()
			{
                ServerInfo serverInfo = new ServerInfo
                {
                    cpuUsage = UsageCPU,
                    memoryUsage = checked((ulong)Math.Round(Process.GetCurrentProcess().WorkingSet64 / 1048576.0))
                };
                return serverInfo;
			}

			ServerInfo IWorld.GetServerInfo()
			{
				//ILSpy generated this explicit interface implementation from .override directive in GetServerInfo
				return GetServerInfo();
			}

			public void InstanceCreate(uint MapID)
			{
				if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey(MapID))
				{
					WS_Maps.TMap Map = new WS_Maps.TMap(checked((int)MapID));
				}
			}

			void IWorld.InstanceCreate(uint MapID)
			{
				//ILSpy generated this explicit interface implementation from .override directive in InstanceCreate
				InstanceCreate(MapID);
			}

			public void InstanceDestroy(uint MapID)
			{
				WorldServiceLocator._WS_Maps.Maps[MapID].Dispose();
			}

			void IWorld.InstanceDestroy(uint MapID)
			{
				//ILSpy generated this explicit interface implementation from .override directive in InstanceDestroy
				InstanceDestroy(MapID);
			}

			public bool InstanceCanCreate(int Type)
			{
				WorldServerConfiguration configuration = WorldServiceLocator._ConfigurationProvider.GetConfiguration();
				return Type switch
				{
					3 => configuration.CreateBattlegrounds, 
					1 => configuration.CreatePartyInstances, 
					2 => configuration.CreateRaidInstances, 
					0 => configuration.CreateOther, 
					_ => false, 
				};
			}

			bool IWorld.InstanceCanCreate(int Type)
			{
				//ILSpy generated this explicit interface implementation from .override directive in InstanceCanCreate
				return InstanceCanCreate(Type);
			}

			public void ClientSetGroup(uint ID, long GroupID)
			{
				if (!WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(ID))
				{
					return;
				}
				if (GroupID == -1)
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client group set [G NULL]", ID);
					WorldServiceLocator._WorldServer.CLIENTs[ID].Character.Group = null;
					WorldServiceLocator._WS_Handlers_Instance.InstanceMapLeave(WorldServiceLocator._WorldServer.CLIENTs[ID].Character);
					return;
				}
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client group set [G{1:00000}]", ID, GroupID);
				if (!WorldServiceLocator._WS_Group.Groups.ContainsKey(GroupID))
				{
					WS_Group.Group Group = new WS_Group.Group(GroupID);
					Cluster.GroupRequestUpdate(ID);
				}
				WorldServiceLocator._WorldServer.CLIENTs[ID].Character.Group = WorldServiceLocator._WS_Group.Groups[GroupID];
				WorldServiceLocator._WS_Handlers_Instance.InstanceMapEnter(WorldServiceLocator._WorldServer.CLIENTs[ID].Character);
			}

			void IWorld.ClientSetGroup(uint ID, long GroupID)
			{
				//ILSpy generated this explicit interface implementation from .override directive in ClientSetGroup
				ClientSetGroup(ID, GroupID);
			}

			public void GroupUpdate(long GroupID, byte GroupType, ulong GroupLeader, ulong[] Members)
			{
				if (!WorldServiceLocator._WS_Group.Groups.ContainsKey(GroupID))
				{
					return;
				}
				List<ulong> list = new List<ulong>();
				foreach (ulong GUID in Members)
				{
					if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
					{
						list.Add(GUID);
					}
				}
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update [{2}, {1} local members]", GroupID, list.Count, (GroupType)GroupType);
				if (list.Count == 0)
				{
					WorldServiceLocator._WS_Group.Groups[GroupID].Dispose();
					return;
				}
				WorldServiceLocator._WS_Group.Groups[GroupID].Type = (GroupType)GroupType;
				WorldServiceLocator._WS_Group.Groups[GroupID].Leader = GroupLeader;
				WorldServiceLocator._WS_Group.Groups[GroupID].LocalMembers = list;
			}

			void IWorld.GroupUpdate(long GroupID, byte GroupType, ulong GroupLeader, ulong[] Members)
			{
				//ILSpy generated this explicit interface implementation from .override directive in GroupUpdate
				GroupUpdate(GroupID, GroupType, GroupLeader, Members);
			}

			public void GroupUpdateLoot(long GroupID, byte Difficulty, byte Method, byte Threshold, ulong Master)
			{
				if (WorldServiceLocator._WS_Group.Groups.ContainsKey(GroupID))
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update loot", GroupID);
					WorldServiceLocator._WS_Group.Groups[GroupID].DungeonDifficulty = (GroupDungeonDifficulty)Difficulty;
					WorldServiceLocator._WS_Group.Groups[GroupID].LootMethod = (GroupLootMethod)Method;
					WorldServiceLocator._WS_Group.Groups[GroupID].LootThreshold = (GroupLootThreshold)Threshold;
					if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Master))
					{
						WorldServiceLocator._WS_Group.Groups[GroupID].LocalLootMaster = WorldServiceLocator._WorldServer.CHARACTERs[Master];
					}
					else
					{
						WorldServiceLocator._WS_Group.Groups[GroupID].LocalLootMaster = null;
					}
				}
			}

			void IWorld.GroupUpdateLoot(long GroupID, byte Difficulty, byte Method, byte Threshold, ulong Master)
			{
				//ILSpy generated this explicit interface implementation from .override directive in GroupUpdateLoot
				GroupUpdateLoot(GroupID, Difficulty, Method, Threshold, Master);
			}

			public byte[] GroupMemberStats(ulong GUID, int Flag)
			{
				if (Flag == 0)
				{
					Flag = 1015;
				}
				WS_Group wS_Group = WorldServiceLocator._WS_Group;
				Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
				ulong key;
				WS_PlayerData.CharacterObject objCharacter = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
				Packets.PacketClass packetClass = wS_Group.BuildPartyMemberStats(ref objCharacter, checked((uint)Flag));
				cHARACTERs[key] = objCharacter;
				Packets.PacketClass p = packetClass;
				p.UpdateLength();
				return p.Data;
			}

			byte[] IWorld.GroupMemberStats(ulong GUID, int Flag)
			{
				//ILSpy generated this explicit interface implementation from .override directive in GroupMemberStats
				return GroupMemberStats(GUID, Flag);
			}

			public void GuildUpdate(ulong GUID, uint GuildID, byte GuildRank)
			{
				WorldServiceLocator._WorldServer.CHARACTERs[GUID].GuildID = GuildID;
				WorldServiceLocator._WorldServer.CHARACTERs[GUID].GuildRank = GuildRank;
				WorldServiceLocator._WorldServer.CHARACTERs[GUID].SetUpdateFlag(191, GuildID);
				WorldServiceLocator._WorldServer.CHARACTERs[GUID].SetUpdateFlag(192, GuildRank);
				WorldServiceLocator._WorldServer.CHARACTERs[GUID].SendCharacterUpdate();
			}

			void IWorld.GuildUpdate(ulong GUID, uint GuildID, byte GuildRank)
			{
				//ILSpy generated this explicit interface implementation from .override directive in GuildUpdate
				GuildUpdate(GUID, GuildID, GuildRank);
			}

			public void BattlefieldCreate(int BattlefieldID, byte BattlefieldMapType, uint Map)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Battlefield created", BattlefieldID);
			}

			void IWorld.BattlefieldCreate(int BattlefieldID, byte BattlefieldMapType, uint Map)
			{
				//ILSpy generated this explicit interface implementation from .override directive in BattlefieldCreate
				BattlefieldCreate(BattlefieldID, BattlefieldMapType, Map);
			}

			public void BattlefieldDelete(int BattlefieldID)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Battlefield deleted", BattlefieldID);
			}

			void IWorld.BattlefieldDelete(int BattlefieldID)
			{
				//ILSpy generated this explicit interface implementation from .override directive in BattlefieldDelete
				BattlefieldDelete(BattlefieldID);
			}

			public void BattlefieldJoin(int BattlefieldID, ulong GUID)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Character [0x{1:X}] joined battlefield", BattlefieldID, GUID);
			}

			void IWorld.BattlefieldJoin(int BattlefieldID, ulong GUID)
			{
				//ILSpy generated this explicit interface implementation from .override directive in BattlefieldJoin
				BattlefieldJoin(BattlefieldID, GUID);
			}

			public void BattlefieldLeave(int BattlefieldID, ulong GUID)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Character [0x{1:X}] left battlefield", BattlefieldID, GUID);
			}

			void IWorld.BattlefieldLeave(int BattlefieldID, ulong GUID)
			{
				//ILSpy generated this explicit interface implementation from .override directive in BattlefieldLeave
				BattlefieldLeave(BattlefieldID, GUID);
			}
		}

		public class ClientClass : ClientInfo, IDisposable
		{
			public WS_PlayerData.CharacterObject Character;

			public Queue<Packets.PacketClass> Packets;

			public bool DEBUG_CONNECTION;

			private bool _disposedValue;

			public void OnPacket(object state)
			{
                if (state is null)
                {
                    throw new ArgumentNullException(nameof(state));
                }

                while (Packets.Count >= 1)
				{
					try
					{
						Packets.PacketClass p = Packets.Dequeue();
						int start = WorldServiceLocator._NativeMethods.timeGetTime("");
						try
						{
							if (!Information.IsNothing(p))
							{
								if (WorldServiceLocator._WorldServer.PacketHandlers.ContainsKey(p.OpCode))
								{
									checked
									{
										try
										{
											WorldServer.HandlePacket handlePacket = WorldServiceLocator._WorldServer.PacketHandlers[p.OpCode];
											ClientClass client = this;
											handlePacket(ref p, ref client);
											if (WorldServiceLocator._NativeMethods.timeGetTime("") - start > 100)
											{
												WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Packet processing took too long: {0}, {1}ms", p.OpCode, WorldServiceLocator._NativeMethods.timeGetTime("") - start);
											}
										}
										catch (Exception ex3)
										{
											ProjectData.SetProjectError(ex3);
											Exception e = ex3;
											WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Opcode handler {2}:{3} caused an error:{1}{0}", e.ToString(), Environment.NewLine, p.OpCode, p.OpCode);
											if (!Information.IsNothing(p))
											{
												Packets packets = WorldServiceLocator._Packets;
												byte[] data = p.Data;
												ClientClass client = this;
												packets.DumpPacket(data, client);
											}
											ProjectData.ClearProjectError();
										}
									}
								}
								else
								{
									WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Unknown Opcode 0x{2:X} [DataLen={3} {4}]", IP, Port, (int)p.OpCode, p.Data.Length, p.OpCode);
									if (!Information.IsNothing(p))
									{
										Packets packets2 = WorldServiceLocator._Packets;
										byte[] data2 = p.Data;
										ClientClass client = this;
										packets2.DumpPacket(data2, client);
									}
								}
							}
							else
							{
								WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] No Packet Information in Queue", IP, Port);
								if (!Information.IsNothing(p))
								{
									Packets packets3 = WorldServiceLocator._Packets;
									byte[] data3 = p.Data;
									ClientClass client = this;
									packets3.DumpPacket(data3, client);
								}
							}
						}
						catch (Exception ex4)
						{
							ProjectData.SetProjectError(ex4);
							Exception err2 = ex4;
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Connection from [{0}:{1}] cause error {2}{3}", IP, Port, err2.ToString(), Environment.NewLine);
							Delete();
							ProjectData.ClearProjectError();
						}
						finally
						{
							try
							{
							}
							catch (Exception ex5)
							{
								ProjectData.SetProjectError(ex5);
								Exception ex2 = ex5;
								if (Packets.Count == 0)
								{
									p.Dispose();
								}
								WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Unable to dispose of packet: {0}", p.OpCode);
								if (!Information.IsNothing(p))
								{
									Packets packets4 = WorldServiceLocator._Packets;
									byte[] data4 = p.Data;
									ClientClass client = this;
									packets4.DumpPacket(data4, client);
								}
								ProjectData.ClearProjectError();
							}
						}
					}
					catch (Exception ex6)
					{
						ProjectData.SetProjectError(ex6);
						Exception err = ex6;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Connection from [{0}:{1}] cause error {2}{3}", IP, Port, err.ToString(), Environment.NewLine);
						Delete();
						ProjectData.ClearProjectError();
					}
					finally
					{
						try
						{
						}
						catch (Exception ex7)
						{
							ProjectData.SetProjectError(ex7);
                            Exception ex;
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Unable to dispose of packet");
							ProjectData.ClearProjectError();
						}
					}
				}
			}

			public void Send(ref byte[] data)
			{
                if (data is null)
                {
                    throw new ArgumentNullException(nameof(data));
                }

                lock (this)
				{
					try
					{
						WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSend(Index, data);
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception Err = ex;
						if (DEBUG_CONNECTION)
						{
							ProjectData.ClearProjectError();
							return;
						}
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] cause error {3}{2}", IP, Port, Err.ToString(), Environment.NewLine);
						WorldServiceLocator._WorldServer.ClsWorldServer.Cluster = null;
						Delete();
						ProjectData.ClearProjectError();
					}
				}
			}

			public void Send(ref Packets.PacketClass packet)
			{
				if (packet == null)
				{
					throw new ApplicationException("Packet doesn't contain data!");
				}
				lock (this)
				{
					try
					{
						if (packet.OpCode == OPCODES.SMSG_UPDATE_OBJECT)
						{
							packet.CompressUpdatePacket();
						}
						packet.UpdateLength();
						if (!Information.IsNothing(WorldServiceLocator._WorldServer.ClsWorldServer.Cluster))
						{
							WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSend(Index, packet.Data);
						}
						packet.Dispose();
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception Err = ex;
						if (DEBUG_CONNECTION)
						{
							ProjectData.ClearProjectError();
							return;
						}
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] cause error {3}{2}", IP, Port, Err.ToString(), Environment.NewLine);
						WorldServiceLocator._WorldServer.ClsWorldServer.Cluster = null;
						Delete();
						ProjectData.ClearProjectError();
					}
				}
			}

			public void SendMultiplyPackets(ref Packets.PacketClass packet)
			{
				if (packet == null)
				{
					throw new ApplicationException("Packet doesn't contain data!");
				}
				lock (this)
				{
					try
					{
						if (packet.OpCode == OPCODES.SMSG_UPDATE_OBJECT)
						{
							packet.CompressUpdatePacket();
						}
						packet.UpdateLength();
						byte[] data = (byte[])packet.Data.Clone();
						if (!Information.IsNothing(WorldServiceLocator._WorldServer.ClsWorldServer.Cluster))
						{
							WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSend(Index, data);
						}
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception Err = ex;
						if (DEBUG_CONNECTION)
						{
							ProjectData.ClearProjectError();
							return;
						}
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] cause error {3}{2}", IP, Port, Err.ToString(), Environment.NewLine);
						WorldServiceLocator._WorldServer.ClsWorldServer.Cluster = null;
						Delete();
						ProjectData.ClearProjectError();
					}
				}
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "Connection from [{0}:{1}] disposed", IP, Port);
					if (!Information.IsNothing(WorldServiceLocator._WorldServer.ClsWorldServer.Cluster))
					{
						WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientDrop(Index);
					}
					WorldServiceLocator._WorldServer.CLIENTs.Remove(Index);
					if (Character != null)
					{
						Character.client = null;
						Character.Dispose();
					}
				}
				_disposedValue = true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				Dispose();
			}

			public void Delete()
			{
				//Discarded unreachable code: IL_005b, IL_0089, IL_008b, IL_0092, IL_0095, IL_0096, IL_00a3, IL_00c5
				int num = default;
				int num3 = default;
				try
				{
					ProjectData.ClearProjectError();
					num = -2;
					int num2 = 2;
					WorldServiceLocator._WorldServer.CLIENTs.Remove(Index);
					num2 = 3;
					if (Character != null)
					{
						num2 = 4;
						Character.client = null;
						num2 = 5;
						Character.Dispose();
					}
					num2 = 7;
					Dispose();
				}
				catch (Exception obj) when (num != 0 && num3 == 0)
				{
					ProjectData.SetProjectError(obj);
					/*Error near IL_00c3: Could not find block for branch target IL_008b*/;
				}
				if (num3 != 0)
				{
					ProjectData.ClearProjectError();
				}
			}

			public void Disconnect()
			{
				Delete();
			}

			public ClientClass()
			{
				Packets = new Queue<Packets.PacketClass>();
				DEBUG_CONNECTION = false;
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Creating debug connection!", null);
				DEBUG_CONNECTION = true;
			}

			public ClientClass(ClientInfo ci)
			{
				Packets = new Queue<Packets.PacketClass>();
				DEBUG_CONNECTION = false;
                Access = ci.Access;
                Account = ci.Account;
                Index = ci.Index;
                IP = ci.IP;
                Port = ci.Port;
			}
		}

		private int LastPing;

		public int WC_MsTime;

		public WS_Network()
		{
			LastPing = 0;
			WC_MsTime = 0;
		}

		public int MsTime()
		{
			return checked(WC_MsTime + (WorldServiceLocator._NativeMethods.timeGetTime("") - LastPing));
		}
	}
}
