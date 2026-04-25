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
using System.IO;

namespace Mangos.Logging;

// Main logger implementation that handles all logging operations
// Outputs to both console (with color) and optional file simultaneously
// Thread-safe with lock protection for concurrent access
internal sealed class MangosLogger : IMangosLogger, IDisposable
{
    // Lock to synchronize console and file output across threads
    private readonly object _lock = new();
    // Optional file writer for logging to disk
    private StreamWriter? _fileWriter;

    // Minimum severity level - messages below this are ignored
    public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;

    // Property setter to change the log file path and open/close file writers
    public string? LogFilePath
    {
        set => SetLogFilePath(value);
    }

    public void Dispose() => _fileWriter?.Dispose();

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

    public void Network(string message) => Log(LogLevel.Network, message);
    public void Network(Exception exception, string message) => Log(LogLevel.Network, exception, message);

    public void User(string message) => Log(LogLevel.User, message);
    public void User(Exception exception, string message) => Log(LogLevel.User, exception, message);

    public void Success(string message) => Log(LogLevel.Success, message);
    public void Success(Exception exception, string message) => Log(LogLevel.Success, exception, message);

    public void Failed(string message) => Log(LogLevel.Failed, message);
    public void Failed(Exception exception, string message) => Log(LogLevel.Failed, exception, message);

    public void Database(string message) => Log(LogLevel.Database, message);
    public void Database(Exception exception, string message) => Log(LogLevel.Database, exception, message);

    public void Alert(string message) => Log(LogLevel.Alert, message);
    public void Alert(Exception exception, string message) => Log(LogLevel.Alert, exception, message);

    public void Emerg(string message) => Log(LogLevel.Emerg, message);
    public void Emerg(Exception exception, string message) => Log(LogLevel.Emerg, exception, message);

    public void Func(string message) => Log(LogLevel.Func, message);
    public void Func(Exception exception, string message) => Log(LogLevel.Func, exception, message);

    public void Notice(string message) => Log(LogLevel.Notice, message);
    public void Notice(Exception exception, string message) => Log(LogLevel.Notice, exception, message);

    public void Thread(string message) => Log(LogLevel.Thread, message);
    public void Thread(Exception exception, string message) => Log(LogLevel.Thread, exception, message);

    // Core logging method - filters by level, formats message, and outputs to console and file
    public void Log(LogLevel level, string message)
    {
        if (level < MinimumLevel)
        {
            return;
        }

        var formatted = FormatMessage(level, message);

        lock (_lock)
        {
            Console.ForegroundColor = GetColor(level);
            try
            {
                Console.WriteLine(formatted);
                _fileWriter?.WriteLine(formatted);
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }

    // Core logging method with exception - includes exception details in output
    public void Log(LogLevel level, Exception exception, string message)
    {
        if (level < MinimumLevel)
        {
            return;
        }

        var formatted = FormatMessage(level, message);
        var exceptionText = exception.ToString();

        lock (_lock)
        {
            Console.ForegroundColor = GetColor(level);
            try
            {
                Console.WriteLine(formatted);
                Console.WriteLine(exceptionText);
                _fileWriter?.WriteLine(formatted);
                _fileWriter?.WriteLine(exceptionText);
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }

    // Formats the log message with timestamp and log level
    private static string FormatMessage(LogLevel level, string message) =>
        $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} [{level,-11}] {message}";

    // Maps each log level to its corresponding console color for visual distinction
    private static ConsoleColor GetColor(LogLevel level) => level switch
    {
        LogLevel.Trace => ConsoleColor.Gray,
        LogLevel.Debug => ConsoleColor.DarkGray,
        LogLevel.Information => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.DarkRed,
        LogLevel.Network => ConsoleColor.DarkGray,
        LogLevel.User => ConsoleColor.White,
        LogLevel.Success => ConsoleColor.White,
        LogLevel.Failed => ConsoleColor.Red,
        LogLevel.Database => ConsoleColor.White,
        LogLevel.Alert => ConsoleColor.Red,
        LogLevel.Emerg => ConsoleColor.DarkRed,
        LogLevel.Func => ConsoleColor.Gray,
        LogLevel.Notice => ConsoleColor.White,
        LogLevel.Thread => ConsoleColor.Gray,
        _ => ConsoleColor.White
    };

    // Sets or changes the file writer - thread-safe, disposes old writer before creating new one
    private void SetLogFilePath(string? path)
    {
        lock (_lock)
        {
            _fileWriter?.Dispose();
            _fileWriter = path is not null
                ? new StreamWriter(path, append: true) { AutoFlush = true }
                : null;
        }
    }
}
