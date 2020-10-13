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

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Mangos.WoWFakeClient
{
    public static class ZLib
    {
        public static byte[] Compress(byte[] b, int offset, int len)
        {
            byte[] buffer2;
            try
            {
                var outputStream = new MemoryStream();
                var compressordStream = new DeflaterOutputStream(outputStream);
                compressordStream.Write(b, offset, len);
                compressordStream.Flush();
                compressordStream.Close();
                buffer2 = outputStream.ToArray();
            }
            catch (Exception)
            {
                buffer2 = null;
            }

            return buffer2;
        }

        public static byte[] DeCompress(byte[] b)
        {
            byte[] buffer2 = null;
            var writeBuffer = new byte[(short.MaxValue + 1)];
            var decopressorStream = new InflaterInputStream(new MemoryStream(b));
            try
            {
                int bytesRead = decopressorStream.Read(writeBuffer, 0, writeBuffer.Length);
                if (bytesRead > 0)
                {
                    buffer2 = new byte[bytesRead];
                    Buffer.BlockCopy(writeBuffer, 0, buffer2, 0, bytesRead);
                }

                decopressorStream.Flush();
                decopressorStream.Close();
            }
            catch (Exception)
            {
                buffer2 = null;
            }

            return buffer2;
        }
    }
}