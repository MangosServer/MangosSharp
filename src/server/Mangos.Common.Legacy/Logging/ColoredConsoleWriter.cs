//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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
using System.Runtime.CompilerServices;

// Using this logging type, all logs are displayed in console.
// Writting commands is done trought console.

namespace Mangos.Common.Legacy.Logging;

public class ColoredConsoleWriter : BaseWriter
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public override void Write(LogType type, string formatStr, params object[] arg)
    {
        if (LogLevel > type)
        {
            return;
        }

        switch (type)
        {
            case LogType.NETWORK:
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                }

            case LogType.DEBUG:
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                }

            case LogType.INFORMATION:
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                }

            case LogType.USER:
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                }

            case LogType.SUCCESS:
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                }

            case LogType.WARNING:
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                }

            case LogType.FAILED:
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                }

            case LogType.CRITICAL:
                {
                    Console.ForegroundColor = ConsoleColor.Red; // Red
                    break;
                }

            case LogType.DATABASE:
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                }
        }

        if (arg is null)
        {
            Console.Write(formatStr);
        }
        else
        {
            Console.Write(formatStr, arg);
        }

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public override void WriteLine(LogType type, string formatStr, params object[] arg)
    {
        if (LogLevel > type)
        {
            return;
        }

        switch (type)
        {
            case LogType.NETWORK:
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                }

            case LogType.DEBUG:
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                }

            case LogType.INFORMATION:
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                }

            case LogType.USER:
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                }

            case LogType.SUCCESS:
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                }

            case LogType.WARNING:
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                }

            case LogType.FAILED:
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                }

            case LogType.CRITICAL:
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                }

            case LogType.DATABASE:
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                }
        }

        if (arg is null)
        {
            Console.WriteLine($"[{DateTime.Now:hh:mm:ss}] {formatStr}");
        }
        else
        {
            Console.WriteLine($"[{DateTime.Now:hh:mm:ss}] {string.Format(formatStr, arg)}");
        }

        Console.ForegroundColor = ConsoleColor.Gray;
    }
}
