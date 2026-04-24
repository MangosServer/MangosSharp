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

public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    Network = 6,
    User = 7,
    Success = 8,
    Failed = 9,
    Database = 10,
    Alert = 11,
    Emerg = 12,
    Func = 13,
    Notice = 14,
    Thread = 15
}

public interface IMangosLogger
{
    LogLevel MinimumLevel { get; set; }

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
