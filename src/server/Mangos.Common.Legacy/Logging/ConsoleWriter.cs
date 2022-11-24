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
using Microsoft.VisualBasic;
using System;
using System.Threading;

namespace Mangos.Common.Legacy.Logging;

public class ConsoleWriter : BaseWriter
{
    public override void Write(LogType type, string formatStr, params object[] arg)
    {
        if (LogLevel > type)
        {
            return;
        }

        Console.Write(formatStr, arg);
    }

    public override void WriteLine(LogType type, string formatStr, params object[] arg)
    {
        if (LogLevel > type)
        {
            return;
        }

        Console.WriteLine(L[(int)type] + ":" + "[" + Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss") + "] " + formatStr, arg);
    }

    public override string ReadLine()
    {
        Thread.Sleep(TimeSpan.FromMinutes(1d));
        return "info";
    }
}
