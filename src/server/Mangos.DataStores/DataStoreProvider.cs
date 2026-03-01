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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mangos.DataStores;

public class DataStoreProvider
{
    private readonly string dbcDirectory = "dbc";

    private readonly Dictionary<string, DataStore> dataStores;

    public DataStoreProvider()
    {
        dataStores = new Dictionary<string, DataStore>();
    }

    public async ValueTask<DataStore> GetDataStoreAsync(string dbcFileName)
    {
        if (dataStores.ContainsKey(dbcFileName))
        {
            Console.WriteLine($"[DBC] Cache hit for '{dbcFileName}' ({dataStores.Count} total cached)");
            return dataStores[dbcFileName];
        }

        Console.WriteLine($"[DBC] Cache miss for '{dbcFileName}', loading from disk");
        var path = Path.Combine(dbcDirectory, dbcFileName);
        Console.WriteLine($"[DBC] Full path: {Path.GetFullPath(path)}");
        DataStore dataStore = new();
        await dataStore.LoadFromFileAsync(path);
        dataStores[dbcFileName] = dataStore;
        Console.WriteLine($"[DBC] '{dbcFileName}' cached ({dataStores.Count} total cached DBC files)");
        return dataStore;
    }
}
