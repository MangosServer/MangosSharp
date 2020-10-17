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
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;

namespace Mangos.Cluster.Handlers
{
    public class WC_Handlers
    {
        private readonly ClusterServiceLocator clusterServiceLocator;

        public WC_Handlers(ClusterServiceLocator clusterServiceLocator)
        {
            this.clusterServiceLocator = clusterServiceLocator;
        }

        public void IntializePacketHandlers()
        {
            // NOTE: These opcodes are not used in any way
            // _WorldCluster.PacketHandlers[OPCODES.CMSG_MOVE_TIME_SKIPPED] = AddressOf On_CMSG_MOVE_TIME_SKIPPED
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_NEXT_CINEMATIC_CAMERA] = clusterServiceLocator._WC_Handlers_Misc.On_CMSG_NEXT_CINEMATIC_CAMERA;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_COMPLETE_CINEMATIC] = clusterServiceLocator._WC_Handlers_Misc.On_CMSG_COMPLETE_CINEMATIC;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_UPDATE_ACCOUNT_DATA] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_UPDATE_ACCOUNT_DATA;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_REQUEST_ACCOUNT_DATA] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_REQUEST_ACCOUNT_DATA;

            // NOTE: These opcodes are only partialy handled by Cluster and must be handled by WorldServer
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_HEARTBEAT] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_HEARTBEAT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_BACKWARD] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_START_BACKWARD;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_FORWARD] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_FORWARD;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_PITCH_DOWN] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_PITCH_DOWN;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_PITCH_UP] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_PITCH_UP;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_STRAFE_LEFT] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STRAFE_LEFT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_STRAFE_RIGHT] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STRAFE_LEFT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_SWIM] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_SWIM;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_TURN_LEFT] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_TURN_LEFT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_START_TURN_RIGHT] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_START_TURN_RIGHT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_PITCH] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP_PITCH;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_STRAFE] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP_STRAFE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_SWIM] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP_SWIM;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_STOP_TURN] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_STOP_TURN;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_SET_FACING] = clusterServiceLocator._WC_Handlers_Movement.On_MSG_MOVE_SET_FACING;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CANCEL_TRADE] = clusterServiceLocator._WC_Handlers_Misc.On_CMSG_CANCEL_TRADE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LOGOUT_CANCEL] = clusterServiceLocator._WC_Handlers_Misc.On_CMSG_LOGOUT_CANCEL;

            // NOTE: These opcodes below must be exluded form WorldServer
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PING] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_PING;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_AUTH_SESSION] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_AUTH_SESSION;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_ENUM] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_CHAR_ENUM;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_CREATE] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_CHAR_CREATE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_DELETE] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_CHAR_DELETE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAR_RENAME] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_CHAR_RENAME;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PLAYER_LOGIN] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_PLAYER_LOGIN;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PLAYER_LOGOUT] = clusterServiceLocator._WC_Handlers_Auth.On_CMSG_PLAYER_LOGOUT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MOVE_WORLDPORT_ACK] = clusterServiceLocator._WC_Handlers_Auth.On_MSG_MOVE_WORLDPORT_ACK;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_QUERY_TIME] = clusterServiceLocator._WC_Handlers_Misc.On_CMSG_QUERY_TIME;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_INSPECT] = clusterServiceLocator._WC_Handlers_Misc.On_CMSG_INSPECT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_WHO] = clusterServiceLocator._WC_Handlers_Social.On_CMSG_WHO;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_WHOIS] = clusterServiceLocator._WC_Handlers_Tickets.On_CMSG_WHOIS;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_PLAYED_TIME] = clusterServiceLocator._WC_Handlers_Misc.On_CMSG_PLAYED_TIME;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_NAME_QUERY] = clusterServiceLocator._WC_Handlers_Misc.On_CMSG_NAME_QUERY;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_BUG] = clusterServiceLocator._WC_Handlers_Tickets.On_CMSG_BUG;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_GETTICKET] = clusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_GETTICKET;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_CREATE] = clusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_CREATE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_SYSTEMSTATUS] = clusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_SYSTEMSTATUS;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_DELETETICKET] = clusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_DELETETICKET;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GMTICKET_UPDATETEXT] = clusterServiceLocator._WC_Handlers_Tickets.On_CMSG_GMTICKET_UPDATETEXT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_BATTLEMASTER_JOIN] = clusterServiceLocator._WC_Handlers_Battleground.On_CMSG_BATTLEMASTER_JOIN;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_BATTLEFIELD_PORT] = clusterServiceLocator._WC_Handlers_Battleground.On_CMSG_BATTLEFIELD_PORT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LEAVE_BATTLEFIELD] = clusterServiceLocator._WC_Handlers_Battleground.On_CMSG_LEAVE_BATTLEFIELD;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_BATTLEGROUND_PLAYER_POSITIONS] = clusterServiceLocator._WC_Handlers_Battleground.On_MSG_BATTLEGROUND_PLAYER_POSITIONS;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_FRIEND_LIST] = clusterServiceLocator._WC_Handlers_Social.On_CMSG_FRIEND_LIST;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_ADD_FRIEND] = clusterServiceLocator._WC_Handlers_Social.On_CMSG_ADD_FRIEND;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_ADD_IGNORE] = clusterServiceLocator._WC_Handlers_Social.On_CMSG_ADD_IGNORE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_DEL_FRIEND] = clusterServiceLocator._WC_Handlers_Social.On_CMSG_DEL_FRIEND;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_DEL_IGNORE] = clusterServiceLocator._WC_Handlers_Social.On_CMSG_DEL_IGNORE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_REQUEST_RAID_INFO] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_REQUEST_RAID_INFO;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_INVITE] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_INVITE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_CANCEL] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_CANCEL;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_ACCEPT] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_ACCEPT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_DECLINE] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_DECLINE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_UNINVITE] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_UNINVITE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_UNINVITE_GUID] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_UNINVITE_GUID;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_DISBAND] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_DISBAND;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_RAID_CONVERT] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_RAID_CONVERT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_SET_LEADER] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_SET_LEADER;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_CHANGE_SUB_GROUP] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_CHANGE_SUB_GROUP;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GROUP_SWAP_SUB_GROUP] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_GROUP_SWAP_SUB_GROUP;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LOOT_METHOD] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_LOOT_METHOD;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_MINIMAP_PING] = clusterServiceLocator._WC_Handlers_Group.On_MSG_MINIMAP_PING;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_RANDOM_ROLL] = clusterServiceLocator._WC_Handlers_Group.On_MSG_RANDOM_ROLL;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_RAID_READY_CHECK] = clusterServiceLocator._WC_Handlers_Group.On_MSG_RAID_READY_CHECK;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_RAID_ICON_TARGET] = clusterServiceLocator._WC_Handlers_Group.On_MSG_RAID_ICON_TARGET;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_REQUEST_PARTY_MEMBER_STATS] = clusterServiceLocator._WC_Handlers_Group.On_CMSG_REQUEST_PARTY_MEMBER_STATS;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_TURN_IN_PETITION] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_TURN_IN_PETITION;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_QUERY] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_QUERY;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_CREATE] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_CREATE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DISBAND] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_DISBAND;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_ROSTER] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_ROSTER;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_INFO] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_INFO;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_RANK] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_RANK;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_ADD_RANK] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_ADD_RANK;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DEL_RANK] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_DEL_RANK;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_PROMOTE] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_PROMOTE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DEMOTE] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_DEMOTE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_LEADER] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_LEADER;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.MSG_SAVE_GUILD_EMBLEM] = clusterServiceLocator._WC_Handlers_Guild.On_MSG_SAVE_GUILD_EMBLEM;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_SET_OFFICER_NOTE] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_SET_OFFICER_NOTE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_SET_PUBLIC_NOTE] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_SET_PUBLIC_NOTE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_MOTD] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_MOTD;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_INVITE] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_INVITE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_ACCEPT] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_ACCEPT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_DECLINE] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_DECLINE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_REMOVE] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_REMOVE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GUILD_LEAVE] = clusterServiceLocator._WC_Handlers_Guild.On_CMSG_GUILD_LEAVE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHAT_IGNORED] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHAT_IGNORED;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_MESSAGECHAT] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_MESSAGECHAT;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_JOIN_CHANNEL] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_JOIN_CHANNEL;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_LEAVE_CHANNEL] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_LEAVE_CHANNEL;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_LIST] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_LIST;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_PASSWORD] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_PASSWORD;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_SET_OWNER] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_SET_OWNER;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_OWNER] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_OWNER;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_MODERATOR] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_MODERATOR;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_UNMODERATOR] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_UNMODERATOR;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_MUTE] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_MUTE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_UNMUTE] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_UNMUTE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_KICK] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_KICK;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_INVITE] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_INVITE;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_BAN] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_BAN;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_UNBAN] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_UNBAN;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_ANNOUNCEMENTS] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_ANNOUNCEMENTS;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CHANNEL_MODERATE] = clusterServiceLocator._WC_Handlers_Chat.On_CMSG_CHANNEL_MODERATE;

            // Opcodes redirected from the WorldServer
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_CREATURE_QUERY] = OnClusterPacket;
            clusterServiceLocator._WorldCluster.GetPacketHandlers()[Opcodes.CMSG_GAMEOBJECT_QUERY] = OnClusterPacket;

            // NOTE: TODO Opcodes
            // none

        }

        public void OnUnhandledPacket(Packets.PacketClass packet, ClientClass client)
        {
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Unhandled Packet]", client.IP, client.Port, packet.OpCode);
        }

        public void OnClusterPacket(Packets.PacketClass packet, ClientClass client)
        {
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] {2} [Redirected Packet]", client.IP, client.Port, packet.OpCode);
            if (client.Character is null || client.Character.IsInWorld == false)
            {
                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Unknown Opcode 0x{2:X} [{2}], DataLen={4}", client.IP, client.Port, packet.OpCode, Constants.vbCrLf, packet.Length);
                clusterServiceLocator._Packets.DumpPacket(packet.Data, client);
            }
            else
            {
                client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            }
        }
    }
}