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
using System.Text.Json.Nodes;

namespace Mangos.Configuration;

internal sealed class ConfigurationLoader
{
    private const string ConfigurationFileName = "configuration.json";
    private const string EnvironmentVariablePrefix = "MANGOS_";

    public MangosConfiguration GetMangosConfiguration()
    {
        var configPath = Environment.GetEnvironmentVariable($"{EnvironmentVariablePrefix}CONFIG_PATH")
                         ?? ConfigurationFileName;

        var json = ReadConfiguration(configPath);
        var jsonNode = JsonNode.Parse(json);
        if (jsonNode == null)
        {
            throw new InvalidOperationException($"Unable to parse {configPath} as JSON");
        }

        ApplyEnvironmentOverrides(jsonNode);

        var mangosConfiguration = jsonNode.Deserialize<MangosConfiguration>();
        if (mangosConfiguration == null)
        {
            throw new InvalidOperationException($"Unable to deserialize {configPath}");
        }

        Validate(mangosConfiguration);

        return mangosConfiguration;
    }

    private static string ReadConfiguration(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Unable to locate configuration file: {path}", path);
        }
        return File.ReadAllText(path);
    }

    private static void ApplyEnvironmentOverrides(JsonNode root)
    {
        OverrideString(root, "AccountDataBaseConnectionString", $"{EnvironmentVariablePrefix}ACCOUNT_DB");
        OverrideString(root, "Realm", "RealmServerEndpoint", $"{EnvironmentVariablePrefix}REALM_ENDPOINT");
        OverrideString(root, "Cluster", "ClusterServerEndpoint", $"{EnvironmentVariablePrefix}CLUSTER_ENDPOINT");
        OverrideString(root, "Cluster", "ClusterListenAddress", $"{EnvironmentVariablePrefix}CLUSTER_LISTEN_ADDRESS");
        OverrideString(root, "World", "ClusterConnectHost", $"{EnvironmentVariablePrefix}WORLD_CLUSTER_HOST");
    }

    private static void OverrideString(JsonNode root, string key, string envVar)
    {
        var value = Environment.GetEnvironmentVariable(envVar);
        if (value != null)
        {
            root[key] = value;
        }
    }

    private static void OverrideString(JsonNode root, string section, string key, string envVar)
    {
        var value = Environment.GetEnvironmentVariable(envVar);
        if (value != null && root[section] is JsonNode sectionNode)
        {
            sectionNode[key] = value;
        }
    }

    private static void Validate(MangosConfiguration config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.AccountDataBaseConnectionString))
            errors.Add("AccountDataBaseConnectionString is required");

        if (string.IsNullOrWhiteSpace(config.Realm.RealmServerEndpoint))
            errors.Add("Realm.RealmServerEndpoint is required");

        if (string.IsNullOrWhiteSpace(config.Cluster.ClusterServerEndpoint))
            errors.Add("Cluster.ClusterServerEndpoint is required");

        if (config.Cluster.ClusterListenPort <= 0 || config.Cluster.ClusterListenPort > 65535)
            errors.Add($"Cluster.ClusterListenPort must be between 1 and 65535, got {config.Cluster.ClusterListenPort}");

        if (config.Cluster.ServerPlayerLimit < 0)
            errors.Add("Cluster.ServerPlayerLimit cannot be negative");

        if (config.World.ClusterConnectPort <= 0 || config.World.ClusterConnectPort > 65535)
            errors.Add($"World.ClusterConnectPort must be between 1 and 65535, got {config.World.ClusterConnectPort}");

        if (config.World.XPRate < 0)
            errors.Add("World.XPRate cannot be negative");

        if (config.World.MapResolution < 64 || config.World.MapResolution > 256)
            errors.Add($"World.MapResolution must be between 64 and 256, got {config.World.MapResolution}");

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Configuration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}
