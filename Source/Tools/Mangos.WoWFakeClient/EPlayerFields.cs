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

namespace Mangos.WoWFakeClient;

public enum EPlayerFields
{
    PLAYER_DUEL_ARBITER = EUnitFields.UNIT_END + 0x0,                              // 0x0B8 - Size: 2 - Type: GUID - Flags: PUBLIC
    PLAYER_FLAGS = EUnitFields.UNIT_END + 0x2,                                     // 0x0BA - Size: 1 - Type: INT - Flags: PUBLIC
    PLAYER_GUILDID = EUnitFields.UNIT_END + 0x3,                                   // 0x0BB - Size: 1 - Type: INT - Flags: PUBLIC
    PLAYER_GUILDRANK = EUnitFields.UNIT_END + 0x4,                                 // 0x0BC - Size: 1 - Type: INT - Flags: PUBLIC
    PLAYER_BYTES = EUnitFields.UNIT_END + 0x5,                                     // 0x0BD - Size: 1 - Type: BYTES - Flags: PUBLIC
    PLAYER_BYTES_2 = EUnitFields.UNIT_END + 0x6,                                   // 0x0BE - Size: 1 - Type: BYTES - Flags: PUBLIC
    PLAYER_BYTES_3 = EUnitFields.UNIT_END + 0x7,                                   // 0x0BF - Size: 1 - Type: BYTES - Flags: PUBLIC
    PLAYER_DUEL_TEAM = EUnitFields.UNIT_END + 0x8,                                 // 0x0C0 - Size: 1 - Type: INT - Flags: PUBLIC
    PLAYER_GUILD_TIMESTAMP = EUnitFields.UNIT_END + 0x9,                           // 0x0C1 - Size: 1 - Type: INT - Flags: PUBLIC
    PLAYER_QUEST_LOG_1_1 = EUnitFields.UNIT_END + 0xA,                             // 0x0C2 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    PLAYER_QUEST_LOG_1_2 = EUnitFields.UNIT_END + 0xB,                             // 0x0C3 - Size: 2 - Type: INT - Flags: PRIVATE
    PLAYER_QUEST_LOG_1_3 = EUnitFields.UNIT_END + 0xC,
    PLAYER_QUEST_LOG_LAST_1 = EUnitFields.UNIT_END + 0x43,
    PLAYER_QUEST_LOG_LAST_2 = EUnitFields.UNIT_END + 0x44,
    PLAYER_QUEST_LOG_LAST_3 = EUnitFields.UNIT_END + 0x45,
    PLAYER_VISIBLE_ITEM_1_CREATOR = EUnitFields.UNIT_END + 0x46,                   // 0x0FE - Size: 2 - Type: GUID - Flags: PUBLIC
    PLAYER_VISIBLE_ITEM_1_0 = EUnitFields.UNIT_END + 0x48,                         // 0x100 - Size: 8 - Type: INT - Flags: PUBLIC
    PLAYER_VISIBLE_ITEM_1_PROPERTIES = EUnitFields.UNIT_END + 0x50,                // 0x108 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    PLAYER_VISIBLE_ITEM_1_PAD = EUnitFields.UNIT_END + 0x51,                       // 0x109 - Size: 1 - Type: INT - Flags: PUBLIC
    PLAYER_VISIBLE_ITEM_LAST_CREATOR = EUnitFields.UNIT_END + 0x11E,
    PLAYER_VISIBLE_ITEM_LAST_0 = EUnitFields.UNIT_END + 0x120,
    PLAYER_VISIBLE_ITEM_LAST_PROPERTIES = EUnitFields.UNIT_END + 0x128,
    PLAYER_VISIBLE_ITEM_LAST_PAD = EUnitFields.UNIT_END + 0x129,
    PLAYER_FIELD_INV_SLOT_HEAD = EUnitFields.UNIT_END + 0x12A,                     // 0x1E2 - Size: 46 - Type: GUID - Flags: PRIVATE
    PLAYER_FIELD_PACK_SLOT_1 = EUnitFields.UNIT_END + 0x158,                       // 0x210 - Size: 32 - Type: GUID - Flags: PRIVATE
    PLAYER_FIELD_PACK_SLOT_LAST = EUnitFields.UNIT_END + 0x176,
    PLAYER_FIELD_BANK_SLOT_1 = EUnitFields.UNIT_END + 0x178,                       // 0x230 - Size: 48 - Type: GUID - Flags: PRIVATE
    PLAYER_FIELD_BANK_SLOT_LAST = EUnitFields.UNIT_END + 0x1A6,
    PLAYER_FIELD_BANKBAG_SLOT_1 = EUnitFields.UNIT_END + 0x1A8,                    // 0x260 - Size: 12 - Type: GUID - Flags: PRIVATE
    PLAYER_FIELD_BANKBAG_SLOT_LAST = EUnitFields.UNIT_END + 0xAB2,
    PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = EUnitFields.UNIT_END + 0x1B4,              // 0x26C - Size: 24 - Type: GUID - Flags: PRIVATE
    PLAYER_FIELD_VENDORBUYBACK_SLOT_LAST = EUnitFields.UNIT_END + 0x1CA,
    PLAYER_FIELD_KEYRING_SLOT_1 = EUnitFields.UNIT_END + 0x1CC,                    // 0x284 - Size: 64 - Type: GUID - Flags: PRIVATE
    PLAYER_FIELD_KEYRING_SLOT_LAST = EUnitFields.UNIT_END + 0x20A,
    PLAYER_FARSIGHT = EUnitFields.UNIT_END + 0x20C,                                // 0x2C4 - Size: 2 - Type: GUID - Flags: PRIVATE
    PLAYER_FIELD_COMBO_TARGET = EUnitFields.UNIT_END + 0x20E,                      // 0x2C6 - Size: 2 - Type: GUID - Flags: PRIVATE
    PLAYER_XP = EUnitFields.UNIT_END + 0x210,                                      // 0x2C8 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_NEXT_LEVEL_XP = EUnitFields.UNIT_END + 0x211,                           // 0x2C9 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_SKILL_INFO_1_1 = EUnitFields.UNIT_END + 0x212,                          // 0x2CA - Size: 384 - Type: TWO_SHORT - Flags: PRIVATE
    PLAYER_CHARACTER_POINTS1 = EUnitFields.UNIT_END + 0x392,                       // 0x44A - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_CHARACTER_POINTS2 = EUnitFields.UNIT_END + 0x393,                       // 0x44B - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_TRACK_CREATURES = EUnitFields.UNIT_END + 0x394,                         // 0x44C - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_TRACK_RESOURCES = EUnitFields.UNIT_END + 0x395,                         // 0x44D - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_BLOCK_PERCENTAGE = EUnitFields.UNIT_END + 0x396,                        // 0x44E - Size: 1 - Type: FLOAT - Flags: PRIVATE
    PLAYER_DODGE_PERCENTAGE = EUnitFields.UNIT_END + 0x397,                        // 0x44F - Size: 1 - Type: FLOAT - Flags: PRIVATE
    PLAYER_PARRY_PERCENTAGE = EUnitFields.UNIT_END + 0x398,                        // 0x450 - Size: 1 - Type: FLOAT - Flags: PRIVATE
    PLAYER_CRIT_PERCENTAGE = EUnitFields.UNIT_END + 0x399,                         // 0x451 - Size: 1 - Type: FLOAT - Flags: PRIVATE
    PLAYER_RANGED_CRIT_PERCENTAGE = EUnitFields.UNIT_END + 0x39A,                  // 0x452 - Size: 1 - Type: FLOAT - Flags: PRIVATE
    PLAYER_EXPLORED_ZONES_1 = EUnitFields.UNIT_END + 0x39B,                        // 0x453 - Size: 64 - Type: BYTES - Flags: PRIVATE
    PLAYER_REST_STATE_EXPERIENCE = EUnitFields.UNIT_END + 0x3DB,                   // 0x493 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_COINAGE = EUnitFields.UNIT_END + 0x3DC,                           // 0x494 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_POSSTAT0 = EUnitFields.UNIT_END + 0x3DD,                          // 0x495 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_POSSTAT1 = EUnitFields.UNIT_END + 0x3DE,                          // 0x496 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_POSSTAT2 = EUnitFields.UNIT_END + 0x3DF,                          // 0x497 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_POSSTAT3 = EUnitFields.UNIT_END + 0x3E0,                          // 0x498 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_POSSTAT4 = EUnitFields.UNIT_END + 0x3E1,                          // 0x499 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_NEGSTAT0 = EUnitFields.UNIT_END + 0x3E2,                          // 0x49A - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_NEGSTAT1 = EUnitFields.UNIT_END + 0x3E3,                          // 0x49B - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_NEGSTAT2 = EUnitFields.UNIT_END + 0x3E4,                          // 0x49C - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_NEGSTAT3 = EUnitFields.UNIT_END + 0x3E5,                          // 0x49D - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_NEGSTAT4 = EUnitFields.UNIT_END + 0x3E6,                          // 0x49E - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = EUnitFields.UNIT_END + 0x3E7,        // 0x49F - Size: 7 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = EUnitFields.UNIT_END + 0x3EE,        // 0x4A6 - Size: 7 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_MOD_DAMAGE_DONE_POS = EUnitFields.UNIT_END + 0x3F5,               // 0x4AD - Size: 7 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = EUnitFields.UNIT_END + 0x3FC,               // 0x4B4 - Size: 7 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = EUnitFields.UNIT_END + 0x403,               // 0x4BB - Size: 7 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_BYTES = EUnitFields.UNIT_END + 0x40A,                             // 0x4C2 - Size: 1 - Type: BYTES - Flags: PRIVATE
    PLAYER_AMMO_ID = EUnitFields.UNIT_END + 0x40B,                                 // 0x4C3 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_SELF_RES_SPELL = EUnitFields.UNIT_END + 0x40C,                          // 0x4C4 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_PVP_MEDALS = EUnitFields.UNIT_END + 0x40D,                        // 0x4C5 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_BUYBACK_PRICE_1 = EUnitFields.UNIT_END + 0x40E,                   // 0x4C6 - Size: 12 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_BUYBACK_PRICE_LAST = EUnitFields.UNIT_END + 0x419,
    PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = EUnitFields.UNIT_END + 0x41A,               // 0x4D2 - Size: 12 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_BUYBACK_TIMESTAMP_LAST = EUnitFields.UNIT_END + 0x425,
    PLAYER_FIELD_SESSION_KILLS = EUnitFields.UNIT_END + 0x426,                     // 0x4DE - Size: 1 - Type: TWO_SHORT - Flags: PRIVATE
    PLAYER_FIELD_YESTERDAY_KILLS = EUnitFields.UNIT_END + 0x427,                   // 0x4DF - Size: 1 - Type: TWO_SHORT - Flags: PRIVATE
    PLAYER_FIELD_LAST_WEEK_KILLS = EUnitFields.UNIT_END + 0x428,                   // 0x4E0 - Size: 1 - Type: TWO_SHORT - Flags: PRIVATE
    PLAYER_FIELD_THIS_WEEK_KILLS = EUnitFields.UNIT_END + 0x429,                   // 0x4E1 - Size: 1 - Type: TWO_SHORT - Flags: PRIVATE
    PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = EUnitFields.UNIT_END + 0x42A,            // 0x4E2 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = EUnitFields.UNIT_END + 0x42B,          // 0x4E3 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_LIFETIME_DISHONORBALE_KILLS = EUnitFields.UNIT_END + 0x42C,       // 0x4E4 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_YESTERDAY_CONTRIBUTION = EUnitFields.UNIT_END + 0x42D,            // 0x4E5 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = EUnitFields.UNIT_END + 0x42E,            // 0x4E6 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_LAST_WEEK_RANK = EUnitFields.UNIT_END + 0x42F,                    // 0x4E7 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_BYTES2 = EUnitFields.UNIT_END + 0x430,                            // 0x4E8 - Size: 1 - Type: BYTES - Flags: PRIVATE
    PLAYER_FIELD_WATCHED_FACTION_INDEX = EUnitFields.UNIT_END + 0x431,             // 0x4E9 - Size: 1 - Type: INT - Flags: PRIVATE
    PLAYER_FIELD_COMBAT_RATING_1 = EUnitFields.UNIT_END + 0x432,
    PLAYER_END = EUnitFields.UNIT_END + 0x446                                     // 0x4EA
}
