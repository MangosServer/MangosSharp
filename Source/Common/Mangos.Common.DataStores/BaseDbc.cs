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
using System.ComponentModel;
using System.IO;

namespace Mangos.Common.DataStores
{
    [Description("DBC wrapper class.")]
    public class BaseDbc : IDisposable
    {
        protected Stream FileStream;
        
        [Description("Header information: File type.")]
        public string FType = "";
        
        [Description("Header information: Rows contained in the file.")]
        public int Rows = 0;
        
        [Description("Header information: Columns for each row.")]
        public int Columns = 0;
        
        [Description("Header information: Bytes ocupied by each row.")]
        public int RowLength = 0;
        
        [Description("Header information: Strings data block length.")]
        public int StringPartLength = 0;
        protected byte[] Buffer = new byte[4];
        protected long TmpOffset = 0L;

        [Description("Open filename for reading and initialize internals.")]
        public BaseDbc(string fileName)
        {
            FileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            ReadHeader();
        }

        protected void ReadHeader()
        {
            try
            {
                FileStream.Read(Buffer, 0, 4);
                FType = System.Text.Encoding.ASCII.GetString(Buffer);
                FileStream.Read(Buffer, 0, 4);
                Rows = BitConverter.ToInt32(Buffer, 0);
                FileStream.Read(Buffer, 0, 4);
                Columns = BitConverter.ToInt32(Buffer, 0);
                FileStream.Read(Buffer, 0, 4);
                RowLength = BitConverter.ToInt32(Buffer, 0);
                FileStream.Read(Buffer, 0, 4);
                StringPartLength = BitConverter.ToInt32(Buffer, 0);
            }
            catch (Exception)
            {
                throw new ApplicationException("DBC: File could not be read.");
            }
            finally
            {
                if (FType != "WDBC")
                    throw new ApplicationException("DBC: Not valid DBC file format.");
            }
        }

        public virtual T Read<T>(long row, int column)
        {
            if (row >= Rows)
                throw new ApplicationException("DBC: Row index outside file definition.");
            if (column >= Columns)
                throw new ApplicationException("DBC: Column index outside file definition.");

            TmpOffset = 20 + row * RowLength + column * 4;
            if (FileStream.Position != TmpOffset)
                FileStream.Seek(TmpOffset, SeekOrigin.Begin);

            FileStream.Read(Buffer, 0, 4);

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Byte:
                    return (T)Convert.ChangeType(BitConverter.ToInt32(Buffer, 0), typeof(T));
                case TypeCode.UInt32:
                    return (T)Convert.ChangeType(BitConverter.ToUInt32(Buffer, 0), typeof(T));
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return (T)Convert.ChangeType(BitConverter.ToSingle(Buffer, 0), typeof(T));
                case TypeCode.String:
                    int offset = BitConverter.ToInt32(Buffer, 0);
                    FileStream.Seek(20 + Rows * RowLength + offset, SeekOrigin.Begin);
                    byte strByte;
                    string strResult;
                    strResult = "";
                    do
                    {
                        strByte = (byte)FileStream.ReadByte();
                        strResult += Convert.ToString((char)strByte);
                    }
                    while (strByte != 0);
                    return (T)Convert.ChangeType(strResult, typeof(T));
                default:
                    return (T)Convert.ChangeType(BitConverter.ToInt32(Buffer, 0), typeof(T));
            }
        }

        [Description("Close file and dispose the dbc reader.")]
        public virtual void Dispose()
        {
            FileStream?.Close();
        }
    }
}