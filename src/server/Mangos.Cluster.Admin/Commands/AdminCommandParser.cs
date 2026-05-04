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

using System;
using System.Collections.Generic;

namespace Mangos.Cluster.Admin.Commands;

/// <summary>
/// Parser for the unified admin command syntax used by in-game chat,
/// the cluster console REPL, and the external CLI.
///
/// Examples:
///   .server list
///   .server info --world W1
///   .server shutdown --world W1 --grace 30
///   .instance spawn --map 530 --realm 2
///   .realm list
///
/// Leading dot is optional. Tokens are space-separated; flag values are
/// the next token after a "--key" prefix. Unknown flags go into
/// <see cref="AdminCommand.Extras"/>.
/// </summary>
public static class AdminCommandParser
{
    public static bool TryParse(string input, out AdminCommand? command, out string? error)
    {
        command = null;
        error = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            error = "empty command";
            return false;
        }

        var trimmed = input.TrimStart('.', ' ');
        var tokens = Tokenize(trimmed);
        if (tokens.Count < 2)
        {
            error = "expected at least <noun> <verb>";
            return false;
        }

        var noun = tokens[0].ToLowerInvariant();
        var verb = tokens[1].ToLowerInvariant();
        var av = ResolveVerb(noun, verb);
        if (av == AdminVerb.Unknown)
        {
            error = $"unknown command: {noun} {verb}";
            return false;
        }

        var flags = new Dictionary<string, string>();
        for (int i = 2; i < tokens.Count; i++)
        {
            var t = tokens[i];
            if (t.StartsWith("--", StringComparison.Ordinal))
            {
                var k = t[2..];
                var v = i + 1 < tokens.Count ? tokens[++i] : "true";
                flags[k] = v;
            }
            else
            {
                flags[$"_pos{i}"] = t;
            }
        }

        command = new AdminCommand
        {
            Verb = av,
            TargetRealmId = flags.TryGetValue("realm", out var r) && uint.TryParse(r, out var ri) ? ri : 0u,
            WorldId = flags.TryGetValue("world", out var w) ? w : null,
            InstanceId = flags.TryGetValue("instance", out var inst) && uint.TryParse(inst, out var instv) ? instv : 0u,
            MapId = flags.TryGetValue("map", out var m) && uint.TryParse(m, out var mv) ? mv : 0u,
            GraceSeconds = flags.TryGetValue("grace", out var g) && int.TryParse(g, out var gv) ? gv : 0,
            Extras = flags,
        };
        return true;
    }

    private static AdminVerb ResolveVerb(string noun, string verb) => (noun, verb) switch
    {
        ("server", "list") => AdminVerb.ServerList,
        ("server", "info") => AdminVerb.ServerInfo,
        ("server", "shutdown") => AdminVerb.ServerShutdown,
        ("server", "restart") => AdminVerb.ServerRestart,
        ("server", "start") => AdminVerb.ServerStart,
        ("server", "claim") => AdminVerb.ServerClaimMaps,
        ("instance", "list") => AdminVerb.InstanceList,
        ("instance", "info") => AdminVerb.InstanceInfo,
        ("instance", "spawn") => AdminVerb.InstanceSpawn,
        ("instance", "shutdown") => AdminVerb.InstanceShutdown,
        ("instance", "restart") => AdminVerb.InstanceRestart,
        ("instance", "kick") => AdminVerb.InstanceKick,
        ("realm", "list") => AdminVerb.RealmList,
        ("realm", "peers") => AdminVerb.RealmPeers,
        ("realm", "marker") => AdminVerb.RealmMarkerShow, // disambiguated by Extras["_pos2"]
        ("realm", "show") => AdminVerb.RealmMarkerShow,
        ("realm", "hide") => AdminVerb.RealmMarkerHide,
        _ => AdminVerb.Unknown,
    };

    private static List<string> Tokenize(string s)
    {
        // Simple whitespace split with quoted-string support.
        var result = new List<string>();
        var cur = new System.Text.StringBuilder();
        bool inQuotes = false;
        foreach (var c in s)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (!inQuotes && char.IsWhiteSpace(c))
            {
                if (cur.Length > 0) { result.Add(cur.ToString()); cur.Clear(); }
                continue;
            }
            cur.Append(c);
        }
        if (cur.Length > 0) result.Add(cur.ToString());
        return result;
    }
}
