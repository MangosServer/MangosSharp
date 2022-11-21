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

namespace GameServer.Network;

public enum MessageOpcode : ushort
{
    MSG_NULL_ACTION = 0x0,
    CMSG_BOOTME = 0x1,
    CMSG_DBLOOKUP = 0x2,
    SMSG_DBLOOKUP = 0x3,
    CMSG_QUERY_OBJECT_POSITION = 0x4,
    SMSG_QUERY_OBJECT_POSITION = 0x5,
    CMSG_QUERY_OBJECT_ROTATION = 0x6,
    SMSG_QUERY_OBJECT_ROTATION = 0x7,
    CMSG_WORLD_TELEPORT = 0x8,
    CMSG_TELEPORT_TO_UNIT = 0x9,
    CMSG_ZONE_MAP = 0xA,
    SMSG_ZONE_MAP = 0xB,
    CMSG_DEBUG_CHANGECELLZONE = 0xC,
    CMSG_EMBLAZON_TABARD_OBSOLETE = 0xD,
    CMSG_UNEMBLAZON_TABARD_OBSOLETE = 0xE,
    CMSG_RECHARGE = 0xF,
    CMSG_LEARN_SPELL = 0x10,
    CMSG_CREATEMONSTER = 0x11,
    CMSG_DESTROYMONSTER = 0x12,
    CMSG_CREATEITEM = 0x13,
    CMSG_CREATEGAMEOBJECT = 0x14,
    CMSG_MAKEMONSTERATTACKME_OBSOLETE = 0x15,
    CMSG_MAKEMONSTERATTACKGUID = 0x16,
    CMSG_ENABLEDEBUGCOMBATLOGGING_OBSOLETE = 0x17,
    CMSG_FORCEACTION = 0x18,
    CMSG_FORCEACTIONONOTHER = 0x19,
    CMSG_FORCEACTIONSHOW = 0x1A,
    SMSG_FORCEACTIONSHOW = 0x1B,
    SMSG_ATTACKERSTATEUPDATEDEBUGINFO_OBSOLETE = 0x1C,
    SMSG_DEBUGINFOSPELL_OBSOLETE = 0x1D,
    SMSG_DEBUGINFOSPELLMISS_OBSOLETE = 0x1E,
    SMSG_DEBUG_PLAYER_RANGE_OBSOLETE = 0x1F,
    CMSG_UNDRESSPLAYER = 0x20,
    CMSG_BEASTMASTER = 0x21,
    CMSG_GODMODE = 0x22,
    SMSG_GODMODE = 0x23,
    CMSG_CHEAT_SETMONEY = 0x24,
    CMSG_LEVEL_CHEAT = 0x25,
    CMSG_PET_LEVEL_CHEAT = 0x26,
    CMSG_LEVELUP_CHEAT_OBSOLETE = 0x27,
    CMSG_COOLDOWN_CHEAT = 0x28,
    CMSG_USE_SKILL_CHEAT = 0x29,
    CMSG_FLAG_QUEST = 0x2A,
    CMSG_FLAG_QUEST_FINISH = 0x2B,
    CMSG_CLEAR_QUEST = 0x2C,
    CMSG_SEND_EVENT = 0x2D,
    CMSG_DEBUG_AISTATE = 0x2E,
    SMSG_DEBUG_AISTATE = 0x2F,
    CMSG_DISABLE_PVP_CHEAT = 0x30,
    CMSG_ADVANCE_SPAWN_TIME = 0x31,
    CMSG_PVP_PORT_OBSOLETE = 0x32,
    CMSG_AUTH_SRP6_BEGIN = 0x33,
    CMSG_AUTH_SRP6_PROOF = 0x34,
    CMSG_AUTH_SRP6_RECODE = 0x35,
    CMSG_CHAR_CREATE = 0x36,
    CMSG_CHAR_ENUM = 0x37,
    CMSG_CHAR_DELETE = 0x38,
    SMSG_AUTH_SRP6_RESPONSE = 0x39,
    SMSG_CHAR_CREATE = 0x3A,
    SMSG_CHAR_ENUM = 0x3B,
    SMSG_CHAR_DELETE = 0x3C,
    CMSG_PLAYER_LOGIN = 0x3D,
    SMSG_NEW_WORLD = 0x3E,
    SMSG_TRANSFER_PENDING = 0x3F,
    SMSG_TRANSFER_ABORTED = 0x40,
    SMSG_CHARACTER_LOGIN_FAILED = 0x41,
    SMSG_LOGIN_SETTIMESPEED = 0x42,
    SMSG_GAMETIME_UPDATE = 0x43,
    CMSG_GAMETIME_SET = 0x44,
    SMSG_GAMETIME_SET = 0x45,
    CMSG_GAMESPEED_SET = 0x46,
    SMSG_GAMESPEED_SET = 0x47,
    CMSG_SERVERTIME = 0x48,
    SMSG_SERVERTIME = 0x49,
    CMSG_PLAYER_LOGOUT = 0x4A,
    CMSG_LOGOUT_REQUEST = 0x4B,
    SMSG_LOGOUT_RESPONSE = 0x4C,
    SMSG_LOGOUT_COMPLETE = 0x4D,
    CMSG_LOGOUT_CANCEL = 0x4E,
    SMSG_LOGOUT_CANCEL_ACK = 0x4F,
    CMSG_NAME_QUERY = 0x50,
    SMSG_NAME_QUERY_RESPONSE = 0x51,
    CMSG_PET_NAME_QUERY = 0x52,
    SMSG_PET_NAME_QUERY_RESPONSE = 0x53,
    CMSG_GUILD_QUERY = 0x54,
    SMSG_GUILD_QUERY_RESPONSE = 0x55,
    CMSG_ITEM_QUERY_SINGLE = 0x56,
    CMSG_ITEM_QUERY_MULTIPLE = 0x57,
    SMSG_ITEM_QUERY_SINGLE_RESPONSE = 0x58,
    SMSG_ITEM_QUERY_MULTIPLE_RESPONSE = 0x59,
    CMSG_PAGE_TEXT_QUERY = 0x5A,
    SMSG_PAGE_TEXT_QUERY_RESPONSE = 0x5B,
    CMSG_QUEST_QUERY = 0x5C,
    SMSG_QUEST_QUERY_RESPONSE = 0x5D,
    CMSG_GAMEOBJECT_QUERY = 0x5E,
    SMSG_GAMEOBJECT_QUERY_RESPONSE = 0x5F,
    CMSG_CREATURE_QUERY = 0x60,
    SMSG_CREATURE_QUERY_RESPONSE = 0x61,
    CMSG_WHO = 0x62,
    SMSG_WHO = 0x63,
    CMSG_WHOIS = 0x64,
    SMSG_WHOIS = 0x65,
    CMSG_FRIEND_LIST = 0x66,
    SMSG_FRIEND_LIST = 0x67,
    SMSG_FRIEND_STATUS = 0x68,
    CMSG_ADD_FRIEND = 0x69,
    CMSG_DEL_FRIEND = 0x6A,
    SMSG_IGNORE_LIST = 0x6B,
    CMSG_ADD_IGNORE = 0x6C,
    CMSG_DEL_IGNORE = 0x6D,
    CMSG_GROUP_INVITE = 0x6E,
    SMSG_GROUP_INVITE = 0x6F,
    CMSG_GROUP_CANCEL = 0x70,
    SMSG_GROUP_CANCEL = 0x71,
    CMSG_GROUP_ACCEPT = 0x72,
    CMSG_GROUP_DECLINE = 0x73,
    SMSG_GROUP_DECLINE = 0x74,
    CMSG_GROUP_UNINVITE = 0x75,
    CMSG_GROUP_UNINVITE_GUID = 0x76,
    SMSG_GROUP_UNINVITE = 0x77,
    CMSG_GROUP_SET_LEADER = 0x78,
    SMSG_GROUP_SET_LEADER = 0x79,
    CMSG_LOOT_METHOD = 0x7A,
    CMSG_GROUP_DISBAND = 0x7B,
    SMSG_GROUP_DESTROYED = 0x7C,
    SMSG_GROUP_LIST = 0x7D,
    SMSG_PARTY_MEMBER_STATS = 0x7E,
    SMSG_PARTY_COMMAND_RESULT = 0x7F,
    UMSG_UPDATE_GROUP_MEMBERS = 0x80,
    CMSG_GUILD_CREATE = 0x81,
    CMSG_GUILD_INVITE = 0x82,
    SMSG_GUILD_INVITE = 0x83,
    CMSG_GUILD_ACCEPT = 0x84,
    CMSG_GUILD_DECLINE = 0x85,
    SMSG_GUILD_DECLINE = 0x86,
    CMSG_GUILD_INFO = 0x87,
    SMSG_GUILD_INFO = 0x88,
    CMSG_GUILD_ROSTER = 0x89,
    SMSG_GUILD_ROSTER = 0x8A,
    CMSG_GUILD_PROMOTE = 0x8B,
    CMSG_GUILD_DEMOTE = 0x8C,
    CMSG_GUILD_LEAVE = 0x8D,
    CMSG_GUILD_REMOVE = 0x8E,
    CMSG_GUILD_DISBAND = 0x8F,
    CMSG_GUILD_LEADER = 0x90,
    CMSG_GUILD_MOTD = 0x91,
    SMSG_GUILD_EVENT = 0x92,
    SMSG_GUILD_COMMAND_RESULT = 0x93,
    UMSG_UPDATE_GUILD = 0x94,
    CMSG_MESSAGECHAT = 0x95,
    SMSG_MESSAGECHAT = 0x96,
    CMSG_JOIN_CHANNEL = 0x97,
    CMSG_LEAVE_CHANNEL = 0x98,
    SMSG_CHANNEL_NOTIFY = 0x99,
    CMSG_CHANNEL_LIST = 0x9A,
    SMSG_CHANNEL_LIST = 0x9B,
    CMSG_CHANNEL_PASSWORD = 0x9C,
    CMSG_CHANNEL_SET_OWNER = 0x9D,
    CMSG_CHANNEL_OWNER = 0x9E,
    CMSG_CHANNEL_MODERATOR = 0x9F,
    CMSG_CHANNEL_UNMODERATOR = 0xA0,
    CMSG_CHANNEL_MUTE = 0xA1,
    CMSG_CHANNEL_UNMUTE = 0xA2,
    CMSG_CHANNEL_INVITE = 0xA3,
    CMSG_CHANNEL_KICK = 0xA4,
    CMSG_CHANNEL_BAN = 0xA5,
    CMSG_CHANNEL_UNBAN = 0xA6,
    CMSG_CHANNEL_ANNOUNCEMENTS = 0xA7,
    CMSG_CHANNEL_MODERATE = 0xA8,
    SMSG_UPDATE_OBJECT = 0xA9,
    SMSG_DESTROY_OBJECT = 0xAA,
    CMSG_USE_ITEM = 0xAB,
    CMSG_OPEN_ITEM = 0xAC,
    CMSG_READ_ITEM = 0xAD,
    SMSG_READ_ITEM_OK = 0xAE,
    SMSG_READ_ITEM_FAILED = 0xAF,
    SMSG_ITEM_COOLDOWN = 0xB0,
    CMSG_GAMEOBJ_USE = 0xB1,
    CMSG_GAMEOBJ_CHAIR_USE_OBSOLETE = 0xB2,
    SMSG_GAMEOBJECT_CUSTOM_ANIM = 0xB3,
    CMSG_AREATRIGGER = 0xB4,
    MSG_MOVE_START_FORWARD = 0xB5,
    MSG_MOVE_START_BACKWARD = 0xB6,
    MSG_MOVE_STOP = 0xB7,
    MSG_MOVE_START_STRAFE_LEFT = 0xB8,
    MSG_MOVE_START_STRAFE_RIGHT = 0xB9,
    MSG_MOVE_STOP_STRAFE = 0xBA,
    MSG_MOVE_JUMP = 0xBB,
    MSG_MOVE_START_TURN_LEFT = 0xBC,
    MSG_MOVE_START_TURN_RIGHT = 0xBD,
    MSG_MOVE_STOP_TURN = 0xBE,
    MSG_MOVE_START_PITCH_UP = 0xBF,
    MSG_MOVE_START_PITCH_DOWN = 0xC0,
    MSG_MOVE_STOP_PITCH = 0xC1,
    MSG_MOVE_SET_RUN_MODE = 0xC2,
    MSG_MOVE_SET_WALK_MODE = 0xC3,
    MSG_MOVE_TOGGLE_LOGGING = 0xC4,
    MSG_MOVE_TELEPORT = 0xC5,
    MSG_MOVE_TELEPORT_CHEAT = 0xC6,
    MSG_MOVE_TELEPORT_ACK = 0xC7,
    MSG_MOVE_TOGGLE_FALL_LOGGING = 0xC8,
    MSG_MOVE_FALL_LAND = 0xC9,
    MSG_MOVE_START_SWIM = 0xCA,
    MSG_MOVE_STOP_SWIM = 0xCB,
    MSG_MOVE_SET_RUN_SPEED_CHEAT = 0xCC,
    MSG_MOVE_SET_RUN_SPEED = 0xCD,
    MSG_MOVE_SET_RUN_BACK_SPEED_CHEAT = 0xCE,
    MSG_MOVE_SET_RUN_BACK_SPEED = 0xCF,
    MSG_MOVE_SET_WALK_SPEED_CHEAT = 0xD0,
    MSG_MOVE_SET_WALK_SPEED = 0xD1,
    MSG_MOVE_SET_SWIM_SPEED_CHEAT = 0xD2,
    MSG_MOVE_SET_SWIM_SPEED = 0xD3,
    MSG_MOVE_SET_SWIM_BACK_SPEED_CHEAT = 0xD4,
    MSG_MOVE_SET_SWIM_BACK_SPEED = 0xD5,
    MSG_MOVE_SET_ALL_SPEED_CHEAT = 0xD6,
    MSG_MOVE_SET_TURN_RATE_CHEAT = 0xD7,
    MSG_MOVE_SET_TURN_RATE = 0xD8,
    MSG_MOVE_TOGGLE_COLLISION_CHEAT = 0xD9,
    MSG_MOVE_SET_FACING = 0xDA,
    MSG_MOVE_SET_PITCH = 0xDB,
    MSG_MOVE_WORLDPORT_ACK = 0xDC,
    SMSG_MONSTER_MOVE = 0xDD,
    SMSG_MOVE_WATER_WALK = 0xDE,
    SMSG_MOVE_LAND_WALK = 0xDF,
    MSG_MOVE_SET_RAW_POSITION_ACK = 0xE0,
    CMSG_MOVE_SET_RAW_POSITION = 0xE1,
    SMSG_FORCE_RUN_SPEED_CHANGE = 0xE2,
    CMSG_FORCE_RUN_SPEED_CHANGE_ACK = 0xE3,
    SMSG_FORCE_RUN_BACK_SPEED_CHANGE = 0xE4,
    CMSG_FORCE_RUN_BACK_SPEED_CHANGE_ACK = 0xE5,
    SMSG_FORCE_SWIM_SPEED_CHANGE = 0xE6,
    CMSG_FORCE_SWIM_SPEED_CHANGE_ACK = 0xE7,
    SMSG_FORCE_MOVE_ROOT = 0xE8,
    CMSG_FORCE_MOVE_ROOT_ACK = 0xE9,
    SMSG_FORCE_MOVE_UNROOT = 0xEA,
    CMSG_FORCE_MOVE_UNROOT_ACK = 0xEB,
    MSG_MOVE_ROOT = 0xEC,
    MSG_MOVE_UNROOT = 0xED,
    MSG_MOVE_HEARTBEAT = 0xEE,
    SMSG_MOVE_KNOCK_BACK = 0xEF,
    CMSG_MOVE_KNOCK_BACK_ACK = 0xF0,
    MSG_MOVE_KNOCK_BACK = 0xF1,
    SMSG_MOVE_FEATHER_FALL = 0xF2,
    SMSG_MOVE_NORMAL_FALL = 0xF3,
    SMSG_MOVE_SET_HOVER = 0xF4,
    SMSG_MOVE_UNSET_HOVER = 0xF5,
    CMSG_MOVE_HOVER_ACK = 0xF6,
    MSG_MOVE_HOVER = 0xF7,
    CMSG_TRIGGER_CINEMATIC_CHEAT = 0xF8,
    CMSG_OPENING_CINEMATIC = 0xF9,
    SMSG_TRIGGER_CINEMATIC = 0xFA,
    CMSG_NEXT_CINEMATIC_CAMERA = 0xFB,
    CMSG_COMPLETE_CINEMATIC = 0xFC,
    SMSG_TUTORIAL_FLAGS = 0xFD,
    CMSG_TUTORIAL_FLAG = 0xFE,
    CMSG_TUTORIAL_CLEAR = 0xFF,
    CMSG_TUTORIAL_RESET = 0x100,
    CMSG_STANDSTATECHANGE = 0x101,
    CMSG_EMOTE = 0x102,
    SMSG_EMOTE = 0x103,
    CMSG_TEXT_EMOTE = 0x104,
    SMSG_TEXT_EMOTE = 0x105,
    CMSG_AUTOEQUIP_GROUND_ITEM = 0x106,
    CMSG_AUTOSTORE_GROUND_ITEM = 0x107,
    CMSG_AUTOSTORE_LOOT_ITEM = 0x108,
    CMSG_STORE_LOOT_IN_SLOT = 0x109,
    CMSG_AUTOEQUIP_ITEM = 0x10A,
    CMSG_AUTOSTORE_BAG_ITEM = 0x10B,
    CMSG_SWAP_ITEM = 0x10C,
    CMSG_SWAP_INV_ITEM = 0x10D,
    CMSG_SPLIT_ITEM = 0x10E,
    CMSG_PICKUP_ITEM = 0x10F,
    CMSG_DROP_ITEM = 0x110,
    CMSG_DESTROYITEM = 0x111,
    SMSG_INVENTORY_CHANGE_FAILURE = 0x112,
    SMSG_OPEN_CONTAINER = 0x113,
    CMSG_INSPECT = 0x114,
    SMSG_INSPECT = 0x115,
    CMSG_INITIATE_TRADE = 0x116,
    CMSG_BEGIN_TRADE = 0x117,
    CMSG_BUSY_TRADE = 0x118,
    CMSG_IGNORE_TRADE = 0x119,
    CMSG_ACCEPT_TRADE = 0x11A,
    CMSG_UNACCEPT_TRADE = 0x11B,
    CMSG_CANCEL_TRADE = 0x11C,
    CMSG_SET_TRADE_ITEM = 0x11D,
    CMSG_CLEAR_TRADE_ITEM = 0x11E,
    CMSG_SET_TRADE_GOLD = 0x11F,
    SMSG_TRADE_STATUS = 0x120,
    SMSG_TRADE_STATUS_EXTENDED = 0x121,
    SMSG_INITIALIZE_FACTIONS = 0x122,
    SMSG_SET_FACTION_VISIBLE = 0x123,
    SMSG_SET_FACTION_STANDING = 0x124,
    CMSG_SET_FACTION_ATWAR = 0x125,
    CMSG_SET_FACTION_CHEAT = 0x126,
    SMSG_SET_PROFICIENCY = 0x127,
    CMSG_SET_ACTION_BUTTON = 0x128,
    SMSG_ACTION_BUTTONS = 0x129,
    SMSG_INITIAL_SPELLS = 0x12A,
    SMSG_LEARNED_SPELL = 0x12B,
    SMSG_SUPERCEDED_SPELL = 0x12C,
    CMSG_NEW_SPELL_SLOT = 0x12D,
    CMSG_CAST_SPELL = 0x12E,
    CMSG_CANCEL_CAST = 0x12F,
    SMSG_CAST_RESULT = 0x130,
    SMSG_SPELL_START = 0x131,
    SMSG_SPELL_GO = 0x132,
    SMSG_SPELL_FAILURE = 0x133,
    SMSG_SPELL_COOLDOWN = 0x134,
    SMSG_COOLDOWN_EVENT = 0x135,
    CMSG_CANCEL_AURA = 0x136,
    SMSG_UPDATE_AURA_DURATION = 0x137,
    SMSG_PET_CAST_FAILED = 0x138,
    MSG_CHANNEL_START = 0x139,
    MSG_CHANNEL_UPDATE = 0x13A,
    CMSG_CANCEL_CHANNELLING = 0x13B,
    SMSG_AI_REACTION = 0x13C,
    CMSG_SET_SELECTION = 0x13D,
    CMSG_SET_TARGET_OBSOLETE = 0x13E,
    CMSG_UNUSED = 0x13F,
    CMSG_UNUSED2 = 0x140,
    CMSG_ATTACKSWING = 0x141,
    CMSG_ATTACKSTOP = 0x142,
    SMSG_ATTACKSTART = 0x143,
    SMSG_ATTACKSTOP = 0x144,
    SMSG_ATTACKSWING_NOTINRANGE = 0x145,
    SMSG_ATTACKSWING_BADFACING = 0x146,
    SMSG_ATTACKSWING_NOTSTANDING = 0x147,
    SMSG_ATTACKSWING_DEADTARGET = 0x148,
    SMSG_ATTACKSWING_CANT_ATTACK = 0x149,
    SMSG_ATTACKERSTATEUPDATE = 0x14A,
    SMSG_VICTIMSTATEUPDATE_OBSOLETE = 0x14B,
    SMSG_DAMAGE_DONE_OBSOLETE = 0x14C,
    SMSG_DAMAGE_TAKEN_OBSOLETE = 0x14D,
    SMSG_CANCEL_COMBAT = 0x14E,
    SMSG_PLAYER_COMBAT_XP_GAIN_OBSOLETE = 0x14F,
    SMSG_HEALSPELL_ON_PLAYER_OBSOLETE = 0x150,
    SMSG_HEALSPELL_ON_PLAYERS_PET_OBSOLETE = 0x151,
    CMSG_SHEATHE_OBSOLETE = 0x152,
    CMSG_SAVE_PLAYER = 0x153,
    CMSG_SETDEATHBINDPOINT = 0x154,
    SMSG_BINDPOINTUPDATE = 0x155,
    CMSG_GETDEATHBINDZONE = 0x156,
    SMSG_BINDZONEREPLY = 0x157,
    SMSG_PLAYERBOUND = 0x158,
    SMSG_DEATH_NOTIFY_OBSOLETE = 0x159,
    CMSG_REPOP_REQUEST = 0x15A,
    SMSG_RESURRECT_REQUEST = 0x15B,
    CMSG_RESURRECT_RESPONSE = 0x15C,
    CMSG_LOOT = 0x15D,
    CMSG_LOOT_MONEY = 0x15E,
    CMSG_LOOT_RELEASE = 0x15F,
    SMSG_LOOT_RESPONSE = 0x160,
    SMSG_LOOT_RELEASE_RESPONSE = 0x161,
    SMSG_LOOT_REMOVED = 0x162,
    SMSG_LOOT_MONEY_NOTIFY = 0x163,
    SMSG_LOOT_ITEM_NOTIFY = 0x164,
    SMSG_LOOT_CLEAR_MONEY = 0x165,
    SMSG_ITEM_PUSH_RESULT = 0x166,
    SMSG_DUEL_REQUESTED = 0x167,
    SMSG_DUEL_OUTOFBOUNDS = 0x168,
    SMSG_DUEL_INBOUNDS = 0x169,
    SMSG_DUEL_COMPLETE = 0x16A,
    SMSG_DUEL_WINNER = 0x16B,
    CMSG_DUEL_ACCEPTED = 0x16C,
    CMSG_DUEL_CANCELLED = 0x16D,
    SMSG_MOUNTRESULT = 0x16E,
    SMSG_DISMOUNTRESULT = 0x16F,
    SMSG_PUREMOUNT_CANCELLED_OBSOLETE = 0x170,
    CMSG_MOUNTSPECIAL_ANIM = 0x171,
    SMSG_MOUNTSPECIAL_ANIM = 0x172,
    SMSG_PET_TAME_FAILURE = 0x173,
    CMSG_PET_SET_ACTION = 0x174,
    CMSG_PET_ACTION = 0x175,
    CMSG_PET_ABANDON = 0x176,
    CMSG_PET_RENAME = 0x177,
    SMSG_PET_NAME_INVALID = 0x178,
    SMSG_PET_SPELLS = 0x179,
    SMSG_PET_MODE = 0x17A,
    CMSG_GOSSIP_HELLO = 0x17B,
    CMSG_GOSSIP_SELECT_OPTION = 0x17C,
    SMSG_GOSSIP_MESSAGE = 0x17D,
    SMSG_GOSSIP_COMPLETE = 0x17E,
    CMSG_NPC_TEXT_QUERY = 0x17F,
    SMSG_NPC_TEXT_UPDATE = 0x180,
    SMSG_NPC_WONT_TALK = 0x181,
    CMSG_QUESTGIVER_STATUS_QUERY = 0x182,
    SMSG_QUESTGIVER_STATUS = 0x183,
    CMSG_QUESTGIVER_HELLO = 0x184,
    SMSG_QUESTGIVER_QUEST_LIST = 0x185,
    CMSG_QUESTGIVER_QUERY_QUEST = 0x186,
    CMSG_QUESTGIVER_QUEST_AUTOLAUNCH = 0x187,
    SMSG_QUESTGIVER_QUEST_DETAILS = 0x188,
    CMSG_QUESTGIVER_ACCEPT_QUEST = 0x189,
    CMSG_QUESTGIVER_COMPLETE_QUEST = 0x18A,
    SMSG_QUESTGIVER_REQUEST_ITEMS = 0x18B,
    CMSG_QUESTGIVER_REQUEST_REWARD = 0x18C,
    SMSG_QUESTGIVER_OFFER_REWARD = 0x18D,
    CMSG_QUESTGIVER_CHOOSE_REWARD = 0x18E,
    SMSG_QUESTGIVER_QUEST_INVALID = 0x18F,
    CMSG_QUESTGIVER_CANCEL = 0x190,
    SMSG_QUESTGIVER_QUEST_COMPLETE = 0x191,
    SMSG_QUESTGIVER_QUEST_FAILED = 0x192,
    CMSG_QUESTLOG_SWAP_QUEST = 0x193,
    CMSG_QUESTLOG_REMOVE_QUEST = 0x194,
    SMSG_QUESTLOG_FULL = 0x195,
    SMSG_QUESTUPDATE_FAILED = 0x196,
    SMSG_QUESTUPDATE_FAILEDTIMER = 0x197,
    SMSG_QUESTUPDATE_COMPLETE = 0x198,
    SMSG_QUESTUPDATE_ADD_KILL = 0x199,
    SMSG_QUESTUPDATE_ADD_ITEM = 0x19A,
    CMSG_QUEST_CONFIRM_ACCEPT = 0x19B,
    SMSG_QUEST_CONFIRM_ACCEPT = 0x19C,
    CMSG_PUSHQUESTTOPARTY = 0x19D,
    CMSG_LIST_INVENTORY = 0x19E,
    SMSG_LIST_INVENTORY = 0x19F,
    CMSG_SELL_ITEM = 0x1A0,
    SMSG_SELL_ITEM = 0x1A1,
    CMSG_BUY_ITEM = 0x1A2,
    CMSG_BUY_ITEM_IN_SLOT = 0x1A3,
    SMSG_BUY_ITEM = 0x1A4,
    SMSG_BUY_FAILED = 0x1A5,
    CMSG_TAXICLEARALLNODES = 0x1A6,
    CMSG_TAXIENABLEALLNODES = 0x1A7,
    CMSG_TAXISHOWNODES = 0x1A8,
    SMSG_SHOWTAXINODES = 0x1A9,
    CMSG_TAXINODE_STATUS_QUERY = 0x1AA,
    SMSG_TAXINODE_STATUS = 0x1AB,
    CMSG_TAXIQUERYAVAILABLENODES = 0x1AC,
    CMSG_ACTIVATETAXI = 0x1AD,
    SMSG_ACTIVATETAXIREPLY = 0x1AE,
    SMSG_NEW_TAXI_PATH = 0x1AF,
    CMSG_TRAINER_LIST = 0x1B0,
    SMSG_TRAINER_LIST = 0x1B1,
    CMSG_TRAINER_BUY_SPELL = 0x1B2,
    SMSG_TRAINER_BUY_SUCCEEDED = 0x1B3,
    SMSG_TRAINER_BUY_FAILED = 0x1B4,
    CMSG_BINDER_ACTIVATE = 0x1B5,
    SMSG_PLAYERBINDERROR = 0x1B6,
    CMSG_BANKER_ACTIVATE = 0x1B7,
    SMSG_SHOW_BANK = 0x1B8,
    CMSG_BUY_BANK_SLOT = 0x1B9,
    SMSG_BUY_BANK_SLOT_RESULT = 0x1BA,
    CMSG_PETITION_SHOWLIST = 0x1BB,
    SMSG_PETITION_SHOWLIST = 0x1BC,
    CMSG_PETITION_BUY = 0x1BD,
    CMSG_PETITION_SHOW_SIGNATURES = 0x1BE,
    SMSG_PETITION_SHOW_SIGNATURES = 0x1BF,
    CMSG_PETITION_SIGN = 0x1C0,
    SMSG_PETITION_SIGN_RESULTS = 0x1C1,
    MSG_PETITION_DECLINE = 0x1C2,
    CMSG_OFFER_PETITION = 0x1C3,
    CMSG_TURN_IN_PETITION = 0x1C4,
    SMSG_TURN_IN_PETITION_RESULTS = 0x1C5,
    CMSG_PETITION_QUERY = 0x1C6,
    SMSG_PETITION_QUERY_RESPONSE = 0x1C7,
    SMSG_FISH_NOT_HOOKED = 0x1C8,
    SMSG_FISH_ESCAPED = 0x1C9,
    CMSG_BUG = 0x1CA,
    SMSG_NOTIFICATION = 0x1CB,
    CMSG_PLAYED_TIME = 0x1CC,
    SMSG_PLAYED_TIME = 0x1CD,
    CMSG_QUERY_TIME = 0x1CE,
    SMSG_QUERY_TIME_RESPONSE = 0x1CF,
    SMSG_LOG_XPGAIN = 0x1D0,
    MSG_SPLIT_MONEY = 0x1D1,
    CMSG_RECLAIM_CORPSE = 0x1D2,
    CMSG_WRAP_ITEM = 0x1D3,
    SMSG_LEVELUP_INFO = 0x1D4,
    MSG_MINIMAP_PING = 0x1D5,
    SMSG_RESISTLOG = 0x1D6,
    SMSG_ENCHANTMENTLOG = 0x1D7,
    CMSG_SET_SKILL_CHEAT = 0x1D8,
    SMSG_START_MIRROR_TIMER = 0x1D9,
    SMSG_PAUSE_MIRROR_TIMER = 0x1DA,
    SMSG_STOP_MIRROR_TIMER = 0x1DB,
    CMSG_PING = 0x1DC,
    SMSG_PONG = 0x1DD,
    SMSG_CLEAR_COOLDOWN = 0x1DE,
    SMSG_GAMEOBJECT_PAGETEXT = 0x1DF,
    CMSG_SETSHEATHED = 0x1E0,
    SMSG_COOLDOWN_CHEAT = 0x1E1,
    SMSG_SPELL_DELAYED = 0x1E2,
    CMSG_PLAYER_MACRO_OBSOLETE = 0x1E3,
    SMSG_PLAYER_MACRO_OBSOLETE = 0x1E4,
    CMSG_GHOST = 0x1E5,
    CMSG_GM_INVIS = 0x1E6,
    SMSG_INVALID_PROMOTION_CODE = 0x1E7,
    MSG_GM_BIND_OTHER = 0x1E8,
    MSG_GM_SUMMON = 0x1E9,
    SMSG_ITEM_TIME_UPDATE = 0x1EA,
    SMSG_ITEM_ENCHANT_TIME_UPDATE = 0x1EB,
    SMSG_AUTH_CHALLENGE = 0x1EC,
    CMSG_AUTH_SESSION = 0x1ED,
    SMSG_AUTH_RESPONSE = 0x1EE,
    MSG_GM_SHOWLABEL = 0x1EF,
    MSG_ADD_DYNAMIC_TARGET_OBSOLETE = 0x1F0,
    MSG_SAVE_GUILD_EMBLEM = 0x1F1,
    MSG_TABARDVENDOR_ACTIVATE = 0x1F2,
    SMSG_PLAY_SPELL_VISUAL = 0x1F3,
    CMSG_ZONEUPDATE = 0x1F4,
    SMSG_PARTYKILLLOG = 0x1F5,
    SMSG_COMPRESSED_UPDATE_OBJECT = 0x1F6,
    SMSG_OBSOLETE = 0x1F7,
    SMSG_EXPLORATION_EXPERIENCE = 0x1F8,
    CMSG_GM_SET_SECURITY_GROUP = 0x1F9,
    CMSG_GM_NUKE = 0x1FA,
    MSG_RANDOM_ROLL = 0x1FB,
    SMSG_ENVIRONMENTALDAMAGELOG = 0x1FC,
    CMSG_RWHOIS = 0x1FD,
    SMSG_RWHOIS = 0x1FE,
    MSG_LOOKING_FOR_GROUP = 0x1FF,
    CMSG_SET_LOOKING_FOR_GROUP = 0x200,
    CMSG_UNLEARN_SPELL = 0x201,
    CMSG_UNLEARN_SKILL = 0x202,
    SMSG_REMOVED_SPELL = 0x203,
    CMSG_DECHARGE = 0x204,
    CMSG_GMTICKET_CREATE = 0x205,
    SMSG_GMTICKET_CREATE = 0x206,
    CMSG_GMTICKET_UPDATETEXT = 0x207,
    SMSG_GMTICKET_UPDATETEXT = 0x208,
    SMSG_ACCOUNT_DATA_MD5 = 0x209,
    CMSG_REQUEST_ACCOUNT_DATA = 0x20A,
    CMSG_UPDATE_ACCOUNT_DATA = 0x20B,
    SMSG_UPDATE_ACCOUNT_DATA = 0x20C,
    SMSG_CLEAR_FAR_SIGHT_IMMEDIATE = 0x20D,
    SMSG_POWERGAINLOG_OBSOLETE = 0x20E,
    CMSG_GM_TEACH = 0x20F,
    CMSG_GM_CREATE_ITEM_TARGET = 0x210,
    CMSG_GMTICKET_GETTICKET = 0x211,
    SMSG_GMTICKET_GETTICKET = 0x212,
    CMSG_UNLEARN_TALENTS = 0x213,
    SMSG_GAMEOBJECT_SPAWN_ANIM = 0x214,
    SMSG_GAMEOBJECT_DESPAWN_ANIM = 0x215,
    MSG_CORPSE_QUERY = 0x216,
    CMSG_GMTICKET_DELETETICKET = 0x217,
    SMSG_GMTICKET_DELETETICKET = 0x218,
    SMSG_CHAT_WRONG_FACTION = 0x219,
    CMSG_GMTICKET_SYSTEMSTATUS = 0x21A,
    SMSG_GMTICKET_SYSTEMSTATUS = 0x21B,
    CMSG_SPIRIT_HEALER_ACTIVATE = 0x21C,
    CMSG_SET_STAT_CHEAT = 0x21D,
    SMSG_SET_REST_START = 0x21E,
    CMSG_SKILL_BUY_STEP = 0x21F,
    CMSG_SKILL_BUY_RANK = 0x220,
    CMSG_XP_CHEAT = 0x221,
    SMSG_SPIRIT_HEALER_CONFIRM = 0x222,
    CMSG_CHARACTER_POINT_CHEAT = 0x223,
    SMSG_GOSSIP_POI = 0x224,
    CMSG_CHAT_IGNORED = 0x225,
    CMSG_GM_VISION = 0x226,
    CMSG_SERVER_COMMAND = 0x227,
    CMSG_GM_SILENCE = 0x228,
    CMSG_GM_REVEALTO = 0x229,
    CMSG_GM_RESURRECT = 0x22A,
    CMSG_GM_SUMMONMOB = 0x22B,
    CMSG_GM_MOVECORPSE = 0x22C,
    CMSG_GM_FREEZE = 0x22D,
    CMSG_GM_UBERINVIS = 0x22E,
    CMSG_GM_REQUEST_PLAYER_INFO = 0x22F,
    SMSG_GM_PLAYER_INFO = 0x230,
    CMSG_GUILD_RANK = 0x231,
    CMSG_GUILD_ADD_RANK = 0x232,
    CMSG_GUILD_DEL_RANK = 0x233,
    CMSG_GUILD_SET_PUBLIC_NOTE = 0x234,
    CMSG_GUILD_SET_OFFICER_NOTE = 0x235,
    SMSG_LOGIN_VERIFY_WORLD = 0x236,
    CMSG_CLEAR_EXPLORATION = 0x237,
    CMSG_SEND_MAIL = 0x238,
    SMSG_SEND_MAIL_RESULT = 0x239,
    CMSG_GET_MAIL_LIST = 0x23A,
    SMSG_MAIL_LIST_RESULT = 0x23B,
    CMSG_BATTLEFIELD_LIST = 0x23C,
    SMSG_BATTLEFIELD_LIST = 0x23D,
    CMSG_BATTLEFIELD_JOIN = 0x23E,
    SMSG_BATTLEFIELD_WIN = 0x23F,
    SMSG_BATTLEFIELD_LOSE = 0x240,
    CMSG_TAXICLEARNODE = 0x241,
    CMSG_TAXIENABLENODE = 0x242,
    CMSG_ITEM_TEXT_QUERY = 0x243,
    SMSG_ITEM_TEXT_QUERY_RESPONSE = 0x244,
    CMSG_MAIL_TAKE_MONEY = 0x245,
    CMSG_MAIL_TAKE_ITEM = 0x246,
    CMSG_MAIL_MARK_AS_READ = 0x247,
    CMSG_MAIL_RETURN_TO_SENDER = 0x248,
    CMSG_MAIL_DELETE = 0x249,
    CMSG_MAIL_CREATE_TEXT_ITEM = 0x24A,
    SMSG_SPELLLOGMISS = 0x24B,
    SMSG_SPELLLOGEXECUTE = 0x24C,
    SMSG_DEBUGAURAPROC = 0x24D,
    SMSG_PERIODICAURALOG = 0x24E,
    SMSG_SPELLDAMAGESHIELD = 0x24F,
    SMSG_SPELLNONMELEEDAMAGELOG = 0x250,
    CMSG_LEARN_TALENT = 0x251,
    SMSG_RESURRECT_FAILED = 0x252,
    CMSG_TOGGLE_PVP = 0x253,
    SMSG_ZONE_UNDER_ATTACK = 0x254,
    MSG_AUCTION_HELLO = 0x255,
    CMSG_AUCTION_SELL_ITEM = 0x256,
    CMSG_AUCTION_REMOVE_ITEM = 0x257,
    CMSG_AUCTION_LIST_ITEMS = 0x258,
    CMSG_AUCTION_LIST_OWNER_ITEMS = 0x259,
    CMSG_AUCTION_PLACE_BID = 0x25A,
    SMSG_AUCTION_COMMAND_RESULT = 0x25B,
    SMSG_AUCTION_LIST_RESULT = 0x25C,
    SMSG_AUCTION_OWNER_LIST_RESULT = 0x25D,
    SMSG_AUCTION_BIDDER_NOTIFICATION = 0x25E,
    SMSG_AUCTION_OWNER_NOTIFICATION = 0x25F,
    SMSG_PROCRESIST = 0x260,
    SMSG_STANDSTATE_CHANGE_FAILURE = 0x261,
    SMSG_DISPEL_FAILED = 0x262,
    SMSG_SPELLORDAMAGE_IMMUNE = 0x263,
    CMSG_AUCTION_LIST_BIDDER_ITEMS = 0x264,
    SMSG_AUCTION_BIDDER_LIST_RESULT = 0x265,
    SMSG_SET_FLAT_SPELL_MODIFIER = 0x266,
    SMSG_SET_PCT_SPELL_MODIFIER = 0x267,
    CMSG_SET_AMMO = 0x268,
    SMSG_CORPSE_RECLAIM_DELAY = 0x269,
    CMSG_SET_ACTIVE_MOVER = 0x26A,
    CMSG_PET_CANCEL_AURA = 0x26B,
    CMSG_PLAYER_AI_CHEAT = 0x26C,
    CMSG_CANCEL_AUTO_REPEAT_SPELL = 0x26D,
    MSG_GM_ACCOUNT_ONLINE = 0x26E,
    MSG_LIST_STABLED_PETS = 0x26F,
    CMSG_STABLE_PET = 0x270,
    CMSG_UNSTABLE_PET = 0x271,
    CMSG_BUY_STABLE_SLOT = 0x272,
    SMSG_STABLE_RESULT = 0x273,
    CMSG_STABLE_REVIVE_PET = 0x274,
    CMSG_STABLE_SWAP_PET = 0x275,
    MSG_QUEST_PUSH_RESULT = 0x276,
    SMSG_PLAY_MUSIC = 0x277,
    SMSG_PLAY_OBJECT_SOUND = 0x278,
    CMSG_REQUEST_PET_INFO = 0x279,
    CMSG_FAR_SIGHT = 0x27A,
    SMSG_SPELLDISPELLOG = 0x27B,
    SMSG_DAMAGE_CALC_LOG = 0x27C,
    CMSG_ENABLE_DAMAGE_LOG = 0x27D,
    CMSG_GROUP_CHANGE_SUB_GROUP = 0x27E,
    CMSG_REQUEST_PARTY_MEMBER_STATS = 0x27F,
    CMSG_GROUP_SWAP_SUB_GROUP = 0x280,
    CMSG_RESET_FACTION_CHEAT = 0x281,
    CMSG_AUTOSTORE_BANK_ITEM = 0x282,
    CMSG_AUTOBANK_ITEM = 0x283,
    MSG_QUERY_NEXT_MAIL_TIME = 0x284,
    SMSG_RECEIVED_MAIL = 0x285,
    SMSG_RAID_GROUP_ONLY = 0x286,
    CMSG_SET_DURABILITY_CHEAT = 0x287,
    CMSG_SET_PVP_RANK_CHEAT = 0x288,
    CMSG_ADD_PVP_MEDAL_CHEAT = 0x289,
    CMSG_DEL_PVP_MEDAL_CHEAT = 0x28A,
    CMSG_SET_PVP_TITLE = 0x28B,
    SMSG_PVP_CREDIT = 0x28C,
    SMSG_AUCTION_REMOVED_NOTIFICATION = 0x28D,
    CMSG_GROUP_RAID_CONVERT = 0x28E,
    CMSG_GROUP_ASSISTANT = 0x28F,
    CMSG_BUYBACK_ITEM = 0x290,
    SMSG_SERVER_MESSAGE = 0x291,
    CMSG_MEETINGSTONE_JOIN = 0x292,
    CMSG_MEETINGSTONE_LEAVE = 0x293,
    CMSG_MEETINGSTONE_CHEAT = 0x294,
    SMSG_MEETINGSTONE_SETQUEUE = 0x295,
    CMSG_MEETINGSTONE_INFO = 0x296,
    SMSG_MEETINGSTONE_COMPLETE = 0x297,
    SMSG_MEETINGSTONE_IN_PROGRESS = 0x298,
    SMSG_MEETINGSTONE_MEMBER_ADDED = 0x299,
    CMSG_GMTICKETSYSTEM_TOGGLE = 0x29A,
    CMSG_CANCEL_GROWTH_AURA = 0x29B,
    SMSG_CANCEL_AUTO_REPEAT = 0x29C,
    SMSG_STANDSTATE_CHANGE_ACK = 0x29D,
    SMSG_LOOT_ALL_PASSED = 0x29E,
    SMSG_LOOT_ROLL_WON = 0x29F,
    CMSG_LOOT_ROLL = 0x2A0,
    SMSG_LOOT_START_ROLL = 0x2A1,
    SMSG_LOOT_ROLL = 0x2A2,
    CMSG_LOOT_MASTER_GIVE = 0x2A3,
    SMSG_LOOT_MASTER_LIST = 0x2A4,
    SMSG_SET_FORCED_REACTIONS = 0x2A5,
    SMSG_SPELL_FAILED_OTHER = 0x2A6,
    SMSG_GAMEOBJECT_RESET_STATE = 0x2A7,
    CMSG_REPAIR_ITEM = 0x2A8,
    SMSG_CHAT_PLAYER_NOT_FOUND = 0x2A9,
    MSG_TALENT_WIPE_CONFIRM = 0x2AA,
    SMSG_SUMMON_REQUEST = 0x2AB,
    CMSG_SUMMON_RESPONSE = 0x2AC,
    MSG_MOVE_TOGGLE_GRAVITY_CHEAT = 0x2AD,
    SMSG_MONSTER_MOVE_TRANSPORT = 0x2AE,
    SMSG_PET_BROKEN = 0x2AF,
    MSG_MOVE_FEATHER_FALL = 0x2B0,
    MSG_MOVE_WATER_WALK = 0x2B1,
    CMSG_SERVER_BROADCAST = 0x2B2,
    CMSG_SELF_RES = 0x2B3,
    SMSG_FEIGN_DEATH_RESISTED = 0x2B4,
    CMSG_RUN_SCRIPT = 0x2B5,
    SMSG_SCRIPT_MESSAGE = 0x2B6,
    SMSG_DUEL_COUNTDOWN = 0x2B7,
    SMSG_AREA_TRIGGER_MESSAGE = 0x2B8,
    CMSG_TOGGLE_HELM = 0x2B9,
    CMSG_TOGGLE_CLOAK = 0x2BA,
    SMSG_MEETINGSTONE_JOINFAILED = 0x2BB,
    SMSG_PLAYER_SKINNED = 0x2BC,
    SMSG_DURABILITY_DAMAGE_DEATH = 0x2BD,
    CMSG_SET_EXPLORATION = 0x2BE,
    CMSG_SET_ACTIONBAR_TOGGLES = 0x2BF,
    UMSG_DELETE_GUILD_CHARTER = 0x2C0,
    MSG_PETITION_RENAME = 0x2C1,
    SMSG_INIT_WORLD_STATES = 0x2C2,
    SMSG_UPDATE_WORLD_STATE = 0x2C3,
    CMSG_ITEM_NAME_QUERY = 0x2C4,
    SMSG_ITEM_NAME_QUERY_RESPONSE = 0x2C5,
    SMSG_PET_ACTION_FEEDBACK = 0x2C6,
    CMSG_CHAR_RENAME = 0x2C7,
    SMSG_CHAR_RENAME = 0x2C8,
    CMSG_MOVE_SPLINE_DONE = 0x2C9,
    CMSG_MOVE_FALL_RESET = 0x2CA,
    SMSG_INSTANCE_SAVE_CREATED = 0x2CB,
    SMSG_RAID_INSTANCE_INFO = 0x2CC,
    CMSG_REQUEST_RAID_INFO = 0x2CD,
    CMSG_MOVE_TIME_SKIPPED = 0x2CE,
    CMSG_MOVE_FEATHER_FALL_ACK = 0x2CF,
    CMSG_MOVE_WATER_WALK_ACK = 0x2D0,
    CMSG_MOVE_NOT_ACTIVE_MOVER = 0x2D1,
    SMSG_PLAY_SOUND = 0x2D2,
    CMSG_BATTLEFIELD_STATUS = 0x2D3,
    SMSG_BATTLEFIELD_STATUS = 0x2D4,
    CMSG_BATTLEFIELD_PORT = 0x2D5,
    MSG_INSPECT_HONOR_STATS = 0x2D6,
    CMSG_BATTLEMASTER_HELLO = 0x2D7,
    CMSG_MOVE_START_SWIM_CHEAT = 0x2D8,
    CMSG_MOVE_STOP_SWIM_CHEAT = 0x2D9,
    SMSG_FORCE_WALK_SPEED_CHANGE = 0x2DA,
    CMSG_FORCE_WALK_SPEED_CHANGE_ACK = 0x2DB,
    SMSG_FORCE_SWIM_BACK_SPEED_CHANGE = 0x2DC,
    CMSG_FORCE_SWIM_BACK_SPEED_CHANGE_ACK = 0x2DD,
    SMSG_FORCE_TURN_RATE_CHANGE = 0x2DE,
    CMSG_FORCE_TURN_RATE_CHANGE_ACK = 0x2DF,
    MSG_PVP_LOG_DATA = 0x2E0,
    CMSG_LEAVE_BATTLEFIELD = 0x2E1,
    CMSG_AREA_SPIRIT_HEALER_QUERY = 0x2E2,
    CMSG_AREA_SPIRIT_HEALER_QUEUE = 0x2E3,
    SMSG_AREA_SPIRIT_HEALER_TIME = 0x2E4,
    CMSG_GM_UNTEACH = 0x2E5,
    SMSG_WARDEN_DATA = 0x2E6,
    CMSG_WARDEN_DATA = 0x2E7,
    SMSG_GROUP_JOINED_BATTLEGROUND = 0x2E8,
    MSG_BATTLEGROUND_PLAYER_POSITIONS = 0x2E9,
    CMSG_PET_STOP_ATTACK = 0x2EA,
    SMSG_BINDER_CONFIRM = 0x2EB,
    SMSG_BATTLEGROUND_PLAYER_JOINED = 0x2EC,
    SMSG_BATTLEGROUND_PLAYER_LEFT = 0x2ED,
    CMSG_BATTLEMASTER_JOIN = 0x2EE,
    SMSG_ADDON_INFO = 0x2EF,
    CMSG_PET_UNLEARN = 0x2F0,
    SMSG_PET_UNLEARN_CONFIRM = 0x2F1,
    SMSG_PARTY_MEMBER_STATS_FULL = 0x2F2,
    CMSG_PET_SPELL_AUTOCAST = 0x2F3,
    SMSG_WEATHER = 0x2F4,
    SMSG_PLAY_TIME_WARNING = 0x2F5,
    SMSG_MINIGAME_SETUP = 0x2F6,
    SMSG_MINIGAME_STATE = 0x2F7,
    CMSG_MINIGAME_MOVE = 0x2F8,
    SMSG_MINIGAME_MOVE_FAILED = 0x2F9,
    SMSG_RAID_INSTANCE_MESSAGE = 0x2FA,
    SMSG_COMPRESSED_MOVES = 0x2FB,
    CMSG_GUILD_INFO_TEXT = 0x2FC,
    SMSG_CHAT_RESTRICTED = 0x2FD,
    SMSG_SPLINE_SET_RUN_SPEED = 0x2FE,
    SMSG_SPLINE_SET_RUN_BACK_SPEED = 0x2FF,
    SMSG_SPLINE_SET_SWIM_SPEED = 0x300,
    SMSG_SPLINE_SET_WALK_SPEED = 0x301,
    SMSG_SPLINE_SET_SWIM_BACK_SPEED = 0x302,
    SMSG_SPLINE_SET_TURN_RATE = 0x303,
    SMSG_SPLINE_MOVE_UNROOT = 0x304,
    SMSG_SPLINE_MOVE_FEATHER_FALL = 0x305,
    SMSG_SPLINE_MOVE_NORMAL_FALL = 0x306,
    SMSG_SPLINE_MOVE_SET_HOVER = 0x307,
    SMSG_SPLINE_MOVE_UNSET_HOVER = 0x308,
    SMSG_SPLINE_MOVE_WATER_WALK = 0x309,
    SMSG_SPLINE_MOVE_LAND_WALK = 0x30A,
    SMSG_SPLINE_MOVE_START_SWIM = 0x30B,
    SMSG_SPLINE_MOVE_STOP_SWIM = 0x30C,
    SMSG_SPLINE_MOVE_SET_RUN_MODE = 0x30D,
    SMSG_SPLINE_MOVE_SET_WALK_MODE = 0x30E,
    CMSG_GM_NUKE_ACCOUNT = 0x30F,
    MSG_GM_DESTROY_CORPSE = 0x310,
    CMSG_GM_DESTROY_ONLINE_CORPSE = 0x311,
    CMSG_ACTIVATETAXI_FAR = 0x312,
    SMSG_SET_FACTION_ATWAR = 0x313,
    SMSG_GAMETIMEBIAS_SET = 0x314,
    CMSG_DEBUG_ACTIONS_START = 0x315,
    CMSG_DEBUG_ACTIONS_STOP = 0x316,
    CMSG_SET_FACTION_INACTIVE = 0x317,
    CMSG_SET_WATCHED_FACTION = 0x318,
    MSG_MOVE_TIME_SKIPPED = 0x319,
    SMSG_SPLINE_MOVE_ROOT = 0x31A,
    CMSG_SET_EXPLORATION_ALL = 0x31B,
    SMSG_INVALIDATE_PLAYER = 0x31C,
    CMSG_RESET_INSTANCES = 0x31D,
    SMSG_INSTANCE_RESET = 0x31E,
    SMSG_INSTANCE_RESET_FAILED = 0x31F,
    SMSG_UPDATE_LAST_INSTANCE = 0x320,
    MSG_RAID_ICON_TARGET = 0x321,
    MSG_RAID_READY_CHECK = 0x322,
    CMSG_LUA_USAGE = 0x323,
    SMSG_PET_ACTION_SOUND = 0x324,
    SMSG_PET_DISMISS_SOUND = 0x325,
    SMSG_GHOSTEE_GONE = 0x326,
    CMSG_GM_UPDATE_TICKET_STATUS = 0x327,
    SMSG_GM_TICKET_STATUS_UPDATE = 0x328,
    CMSG_GMSURVEY_SUBMIT = 0x32A,
    SMSG_UPDATE_INSTANCE_OWNERSHIP = 0x32B,
    CMSG_IGNORE_KNOCKBACK_CHEAT = 0x32C,
    SMSG_CHAT_PLAYER_AMBIGUOUS = 0x32D,
    MSG_DELAY_GHOST_TELEPORT = 0x32E,
    SMSG_SPELLINSTAKILLLOG = 0x32F,
    SMSG_SPELL_UPDATE_CHAIN_TARGETS = 0x330,
    CMSG_CHAT_FILTERED = 0x331,
    SMSG_EXPECTED_SPAM_RECORDS = 0x332,
    SMSG_SPELLSTEALLOG = 0x333,
    CMSG_LOTTERY_QUERY_OBSOLETE = 0x334,
    SMSG_LOTTERY_QUERY_RESULT_OBSOLETE = 0x335,
    CMSG_BUY_LOTTERY_TICKET_OBSOLETE = 0x336,
    SMSG_LOTTERY_RESULT_OBSOLETE = 0x337,
    SMSG_CHARACTER_PROFILE = 0x338,
    SMSG_CHARACTER_PROFILE_REALM_CONNECTED = 0x339,
    SMSG_DEFENSE_MESSAGE = 0x33A,
    MSG_GM_RESETINSTANCELIMIT = 0x33C,

    // // SMSG_MOTD                                      = &H33D
    SMSG_MOVE_SET_FLIGHT = 0x33E,

    SMSG_MOVE_UNSET_FLIGHT = 0x33F,
    CMSG_MOVE_FLIGHT_ACK = 0x340,
    MSG_MOVE_START_SWIM_CHEAT = 0x341,
    MSG_MOVE_STOP_SWIM_CHEAT = 0x342,
    SMSG_OUTDOORPVP_NOTIFY = 0x33B
}
