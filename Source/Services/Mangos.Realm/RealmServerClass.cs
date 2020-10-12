﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using global;
using Mangos.Configuration;
using Mangos.Realm.Factories;
using Microsoft.VisualBasic;

namespace Mangos.Realm
{
    public class RealmServerClass : IDisposable
    {
        private readonly IConfigurationProvider<RealmServerConfiguration> _configurationProvider;
        private readonly Global_Constants _Global_Constants;
        private readonly ClientClassFactory _ClientClassFactory;
        private readonly RealmServer _RealmServer;

        public RealmServerClass(Global_Constants globalConstants, ClientClassFactory clientClassFactory, RealmServer realmServer, IConfigurationProvider<RealmServerConfiguration> configurationProvider)
        {
            _Global_Constants = globalConstants;
            _ClientClassFactory = clientClassFactory;
            _RealmServer = realmServer;
            _configurationProvider = configurationProvider;
        }

        public void Start()
        {
            var configuration = _configurationProvider.GetConfiguration();
            LstHost = IPAddress.Parse(configuration.RealmServerAddress);
            try
            {
                var tcpListener = new TcpListener(LstHost, configuration.RealmServerPort);
                LstConnection = tcpListener;
                LstConnection.Start();
                Thread rsListenThread;
                var thread = new Thread(AcceptConnection) { Name = "Realm Server, Listening" };
                rsListenThread = thread;
                rsListenThread.Start();
                Console.WriteLine("[{0}] Listening on {1} on port {2}", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), LstHost, configuration.RealmServerPort);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[{0}] Error in {2}: {1}.", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), e.Message, e.Source);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private void AcceptConnection()
        {
            while (!FlagStopListen)
            {
                Thread.Sleep(_Global_Constants.ConnectionSleepTime);
                if (LstConnection.Pending())
                {
                    var client = _ClientClassFactory.Create(_RealmServer);
                    client.Socket = LstConnection.AcceptSocket();
                    new Thread(client.Process).Start();
                }
            }
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private bool _disposedValue; // To detect redundant calls

        public bool FlagStopListen { get; set; } = false;
        public TcpListener LstConnection { get; set; }
        public IPAddress LstHost { get; set; }

        // IDisposable
        // Default Functions
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                FlagStopListen = true;
                LstConnection.Stop();
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