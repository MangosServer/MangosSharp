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

public enum SpellMissInfo : byte
{
    SPELL_MISS_NONE = 0,
    SPELL_MISS_MISS = 1,
    SPELL_MISS_RESIST = 2,
    SPELL_MISS_DODGE = 3,
    SPELL_MISS_PARRY = 4,
    SPELL_MISS_BLOCK = 5,
    SPELL_MISS_EVADE = 6,
    SPELL_MISS_IMMUNE = 7,
    SPELL_MISS_IMMUNE2 = 8,
    SPELL_MISS_DEFLECT = 9,
    SPELL_MISS_ABSORB = 10,
    SPELL_MISS_REFLECT = 11
}
