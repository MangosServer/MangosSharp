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
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Network;
using System;

namespace Mangos.World.Handlers;

public class WS_Handlers
{
    public void IntializePacketHandlers()
    {
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_MOVE_ROOT_ACK] = OnUnhandledPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_MOVE_UNROOT_ACK] = OnUnhandledPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MOVE_WATER_WALK_ACK] = OnUnhandledPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_TELEPORT_ACK] = OnUnhandledPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_WARDEN_DATA] = WorldServiceLocator._WS_Handlers_Warden.On_CMSG_WARDEN_DATA;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_NAME_QUERY] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_NAME_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MESSAGECHAT] = WorldServiceLocator._WS_Handlers_Chat.On_CMSG_MESSAGECHAT;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_LOGOUT_REQUEST] = WorldServiceLocator._CharManagementHandler.On_CMSG_LOGOUT_REQUEST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_LOGOUT_CANCEL] = WorldServiceLocator._CharManagementHandler.On_CMSG_LOGOUT_CANCEL;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_TRADE] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_CANCEL_TRADE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BEGIN_TRADE] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_BEGIN_TRADE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_UNACCEPT_TRADE] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_UNACCEPT_TRADE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_ACCEPT_TRADE] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_ACCEPT_TRADE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_INITIATE_TRADE] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_INITIATE_TRADE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_TRADE_GOLD] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_SET_TRADE_GOLD;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_TRADE_ITEM] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_SET_TRADE_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_CLEAR_TRADE_ITEM] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_CLEAR_TRADE_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_IGNORE_TRADE] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_IGNORE_TRADE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BUSY_TRADE] = WorldServiceLocator._WS_Handlers_Trade.On_CMSG_BUSY_TRADE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_FORWARD] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_BACKWARD] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_STRAFE_LEFT] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_STRAFE_RIGHT] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP_STRAFE] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_JUMP] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_TURN_LEFT] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_TURN_RIGHT] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP_TURN] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_PITCH_UP] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_PITCH_DOWN] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP_PITCH] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_SET_RUN_MODE] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_SET_WALK_MODE] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_SWIM] = WorldServiceLocator._WS_CharMovement.OnStartSwim;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP_SWIM] = WorldServiceLocator._WS_CharMovement.OnStopSwim;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_SET_FACING] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_SET_PITCH] = WorldServiceLocator._WS_CharMovement.OnMovementPacket;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MOVE_FALL_RESET] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_MOVE_FALL_RESET;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_HEARTBEAT] = WorldServiceLocator._WS_CharMovement.On_MSG_MOVE_HEARTBEAT;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AREATRIGGER] = WorldServiceLocator._WS_CharMovement.On_CMSG_AREATRIGGER;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_MOVE_FALL_LAND] = WorldServiceLocator._WS_CharMovement.On_MSG_MOVE_FALL_LAND;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_ZONEUPDATE] = WorldServiceLocator._WS_CharMovement.On_CMSG_ZONEUPDATE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_RUN_SPEED_CHANGE_ACK] = WorldServiceLocator._WS_CharMovement.OnChangeSpeed;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_RUN_BACK_SPEED_CHANGE_ACK] = WorldServiceLocator._WS_CharMovement.OnChangeSpeed;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_SWIM_SPEED_CHANGE_ACK] = WorldServiceLocator._WS_CharMovement.OnChangeSpeed;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_SWIM_BACK_SPEED_CHANGE_ACK] = WorldServiceLocator._WS_CharMovement.OnChangeSpeed;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_TURN_RATE_CHANGE_ACK] = WorldServiceLocator._WS_CharMovement.OnChangeSpeed;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_STANDSTATECHANGE] = WorldServiceLocator._CharManagementHandler.On_CMSG_STANDSTATECHANGE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_SELECTION] = WorldServiceLocator._WS_Combat.On_CMSG_SET_SELECTION;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_REPOP_REQUEST] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_REPOP_REQUEST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_CORPSE_QUERY] = WorldServiceLocator._WS_Handlers_Misc.On_MSG_CORPSE_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SPIRIT_HEALER_ACTIVATE] = WorldServiceLocator._WS_Creatures.On_CMSG_SPIRIT_HEALER_ACTIVATE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_RECLAIM_CORPSE] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_RECLAIM_CORPSE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TUTORIAL_FLAG] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_TUTORIAL_FLAG;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TUTORIAL_CLEAR] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_TUTORIAL_CLEAR;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TUTORIAL_RESET] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_TUTORIAL_RESET;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_ACTION_BUTTON] = WorldServiceLocator._CharManagementHandler.On_CMSG_SET_ACTION_BUTTON;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_ACTIONBAR_TOGGLES] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_SET_ACTIONBAR_TOGGLES;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TOGGLE_HELM] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_TOGGLE_HELM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TOGGLE_CLOAK] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_TOGGLE_CLOAK;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MOUNTSPECIAL_ANIM] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_MOUNTSPECIAL_ANIM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_EMOTE] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_EMOTE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TEXT_EMOTE] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_TEXT_EMOTE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_ITEM_QUERY_SINGLE] = WorldServiceLocator._WS_Items.On_CMSG_ITEM_QUERY_SINGLE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_ITEM_NAME_QUERY] = WorldServiceLocator._WS_Items.On_CMSG_ITEM_NAME_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SETSHEATHED] = WorldServiceLocator._WS_Combat.On_CMSG_SETSHEATHED;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SWAP_INV_ITEM] = WorldServiceLocator._WS_Items.On_CMSG_SWAP_INV_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SPLIT_ITEM] = WorldServiceLocator._WS_Items.On_CMSG_SPLIT_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUTOEQUIP_ITEM] = WorldServiceLocator._WS_Items.On_CMSG_AUTOEQUIP_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUTOSTORE_BAG_ITEM] = WorldServiceLocator._WS_Items.On_CMSG_AUTOSTORE_BAG_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SWAP_ITEM] = WorldServiceLocator._WS_Items.On_CMSG_SWAP_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_DESTROYITEM] = WorldServiceLocator._WS_Items.On_CMSG_DESTROYITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_READ_ITEM] = WorldServiceLocator._WS_Items.On_CMSG_READ_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PAGE_TEXT_QUERY] = WorldServiceLocator._WS_Items.On_CMSG_PAGE_TEXT_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_USE_ITEM] = WorldServiceLocator._WS_Items.On_CMSG_USE_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_OPEN_ITEM] = WorldServiceLocator._WS_Items.On_CMSG_OPEN_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_WRAP_ITEM] = WorldServiceLocator._WS_Items.On_CMSG_WRAP_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_AMMO] = WorldServiceLocator._WS_Combat.On_CMSG_SET_AMMO;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_CREATURE_QUERY] = WorldServiceLocator._WS_Creatures.On_CMSG_CREATURE_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_GOSSIP_HELLO] = WorldServiceLocator._WS_Creatures.On_CMSG_GOSSIP_HELLO;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_GOSSIP_SELECT_OPTION] = WorldServiceLocator._WS_Creatures.On_CMSG_GOSSIP_SELECT_OPTION;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_NPC_TEXT_QUERY] = WorldServiceLocator._WS_Creatures.On_CMSG_NPC_TEXT_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_LIST_INVENTORY] = WorldServiceLocator._WS_NPCs.On_CMSG_LIST_INVENTORY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BUY_ITEM_IN_SLOT] = WorldServiceLocator._WS_NPCs.On_CMSG_BUY_ITEM_IN_SLOT;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BUY_ITEM] = WorldServiceLocator._WS_NPCs.On_CMSG_BUY_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BUYBACK_ITEM] = WorldServiceLocator._WS_NPCs.On_CMSG_BUYBACK_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SELL_ITEM] = WorldServiceLocator._WS_NPCs.On_CMSG_SELL_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_REPAIR_ITEM] = WorldServiceLocator._WS_NPCs.On_CMSG_REPAIR_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_ATTACKSWING] = WorldServiceLocator._WS_Combat.On_CMSG_ATTACKSWING;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_ATTACKSTOP] = WorldServiceLocator._WS_Combat.On_CMSG_ATTACKSTOP;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_GAMEOBJECT_QUERY] = WorldServiceLocator._WS_GameObjects.On_CMSG_GAMEOBJECT_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_GAMEOBJ_USE] = WorldServiceLocator._WS_GameObjects.On_CMSG_GAMEOBJ_USE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BATTLEFIELD_STATUS] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_BATTLEFIELD_STATUS;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_ACTIVE_MOVER] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_SET_ACTIVE_MOVER;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MEETINGSTONE_INFO] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_MEETINGSTONE_INFO;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_INSPECT_HONOR_STATS] = WorldServiceLocator._WS_Handlers_Misc.On_MSG_INSPECT_HONOR_STATS;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_PVP_LOG_DATA] = WorldServiceLocator._WS_Handlers_Misc.On_MSG_PVP_LOG_DATA;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MOVE_TIME_SKIPPED] = WorldServiceLocator._WS_CharMovement.On_CMSG_MOVE_TIME_SKIPPED;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_GET_MAIL_LIST] = WorldServiceLocator._WS_Mail.On_CMSG_GET_MAIL_LIST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SEND_MAIL] = WorldServiceLocator._WS_Mail.On_CMSG_SEND_MAIL;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_CREATE_TEXT_ITEM] = WorldServiceLocator._WS_Mail.On_CMSG_MAIL_CREATE_TEXT_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_ITEM_TEXT_QUERY] = WorldServiceLocator._WS_Mail.On_CMSG_ITEM_TEXT_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_DELETE] = WorldServiceLocator._WS_Mail.On_CMSG_MAIL_DELETE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_TAKE_ITEM] = WorldServiceLocator._WS_Mail.On_CMSG_MAIL_TAKE_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_TAKE_MONEY] = WorldServiceLocator._WS_Mail.On_CMSG_MAIL_TAKE_MONEY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_RETURN_TO_SENDER] = WorldServiceLocator._WS_Mail.On_CMSG_MAIL_RETURN_TO_SENDER;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_MARK_AS_READ] = WorldServiceLocator._WS_Mail.On_CMSG_MAIL_MARK_AS_READ;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_QUERY_NEXT_MAIL_TIME] = WorldServiceLocator._WS_Mail.On_MSG_QUERY_NEXT_MAIL_TIME;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUTOSTORE_LOOT_ITEM] = WorldServiceLocator._WS_Loot.On_CMSG_AUTOSTORE_LOOT_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_LOOT_MONEY] = WorldServiceLocator._WS_Loot.On_CMSG_LOOT_MONEY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_LOOT] = WorldServiceLocator._WS_Loot.On_CMSG_LOOT;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_LOOT_ROLL] = WorldServiceLocator._WS_Loot.On_CMSG_LOOT_ROLL;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_LOOT_RELEASE] = WorldServiceLocator._WS_Loot.On_CMSG_LOOT_RELEASE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TAXINODE_STATUS_QUERY] = WorldServiceLocator._WS_Handlers_Taxi.On_CMSG_TAXINODE_STATUS_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TAXIQUERYAVAILABLENODES] = WorldServiceLocator._WS_Handlers_Taxi.On_CMSG_TAXIQUERYAVAILABLENODES;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_ACTIVATETAXI] = WorldServiceLocator._WS_Handlers_Taxi.On_CMSG_ACTIVATETAXI;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_ACTIVATETAXI_FAR] = WorldServiceLocator._WS_Handlers_Taxi.On_CMSG_ACTIVATETAXI_FAR;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_MOVE_SPLINE_DONE] = WorldServiceLocator._WS_Handlers_Taxi.On_CMSG_MOVE_SPLINE_DONE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_CAST_SPELL] = WorldServiceLocator._WS_Spells.On_CMSG_CAST_SPELL;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_CAST] = WorldServiceLocator._WS_Spells.On_CMSG_CANCEL_CAST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_AURA] = WorldServiceLocator._WS_Spells.On_CMSG_CANCEL_AURA;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_AUTO_REPEAT_SPELL] = WorldServiceLocator._WS_Spells.On_CMSG_CANCEL_AUTO_REPEAT_SPELL;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_CHANNELLING] = WorldServiceLocator._WS_Spells.On_CMSG_CANCEL_CHANNELLING;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TOGGLE_PVP] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_TOGGLE_PVP;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_BATTLEGROUND_PLAYER_POSITIONS] = WorldServiceLocator._WS_Handlers_Battleground.On_MSG_BATTLEGROUND_PLAYER_POSITIONS;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_STATUS_QUERY] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_STATUS_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_HELLO] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_HELLO;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_QUERY_QUEST] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_QUERY_QUEST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_ACCEPT_QUEST] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_ACCEPT_QUEST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_QUESTLOG_REMOVE_QUEST] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_QUESTLOG_REMOVE_QUEST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_QUEST_QUERY] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_QUEST_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_COMPLETE_QUEST] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_COMPLETE_QUEST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_REQUEST_REWARD] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_REQUEST_REWARD;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_CHOOSE_REWARD] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_CHOOSE_REWARD;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_QUEST_PUSH_RESULT] = WorldServiceLocator._WorldServer.ALLQUESTS.On_MSG_QUEST_PUSH_RESULT;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PUSHQUESTTOPARTY] = WorldServiceLocator._WorldServer.ALLQUESTS.On_CMSG_PUSHQUESTTOPARTY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BINDER_ACTIVATE] = WorldServiceLocator._WS_NPCs.On_CMSG_BINDER_ACTIVATE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BANKER_ACTIVATE] = WorldServiceLocator._WS_NPCs.On_CMSG_BANKER_ACTIVATE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BUY_BANK_SLOT] = WorldServiceLocator._WS_NPCs.On_CMSG_BUY_BANK_SLOT;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUTOBANK_ITEM] = WorldServiceLocator._WS_NPCs.On_CMSG_AUTOBANK_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUTOSTORE_BANK_ITEM] = WorldServiceLocator._WS_NPCs.On_CMSG_AUTOSTORE_BANK_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_TALENT_WIPE_CONFIRM] = WorldServiceLocator._WS_NPCs.On_MSG_TALENT_WIPE_CONFIRM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TRAINER_BUY_SPELL] = WorldServiceLocator._WS_NPCs.On_CMSG_TRAINER_BUY_SPELL;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_TRAINER_LIST] = WorldServiceLocator._WS_NPCs.On_CMSG_TRAINER_LIST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_AUCTION_HELLO] = WorldServiceLocator._WS_Auction.On_MSG_AUCTION_HELLO;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_SELL_ITEM] = WorldServiceLocator._WS_Auction.On_CMSG_AUCTION_SELL_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_REMOVE_ITEM] = WorldServiceLocator._WS_Auction.On_CMSG_AUCTION_REMOVE_ITEM;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_LIST_ITEMS] = WorldServiceLocator._WS_Auction.On_CMSG_AUCTION_LIST_ITEMS;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_LIST_OWNER_ITEMS] = WorldServiceLocator._WS_Auction.On_CMSG_AUCTION_LIST_OWNER_ITEMS;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_PLACE_BID] = WorldServiceLocator._WS_Auction.On_CMSG_AUCTION_PLACE_BID;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_LIST_BIDDER_ITEMS] = WorldServiceLocator._WS_Auction.On_CMSG_AUCTION_LIST_BIDDER_ITEMS;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_SHOWLIST] = WorldServiceLocator._WS_Guilds.On_CMSG_PETITION_SHOWLIST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_BUY] = WorldServiceLocator._WS_Guilds.On_CMSG_PETITION_BUY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_SHOW_SIGNATURES] = WorldServiceLocator._WS_Guilds.On_CMSG_PETITION_SHOW_SIGNATURES;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_QUERY] = WorldServiceLocator._WS_Guilds.On_CMSG_PETITION_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_OFFER_PETITION] = WorldServiceLocator._WS_Guilds.On_CMSG_OFFER_PETITION;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_SIGN] = WorldServiceLocator._WS_Guilds.On_CMSG_PETITION_SIGN;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_PETITION_RENAME] = WorldServiceLocator._WS_Guilds.On_MSG_PETITION_RENAME;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.MSG_PETITION_DECLINE] = WorldServiceLocator._WS_Guilds.On_MSG_PETITION_DECLINE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BATTLEMASTER_HELLO] = WorldServiceLocator._WS_Handlers_Battleground.On_CMSG_BATTLEMASTER_HELLO;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_BATTLEFIELD_LIST] = WorldServiceLocator._WS_Handlers_Battleground.On_CMSG_BATTLEMASTER_HELLO;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_DUEL_CANCELLED] = WorldServiceLocator._WS_Spells.On_CMSG_DUEL_CANCELLED;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_DUEL_ACCEPTED] = WorldServiceLocator._WS_Spells.On_CMSG_DUEL_ACCEPTED;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_RESURRECT_RESPONSE] = WorldServiceLocator._WS_Spells.On_CMSG_RESURRECT_RESPONSE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_LEARN_TALENT] = WorldServiceLocator._WS_Spells.On_CMSG_LEARN_TALENT;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_WORLD_TELEPORT] = WorldServiceLocator._WS_Handlers_Gamemaster.On_CMSG_WORLD_TELEPORT;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_FACTION_ATWAR] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_SET_FACTION_ATWAR;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_FACTION_INACTIVE] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_SET_FACTION_INACTIVE;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_SET_WATCHED_FACTION] = WorldServiceLocator._WS_Handlers_Misc.On_CMSG_SET_WATCHED_FACTION;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PET_NAME_QUERY] = WorldServiceLocator._WS_Pets.On_CMSG_PET_NAME_QUERY;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_REQUEST_PET_INFO] = WorldServiceLocator._WS_Pets.On_CMSG_REQUEST_PET_INFO;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PET_ACTION] = WorldServiceLocator._WS_Pets.On_CMSG_PET_ACTION;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PET_CANCEL_AURA] = WorldServiceLocator._WS_Pets.On_CMSG_PET_CANCEL_AURA;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PET_ABANDON] = WorldServiceLocator._WS_Pets.On_CMSG_PET_ABANDON;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PET_RENAME] = WorldServiceLocator._WS_Pets.On_CMSG_PET_RENAME;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PET_SET_ACTION] = WorldServiceLocator._WS_Pets.On_CMSG_PET_SET_ACTION;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PET_SPELL_AUTOCAST] = WorldServiceLocator._WS_Pets.On_CMSG_PET_SPELL_AUTOCAST;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PET_STOP_ATTACK] = WorldServiceLocator._WS_Pets.On_CMSG_PET_STOP_ATTACK;
        WorldServiceLocator._WorldServer.PacketHandlers[Opcodes.CMSG_PET_UNLEARN] = WorldServiceLocator._WS_Pets.On_CMSG_PET_UNLEARN;
    }

    public void OnUnhandledPacket(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Unhandled Packet]", client.IP, client.Port, packet.OpCode);
    }

    public void OnWorldPacket(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Redirected Packet]", client.IP, client.Port, packet.OpCode);
        if (client.Character == null || !client.Character.FullyLoggedIn)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Unknown Opcode 0x{2:X} [{2}], DataLen={4}", client.IP, client.Port, packet.OpCode, Environment.NewLine, packet.Length);
            WorldServiceLocator._Packets.DumpPacket(packet.Data, client);
        }
    }
}
