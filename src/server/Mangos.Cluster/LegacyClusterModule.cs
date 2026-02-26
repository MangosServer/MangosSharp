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

using Autofac;
using Mangos.Cluster.DataStores;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Handlers.Guild;
using Mangos.Cluster.Network;
using Mangos.Cluster.Verification;
using Mangos.Common.Globals;
using Mangos.Cluster.Interop;
using Mangos.Common;
using Mangos.DataStores;
using Mangos.Zip;

namespace Mangos.Cluster;

public sealed class LegacyClusterModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DataStoreProvider>().AsSelf().SingleInstance();
        builder.RegisterType<ClientClass>().AsSelf().InstancePerLifetimeScope();

        builder.RegisterType<MangosGlobalConstants>().As<MangosGlobalConstants>().SingleInstance();
        builder.RegisterType<Common.Globals.Functions>().As<Common.Globals.Functions>().SingleInstance();
        builder.RegisterType<Common.StringFunctions>().As<Common.StringFunctions>().SingleInstance();
        builder.RegisterType<ZipService>().As<ZipService>().SingleInstance();
        builder.RegisterType<NativeMethods>().As<NativeMethods>().SingleInstance();
        builder.RegisterType<LegacyWorldCluster>().As<LegacyWorldCluster>().SingleInstance();
        builder.RegisterType<WorldServerClass>().As<WorldServerClass>().As<ICluster>().SingleInstance();
        builder.RegisterType<WsDbcDatabase>().As<WsDbcDatabase>().SingleInstance();
        builder.RegisterType<WsDbcLoad>().As<WsDbcLoad>().SingleInstance();
        builder.RegisterType<Globals.Functions>().As<Globals.Functions>().SingleInstance();
        builder.RegisterType<Packets>().As<Packets>().SingleInstance();
        builder.RegisterType<WcGuild>().As<WcGuild>().SingleInstance();
        builder.RegisterType<WcNetwork>().As<WcNetwork>().SingleInstance();
        builder.RegisterType<WcHandlers>().As<WcHandlers>().SingleInstance();
        builder.RegisterType<WcHandlersAuth>().As<WcHandlersAuth>().SingleInstance();
        builder.RegisterType<WcHandlersBattleground>().As<WcHandlersBattleground>().SingleInstance();
        builder.RegisterType<WcHandlersChat>().As<WcHandlersChat>().SingleInstance();
        builder.RegisterType<WcHandlersGroup>().As<WcHandlersGroup>().SingleInstance();
        builder.RegisterType<WcHandlersGuild>().As<WcHandlersGuild>().SingleInstance();
        builder.RegisterType<WcHandlersMisc>().As<WcHandlersMisc>().SingleInstance();
        builder.RegisterType<WcHandlersMovement>().As<WcHandlersMovement>().SingleInstance();
        builder.RegisterType<WcHandlersSocial>().As<WcHandlersSocial>().SingleInstance();
        builder.RegisterType<WcHandlersTickets>().As<WcHandlersTickets>().SingleInstance();
        builder.RegisterType<WsHandlerChannels>().As<WsHandlerChannels>().SingleInstance();
        builder.RegisterType<WcHandlerCharacter>().As<WcHandlerCharacter>().SingleInstance();
        builder.RegisterType<WcHandlersPetition>().As<WcHandlersPetition>().SingleInstance();
        builder.RegisterType<ClusterVerifier>().As<ClusterVerifier>().SingleInstance();

        builder.RegisterType<ClusterServiceLocator>().As<ClusterServiceLocator>()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
            .SingleInstance();
    }
}
