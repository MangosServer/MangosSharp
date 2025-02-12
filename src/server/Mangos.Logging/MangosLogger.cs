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

internal sealed class MangosLogger : IMangosLogger
{
    public void Error(string message)
    {
        Log(message, ConsoleColor.Red);
    }

    public void Error(Exception exception, string message)
    {
        Log(exception, message, ConsoleColor.Red);
    }

    public void Information(string message)
    {
        Log(message, ConsoleColor.White);
    }

    public void Information(Exception exception, string message)
    {
        Log(exception, message, ConsoleColor.White);
    }

    public void Trace(string message)
    {
        Log(message, ConsoleColor.Gray);
    }

    public void Trace(Exception exception, string message)
    {
        Log(exception, message, ConsoleColor.Gray);
    }

    public void Warning(string message)
    {
        Log(message, ConsoleColor.Yellow);
    }

    public void Warning(Exception exception, string message)
    {
        Log(message, ConsoleColor.Yellow);
    }

    private void Log(string message, ConsoleColor color)
    {
        lock (this)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.UtcNow.TimeOfDay}: {message}");
        }
    }

    private void Log(Exception exception, string message, ConsoleColor color)
    {
        lock (this)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.UtcNow.TimeOfDay}: {message}");
            Console.WriteLine(exception);
        }
    }
}
