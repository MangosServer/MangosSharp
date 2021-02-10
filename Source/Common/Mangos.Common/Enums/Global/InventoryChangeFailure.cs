﻿//
//  Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
//
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

namespace Mangos.Common.Enums.Global
{
    public enum InventoryChangeFailure : byte
    {
        EQUIP_ERR_OK = 0,
        EQUIP_ERR_YOU_MUST_REACH_LEVEL_N = 1,
        EQUIP_ERR_SKILL_ISNT_HIGH_ENOUGH = 2,
        EQUIP_ERR_ITEM_DOESNT_GO_TO_SLOT = 3,
        EQUIP_ERR_BAG_FULL = 4,
        EQUIP_ERR_NONEMPTY_BAG_OVER_OTHER_BAG = 5,
        EQUIP_ERR_CANT_TRADE_EQUIP_BAGS = 6,
        EQUIP_ERR_ONLY_AMMO_CAN_GO_HERE = 7,
        EQUIP_ERR_NO_REQUIRED_PROFICIENCY = 8,
        EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE = 9,
        EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM = 10,
        EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM2 = 11,
        EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE2 = 12,
        EQUIP_ERR_CANT_EQUIP_WITH_TWOHANDED = 13,
        EQUIP_ERR_CANT_DUAL_WIELD = 14,
        EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG = 15,
        EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG2 = 16,
        EQUIP_ERR_CANT_CARRY_MORE_OF_THIS = 17,
        EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE3 = 18,
        EQUIP_ERR_ITEM_CANT_STACK = 19,
        EQUIP_ERR_ITEM_CANT_BE_EQUIPPED = 20,
        EQUIP_ERR_ITEMS_CANT_BE_SWAPPED = 21,
        EQUIP_ERR_SLOT_IS_EMPTY = 22,
        EQUIP_ERR_ITEM_NOT_FOUND = 23,
        EQUIP_ERR_CANT_DROP_SOULBOUND = 24,
        EQUIP_ERR_OUT_OF_RANGE = 25,
        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT = 26,
        EQUIP_ERR_COULDNT_SPLIT_ITEMS = 27,
        EQUIP_ERR_MISSING_REAGENT = 28,
        EQUIP_ERR_NOT_ENOUGH_MONEY = 29,
        EQUIP_ERR_NOT_A_BAG = 30,
        EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS = 31,
        EQUIP_ERR_DONT_OWN_THAT_ITEM = 32,
        EQUIP_ERR_CAN_EQUIP_ONLY1_QUIVER = 33,
        EQUIP_ERR_MUST_PURCHASE_THAT_BAG_SLOT = 34,
        EQUIP_ERR_TOO_FAR_AWAY_FROM_BANK = 35,
        EQUIP_ERR_ITEM_LOCKED = 36,
        EQUIP_ERR_YOU_ARE_STUNNED = 37,
        EQUIP_ERR_YOU_ARE_DEAD = 38,
        EQUIP_ERR_CANT_DO_RIGHT_NOW = 39,
        EQUIP_ERR_BAG_FULL2 = 40,
        EQUIP_ERR_CAN_EQUIP_ONLY1_QUIVER2 = 41,
        EQUIP_ERR_CAN_EQUIP_ONLY1_AMMOPOUCH = 42,
        EQUIP_ERR_STACKABLE_CANT_BE_WRAPPED = 43,
        EQUIP_ERR_EQUIPPED_CANT_BE_WRAPPED = 44,
        EQUIP_ERR_WRAPPED_CANT_BE_WRAPPED = 45,
        EQUIP_ERR_BOUND_CANT_BE_WRAPPED = 46,
        EQUIP_ERR_UNIQUE_CANT_BE_WRAPPED = 47,
        EQUIP_ERR_BAGS_CANT_BE_WRAPPED = 48,
        EQUIP_ERR_ALREADY_LOOTED = 49,
        EQUIP_ERR_INVENTORY_FULL = 50,
        EQUIP_ERR_BANK_FULL = 51,
        EQUIP_ERR_ITEM_IS_CURRENTLY_SOLD_OUT = 52,
        EQUIP_ERR_BAG_FULL3 = 53,
        EQUIP_ERR_ITEM_NOT_FOUND2 = 54,
        EQUIP_ERR_ITEM_CANT_STACK2 = 55,
        EQUIP_ERR_BAG_FULL4 = 56,
        EQUIP_ERR_ITEM_SOLD_OUT = 57,
        EQUIP_ERR_OBJECT_IS_BUSY = 58,
        EQUIP_ERR_NONE = 59,
        EQUIP_ERR_CANT_DO_IN_COMBAT = 60,
        EQUIP_CANT_DO_WHILE_DISARMED = 61,
        EQUIP_ERR_BAG_FULL6 = 62,
        EQUIP_ITEM_RANK_NOT_ENOUGH = 63,
        EQUIP_ITEM_REPUTATION_NOT_ENOUGH = 64,
        EQUIP_MORE_THAN1_SPECIAL_BAG = 65
    }
}