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

namespace Mangos.Common.Enums.Spell;

public enum SpellCastTargetFlags
{
    TARGET_FLAG_SELF = 0x0,
    TARGET_FLAG_UNIT = 0x2,
    TARGET_FLAG_ITEM = 0x10,
    TARGET_FLAG_SOURCE_LOCATION = 0x20,
    TARGET_FLAG_DEST_LOCATION = 0x40,
    TARGET_FLAG_OBJECT_UNK = 0x80,
    TARGET_FLAG_PVP_CORPSE = 0x200,
    TARGET_FLAG_OBJECT = 0x800,
    TARGET_FLAG_TRADE_ITEM = 0x1000,
    TARGET_FLAG_STRING = 0x2000,
    TARGET_FLAG_UNK1 = 0x4000,
    TARGET_FLAG_CORPSE = 0x8000,
    TARGET_FLAG_UNK2 = 0x10000
}
