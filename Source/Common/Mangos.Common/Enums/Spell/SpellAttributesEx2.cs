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

public enum SpellAttributesEx2
{
    SPELL_ATTR_EX2_AUTO_SHOOT = 0x20, // Auto Shoot?
    SPELL_ATTR_EX2_HEALTH_FUNNEL = 0x800, // Health funnel pets?
    SPELL_ATTR_EX2_NOT_NEED_SHAPESHIFT = 0x80000, // does not necessarly need shapeshift
    SPELL_ATTR_EX2_CANT_CRIT = 0x20000000 // Spell can't crit
}
