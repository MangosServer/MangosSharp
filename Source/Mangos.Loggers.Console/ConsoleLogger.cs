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

using System;

namespace Mangos.Loggers.Console;

public class ConsoleLogger : ILogger
{
    private static readonly object _lockObject = new();

    public void Debug(string format, params object[] args)
    {
        Write(ConsoleColor.Gray, format, args);
    }

    public void Message(string format, params object[] args)
    {
        Write(ConsoleColor.White, format, args);
    }

    public void Warning(string format, params object[] args)
    {
        Write(ConsoleColor.Yellow, format, args);
    }

    public void Error(string format, params object[] args)
    {
        Write(ConsoleColor.Red, format, args);
    }

    public void Error(string format, Exception exception, params object[] args)
    {
        lock (_lockObject)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(Format(format, args));
            System.Console.WriteLine(exception.ToString());
        }
    }

    private void Write(ConsoleColor color, string format, object[] args)
    {
        lock (_lockObject)
        {
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(Format(format, args));
        }
    }

    private string Format(string format, object[] args)
    {
        return string.Format("[{0}] {1}", DateTime.Now, string.Format(format, args));
    }
}
