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
    public string[] L = { "N", "D", "I", "U", "S", "W", "F", "C", "DB" };
    public LogType LogLevel = LogType.NETWORK;

    private bool _disposedValue; // To detect redundant calls

    // IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            // TODO: set large fields to null.
        }

        _disposedValue = true;
    }

    // This code added by Visual Basic to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Write(LogType type, string format, params object[] arg)
    {
    }

    public virtual void WriteLine(LogType type, string format, params object[] arg)
    {
    }

    public virtual string ReadLine()
    {
        return Console.ReadLine();
    }

    public void PrintDiagnosticTest()
    {
        WriteLine(LogType.NETWORK, "{0}:************************* TEST *************************", (object)1);
        WriteLine(LogType.DEBUG, "{0}:************************* TEST *************************", (object)1);
        WriteLine(LogType.INFORMATION, "{0}:************************* TEST *************************", (object)1);
        WriteLine(LogType.USER, "{0}:************************* TEST *************************", (object)1);
        WriteLine(LogType.SUCCESS, "{0}:************************* TEST *************************", (object)1);
        WriteLine(LogType.WARNING, "{0}:************************* TEST *************************", (object)1);
        WriteLine(LogType.FAILED, "{0}:************************* TEST *************************", (object)1);
        WriteLine(LogType.CRITICAL, "{0}:************************* TEST *************************", (object)1);
        WriteLine(LogType.DATABASE, "{0}:************************* TEST *************************", (object)1);
    }

    public static BaseWriter CreateLog(string logType, string logConfig)
    {
        switch (logType.ToUpper() ?? "")
        {
            case "COLORCONSOLE":
                {
                    return new ColoredConsoleWriter();
                }

            case "CONSOLE":
                {
                    return new ConsoleWriter();
                }

            case "FILE":
                {
                    return new FileWriter(logConfig);
                }

            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(logType));
                }
        }
    }
}
