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
using System.Threading;
using Mangos.Common.Enums.Global;

namespace Mangos.Logging;

// Writes log messages to console without color formatting
// Simple text output for systems that don't support colored console output
public class ConsoleWriter : BaseWriter
{
    public override void Write(LogType type, string formatStr, params object?[] arg)
    {
        if (!IsEnabled(type))
        {
            return;
        }

        Console.Write(formatStr, arg);
    }

    public override void WriteLine(LogType type, string formatStr, params object?[] arg)
    {
        if (!IsEnabled(type))
        {
            return;
        }

        var message = string.Format(formatStr, arg);
        Console.WriteLine($"{Labels[(int)type]}:[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
