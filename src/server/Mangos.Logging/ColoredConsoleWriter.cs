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

public class ColoredConsoleWriter : BaseWriter
{
    private static readonly object s_consoleLock = new();

    public override void Write(LogType type, string formatStr, params object?[] arg)
    {
        if (!IsEnabled(type))
        {
            return;
        }

        PerformWrite(type, () => Console.Write(formatStr, arg));
    }

    public override void WriteLine(LogType type, string formatStr, params object?[] arg)
    {
        if (!IsEnabled(type))
        {
            return;
        }

        var message = string.Format(formatStr, arg);
        PerformWrite(type, () => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}"));
    }

    private static void PerformWrite(LogType type, Action writeAction)
    {
        lock (s_consoleLock)
        {
            Console.ForegroundColor = type switch
            {
                LogType.NETWORK => ConsoleColor.DarkGray,
                LogType.DEBUG => ConsoleColor.Gray,
                LogType.INFORMATION => ConsoleColor.White,
                LogType.USER => ConsoleColor.Blue,
                LogType.SUCCESS => ConsoleColor.DarkGreen,
                LogType.WARNING => ConsoleColor.Yellow,
                LogType.FAILED => ConsoleColor.Cyan,
                LogType.CRITICAL => ConsoleColor.Red,
                LogType.DATABASE => ConsoleColor.DarkMagenta,
                LogType.ALERT => ConsoleColor.Red,
                LogType.EMERG => ConsoleColor.DarkRed,
                LogType.FUNC => ConsoleColor.Gray,
                LogType.NOTICE => ConsoleColor.White,
                LogType.THREAD => ConsoleColor.Cyan,
                LogType.TRACE => ConsoleColor.DarkGray,
                _ => ConsoleColor.Gray,
            };

            try
            {
                writeAction();
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
