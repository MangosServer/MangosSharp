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

namespace Mangos.Common.Enums.Item
{
    public enum ITEM_SUBCLASS : byte
    {
        // Consumable
        ITEM_SUBCLASS_CONSUMABLE = 0,

        ITEM_SUBCLASS_FOOD = 1,
        ITEM_SUBCLASS_LIQUID = 2,
        ITEM_SUBCLASS_POTION = 3,
        ITEM_SUBCLASS_SCROLL = 4,
        ITEM_SUBCLASS_BANDAGE = 5,
        ITEM_SUBCLASS_HEALTHSTONE = 6,
        ITEM_SUBCLASS_COMBAT_EFFECT = 7,

        // Container
        ITEM_SUBCLASS_BAG = 0,

        ITEM_SUBCLASS_SOUL_BAG = 1,
        ITEM_SUBCLASS_HERB_BAG = 2,
        ITEM_SUBCLASS_ENCHANTING_BAG = 3,

        // Weapon
        ITEM_SUBCLASS_AXE = 0,

        ITEM_SUBCLASS_TWOHAND_AXE = 1,
        ITEM_SUBCLASS_BOW = 2,
        ITEM_SUBCLASS_GUN = 3,
        ITEM_SUBCLASS_MACE = 4,
        ITEM_SUBCLASS_TWOHAND_MACE = 5,
        ITEM_SUBCLASS_POLEARM = 6,
        ITEM_SUBCLASS_SWORD = 7,
        ITEM_SUBCLASS_TWOHAND_SWORD = 8,
        ITEM_SUBCLASS_WEAPON_obsolete = 9,
        ITEM_SUBCLASS_STAFF = 10,
        ITEM_SUBCLASS_WEAPON_EXOTIC = 11,
        ITEM_SUBCLASS_WEAPON_EXOTIC2 = 12,
        ITEM_SUBCLASS_FIST_WEAPON = 13,
        ITEM_SUBCLASS_MISC_WEAPON = 14,
        ITEM_SUBCLASS_DAGGER = 15,
        ITEM_SUBCLASS_THROWN = 16,
        ITEM_SUBCLASS_SPEAR = 17,
        ITEM_SUBCLASS_CROSSBOW = 18,
        ITEM_SUBCLASS_WAND = 19,
        ITEM_SUBCLASS_FISHING_POLE = 20,

        // Armor
        ITEM_SUBCLASS_MISC = 0,

        ITEM_SUBCLASS_CLOTH = 1,
        ITEM_SUBCLASS_LEATHER = 2,
        ITEM_SUBCLASS_MAIL = 3,
        ITEM_SUBCLASS_PLATE = 4,
        ITEM_SUBCLASS_BUCKLER = 5,
        ITEM_SUBCLASS_SHIELD = 6,
        ITEM_SUBCLASS_LIBRAM = 7,
        ITEM_SUBCLASS_IDOL = 8,
        ITEM_SUBCLASS_TOTEM = 9,

        // Projectile
        ITEM_SUBCLASS_WAND_obslete = 0,

        ITEM_SUBCLASS_BOLT_obslete = 1,
        ITEM_SUBCLASS_ARROW = 2,
        ITEM_SUBCLASS_BULLET = 3,
        ITEM_SUBCLASS_THROWN_obslete = 4,

        // Trade goods
        ITEM_SUBCLASS_TRADE_GOODS = 0,

        ITEM_SUBCLASS_PARTS = 1,
        ITEM_SUBCLASS_EXPLOSIVES = 2,
        ITEM_SUBCLASS_DEVICES = 3,
        ITEM_SUBCLASS_GEMS = 4,
        ITEM_SUBCLASS_CLOTHS = 5,
        ITEM_SUBCLASS_LEATHERS = 6,
        ITEM_SUBCLASS_METAL_AND_STONE = 7,
        ITEM_SUBCLASS_MEAT = 8,
        ITEM_SUBCLASS_HERB = 9,
        ITEM_SUBCLASS_ELEMENTAL = 10,
        ITEM_SUBCLASS_OTHERS = 11,
        ITEM_SUBCLASS_ENCHANTANTS = 12,
        ITEM_SUBCLASS_MATERIALS = 13,

        // Recipe
        ITEM_SUBCLASS_BOOK = 0,

        ITEM_SUBCLASS_LEATHERWORKING = 1,
        ITEM_SUBCLASS_TAILORING = 2,
        ITEM_SUBCLASS_ENGINEERING = 3,
        ITEM_SUBCLASS_BLACKSMITHING = 4,
        ITEM_SUBCLASS_COOKING = 5,
        ITEM_SUBCLASS_ALCHEMY = 6,
        ITEM_SUBCLASS_FIRST_AID = 7,
        ITEM_SUBCLASS_ENCHANTING = 8,
        ITEM_SUBCLASS_FISHING = 9,
        ITEM_SUBCLASS_JEWELCRAFTING = 10,

        // Quiver
        ITEM_SUBCLASS_QUIVER0_obslete = 0,

        ITEM_SUBCLASS_QUIVER1_obslete = 1,
        ITEM_SUBCLASS_QUIVER = 2,
        ITEM_SUBCLASS_AMMO_POUCH = 3,

        // Keys
        ITEM_SUBCLASS_KEY = 0,

        ITEM_SUBCLASS_LOCKPICK = 1,

        // Misc
        ITEM_SUBCLASS_JUNK = 0,

        ITEM_SUBCLASS_REAGENT = 1,
        ITEM_SUBCLASS_PET = 2,
        ITEM_SUBCLASS_HOLIDAY = 3,
        ITEM_SUBCLASS_OTHER = 4,
        ITEM_SUBCLASS_MOUNT = 5
    }
}