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

public enum SpellImplicitTargets : byte
{
    TARGET_NOTHING = 0,
    TARGET_SELF = 1,
    TARGET_RANDOM_ENEMY_CHAIN_IN_AREA = 2,           // Only one spell has this one, but regardless, it's a target type after all
    TARGET_PET = 5,
    TARGET_CHAIN_DAMAGE = 6,
    TARGET_AREAEFFECT_CUSTOM = 8,
    TARGET_INNKEEPER_COORDINATES = 9,                // Used in teleport to innkeeper spells
    TARGET_ALL_ENEMY_IN_AREA = 15,
    TARGET_ALL_ENEMY_IN_AREA_INSTANT = 16,
    TARGET_TABLE_X_Y_Z_COORDINATES = 17,             // Used in teleport spells and some other
    TARGET_EFFECT_SELECT = 18,                       // Highly depends on the spell effect
    TARGET_AROUND_CASTER_PARTY = 20,
    TARGET_SELECTED_FRIEND = 21,
    TARGET_AROUND_CASTER_ENEMY = 22,                 // Used only in TargetA, target selection dependent from TargetB
    TARGET_SELECTED_GAMEOBJECT = 23,
    TARGET_INFRONT = 24,
    TARGET_DUEL_VS_PLAYER = 25,                      // Used when part of spell is casted on another target
    TARGET_GAMEOBJECT_AND_ITEM = 26,
    TARGET_MASTER = 27,      // not tested
    TARGET_AREA_EFFECT_ENEMY_CHANNEL = 28,
    TARGET_ALL_FRIENDLY_UNITS_AROUND_CASTER = 30,    // In TargetB used only with TARGET_ALL_AROUND_CASTER and in self casting range in TargetA
    TARGET_ALL_FRIENDLY_UNITS_IN_AREA = 31,
    TARGET_MINION = 32,                              // Summons your pet to you.
    TARGET_ALL_PARTY = 33,
    TARGET_ALL_PARTY_AROUND_CASTER_2 = 34,           // Used in Tranquility
    TARGET_SINGLE_PARTY = 35,
    TARGET_AREAEFFECT_PARTY = 37,                    // Power infuses the target's party, increasing their Shadow resistance by $s1 for $d.
    TARGET_SCRIPT = 38,
    TARGET_SELF_FISHING = 39,                        // Equip a fishing pole and find a body of water to fish.
    TARGET_TOTEM_EARTH = 41,
    TARGET_TOTEM_WATER = 42,
    TARGET_TOTEM_AIR = 43,
    TARGET_TOTEM_FIRE = 44,
    TARGET_CHAIN_HEAL = 45,
    TARGET_DYNAMIC_OBJECT = 47,
    TARGET_AREA_EFFECT_SELECTED = 53,                // Inflicts $s1 Fire damage to all enemies in a selected area.
    TARGET_UNK54 = 54,
    TARGET_RANDOM_RAID_MEMBER = 56,
    TARGET_SINGLE_FRIEND_2 = 57,
    TARGET_AREAEFFECT_PARTY_AND_CLASS = 61,
    TARGET_DUELVSPLAYER_COORDINATES = 63,
    TARGET_BEHIND_VICTIM = 65,                       // uses in teleport behind spells
    TARGET_SINGLE_ENEMY = 77,
    TARGET_SELF2 = 87,
    TARGET_NONCOMBAT_PET = 90
}
