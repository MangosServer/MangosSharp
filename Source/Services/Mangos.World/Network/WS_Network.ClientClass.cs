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
using Mangos.Common;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.World.AntiCheat;
using Mangos.World.Globals;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Server
{
    public partial class WS_Network
	{
        public class ClientClass : ClientInfo, IDisposable
		{
			public WS_PlayerData.CharacterObject Character;

			public Queue<Packets.PacketClass> Packets;

			public bool DEBUG_CONNECTION;

			private bool _disposedValue;

			public void OnPacket(object state)
			{
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
				lock (this)
				{
					try
					{
						if (packet.OpCode == Opcodes.SMSG_UPDATE_OBJECT)
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
				lock (this)
				{
					try
					{
						if (packet.OpCode == Opcodes.SMSG_UPDATE_OBJECT)
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
	}
}
