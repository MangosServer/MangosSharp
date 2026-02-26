//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_MOVE_ROOT_ACK] = OnUnhandledPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_MOVE_UNROOT_ACK] = OnUnhandledPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MOVE_WATER_WALK_ACK] = OnUnhandledPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_TELEPORT_ACK] = OnUnhandledPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_WARDEN_DATA] = WorldServiceLocator.WSHandlersWarden.On_CMSG_WARDEN_DATA;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_NAME_QUERY] = WorldServiceLocator.WSHandlersMisc.On_CMSG_NAME_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MESSAGECHAT] = WorldServiceLocator.WSHandlersChat.On_CMSG_MESSAGECHAT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LOGOUT_REQUEST] = WorldServiceLocator.CharManagementHandler.On_CMSG_LOGOUT_REQUEST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LOGOUT_CANCEL] = WorldServiceLocator.CharManagementHandler.On_CMSG_LOGOUT_CANCEL;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_TRADE] = WorldServiceLocator.WSHandlersTrade.On_CMSG_CANCEL_TRADE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BEGIN_TRADE] = WorldServiceLocator.WSHandlersTrade.On_CMSG_BEGIN_TRADE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_UNACCEPT_TRADE] = WorldServiceLocator.WSHandlersTrade.On_CMSG_UNACCEPT_TRADE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_ACCEPT_TRADE] = WorldServiceLocator.WSHandlersTrade.On_CMSG_ACCEPT_TRADE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_INITIATE_TRADE] = WorldServiceLocator.WSHandlersTrade.On_CMSG_INITIATE_TRADE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_TRADE_GOLD] = WorldServiceLocator.WSHandlersTrade.On_CMSG_SET_TRADE_GOLD;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_TRADE_ITEM] = WorldServiceLocator.WSHandlersTrade.On_CMSG_SET_TRADE_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CLEAR_TRADE_ITEM] = WorldServiceLocator.WSHandlersTrade.On_CMSG_CLEAR_TRADE_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_IGNORE_TRADE] = WorldServiceLocator.WSHandlersTrade.On_CMSG_IGNORE_TRADE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BUSY_TRADE] = WorldServiceLocator.WSHandlersTrade.On_CMSG_BUSY_TRADE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_FORWARD] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_BACKWARD] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_STRAFE_LEFT] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_STRAFE_RIGHT] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP_STRAFE] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_JUMP] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_TURN_LEFT] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_TURN_RIGHT] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP_TURN] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_PITCH_UP] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_PITCH_DOWN] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP_PITCH] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_SET_RUN_MODE] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_SET_WALK_MODE] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_START_SWIM] = WorldServiceLocator.WSCharMovement.OnStartSwim;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_STOP_SWIM] = WorldServiceLocator.WSCharMovement.OnStopSwim;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_SET_FACING] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_SET_PITCH] = WorldServiceLocator.WSCharMovement.OnMovementPacket;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MOVE_FALL_RESET] = WorldServiceLocator.WSHandlersMisc.On_CMSG_MOVE_FALL_RESET;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_HEARTBEAT] = WorldServiceLocator.WSCharMovement.On_MSG_MOVE_HEARTBEAT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AREATRIGGER] = WorldServiceLocator.WSCharMovement.On_CMSG_AREATRIGGER;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_MOVE_FALL_LAND] = WorldServiceLocator.WSCharMovement.On_MSG_MOVE_FALL_LAND;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_ZONEUPDATE] = WorldServiceLocator.WSCharMovement.On_CMSG_ZONEUPDATE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_RUN_SPEED_CHANGE_ACK] = WorldServiceLocator.WSCharMovement.OnChangeSpeed;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_RUN_BACK_SPEED_CHANGE_ACK] = WorldServiceLocator.WSCharMovement.OnChangeSpeed;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_SWIM_SPEED_CHANGE_ACK] = WorldServiceLocator.WSCharMovement.OnChangeSpeed;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_SWIM_BACK_SPEED_CHANGE_ACK] = WorldServiceLocator.WSCharMovement.OnChangeSpeed;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_FORCE_TURN_RATE_CHANGE_ACK] = WorldServiceLocator.WSCharMovement.OnChangeSpeed;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_STANDSTATECHANGE] = WorldServiceLocator.CharManagementHandler.On_CMSG_STANDSTATECHANGE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_SELECTION] = WorldServiceLocator.WSCombat.On_CMSG_SET_SELECTION;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_REPOP_REQUEST] = WorldServiceLocator.WSHandlersMisc.On_CMSG_REPOP_REQUEST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_CORPSE_QUERY] = WorldServiceLocator.WSHandlersMisc.On_MSG_CORPSE_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SPIRIT_HEALER_ACTIVATE] = WorldServiceLocator.WSCreatures.On_CMSG_SPIRIT_HEALER_ACTIVATE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_RECLAIM_CORPSE] = WorldServiceLocator.WSHandlersMisc.On_CMSG_RECLAIM_CORPSE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TUTORIAL_FLAG] = WorldServiceLocator.WSHandlersMisc.On_CMSG_TUTORIAL_FLAG;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TUTORIAL_CLEAR] = WorldServiceLocator.WSHandlersMisc.On_CMSG_TUTORIAL_CLEAR;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TUTORIAL_RESET] = WorldServiceLocator.WSHandlersMisc.On_CMSG_TUTORIAL_RESET;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_ACTION_BUTTON] = WorldServiceLocator.CharManagementHandler.On_CMSG_SET_ACTION_BUTTON;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_ACTIONBAR_TOGGLES] = WorldServiceLocator.WSHandlersMisc.On_CMSG_SET_ACTIONBAR_TOGGLES;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TOGGLE_HELM] = WorldServiceLocator.WSHandlersMisc.On_CMSG_TOGGLE_HELM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TOGGLE_CLOAK] = WorldServiceLocator.WSHandlersMisc.On_CMSG_TOGGLE_CLOAK;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MOUNTSPECIAL_ANIM] = WorldServiceLocator.WSHandlersMisc.On_CMSG_MOUNTSPECIAL_ANIM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_EMOTE] = WorldServiceLocator.WSHandlersMisc.On_CMSG_EMOTE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TEXT_EMOTE] = WorldServiceLocator.WSHandlersMisc.On_CMSG_TEXT_EMOTE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_ITEM_QUERY_SINGLE] = WorldServiceLocator.WSItems.On_CMSG_ITEM_QUERY_SINGLE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_ITEM_NAME_QUERY] = WorldServiceLocator.WSItems.On_CMSG_ITEM_NAME_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SETSHEATHED] = WorldServiceLocator.WSCombat.On_CMSG_SETSHEATHED;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SWAP_INV_ITEM] = WorldServiceLocator.WSItems.On_CMSG_SWAP_INV_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SPLIT_ITEM] = WorldServiceLocator.WSItems.On_CMSG_SPLIT_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUTOEQUIP_ITEM] = WorldServiceLocator.WSItems.On_CMSG_AUTOEQUIP_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUTOSTORE_BAG_ITEM] = WorldServiceLocator.WSItems.On_CMSG_AUTOSTORE_BAG_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SWAP_ITEM] = WorldServiceLocator.WSItems.On_CMSG_SWAP_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_DESTROYITEM] = WorldServiceLocator.WSItems.On_CMSG_DESTROYITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_READ_ITEM] = WorldServiceLocator.WSItems.On_CMSG_READ_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PAGE_TEXT_QUERY] = WorldServiceLocator.WSItems.On_CMSG_PAGE_TEXT_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_USE_ITEM] = WorldServiceLocator.WSItems.On_CMSG_USE_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_OPEN_ITEM] = WorldServiceLocator.WSItems.On_CMSG_OPEN_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_WRAP_ITEM] = WorldServiceLocator.WSItems.On_CMSG_WRAP_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_AMMO] = WorldServiceLocator.WSCombat.On_CMSG_SET_AMMO;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CREATURE_QUERY] = WorldServiceLocator.WSCreatures.On_CMSG_CREATURE_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_GOSSIP_HELLO] = WorldServiceLocator.WSCreatures.On_CMSG_GOSSIP_HELLO;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_GOSSIP_SELECT_OPTION] = WorldServiceLocator.WSCreatures.On_CMSG_GOSSIP_SELECT_OPTION;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_NPC_TEXT_QUERY] = WorldServiceLocator.WSCreatures.On_CMSG_NPC_TEXT_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LIST_INVENTORY] = WorldServiceLocator.WSNPCs.On_CMSG_LIST_INVENTORY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BUY_ITEM_IN_SLOT] = WorldServiceLocator.WSNPCs.On_CMSG_BUY_ITEM_IN_SLOT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BUY_ITEM] = WorldServiceLocator.WSNPCs.On_CMSG_BUY_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BUYBACK_ITEM] = WorldServiceLocator.WSNPCs.On_CMSG_BUYBACK_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SELL_ITEM] = WorldServiceLocator.WSNPCs.On_CMSG_SELL_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_REPAIR_ITEM] = WorldServiceLocator.WSNPCs.On_CMSG_REPAIR_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_ATTACKSWING] = WorldServiceLocator.WSCombat.On_CMSG_ATTACKSWING;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_ATTACKSTOP] = WorldServiceLocator.WSCombat.On_CMSG_ATTACKSTOP;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_GAMEOBJECT_QUERY] = WorldServiceLocator.WSGameObjects.On_CMSG_GAMEOBJECT_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_GAMEOBJ_USE] = WorldServiceLocator.WSGameObjects.On_CMSG_GAMEOBJ_USE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BATTLEFIELD_STATUS] = WorldServiceLocator.WSHandlersMisc.On_CMSG_BATTLEFIELD_STATUS;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_ACTIVE_MOVER] = WorldServiceLocator.WSHandlersMisc.On_CMSG_SET_ACTIVE_MOVER;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MEETINGSTONE_INFO] = WorldServiceLocator.WSHandlersMisc.On_CMSG_MEETINGSTONE_INFO;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_INSPECT_HONOR_STATS] = WorldServiceLocator.WSHandlersMisc.On_MSG_INSPECT_HONOR_STATS;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_PVP_LOG_DATA] = WorldServiceLocator.WSHandlersMisc.On_MSG_PVP_LOG_DATA;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MOVE_TIME_SKIPPED] = WorldServiceLocator.WSCharMovement.On_CMSG_MOVE_TIME_SKIPPED;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_GET_MAIL_LIST] = WorldServiceLocator.WSMail.On_CMSG_GET_MAIL_LIST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SEND_MAIL] = WorldServiceLocator.WSMail.On_CMSG_SEND_MAIL;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_CREATE_TEXT_ITEM] = WorldServiceLocator.WSMail.On_CMSG_MAIL_CREATE_TEXT_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_ITEM_TEXT_QUERY] = WorldServiceLocator.WSMail.On_CMSG_ITEM_TEXT_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_DELETE] = WorldServiceLocator.WSMail.On_CMSG_MAIL_DELETE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_TAKE_ITEM] = WorldServiceLocator.WSMail.On_CMSG_MAIL_TAKE_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_TAKE_MONEY] = WorldServiceLocator.WSMail.On_CMSG_MAIL_TAKE_MONEY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_RETURN_TO_SENDER] = WorldServiceLocator.WSMail.On_CMSG_MAIL_RETURN_TO_SENDER;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MAIL_MARK_AS_READ] = WorldServiceLocator.WSMail.On_CMSG_MAIL_MARK_AS_READ;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_QUERY_NEXT_MAIL_TIME] = WorldServiceLocator.WSMail.On_MSG_QUERY_NEXT_MAIL_TIME;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUTOSTORE_LOOT_ITEM] = WorldServiceLocator.WSLoot.On_CMSG_AUTOSTORE_LOOT_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LOOT_MONEY] = WorldServiceLocator.WSLoot.On_CMSG_LOOT_MONEY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LOOT] = WorldServiceLocator.WSLoot.On_CMSG_LOOT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LOOT_ROLL] = WorldServiceLocator.WSLoot.On_CMSG_LOOT_ROLL;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LOOT_RELEASE] = WorldServiceLocator.WSLoot.On_CMSG_LOOT_RELEASE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TAXINODE_STATUS_QUERY] = WorldServiceLocator.WSHandlersTaxi.On_CMSG_TAXINODE_STATUS_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TAXIQUERYAVAILABLENODES] = WorldServiceLocator.WSHandlersTaxi.On_CMSG_TAXIQUERYAVAILABLENODES;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_ACTIVATETAXI] = WorldServiceLocator.WSHandlersTaxi.On_CMSG_ACTIVATETAXI;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_ACTIVATETAXI_FAR] = WorldServiceLocator.WSHandlersTaxi.On_CMSG_ACTIVATETAXI_FAR;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_MOVE_SPLINE_DONE] = WorldServiceLocator.WSHandlersTaxi.On_CMSG_MOVE_SPLINE_DONE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CAST_SPELL] = WorldServiceLocator.WSSpells.On_CMSG_CAST_SPELL;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_CAST] = WorldServiceLocator.WSSpells.On_CMSG_CANCEL_CAST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_AURA] = WorldServiceLocator.WSSpells.On_CMSG_CANCEL_AURA;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_AUTO_REPEAT_SPELL] = WorldServiceLocator.WSSpells.On_CMSG_CANCEL_AUTO_REPEAT_SPELL;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_CHANNELLING] = WorldServiceLocator.WSSpells.On_CMSG_CANCEL_CHANNELLING;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TOGGLE_PVP] = WorldServiceLocator.WSHandlersMisc.On_CMSG_TOGGLE_PVP;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_BATTLEGROUND_PLAYER_POSITIONS] = WorldServiceLocator.WSHandlersBattleground.On_MSG_BATTLEGROUND_PLAYER_POSITIONS;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_STATUS_QUERY] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_STATUS_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_HELLO] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_HELLO;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_QUERY_QUEST] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_QUERY_QUEST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_ACCEPT_QUEST] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_ACCEPT_QUEST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_QUESTLOG_REMOVE_QUEST] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_QUESTLOG_REMOVE_QUEST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_QUEST_QUERY] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_QUEST_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_COMPLETE_QUEST] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_COMPLETE_QUEST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_REQUEST_REWARD] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_REQUEST_REWARD;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_QUESTGIVER_CHOOSE_REWARD] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_QUESTGIVER_CHOOSE_REWARD;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_QUEST_PUSH_RESULT] = WorldServiceLocator.WorldServer.ALLQUESTS.On_MSG_QUEST_PUSH_RESULT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PUSHQUESTTOPARTY] = WorldServiceLocator.WorldServer.ALLQUESTS.On_CMSG_PUSHQUESTTOPARTY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BINDER_ACTIVATE] = WorldServiceLocator.WSNPCs.On_CMSG_BINDER_ACTIVATE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BANKER_ACTIVATE] = WorldServiceLocator.WSNPCs.On_CMSG_BANKER_ACTIVATE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BUY_BANK_SLOT] = WorldServiceLocator.WSNPCs.On_CMSG_BUY_BANK_SLOT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUTOBANK_ITEM] = WorldServiceLocator.WSNPCs.On_CMSG_AUTOBANK_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUTOSTORE_BANK_ITEM] = WorldServiceLocator.WSNPCs.On_CMSG_AUTOSTORE_BANK_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_TALENT_WIPE_CONFIRM] = WorldServiceLocator.WSNPCs.On_MSG_TALENT_WIPE_CONFIRM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TRAINER_BUY_SPELL] = WorldServiceLocator.WSNPCs.On_CMSG_TRAINER_BUY_SPELL;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_TRAINER_LIST] = WorldServiceLocator.WSNPCs.On_CMSG_TRAINER_LIST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_AUCTION_HELLO] = WorldServiceLocator.WSAuction.On_MSG_AUCTION_HELLO;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_SELL_ITEM] = WorldServiceLocator.WSAuction.On_CMSG_AUCTION_SELL_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_REMOVE_ITEM] = WorldServiceLocator.WSAuction.On_CMSG_AUCTION_REMOVE_ITEM;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_LIST_ITEMS] = WorldServiceLocator.WSAuction.On_CMSG_AUCTION_LIST_ITEMS;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_LIST_OWNER_ITEMS] = WorldServiceLocator.WSAuction.On_CMSG_AUCTION_LIST_OWNER_ITEMS;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_PLACE_BID] = WorldServiceLocator.WSAuction.On_CMSG_AUCTION_PLACE_BID;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AUCTION_LIST_BIDDER_ITEMS] = WorldServiceLocator.WSAuction.On_CMSG_AUCTION_LIST_BIDDER_ITEMS;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_SHOWLIST] = WorldServiceLocator.WSGuilds.On_CMSG_PETITION_SHOWLIST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_BUY] = WorldServiceLocator.WSGuilds.On_CMSG_PETITION_BUY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_SHOW_SIGNATURES] = WorldServiceLocator.WSGuilds.On_CMSG_PETITION_SHOW_SIGNATURES;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_QUERY] = WorldServiceLocator.WSGuilds.On_CMSG_PETITION_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_OFFER_PETITION] = WorldServiceLocator.WSGuilds.On_CMSG_OFFER_PETITION;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PETITION_SIGN] = WorldServiceLocator.WSGuilds.On_CMSG_PETITION_SIGN;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_PETITION_RENAME] = WorldServiceLocator.WSGuilds.On_MSG_PETITION_RENAME;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_PETITION_DECLINE] = WorldServiceLocator.WSGuilds.On_MSG_PETITION_DECLINE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BATTLEMASTER_HELLO] = WorldServiceLocator.WSHandlersBattleground.On_CMSG_BATTLEMASTER_HELLO;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BATTLEFIELD_LIST] = WorldServiceLocator.WSHandlersBattleground.On_CMSG_BATTLEMASTER_HELLO;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_DUEL_CANCELLED] = WorldServiceLocator.WSSpells.On_CMSG_DUEL_CANCELLED;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_DUEL_ACCEPTED] = WorldServiceLocator.WSSpells.On_CMSG_DUEL_ACCEPTED;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_RESURRECT_RESPONSE] = WorldServiceLocator.WSSpells.On_CMSG_RESURRECT_RESPONSE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LEARN_TALENT] = WorldServiceLocator.WSSpells.On_CMSG_LEARN_TALENT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_WORLD_TELEPORT] = WorldServiceLocator.WSHandlersGamemaster.On_CMSG_WORLD_TELEPORT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_FACTION_ATWAR] = WorldServiceLocator.WSHandlersMisc.On_CMSG_SET_FACTION_ATWAR;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_FACTION_INACTIVE] = WorldServiceLocator.WSHandlersMisc.On_CMSG_SET_FACTION_INACTIVE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_WATCHED_FACTION] = WorldServiceLocator.WSHandlersMisc.On_CMSG_SET_WATCHED_FACTION;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PET_NAME_QUERY] = WorldServiceLocator.WSPets.On_CMSG_PET_NAME_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_REQUEST_PET_INFO] = WorldServiceLocator.WSPets.On_CMSG_REQUEST_PET_INFO;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PET_ACTION] = WorldServiceLocator.WSPets.On_CMSG_PET_ACTION;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PET_CANCEL_AURA] = WorldServiceLocator.WSPets.On_CMSG_PET_CANCEL_AURA;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PET_ABANDON] = WorldServiceLocator.WSPets.On_CMSG_PET_ABANDON;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PET_RENAME] = WorldServiceLocator.WSPets.On_CMSG_PET_RENAME;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PET_SET_ACTION] = WorldServiceLocator.WSPets.On_CMSG_PET_SET_ACTION;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PET_SPELL_AUTOCAST] = WorldServiceLocator.WSPets.On_CMSG_PET_SPELL_AUTOCAST;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PET_STOP_ATTACK] = WorldServiceLocator.WSPets.On_CMSG_PET_STOP_ATTACK;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PET_UNLEARN] = WorldServiceLocator.WSPets.On_CMSG_PET_UNLEARN;

        // Stable pet handlers
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_LIST_STABLED_PETS] = WorldServiceLocator.WSPets.On_MSG_LIST_STABLED_PETS;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_STABLE_PET] = WorldServiceLocator.WSPets.On_CMSG_STABLE_PET;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_UNSTABLE_PET] = WorldServiceLocator.WSPets.On_CMSG_UNSTABLE_PET;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BUY_STABLE_SLOT] = WorldServiceLocator.WSPets.On_CMSG_BUY_STABLE_SLOT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_STABLE_REVIVE_PET] = WorldServiceLocator.WSPets.On_CMSG_STABLE_REVIVE_PET;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_STABLE_SWAP_PET] = WorldServiceLocator.WSPets.On_CMSG_STABLE_SWAP_PET;

        // Battleground handlers
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BATTLEMASTER_JOIN] = WorldServiceLocator.WSHandlersBattleground.On_CMSG_BATTLEMASTER_JOIN;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_BATTLEFIELD_PORT] = WorldServiceLocator.WSHandlersBattleground.On_CMSG_BATTLEFIELD_PORT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LEAVE_BATTLEFIELD] = WorldServiceLocator.WSHandlersBattleground.On_CMSG_LEAVE_BATTLEFIELD;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AREA_SPIRIT_HEALER_QUERY] = WorldServiceLocator.WSHandlersBattleground.On_CMSG_AREA_SPIRIT_HEALER_QUERY;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_AREA_SPIRIT_HEALER_QUEUE] = WorldServiceLocator.WSHandlersBattleground.On_CMSG_AREA_SPIRIT_HEALER_QUEUE;

        // Misc handlers - cinematics, far sight, self res, skills
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_COMPLETE_CINEMATIC] = WorldServiceLocator.WSHandlersMisc.On_CMSG_COMPLETE_CINEMATIC;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_NEXT_CINEMATIC_CAMERA] = WorldServiceLocator.WSHandlersMisc.On_CMSG_NEXT_CINEMATIC_CAMERA;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_FAR_SIGHT] = WorldServiceLocator.WSHandlersMisc.On_CMSG_FAR_SIGHT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SELF_RES] = WorldServiceLocator.WSHandlersMisc.On_CMSG_SELF_RES;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_UNLEARN_SKILL] = WorldServiceLocator.WSHandlersMisc.On_CMSG_UNLEARN_SKILL;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_RANDOM_ROLL] = WorldServiceLocator.WSHandlersMisc.On_MSG_RANDOM_ROLL;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CANCEL_GROWTH_AURA] = WorldServiceLocator.WSHandlersMisc.On_CMSG_CANCEL_GROWTH_AURA;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_REQUEST_RAID_INFO] = WorldServiceLocator.WSHandlersMisc.On_CMSG_REQUEST_RAID_INFO;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_RESET_INSTANCES] = WorldServiceLocator.WSHandlersInstance.On_CMSG_RESET_INSTANCES;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_RAID_ICON_TARGET] = WorldServiceLocator.WSHandlersMisc.On_MSG_RAID_ICON_TARGET;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.MSG_RAID_READY_CHECK] = WorldServiceLocator.WSHandlersMisc.On_MSG_RAID_READY_CHECK;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_OPENING_CINEMATIC] = WorldServiceLocator.WSHandlersMisc.On_CMSG_OPENING_CINEMATIC;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_PLAYED_TIME] = WorldServiceLocator.WSHandlersMisc.On_CMSG_PLAYED_TIME;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_INSPECT] = WorldServiceLocator.WSHandlersMisc.On_CMSG_INSPECT;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SUMMON_RESPONSE] = WorldServiceLocator.WSHandlersMisc.On_CMSG_SUMMON_RESPONSE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_LOOT_MASTER_GIVE] = WorldServiceLocator.WSHandlersMisc.On_CMSG_LOOT_MASTER_GIVE;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_SET_EXPLORATION] = WorldServiceLocator.WSHandlersMisc.On_CMSG_SET_EXPLORATION;
        WorldServiceLocator.WorldServer.PacketHandlers[Opcodes.CMSG_CHAT_IGNORED] = WorldServiceLocator.WSHandlersMisc.On_CMSG_CHAT_IGNORED;

    }

    public void OnUnhandledPacket(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Unhandled Packet]", client.IP, client.Port, packet.OpCode);
    }

    public void OnWorldPacket(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Redirected Packet]", client.IP, client.Port, packet.OpCode);
        if (client.Character == null || !client.Character.FullyLoggedIn)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Unknown Opcode 0x{2:X} [{2}], DataLen={4}", client.IP, client.Port, packet.OpCode, Environment.NewLine, packet.Length);
            WorldServiceLocator.Packets.DumpPacket(packet.Data, client);
        }
    }
}
