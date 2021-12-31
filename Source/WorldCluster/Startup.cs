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

using Mangos.Cluster.Configuration;
using Mangos.Configuration.Xml;
using Mangos.Loggers;
using Mangos.Network.Tcp;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace WorldCluster;

public class Startup
{
    private readonly ILogger logger;
    private readonly XmlConfigurationProvider<ClusterConfiguration> configurationProvider;
    private readonly TcpServer tcpServer;

    private readonly Mangos.Cluster.WorldCluster worldCluster;

    public Startup(
        ILogger logger,
        XmlConfigurationProvider<ClusterConfiguration> configurationProvider,
        Mangos.Cluster.WorldCluster worldCluster,
        TcpServer tcpServer)
    {
        this.worldCluster = worldCluster;
        this.configurationProvider = configurationProvider;
        this.logger = logger;
        this.tcpServer = tcpServer;
    }

    public async Task StartAsync()
    {
        LoadConfiguration();
        WriteServiceInformation();

        await worldCluster.StartAsync();

        StartTcpServer();

        worldCluster.WaitConsoleCommand();
    }

    private void LoadConfiguration()
    {
        configurationProvider.LoadFromFile("configs/WorldCluster.ini");
        logger.Debug("Cluster configuration has been loaded");
    }

    private void StartTcpServer()
    {
        var configuration = configurationProvider.GetConfiguration();
        IPEndPoint endpoint = IPEndPoint.Parse(configuration.WorldClusterEndpoint);
        tcpServer.Start(endpoint, 10);
        logger.Debug("Tcp server has been started");
    }

    private void WriteServiceInformation()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        var assemblyTitle = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
        var product = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

        Console.Title = $"{assemblyTitle} v{Assembly.GetExecutingAssembly().GetName().Version}";
        logger.Debug(product);
        logger.Debug(copyright);
        logger.Message(@" __  __      _  _  ___  ___  ___               ");
        logger.Message(@"|  \/  |__ _| \| |/ __|/ _ \/ __|   We Love    ");
        logger.Message(@"| |\/| / _` | .` | (_ | (_) \__ \   Vanilla Wow");
        logger.Message(@"|_|  |_\__,_|_|\_|\___|\___/|___/              ");
        logger.Message("                                                ");
        logger.Message("Website / Forum / Support: https://getmangos.eu/");
    }
}
