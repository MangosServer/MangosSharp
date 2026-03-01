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
        var configPathOverride = Environment.GetEnvironmentVariable($"{EnvironmentVariablePrefix}CONFIG_PATH");
        var configPath = configPathOverride ?? ConfigurationFileName;

        Console.WriteLine($"[Config] Loading configuration from: {configPath}{(configPathOverride != null ? " (overridden by MANGOS_CONFIG_PATH)" : " (default)")}");
        Console.WriteLine($"[Config] Working directory: {Environment.CurrentDirectory}");

        var json = ReadConfiguration(configPath);
        Console.WriteLine($"[Config] Configuration file read successfully ({json.Length} chars)");

        var jsonNode = JsonNode.Parse(json);
        if (jsonNode == null)
        {
            Console.WriteLine($"[Config] ERROR: Failed to parse {configPath} as JSON");
            throw new InvalidOperationException($"Unable to parse {configPath} as JSON");
        }
        Console.WriteLine("[Config] JSON parsed successfully");

        Console.WriteLine("[Config] Checking for environment variable overrides");
        ApplyEnvironmentOverrides(jsonNode);

        Console.WriteLine("[Config] Deserializing configuration");
        var mangosConfiguration = jsonNode.Deserialize<MangosConfiguration>();
        if (mangosConfiguration == null)
        {
            Console.WriteLine($"[Config] ERROR: Failed to deserialize {configPath}");
            throw new InvalidOperationException($"Unable to deserialize {configPath}");
        }
        Console.WriteLine("[Config] Configuration deserialized successfully");

        Console.WriteLine("[Config] Validating configuration");
        Validate(mangosConfiguration);
        Console.WriteLine("[Config] Configuration validation passed");

        Console.WriteLine($"[Config] Realm endpoint: {mangosConfiguration.Realm.RealmServerEndpoint}");
        Console.WriteLine($"[Config] Cluster endpoint: {mangosConfiguration.Cluster.ClusterServerEndpoint}");
        Console.WriteLine($"[Config] Cluster listen: {mangosConfiguration.Cluster.ClusterListenAddress}:{mangosConfiguration.Cluster.ClusterListenPort}");
        Console.WriteLine($"[Config] World cluster host: {mangosConfiguration.World.ClusterConnectHost}:{mangosConfiguration.World.ClusterConnectPort}");

        return mangosConfiguration;
    }

    private static string ReadConfiguration(string path)
    {
        Console.WriteLine($"[Config] Checking file existence: {path}");
        if (!File.Exists(path))
        {
            Console.WriteLine($"[Config] ERROR: Configuration file not found: {Path.GetFullPath(path)}");
            throw new FileNotFoundException($"Unable to locate configuration file: {path}", path);
        }
        Console.WriteLine($"[Config] Reading file: {Path.GetFullPath(path)}");
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
            Console.WriteLine($"[Config] Environment override applied: {envVar} -> {key}");
            root[key] = value;
        }
    }

    private static void OverrideString(JsonNode root, string section, string key, string envVar)
    {
        var value = Environment.GetEnvironmentVariable(envVar);
        if (value != null && root[section] is JsonNode sectionNode)
        {
            Console.WriteLine($"[Config] Environment override applied: {envVar} -> {section}.{key}");
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
