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

public enum SpellAuraProcFlags
{
    AURA_PROC_NULL = 0x0,
    AURA_PROC_ON_ANY_HOSTILE_ACTION = 0x1,
    AURA_PROC_ON_GAIN_EXPIERIENCE = 0x2,
    AURA_PROC_ON_MELEE_ATTACK = 0x4,
    AURA_PROC_ON_CRIT_HIT_VICTIM = 0x8,
    AURA_PROC_ON_CAST_SPELL = 0x10,
    AURA_PROC_ON_PHYSICAL_ATTACK_VICTIM = 0x20,
    AURA_PROC_ON_RANGED_ATTACK = 0x40,
    AURA_PROC_ON_RANGED_CRIT_ATTACK = 0x80,
    AURA_PROC_ON_PHYSICAL_ATTACK = 0x100,
    AURA_PROC_ON_MELEE_ATTACK_VICTIM = 0x200,
    AURA_PROC_ON_SPELL_HIT = 0x400,
    AURA_PROC_ON_RANGED_CRIT_ATTACK_VICTIM = 0x800,
    AURA_PROC_ON_CRIT_ATTACK = 0x1000,
    AURA_PROC_ON_RANGED_ATTACK_VICTIM = 0x2000,
    AURA_PROC_ON_PRE_DISPELL_AURA_VICTIM = 0x4000,
    AURA_PROC_ON_SPELL_LAND_VICTIM = 0x8000,
    AURA_PROC_ON_CAST_SPECIFIC_SPELL = 0x10000,
    AURA_PROC_ON_SPELL_HIT_VICTIM = 0x20000,
    AURA_PROC_ON_SPELL_CRIT_HIT_VICTIM = 0x40000,
    AURA_PROC_ON_TARGET_DIE = 0x80000,
    AURA_PROC_ON_ANY_DAMAGE_VICTIM = 0x100000,
    AURA_PROC_ON_TRAP_TRIGGER = 0x200000,                // triggers on trap activation
    AURA_PROC_ON_AUTO_SHOT_HIT = 0x400000,
    AURA_PROC_ON_ABSORB = 0x800000,
    AURA_PROC_ON_RESIST_VICTIM = 0x1000000,
    AURA_PROC_ON_DODGE_VICTIM = 0x2000000,
    AURA_PROC_ON_DIE = 0x4000000,
    AURA_PROC_REMOVEONUSE = 0x8000000,                   // remove AURA_PROChcharge only when it is used
    AURA_PROC_MISC = 0x10000000,                          // our custom flag to decide if AURA_PROC dmg or shield
    AURA_PROC_ON_BLOCK_VICTIM = 0x20000000,
    AURA_PROC_ON_SPELL_CRIT_HIT = 0x40000000,
    AURA_PROC_TARGET_SELF = unchecked((int)0x80000000)                // our custom flag to decide if AURA_PROC target is self or victim
}
