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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using global;
using Mangos.Configuration;
using Mangos.Loggers;
using Mangos.Realm.Factories;
using Microsoft.VisualBasic;

namespace Mangos.Realm
{
	public class RealmServerClass : IDisposable
    {
        private readonly IConfigurationProvider<RealmServerConfiguration> configurationProvider;
		private readonly ILogger logger;

        private readonly Global_Constants _Global_Constants;
        private readonly ClientClassFactory _ClientClassFactory;
        private readonly RealmServer _RealmServer;

		public RealmServerClass(
			Global_Constants globalConstants,
			ClientClassFactory clientClassFactory,
			RealmServer realmServer,
			IConfigurationProvider<RealmServerConfiguration> configurationProvider, 
			ILogger logger)
		{
			_Global_Constants = globalConstants;
			_ClientClassFactory = clientClassFactory;
			_RealmServer = realmServer;
			this.configurationProvider = configurationProvider;
			this.logger = logger;
		}

		public void Start()
        {
            var configuration = configurationProvider.GetConfiguration();
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
				logger.Debug("Listening on {0} on port {1}", LstHost, configuration.RealmServerPort);
            }
            catch (Exception e)
            {
				logger.Error("Error in {0}: {1}.", e.Message, e.Source);
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