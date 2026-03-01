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

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mangos.Logging;

public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}

public interface IMangosLogger
{
    LogLevel MinimumLevel { get; set; }

    long TotalLogCount { get; }
    long ErrorCount { get; }
    long WarningCount { get; }

    void Trace(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Trace(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Debug(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Debug(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Information(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Information(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Warning(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Warning(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Error(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Error(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Critical(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Critical(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Log(LogLevel level, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    void Log(LogLevel level, Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    IDisposable BeginTimedOperation(string operationName,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    IDisposable BeginScope(string scopeName,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);
}

public sealed class TimedOperation : IDisposable
{
    private readonly IMangosLogger _logger;
    private readonly string _operationName;
    private readonly string _memberName;
    private readonly string _filePath;
    private readonly int _lineNumber;
    private readonly Stopwatch _stopwatch;

    public TimedOperation(IMangosLogger logger, string operationName,
        string memberName, string filePath, int lineNumber)
    {
        _logger = logger;
        _operationName = operationName;
        _memberName = memberName;
        _filePath = filePath;
        _lineNumber = lineNumber;
        _stopwatch = Stopwatch.StartNew();
        _logger.Trace($"[PERF] Starting: {_operationName}", _memberName, _filePath, _lineNumber);
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _logger.Trace($"[PERF] Completed: {_operationName} in {_stopwatch.ElapsedMilliseconds}ms", _memberName, _filePath, _lineNumber);
    }
}

public sealed class LogScope : IDisposable
{
    private readonly IMangosLogger _logger;
    private readonly string _scopeName;
    private readonly string _memberName;
    private readonly string _filePath;
    private readonly int _lineNumber;
    private readonly Stopwatch _stopwatch;

    public LogScope(IMangosLogger logger, string scopeName,
        string memberName, string filePath, int lineNumber)
    {
        _logger = logger;
        _scopeName = scopeName;
        _memberName = memberName;
        _filePath = filePath;
        _lineNumber = lineNumber;
        _stopwatch = Stopwatch.StartNew();
        _logger.Trace($"[SCOPE:ENTER] {_scopeName}", _memberName, _filePath, _lineNumber);
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _logger.Trace($"[SCOPE:EXIT] {_scopeName} ({_stopwatch.ElapsedMilliseconds}ms)", _memberName, _filePath, _lineNumber);
    }
}
