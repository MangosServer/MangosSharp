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
using System.Threading;
using System.Threading.Tasks;

namespace Mangos.Cluster.Admin.Commands;

/// <summary>
/// Tiny stdin-driven REPL for the cluster console. Reads lines, runs
/// them through <see cref="AdminCommandParser"/>, and dispatches to the
/// local <see cref="IAdminCommandHandler"/>. Same syntax used by the
/// in-game GM commands and the external CLI tool, so an operator can
/// exercise everything from any of three entrypoints.
/// </summary>
public sealed class ConsoleAdminRepl
{
    private readonly IAdminCommandHandler _handler;
    private readonly Action<string> _writeLine;

    public ConsoleAdminRepl(IAdminCommandHandler handler, Action<string>? writeLine = null)
    {
        _handler = handler;
        _writeLine = writeLine ?? Console.WriteLine;
    }

    public Task RunAsync(CancellationToken ct = default)
    {
        return Task.Run(async () =>
        {
            _writeLine("Cluster console ready. Type 'help' for commands.");
            while (!ct.IsCancellationRequested)
            {
                string? line;
                try { line = Console.ReadLine(); }
                catch { return; }
                if (line is null) return; // stdin closed
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.Trim().Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    PrintHelp();
                    continue;
                }
                if (line.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase) ||
                    line.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!AdminCommandParser.TryParse(line, out var cmd, out var err) || cmd is null)
                {
                    _writeLine($"parse error: {err}");
                    continue;
                }

                var reply = await _handler.ExecuteAsync(cmd, ct);
                _writeLine($"[{reply.Status}]");
                foreach (var l in reply.Lines)
                    _writeLine(l);
            }
        }, ct);
    }

    private void PrintHelp()
    {
        _writeLine("Commands (leading '.' optional):");
        _writeLine("  .server list");
        _writeLine("  .server info --world <id>");
        _writeLine("  .server shutdown --world <id> [--grace <s>]");
        _writeLine("  .server restart --world <id> [--grace <s>]");
        _writeLine("  .server start --world <id>");
        _writeLine("  .instance list [--map <id>] [--realm <id>]");
        _writeLine("  .instance spawn --map <id> [--realm <id>]");
        _writeLine("  .instance shutdown --instance <id> [--realm <id>]");
        _writeLine("  .instance restart --instance <id> [--realm <id>]");
        _writeLine("  .realm list");
        _writeLine("Add --realm <id> to target a peer cluster.");
    }
}
