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

using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;

namespace Mangos.Cluster.Handlers;

public class WcHandlers
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlers(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void IntializePacketHandlers()
    {
        // NOTE: These opcodes are not used in any way
        // _WorldCluster.PacketHandlers[OPCODES.CMSG_MOVE_TIME_SKIPPED] = AddressOf On_CMSG_MOVE_TIME_SKIPPED
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_NEXT_CINEMATIC_CAMERA] = _clusterServiceLocator.WcHandlersMisc.On_CMSG_NEXT_CINEMATIC_CAMERA;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_COMPLETE_CINEMATIC] = _clusterServiceLocator.WcHandlersMisc.On_CMSG_COMPLETE_CINEMATIC;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_UPDATE_ACCOUNT_DATA] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_UPDATE_ACCOUNT_DATA;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_REQUEST_ACCOUNT_DATA] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_REQUEST_ACCOUNT_DATA;

        // NOTE: These opcodes are only partialy handled by Cluster and must be handled by WorldServer
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_HEARTBEAT] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_HEARTBEAT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_BACKWARD] = _clusterServiceLocator.WcHandlersMovement.On_MSG_START_BACKWARD;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_FORWARD] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_START_FORWARD;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_PITCH_DOWN] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_START_PITCH_DOWN;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_PITCH_UP] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_START_PITCH_UP;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_STRAFE_LEFT] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_STRAFE_LEFT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_STRAFE_RIGHT] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_STRAFE_LEFT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_SWIM] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_START_SWIM;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_TURN_LEFT] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_START_TURN_LEFT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_TURN_RIGHT] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_START_TURN_RIGHT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_STOP;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_PITCH] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_STOP_PITCH;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_STRAFE] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_STOP_STRAFE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_SWIM] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_STOP_SWIM;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_TURN] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_STOP_TURN;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_SET_FACING] = _clusterServiceLocator.WcHandlersMovement.On_MSG_MOVE_SET_FACING;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CANCEL_TRADE] = _clusterServiceLocator.WcHandlersMisc.On_CMSG_CANCEL_TRADE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LOGOUT_CANCEL] = _clusterServiceLocator.WcHandlersMisc.On_CMSG_LOGOUT_CANCEL;

        // NOTE: These opcodes below must be exluded form WorldServer
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PING] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_PING;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_AUTH_SESSION] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_AUTH_SESSION;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_ENUM] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_CHAR_ENUM;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_CREATE] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_CHAR_CREATE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_DELETE] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_CHAR_DELETE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_RENAME] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_CHAR_RENAME;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PLAYER_LOGIN] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_PLAYER_LOGIN;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PLAYER_LOGOUT] = _clusterServiceLocator.WcHandlersAuth.On_CMSG_PLAYER_LOGOUT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_WORLDPORT_ACK] = _clusterServiceLocator.WcHandlersAuth.On_MSG_MOVE_WORLDPORT_ACK;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_QUERY_TIME] = _clusterServiceLocator.WcHandlersMisc.On_CMSG_QUERY_TIME;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_INSPECT] = _clusterServiceLocator.WcHandlersMisc.On_CMSG_INSPECT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_WHO] = _clusterServiceLocator.WcHandlersSocial.On_CMSG_WHO;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_WHOIS] = _clusterServiceLocator.WcHandlersTickets.On_CMSG_WHOIS;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PLAYED_TIME] = _clusterServiceLocator.WcHandlersMisc.On_CMSG_PLAYED_TIME;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_NAME_QUERY] = _clusterServiceLocator.WcHandlersMisc.On_CMSG_NAME_QUERY;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_BUG] = _clusterServiceLocator.WcHandlersTickets.On_CMSG_BUG;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_GETTICKET] = _clusterServiceLocator.WcHandlersTickets.On_CMSG_GMTICKET_GETTICKET;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_CREATE] = _clusterServiceLocator.WcHandlersTickets.On_CMSG_GMTICKET_CREATE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_SYSTEMSTATUS] = _clusterServiceLocator.WcHandlersTickets.On_CMSG_GMTICKET_SYSTEMSTATUS;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_DELETETICKET] = _clusterServiceLocator.WcHandlersTickets.On_CMSG_GMTICKET_DELETETICKET;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_UPDATETEXT] = _clusterServiceLocator.WcHandlersTickets.On_CMSG_GMTICKET_UPDATETEXT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_BATTLEMASTER_JOIN] = _clusterServiceLocator.WcHandlersBattleground.On_CMSG_BATTLEMASTER_JOIN;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_BATTLEFIELD_PORT] = _clusterServiceLocator.WcHandlersBattleground.On_CMSG_BATTLEFIELD_PORT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LEAVE_BATTLEFIELD] = _clusterServiceLocator.WcHandlersBattleground.On_CMSG_LEAVE_BATTLEFIELD;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_BATTLEGROUND_PLAYER_POSITIONS] = _clusterServiceLocator.WcHandlersBattleground.On_MSG_BATTLEGROUND_PLAYER_POSITIONS;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_FRIEND_LIST] = _clusterServiceLocator.WcHandlersSocial.On_CMSG_FRIEND_LIST;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_ADD_FRIEND] = _clusterServiceLocator.WcHandlersSocial.On_CMSG_ADD_FRIEND;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_ADD_IGNORE] = _clusterServiceLocator.WcHandlersSocial.On_CMSG_ADD_IGNORE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_DEL_FRIEND] = _clusterServiceLocator.WcHandlersSocial.On_CMSG_DEL_FRIEND;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_DEL_IGNORE] = _clusterServiceLocator.WcHandlersSocial.On_CMSG_DEL_IGNORE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_REQUEST_RAID_INFO] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_REQUEST_RAID_INFO;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_INVITE] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_INVITE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_CANCEL] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_CANCEL;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_ACCEPT] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_ACCEPT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_DECLINE] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_DECLINE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_UNINVITE] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_UNINVITE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_UNINVITE_GUID] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_UNINVITE_GUID;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_DISBAND] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_DISBAND;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_RAID_CONVERT] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_RAID_CONVERT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_SET_LEADER] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_SET_LEADER;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_CHANGE_SUB_GROUP] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_CHANGE_SUB_GROUP;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_SWAP_SUB_GROUP] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_GROUP_SWAP_SUB_GROUP;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LOOT_METHOD] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_LOOT_METHOD;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_MINIMAP_PING] = _clusterServiceLocator.WcHandlersGroup.On_MSG_MINIMAP_PING;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_RANDOM_ROLL] = _clusterServiceLocator.WcHandlersGroup.On_MSG_RANDOM_ROLL;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_RAID_READY_CHECK] = _clusterServiceLocator.WcHandlersGroup.On_MSG_RAID_READY_CHECK;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_RAID_ICON_TARGET] = _clusterServiceLocator.WcHandlersGroup.On_MSG_RAID_ICON_TARGET;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_REQUEST_PARTY_MEMBER_STATS] = _clusterServiceLocator.WcHandlersGroup.On_CMSG_REQUEST_PARTY_MEMBER_STATS;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_TURN_IN_PETITION] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_TURN_IN_PETITION;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_QUERY] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_QUERY;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_CREATE] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_CREATE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DISBAND] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_DISBAND;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_ROSTER] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_ROSTER;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_INFO] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_INFO;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_RANK] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_RANK;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_ADD_RANK] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_ADD_RANK;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DEL_RANK] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_DEL_RANK;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_PROMOTE] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_PROMOTE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DEMOTE] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_DEMOTE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_LEADER] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_LEADER;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.MSG_SAVE_GUILD_EMBLEM] = _clusterServiceLocator.WcHandlersGuild.On_MSG_SAVE_GUILD_EMBLEM;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_SET_OFFICER_NOTE] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_SET_OFFICER_NOTE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_SET_PUBLIC_NOTE] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_SET_PUBLIC_NOTE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_MOTD] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_MOTD;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_INVITE] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_INVITE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_ACCEPT] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_ACCEPT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DECLINE] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_DECLINE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_REMOVE] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_REMOVE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_LEAVE] = _clusterServiceLocator.WcHandlersGuild.On_CMSG_GUILD_LEAVE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAT_IGNORED] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHAT_IGNORED;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_MESSAGECHAT] = _clusterServiceLocator.WcHandlersChat.On_CMSG_MESSAGECHAT;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_JOIN_CHANNEL] = _clusterServiceLocator.WcHandlersChat.On_CMSG_JOIN_CHANNEL;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LEAVE_CHANNEL] = _clusterServiceLocator.WcHandlersChat.On_CMSG_LEAVE_CHANNEL;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_LIST] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_LIST;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_PASSWORD] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_PASSWORD;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_SET_OWNER] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_SET_OWNER;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_OWNER] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_OWNER;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_MODERATOR] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_MODERATOR;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_UNMODERATOR] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_UNMODERATOR;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_MUTE] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_MUTE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_UNMUTE] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_UNMUTE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_KICK] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_KICK;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_INVITE] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_INVITE;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_BAN] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_BAN;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_UNBAN] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_UNBAN;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_ANNOUNCEMENTS] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_ANNOUNCEMENTS;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_MODERATE] = _clusterServiceLocator.WcHandlersChat.On_CMSG_CHANNEL_MODERATE;

        // Opcodes redirected from the WorldServer
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CREATURE_QUERY] = OnClusterPacket;
        _clusterServiceLocator.WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GAMEOBJECT_QUERY] = OnClusterPacket;

        // NOTE: TODO Opcodes
        // none
    }

    public void OnUnhandledPacket(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Unhandled Packet]", client.IP, client.Port, packet.OpCode);
    }

    public void OnClusterPacket(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Redirected Packet]", client.IP, client.Port, packet.OpCode);
        if (client.Character is null || client.Character.IsInWorld == false)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Unknown Opcode 0x{2:X} [{2}], DataLen={4}", client.IP, client.Port, packet.OpCode, Constants.vbCrLf, packet.Length);
            _clusterServiceLocator.Packets.DumpPacket(packet.Data, client);
        }
        else
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
        }
    }
}
