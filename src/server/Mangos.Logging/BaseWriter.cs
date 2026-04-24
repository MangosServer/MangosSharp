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

using Mangos.Common.Enums.Global;
using System;

namespace Mangos.Logging;

public class BaseWriter : IDisposable
{
    protected static readonly string[] Labels = { "N", "D", "I", "U", "S", "W", "F", "C", "DB", "A", "E", "FN", "NT", "TH", "TR" };

    public LogType LogLevel { get; set; } = LogType.NETWORK;

    protected bool _disposedValue;

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

    protected bool IsEnabled(LogType type) => type >= LogLevel;

    public virtual void Write(LogType type, string format, params object?[] arg)
    {
    }

    public virtual void WriteLine(LogType type, string format, params object?[] arg)
    {
    }

    public virtual string ReadLine() => Console.ReadLine() ?? string.Empty;

    public void PrintDiagnosticTest()
    {
        foreach (var type in Enum.GetValues<LogType>())
        {
            WriteLine(type, "{0}:************************* TEST *************************", 1);
        }
    }

    public static BaseWriter CreateLog(string logType, string logConfig) =>
        logType?.Trim().ToUpperInvariant() switch
        {
            "COLORCONSOLE" => new ColoredConsoleWriter(),
            "CONSOLE" => new ConsoleWriter(),
            "FILE" => new FileWriter(logConfig),
            _ => throw new ArgumentOutOfRangeException(nameof(logType))
        };
}
