using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using global;
using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Misc;
using Microsoft.VisualBasic;

namespace Mangos.Realm
{
    public sealed class ClientClass : IDisposable
    {
        private readonly Global_Constants _Global_Constants;
        private readonly RealmServer _RealmServer;

        public ClientClass(Global_Constants globalConstants, RealmServer realmServer)
        {
            _Global_Constants = globalConstants;
            _RealmServer = realmServer;
        }

        public Socket Socket;
        public IPAddress Ip = IPAddress.Parse("127.0.0.1");
        public int Port = 0;
        public AuthEngineClass AuthEngine;
        public string Account = "";
        // Public Language As String = "enGB"
        // Public Expansion As ExpansionLevel = ExpansionLevel.NORMAL
        public string UpdateFile = "";
        public AccessLevel Access = AccessLevel.Player;

        private void OnData(byte[] data)
        {
            switch ((AuthCMD)data[0])
            {
                case var @case when @case == AuthCMD.CMD_AUTH_LOGON_CHALLENGE:
                case var case1 when case1 == AuthCMD.CMD_AUTH_RECONNECT_CHALLENGE:
                    {
                        Console.WriteLine("[{0}] [{1}:{2}] RS_LOGON_CHALLENGE", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port);
                        var argclient = this;
                        _RealmServer.On_RS_LOGON_CHALLENGE(ref data, ref argclient);
                        break;
                    }

                case var case2 when case2 == AuthCMD.CMD_AUTH_LOGON_PROOF:
                case var case3 when case3 == AuthCMD.CMD_AUTH_RECONNECT_PROOF:
                    {
                        Console.WriteLine("[{0}] [{1}:{2}] RS_LOGON_PROOF", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port);
                        var argclient1 = this;
                        _RealmServer.On_RS_LOGON_PROOF(ref data, ref argclient1);
                        break;
                    }

                case var case4 when case4 == AuthCMD.CMD_AUTH_REALMLIST:
                    {
                        Console.WriteLine("[{0}] [{1}:{2}] RS_REALMLIST", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port);
                        var argclient2 = this;
                        _RealmServer.On_RS_REALMLIST(ref data, ref argclient2);
                        break;
                    }

                // TODO: No Value listed for AuthCMD
                // Case CMD_AUTH_UPDATESRV
                // Console.WriteLine("[{0}] [{1}:{2}] RS_UPDATESRV", Format(TimeOfDay, "hh:mm:ss"), Ip, Port)

                // ToDo: Check if these packets exist in supported version
                case var case5 when case5 == AuthCMD.CMD_XFER_ACCEPT:
                    {
                        // Console.WriteLine("[{0}] [{1}:{2}] CMD_XFER_ACCEPT", Format(TimeOfDay, "hh:mm:ss"), IP, Port)
                        var argclient3 = this;
                        _RealmServer.On_CMD_XFER_ACCEPT(ref data, ref argclient3);
                        break;
                    }

                case var case6 when case6 == AuthCMD.CMD_XFER_RESUME:
                    {
                        // Console.WriteLine("[{0}] [{1}:{2}] CMD_XFER_RESUME", Format(TimeOfDay, "hh:mm:ss"), IP, Port)
                        var argclient4 = this;
                        _RealmServer.On_CMD_XFER_RESUME(ref data, ref argclient4);
                        break;
                    }

                case var case7 when case7 == AuthCMD.CMD_XFER_CANCEL:
                    {
                        // Console.WriteLine("[{0}] [{1}:{2}] CMD_XFER_CANCEL", Format(TimeOfDay, "hh:mm:ss"), IP, Port)
                        var argclient5 = this;
                        _RealmServer.On_CMD_XFER_CANCEL(ref data, ref argclient5);
                        break;
                    }

                default:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[{0}] [{1}:{2}] Unknown Opcode 0x{3}", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port, data[0]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        var argclient6 = this;
                        _RealmServer.DumpPacket(ref data, ref argclient6);
                        break;
                    }
            }
        }

        public void Process()
        {
            IPEndPoint remoteEndPoint = (IPEndPoint)Socket.RemoteEndPoint;
            Ip = remoteEndPoint.Address;
            Port = remoteEndPoint.Port;

            // DONE: Connection spam protection
            uint ipInt;
            ipInt = _RealmServer.Ip2Int(Ip.ToString());
            if (!_RealmServer.LastSocketConnection.ContainsKey(ipInt))
            {
                _RealmServer.LastSocketConnection.Add(ipInt, DateAndTime.Now.AddSeconds(5d));
            }
            else if (DateAndTime.Now > _RealmServer.LastSocketConnection[ipInt])
            {
                _RealmServer.LastSocketConnection[ipInt] = DateAndTime.Now.AddSeconds(5d);
            }
            else
            {
                Socket.Close();
                Dispose();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[{0}] Incoming connection from [{1}:{2}]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port);
            Console.WriteLine("[{0}] [{1}:{2}] Checking for banned IP.", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port);
            Console.ForegroundColor = ConsoleColor.Gray;
            if (!_RealmServer.AccountDatabase.QuerySQL("SELECT ip FROM ip_banned WHERE ip = '" + Ip.ToString() + "';"))
            {
                while (!_RealmServer.RealmServerClass.FlagStopListen)
                {
                    Thread.Sleep(_Global_Constants.ConnectionSleepTime);
                    if (Socket.Available > 0)
                    {
                        if (Socket.Available > 100) // DONE: Data flood protection
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[{0}] Incoming Connection dropped for flooding", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        }

                        byte[] buffer;
                        buffer = new byte[Socket.Available];
                        int dummyBytes = Socket.Receive(buffer, buffer.Length, 0);
                        Console.WriteLine("[{0}] Incoming connection from [{1}:{2}]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port);
                        Console.WriteLine("Data Packet: [{0}] ", dummyBytes);
                        OnData(buffer);
                    }

                    if (!Socket.Connected)
                        break;
                    if (Socket.Poll(100, SelectMode.SelectRead) & Socket.Available == 0)
                        break;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[{0}] [{1}:{2}] This ip is banned.", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Socket.Close();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[{0}] Connection from [{1}:{2}] closed", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port);
            Console.ForegroundColor = ConsoleColor.Gray;
            Dispose();
        }

        public void Send(byte[] data, string packetName)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[{0}] [{1}:{2}] ({4}) Data sent, result code {3}", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port, Socket.Send(data, 0, data.Length, SocketFlags.None), packetName);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (Exception err)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[{0}] Connection from [{1}:{2}] do not exist - ERROR!!!", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Ip, Port);
                Console.ForegroundColor = ConsoleColor.Gray;
                Socket.Close();
            }
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private bool _disposedValue;

        // To detect redundant calls

        // IDisposable
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
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
    }
}