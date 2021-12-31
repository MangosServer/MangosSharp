//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.Globals;
using Mangos.World.Player;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Mangos.World.Network;

public partial class WS_Network
{
    public class ClientClass : ClientInfo, IDisposable
    {
        public WS_PlayerData.CharacterObject Character;
        public ConcurrentQueue<Packets.PacketClass> Packets = new();
        public bool DEBUG_CONNECTION;
        private Thread ProcessQueueThread;
        private readonly ManualResetEvent ProcessQueueSempahore = new(false);
        private volatile bool IsActive = true;

        public ClientClass(ClientInfo ci, bool isDebug = false)
        {
            if (isDebug)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Creating debug connection!", null);
                DEBUG_CONNECTION = true;
            }

            Access = ci.Access;
            Account = ci.Account;
            Index = ci.Index;
            IP = ci.IP;
            Port = ci.Port;

            ProcessQueueThread = new Thread(QueueProcessor)
            {
                IsBackground = true
            };
            ProcessQueueThread.Start();
        }

        public void PushPacket(Packets.PacketClass packet)
        {
            if (Character == null)
            {
                return;
            }

            Packets.Enqueue(packet);

            lock (_sempahoreLock)
            {
                ProcessQueueSempahore.Set();
            }
        }

        private readonly object _sempahoreLock = new();

        private void QueueProcessor()
        {
            try
            {
                while (IsActive)
                {
                    if (Packets.IsEmpty)
                    {
                        ProcessQueueSempahore.WaitOne();

                        if (!IsActive)
                        {
                            break;
                        }

                        lock (_sempahoreLock)
                        {
                            ProcessQueueSempahore.Reset();
                        }
                    }

                    while (Packets.TryDequeue(out var packet))
                    {
                        using (packet)
                        {
                            if (!WorldServiceLocator._WorldServer.PacketHandlers.ContainsKey(packet.OpCode))
                            {
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, $"[{IP}:{Port}] Unknown Opcode 0x{(int)packet.OpCode:X2} [DataLen={packet.Data.Length} {packet.OpCode}]");
                                DumpPacket(packet);
                            }
                            else
                            {
                                var start = WorldServiceLocator._NativeMethods.timeGetTime("");
                                checked
                                {
                                    try
                                    {
                                        var handlePacket = WorldServiceLocator._WorldServer.PacketHandlers[packet.OpCode];
                                        var client = this;
                                        handlePacket(ref packet, ref client);

                                        if (WorldServiceLocator._NativeMethods.timeGetTime("") - start > 100)
                                        {
                                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Packet processing took too long: {0}, {1}ms", packet.OpCode, WorldServiceLocator._NativeMethods.timeGetTime("") - start);
                                        }
                                    }
                                    catch (Exception ex3)
                                    {
                                        DumpPacket(packet);
                                        SetError(ex3, $"Opcode handler {packet?.OpCode}:{packet?.OpCode} caused an error: {ex3.Message}{Environment.NewLine}", LogType.FAILED);
                                        SetError(ex3, $"Connection from [{IP}:{Port}] cause error {ex3.Message}{Environment.NewLine}", LogType.FAILED);
                                        Delete();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (ThreadInterruptedException) { } //Disposing
            catch (Exception ex)
            {
                SetError(ex, $"Connection from [{IP}:{Port}] cause error {ex.Message}{Environment.NewLine}", LogType.FAILED);
                Delete();
            }
        }

        private readonly object lockObj = new();

        public void Send(ref byte[] data)
        {
            lock (lockObj)
            {
                try
                {
                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSend(Index, data);
                }
                catch (Exception ex)
                {
                    SetError(ex, $"Connection from [{IP}:{Port}] cause error {ex.Message}{Environment.NewLine}", LogType.CRITICAL);

                    if (DEBUG_CONNECTION)
                    {
                        return;
                    }

                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster = null;
                    Delete();
                }
            }
        }

        public void Send(ref Packets.PacketClass packet)
        {
            lock (this)
            {
                try
                {
                    using (packet)
                    {
                        if (packet.OpCode == Opcodes.SMSG_UPDATE_OBJECT)
                        {
                            packet.CompressUpdatePacket();
                        }
                        packet.UpdateLength();

                        if (WorldServiceLocator._WorldServer.ClsWorldServer.Cluster != null)
                        {
                            WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSend(Index, packet.Data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SetError(ex, $"Connection from [{IP}:{Port}] cause error {ex.Message}{Environment.NewLine}", LogType.CRITICAL);

                    if (DEBUG_CONNECTION)
                    {
                        return;
                    }

                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster = null;
                    Delete();
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
                    var data = (byte[])packet.Data.Clone();

                    if (WorldServiceLocator._WorldServer.ClsWorldServer.Cluster != null)
                    {
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSend(Index, data);
                    }
                }
                catch (Exception ex)
                {
                    SetError(ex, $"Connection from [{IP}:{Port}] cause error {ex.Message}{Environment.NewLine}", LogType.CRITICAL);

                    if (DEBUG_CONNECTION)
                    {
                        return;
                    }

                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster = null;
                    Delete();
                }
            }
        }

        public void Disconnect()
        {
            Delete();
        }

        public void Delete()
        {
            try
            {
                Dispose();
            }
            catch (Exception ex)
            {
                SetError(ex, "", LogType.FAILED);
            }
        }

        private void SetError(Exception ex, string message, LogType logType)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(logType, message, ex);
        }

        private void DumpPacket(Packets.PacketClass packet)
        {
            if (packet == null)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Unable to dump packet");
                return;
            }

            try
            {
                var packets4 = WorldServiceLocator._Packets;
                var data4 = packet.Data;
                var client = this;
                packets4.DumpPacket(data4, client);
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Unable to dump packet", ex);
            }
        }

        public void Dispose()
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, $"Connection from [{IP}:{Port}] disposed.");

            IsActive = false;
            ProcessQueueSempahore.Set(); //Allow thread to exit.
            ProcessQueueSempahore?.Dispose();

            try
            {
                ProcessQueueThread?.Interrupt();
                ProcessQueueThread?.Join(1000);
            }
            catch (ThreadInterruptedException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "{0} Thread ID: {1}", ex, Thread.CurrentThread.ManagedThreadId);
            }
            ProcessQueueThread = null;

            Packets?.Clear();

            try
            {
                if (WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(Index))
                {
                    WorldServiceLocator._WorldServer.CLIENTs.Remove(Index);
                }

                if (WorldServiceLocator._WorldServer.ClsWorldServer.Cluster != null)
                {
                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientDrop(Index);
                }

                if (WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(Index))
                {
                    WorldServiceLocator._WorldServer.CLIENTs.Remove(Index);
                }

                if (Character != null)
                {
                    Character.client = null;
                    Character.Dispose();
                    Character = null;
                }
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, $"Connection from [{IP}:{Port}] was not properly disposed.", ex);
            }
        }
    }
}
