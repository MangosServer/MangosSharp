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
using System.IO;

// Using this logging type, all logs are saved in files numbered by date.
// Writting commands is done trought console.

namespace Mangos.Common.Legacy.Logging;

public class FileWriter : BaseWriter
{
    protected StreamWriter Output;
    protected DateTime LastDate = DateTime.Parse("2007-01-01");
    protected string Filename = "";

    protected void CreateNewFile()
    {
        LastDate = DateAndTime.Now.Date;
        Output = new StreamWriter(string.Format("{0}-{1}.log", Filename, Strings.Format(LastDate, "yyyy-MM-dd")), true) { AutoFlush = true };
        WriteLine(LogType.INFORMATION, "Log started successfully.");
    }

    public FileWriter(string createfilename)
    {
        Filename = createfilename;
        CreateNewFile();
    }

    private bool _disposedValue; // To detect redundant calls

    // IDisposable
    protected override void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            // TODO: set large fields to null.
            Output.Close();
        }

        _disposedValue = true;
    }

    public override void Write(LogType type, string formatStr, params object[] arg)
    {
        if (LogLevel > type)
        {
            return;
        }

        if (LastDate != DateAndTime.Now.Date)
        {
            CreateNewFile();
        }

        Output.Write(formatStr, arg);
    }

    public override void WriteLine(LogType type, string formatStr, params object[] arg)
    {
        if (LogLevel > type)
        {
            return;
        }

        if (LastDate != DateAndTime.Now.Date)
        {
            CreateNewFile();
        }

        Output.WriteLine(L[(int)type] + ":[" + Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss") + "] " + formatStr, arg);
    }
}
