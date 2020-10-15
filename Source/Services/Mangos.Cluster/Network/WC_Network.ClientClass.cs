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
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Common;
using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Cluster.Server
{
    public partial class WC_Network
    {
        public class ClientClass : ClientInfo, IDisposable
        {
            public Socket Socket = null;
            public Queue Queue = new Queue();
            public WcHandlerCharacter.CharacterObject Character = null;
            public byte[] SS_Hash;
            public bool Encryption = false;
            protected byte[] SocketBuffer = new byte[8193];
            protected int SocketBytes;
            protected byte[] SavedBytes = Array.Empty<byte>();
            public bool DEBUG_CONNECTION = false;
            private readonly byte[] Key = new byte[] { 0, 0, 0, 0 };
            private bool HandingPackets = false;

            public ClientInfo GetClientInfo()
            {
                var ci = new ClientInfo()
                {
                    Access = Access,
                    Account = Account,
                    Index = Index,
                    IP = IP,
                    Port = Port
                };
                return ci;
            }

            public void OnConnect(object state)
            {
                if (Socket is null)
                    throw new ApplicationException("socket doesn't exist!");
                if (ClusterServiceLocator._WorldCluster.CLIENTs is null)
                    throw new ApplicationException("Clients doesn't exist!");
                IPEndPoint remoteEndPoint = (IPEndPoint)Socket.RemoteEndPoint;
                IP = remoteEndPoint.Address.ToString();
                Port = (uint)remoteEndPoint.Port;

                // DONE: Connection spam protection
                if (ClusterServiceLocator._WC_Network.LastConnections.ContainsKey(ClusterServiceLocator._WC_Network.Ip2Int(IP)))
                {
                    if (DateAndTime.Now > ClusterServiceLocator._WC_Network.LastConnections[ClusterServiceLocator._WC_Network.Ip2Int(IP)])
                    {
                        ClusterServiceLocator._WC_Network.LastConnections[ClusterServiceLocator._WC_Network.Ip2Int(IP)] = DateAndTime.Now.AddSeconds(5d);
                    }
                    else
                    {
                        Socket.Close();
                        Dispose();
                        return;
                    }
                }
                else
                {
                    ClusterServiceLocator._WC_Network.LastConnections.Add(ClusterServiceLocator._WC_Network.Ip2Int(IP), DateAndTime.Now.AddSeconds(5d));
                }

                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "Incoming connection from [{0}:{1}]", IP, Port);
                Socket.BeginReceive(SocketBuffer, 0, SocketBuffer.Length, SocketFlags.None, OnData, null);

                // Send Auth Challenge
                var p = new Packets.PacketClass(Opcodes.SMSG_AUTH_CHALLENGE);
                p.AddInt32((int)Index);
                Send(p);
                Index = (uint)Interlocked.Increment(ref ClusterServiceLocator._WorldCluster.CLIETNIDs);
                lock (((ICollection)ClusterServiceLocator._WorldCluster.CLIENTs).SyncRoot)
                    ClusterServiceLocator._WorldCluster.CLIENTs.Add(Index, this);
                ClusterServiceLocator._WC_Stats.ConnectionsIncrement();
            }

            public void OnData(IAsyncResult ar)
            {
                if (!Socket.Connected)
                    return;
                if (ClusterServiceLocator._WC_Network.WorldServer.m_flagStopListen)
                    return;
                if (ar is null)
                    throw new ApplicationException("Value ar is empty!");
                if (SocketBuffer is null)
                    throw new ApplicationException("SocketBuffer is empty!");
                if (Socket is null)
                    throw new ApplicationException("Socket is Null!");
                if (ClusterServiceLocator._WorldCluster.CLIENTs is null)
                    throw new ApplicationException("Clients doesn't exist!");
                if (Queue is null)
                    throw new ApplicationException("Queue is Null!");
                if (SavedBytes is null)
                    throw new ApplicationException("SavedBytes is empty!");
                try
                {
                    SocketBytes = Socket.EndReceive(ar);
                    if (SocketBytes == 0)
                    {
                        Dispose(Conversions.ToBoolean(SocketBytes));
                    }
                    else
                    {
                        Interlocked.Add(ref ClusterServiceLocator._WC_Stats.DataTransferIn, SocketBytes);
                        while (SocketBytes > 0)
                        {
                            if (SavedBytes.Length == 0)
                            {
                                if (Encryption)
                                    Decode(SocketBuffer);
                            }
                            else
                            {
                                SocketBuffer = ClusterServiceLocator._Functions.Concat(SavedBytes, SocketBuffer);
                                SavedBytes = (new byte[] { });
                            }

                            // Calculate Length from packet
                            int PacketLen = SocketBuffer[1] + SocketBuffer[0] * 256 + 2;
                            if (SocketBytes < PacketLen)
                            {
                                SavedBytes = new byte[SocketBytes];
                                try
                                {
                                    Array.Copy(SocketBuffer, 0, SavedBytes, 0, SocketBytes);
                                }
                                catch (Exception)
                                {
                                    Dispose(Conversions.ToBoolean(SocketBytes));
                                    Socket.Dispose();
                                    Socket.Close();
                                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "[{0}:{1}] BAD PACKET {2}({3}) bytes, ", IP, Port, SocketBytes, PacketLen);
                                }

                                break;
                            }

                            // Move packet to Data
                            var data = new byte[PacketLen];
                            Array.Copy(SocketBuffer, data, PacketLen);

                            // Create packet and add it to queue
                            var p = new Packets.PacketClass(data);
                            lock (Queue.SyncRoot)
                                Queue.Enqueue(p);
                            try
                            {
                                // Delete packet from buffer
                                SocketBytes -= PacketLen;
                                Array.Copy(SocketBuffer, PacketLen, SocketBuffer, 0, SocketBytes);
                            }
                            catch (Exception)
                            {
                                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "[{0}:{1}] Could not delete packet from buffer! {2}({3}{4}) bytes, ", IP, Port, SocketBuffer, PacketLen, SocketBytes);
                            }
                        }

                        if (SocketBuffer.Length > 0)
                        {
                            try
                            {
                                SocketError argerrorCode = (SocketError)SocketFlags.None;
                                Socket.BeginReceive(SocketBuffer, 0, SocketBuffer.Length, (SocketFlags)SocketBytes, out argerrorCode, OnData, null);
                                if (HandingPackets == false)
                                    ThreadPool.QueueUserWorkItem(OnPacket);
                            }
                            catch (Exception)
                            {
                                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "Packet Disconnect from [{0}:{1}] caused an error {2}{3}", IP, Port, Information.Err().ToString(), Constants.vbCrLf);
                            }
                        }
                    }
                }
                catch (Exception Err)
                {
                    // NOTE: If it's a error here it means the connection is closed?
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "Connection from [{0}:{1}] caused an error {2}{3}", IP, Port, Err.ToString(), Constants.vbCrLf);
                    Dispose(Conversions.ToBoolean(SocketBuffer.Length));
                    Dispose(HandingPackets);
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void OnPacket(object state)
            {
                if (SocketBuffer is null)
                    throw new ApplicationException("SocketBuffer is empty!");
                if (Socket is null)
                    throw new ApplicationException("Socket is Null!");
                if (ClusterServiceLocator._WorldCluster.CLIENTs is null)
                    throw new ApplicationException("Clients doesn't exist!");
                if (Queue is null)
                    throw new ApplicationException("Queue is Null!");
                if (SavedBytes is null)
                    throw new ApplicationException("SavedBytes is empty!");
                if (ClusterServiceLocator._WorldCluster.GetPacketHandlers() is null)
                    throw new ApplicationException("PacketHandler is empty!");
                try
                {
                }
                catch (Exception)
                {
                    HandingPackets = true;
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "Handing Packets Failed: {0}", HandingPackets);
                }

                while (Queue.Count > 0)
                {
                    Packets.PacketClass p;
                    lock (Queue.SyncRoot)
                        p = (Packets.PacketClass)Queue.Dequeue();
                    if (ClusterServiceLocator._WorldCluster.GetConfig().PacketLogging)
                    {
                        var argclient = this;
                        ClusterServiceLocator._Packets.LogPacket(p.Data, false, argclient);
                    }

                    if (ClusterServiceLocator._WorldCluster.GetPacketHandlers().ContainsKey(p.OpCode) != true)
                    {
                        if (Character is null || Character.IsInWorld == false)
                        {
                            Socket.Dispose();
                            Socket.Close();
                            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Unknown Opcode 0x{2:X} [{2}], DataLen={4}", IP, Port, p.OpCode, Constants.vbCrLf, p.Length);
                            var argclient1 = this;
                            ClusterServiceLocator._Packets.DumpPacket(p.Data, argclient1);
                        }
                        else
                        {
                            try
                            {
                                Character.GetWorld.ClientPacket(Index, p.Data);
                            }
                            catch
                            {
                                ClusterServiceLocator._WC_Network.WorldServer.Disconnect("NULL", new List<uint>() { Character.Map });
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            var argclient2 = this;
                            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[p.OpCode].Invoke(p, argclient2);
                        }
                        catch (Exception e)
                        {
                            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "Opcode handler {2}:{2:X} caused an error: {1}{0}", e.ToString(), Constants.vbCrLf, p.OpCode);
                        }
                    }

                    try
                    {
                    }
                    catch (Exception)
                    {
                        if (Queue.Count == 0)
                            p.Dispose();
                        ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "Unable to dispose of packet: {0}", p.OpCode);
                        var argclient = this;
                        ClusterServiceLocator._Packets.DumpPacket(p.Data, argclient);
                    }
                }

                HandingPackets = false;
            }

            public void Send(byte[] data)
            {
                if (!Socket.Connected)
                    return;
                try
                {
                    if (ClusterServiceLocator._WorldCluster.GetConfig().PacketLogging)
                    {
                        var argclient = this;
                        ClusterServiceLocator._Packets.LogPacket(data, true, argclient);
                    }

                    if (Encryption)
                        Encode(data);
                    Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSendComplete, null);
                }
                catch (Exception Err)
                {
                    // NOTE: If it's a error here it means the connection is closed?
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] caused an error {2}{3}", IP, Port, Err.ToString(), Constants.vbCrLf);
                    Delete();
                }
            }

            public void Send(Packets.PacketClass packet)
            {
                if (Information.IsNothing(packet))
                    throw new ApplicationException("Packet doesn't contain data!");
                if (Information.IsNothing(Socket) | Socket.Connected == false)
                    return;
                try
                {
                    var data = packet.Data;
                    if (ClusterServiceLocator._WorldCluster.GetConfig().PacketLogging)
                    {
                        var argclient = this;
                        ClusterServiceLocator._Packets.LogPacket(data, true, argclient);
                    }

                    if (Encryption)
                        Encode(data);
                    Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSendComplete, null);
                }
                catch (Exception err)
                {
                    // NOTE: If it's a error here it means the connection is closed?
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] caused an error {2}{3}", IP, Port, err.ToString(), Constants.vbCrLf);
                    Delete();
                }

                // Only attempt to dispose of the packet if it actually exists
                if (!Information.IsNothing(packet))
                    packet.Dispose();
            }

            public void SendMultiplyPackets(Packets.PacketClass packet)
            {
                if (packet is null)
                    throw new ApplicationException("Packet doesn't contain data!");
                if (!Socket.Connected)
                    return;
                try
                {
                    byte[] data = (byte[])packet.Data.Clone();
                    if (ClusterServiceLocator._WorldCluster.GetConfig().PacketLogging)
                    {
                        var argclient = this;
                        ClusterServiceLocator._Packets.LogPacket(data, true, argclient);
                    }

                    if (Encryption)
                        Encode(data);
                    Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSendComplete, null);
                }
                catch (Exception Err)
                {
                    // NOTE: If it's a error here it means the connection is closed?
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] caused an error {2}{3}", IP, Port, Err.ToString(), Constants.vbCrLf);
                    Delete();
                }

                // Don't forget to clean after using this function
            }

            public void OnSendComplete(IAsyncResult ar)
            {
                if (Socket is object)
                {
                    int bytesSent = Socket.EndSend(ar);
                    Interlocked.Add(ref ClusterServiceLocator._WC_Stats.DataTransferOut, bytesSent);
                }
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

                    // On Error Resume Next
                    // May have to trap and use exception handler rather than the on error resume next rubbish

                    if (Socket is object)
                        Socket.Close();
                    Socket = null;
                    lock (((ICollection)ClusterServiceLocator._WorldCluster.CLIENTs).SyncRoot)
                        ClusterServiceLocator._WorldCluster.CLIENTs.Remove(Index);
                    if (Character is object)
                    {
                        if (Character.IsInWorld)
                        {
                            Character.IsInWorld = false;
                            Character.GetWorld.ClientDisconnect(Index);
                        }

                        Character.Dispose();
                    }

                    Character = null;
                    ClusterServiceLocator._WC_Stats.ConnectionsDecrement();
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
            public void Delete()
            {
                Socket.Close();
                Dispose();
            }

            public void Decode(byte[] data)
            {
                int tmp;
                for (int i = 0; i <= 6 - 1; i++)
                {
                    tmp = data[i];
                    data[i] = (byte)(SS_Hash[Key[1]] ^ (256 + data[i] - Key[0]) % 256);
                    Key[0] = (byte)tmp;
                    Key[1] = (byte)((Key[1] + 1) % 40);
                }
            }

            public void Encode(byte[] data)
            {
                for (int i = 0; i <= 4 - 1; i++)
                {
                    data[i] = (byte)(((SS_Hash[Key[3]] ^ data[i]) + Key[2]) % 256);
                    Key[2] = data[i];
                    Key[3] = (byte)((Key[3] + 1) % 40);
                }
            }

            public void EnQueue(object state)
            {
                while (ClusterServiceLocator._WorldCluster.CHARACTERs.Count > ClusterServiceLocator._WorldCluster.GetConfig().ServerPlayerLimit)
                {
                    if (!Socket.Connected)
                        return;
                    new Packets.PacketClass(Opcodes.SMSG_AUTH_RESPONSE).AddInt8((byte)LoginResponse.LOGIN_WAIT_QUEUE);
                    new Packets.PacketClass(Opcodes.SMSG_AUTH_RESPONSE).AddInt32(ClusterServiceLocator._WorldCluster.CLIENTs.Count - ClusterServiceLocator._WorldCluster.CHARACTERs.Count);            // amount of players in queue
                    Send(new Packets.PacketClass(Opcodes.SMSG_AUTH_RESPONSE));
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{1}:{2}] AUTH_WAIT_QUEUE: Server player limit reached!", IP, Port);
                    Thread.Sleep(6000);
                }

                var argclient = this;
                ClusterServiceLocator._WC_Handlers_Auth.SendLoginOk(argclient);
            }
        }
    }
}