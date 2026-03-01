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

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO;

namespace Mangos.Zip;

public class ZipService
{
    public byte[] Compress(byte[] data, int offset, int length)
    {
        Console.WriteLine($"[Zip] Compressing {length} bytes (offset={offset})");
        using MemoryStream outputStream = new();
        using DeflaterOutputStream compressordStream = new(outputStream);
        compressordStream.Write(data, offset, length);
        compressordStream.Flush();
        var result = outputStream.ToArray();
        Console.WriteLine($"[Zip] Compressed {length} -> {result.Length} bytes ({(result.Length * 100.0 / length):F1}% ratio)");
        return result;
    }

    public byte[] DeCompress(byte[] data)
    {
        Console.WriteLine($"[Zip] Decompressing {data.Length} bytes");
        using MemoryStream outputStream = new();
        using MemoryStream compressedStream = new(data);
        using InflaterInputStream inputStream = new(compressedStream);
        inputStream.CopyTo(outputStream);
        outputStream.Position = 0;
        var result = outputStream.ToArray();
        Console.WriteLine($"[Zip] Decompressed {data.Length} -> {result.Length} bytes");
        return result;
    }
}
