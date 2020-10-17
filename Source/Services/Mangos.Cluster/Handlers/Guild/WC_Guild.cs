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

using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Common;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Guild;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;

namespace Mangos.Cluster.Handlers.Guild
{
    public partial class WC_Guild
    {
        private readonly ClusterServiceLocator clusterServiceLocator;

        public WC_Guild(ClusterServiceLocator clusterServiceLocator)
        {
            this.clusterServiceLocator = clusterServiceLocator;
        }

        public Dictionary<uint, Guild> GUILDs = new Dictionary<uint, Guild>();

        // Basic Guild Framework
        public void AddCharacterToGuild(WcHandlerCharacter.CharacterObject objCharacter, int guildId, int guildRank = 4)
        {
            clusterServiceLocator._WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = {2}, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", guildId, objCharacter.Guid, guildRank));
            if (GUILDs.ContainsKey((uint)guildId) == false)
            {
                var tmpGuild = new Guild((uint)guildId);
                GUILDs.Add((uint)guildId, tmpGuild);
            }

            objCharacter.Guild = GUILDs[(uint)guildId];
            objCharacter.Guild.Members.Add(objCharacter.Guid);
            objCharacter.GuildRank = (byte)guildRank;
            objCharacter.SendGuildUpdate();
        }

        public void AddCharacterToGuild(ulong guid, int guildId, int guildRank = 4)
        {
            clusterServiceLocator._WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = {2}, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", guildId, guid, guildRank));
        }

        public void RemoveCharacterFromGuild(WcHandlerCharacter.CharacterObject objCharacter)
        {
            clusterServiceLocator._WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = 0, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", 0, objCharacter.Guid));
            objCharacter.Guild.Members.Remove(objCharacter.Guid);
            objCharacter.Guild = null;
            objCharacter.GuildRank = 0;
            objCharacter.SendGuildUpdate();
        }

        public void RemoveCharacterFromGuild(ulong guid)
        {
            clusterServiceLocator._WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = 0, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", 0, guid));
        }

        public void BroadcastChatMessageGuild(WcHandlerCharacter.CharacterObject sender, string message, LANGUAGES language, int guildId)
        {
            // DONE: Check for guild member
            if (!sender.IsInGuild)
            {
                SendGuildResult(sender.Client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
                return;
            }

            // DONE: Check for rights to speak
            if (!sender.IsGuildRightSet(GuildRankRights.GR_RIGHT_GCHATSPEAK))
            {
                SendGuildResult(sender.Client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
                return;
            }

            // DONE: Build packet
            var packet = clusterServiceLocator._Functions.BuildChatMessage(sender.Guid, message, ChatMsg.CHAT_MSG_GUILD, language, (byte)sender.ChatFlag);

            // DONE: Send message to everyone
            var tmpArray = sender.Guild.Members.ToArray();
            foreach (ulong member in tmpArray)
            {
                if (clusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(member))
                {
                    if (clusterServiceLocator._WorldCluster.CHARACTERs[member].IsGuildRightSet(GuildRankRights.GR_RIGHT_GCHATLISTEN))
                    {
                        clusterServiceLocator._WorldCluster.CHARACTERs[member].Client.SendMultiplyPackets(packet);
                    }
                }
            }

            packet.Dispose();
        }

        public void BroadcastChatMessageOfficer(WcHandlerCharacter.CharacterObject sender, string message, LANGUAGES language, int guildId)
        {
            // DONE: Check for guild member
            if (!sender.IsInGuild)
            {
                SendGuildResult(sender.Client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
                return;
            }

            // DONE: Check for rights to speak
            if (!sender.IsGuildRightSet(GuildRankRights.GR_RIGHT_OFFCHATSPEAK))
            {
                SendGuildResult(sender.Client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
                return;
            }

            // DONE: Build packet
            var packet = clusterServiceLocator._Functions.BuildChatMessage(sender.Guid, message, ChatMsg.CHAT_MSG_OFFICER, language, (byte)sender.ChatFlag);

            // DONE: Send message to everyone
            var tmpArray = sender.Guild.Members.ToArray();
            foreach (ulong member in tmpArray)
            {
                if (clusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(member))
                {
                    if (clusterServiceLocator._WorldCluster.CHARACTERs[member].IsGuildRightSet(GuildRankRights.GR_RIGHT_OFFCHATLISTEN))
                    {
                        clusterServiceLocator._WorldCluster.CHARACTERs[member].Client.SendMultiplyPackets(packet);
                    }
                }
            }

            packet.Dispose();
        }

        public void SendGuildQuery(ClientClass client, uint guildId)
        {
            if (guildId == 0L)
                return;
            // WARNING: This opcode is used also in character enum, so there must not be used any references to CharacterObject, only ClientClass

            // DONE: Load the guild if it doesn't exist in the memory
            if (GUILDs.ContainsKey(guildId) == false)
            {
                var tmpGuild = new Guild(guildId);
                GUILDs.Add(guildId, tmpGuild);
            }

            var response = new Packets.PacketClass(Opcodes.SMSG_GUILD_QUERY_RESPONSE);
            response.AddUInt32(guildId);
            response.AddString(GUILDs[guildId].Name);
            for (int i = 0; i <= 9; i++)
                response.AddString(GUILDs[guildId].Ranks[i]);
            response.AddInt32(GUILDs[guildId].EmblemStyle);
            response.AddInt32(GUILDs[guildId].EmblemColor);
            response.AddInt32(GUILDs[guildId].BorderStyle);
            response.AddInt32(GUILDs[guildId].BorderColor);
            response.AddInt32(GUILDs[guildId].BackgroundColor);
            response.AddInt32(0);
            client.Send(response);
            response.Dispose();
        }

        public void SendGuildRoster(WcHandlerCharacter.CharacterObject objCharacter)
        {
            if (!objCharacter.IsInGuild)
                return;

            // DONE: Count the ranks
            byte guildRanksCount = 0;
            for (int i = 0; i <= 9; i++)
            {
                if (!string.IsNullOrEmpty(objCharacter.Guild.Ranks[i]))
                    guildRanksCount = (byte)(guildRanksCount + 1);
            }

            // DONE: Count the members
            var Members = new DataTable();
            clusterServiceLocator._WorldCluster.GetCharacterDatabase().Query("SELECT char_online, char_guid, char_name, char_class, char_level, char_zone_id, char_logouttime, char_guildRank, char_guildPNote, char_guildOffNote FROM characters WHERE char_guildId = " + objCharacter.Guild.ID + ";", ref Members);
            var response = new Packets.PacketClass(Opcodes.SMSG_GUILD_ROSTER);
            response.AddInt32(Members.Rows.Count);
            response.AddString(objCharacter.Guild.Motd);
            response.AddString(objCharacter.Guild.Info);
            response.AddInt32(guildRanksCount);
            for (int i = 0; i <= 9; i++)
            {
                if (!string.IsNullOrEmpty(objCharacter.Guild.Ranks[i]))
                {
                    response.AddUInt32(objCharacter.Guild.RankRights[i]);
                }
            }

            bool Officer = objCharacter.IsGuildRightSet(GuildRankRights.GR_RIGHT_VIEWOFFNOTE);
            for (int i = 0, loopTo = Members.Rows.Count - 1; i <= loopTo; i++)
            {
                if (Members.Rows[i].As<byte>("char_online") == 1)
                {
                    response.AddUInt64(Members.Rows[i].As<ulong>("char_guid"));
                    response.AddInt8(1);                         // OnlineFlag
                    response.AddString(Members.Rows[i].As<string>("char_name"));
                    response.AddInt32(Members.Rows[i].As<int>("char_guildRank"));
                    response.AddInt8(Members.Rows[i].As<byte>("char_level"));
                    response.AddInt8(Members.Rows[i].As<byte>("char_class"));
                    response.AddInt32(Members.Rows[i].As<int>("char_zone_id"));
                    response.AddString(Members.Rows[i].As<string>("char_guildPNote"));
                    if (Officer)
                    {
                        response.AddString(Members.Rows[i].As<string>("char_guildOffNote"));
                    }
                    else
                    {
                        response.AddInt8(0);
                    }
                }
                else
                {
                    response.AddUInt64(Members.Rows[i].As<ulong>("char_guid"));
                    response.AddInt8(0);                         // OfflineFlag
                    response.AddString(Members.Rows[i].As<string>("char_name"));
                    response.AddInt32(Members.Rows[i].As<int>("char_guildRank"));
                    response.AddInt8(Members.Rows[i].As<byte>("char_level"));
                    response.AddInt8(Members.Rows[i].As<byte>("char_class"));
                    response.AddInt32(Members.Rows[i].As<int>("char_zone_id"));
                    // 0 = < 1 hour / 0.1 = 2.4 hours / 1 = 24 hours (1 day)
                    // (Time logged out / 86400) = Days offline
                    float DaysOffline = (float)((clusterServiceLocator._Functions.GetTimestamp(DateAndTime.Now) - Members.Rows[i].As<uint>("char_logouttime")) / (double)DateInterval.Day);
                    response.AddSingle(DaysOffline); // Days offline
                    response.AddString(Members.Rows[i].As<string>("char_guildPNote"));
                    if (Officer)
                    {
                        response.AddString(Members.Rows[i].As<string>("char_guildOffNote"));
                    }
                    else
                    {
                        response.AddInt8(0);
                    }
                }
            }

            objCharacter.Client.Send(response);
            response.Dispose();
        }

        public void SendGuildResult(ClientClass client, GuildCommand command, GuildError result, string text = "")
        {
            var response = new Packets.PacketClass(Opcodes.SMSG_GUILD_COMMAND_RESULT);
            response.AddInt32((int)command);
            response.AddString(text);
            response.AddInt32((int)result);
            client.Send(response);
            response.Dispose();
        }

        public void NotifyGuildStatus(WcHandlerCharacter.CharacterObject objCharacter, GuildEvent status)
        {
            if (objCharacter.Guild is null)
                return;
            var statuspacket = new Packets.PacketClass(Opcodes.SMSG_GUILD_EVENT);
            statuspacket.AddInt8((byte)status);
            statuspacket.AddInt8(1);
            statuspacket.AddString(objCharacter.Name);
            statuspacket.AddInt8(0);
            statuspacket.AddInt8(0);
            statuspacket.AddInt8(0);
            BroadcastToGuild(statuspacket, objCharacter.Guild, objCharacter.Guid);
            statuspacket.Dispose();
        }

        public void BroadcastToGuild(Packets.PacketClass packet, Guild guild, [Optional, DefaultParameterValue(0UL)] ulong notTo)
        {
            var tmpArray = guild.Members.ToArray();
            foreach (ulong member in tmpArray)
            {
                if (member == notTo)
                    continue;
                if (clusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(member))
                {
                    clusterServiceLocator._WorldCluster.CHARACTERs[member].Client.SendMultiplyPackets(packet);
                }
            }
        }

        // Members Options
        public void SendGuildMOTD(WcHandlerCharacter.CharacterObject objCharacter)
        {
            if (objCharacter.IsInGuild)
            {
                if (!string.IsNullOrEmpty(objCharacter.Guild.Motd))
                {
                    var response = new Packets.PacketClass(Opcodes.SMSG_GUILD_EVENT);
                    response.AddInt8((byte)GuildEvent.MOTD);
                    response.AddInt8(1);
                    response.AddString(objCharacter.Guild.Motd);
                    objCharacter.Client.Send(response);
                    response.Dispose();
                }
            }
        }
    }
}