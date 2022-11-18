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

public enum LockType : byte
{
    LOCKTYPE_PICKLOCK = 1,
    LOCKTYPE_HERBALISM = 2,
    LOCKTYPE_MINING = 3,
    LOCKTYPE_DISARM_TRAP = 4,
    LOCKTYPE_OPEN = 5,
    LOCKTYPE_TREASURE = 6,
    LOCKTYPE_CALCIFIED_ELVEN_GEMS = 7,
    LOCKTYPE_CLOSE = 8,
    LOCKTYPE_ARM_TRAP = 9,
    LOCKTYPE_QUICK_OPEN = 10,
    LOCKTYPE_QUICK_CLOSE = 11,
    LOCKTYPE_OPEN_TINKERING = 12,
    LOCKTYPE_OPEN_KNEELING = 13,
    LOCKTYPE_OPEN_ATTACKING = 14,
    LOCKTYPE_GAHZRIDIAN = 15,
    LOCKTYPE_BLASTING = 16,
    LOCKTYPE_SLOW_OPEN = 17,
    LOCKTYPE_SLOW_CLOSE = 18,
    LOCKTYPE_FISHING = 19
}
