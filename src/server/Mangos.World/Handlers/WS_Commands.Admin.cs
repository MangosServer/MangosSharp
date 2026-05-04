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
using Autofac;
using Mangos.Cluster.Admin.Commands;
using Mangos.Common.Enums.Misc;
using Mangos.MySql.UpdateFederationMarker;
using Mangos.World.Player;

namespace Mangos.World.Handlers;

/// <summary>
/// In-game GM commands for cluster/world supervision and federation.
///
/// .server   - manage worlds (list/info/start/shutdown/restart)
/// .instance - manage instances (list/spawn/shutdown/restart)
/// .realm    - cross-realm queries (list/peers)
///
/// All three forward an AdminCommand through the cluster's IPC channel
/// (ICluster.RunAdminCommand) which routes locally or - if --realm N
/// is set - to a peer cluster over the federation transport.
/// </summary>
public partial class WS_Commands
{
    [ChatCommand("server", "server <list|info|shutdown|restart|start|claim> [--world <id>] [--grace <s>] [--realm <id>] - Manage worlds.", AccessLevel.Admin)]
    public bool cmdAdminServer(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        => DispatchAdmin(objCharacter, "server " + Message);

    [ChatCommand("instance", "instance <list|info|spawn|shutdown|restart|kick> [--map <id>] [--instance <id>] [--realm <id>] - Manage instances.", AccessLevel.Admin)]
    public bool cmdAdminInstance(ref WS_PlayerData.CharacterObject objCharacter, string Message)
        => DispatchAdmin(objCharacter, "instance " + Message);

    /// <summary>
    /// .realm is dual-purpose: list/peers are admin-level, while show/hide
    /// flips the calling player's own federation marker preference and is
    /// available to all players. The dispatcher routes by verb.
    /// </summary>
    [ChatCommand("realm", "realm <list|peers|show|hide> - Cross-realm queries; show/hide toggles your federation marker preference.", AccessLevel.Player)]
    public bool cmdAdminRealm(ref WS_PlayerData.CharacterObject objCharacter, string Message)
    {
        var trimmed = (Message ?? string.Empty).Trim();
        var firstWord = trimmed.Split(' ', 2)[0].ToLowerInvariant();

        // show/hide are per-account preference flips handled locally so we
        // can write the calling player's account row without round-tripping.
        if (firstWord == "show" || firstWord == "hide")
        {
            return SetMarkerPreference(objCharacter, show: firstWord == "show");
        }

        if (objCharacter.Access < AccessLevel.Admin)
        {
            objCharacter.CommandResponse("This subcommand requires Admin access.");
            return true;
        }
        return DispatchAdmin(objCharacter, "realm " + Message);
    }

    private bool SetMarkerPreference(WS_PlayerData.CharacterObject character, bool show)
    {
        var account = character.client?.Account;
        if (string.IsNullOrEmpty(account))
        {
            character.CommandResponse("could not resolve your account name");
            return true;
        }
        try
        {
            var cmd = WorldServiceLocator.Container?.Resolve<IUpdateFederationMarkerCommand>();
            if (cmd is null)
            {
                character.CommandResponse("federation marker command not available");
                return true;
            }
            cmd.ExecuteAsync(account, show).GetAwaiter().GetResult();
            character.CommandResponse(show
                ? "Cross-realm markers will now be shown for your account."
                : "Cross-realm markers will be hidden for your account (whispers always carry the tag).");
        }
        catch (Exception ex)
        {
            character.CommandResponse($"failed to persist preference: {ex.Message}");
        }
        return true;
    }

    private bool DispatchAdmin(WS_PlayerData.CharacterObject character, string commandLine)
    {
        if (!AdminCommandParser.TryParse(commandLine, out var cmd, out var err) || cmd is null)
        {
            character.CommandResponse($"parse error: {err}");
            return true;
        }

        var cluster = WorldServiceLocator.WorldServer.ClsWorldServer.Cluster;
        if (cluster is null)
        {
            character.CommandResponse("cluster not available");
            return true;
        }

        try
        {
            var bytes = cluster.RunAdminCommand(cmd.Serialize());
            var reply = AdminCommandReply.Deserialize(bytes);
            character.CommandResponse($"[{reply.Status}]");
            foreach (var line in reply.Lines)
                character.CommandResponse(line);
        }
        catch (Exception ex)
        {
            character.CommandResponse($"admin call failed: {ex.Message}");
        }
        return true;
    }
}
