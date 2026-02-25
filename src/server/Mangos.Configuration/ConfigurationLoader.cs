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

using System.Text.Json;

namespace Mangos.Configuration;

internal sealed class ConfigurationLoader
{
    private const string ConfigurationFileName = "configuration.json";

    public MangosConfiguration GetMangosConfiguration()
    {
        var configuration = ReadConfigurationAsync();
        var mangosConfiguration = JsonSerializer.Deserialize<MangosConfiguration>(configuration);
        if (mangosConfiguration == null)
        {
            throw new Exception($"Unable to deserialzie {ConfigurationFileName}");
        }
        return mangosConfiguration;
    }

    private string ReadConfigurationAsync()
    {
        if (!File.Exists(ConfigurationFileName))
        {
            throw new Exception($"Unable to locate {ConfigurationFileName}");
        }
        return File.ReadAllText(ConfigurationFileName);
    }
}
