//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
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

using Mangos.Cluster.Globals;
using Mangos.Cluster.Server;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;

namespace Mangos.Cluster.Handlers
{
    public class WC_Handlers
    {
        public void IntializePacketHandlers()
        {
            // NOTE: These opcodes are not used in any way
            // _WorldCluster.PacketHandlers[OPCODES.CMSG_MOVE_TIME_SKIPPED] = AddressOf On_CMSG_MOVE_TIME_SKIPPED
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_NEXT_CINEMATIC_CAMERA] = ClusterServiceLocator._WC_Handlers_Misc.On_CMSG_NEXT_CINEMATIC_CAMERA;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_COMPLETE_CINEMATIC] = ClusterServiceLocator._WC_Handlers_Misc.On_CMSG_COMPLETE_CINEMATIC;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_UPDATE_ACCOUNT_DATA] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_UPDATE_ACCOUNT_DATA;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_REQUEST_ACCOUNT_DATA] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_REQUEST_ACCOUNT_DATA;

            // NOTE: These opcodes are only partialy handled by Cluster and must be handled by WorldServer
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_HEARTBEAT] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_HEARTBEAT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_BACKWARD] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_START_BACKWARD;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_FORWARD] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_FORWARD;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_PITCH_DOWN] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_PITCH_DOWN;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_PITCH_UP] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_PITCH_UP;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_STRAFE_LEFT] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STRAFE_LEFT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_STRAFE_RIGHT] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STRAFE_LEFT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_SWIM] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_SWIM;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_TURN_LEFT] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_TURN_LEFT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_TURN_RIGHT] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_TURN_RIGHT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_PITCH] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP_PITCH;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_STRAFE] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP_STRAFE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_SWIM] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP_SWIM;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_TURN] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP_TURN;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_SET_FACING] = ClusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_SET_FACING;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CANCEL_TRADE] = ClusterServiceLocator._WC_Handlers_Misc.On_CMSG_CANCEL_TRADE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LOGOUT_CANCEL] = ClusterServiceLocator._WC_Handlers_Misc.On_CMSG_LOGOUT_CANCEL;

            // NOTE: These opcodes below must be exluded form WorldServer
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PING] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_PING;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_AUTH_SESSION] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_AUTH_SESSION;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_ENUM] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_CHAR_ENUM;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_CREATE] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_CHAR_CREATE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_DELETE] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_CHAR_DELETE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_RENAME] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_CHAR_RENAME;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PLAYER_LOGIN] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_PLAYER_LOGIN;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PLAYER_LOGOUT] = ClusterServiceLocator._WC_Handlers_Auth.On_CMSG_PLAYER_LOGOUT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_WORLDPORT_ACK] = ClusterServiceLocator._WC_Handlers_Auth.On_MSG_MOVE_WORLDPORT_ACK;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_QUERY_TIME] = ClusterServiceLocator._WC_Handlers_Misc.On_CMSG_QUERY_TIME;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_INSPECT] = ClusterServiceLocator._WC_Handlers_Misc.On_CMSG_INSPECT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_WHO] = ClusterServiceLocator._WC_Handlers_Social.On_CMSG_WHO;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_WHOIS] = ClusterServiceLocator._WC_Handlers_Tickets.On_CMSG_WHOIS;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PLAYED_TIME] = ClusterServiceLocator._WC_Handlers_Misc.On_CMSG_PLAYED_TIME;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_NAME_QUERY] = ClusterServiceLocator._WC_Handlers_Misc.On_CMSG_NAME_QUERY;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_BUG] = ClusterServiceLocator._WC_Handlers_Tickets.On_CMSG_BUG;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_GETTICKET] = ClusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_GETTICKET;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_CREATE] = ClusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_CREATE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_SYSTEMSTATUS] = ClusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_SYSTEMSTATUS;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_DELETETICKET] = ClusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_DELETETICKET;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_UPDATETEXT] = ClusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_UPDATETEXT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_BATTLEMASTER_JOIN] = ClusterServiceLocator._WC_Handlers_Battleground.On_CMSG_BATTLEMASTER_JOIN;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_BATTLEFIELD_PORT] = ClusterServiceLocator._WC_Handlers_Battleground.On_CMSG_BATTLEFIELD_PORT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LEAVE_BATTLEFIELD] = ClusterServiceLocator._WC_Handlers_Battleground.On_CMSG_LEAVE_BATTLEFIELD;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_BATTLEGROUND_PLAYER_POSITIONS] = ClusterServiceLocator._WC_Handlers_Battleground.On_MSG_BATTLEGROUND_PLAYER_POSITIONS;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_FRIEND_LIST] = ClusterServiceLocator._WC_Handlers_Social.On_CMSG_FRIEND_LIST;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_ADD_FRIEND] = ClusterServiceLocator._WC_Handlers_Social.On_CMSG_ADD_FRIEND;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_ADD_IGNORE] = ClusterServiceLocator._WC_Handlers_Social.On_CMSG_ADD_IGNORE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_DEL_FRIEND] = ClusterServiceLocator._WC_Handlers_Social.On_CMSG_DEL_FRIEND;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_DEL_IGNORE] = ClusterServiceLocator._WC_Handlers_Social.On_CMSG_DEL_IGNORE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_REQUEST_RAID_INFO] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_REQUEST_RAID_INFO;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_INVITE] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_INVITE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_CANCEL] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_CANCEL;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_ACCEPT] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_ACCEPT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_DECLINE] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_DECLINE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_UNINVITE] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_UNINVITE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_UNINVITE_GUID] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_UNINVITE_GUID;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_DISBAND] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_DISBAND;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_RAID_CONVERT] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_RAID_CONVERT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_SET_LEADER] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_SET_LEADER;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_CHANGE_SUB_GROUP] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_CHANGE_SUB_GROUP;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_SWAP_SUB_GROUP] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_SWAP_SUB_GROUP;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LOOT_METHOD] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_LOOT_METHOD;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MINIMAP_PING] = ClusterServiceLocator._WC_Handlers_Group.On_MSG_MINIMAP_PING;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_RANDOM_ROLL] = ClusterServiceLocator._WC_Handlers_Group.On_MSG_RANDOM_ROLL;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_RAID_READY_CHECK] = ClusterServiceLocator._WC_Handlers_Group.On_MSG_RAID_READY_CHECK;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_RAID_ICON_TARGET] = ClusterServiceLocator._WC_Handlers_Group.On_MSG_RAID_ICON_TARGET;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_REQUEST_PARTY_MEMBER_STATS] = ClusterServiceLocator._WC_Handlers_Group.On_CMSG_REQUEST_PARTY_MEMBER_STATS;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_TURN_IN_PETITION] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_TURN_IN_PETITION;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_QUERY] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_QUERY;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_CREATE] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_CREATE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DISBAND] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_DISBAND;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_ROSTER] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_ROSTER;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_INFO] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_INFO;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_RANK] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_RANK;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_ADD_RANK] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_ADD_RANK;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DEL_RANK] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_DEL_RANK;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_PROMOTE] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_PROMOTE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DEMOTE] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_DEMOTE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_LEADER] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_LEADER;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_SAVE_GUILD_EMBLEM] = ClusterServiceLocator._WC_Handlers_Guild.On_MSG_SAVE_GUILD_EMBLEM;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_SET_OFFICER_NOTE] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_SET_OFFICER_NOTE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_SET_PUBLIC_NOTE] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_SET_PUBLIC_NOTE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_MOTD] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_MOTD;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_INVITE] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_INVITE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_ACCEPT] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_ACCEPT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DECLINE] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_DECLINE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_REMOVE] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_REMOVE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_LEAVE] = ClusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_LEAVE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAT_IGNORED] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHAT_IGNORED;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_MESSAGECHAT] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_MESSAGECHAT;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_JOIN_CHANNEL] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_JOIN_CHANNEL;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LEAVE_CHANNEL] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_LEAVE_CHANNEL;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_LIST] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_LIST;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_PASSWORD] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_PASSWORD;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_SET_OWNER] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_SET_OWNER;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_OWNER] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_OWNER;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_MODERATOR] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_MODERATOR;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_UNMODERATOR] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_UNMODERATOR;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_MUTE] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_MUTE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_UNMUTE] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_UNMUTE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_KICK] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_KICK;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_INVITE] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_INVITE;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_BAN] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_BAN;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_UNBAN] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_UNBAN;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_ANNOUNCEMENTS] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_ANNOUNCEMENTS;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_MODERATE] = ClusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_MODERATE;

            // Opcodes redirected from the WorldServer
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CREATURE_QUERY] = OnClusterPacket;
            ClusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GAMEOBJECT_QUERY] = OnClusterPacket;

            // NOTE: TODO Opcodes
            // none

        }

        public void OnUnhandledPacket(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Unhandled Packet]", client.IP, client.Port, packet.OpCode);
        }

        public void OnClusterPacket(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Redirected Packet]", client.IP, client.Port, packet.OpCode);
            if (client.Character is null || client.Character.IsInWorld == false)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Unknown Opcode 0x{2:X} [{2}], DataLen={4}", client.IP, client.Port, packet.OpCode, Constants.vbCrLf, packet.Length);
                ClusterServiceLocator._Packets.DumpPacket(packet.Data, client);
            }
            else
            {
                client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            }
        }
    }
}