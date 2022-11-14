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
using Mangos.MySql.GetAccountInfo;
using Mangos.MySql.GetRealmList;
using Mangos.MySql.Implementation.Commands;
using Mangos.MySql.Implementation.Queries;
using Mangos.MySql.IsBannedAccount;
using Mangos.MySql.UpdateAccount;

namespace Mangos.MySql.Implementation;

public sealed class MySqlModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ConnectionFactory>().SingleInstance();
        builder.Register(context => context.Resolve<ConnectionFactory>().ConnectToAccountDataBase()).SingleInstance();

        RegisterQueries(builder);
    }

    private void RegisterQueries(ContainerBuilder builder)
    {
        builder.RegisterType<GetAccountInfoQuery>().As<IGetAccountInfoQuery>().SingleInstance();
        builder.RegisterType<IsBannedAccountQuery>().As<IIsBannedAccountQuery>().SingleInstance();
        builder.RegisterType<UpdateAccountCommand>().As<IUpdateAccountCommand>().SingleInstance();
        builder.RegisterType<GetRealmListQuery>().As<IGetRealmListQuery>().SingleInstance();
    }
}
