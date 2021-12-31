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
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Mangos.WoWFakeClient;

internal static class Realmserver
{
    private static readonly Random Random = new();
    private static readonly Socket Connection = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
    private static IPAddress ConnIP;
    private static int ConnPort;
    public static string Account = "Administrator";
    public static string Password = "Administrator";
    public static byte VersionA = 1;
    public static byte VersionB = 12;
    public static byte VersionC = 1;
    public static ushort Revision = 5875; // 5875 = 1.12.1, 6005 = 1.12.2, 6141 = 1.12.3
    public static string RealmIP = "127.0.0.1";
    public static int RealmPort = 3724;
    public static byte[] ServerB = new byte[32];
    public static byte[] G;
    public static byte[] N;
    public static byte[] Salt = new byte[32];
    public static byte[] CrcSalt = new byte[16];
    public static byte[] A = new byte[32];
    public static byte[] PublicA = new byte[32];
    public static byte[] M1 = new byte[20];
    public static byte[] CrcHash = new byte[20];
    public static byte[] SS_Hash;
    private const byte CMD_AUTH_LOGON_CHALLENGE = 0x0;
    private const byte CMD_AUTH_LOGON_PROOF = 0x1;
    private const byte CMD_AUTH_RECONNECT_CHALLENGE = 0x2;
    private const byte CMD_AUTH_RECONNECT_PROOF = 0x3;
    private const byte CMD_AUTH_UPDATESRV = 0x4;
    private const byte CMD_AUTH_REALMLIST = 0x10;

    public static void Main()
    {
        Console.Title = "WoW Fake Client";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("WoW Fake Client made by UniX");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Worldserver.InitializePackets();
        Worldserver.timeBeginPeriod(1);
        ConnectToRealm();
        Thread NewThread;
        NewThread = new Thread(CheckConnection)
        {
            Name = "Checking Connection State"
        };
        NewThread.Start();
        var sChatMsg = "";
        while (true)
        {
            sChatMsg = Console.ReadLine();
            if (sChatMsg.ToLower() == "quit")
            {
                break;
            }

            WS_Chat.SendChatMessage(sChatMsg);
        }
    }

    public static bool RS_Connected()
    {
        return Connection is not null && Connection.Connected;
    }

    public static void CheckConnection()
    {
        while (true)
        {
            if (RS_Connected() == false && Worldserver.WS_Connected() == false)
            {
                Thread.Sleep(6000);
                ConnectToRealm();
            }
        }
    }

    public static void ConnectToRealm()
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Connecting to {0}:{1}", RealmIP, RealmPort);
            Console.ForegroundColor = ConsoleColor.White;
            Connection.Connect(RealmIP, RealmPort);
            Thread NewThread;
            NewThread = new Thread(ProcessServerConnection)
            {
                Name = "Realm Server, Connected"
            };
            NewThread.Start();
        }
        catch (Exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Could not connect to the server.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public static void ProcessServerConnection()
    {
        ConnIP = ((IPEndPoint)Connection.RemoteEndPoint).Address;
        ConnPort = ((IPEndPoint)Connection.RemoteEndPoint).Port;
        Console.WriteLine("[{0}][Realm] Connected to [{1}:{2}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), ConnIP, ConnPort);
        byte[] Buffer;
        int bytes;
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
                    OnData(Buffer);
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
        catch (ObjectDisposedException)
        {
        }
        // Nothing
        catch (Exception ex)
        {
            Console.WriteLine("Error in realm socket.{0}{1}", Constants.vbCrLf, ex);
        }

        Connection.Close();
        Console.WriteLine("[{0}][Realm] Disconnected.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
    }

    public static void Disconnect()
    {
        Connection.Close();
    }

    public static void OnConnect()
    {
        Packets.PacketClass LogonChallenge = new(CMD_AUTH_LOGON_CHALLENGE);
        LogonChallenge.AddInt8(0x8);
        LogonChallenge.AddUInt16(0); // Packet length
        LogonChallenge.AddString("WoW");
        LogonChallenge.AddInt8(VersionA); // Version
        LogonChallenge.AddInt8(VersionB); // Version
        LogonChallenge.AddInt8(VersionC); // Version
        LogonChallenge.AddUInt16(Revision); // Revision
        LogonChallenge.AddString("x86", Reversed: true);
        LogonChallenge.AddString("Win", Reversed: true);
        LogonChallenge.AddString("enUS", false, true);
        LogonChallenge.AddInt32(0x3C); // Timezone
        LogonChallenge.AddUInt32(BitConverter.ToUInt32(((IPEndPoint)Connection.LocalEndPoint).Address.GetAddressBytes(), 0));
        LogonChallenge.AddInt8((byte)Account.Length);
        LogonChallenge.AddString(Account.ToUpper(), false);
        LogonChallenge.AddUInt16((ushort)(LogonChallenge.Data.Length - 4), 2);
        SendR(LogonChallenge);
        LogonChallenge.Dispose();
        Console.WriteLine("[{0}][Realm] Sent Logon Challenge.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
    }

    public static void OnData(byte[] Buffer)
    {
        Packets.PacketClass Packet = new(ref Buffer, true);
        switch (Packet.OpCode)
        {
            case CMD_AUTH_LOGON_CHALLENGE:
                {
                    Console.WriteLine("[{0}][Realm] Received Logon Challenged.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                    switch (Buffer[1])
                    {
                        case 0: // No error
                            {
                                Console.WriteLine("[{0}][Realm] Challenge Success.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                                Packet.Offset = 3;
                                ServerB = Packet.GetByteArray(32);
                                var G_len = Packet.GetInt8();
                                G = Packet.GetByteArray(G_len);
                                var N_len = Packet.GetInt8();
                                N = Packet.GetByteArray(N_len);
                                Salt = Packet.GetByteArray(32);
                                CrcSalt = Packet.GetByteArray(16);
                                CalculateProof();
                                Thread.Sleep(100);
                                Packets.PacketClass LogonProof = new(CMD_AUTH_LOGON_PROOF);
                                LogonProof.AddByteArray(PublicA);
                                LogonProof.AddByteArray(M1);
                                LogonProof.AddByteArray(CrcHash);
                                LogonProof.AddInt8(0); // Added in 1.12.x client branch? Security Flags (&H0...&H4)?
                                SendR(LogonProof);
                                LogonProof.Dispose();
                                break;
                            }

                        case 4:
                        case 5: // Bad user
                            {
                                Console.WriteLine("[{0}][Realm] Bad account information, the account did not exist.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                                Connection.Close();
                                break;
                            }

                        case 9: // Bad version
                            {
                                Console.WriteLine("[{0}][Realm] Bad client version (the server does not allow our version).", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                                Connection.Close();
                                break;
                            }

                        default:
                            {
                                Console.WriteLine("[{0}][Realm] Unknown challenge error [{1}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), Buffer[1]);
                                Connection.Close();
                                break;
                            }
                    }

                    break;
                }

            case CMD_AUTH_LOGON_PROOF:
                {
                    Console.WriteLine("[{0}][Realm] Received Logon Proof.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                    switch (Buffer[1])
                    {
                        case 0: // No error
                            {
                                Console.WriteLine("[{0}][Realm] Proof Success.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                                Packets.PacketClass RealmList = new(CMD_AUTH_REALMLIST);
                                RealmList.AddInt32(0);
                                SendR(RealmList);
                                RealmList.Dispose();
                                break;
                            }

                        case 4:
                        case 5: // Bad user
                            {
                                Console.WriteLine("[{0}][Realm] Bad account information, your password was incorrect.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                                Connection.Close();
                                break;
                            }

                        case 9: // Bad version
                            {
                                Console.WriteLine("[{0}][Realm] Bad client version (the crc files are either too old or to new for this server).", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                                Connection.Close();
                                break;
                            }

                        default:
                            {
                                Console.WriteLine("[{0}][Realm] Unknown proof error [{1}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), Buffer[1]);
                                Connection.Close();
                                break;
                            }
                    }

                    break;
                }

            case CMD_AUTH_REALMLIST:
                {
                    Console.WriteLine("[{0}][Realm] Received Realm List.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                    Packet.Offset = 7;
                    int RealmCount = Packet.GetInt8();
                    if (RealmCount > 0)
                    {
                        for (int i = 1, loopTo = RealmCount; i <= loopTo; i++)
                        {
                            var RealmType = Packet.GetInt8();
                            var RealmLocked = Packet.GetInt8();
                            var Unk1 = Packet.GetInt8();
                            var Unk2 = Packet.GetInt8();
                            var RealmStatus = Packet.GetInt8();
                            var RealmName = Packet.GetString();
                            var RealmIP = Packet.GetString();
                            var RealmPopulation = Packet.GetFloat();
                            var RealmCharacters = Packet.GetInt8();
                            var RealmTimezone = Packet.GetInt8();
                            var Unk3 = Packet.GetInt8();
                            Console.WriteLine("[{0}][Realm] Connecting to realm [{1}][{2}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), RealmName, RealmIP);
                            if (Strings.InStr(RealmIP, ":") > 0)
                            {
                                var SplitIP = Strings.Split(RealmIP, ":");
                                if (SplitIP.Length == 2)
                                {
                                    if (Information.IsNumeric(SplitIP[1]))
                                    {
                                        Worldserver.ConnectToServer(SplitIP[0], Conversions.ToInteger(SplitIP[1]));
                                    }
                                    else
                                    {
                                        Console.WriteLine("[{0}][Realm] Invalid IP in realmlist [{1}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), RealmIP);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("[{0}][Realm] Invalid IP in realmlist [{1}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), RealmIP);
                                }
                            }
                            else
                            {
                                Console.WriteLine("[{0}][Realm] Invalid IP in realmlist [{1}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), RealmIP);
                            }

                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("[{0}][Realm] No realms were found.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                    }

                    break;
                }

            default:
                {
                    Console.WriteLine("[{0}][Realm] Unknown opcode [{1}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), Packet.OpCode);
                    break;
                }
        }
    }

    public static void SendR(Packets.PacketClass Packet)
    {
        var i = Connection.Send(Packet.Data, 0, Packet.Data.Length, SocketFlags.None);
    }

    public static void CalculateProof()
    {
        Random.NextBytes(A);
        Array.Reverse(A);
        var tempStr = Account.ToUpper() + ":" + Password.ToUpper();
        var temp = Encoding.ASCII.GetBytes(tempStr.ToCharArray());
        SHA1Managed algorithm1 = new();
        temp = algorithm1.ComputeHash(temp);
        var X = algorithm1.ComputeHash(Concat(Salt, temp));
        Array.Reverse(X);
        Array.Reverse(N);
        var K = new byte[] { 3 };
        var S = new byte[32];
        BigInteger BNg = new(G, isUnsigned: true, isBigEndian: true);
        BigInteger BNa = new(A, isUnsigned: true, isBigEndian: true);
        BigInteger BNn = new(N, isUnsigned: true, isBigEndian: true);
        BigInteger BNx = new(X, isUnsigned: true, isBigEndian: true);
        BigInteger BNk = new(K, isUnsigned: true, isBigEndian: true);
        BigInteger BNpublicA = BigInteger.ModPow(BNg, BNa, BNn);
        PublicA = BNpublicA.ToByteArray(isUnsigned: true, isBigEndian: true);
        Array.Reverse(PublicA);
        var U = algorithm1.ComputeHash(Concat(PublicA, ServerB));
        Array.Reverse(ServerB);
        Array.Reverse(U);
        BigInteger BNu = new(U, isUnsigned: true, isBigEndian: true);
        BigInteger BNb = new(ServerB, isUnsigned: true, isBigEndian: true);

        // S= (B - kg^x) ^ (a + ux)   (mod N)
        BigInteger temp1 = new();
        BigInteger temp2 = new();
        BigInteger temp3 = new();
        BigInteger temp4 = new();
        BigInteger temp5 = new();

        // Temp1 = g ^ x mod n
        temp1 = BigInteger.ModPow(BNg, BNx, BNn);

        // Temp2 = k * Temp1
        temp2 = BNk * temp1;

        // Temp3 = B - Temp2
        temp3 = BNb - temp2;

        // Temp4 = u * x
        temp4 = BNu * BNx;

        // Temp5 = a + Temp4
        temp5 = BNa + temp4;

        // S = Temp3 ^ Temp5 mod n
        BigInteger BNs = BigInteger.ModPow(temp3, temp5, BNn);
        S = BNs.ToByteArray(isUnsigned: true, isBigEndian: true);
        Array.Reverse(S);
        ArrayList list1 = new();
        list1 = SplitArray(S);
        list1[0] = algorithm1.ComputeHash((byte[])list1[0]);
        list1[1] = algorithm1.ComputeHash((byte[])list1[1]);
        SS_Hash = Combine((byte[])list1[0], (byte[])list1[1]);
        tempStr = Strings.UCase(Account.ToUpper());
        var User_Hash = algorithm1.ComputeHash(Encoding.UTF8.GetBytes(tempStr.ToCharArray()));
        Array.Reverse(N);
        Array.Reverse(ServerB);
        var N_Hash = algorithm1.ComputeHash(N);
        var G_Hash = algorithm1.ComputeHash(G);
        var NG_Hash = new byte[20];
        for (var i = 0; i <= 19; i++)
        {
            NG_Hash[i] = (byte)(N_Hash[i] ^ G_Hash[i]);
        }

        temp = Concat(NG_Hash, User_Hash);
        temp = Concat(temp, Salt);
        temp = Concat(temp, PublicA);
        temp = Concat(temp, ServerB);
        temp = Concat(temp, SS_Hash);
        M1 = algorithm1.ComputeHash(temp);
        CrcHash = new byte[17];
    }

    private static ArrayList SplitArray(byte[] bo)
    {
        var buffer1 = new byte[(bo.Length - 1)];
        if (bo.Length % 2 != 0 && bo.Length > 2)
        {
            Buffer.BlockCopy(bo, 1, buffer1, 0, bo.Length);
        }

        var buffer2 = new byte[(int)((bo.Length / 2d) - 1d + 1)];
        var buffer3 = new byte[(int)((bo.Length / 2d) - 1d + 1)];
        var num1 = 0;
        var num2 = 1;
        int num3;
        var loopTo = buffer2.Length - 1;
        for (num3 = 0; num3 <= loopTo; num3++)
        {
            buffer2[num3] = bo[num1];
            num1 += 1;
            num1 += 1;
        }

        int num4;
        var loopTo1 = buffer3.Length - 1;
        for (num4 = 0; num4 <= loopTo1; num4++)
        {
            buffer3[num4] = bo[num2];
            num2 += 1;
            num2 += 1;
        }

        ArrayList list1 = new()
        {
            buffer2,
            buffer3
        };
        return list1;
    }

    private static byte[] Combine(byte[] b1, byte[] b2)
    {
        if (b1.Length == b2.Length)
        {
            var buffer1 = new byte[(b1.Length + b2.Length)];
            var num1 = 0;
            var num2 = 1;
            int num3;
            var loopTo = b1.Length - 1;
            for (num3 = 0; num3 <= loopTo; num3++)
            {
                buffer1[num1] = b1[num3];
                num1 += 1;
                num1 += 1;
            }

            int num4;
            var loopTo1 = b2.Length - 1;
            for (num4 = 0; num4 <= loopTo1; num4++)
            {
                buffer1[num2] = b2[num4];
                num2 += 1;
                num2 += 1;
            }

            return buffer1;
        }

        return null;
    }

    public static byte[] Concat(byte[] a, byte[] b)
    {
        var buffer1 = new byte[(a.Length + b.Length)];
        int num1;
        var loopTo = a.Length - 1;
        for (num1 = 0; num1 <= loopTo; num1++)
        {
            buffer1[num1] = a[num1];
        }

        int num2;
        var loopTo1 = b.Length - 1;
        for (num2 = 0; num2 <= loopTo1; num2++)
        {
            buffer1[num2 + a.Length] = b[num2];
        }

        return buffer1;
    }
}
