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

// Writes log messages to files with automatic daily log rotation
// Creates a new log file each day with the format: {filename}-YYYY-MM-DD.log
public class FileWriter : BaseWriter
{
    // Current log file stream writer
    private StreamWriter _output = null!;
    // Tracks the date when the current log file was created
    private DateOnly _lastDate = DateOnly.Parse("2007-01-01");
    // Base filename (without date extension)
    private readonly string _filename;

    public FileWriter(string filename)
    {
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        CreateNewFile();
    }

    // Lazy-evaluated property that gets today's date
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    // Creates a new log file, disposing the old one if it exists
    // Called when the date changes to implement daily rotation
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

    // Checks if the date has changed and creates a new file if needed
    // Then checks if this writer is disposed before allowing writes
    private void ThrowIfDisposed()
    {
        if (_disposedValue)
        {
            throw new ObjectDisposedException(nameof(FileWriter));
        }
    }
}
