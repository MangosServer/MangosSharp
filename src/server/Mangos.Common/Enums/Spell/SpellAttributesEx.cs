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

public enum SpellAttributesEx
{
    SPELL_ATTR_EX_DRAIN_ALL_POWER = 0x2, // use all power (Only paladin Lay of Hands and Bunyanize)
    SPELL_ATTR_EX_CHANNELED_1 = 0x4, // channeled 1
    SPELL_ATTR_EX_NOT_BREAK_STEALTH = 0x20, // Not break stealth
    SPELL_ATTR_EX_CHANNELED_2 = 0x40, // channeled 2
    SPELL_ATTR_EX_NEGATIVE = 0x80, // negative spell?
    SPELL_ATTR_EX_NOT_IN_COMBAT_TARGET = 0x100, // Spell req target not to be in combat state
    SPELL_ATTR_EX_NOT_PASSIVE = 0x400, // not passive? (if this flag is set and SPELL_PASSIVE is set in Attributes it shouldn't be counted as a passive?)
    SPELL_ATTR_EX_DISPEL_AURAS_ON_IMMUNITY = 0x8000, // remove auras on immunity
    SPELL_ATTR_EX_UNAFFECTED_BY_SCHOOL_IMMUNE = 0x10000, // unaffected by school immunity
    SPELL_ATTR_EX_REQ_COMBO_POINTS1 = 0x100000, // Req combo points on target
    SPELL_ATTR_EX_REQ_COMBO_POINTS2 = 0x400000 // Req combo points on target
}
