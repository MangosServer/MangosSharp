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

using Mangos.Common.Enums.Global;
using System;

namespace Mangos.Common.Globals;

public class MangosGlobalConstants
{
    public MangosGlobalConstants()
    {
        movementOrTurningFlagsMask = movementFlagsMask | TurningFlagsMask;
        MAX_AURA_EFFECTs = MAX_AURA_EFFECTs_VISIBLE + MAX_AURA_EFFECTs_PASSIVE;
        MAX_AURA_EFFECT_FLAGs = MAX_AURA_EFFECTs_VISIBLE / 8;
        MAX_AURA_EFFECT_LEVELSs = MAX_AURA_EFFECTs_VISIBLE / 4;
        MAX_NEGATIVE_AURA_EFFECTs = MAX_AURA_EFFECTs_VISIBLE - MAX_POSITIVE_AURA_EFFECTs;
    }

    public readonly int RevisionDbCharactersVersion = 1;
    public readonly int RevisionDbCharactersStructure;
    public readonly int RevisionDbCharactersContent;
    public readonly int RevisionDbMangosVersion = 21;
    public readonly int RevisionDbMangosStructure = 1;
    public readonly int RevisionDbMangosContent = 0;
    public readonly int RevisionDbRealmVersion = 21;
    public readonly int RevisionDbRealmStructure = 2;
    public readonly int RevisionDbRealmContent = 1;
    public readonly int GROUP_SUBGROUPSIZE = 5;  // (MAX_RAID_SIZE / MAX_GROUP_SIZE)
    public readonly int GROUP_SIZE = 5;          // Normal Group Size/More then 5, it's a raid group
    public readonly int GROUP_RAIDSIZE = 40;     // Max Raid Size
    public readonly ulong GUID_ITEM = 0x4000000000000000UL;
    public readonly ulong GUID_CONTAINER = 0x4000000000000000UL;
    public readonly ulong GUID_PLAYER;
    public readonly ulong GUID_GAMEOBJECT = 0xF110000000000000UL;
    public readonly ulong GUID_TRANSPORT = 0xF120000000000000UL;
    public readonly ulong GUID_UNIT = 0xF130000000000000UL;
    public readonly ulong GUID_PET = 0xF140000000000000UL;
    public readonly ulong GUID_DYNAMICOBJECT = 0xF100000000000000UL;
    public readonly ulong GUID_CORPSE = 0xF101000000000000UL;
    public readonly ulong GUID_MO_TRANSPORT = 0x1FC0000000000000UL;
    public readonly uint GUID_MASK_LOW = 0xFFFFFFFFU;
    public readonly ulong GUID_MASK_HIGH = 0xFFFFFFFF00000000UL;
    public readonly float DEFAULT_DISTANCE_VISIBLE = 155.8f;
    public readonly float DEFAULT_DISTANCE_DETECTION = 7f;

    // TODO: Is this correct? The amount of time since last pvp action until you go out of combat again
    public readonly int DEFAULT_PVP_COMBAT_TIME = 6000; // 6 seconds

    public readonly int DEFAULT_LOCK_TIMEOUT = 2000;
    public readonly int DEFAULT_INSTANCE_EXPIRE_TIME = 3600;              // 1 hour
    public readonly int DEFAULT_BATTLEFIELD_EXPIRE_TIME = 3600 * 24;      // 24 hours
    public bool[] SERVER_CONFIG_DISABLED_CLASSES = { false, false, false, false, false, false, false, false, false, true, false };
    public bool[] SERVER_CONFIG_DISABLED_RACES = { false, false, false, false, false, false, false, false, true, false, false };
    public readonly float UNIT_NORMAL_WALK_SPEED = 2.5f;
    public readonly float UNIT_NORMAL_RUN_SPEED = 7.0f;
    public readonly float UNIT_NORMAL_SWIM_SPEED = 4.722222f;
    public readonly float UNIT_NORMAL_SWIM_BACK_SPEED = 2.5f;
    public readonly float UNIT_NORMAL_WALK_BACK_SPEED = 4.5f;
    public readonly float UNIT_NORMAL_TURN_RATE = (float)Math.PI;
    public readonly float UNIT_NORMAL_TAXI_SPEED = 32.0f;
    public readonly int PLAYER_VISIBLE_ITEM_SIZE = 12;
    public readonly int PLAYER_SKILL_INFO_SIZE = 384 - 1;
    public readonly int PLAYER_EXPLORED_ZONES_SIZE = 64 - 1;
    public readonly int FIELD_MASK_SIZE_PLAYER = ((int)EPlayerFields.PLAYER_END + 32) / 32 * 32;
    public readonly int FIELD_MASK_SIZE_UNIT = ((int)EUnitFields.UNIT_END + 32) / 32 * 32;
    public readonly int FIELD_MASK_SIZE_GAMEOBJECT = ((int)EGameObjectFields.GAMEOBJECT_END + 32) / 32 * 32;
    public readonly int FIELD_MASK_SIZE_DYNAMICOBJECT = ((int)EDynamicObjectFields.DYNAMICOBJECT_END + 32) / 32 * 32;
    public readonly int FIELD_MASK_SIZE_ITEM = ((int)EContainerFields.CONTAINER_END + 32) / 32 * 32;
    public readonly int FIELD_MASK_SIZE_CORPSE = ((int)ECorpseFields.CORPSE_END + 32) / 32 * 32;
    public readonly string[] WorldServerStatus = { "ONLINE/G", "ONLINE/R", "OFFLINE " };
    // Public ConsoleColor As New ConsoleColor
    // 1.12.1 - 5875
    // 1.12.2 - 6005
    // 1.12.3 - 6141

    // New Auto Detection Build
    public readonly int Required_Build_1_12_1 = 5875;

    public readonly int Required_Build_1_12_2 = 6005;
    public readonly int Required_Build_1_12_3 = 6141;
    public readonly int ConnectionSleepTime = 100;
    public readonly int GUILD_RANK_MAX = 9; // Max Ranks Per Guild
    public readonly int GUILD_RANK_MIN = 5; // Min Ranks Per Guild
    public readonly string GOSSIP_TEXT_BANK = "The Bank";
    public readonly string GOSSIP_TEXT_WINDRIDER = "Wind rider master";
    public readonly string GOSSIP_TEXT_GRYPHON = "Gryphon Master";
    public readonly string GOSSIP_TEXT_BATHANDLER = "Bat Handler";
    public readonly string GOSSIP_TEXT_HIPPOGRYPH = "Hippogryph Master";
    public readonly string GOSSIP_TEXT_FLIGHTMASTER = "Flight Master";
    public readonly string GOSSIP_TEXT_AUCTIONHOUSE = "Auction House";
    public readonly string GOSSIP_TEXT_GUILDMASTER = "Guild Master";
    public readonly string GOSSIP_TEXT_INN = "The Inn";
    public readonly string GOSSIP_TEXT_MAILBOX = "Mailbox";
    public readonly string GOSSIP_TEXT_STABLEMASTER = "Stable Master";
    public readonly string GOSSIP_TEXT_WEAPONMASTER = "Weapons Trainer";
    public readonly string GOSSIP_TEXT_BATTLEMASTER = "Battlemaster";
    public readonly string GOSSIP_TEXT_CLASSTRAINER = "Class Trainer";
    public readonly string GOSSIP_TEXT_PROFTRAINER = "Profession Trainer";
    public readonly string GOSSIP_TEXT_OFFICERS = "The officers` lounge";
    public readonly string GOSSIP_TEXT_ALTERACVALLEY = "Alterac Valley";
    public readonly string GOSSIP_TEXT_ARATHIBASIN = "Arathi Basin";
    public readonly string GOSSIP_TEXT_WARSONGULCH = "Warsong Gulch";
    public readonly string GOSSIP_TEXT_IRONFORGE_BANK = "Bank of Ironforge";
    public readonly string GOSSIP_TEXT_STORMWIND_BANK = "Bank of Stormwind";
    public readonly string GOSSIP_TEXT_DEEPRUNTRAM = "Deeprun Tram";
    public readonly string GOSSIP_TEXT_ZEPPLINMASTER = "Zeppelin master";
    public readonly string GOSSIP_TEXT_FERRY = "Rut'theran Ferry";
    public readonly string GOSSIP_TEXT_DRUID = "Druid";
    public readonly string GOSSIP_TEXT_HUNTER = "Hunter";
    public readonly string GOSSIP_TEXT_PRIEST = "Priest";
    public readonly string GOSSIP_TEXT_ROGUE = "Rogue";
    public readonly string GOSSIP_TEXT_WARRIOR = "Warrior";
    public readonly string GOSSIP_TEXT_PALADIN = "Paladin";
    public readonly string GOSSIP_TEXT_SHAMAN = "Shaman";
    public readonly string GOSSIP_TEXT_MAGE = "Mage";
    public readonly string GOSSIP_TEXT_WARLOCK = "Warlock";
    public readonly string GOSSIP_TEXT_ALCHEMY = "Alchemy";
    public readonly string GOSSIP_TEXT_BLACKSMITHING = "Blacksmithing";
    public readonly string GOSSIP_TEXT_COOKING = "Cooking";
    public readonly string GOSSIP_TEXT_ENCHANTING = "Enchanting";
    public readonly string GOSSIP_TEXT_ENGINEERING = "Engineering";
    public readonly string GOSSIP_TEXT_FIRSTAID = "First Aid";
    public readonly string GOSSIP_TEXT_HERBALISM = "Herbalism";
    public readonly string GOSSIP_TEXT_LEATHERWORKING = "Leatherworking";
    public readonly string GOSSIP_TEXT_POISONS = "Poisons";
    public readonly string GOSSIP_TEXT_TAILORING = "Tailoring";
    public readonly string GOSSIP_TEXT_MINING = "Mining";
    public readonly string GOSSIP_TEXT_FISHING = "Fishing";
    public readonly string GOSSIP_TEXT_SKINNING = "Skinning";

    // VMAPS
    public readonly string VMAP_MAGIC = "VMAP_2.0";

    public readonly float VMAP_MAX_CAN_FALL_DISTANCE = 10.0f;
    public readonly float VMAP_INVALID_HEIGHT = -100000.0f; // for check
    public readonly float VMAP_INVALID_HEIGHT_VALUE = -200000.0f; // real assigned value in unknown height case

    // MAPS
    public readonly float SIZE = 533.3333f;

    public readonly int RESOLUTION_WATER = 128 - 1;
    public readonly int RESOLUTION_FLAGS = 16 - 1;
    public readonly int RESOLUTION_TERRAIN = 16 - 1;
    public readonly int groundFlagsMask = unchecked((int)(0xFFFFFFFF & (int)~(MovementFlags.MOVEMENTFLAG_LEFT | MovementFlags.MOVEMENTFLAG_RIGHT | MovementFlags.MOVEMENTFLAG_BACKWARD | MovementFlags.MOVEMENTFLAG_FORWARD | MovementFlags.MOVEMENTFLAG_WALK)));
    public readonly int movementFlagsMask = (int)(MovementFlags.MOVEMENTFLAG_FORWARD | MovementFlags.MOVEMENTFLAG_BACKWARD | MovementFlags.MOVEMENTFLAG_STRAFE_LEFT | MovementFlags.MOVEMENTFLAG_STRAFE_RIGHT | MovementFlags.MOVEMENTFLAG_PITCH_UP | MovementFlags.MOVEMENTFLAG_PITCH_DOWN | MovementFlags.MOVEMENTFLAG_JUMPING | MovementFlags.MOVEMENTFLAG_FALLING | MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_SPLINE);
    public readonly int TurningFlagsMask = (int)(MovementFlags.MOVEMENTFLAG_LEFT | MovementFlags.MOVEMENTFLAG_RIGHT);
    public readonly int movementOrTurningFlagsMask;
    public readonly byte ITEM_SLOT_NULL = 255;
    public readonly long ITEM_BAG_NULL = -1;
    public readonly int PETITION_GUILD_PRICE = 1000;
    public readonly int PETITION_GUILD = 5863;       // Guild Charter, ItemFlags = &H2000
    public readonly int GUILD_TABARD_ITEM = 5976;
    public readonly int CREATURE_MAX_SPELLS = 4;
    public readonly int MAX_OWNER_DIS = 100;
    public readonly int SPELL_DURATION_INFINITE = -1;
    public readonly int MAX_AURA_EFFECTs_VISIBLE = 48;                  // 48 AuraSlots (32 buff, 16 debuff)
    public readonly int MAX_AURA_EFFECTs_PASSIVE = 192;
    public readonly int MAX_AURA_EFFECTs;
    public readonly int MAX_AURA_EFFECT_FLAGs;
    public readonly int MAX_AURA_EFFECT_LEVELSs;
    public readonly int MAX_POSITIVE_AURA_EFFECTs = 32;
    public readonly int MAX_NEGATIVE_AURA_EFFECTs;
    public readonly uint UINT32_MAX = 0xFFFFFFFF;
    public readonly int UINT32_MIN;
    public readonly long MpqId = 441536589L;
    public readonly long MpqHeaderSize = 32L;
}
