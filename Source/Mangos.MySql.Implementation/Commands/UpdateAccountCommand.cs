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

using Mangos.MySql.Implementation.Connections;
using Mangos.MySql.UpdateAccount;

namespace Mangos.MySql.Implementation.Commands;

internal sealed class UpdateAccountCommand : IUpdateAccountCommand
{
    private readonly AccountConnection accountConnection;

    public UpdateAccountCommand(AccountConnection accountConnection)
    {
        this.accountConnection = accountConnection;
    }

    public async Task ExecuteAsync(string sessionkey, string last_ip, string last_login, string username)
    {
        await accountConnection.ExecuteAsync("UpdateAccountCommand.sql", new
        {
            Sessionkey = sessionkey,
            Last_ip = last_ip,
            Last_login = last_login,
            Username = username
        });
    }
}
