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

using Mangos.Common.Enums.Warden;
using Microsoft.VisualBasic.CompilerServices;
using System.IO;

namespace Mangos.World.Warden;

public partial class WS_Warden
{
    public class CheatCheck
    {
        public CheckTypes Type;

        public string Str;

        public string Str2;

        public int Addr;

        public byte[] Hash;

        public int Seed;

        public byte Length;

        public CheatCheck(CheckTypes Type_)
        {
            Str = "";
            Str2 = "";
            Addr = 0;
            Hash = System.Array.Empty<byte>();
            Seed = 0;
            Length = 0;
            Type = Type_;
        }

        public byte[] ToData(byte XorCheck, ref byte index)
        {
            MemoryStream ms = new();
            BinaryWriter bw = new(ms);
            bw.Write(XorCheck);
            checked
            {
                switch (Type)
                {
                    case CheckTypes.MEM_CHECK:
                        if (Operators.CompareString(Str, "", TextCompare: false) == 0)
                        {
                            bw.Write((byte)0);
                        }
                        else
                        {
                            bw.Write(index);
                            index++;
                        }
                        bw.Write(Addr);
                        bw.Write(Length);
                        break;

                    case CheckTypes.PAGE_CHECK_A_B:
                        bw.Write(Seed);
                        bw.Write(Hash, 0, Hash.Length);
                        bw.Write(Addr);
                        bw.Write(Length);
                        break;

                    case CheckTypes.MPQ_CHECK:
                        bw.Write(index);
                        index++;
                        break;

                    case CheckTypes.LUA_STR_CHECK:
                        bw.Write(index);
                        index++;
                        break;

                    case CheckTypes.DRIVER_CHECK:
                        bw.Write(Seed);
                        bw.Write(Hash, 0, Hash.Length);
                        bw.Write(index);
                        index++;
                        break;

                    case CheckTypes.PROC_CHECK:
                        bw.Write(Seed);
                        bw.Write(Hash, 0, Hash.Length);
                        bw.Write(index);
                        index++;
                        bw.Write(index);
                        index++;
                        bw.Write(Addr);
                        bw.Write(Length);
                        break;

                    case CheckTypes.MODULE_CHECK:
                        bw.Write(Seed);
                        bw.Write(Hash, 0, Hash.Length);
                        break;
                }
                var tmpData = ms.ToArray();
                ms.Close();
                return tmpData;
            }
        }
    }
}
