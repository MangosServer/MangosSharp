// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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
using System.Runtime.CompilerServices;
using Mangos.Common.Enums.Global;
using Microsoft.VisualBasic;

// Using this logging type, all logs are displayed in console.
// Writting commands is done trought console.

namespace Mangos.Common.Logging
{
    public class ColoredConsoleWriter : BaseWriter
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Write(LogType type, string formatStr, params object[] arg)
        {
            if (LogLevel > type)
                return;
            switch (type)
            {
                case Global.LogType.NETWORK:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    }

                case Global.LogType.DEBUG:
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    }

                case Global.LogType.INFORMATION:
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    }

                case Global.LogType.USER:
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    }

                case Global.LogType.SUCCESS:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    }

                case Global.LogType.WARNING:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    }

                case Global.LogType.FAILED:
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    }

                case Global.LogType.CRITICAL:
                    {
                        Console.ForegroundColor = ConsoleColor.Red; // Red
                        break;
                    }

                case Global.LogType.DATABASE:
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
                return;
            switch (type)
            {
                case Global.LogType.NETWORK:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    }

                case Global.LogType.DEBUG:
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    }

                case Global.LogType.INFORMATION:
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    }

                case Global.LogType.USER:
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    }

                case Global.LogType.SUCCESS:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    }

                case Global.LogType.WARNING:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    }

                case Global.LogType.FAILED:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    }

                case Global.LogType.CRITICAL:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    }

                case Global.LogType.DATABASE:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        break;
                    }
            }

            if (arg is null)
            {
                Console.WriteLine("[" + Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss") + "] " + formatStr);
            }
            else
            {
                Console.WriteLine("[" + Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss") + "] " + formatStr, arg);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}