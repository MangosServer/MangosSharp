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
using System.IO;

namespace Mangos.Logging;

public class FileWriter : BaseWriter
{
    private StreamWriter _output = null!;
    private DateOnly _lastDate = DateOnly.Parse("2007-01-01");
    private readonly string _filename;

    public FileWriter(string filename)
    {
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        CreateNewFile();
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    protected void CreateNewFile()
    {
        ThrowIfDisposed();
        _output?.Dispose();
        _lastDate = Today;
        _output = new StreamWriter($"{_filename}-{_lastDate:yyyy-MM-dd}.log", true) { AutoFlush = true };
        WriteLine(LogType.INFORMATION, "Log started successfully.");
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
            _output?.Dispose();
            _output = null!;
        }

        _disposedValue = true;
        base.Dispose(disposing);
    }

    public override void Write(LogType type, string formatStr, params object?[] arg)
    {
        ThrowIfDisposed();

        if (!IsEnabled(type))
        {
            return;
        }

        if (_lastDate != Today)
        {
            CreateNewFile();
        }

        _output.Write(formatStr, arg);
    }

    public override void WriteLine(LogType type, string formatStr, params object?[] arg)
    {
        ThrowIfDisposed();

        if (!IsEnabled(type))
        {
            return;
        }

        if (_lastDate != Today)
        {
            CreateNewFile();
        }

        var message = string.Format(formatStr, arg);
        _output.WriteLine($"{Labels[(int)type]}:[{DateTime.Now:HH:mm:ss}] {message}");
    }

    private void ThrowIfDisposed()
    {
        if (_disposedValue)
        {
            throw new ObjectDisposedException(nameof(FileWriter));
        }
    }
}
