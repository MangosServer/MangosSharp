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

public enum SpellFailedReason : byte
{
    SPELL_FAILED_AFFECTING_COMBAT = 0x0,                             // 0x000
    SPELL_FAILED_ALREADY_AT_FULL_HEALTH = 0x1,                       // 0x001
    SPELL_FAILED_ALREADY_AT_FULL_POWER = 0x2,                        // 0x002
    SPELL_FAILED_ALREADY_BEING_TAMED = 0x3,                          // 0x003
    SPELL_FAILED_ALREADY_HAVE_CHARM = 0x4,                           // 0x004
    SPELL_FAILED_ALREADY_HAVE_SUMMON = 0x5,                          // 0x005
    SPELL_FAILED_ALREADY_OPEN = 0x6,                                 // 0x006

    // SPELL_FAILED_AURA_BOUNCED = &H7                                ' 0x007
    SPELL_FAILED_MORE_POWERFUL_SPELL_ACTIVE = 0x7,                   // 0x007

    // SPELL_FAILED_AUTOTRACK_INTERRUPTED = &H8                       ' 0x008 ' old commented CAST_FAIL_FAILED = 8,-> 29
    SPELL_FAILED_BAD_IMPLICIT_TARGETS = 0x9,                         // 0x009

    SPELL_FAILED_BAD_TARGETS = 0xA,                                  // 0x00A
    SPELL_FAILED_CANT_BE_CHARMED = 0xB,                              // 0x00B
    SPELL_FAILED_CANT_BE_DISENCHANTED = 0xC,                         // 0x00C
    SPELL_FAILED_CANT_BE_PROSPECTED = 0xD,
    SPELL_FAILED_CANT_CAST_ON_TAPPED = 0xE,                          // 0x00D
    SPELL_FAILED_CANT_DUEL_WHILE_INVISIBLE = 0xF,                    // 0x00E
    SPELL_FAILED_CANT_DUEL_WHILE_STEALTHED = 0x10,                   // 0x00F

    // SPELL_FAILED_CANT_STEALTH = &H10                               ' 0x010
    // SPELL_FAILED_CASTER_AURASTATE = &H11                           ' 0x011
    SPELL_FAILED_CANT_TOO_CLOSE_TO_ENEMY = 0x11,

    SPELL_FAILED_CANT_DO_THAT_YET = 0x12,
    SPELL_FAILED_CASTER_DEAD = 0x13,                                 // 0x012
    SPELL_FAILED_CHARMED = 0x14,                                     // 0x013
    SPELL_FAILED_CHEST_IN_USE = 0x15,                                // 0x014
    SPELL_FAILED_CONFUSED = 0x16,                                    // 0x015
    SPELL_FAILED_DONT_REPORT = 0x17,                                 // 0x016 ' [-ZERO] need check
    SPELL_FAILED_EQUIPPED_ITEM = 0x18,                               // 0x017
    SPELL_FAILED_EQUIPPED_ITEM_CLASS = 0x19,                         // 0x018
    SPELL_FAILED_EQUIPPED_ITEM_CLASS_MAINHAND = 0x1A,                // 0x019
    SPELL_FAILED_EQUIPPED_ITEM_CLASS_OFFHAND = 0x1B,                 // 0x01A
    SPELL_FAILED_ERROR = 0x1C,                                       // 0x01B
    SPELL_FAILED_FIZZLE = 0x1D,                                      // 0x01C
    SPELL_FAILED_FLEEING = 0x1E,                                     // 0x01D
    SPELL_FAILED_FOOD_LOWLEVEL = 0x1F,                               // 0x01E
    SPELL_FAILED_HIGHLEVEL = 0x20,                                   // 0x01F

    // SPELL_FAILED_HUNGER_SATIATED = &H21                            ' 0x020
    SPELL_FAILED_IMMUNE = 0x22,                                      // 0x021

    SPELL_FAILED_INTERRUPTED = 0x23,                                 // 0x022
    SPELL_FAILED_INTERRUPTED_COMBAT = 0x24,                          // 0x023
    SPELL_FAILED_ITEM_ALREADY_ENCHANTED = 0x25,                      // 0x024
    SPELL_FAILED_ITEM_GONE = 0x26,                                   // 0x025
    SPELL_FAILED_ITEM_NOT_FOUND = 0x27,                              // 0x026
    SPELL_FAILED_ITEM_NOT_READY = 0x28,                              // 0x027
    SPELL_FAILED_LEVEL_REQUIREMENT = 0x29,                           // 0x028
    SPELL_FAILED_LINE_OF_SIGHT = 0x2A,                               // 0x029
    SPELL_FAILED_LOWLEVEL = 0x2B,                                    // 0x02A
    SPELL_FAILED_LOW_CASTLEVEL = 0x2C,                               // 0x02B
    SPELL_FAILED_MAINHAND_EMPTY = 0x2D,                              // 0x02C
    SPELL_FAILED_MOVING = 0x2E,                                      // 0x02D
    SPELL_FAILED_NEED_AMMO = 0x2F,                                   // 0x02E
    SPELL_FAILED_NEED_REQUIRES_SOMETHING = 0x30,                     // 0x02F
    SPELL_FAILED_NEED_EXOTIC_AMMO = 0x31,                            // 0x030
    SPELL_FAILED_NOPATH = 0x32,                                      // 0x031
    SPELL_FAILED_NOT_BEHIND = 0x33,                                  // 0x032
    SPELL_FAILED_NOT_FISHABLE = 0x34,                                // 0x033
    SPELL_FAILED_NOT_HERE = 0x35,                                    // 0x034
    SPELL_FAILED_NOT_INFRONT = 0x36,                                 // 0x035
    SPELL_FAILED_NOT_IN_CONTROL = 0x37,                              // 0x036
    SPELL_FAILED_NOT_KNOWN = 0x38,                                   // 0x037
    SPELL_FAILED_NOT_MOUNTED = 0x39,                                 // 0x038
    SPELL_FAILED_NOT_ON_TAXI = 0x3A,                                 // 0x039
    SPELL_FAILED_NOT_ON_TRANSPORT = 0x3B,                            // 0x03A
    SPELL_FAILED_NOT_READY = 0x3C,                                   // 0x03B
    SPELL_FAILED_NOT_SHAPESHIFT = 0x3D,                              // 0x03C
    SPELL_FAILED_NOT_STANDING = 0x3E,                                // 0x03D
    SPELL_FAILED_NOT_TRADEABLE = 0x3F,                               // 0x03E ' rogues trying "enchant" other's weapon with poison
    SPELL_FAILED_NOT_TRADING = 0x40,                                 // 0x03F ' CAST_FAIL_CANT_ENCHANT_TRADE_ITEM
    SPELL_FAILED_NOT_UNSHEATHED = 0x41,                              // 0x040 ' yellow text
    SPELL_FAILED_NOT_WHILE_GHOST = 0x42,                             // 0x041
    SPELL_FAILED_NO_AMMO = 0x43,                                     // 0x042
    SPELL_FAILED_NO_CHARGES_REMAIN = 0x44,                           // 0x043
    SPELL_FAILED_NO_CHAMPION = 0x45,                                 // 0x044 ' CAST_FAIL_NOT_SELECT
    SPELL_FAILED_NO_COMBO_POINTS = 0x46,                             // 0x045
    SPELL_FAILED_NO_DUELING = 0x47,                                  // 0x046
    SPELL_FAILED_NO_ENDURANCE = 0x48,                                // 0x047
    SPELL_FAILED_NO_FISH = 0x49,                                     // 0x048
    SPELL_FAILED_NO_ITEMS_WHILE_SHAPESHIFTED = 0x4A,                 // 0x049
    SPELL_FAILED_NO_MOUNTS_ALLOWED = 0x4B,                           // 0x04A
    SPELL_FAILED_NO_PET = 0x4C,                                      // 0x04B
    SPELL_FAILED_NO_POWER = 0x4D,                                    // 0x04C ' CAST_FAIL_NOT_ENOUGH_MANA
    SPELL_FAILED_NOTHING_TO_DISPEL = 0x4E,                           // 0x04D
    SPELL_FAILED_NOTHING_TO_STEAL = 0x4F,
    SPELL_FAILED_ONLY_ABOVEWATER = 0x50,                             // 0x04E ' CAST_FAIL_CANT_USE_WHILE_SWIMMING
    SPELL_FAILED_ONLY_DAYTIME = 0x51,                                // 0x04F
    SPELL_FAILED_ONLY_INDOORS = 0x52,                                // 0x050
    SPELL_FAILED_ONLY_MOUNTED = 0x53,                                // 0x051
    SPELL_FAILED_ONLY_NIGHTTIME = 0x54,                              // 0x052
    SPELL_FAILED_ONLY_OUTDOORS = 0x55,                               // 0x053
    SPELL_FAILED_ONLY_SHAPESHIFT = 0x56,                             // 0x054
    SPELL_FAILED_ONLY_STEALTHED = 0x57,                              // 0x055
    SPELL_FAILED_ONLY_UNDERWATER = 0x58,                             // 0x056 ' CAST_FAIL_CAN_ONLY_USE_WHILE_SWIMMING
    SPELL_FAILED_OUT_OF_RANGE = 0x59,                                // 0x057
    SPELL_FAILED_PACIFIED = 0x5,                                    // 0x058
    SPELL_FAILED_POSSESSED = 0x5B,                                   // 0x059

    // SPELL_FAILED_REAGENTS = &H5C                                   ' 0x05A ' [-ZERO] not in 1.12
    SPELL_FAILED_REQUIRES_AREA = 0x5D,                               // 0x05B ' CAST_FAIL_YOU_NEED_TO_BE_IN_XXX

    SPELL_FAILED_REQUIRES_SPELL_FOCUS = 0x5E,                        // 0x05C ' CAST_FAIL_REQUIRES_XXX
    SPELL_FAILED_ROOTED = 0x5F,                                      // 0x05D ' CAST_FAIL_UNABLE_TO_MOVE
    SPELL_FAILED_SILENCED = 0x60,                                    // 0x05E
    SPELL_FAILED_SPELL_IN_PROGRESS = 0x61,                           // 0x05F
    SPELL_FAILED_SPELL_LEARNED = 0x62,                               // 0x060
    SPELL_FAILED_SPELL_UNAVAILABLE = 0x63,                           // 0x061
    SPELL_FAILED_STUNNED = 0x64,                                     // 0x062
    SPELL_FAILED_TARGETS_DEAD = 0x65,                                // 0x063
    SPELL_FAILED_TARGET_AFFECTING_COMBAT = 0x66,                     // 0x064
    SPELL_FAILED_TARGET_AURASTATE = 0x67,                            // 0x065 ' CAST_FAIL_CANT_DO_THAT_YET_2
    SPELL_FAILED_TARGET_DUELING = 0x68,                              // 0x066
    SPELL_FAILED_TARGET_ENEMY = 0x69,                                // 0x067
    SPELL_FAILED_TARGET_ENRAGED = 0x6A,                              // 0x068 ' CAST_FAIL_TARGET_IS_TOO_ENRAGED_TO_CHARM
    SPELL_FAILED_TARGET_FRIENDLY = 0x6B,                             // 0x069
    SPELL_FAILED_TARGET_IN_COMBAT = 0x6C,                            // 0x06A
    SPELL_FAILED_TARGET_IS_PLAYER = 0x6D,                            // 0x06B
    SPELL_FAILED_TARGET_NOT_DEAD = 0x6E,                             // 0x06C
    SPELL_FAILED_TARGET_NOT_IN_PARTY = 0x6F,                         // 0x06D
    SPELL_FAILED_TARGET_NOT_LOOTED = 0x70,                           // 0x06E ' CAST_FAIL_CREATURE_MUST_BE_LOOTED_FIRST
    SPELL_FAILED_TARGET_NOT_PLAYER = 0x71,                           // 0x06F
    SPELL_FAILED_TARGET_NO_POCKETS = 0x72,                           // 0x070 ' CAST_FAIL_NOT_ITEM_TO_STEAL
    SPELL_FAILED_TARGET_NO_WEAPONS = 0x73,                           // 0x071
    SPELL_FAILED_TARGET_UNSKINNABLE = 0x74,                          // 0x072
    SPELL_FAILED_THIRST_SATIATED = 0x75,                             // 0x073
    SPELL_FAILED_TOO_CLOSE = 0x76,                                   // 0x074
    SPELL_FAILED_TOO_MANY_OF_ITEM = 0x77,                            // 0x075

    // SPELL_FAILED_TOTEMS = &H78                                     ' 0x076 ' [-ZERO] not in 1.12
    SPELL_FAILED_TRAINING_POINTS = 0x79,                             // 0x077

    SPELL_FAILED_TRY_AGAIN = 0x7A,                                   // 0x078 ' CAST_FAIL_FAILED_ATTEMPT
    SPELL_FAILED_UNIT_NOT_BEHIND = 0x7B,                             // 0x079
    SPELL_FAILED_UNIT_NOT_INFRONT = 0x7C,                            // 0x07A
    SPELL_FAILED_WRONG_PET_FOOD = 0x7D,                              // 0x07B
    SPELL_FAILED_NOT_WHILE_FATIGUED = 0x7E,                          // 0x07C
    SPELL_FAILED_TARGET_NOT_IN_INSTANCE = 0x7F,                      // 0x07D ' CAST_FAIL_TARGET_MUST_BE_IN_THIS_INSTANCE
    SPELL_FAILED_NOT_WHILE_TRADING = 0x80,                           // 0x07E
    SPELL_FAILED_TARGET_NOT_IN_RAID = 0x81,                          // 0x07F
    SPELL_FAILED_DISENCHANT_WHILE_LOOTING = 0x82,                    // 0x080
    SPELL_FAILED_PROSPECT_WHILE_LOOTING = 0x83,

    // SPELL_FAILED_PROSPECT_NEED_MORE = &H85
    SPELL_FAILED_TARGET_FREEFORALL = 0x85,                           // 0x081

    SPELL_FAILED_NO_EDIBLE_CORPSES = 0x86,                           // 0x082
    SPELL_FAILED_ONLY_BATTLEGROUNDS = 0x87,                          // 0x083
    SPELL_FAILED_TARGET_NOT_GHOST = 0x88,                            // 0x084
    SPELL_FAILED_TOO_MANY_SKILLS = 0x89,                             // 0x085 ' CAST_FAIL_YOUR_PET_CANT_LEARN_MORE_SKILLS
    SPELL_FAILED_CANT_USE_NEW_ITEM = 0x8A,                           // 0x086
    SPELL_FAILED_WRONG_WEATHER = 0x8B,                               // 0x087 ' CAST_FAIL_CANT_DO_IN_THIS_WEATHER
    SPELL_FAILED_DAMAGE_IMMUNE = 0x8C,                               // 0x088 ' CAST_FAIL_CANT_DO_IN_IMMUNE
    SPELL_FAILED_PREVENTED_BY_MECHANIC = 0x8D,                       // 0x089 ' CAST_FAIL_CANT_DO_IN_XXX
    SPELL_FAILED_PLAY_TIME = 0x8E,                                   // 0x08A ' CAST_FAIL_GAME_TIME_OVER
    SPELL_FAILED_REPUTATION = 0x8F,                                  // 0x08B
    SPELL_FAILED_MIN_SKILL = 0x90,
    SPELL_FAILED_UNKNOWN = 0x91,                                     // 0x08C
    SPELL_NO_ERROR = 0xFF                                           // 0x0FF
}
