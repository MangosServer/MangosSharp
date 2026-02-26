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

namespace Mangos.Logging;

internal sealed class MangosLogger : IMangosLogger
{
    private readonly object _lock = new();
    private StreamWriter? _fileWriter;

    public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;

    public string? LogFilePath
    {
        set
        {
            lock (_lock)
            {
                _fileWriter?.Dispose();
                _fileWriter = value != null
                    ? new StreamWriter(new FileStream(value, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true }
                    : null;
            }
        }
    }

    public void Trace(string message) => Log(LogLevel.Trace, message);
    public void Trace(Exception exception, string message) => Log(LogLevel.Trace, exception, message);
    public void Debug(string message) => Log(LogLevel.Debug, message);
    public void Debug(Exception exception, string message) => Log(LogLevel.Debug, exception, message);
    public void Information(string message) => Log(LogLevel.Information, message);
    public void Information(Exception exception, string message) => Log(LogLevel.Information, exception, message);
    public void Warning(string message) => Log(LogLevel.Warning, message);
    public void Warning(Exception exception, string message) => Log(LogLevel.Warning, exception, message);
    public void Error(string message) => Log(LogLevel.Error, message);
    public void Error(Exception exception, string message) => Log(LogLevel.Error, exception, message);
    public void Critical(string message) => Log(LogLevel.Critical, message);
    public void Critical(Exception exception, string message) => Log(LogLevel.Critical, exception, message);

    public void Log(LogLevel level, string message)
    {
        if (level < MinimumLevel) return;

        var formatted = FormatMessage(level, message);
        lock (_lock)
        {
            Console.ForegroundColor = GetColor(level);
            Console.WriteLine(formatted);
            Console.ResetColor();
            _fileWriter?.WriteLine(formatted);
        }
    }

    public void Log(LogLevel level, Exception exception, string message)
    {
        if (level < MinimumLevel) return;

        var formatted = FormatMessage(level, message);
        lock (_lock)
        {
            Console.ForegroundColor = GetColor(level);
            Console.WriteLine(formatted);
            Console.WriteLine(exception);
            Console.ResetColor();
            _fileWriter?.WriteLine(formatted);
            _fileWriter?.WriteLine(exception.ToString());
        }
    }

    private static string FormatMessage(LogLevel level, string message)
    {
        return $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} [{level,-11}] {message}";
    }

    private static ConsoleColor GetColor(LogLevel level) => level switch
    {
        LogLevel.Trace => ConsoleColor.Gray,
        LogLevel.Debug => ConsoleColor.DarkGray,
        LogLevel.Information => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.DarkRed,
        _ => ConsoleColor.White
    };
}
