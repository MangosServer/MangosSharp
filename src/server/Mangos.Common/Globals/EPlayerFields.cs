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

namespace Mangos.Common.Globals;

public enum EPlayerFields
{
    // PLAYER_SELECTION = EUnitFields.UNIT_END + &H0                                 ' 0x0B6 - Size: 2 - Type: GUID - Flags: PUBLIC
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

    // PLAYER_QUEST_LOG_2_1 = EUnitFields.UNIT_END + &HF                             ' 0x0C5 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_2_2 = EUnitFields.UNIT_END + &H10                            ' 0x0C6 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_3_1 = EUnitFields.UNIT_END + &H12                            ' 0x0C8 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_3_2 = EUnitFields.UNIT_END + &H13                            ' 0x0C9 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_4_1 = EUnitFields.UNIT_END + &H15                            ' 0x0CB - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_4_2 = EUnitFields.UNIT_END + &H16                            ' 0x0CC - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_5_1 = EUnitFields.UNIT_END + &H18                            ' 0x0CE - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_5_2 = EUnitFields.UNIT_END + &H19                            ' 0x0CF - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_6_1 = EUnitFields.UNIT_END + &H1B                            ' 0x0D1 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_6_2 = EUnitFields.UNIT_END + &H1C                            ' 0x0D2 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_7_1 = EUnitFields.UNIT_END + &H1E                            ' 0x0D4 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_7_2 = EUnitFields.UNIT_END + &H1F                            ' 0x0D5 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_8_1 = EUnitFields.UNIT_END + &H21                            ' 0x0D7 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_8_2 = EUnitFields.UNIT_END + &H22                            ' 0x0D8 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_9_1 = EUnitFields.UNIT_END + &H24                            ' 0x0DA - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_9_2 = EUnitFields.UNIT_END + &H25                            ' 0x0DB - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_10_1 = EUnitFields.UNIT_END + &H27                           ' 0x0DD - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_10_2 = EUnitFields.UNIT_END + &H28                           ' 0x0DE - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_11_1 = EUnitFields.UNIT_END + &H2A                           ' 0x0E0 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_11_2 = EUnitFields.UNIT_END + &H2B                           ' 0x0E1 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_12_1 = EUnitFields.UNIT_END + &H2D                           ' 0x0E3 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_12_2 = EUnitFields.UNIT_END + &H2E                           ' 0x0E4 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_13_1 = EUnitFields.UNIT_END + &H30                           ' 0x0E6 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_13_2 = EUnitFields.UNIT_END + &H31                           ' 0x0E7 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_14_1 = EUnitFields.UNIT_END + &H33                           ' 0x0E9 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_14_2 = EUnitFields.UNIT_END + &H34                           ' 0x0EA - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_15_1 = EUnitFields.UNIT_END + &H36                           ' 0x0EC - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_15_2 = EUnitFields.UNIT_END + &H37                           ' 0x0ED - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_16_1 = EUnitFields.UNIT_END + &H39                           ' 0x0EF - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_16_2 = EUnitFields.UNIT_END + &H3A                           ' 0x0F0 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_17_1 = EUnitFields.UNIT_END + &H3C                           ' 0x0F2 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_17_2 = EUnitFields.UNIT_END + &H3D                           ' 0x0F3 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_18_1 = EUnitFields.UNIT_END + &H3F                           ' 0x0F5 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_18_2 = EUnitFields.UNIT_END + &H40                           ' 0x0F6 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_19_1 = EUnitFields.UNIT_END + &H42                           ' 0x0F8 - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_19_2 = EUnitFields.UNIT_END + &H43                           ' 0x0F9 - Size: 2 - Type: INT - Flags: PRIVATE
    // PLAYER_QUEST_LOG_20_1 = EUnitFields.UNIT_END + &H45                           ' 0x0FB - Size: 1 - Type: INT - Flags: GROUP_ONLY
    // PLAYER_QUEST_LOG_20_2 = EUnitFields.UNIT_END + &H46                           ' 0x0FC - Size: 2 - Type: INT - Flags: PRIVATE
    PLAYER_QUEST_LOG_LAST_1 = EUnitFields.UNIT_END + 0x43,

    PLAYER_QUEST_LOG_LAST_2 = EUnitFields.UNIT_END + 0x44,
    PLAYER_QUEST_LOG_LAST_3 = EUnitFields.UNIT_END + 0x45,
    PLAYER_VISIBLE_ITEM_1_CREATOR = EUnitFields.UNIT_END + 0x46,                   // 0x0FE - Size: 2 - Type: GUID - Flags: PUBLIC
    PLAYER_VISIBLE_ITEM_1_0 = EUnitFields.UNIT_END + 0x48,                         // 0x100 - Size: 8 - Type: INT - Flags: PUBLIC
    PLAYER_VISIBLE_ITEM_1_PROPERTIES = EUnitFields.UNIT_END + 0x50,                // 0x108 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    PLAYER_VISIBLE_ITEM_1_PAD = EUnitFields.UNIT_END + 0x51,                       // 0x109 - Size: 1 - Type: INT - Flags: PUBLIC

    // PLAYER_VISIBLE_ITEM_2_CREATOR = EUnitFields.UNIT_END + &H54                   ' 0x10A - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_2_0 = EUnitFields.UNIT_END + &H56                         ' 0x10C - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_2_PROPERTIES = EUnitFields.UNIT_END + &H5E                ' 0x114 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_2_PAD = EUnitFields.UNIT_END + &H5F                       ' 0x115 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_3_CREATOR = EUnitFields.UNIT_END + &H60                   ' 0x116 - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_3_0 = EUnitFields.UNIT_END + &H62                         ' 0x118 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_3_PROPERTIES = EUnitFields.UNIT_END + &H6A                ' 0x120 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_3_PAD = EUnitFields.UNIT_END + &H6B                       ' 0x121 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_4_CREATOR = EUnitFields.UNIT_END + &H6C                   ' 0x122 - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_4_0 = EUnitFields.UNIT_END + &H6E                         ' 0x124 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_4_PROPERTIES = EUnitFields.UNIT_END + &H76                ' 0x12C - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_4_PAD = EUnitFields.UNIT_END + &H77                       ' 0x12D - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_5_CREATOR = EUnitFields.UNIT_END + &H78                   ' 0x12E - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_5_0 = EUnitFields.UNIT_END + &H7A                         ' 0x130 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_5_PROPERTIES = EUnitFields.UNIT_END + &H82                ' 0x138 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_5_PAD = EUnitFields.UNIT_END + &H83                       ' 0x139 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_6_CREATOR = EUnitFields.UNIT_END + &H84                   ' 0x13A - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_6_0 = EUnitFields.UNIT_END + &H86                         ' 0x13C - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_6_PROPERTIES = EUnitFields.UNIT_END + &H8E                ' 0x144 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_6_PAD = EUnitFields.UNIT_END + &H8F                       ' 0x145 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_7_CREATOR = EUnitFields.UNIT_END + &H90                   ' 0x146 - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_7_0 = EUnitFields.UNIT_END + &H92                         ' 0x148 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_7_PROPERTIES = EUnitFields.UNIT_END + &H9A                ' 0x150 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_7_PAD = EUnitFields.UNIT_END + &H9B                       ' 0x151 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_8_CREATOR = EUnitFields.UNIT_END + &H9C                   ' 0x152 - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_8_0 = EUnitFields.UNIT_END + &H9E                         ' 0x154 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_8_PROPERTIES = EUnitFields.UNIT_END + &HA6                ' 0x15C - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_8_PAD = EUnitFields.UNIT_END + &HA7                       ' 0x15D - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_9_CREATOR = EUnitFields.UNIT_END + &HA8                   ' 0x15E - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_9_0 = EUnitFields.UNIT_END + &HAA                         ' 0x160 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_9_PROPERTIES = EUnitFields.UNIT_END + &HB2                ' 0x168 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_9_PAD = EUnitFields.UNIT_END + &HB3                       ' 0x169 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_10_CREATOR = EUnitFields.UNIT_END + &HB4                  ' 0x16A - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_10_0 = EUnitFields.UNIT_END + &HB6                        ' 0x16C - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_10_PROPERTIES = EUnitFields.UNIT_END + &HBE               ' 0x174 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_10_PAD = EUnitFields.UNIT_END + &HBF                      ' 0x175 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_11_CREATOR = EUnitFields.UNIT_END + &HC0                  ' 0x176 - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_11_0 = EUnitFields.UNIT_END + &HC2                        ' 0x178 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_11_PROPERTIES = EUnitFields.UNIT_END + &HCA               ' 0x180 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_11_PAD = EUnitFields.UNIT_END + &HCB                      ' 0x181 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_12_CREATOR = EUnitFields.UNIT_END + &HCC                  ' 0x182 - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_12_0 = EUnitFields.UNIT_END + &HCE                        ' 0x184 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_12_PROPERTIES = EUnitFields.UNIT_END + &HD6               ' 0x18C - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_12_PAD = EUnitFields.UNIT_END + &HD7                      ' 0x18D - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_13_CREATOR = EUnitFields.UNIT_END + &HD8                  ' 0x18E - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_13_0 = EUnitFields.UNIT_END + &HDA                        ' 0x190 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_13_PROPERTIES = EUnitFields.UNIT_END + &HE2               ' 0x198 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_13_PAD = EUnitFields.UNIT_END + &HE3                      ' 0x199 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_14_CREATOR = EUnitFields.UNIT_END + &HE4                  ' 0x19A - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_14_0 = EUnitFields.UNIT_END + &HE6                        ' 0x19C - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_14_PROPERTIES = EUnitFields.UNIT_END + &HEE               ' 0x1A4 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_14_PAD = EUnitFields.UNIT_END + &HEF                      ' 0x1A5 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_15_CREATOR = EUnitFields.UNIT_END + &HF0                  ' 0x1A6 - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_15_0 = EUnitFields.UNIT_END + &HF2                        ' 0x1A8 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_15_PROPERTIES = EUnitFields.UNIT_END + &HFA               ' 0x1B0 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_15_PAD = EUnitFields.UNIT_END + &HFB                      ' 0x1B1 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_16_CREATOR = EUnitFields.UNIT_END + &HFC                  ' 0x1B2 - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_16_0 = EUnitFields.UNIT_END + &HFE                        ' 0x1B4 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_16_PROPERTIES = EUnitFields.UNIT_END + &H106              ' 0x1BC - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_16_PAD = EUnitFields.UNIT_END + &H107                     ' 0x1BD - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_17_CREATOR = EUnitFields.UNIT_END + &H108                 ' 0x1BE - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_17_0 = EUnitFields.UNIT_END + &H10A                       ' 0x1C0 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_17_PROPERTIES = EUnitFields.UNIT_END + &H112              ' 0x1C8 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_17_PAD = EUnitFields.UNIT_END + &H113                     ' 0x1C9 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_18_CREATOR = EUnitFields.UNIT_END + &H114                 ' 0x1CA - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_18_0 = EUnitFields.UNIT_END + &H116                       ' 0x1CC - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_18_PROPERTIES = EUnitFields.UNIT_END + &H11E              ' 0x1D4 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_18_PAD = EUnitFields.UNIT_END + &H11F                     ' 0x1D5 - Size: 1 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_19_CREATOR = EUnitFields.UNIT_END + &H120                 ' 0x1D6 - Size: 2 - Type: GUID - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_19_0 = EUnitFields.UNIT_END + &H122                       ' 0x1D8 - Size: 8 - Type: INT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_19_PROPERTIES = EUnitFields.UNIT_END + &H12A              ' 0x1E0 - Size: 1 - Type: TWO_SHORT - Flags: PUBLIC
    // PLAYER_VISIBLE_ITEM_19_PAD = EUnitFields.UNIT_END + &H12B                     ' 0x1E1 - Size: 1 - Type: INT - Flags: PUBLIC
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
