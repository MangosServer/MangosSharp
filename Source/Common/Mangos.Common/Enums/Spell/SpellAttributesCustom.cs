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

public enum SpellAttributesCustom : uint
{
    SPELL_ATTR_CU_CONE_BACK = 0x1U,
    SPELL_ATTR_CU_CONE_LINE = 0x2U,
    SPELL_ATTR_CU_SHARE_DAMAGE = 0x4U,
    SPELL_ATTR_CU_AURA_HOT = 0x8U,
    SPELL_ATTR_CU_AURA_DOT = 0x10U,
    SPELL_ATTR_CU_AURA_CC = 0x20U,
    SPELL_ATTR_CU_AURA_SPELL = 0x40U,
    SPELL_ATTR_CU_DIRECT_DAMAGE = 0x80U,
    SPELL_ATTR_CU_CHARGE = 0x100U,
    SPELL_ATTR_CU_LINK_CAST = 0x200U,
    SPELL_ATTR_CU_LINK_HIT = 0x400U,
    SPELL_ATTR_CU_LINK_AURA = 0x800U,
    SPELL_ATTR_CU_LINK_REMOVE = 0x1000U,
    SPELL_ATTR_CU_MOVEMENT_IMPAIR = 0x2000U
}
