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

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mangos.WoWFakeClient;

public static class Worldserver
{
    private static readonly Socket Connection = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
    private static IPAddress ConnIP;
    private static int ConnPort;
    public static Queue Queue = new();
    private static Timer PingTimer;
    public static int PingSent;
    public static uint CurrentPing;
    public static int CurrentLatency;
    public static Dictionary<OPCODES, HandlePacket> PacketHandlers = new();

    public delegate void HandlePacket(ref Packets.PacketClass Packet);

    public static uint ClientSeed;
    public static uint ServerSeed;
    public static byte[] Key = new byte[4];
    public static ulong CharacterGUID;
    public static bool Encoding;
    public static bool Decoding;

    [DllImport("winmm.dll")]
    public static extern int timeGetTime();

    [DllImport("winmm.dll")]
    public static extern int timeBeginPeriod(int uPeriod);

    public static bool WS_Connected()
    {
        return Connection is not null && Connection.Connected;
    }

    public static void InitializePackets()
    {
        PacketHandlers[OPCODES.SMSG_PONG] = WS_Auth.On_SMSG_PONG;
        PacketHandlers[OPCODES.SMSG_AUTH_CHALLENGE] = WS_Auth.On_SMSG_AUTH_CHALLENGE;
        PacketHandlers[OPCODES.SMSG_AUTH_RESPONSE] = WS_Auth.On_SMSG_AUTH_RESPONSE;
        PacketHandlers[OPCODES.SMSG_CHAR_ENUM] = WS_Auth.On_SMSG_CHAR_ENUM;
        PacketHandlers[OPCODES.SMSG_SET_REST_START] = WS_Movement.On_SMSG_SET_REST_START;
        PacketHandlers[OPCODES.SMSG_MESSAGECHAT] = WS_Chat.On_SMSG_MESSAGECHAT;
        PacketHandlers[OPCODES.SMSG_WARDEN_DATA] = WS_WardenClient.On_SMSG_WARDEN_DATA;
    }

    public static void ConnectToServer(string IP, int Port)
    {
        try
        {
            Connection.Connect(IP, Port);
            Thread NewThread;
            NewThread = new Thread(ProcessServerConnection)
            {
                Name = "World Server, Connected"
            };
            NewThread.Start();
        }
        catch (Exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Could not connect to the world server.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public static void ProcessServerConnection()
    {
        ConnIP = ((IPEndPoint)Connection.RemoteEndPoint).Address;
        ConnPort = ((IPEndPoint)Connection.RemoteEndPoint).Port;
        Console.WriteLine("[{0}][World] Connected to [{1}:{2}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), ConnIP, ConnPort);
        byte[] Buffer;
        int bytes;
        Thread oThread;
        oThread = Thread.CurrentThread;
        OnConnect();
        Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
        Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
        try
        {
            while (true)
            {
                Thread.Sleep(10);
                if (Connection.Available > 0)
                {
                    Buffer = new byte[Connection.Available];
                    bytes = Connection.Receive(Buffer, Buffer.Length, 0);
                    while (bytes > 0)
                    {
                        if (Decoding)
                        {
                            Decode(ref Buffer);
                        }

                        // Calculate Length from packet
                        var PacketLen = Buffer[1] + (Buffer[0] * 256) + 2;
                        if (bytes < PacketLen)
                        {
                            Console.WriteLine("[{0}][World] Bad Packet length [{1}][{2}] bytes", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), bytes, PacketLen);
                            break;
                        }

                        // Move packet to Data
                        var data = new byte[PacketLen];
                        Array.Copy(Buffer, data, PacketLen);
                        Packets.PacketClass Packet = new(ref data);
                        lock (Queue.SyncRoot)
                        {
                            Queue.Enqueue(Packet);
                        }

                        bytes -= PacketLen;
                        Array.Copy(Buffer, PacketLen, Buffer, 0, bytes);
                    }

                    ThreadPool.QueueUserWorkItem(_ => OnData());
                }

                if (!Connection.Connected)
                {
                    break;
                }

                if (Connection.Poll(100, SelectMode.SelectRead) & Connection.Available == 0)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("ProcessServerConnection has thrown an Exception! The exception is {0}", e);
        }

        Connection.Close();
        Console.WriteLine("[{0}][World] Disconnected.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
        oThread.Interrupt();
    }

    public static void Disconnect()
    {
        Connection.Close();
    }

    public static void OnConnect()
    {
        // Disconnect the realm server now that we're connected
        Realmserver.Disconnect();

        // Reset values
        Encoding = false;
        Decoding = false;
        Key[0] = 0;
        Key[1] = 0;
        Key[2] = 0;
        Key[3] = 0;

        // Start the ping timer
        PingTimer = new Timer(Ping, null, 30000, 30000);
    }

    public static void Ping(object State)
    {
        try
        {
            if (CurrentPing == uint.MaxValue)
            {
                CurrentPing = 0U;
            }

            CurrentPing = (uint)(CurrentPing + 1L);
            PingSent = timeGetTime();
            Packets.PacketClass Ping = new(OPCODES.CMSG_PING);
            Ping.AddUInt32(CurrentPing);
            Ping.AddInt32(CurrentLatency);
            Send(Ping);
            Ping.Dispose();
        }
        catch (Exception)
        {
            PingTimer.Dispose();
        }
    }

    public static void OnData()
    {
        Packets.PacketClass Packet;
        while (Queue.Count > 0)
        {
            lock (Queue.SyncRoot)
            {
                Packet = (Packets.PacketClass)Queue.Dequeue();
            }

            if (PacketHandlers.ContainsKey((OPCODES)Packet.OpCode))
            {
                try
                {
                    PacketHandlers[(OPCODES)Packet.OpCode].Invoke(ref Packet);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Opcode handler {2}:{2:X} caused an error:{1}{0}", e, Constants.vbCrLf, Packet.OpCode);
                }
            }

            Packet.Dispose();
        }
    }

    public static void Send(Packets.PacketClass Packet)
    {
        if (Encoding)
        {
            Encode(ref Packet.Data);
        }

        var i = Connection.Send(Packet.Data, 0, Packet.Data.Length, SocketFlags.None);
    }

    public static void Encode(ref byte[] Buffer)
    {
        // Encoding client messages
        int T;
        for (T = 0; T <= 5; T++)
        {
            Buffer[T] = (byte)((((Realmserver.SS_Hash[Key[3]] ^ Buffer[T]) & 0xFF) + Key[2]) & 0xFF);
            Key[2] = Buffer[T];
            Key[3] = (byte)((Key[3] + 1) % 40);
        }
    }

    public static void Decode(ref byte[] Buffer)
    {
        // Decoding server messages
        int T;
        int A;
        int B;
        int d;
        for (T = 0; T <= 3; T++)
        {
            A = Key[0];
            Key[0] = Buffer[T];
            B = Buffer[T];
            B = (byte)((B - A) & 0xFF);
            d = Key[1];
            A = Realmserver.SS_Hash[d];
            A = (byte)((A ^ B) & 0xFF);
            Buffer[T] = (byte)A;
            A = Key[1];
            A += 1;
            Key[1] = (byte)(A % 40);
        }
    }
}
