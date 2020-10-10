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
    [Description("DBC wrapper class using optimizations for reading row by row.")]
    public class OptimizedDbc : BaseDbc
    {
        protected byte[] TmpRow;
        protected int TmpRowRead = -1;

        [Description("Open filename for reading and initialize internals.")]
        public OptimizedDbc(string fileName) : base(fileName)
        {
            TmpRow = new byte[RowLength + 1];
        }

        [Description("Open filename for reading and initialize internals.")]
        public OptimizedDbc(Stream stream) : base(stream)
        {
            TmpRow = new byte[RowLength + 1];
        }

        protected void ReadRow(int row)
        {
            TmpOffset = 20 + row * RowLength;
            if (Fs.Position != TmpOffset)
                Fs.Seek(TmpOffset, SeekOrigin.Begin);
            Fs.Read(TmpRow, 0, RowLength);
            TmpRowRead = row;
        }

        public override object Item(int row, int column, DBCValueType valueType = DBCValueType.DBC_INTEGER)
        {
            if (row >= Rows)
                throw new ApplicationException("DBC: Row index outside file definition.");
            if (column >= Columns)
                throw new ApplicationException("DBC: Column index outside file definition.");
            if (TmpRowRead != row)
                ReadRow(row);
            Array.Copy(TmpRow, column * 4, Buffer, 0, 4);
            switch (valueType)
            {
                case DBCValueType.DBC_INTEGER:
                    {
                        return BitConverter.ToInt32(Buffer, 0);
                    }

                case DBCValueType.DBC_FLOAT:
                    {
                        return BitConverter.ToSingle(Buffer, 0);
                    }

                case DBCValueType.DBC_STRING:
                    {
                        int offset = BitConverter.ToInt32(Buffer, 0);
                        Fs.Seek(20 + Rows * RowLength + offset, SeekOrigin.Begin);
                        string strResult = "";
                        byte strByte;
                        do
                        {
                            strByte = (byte)Fs.ReadByte();
                            strResult += Conversions.ToString((char)strByte);
                        }
                        while (strByte != 0);
                        return strResult;
                    }

                default:
                    {
                        throw new ApplicationException("DBC: Undefined DBC field type.");
                    }
            }
        }
    }
}