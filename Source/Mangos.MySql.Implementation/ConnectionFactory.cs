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

using Mangos.Configuration;
using Mangos.MySql.Implementation.Connections;
using MySql.Data.MySqlClient;
using System.Reflection;

namespace Mangos.MySql.Implementation;

internal sealed class ConnectionFactory
{
    private readonly MangosConfiguration mangosConfiguration;
    private readonly Dictionary<string, string> scripts;

    public ConnectionFactory(MangosConfiguration mangosConfiguration)
    {
        this.mangosConfiguration = mangosConfiguration;
        scripts = GetScripts();
    }

    public AccountConnection ConnectToAccountDataBase()
    {
        var mySqlConnection = new MySqlConnection(mangosConfiguration.AccountDataBaseConnectionString);
        mySqlConnection.Open();
        return new AccountConnection(mySqlConnection, scripts);
    }

    private Dictionary<string, string> GetScripts()
    {
        var assembly = GetType().Assembly;
        var names = assembly.GetManifestResourceNames().Where(x => x.EndsWith(".sql"));
        return names.ToDictionary(x => x.Split("Mangos.MySql.Implementation.Scripts.")[1], x => GetResource(assembly, x));
    }

    private string GetResource(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException("Unable to read sql script");
        }
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }
}
