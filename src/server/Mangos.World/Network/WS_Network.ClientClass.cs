//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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
using Mangos.Cluster.Interop;
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
        private volatile bool _disposed;
        private readonly object _sendLock = new();

        public ClientClass(ClientInfo ci, bool isDebug = false)
        {
            if (isDebug)
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "Creating debug connection!", null);
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
                            if (!WorldServiceLocator.WorldServer.PacketHandlers.ContainsKey(packet.OpCode))
                            {
                                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, $"[{IP}:{Port}] Unknown Opcode 0x{(int)packet.OpCode:X2} [DataLen={packet.Data.Length} {packet.OpCode}]");
                                DumpPacket(packet);
                            }
                            else
                            {
                                var start = WorldServiceLocator.NativeMethods.timeGetTime("");
                                checked
                                {
                                    try
                                    {
                                        var handlePacket = WorldServiceLocator.WorldServer.PacketHandlers[packet.OpCode];
                                        var client = this;
                                        handlePacket(ref packet, ref client);

                                        if (WorldServiceLocator.NativeMethods.timeGetTime("") - start > 100)
                                        {
                                            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "Packet processing took too long: {0}, {1}ms", packet.OpCode, WorldServiceLocator.NativeMethods.timeGetTime("") - start);
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
                    WorldServiceLocator.WorldServer.ClsWorldServer.Cluster.ClientSend(Index, data);
                }
                catch (Exception ex)
                {
                    SetError(ex, $"Connection from [{IP}:{Port}] cause error {ex.Message}{Environment.NewLine}", LogType.CRITICAL);

                    if (DEBUG_CONNECTION)
                    {
                        return;
                    }

                    WorldServiceLocator.WorldServer.ClsWorldServer.Cluster = null;
                    Delete();
                }
            }
        }

        public void Send(ref Packets.PacketClass packet)
        {
            lock (_sendLock)
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

                        WorldServiceLocator.WorldServer.ClsWorldServer.Cluster?.ClientSend(Index, packet.Data);
                    }
                }
                catch (Exception ex)
                {
                    SetError(ex, $"Connection from [{IP}:{Port}] cause error {ex.Message}{Environment.NewLine}", LogType.CRITICAL);

                    if (DEBUG_CONNECTION)
                    {
                        return;
                    }

                    WorldServiceLocator.WorldServer.ClsWorldServer.Cluster = null;
                    Delete();
                }
            }
        }

        public void SendMultiplyPackets(ref Packets.PacketClass packet)
        {
            lock (_sendLock)
            {
                try
                {
                    if (packet.OpCode == Opcodes.SMSG_UPDATE_OBJECT)
                    {
                        packet.CompressUpdatePacket();
                    }
                    packet.UpdateLength();
                    var data = (byte[])packet.Data.Clone();

                    WorldServiceLocator.WorldServer.ClsWorldServer.Cluster?.ClientSend(Index, data);
                }
                catch (Exception ex)
                {
                    SetError(ex, $"Connection from [{IP}:{Port}] cause error {ex.Message}{Environment.NewLine}", LogType.CRITICAL);

                    if (DEBUG_CONNECTION)
                    {
                        return;
                    }

                    WorldServiceLocator.WorldServer.ClsWorldServer.Cluster = null;
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
            WorldServiceLocator.WorldServer.Log.WriteLine(logType, message, ex);
        }

        private void DumpPacket(Packets.PacketClass packet)
        {
            if (packet == null)
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "Unable to dump packet");
                return;
            }

            try
            {
                var packets4 = WorldServiceLocator.Packets;
                var data4 = packet.Data;
                var client = this;
                packets4.DumpPacket(data4, client);
            }
            catch (Exception ex)
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "Unable to dump packet", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.NETWORK, $"Connection from [{IP}:{Port}] disposed.");

            IsActive = false;

            try
            {
                ProcessQueueSempahore.Set(); //Allow thread to exit.
            }
            catch (ObjectDisposedException) { }

            try
            {
                ProcessQueueThread?.Interrupt();
                ProcessQueueThread?.Join(1000);
            }
            catch (ThreadInterruptedException ex)
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "{0} Thread ID: {1}", ex, Thread.CurrentThread.ManagedThreadId);
            }
            ProcessQueueThread = null;

            try
            {
                ProcessQueueSempahore?.Dispose();
            }
            catch (Exception) { }

            Packets?.Clear();

            try
            {
                if (WorldServiceLocator.WorldServer.CLIENTs.ContainsKey(Index))
                {
                    WorldServiceLocator.WorldServer.CLIENTs.Remove(Index);
                }

                WorldServiceLocator.WorldServer.ClsWorldServer.Cluster?.ClientDrop(Index);

                if (WorldServiceLocator.WorldServer.CLIENTs.ContainsKey(Index))
                {
                    WorldServiceLocator.WorldServer.CLIENTs.Remove(Index);
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
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.FAILED, $"Connection from [{IP}:{Port}] was not properly disposed.", ex);
            }
        }
    }
}
