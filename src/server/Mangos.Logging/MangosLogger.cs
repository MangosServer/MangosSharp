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

using System.Runtime.CompilerServices;

namespace Mangos.Logging;

internal sealed class MangosLogger : IMangosLogger
{
    private readonly object _lock = new();
    private StreamWriter? _fileWriter;
    private long _totalLogCount;
    private long _errorCount;
    private long _warningCount;

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

    public void Trace(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Trace, message, memberName, filePath, lineNumber);
    public void Trace(Exception exception, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Trace, exception, message, memberName, filePath, lineNumber);
    public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Debug, message, memberName, filePath, lineNumber);
    public void Debug(Exception exception, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Debug, exception, message, memberName, filePath, lineNumber);
    public void Information(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Information, message, memberName, filePath, lineNumber);
    public void Information(Exception exception, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Information, exception, message, memberName, filePath, lineNumber);
    public void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Warning, message, memberName, filePath, lineNumber);
    public void Warning(Exception exception, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Warning, exception, message, memberName, filePath, lineNumber);
    public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Error, message, memberName, filePath, lineNumber);
    public void Error(Exception exception, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Error, exception, message, memberName, filePath, lineNumber);
    public void Critical(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Critical, message, memberName, filePath, lineNumber);
    public void Critical(Exception exception, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogLevel.Critical, exception, message, memberName, filePath, lineNumber);

    public void Log(LogLevel level, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (level < MinimumLevel) return;

        Interlocked.Increment(ref _totalLogCount);
        if (level == LogLevel.Error || level == LogLevel.Critical)
            Interlocked.Increment(ref _errorCount);
        if (level == LogLevel.Warning)
            Interlocked.Increment(ref _warningCount);

        var formatted = FormatMessage(level, message, memberName, filePath, lineNumber);
        lock (_lock)
        {
            Console.ForegroundColor = GetColor(level);
            Console.WriteLine(formatted);
            Console.ResetColor();
            _fileWriter?.WriteLine(formatted);
        }
    }

    public void Log(LogLevel level, Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (level < MinimumLevel) return;

        Interlocked.Increment(ref _totalLogCount);
        if (level == LogLevel.Error || level == LogLevel.Critical)
            Interlocked.Increment(ref _errorCount);
        if (level == LogLevel.Warning)
            Interlocked.Increment(ref _warningCount);

        var formatted = FormatMessage(level, message, memberName, filePath, lineNumber);
        lock (_lock)
        {
            Console.ForegroundColor = GetColor(level);
            Console.WriteLine(formatted);
            Console.WriteLine($"  Exception Type: {exception.GetType().FullName}");
            Console.WriteLine($"  Exception Message: {exception.Message}");
            if (exception.InnerException != null)
            {
                Console.WriteLine($"  Inner Exception: {exception.InnerException.GetType().FullName}: {exception.InnerException.Message}");
            }
            Console.WriteLine(exception.StackTrace);
            Console.ResetColor();
            _fileWriter?.WriteLine(formatted);
            _fileWriter?.WriteLine($"  Exception Type: {exception.GetType().FullName}");
            _fileWriter?.WriteLine($"  Exception Message: {exception.Message}");
            if (exception.InnerException != null)
            {
                _fileWriter?.WriteLine($"  Inner Exception: {exception.InnerException.GetType().FullName}: {exception.InnerException.Message}");
            }
            _fileWriter?.WriteLine(exception.StackTrace);
        }
    }

    public IDisposable BeginTimedOperation(string operationName,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        return new TimedOperation(this, operationName, memberName, filePath, lineNumber);
    }

    private static string FormatMessage(LogLevel level, string message, string memberName, string filePath, int lineNumber)
    {
        var fileName = Path.GetFileName(filePath);
        var threadId = Environment.CurrentManagedThreadId;
        return $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} [{level,-11}] [Thread:{threadId:D3}] [{fileName}:{memberName}:{lineNumber}] {message}";
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
