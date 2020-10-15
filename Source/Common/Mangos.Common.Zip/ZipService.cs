//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Mangos.Common.Zip
{
    public class ZipService
    {
        public byte[] Compress(byte[] data, int offset, int length)
        {
            using var outputStream = new MemoryStream();
            using var compressordStream = new DeflaterOutputStream(outputStream);
            compressordStream.Write(data, offset, length);
            compressordStream.Flush();
            return outputStream.ToArray();
        }

        public byte[] DeCompress(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            using (var compressedStream = new MemoryStream(data))
            using (var inputStream = new InflaterInputStream(compressedStream))
            {
                inputStream.CopyTo(outputStream);
                outputStream.Position = 0;
                return outputStream.ToArray();
            }
        }
    }
}