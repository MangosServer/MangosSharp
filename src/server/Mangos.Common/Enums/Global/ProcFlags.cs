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

public enum ProcFlags : uint
{
    PROC_FLAG_NONE = 0x0,                            // None
    PROC_FLAG_HIT_MELEE = 0x1,                       // On melee hit
    PROC_FLAG_STRUCK_MELEE = 0x2,                    // On being struck melee
    PROC_FLAG_KILL_XP_GIVER = 0x4,                   // On kill target giving XP or honor
    PROC_FLAG_SPECIAL_DROP = 0x8,                    //
    PROC_FLAG_DODGE = 0x10,                          // On dodge melee attack
    PROC_FLAG_PARRY = 0x20,                          // On parry melee attack
    PROC_FLAG_BLOCK = 0x40,                          // On block attack
    PROC_FLAG_TOUCH = 0x80,                          // On being touched (for bombs, probably?)
    PROC_FLAG_TARGET_LOW_HEALTH = 0x100,             // On deal damage to enemy with 20% or less health
    PROC_FLAG_LOW_HEALTH = 0x200,                    // On health dropped below 20%
    PROC_FLAG_STRUCK_RANGED = 0x400,                 // On being struck ranged
    PROC_FLAG_HIT_SPECIAL = 0x800,                   // (!)Removed, may be reassigned in future
    PROC_FLAG_CRIT_MELEE = 0x1000,                   // On crit melee
    PROC_FLAG_STRUCK_CRIT_MELEE = 0x2000,            // On being critically struck in melee
    PROC_FLAG_CAST_SPELL = 0x4000,                   // On cast spell
    PROC_FLAG_TAKE_DAMAGE = 0x8000,                  // On take damage
    PROC_FLAG_CRIT_SPELL = 0x10000,                  // On crit spell
    PROC_FLAG_HIT_SPELL = 0x20000,                   // On hit spell
    PROC_FLAG_STRUCK_CRIT_SPELL = 0x40000,           // On being critically struck by a spell
    PROC_FLAG_HIT_RANGED = 0x80000,                  // On getting ranged hit
    PROC_FLAG_STRUCK_SPELL = 0x100000,               // On being struck by a spell
    PROC_FLAG_TRAP = 0x200000,                       // On trap activation (?)
    PROC_FLAG_CRIT_RANGED = 0x400000,                // On getting ranged crit
    PROC_FLAG_STRUCK_CRIT_RANGED = 0x800000,         // On being critically struck by a ranged attack
    PROC_FLAG_RESIST_SPELL = 0x1000000,              // On resist enemy spell
    PROC_FLAG_TARGET_RESISTS = 0x2000000,            // On enemy resisted spell
    PROC_FLAG_TARGET_DODGE_OR_PARRY = 0x4000000,     // On enemy dodges/parries
    PROC_FLAG_HEAL = 0x8000000,                      // On heal
    PROC_FLAG_CRIT_HEAL = 0x10000000,                // On critical healing effect
    PROC_FLAG_HEALED = 0x20000000,                   // On healing
    PROC_FLAG_TARGET_BLOCK = 0x40000000,             // On enemy blocks
    PROC_FLAG_MISS = 0x80000000                     // On miss melee attack
}
