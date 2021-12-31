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

using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.WoWFakeClient;

public class RC4
{
    // http://www.skullsecurity.org/wiki/index.php/Crypto_and_Hashing

    public static byte[] Init(byte[] @base)
    {
        var val = 0;
        var position = 0;
        byte temp;
        var key = new byte[258];
        for (var i = 0; i <= 256 - 1; i++)
        {
            key[i] = (byte)i;
        }

        key[256] = 0;
        key[257] = 0;
        for (var i = 1; i <= 64; i++)
        {
            val = val + key[(i * 4) - 4] + @base[position % @base.Length];
            val &= 0xFF;
            position += 1;
            temp = key[(i * 4) - 4];
            key[(i * 4) - 4] = key[val & 0xFF];
            key[val & 0xFF] = temp;
            val = val + key[(i * 4) - 3] + @base[position % @base.Length];
            val &= 0xFF;
            position += 1;
            temp = key[(i * 4) - 3];
            key[(i * 4) - 3] = key[val & 0xFF];
            key[val & 0xFF] = temp;
            val = val + key[(i * 4) - 2] + @base[position % @base.Length];
            val &= 0xFF;
            position += 1;
            temp = key[(i * 4) - 2];
            key[(i * 4) - 2] = key[val & 0xFF];
            key[val & 0xFF] = temp;
            val = val + key[(i * 4) - 1] + @base[position % @base.Length];
            val &= 0xFF;
            position += 1;
            temp = key[(i * 4) - 1];
            key[(i * 4) - 1] = key[val & 0xFF];
            key[val & 0xFF] = temp;
        }

        return key;
    }

    public static void Crypt(ref byte[] data, byte[] key)
    {
        byte temp;
        for (int i = 0, loopTo = data.Length - 1; i <= loopTo; i++)
        {
            key[256] = (byte)((Conversions.ToInteger(key[256]) + 1) & 0xFF);
            key[257] = (byte)((Conversions.ToInteger(key[257]) + Conversions.ToInteger(key[key[256]])) & 0xFF);
            temp = key[key[257] & 0xFF];
            key[key[257] & 0xFF] = key[key[256] & 0xFF];
            key[key[256] & 0xFF] = temp;
            data[i] = (byte)(data[i] ^ key[(Conversions.ToInteger(key[key[257]]) + Conversions.ToInteger(key[key[256]])) & 0xFF]);
        }
    }
}
