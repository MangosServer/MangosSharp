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

public enum SKILL_IDs
{
    SKILL_NONE = 0,
    SKILL_FROST = 6,
    SKILL_FIRE = 8,
    SKILL_ARMS = 26,
    SKILL_COMBAT = 38,
    SKILL_SUBTLETY = 39,
    SKILL_POISONS = 40,
    SKILL_SWORDS = 43,                  // Higher weapon skill increases your chance to hit.
    SKILL_AXES = 44,                    // Higher weapon skill increases your chance to hit.
    SKILL_BOWS = 45,                    // Higher weapon skill increases your chance to hit.
    SKILL_GUNS = 46,                    // Higher weapon skill increases your chance to hit.
    SKILL_BEAST_MASTERY = 50,
    SKILL_SURVIVAL = 51,
    SKILL_MACES = 54,                   // Higher weapon skill increases your chance to hit.
    SKILL_TWO_HANDED_SWORDS = 55,       // Higher weapon skill increases your chance to hit.
    SKILL_HOLY = 56,
    SKILL_SHADOW_MAGIC = 78,
    SKILL_DEFENSE = 95,                 // Higher defense makes you harder to hit and makes monsters less likely to land a crushing blow.
    SKILL_LANGUAGE_COMMON = 98,
    SKILL_DWARVEN_RACIAL = 101,
    SKILL_LANGUAGE_ORCISH = 109,
    SKILL_LANGUAGE_DWARVEN = 111,
    SKILL_LANGUAGE_DARNASSIAN = 113,
    SKILL_LANGUAGE_TAURAHE = 115,
    SKILL_DUAL_WIELD = 118,
    SKILL_TAUREN_RACIAL = 124,
    SKILL_ORC_RACIAL = 125,
    SKILL_NIGHT_ELF_RACIAL = 126,
    SKILL_FIRST_AID = 129,               // Higher first aid skill allows you to learn higher level first aid abilities.  First aid abilities can be found on trainers around the world as well as from quests and as drops from monsters.
    SKILL_FERAL_COMBAT = 134,
    SKILL_STAVES = 136,                  // Higher weapon skill increases your chance to hit.
    SKILL_LANGUAGE_THALASSIAN = 137,
    SKILL_LANGUAGE_DRACONIC = 138,
    SKILL_LANGUAGE_DEMON_TONGUE = 139,
    SKILL_LANGUAGE_TITAN = 140,
    SKILL_LANGUAGE_OLD_TONGUE = 141,
    SKILL_SURVIVAL_1 = 142,
    SKILL_HORSE_RIDING = 148,
    SKILL_WOLF_RIDING = 149,
    SKILL_TIGER_RIDING = 150,
    SKILL_RAM_RIDING = 152,
    SKILL_SWIMMING = 155,
    SKILL_TWO_HANDED_MACES = 160,        // Higher weapon skill increases your chance to hit.
    SKILL_UNARMED = 162,                 // Higher skill increases your chance to hit.
    SKILL_MARKSMANSHIP = 163,
    SKILL_BLACKSMITHING = 164,           // Higher smithing skill allows you to learn higher level smithing plans.  Blacksmithing plans can be found on trainers around the world as well as from quests and monsters.
    SKILL_LEATHERWORKING = 165,          // Higher leatherworking skill allows you to learn higher level leatherworking patterns.  Leatherworking patterns can be found on trainers around the world as well as from quests and monsters.
    SKILL_ALCHEMY = 171,                 // Higher alchemy skill allows you to learn higher level alchemy recipes.  Alchemy recipes can be found on trainers around the world as well as from quests and monsters.
    SKILL_TWO_HANDED_AXES = 172,         // Higher weapon skill increases your chance to hit.
    SKILL_DAGGERS = 173,                 // Higher weapon skill increases your chance to hit.
    SKILL_THROWN = 176,                  // Higher weapon skill increases your chance to hit.
    SKILL_HERBALISM = 182,               // Higher herbalism skill allows you to harvest more difficult herbs around the world.  If you cannot harvest a specific herb
    SKILL_GENERIC_DND = 183,
    SKILL_RETRIBUTION = 184,
    SKILL_COOKING = 185,                 // Higher cooking skill allows you to learn higher level cooking recipes.  Recipes can be found on trainers around the world as well as from quests and as drops from monsters.
    SKILL_MINING = 186,                  // Higher mining skill allows you to harvest more difficult minerals nodes around the world.  If you cannot harvest a specific mineral
    SKILL_PET_IMP = 188,
    SKILL_PET_FELHUNTER = 189,
    SKILL_TAILORING = 197,               // Higher tailoring skill allows you to learn higher level tailoring patterns.  Tailoring patterns can be found on trainers around the world as well as from quests and monsters.
    SKILL_ENGINEERING = 202,             // Higher engineering skill allows you to learn higher level engineering schematics.  Schematics can be found on trainers around the world as well as from quests and monsters.
    SKILL_PET_SPIDER = 203,
    SKILL_PET_VOIDWALKER = 204,
    SKILL_PET_SUCCUBUS = 205,
    SKILL_PET_INFERNAL = 206,
    SKILL_PET_DOOMGUARD = 207,
    SKILL_PET_WOLF = 208,
    SKILL_PET_CAT = 209,
    SKILL_PET_BEAR = 210,
    SKILL_PET_BOAR = 211,
    SKILL_PET_CROCILISK = 212,
    SKILL_PET_CARRION_BIRD = 213,
    SKILL_PET_CRAB = 214,
    SKILL_PET_GORILLA = 215,
    SKILL_PET_RAPTOR = 217,
    SKILL_PET_TALLSTRIDER = 218,
    SKILL_RACIAL_UNDEAD = 220,
    SKILL_WEAPON_TALENTS = 222,
    SKILL_CROSSBOWS = 226,               // Higher weapon skill increases your chance to hit.
    SKILL_SPEARS = 227,
    SKILL_WANDS = 228,
    SKILL_POLEARMS = 229,                // Higher weapon skill increases your chance to hit.
    SKILL_PET_SCORPID = 236,
    SKILL_ARCANE = 237,
    SKILL_PET_TURTLE = 251,
    SKILL_ASSASSINATION = 253,
    SKILL_FURY = 256,
    SKILL_PROTECTION = 257,
    SKILL_BEAST_TRAINING = 261,
    SKILL_PROTECTION_1 = 267,
    SKILL_PET_TALENTS = 270,
    SKILL_PLATE_MAIL = 293,              // Allows the wearing of plate armor.
    SKILL_LANGUAGE_GNOMISH = 313,
    SKILL_LANGUAGE_TROLL = 315,
    SKILL_ENCHANTING = 333,              // Higher enchanting skill allows you to learn more powerful forumulas.  Formulas can be found on trainers around the world as well as from quests and monsters.
    SKILL_DEMONOLOGY = 354,
    SKILL_AFFLICTION = 355,
    SKILL_FISHING = 356,                 // Higher fishing skill increases your chance of catching fish in bodies of water around the world.  If you are having trouble catching fish in a given area
    SKILL_ENHANCEMENT = 373,
    SKILL_RESTORATION = 374,
    SKILL_ELEMENTAL_COMBAT = 375,
    SKILL_SKINNING = 393,                // Higher skinning skill allows you to skin hides from higher level monsters around the world.    Once your skill is above 100
    SKILL_MAIL = 413,                    // Allows the wearing of mail armor.
    SKILL_LEATHER = 414,                 // Allows the wearing of leather armor.
    SKILL_CLOTH = 415,                   // Allows the wearing of cloth armor.
    SKILL_SHIELD = 433,                  // Allows the use of shields.
    SKILL_FIST_WEAPONS = 473,            // Allows for the use of fist weapons.  Chance to hit is determined by the Unarmed skill.
    SKILL_RAPTOR_RIDING = 533,
    SKILL_MECHANOSTRIDER_PILOTING = 553,
    SKILL_UNDEAD_HORSEMANSHIP = 554,
    SKILL_RESTORATION_1 = 573,
    SKILL_BALANCE = 574,
    SKILL_DESTRUCTION = 593,
    SKILL_HOLY_1 = 594,
    SKILL_DISCIPLINE = 613,
    SKILL_LOCKPICKING = 633,
    SKILL_PET_BAT = 653,
    SKILL_PET_HYENA = 654,
    SKILL_PET_OWL = 655,
    SKILL_PET_WIND_SERPENT = 656,
    SKILL_LANGUAGE_GUTTERSPEAK = 673,
    SKILL_KODO_RIDING = 713,
    SKILL_RACIAL_TROLL = 733,
    SKILL_RACIAL_GNOME = 753,
    SKILL_RACIAL_HUMAN = 754,
    SKILL_JEWELCRAFTING = 755,
    SKILL_RACIAL_BLOODELF = 756,
    SKILL_PET_EVENT_REMOTE_CONTROL = 758,
    SKILL_LANGUAGE_DRAENEI = 759,
    SKILL_RACIAL_DRAENEI = 760,
    SKILL_PET_FELGUARD = 761,
    SKILL_RIDING = 762,
    SKILL_PET_DRAGONHAWK = 763,
    SKILL_PET_NETHER_RAY = 764,
    SKILL_PET_SPOREBAT = 765,
    SKILL_PET_WARP_STALKER = 766,
    SKILL_PET_RAVAGER = 767,
    SKILL_PET_SERPENT = 768,
    SKILL_INTERNAL = 769
}
