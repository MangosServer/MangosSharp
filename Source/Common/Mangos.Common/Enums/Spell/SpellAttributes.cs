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

public enum SpellAttributes : uint
{
    SPELL_ATTR_NONE = 0x0,
    SPELL_ATTR_RANGED = 0x2,
    SPELL_ATTR_NEXT_ATTACK = 0x4,
    SPELL_ATTR_IS_ABILITY = 0x8, // Means this is not a "magic spell"
    SPELL_ATTR_UNK1 = 0x10,
    SPELL_ATTR_IS_TRADE_SKILL = 0x20,
    SPELL_ATTR_PASSIVE = 0x40,
    SPELL_ATTR_NO_VISIBLE_AURA = 0x80, // Does not show in buff/debuff pane. normally passive buffs
    SPELL_ATTR_UNK2 = 0x100,
    SPELL_ATTR_TEMP_WEAPON_ENCH = 0x200,
    SPELL_ATTR_NEXT_ATTACK2 = 0x400,
    SPELL_ATTR_UNK3 = 0x800,
    SPELL_ATTR_ONLY_DAYTIME = 0x1000, // Only usable at day
    SPELL_ATTR_ONLY_NIGHT = 0x2000, // Only usable at night
    SPELL_ATTR_ONLY_INDOOR = 0x4000, // Only usable indoors
    SPELL_ATTR_ONLY_OUTDOOR = 0x8000, // Only usable outdoors
    SPELL_ATTR_NOT_WHILE_SHAPESHIFTED = 0x10000, // Not while shapeshifted
    SPELL_ATTR_REQ_STEALTH = 0x20000, // Requires stealth
    SPELL_ATTR_UNK4 = 0x40000,
    SPELL_ATTR_SCALE_DMG_LVL = 0x80000, // Scale the damage with the caster's level
    SPELL_ATTR_STOP_ATTACK = 0x100000, // Stop attack after use this spell (and not begin attack if use)
    SPELL_ATTR_CANT_BLOCK = 0x200000, // This attack cannot be dodged, blocked, or parried
    SPELL_ATTR_UNK5 = 0x400000,
    SPELL_ATTR_WHILE_DEAD = 0x800000, // Castable while dead
    SPELL_ATTR_WHILE_MOUNTED = 0x1000000, // Castable while mounted
    SPELL_ATTR_COOLDOWN_AFTER_FADE = 0x2000000, // Activate and start cooldown after aura fade or remove summoned creature or go
    SPELL_ATTR_UNK6 = 0x4000000,
    SPELL_ATTR_WHILE_SEATED = 0x8000000, // Castable while seated
    SPELL_ATTR_NOT_WHILE_COMBAT = 0x10000000, // Set for all spells that are not allowed during combat
    SPELL_ATTR_IGNORE_IMMUNE = 0x20000000, // Ignore all immune effects
    SPELL_ATTR_BREAKABLE_BY_DAMAGE = 0x40000000, // Breakable by damage
    SPELL_ATTR_CANT_REMOVE = 0x80000000 // Positive Aura but cannot right click to remove
}
