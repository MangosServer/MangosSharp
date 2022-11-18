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
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Guild;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;

namespace Mangos.Cluster.Handlers.Guild;

public partial class WcGuild
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcGuild(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public Dictionary<uint, Guild> GuilDs = new();

    // Basic Guild Framework
    public void AddCharacterToGuild(WcHandlerCharacter.CharacterObject objCharacter, int guildId, int guildRank = 4)
    {
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = {2}, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", guildId, objCharacter.Guid, guildRank));
        if (GuilDs.ContainsKey((uint)guildId) == false)
        {
            Guild tmpGuild = new((uint)guildId);
            GuilDs.Add((uint)guildId, tmpGuild);
        }

        objCharacter.Guild = GuilDs[(uint)guildId];
        objCharacter.Guild.Members.Add(objCharacter.Guid);
        objCharacter.GuildRank = (byte)guildRank;
        objCharacter.SendGuildUpdate();
    }

    public void AddCharacterToGuild(ulong guid, int guildId, int guildRank = 4)
    {
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = {2}, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", guildId, guid, guildRank));
    }

    public void RemoveCharacterFromGuild(WcHandlerCharacter.CharacterObject objCharacter)
    {
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = 0, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", 0, objCharacter.Guid));
        objCharacter.Guild.Members.Remove(objCharacter.Guid);
        objCharacter.Guild = null;
        objCharacter.GuildRank = 0;
        objCharacter.SendGuildUpdate();
    }

    public void RemoveCharacterFromGuild(ulong guid)
    {
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = 0, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", 0, guid));
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
        var packet = _clusterServiceLocator.Functions.BuildChatMessage(sender.Guid, message, ChatMsg.CHAT_MSG_GUILD, language, (byte)sender.ChatFlag);

        // DONE: Send message to everyone
        var tmpArray = sender.Guild.Members.ToArray();
        foreach (var member in tmpArray)
        {
            if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(member))
            {
                if (_clusterServiceLocator.WorldCluster.CharacteRs[member].IsGuildRightSet(GuildRankRights.GR_RIGHT_GCHATLISTEN))
                {
                    _clusterServiceLocator.WorldCluster.CharacteRs[member].Client.SendMultiplyPackets(packet);
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
        var packet = _clusterServiceLocator.Functions.BuildChatMessage(sender.Guid, message, ChatMsg.CHAT_MSG_OFFICER, language, (byte)sender.ChatFlag);

        // DONE: Send message to everyone
        var tmpArray = sender.Guild.Members.ToArray();
        foreach (var member in tmpArray)
        {
            if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(member))
            {
                if (_clusterServiceLocator.WorldCluster.CharacteRs[member].IsGuildRightSet(GuildRankRights.GR_RIGHT_OFFCHATLISTEN))
                {
                    _clusterServiceLocator.WorldCluster.CharacteRs[member].Client.SendMultiplyPackets(packet);
                }
            }
        }

        packet.Dispose();
    }

    public void SendGuildQuery(ClientClass client, uint guildId)
    {
        if (guildId == 0L)
        {
            return;
        }
        // WARNING: This opcode is used also in character enum, so there must not be used any references to CharacterObject, only ClientClass

        // DONE: Load the guild if it doesn't exist in the memory
        if (GuilDs.ContainsKey(guildId) == false)
        {
            Guild tmpGuild = new(guildId);
            GuilDs.Add(guildId, tmpGuild);
        }

        PacketClass response = new(Opcodes.SMSG_GUILD_QUERY_RESPONSE);
        response.AddUInt32(guildId);
        response.AddString(GuilDs[guildId].Name);
        for (var i = 0; i <= 9; i++)
        {
            response.AddString(GuilDs[guildId].Ranks[i]);
        }

        response.AddInt32(GuilDs[guildId].EmblemStyle);
        response.AddInt32(GuilDs[guildId].EmblemColor);
        response.AddInt32(GuilDs[guildId].BorderStyle);
        response.AddInt32(GuilDs[guildId].BorderColor);
        response.AddInt32(GuilDs[guildId].BackgroundColor);
        response.AddInt32(0);
        client.Send(response);
        response.Dispose();
    }

    public void SendGuildRoster(WcHandlerCharacter.CharacterObject objCharacter)
    {
        if (!objCharacter.IsInGuild)
        {
            return;
        }

        // DONE: Count the ranks
        byte guildRanksCount = 0;
        for (var i = 0; i <= 9; i++)
        {
            if (!string.IsNullOrEmpty(objCharacter.Guild.Ranks[i]))
            {
                guildRanksCount = (byte)(guildRanksCount + 1);
            }
        }

        // DONE: Count the members
        DataTable members = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query("SELECT char_online, char_guid, char_name, char_class, char_level, char_zone_id, char_logouttime, char_guildRank, char_guildPNote, char_guildOffNote FROM characters WHERE char_guildId = " + objCharacter.Guild.Id + ";", ref members);
        PacketClass response = new(Opcodes.SMSG_GUILD_ROSTER);
        response.AddInt32(members.Rows.Count);
        response.AddString(objCharacter.Guild.Motd);
        response.AddString(objCharacter.Guild.Info);
        response.AddInt32(guildRanksCount);
        for (var i = 0; i <= 9; i++)
        {
            if (!string.IsNullOrEmpty(objCharacter.Guild.Ranks[i]))
            {
                response.AddUInt32(objCharacter.Guild.RankRights[i]);
            }
        }

        var officer = objCharacter.IsGuildRightSet(GuildRankRights.GR_RIGHT_VIEWOFFNOTE);
        for (int i = 0, loopTo = members.Rows.Count - 1; i <= loopTo; i++)
        {
            if (members.Rows[i].As<byte>("char_online") == 1)
            {
                response.AddUInt64(members.Rows[i].As<ulong>("char_guid"));
                response.AddInt8(1);                         // OnlineFlag
                response.AddString(members.Rows[i].As<string>("char_name"));
                response.AddInt32(members.Rows[i].As<int>("char_guildRank"));
                response.AddInt8(members.Rows[i].As<byte>("char_level"));
                response.AddInt8(members.Rows[i].As<byte>("char_class"));
                response.AddInt32(members.Rows[i].As<int>("char_zone_id"));
                response.AddString(members.Rows[i].As<string>("char_guildPNote"));
                if (officer)
                {
                    response.AddString(members.Rows[i].As<string>("char_guildOffNote"));
                }
                else
                {
                    response.AddInt8(0);
                }
            }
            else
            {
                response.AddUInt64(members.Rows[i].As<ulong>("char_guid"));
                response.AddInt8(0);                         // OfflineFlag
                response.AddString(members.Rows[i].As<string>("char_name"));
                response.AddInt32(members.Rows[i].As<int>("char_guildRank"));
                response.AddInt8(members.Rows[i].As<byte>("char_level"));
                response.AddInt8(members.Rows[i].As<byte>("char_class"));
                response.AddInt32(members.Rows[i].As<int>("char_zone_id"));
                // 0 = < 1 hour / 0.1 = 2.4 hours / 1 = 24 hours (1 day)
                // (Time logged out / 86400) = Days offline
                var daysOffline = (float)((_clusterServiceLocator.Functions.GetTimestamp(DateAndTime.Now) - members.Rows[i].As<uint>("char_logouttime")) / (double)DateInterval.Day);
                response.AddSingle(daysOffline); // Days offline
                response.AddString(members.Rows[i].As<string>("char_guildPNote"));
                if (officer)
                {
                    response.AddString(members.Rows[i].As<string>("char_guildOffNote"));
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
        PacketClass response = new(Opcodes.SMSG_GUILD_COMMAND_RESULT);
        response.AddInt32((int)command);
        response.AddString(text);
        response.AddInt32((int)result);
        client.Send(response);
        response.Dispose();
    }

    public void NotifyGuildStatus(WcHandlerCharacter.CharacterObject objCharacter, GuildEvent status)
    {
        if (objCharacter.Guild is null)
        {
            return;
        }

        PacketClass statuspacket = new(Opcodes.SMSG_GUILD_EVENT);
        statuspacket.AddInt8((byte)status);
        statuspacket.AddInt8(1);
        statuspacket.AddString(objCharacter.Name);
        statuspacket.AddInt8(0);
        statuspacket.AddInt8(0);
        statuspacket.AddInt8(0);
        BroadcastToGuild(statuspacket, objCharacter.Guild, objCharacter.Guid);
        statuspacket.Dispose();
    }

    public void BroadcastToGuild(PacketClass packet, Guild guild, [Optional, DefaultParameterValue(0UL)] ulong notTo)
    {
        var tmpArray = guild.Members.ToArray();
        foreach (var member in tmpArray)
        {
            if (member == notTo)
            {
                continue;
            }

            if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(member))
            {
                _clusterServiceLocator.WorldCluster.CharacteRs[member].Client.SendMultiplyPackets(packet);
            }
        }
    }

    // Members Options
    public void SendGuildMotd(WcHandlerCharacter.CharacterObject objCharacter)
    {
        if (objCharacter.IsInGuild)
        {
            if (!string.IsNullOrEmpty(objCharacter.Guild.Motd))
            {
                PacketClass response = new(Opcodes.SMSG_GUILD_EVENT);
                response.AddInt8((byte)GuildEvent.MOTD);
                response.AddInt8(1);
                response.AddString(objCharacter.Guild.Motd);
                objCharacter.Client.Send(response);
                response.Dispose();
            }
        }
    }
}
