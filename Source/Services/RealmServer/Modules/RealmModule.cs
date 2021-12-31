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
using Mangos.Realm;
using Mangos.Realm.Network;
using Mangos.Realm.Network.Handlers;
using Mangos.Realm.Network.Readers;
using Mangos.Realm.Network.Writers;

namespace RealmServer.Modules;

public class RealmModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Router>().AsSelf().SingleInstance();
        builder.RegisterType<RS_LOGON_CHALLENGE_Handler>().AsSelf().SingleInstance();
        builder.RegisterType<RS_LOGON_PROOF_Handler>().AsSelf().SingleInstance();
        builder.RegisterType<RS_REALMLIST_Handler>().AsSelf().SingleInstance();
        builder.RegisterType<CMD_XFER_CANCEL_Handler>().AsSelf().SingleInstance();
        builder.RegisterType<CMD_XFER_ACCEPT_Handler>().AsSelf().SingleInstance();
        builder.RegisterType<On_CMD_XFER_RESUME_Handler>().AsSelf().SingleInstance();

        builder.RegisterType<RS_LOGON_PROOF_Reader>().AsSelf().SingleInstance();
        builder.RegisterType<RS_LOGON_CHALLENGE_Reader>().AsSelf().SingleInstance();
        builder.RegisterType<CMD_XFER_RESUME_Reader>().AsSelf().SingleInstance();
        builder.RegisterType<RS_REALMLIST_Reader>().AsSelf().SingleInstance();

        builder.RegisterType<AUTH_LOGON_PROOF_Writer>().AsSelf().SingleInstance();
        builder.RegisterType<AUTH_LOGON_CHALLENGE_Writer>().AsSelf().SingleInstance();
        builder.RegisterType<AUTH_REALMLIST_Writer>().AsSelf().SingleInstance();

        builder.RegisterType<Startup>().AsSelf().SingleInstance();
    }
}
