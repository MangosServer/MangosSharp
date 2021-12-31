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

using System;

namespace Mangos.Common.Enums.Item;

[Flags]
public enum ITEM_FLAGS
{
    ITEM_FLAGS_BINDED = 0x1,
    ITEM_FLAGS_CONJURED = 0x2,
    ITEM_FLAGS_OPENABLE = 0x4,
    ITEM_FLAGS_WRAPPED = 0x8,
    ITEM_FLAGS_WRAPPER = 0x200, // used or not used wrapper
    ITEM_FLAGS_PARTY_LOOT = 0x800, // determines if item is party loot or not
    ITEM_FLAGS_CHARTER = 0x2000, // arena/guild charter
    ITEM_FLAGS_THROWABLE = 0x400000, // not used in game for check trow possibility, only for item in game tooltip
    ITEM_FLAGS_SPECIALUSE = 0x800000
}
