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
    [Description("DBC wrapper class.")]
    public class BaseDbc : IDisposable
    {

        // Variables
        protected Stream Fs;
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

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        // Default Functions
        [Description("Close file and dispose the dbc reader.")]
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                Fs.Close();
            }

            _disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        [Description("Open filename for reading and initialize internals.")]
        public BaseDbc(string fileName)
        {
            Fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            ReadHeader();
        }

        [Description("Open filename for reading and initialize internals.")]
        public BaseDbc(Stream Stream)
        {
            Fs = Stream;
            ReadHeader();
        }

        protected void ReadHeader()
        {
            try
            {
                Fs.Read(Buffer, 0, 4);
                FType = System.Text.Encoding.ASCII.GetString(Buffer);
                Fs.Read(Buffer, 0, 4);
                Rows = BitConverter.ToInt32(Buffer, 0);
                Fs.Read(Buffer, 0, 4);
                Columns = BitConverter.ToInt32(Buffer, 0);
                Fs.Read(Buffer, 0, 4);
                RowLength = BitConverter.ToInt32(Buffer, 0);
                Fs.Read(Buffer, 0, 4);
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

        public virtual object this[int row, int column, DBCValueType valueType = DBCValueType.DBC_INTEGER]
        {
            get
            {
                if (row >= Rows)
                    throw new ApplicationException("DBC: Row index outside file definition.");
                if (column >= Columns)
                    throw new ApplicationException("DBC: Column index outside file definition.");
                TmpOffset = 20 + row * RowLength + column * 4;
                if (Fs.Position != TmpOffset)
                    Fs.Seek(TmpOffset, SeekOrigin.Begin);
                Fs.Read(Buffer, 0, 4);
                switch (valueType)
                {
                    case DBCValueType.DBC_FLOAT:
                        {
                            return BitConverter.ToSingle(Buffer, 0);
                        }

                    case DBCValueType.DBC_INTEGER:
                        {
                            return BitConverter.ToInt32(Buffer, 0);
                        }

                    case DBCValueType.DBC_STRING:
                        {
                            int offset = BitConverter.ToInt32(Buffer, 0);
                            Fs.Seek(20 + Rows * RowLength + offset, SeekOrigin.Begin);
                            byte strByte;
                            string strResult;
                            strResult = "";
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
                            throw new ApplicationException("DBCReader: Undefined DBC field type.");
                        }
                }
            }
        }

        [Description("Return formated DBC header information.")]
        public string GetFileInformation
        {
            get
            {
                return string.Format("DBC: {0}:{1}x{2}:{3}:{4}", FType, Rows, Columns, RowLength, StringPartLength);
            }
        }
    }
}