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

using Autofac;
using Mangos.Common.Globals;
using Mangos.Configuration;
using Mangos.Configuration.Xml;
using Mangos.DataStores;
using Mangos.Loggers;
using Mangos.Loggers.Console;
using Mangos.World.AI;
using Mangos.World.Auction;
using Mangos.World.Battlegrounds;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Gossip;
using Mangos.World.Handlers;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Social;
using Mangos.World.Spells;
using Mangos.World.Warden;
using Mangos.World.Weather;
using Mangos.Zip;
using System.Threading.Tasks;
using Functions = Mangos.Common.Legacy.Globals.Functions;
using NativeMethods = Mangos.Common.Legacy.NativeMethods;

namespace Mangos.World;

public sealed class Program
{
    public static async Task Main()
    {
        var container = WorldServiceLocator._Container;
        var worldServer = container.Resolve<WorldServer>();
        await worldServer.StartAsync();
    }

    public static IContainer CreateContainer()
    {
        ContainerBuilder builder = new();
        RegisterLoggers(builder);
        RegisterConfiguration(builder);
        RegisterServices(builder);
        return builder.Build();
    }

    public static void RegisterLoggers(ContainerBuilder builder)
    {
        builder.RegisterType<ConsoleLogger>().As<ILogger>();
    }

    public static void RegisterConfiguration(ContainerBuilder builder)
    {
        builder.RegisterType<XmlConfigurationProvider<WorldServerConfiguration>>()
            .As<IConfigurationProvider<WorldServerConfiguration>>()
            .AsSelf()
            .SingleInstance();
    }

    public static void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<MangosGlobalConstants>().As<MangosGlobalConstants>().SingleInstance();
        builder.RegisterType<Functions>().As<Functions>().SingleInstance();
        builder.RegisterType<Common.Legacy.Functions>().As<Common.Legacy.Functions>().SingleInstance();
        builder.RegisterType<ZipService>().As<ZipService>().SingleInstance();
        builder.RegisterType<NativeMethods>().As<NativeMethods>().SingleInstance();
        builder.RegisterType<WorldServer>().As<WorldServer>().SingleInstance();
        builder.RegisterType<Globals.Functions>().As<Globals.Functions>().SingleInstance();
        builder.RegisterType<WS_Creatures_AI>().As<WS_Creatures_AI>().SingleInstance();
        builder.RegisterType<WS_Auction>().As<WS_Auction>().SingleInstance();
        builder.RegisterType<WS_Battlegrounds>().As<WS_Battlegrounds>().SingleInstance();
        builder.RegisterType<WS_DBCDatabase>().As<WS_DBCDatabase>().SingleInstance();
        builder.RegisterType<WS_DBCLoad>().As<WS_DBCLoad>().SingleInstance();
        builder.RegisterType<Packets>().As<Packets>().SingleInstance();
        builder.RegisterType<WS_GuardGossip>().As<WS_GuardGossip>().SingleInstance();
        builder.RegisterType<WS_Loot>().As<WS_Loot>().SingleInstance();
        builder.RegisterType<WS_Maps>().As<WS_Maps>().SingleInstance();
        builder.RegisterType<WS_Corpses>().As<WS_Corpses>().SingleInstance();
        builder.RegisterType<WS_Creatures>().As<WS_Creatures>().SingleInstance();
        builder.RegisterType<WS_DynamicObjects>().As<WS_DynamicObjects>().SingleInstance();
        builder.RegisterType<WS_GameObjects>().As<WS_GameObjects>().SingleInstance();
        builder.RegisterType<WS_Items>().As<WS_Items>().SingleInstance();
        builder.RegisterType<WS_NPCs>().As<WS_NPCs>().SingleInstance();
        builder.RegisterType<WS_Pets>().As<WS_Pets>().SingleInstance();
        builder.RegisterType<WS_Transports>().As<WS_Transports>().SingleInstance();
        builder.RegisterType<CharManagementHandler>().As<CharManagementHandler>().SingleInstance();
        builder.RegisterType<WS_CharMovement>().As<WS_CharMovement>().SingleInstance();
        builder.RegisterType<WS_Combat>().As<WS_Combat>().SingleInstance();
        builder.RegisterType<WS_Commands>().As<WS_Commands>().SingleInstance();
        builder.RegisterType<WS_Handlers>().As<WS_Handlers>().SingleInstance();
        builder.RegisterType<WS_Handlers_Battleground>().As<WS_Handlers_Battleground>().SingleInstance();
        builder.RegisterType<WS_Handlers_Chat>().As<WS_Handlers_Chat>().SingleInstance();
        builder.RegisterType<WS_Handlers_Gamemaster>().As<WS_Handlers_Gamemaster>().SingleInstance();
        builder.RegisterType<WS_Handlers_Instance>().As<WS_Handlers_Instance>().SingleInstance();
        builder.RegisterType<WS_Handlers_Misc>().As<WS_Handlers_Misc>().SingleInstance();
        builder.RegisterType<WS_Handlers_Taxi>().As<WS_Handlers_Taxi>().SingleInstance();
        builder.RegisterType<WS_Handlers_Trade>().As<WS_Handlers_Trade>().SingleInstance();
        builder.RegisterType<WS_Handlers_Warden>().As<WS_Handlers_Warden>().SingleInstance();
        builder.RegisterType<WS_Player_Creation>().As<WS_Player_Creation>().SingleInstance();
        builder.RegisterType<WS_Player_Initializator>().As<WS_Player_Initializator>().SingleInstance();
        builder.RegisterType<WS_PlayerData>().As<WS_PlayerData>().SingleInstance();
        builder.RegisterType<WS_PlayerHelper>().As<WS_PlayerHelper>().SingleInstance();
        builder.RegisterType<WS_Network>().As<WS_Network>().SingleInstance();
        builder.RegisterType<WS_TimerBasedEvents>().As<WS_TimerBasedEvents>().SingleInstance();
        builder.RegisterType<WS_Group>().As<WS_Group>().SingleInstance();
        builder.RegisterType<WS_Guilds>().As<WS_Guilds>().SingleInstance();
        builder.RegisterType<WS_Mail>().As<WS_Mail>().SingleInstance();
        builder.RegisterType<WS_Spells>().As<WS_Spells>().SingleInstance();
        builder.RegisterType<WS_Warden>().As<WS_Warden>().SingleInstance();
        builder.RegisterType<WS_Weather>().As<WS_Weather>().SingleInstance();
        builder.RegisterType<DataStoreProvider>().As<DataStoreProvider>().SingleInstance();
    }
}
