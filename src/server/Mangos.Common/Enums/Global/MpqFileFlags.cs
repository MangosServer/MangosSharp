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

namespace Mangos.Common.Enums.Global;

public enum MpqFileFlags : long
{
    MPQ_Changed = 1L,                     // &H00000001
    MPQ_Protected = 2L,                   // &H00000002
    MPQ_CompressedPK = 256L,              // &H00000100
    MPQ_CompressedMulti = 512L,           // &H00000200
    MPQ_Compressed = 65280L,              // &H0000FF00
    MPQ_Encrypted = 65536L,               // &H00010000
    MPQ_FixSeed = 131072L,                // &H00020000
    MPQ_SingleUnit = 16777216L,           // &H01000000
    MPQ_Unknown_02000000 = 33554432L,     // &H02000000 - The file is only 1 byte long and its name is a hash
    MPQ_FileHasMetadata = 67108864L,      // &H04000000 - Indicates the file has associted metadata.
    MPQ_Exists = 2147483648L             // &H80000000
}
