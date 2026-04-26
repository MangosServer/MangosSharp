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

// Defines logging severity levels from least to most severe
// Used to filter which log messages are output based on the logger's MinimumLevel
public enum LogLevel
{
    // Standard severity levels
    Trace = 0,          // Detailed diagnostic information
    Debug = 1,          // Debug-level messages
    Information = 2,    // Informational messages
    Warning = 3,        // Warning messages
    Error = 4,          // Error messages
    Critical = 5,       // Critical errors
    
    // Application-specific log types
    Network = 6,        // Network-related messages
    User = 7,           // User action messages
    Success = 8,        // Operation success messages
    Failed = 9,         // Operation failure messages
    Database = 10,      // Database operation messages
    Alert = 11,         // Alert conditions
    Emerg = 12,         // Emergency conditions
    Func = 13,          // Function call tracking
    Notice = 14,        // Notices
    Thread = 15         // Thread-related messages
}

// Main logging interface for the application
// Provides methods for logging messages at different severity levels
// Both with and without exception handling
public interface IMangosLogger
{
    LogLevel MinimumLevel
    {
        get; set;
    }

    void Trace(string message);
    void Trace(Exception exception, string message);

    void Debug(string message);
    void Debug(Exception exception, string message);

    void Information(string message);
    void Information(Exception exception, string message);

    void Warning(string message);
    void Warning(Exception exception, string message);

    void Error(string message);
    void Error(Exception exception, string message);

    void Critical(string message);
    void Critical(Exception exception, string message);

    void Network(string message);
    void Network(Exception exception, string message);

    void User(string message);
    void User(Exception exception, string message);

    void Success(string message);
    void Success(Exception exception, string message);

    void Failed(string message);
    void Failed(Exception exception, string message);

    void Database(string message);
    void Database(Exception exception, string message);

    void Alert(string message);
    void Alert(Exception exception, string message);

    void Emerg(string message);
    void Emerg(Exception exception, string message);

    void Func(string message);
    void Func(Exception exception, string message);

    void Notice(string message);
    void Notice(Exception exception, string message);

    void Thread(string message);
    void Thread(Exception exception, string message);

    void Log(LogLevel level, string message);
    void Log(LogLevel level, Exception exception, string message);
}
