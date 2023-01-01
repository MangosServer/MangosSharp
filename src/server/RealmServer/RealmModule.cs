//
// Copyright (C) 2013-2023 getMaNGOS <https://getmangos.eu>
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
using Mangos.Realm.Network.Handlers;
using Mangos.Tcp;
using RealmServer.Domain;
using RealmServer.Handlers;
using RealmServer.Network;
using RealmServer.Requests;

namespace RealmServer;

internal sealed class RealmModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<RealmTcpConnection>().As<ITcpConnection>().InstancePerLifetimeScope();

        builder.RegisterType<ClientState>().InstancePerLifetimeScope();

        RegisterHandlers(builder);
        RegisterDispatchers(builder);
    }

    private void RegisterHandlers(ContainerBuilder builder)
    {
        builder.RegisterType<RsLogonChallengeHandler>().InstancePerLifetimeScope();
        builder.RegisterType<RsLogonProofHandler>().InstancePerLifetimeScope();
        builder.RegisterType<AuthReconnectChallengeHandler>().InstancePerLifetimeScope();
        builder.RegisterType<AuthRealmlistHandler>().InstancePerLifetimeScope();
    }

    private void RegisterDispatchers(ContainerBuilder builder)
    {
        builder.RegisterType<HandlerDispatcher<RsLogonChallengeHandler, RsLogonChallengeRequest>>()
            .As<IHandlerDispatcher>()
            .InstancePerLifetimeScope();

        builder.RegisterType<HandlerDispatcher<RsLogonProofHandler, RsLogonProofRequest>>()
            .As<IHandlerDispatcher>()
            .InstancePerLifetimeScope();

        builder.RegisterType<HandlerDispatcher<AuthReconnectChallengeHandler, RsLogonChallengeRequest>>()
            .As<IHandlerDispatcher>()
            .InstancePerLifetimeScope();

        builder.RegisterType<HandlerDispatcher<AuthRealmlistHandler, AuthRealmlistRequest>>()
            .As<IHandlerDispatcher>()
            .InstancePerLifetimeScope();
    }
}
