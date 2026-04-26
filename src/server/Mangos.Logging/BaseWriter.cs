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
using Mangos.Common.Enums.Global;

namespace Mangos.Logging;

// Base class for all log writers (console, file, etc.)
// Provides common functionality and defines the interface for derived writers
public class BaseWriter : IDisposable
{
    // Short labels for each log type used in formatted output
    protected static readonly string[] Labels = { "N", "D", "I", "U", "S", "W", "F", "C", "DB", "A", "E", "FN", "NT", "TH", "TR" };

    // Minimum log level to output - logs below this level are filtered out
    public LogType LogLevel { get; set; } = LogType.NETWORK;

    // Track if this instance has been disposed to prevent double disposal
    protected bool _disposedValue;

    // Virtual dispose method for derived classes to override if they need custom cleanup
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        _disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Check if a log type should be output based on current minimum log level
    protected bool IsEnabled(LogType type) => type >= LogLevel;

    // Virtual method to be overridden by derived writers - outputs text without newline
    public virtual void Write(LogType type, string format, params object?[] arg)
    {
    }

    // Virtual method to be overridden by derived writers - outputs text with newline
    public virtual void WriteLine(LogType type, string format, params object?[] arg)
    {
    }

    // Read a line of input from console
    public virtual string ReadLine() => Console.ReadLine() ?? string.Empty;

    // Factory method to create the appropriate writer based on log type string
    public static BaseWriter CreateLog(string logType, string logConfig) => logType?.Trim().ToUpperInvariant() switch
    {
        "COLORCONSOLE" => new ColoredConsoleWriter(),
        "CONSOLE" => new ConsoleWriter(),
        "FILE" => new FileWriter(logConfig),
        _ => throw new ArgumentOutOfRangeException(nameof(logType))
    };
}
