// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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
using System.ComponentModel;
using System.IO;
using Mangos.Common.Enums.Global;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Common.DataStores
{
    [Description("DBC wrapper class using buffered stream for file access.")]
    public class BufferedDbc : BaseDbc, IDisposable
    {
        protected BufferedStream bs;

        [Description("Open filename for reading and initialize internals.")]
        public BufferedDbc(string fileName) : base(fileName)
        {
            bs = new BufferedStream(Fs);
        }

        [Description("Open filename for reading and initialize internals.")]
        public BufferedDbc(Stream stream) : base(stream)
        {
            bs = new BufferedStream(Fs);
        }

        [Description("Close file and dispose the dbc reader.")]
        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                Fs.Close();
                bs.Close();
                bs.Dispose();

                // MyBase.Dispose()
            }

            _disposedValue = true;
        }

        public override object get_Item(int row, int column, DBCValueType valueType)
        {
            if (row >= Rows)
                throw new ApplicationException("DBC: Row index outside file definition.");
            if (column >= Columns)
                throw new ApplicationException("DBC: Column index outside file definition.");
            TmpOffset = 20 + row * RowLength + column * 4;
            if (bs.Position != TmpOffset)
                bs.Seek(TmpOffset, SeekOrigin.Begin);
            bs.Read(Buffer, 0, 4);
            switch (valueType)
            {
                case Global.DBCValueType.DBC_FLOAT:
                    {
                        return BitConverter.ToSingle(Buffer, 0);
                    }

                case Global.DBCValueType.DBC_INTEGER:
                    {
                        return BitConverter.ToInt32(Buffer, 0);
                    }

                case Global.DBCValueType.DBC_STRING:
                    {
                        int offset = BitConverter.ToInt32(Buffer, 0);
                        bs.Seek(20 + Rows * RowLength + offset, SeekOrigin.Begin);
                        byte strByte = 0;
                        string strResult = "";
                        do
                        {
                            strByte = (byte)bs.ReadByte();
                            if (strByte != 0)
                                strResult += Conversions.ToString((char)strByte);
                        }
                        while (strByte != 0);
                        return strResult;
                    }

                default:
                    {
                        throw new ApplicationException("DBCReader: Undefined DBC field type.");
                        break;
                    }
            }
        }
    }
}