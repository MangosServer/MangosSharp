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
using Mangos.Common.Enums.Guild;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Data;

namespace Mangos.Cluster.Handlers;

public class WcHandlersGuild
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlersGuild(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void On_CMSG_GUILD_QUERY(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 9)
        {
            return;
        }

        packet.GetInt16();
        var guildId = packet.GetUInt32();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_QUERY [{2}]", client.IP, client.Port, guildId);
        _clusterServiceLocator.WcGuild.SendGuildQuery(client, guildId);
    }

    public void On_CMSG_GUILD_ROSTER(PacketClass packet, ClientClass client)
    {
        // packet.GetInt16()

        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_ROSTER", client.IP, client.Port);
        _clusterServiceLocator.WcGuild.SendGuildRoster(client.Character);
    }

    public void On_CMSG_GUILD_CREATE(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var guildName = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_CREATE [{2}]", client.IP, client.Port, guildName);
        if (client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_ALREADY_IN_GUILD);
            return;
        }

        // DONE: Create guild data
        DataTable mySqlQuery = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("INSERT INTO guilds (guild_name, guild_leader, guild_cYear, guild_cMonth, guild_cDay) VALUES (\"{0}\", {1}, {2}, {3}, {4}); SELECT guild_id FROM guilds WHERE guild_name = \"{0}\";", guildName, client.Character.Guid, DateAndTime.Now.Year - 2006, DateAndTime.Now.Month, DateAndTime.Now.Day), ref mySqlQuery);
        _clusterServiceLocator.WcGuild.AddCharacterToGuild(client.Character, mySqlQuery.Rows[0].As<int>("guild_id"), 0);
    }

    public void On_CMSG_GUILD_INFO(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_INFO", client.IP, client.Port);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        PacketClass response = new(Opcodes.SMSG_GUILD_INFO);
        response.AddString(client.Character.Guild.Name);
        response.AddInt32(client.Character.Guild.CDay);
        response.AddInt32(client.Character.Guild.CMonth);
        response.AddInt32(client.Character.Guild.CYear);
        response.AddInt32(0);
        response.AddInt32(0);
        client.Send(response);
        response.Dispose();
    }

    public void On_CMSG_GUILD_RANK(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 14)
        {
            return;
        }

        packet.GetInt16();
        var rankId = packet.GetInt32();
        var rankRights = packet.GetUInt32();
        var rankName = packet.GetString().Replace("\"", "_").Replace("'", "_");
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_RANK [{2}:{3}:{4}]", client.IP, client.Port, rankId, rankRights, rankName);
        if (rankId is < 0 or > 9)
        {
            return;
        }

        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildLeader)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        client.Character.Guild.Ranks[rankId] = rankName;
        client.Character.Guild.RankRights[rankId] = rankRights;
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE guilds SET guild_rank{1} = \"{2}\", guild_rank{1}_Rights = {3} WHERE guild_id = {0};", client.Character.Guild.Id, rankId, rankName, rankRights));
        _clusterServiceLocator.WcGuild.SendGuildQuery(client, client.Character.Guild.Id);
        _clusterServiceLocator.WcGuild.SendGuildRoster(client.Character);
    }

    public void On_CMSG_GUILD_ADD_RANK(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var newRankName = packet.GetString().Replace("\"", "_").Replace("'", "_");
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_ADD_RANK [{2}]", client.IP, client.Port, newRankName);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildLeader)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        if (_clusterServiceLocator.Functions.ValidateGuildName(newRankName) == false)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_INTERNAL);
            return;
        }

        for (var i = 0; i <= 9; i++)
        {
            if (string.IsNullOrEmpty(client.Character.Guild.Ranks[i]))
            {
                client.Character.Guild.Ranks[i] = newRankName;
                client.Character.Guild.RankRights[i] = (uint)(GuildRankRights.GR_RIGHT_GCHATLISTEN | GuildRankRights.GR_RIGHT_GCHATSPEAK);
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE guilds SET guild_rank{1} = '{2}', guild_rank{1}_Rights = '{3}' WHERE guild_id = {0};", client.Character.Guild.Id, i, newRankName, client.Character.Guild.RankRights[i]));
                _clusterServiceLocator.WcGuild.SendGuildQuery(client, client.Character.Guild.Id);
                _clusterServiceLocator.WcGuild.SendGuildRoster(client.Character);
                return;
            }
        }

        _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_INTERNAL);
    }

    public void On_CMSG_GUILD_DEL_RANK(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_DEL_RANK", client.IP, client.Port);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildLeader)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        // TODO: Check if someone in the guild is the rank we're removing?
        // TODO: Can we really remove all ranks?
        for (var i = 9; i >= 0; i -= 1)
        {
            if (!string.IsNullOrEmpty(client.Character.Guild.Ranks[i]))
            {
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE guilds SET guild_rank{1} = '{2}', guild_rank{1}_Rights = '{3}' WHERE guild_id = {0};", client.Character.Guild.Id, i, "", 0));
                _clusterServiceLocator.WcGuild.SendGuildQuery(client, client.Character.Guild.Id);
                _clusterServiceLocator.WcGuild.SendGuildRoster(client.Character);
                return;
            }
        }

        _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_INTERNAL);
    }

    public void On_CMSG_GUILD_LEADER(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var playerName = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_LEADER [{2}]", client.IP, client.Port, playerName);
        if (playerName.Length < 2)
        {
            return;
        }

        playerName = _clusterServiceLocator.Functions.CapitalizeName(playerName);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildLeader)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        // DONE: Find new leader's GUID
        DataTable mySqlQuery = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query("SELECT char_guid, char_guildId, char_guildrank FROM characters WHERE char_name = '" + playerName + "';", ref mySqlQuery);
        if (mySqlQuery.Rows.Count == 0)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PLAYER_NOT_FOUND, playerName);
            return;
        }

        if (mySqlQuery.Rows[0].As<uint>("char_guildId") != client.Character.Guild.Id)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD_S, playerName);
            return;
        }

        var playerGuid = mySqlQuery.Rows[0].As<ulong>("char_guid");
        client.Character.GuildRank = 1; // Officer
        client.Character.SendGuildUpdate();
        if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(playerGuid))
        {
            _clusterServiceLocator.WorldCluster.CharacteRs[playerGuid].GuildRank = 0;
            _clusterServiceLocator.WorldCluster.CharacteRs[playerGuid].SendGuildUpdate();
        }

        client.Character.Guild.Leader = playerGuid;
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE guilds SET guild_leader = \"{1}\" WHERE guild_id = {0};", client.Character.Guild.Id, playerGuid));
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildRank = {0} WHERE char_guid = {1};", 0, playerGuid));
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildRank = {0} WHERE char_guid = {1};", client.Character.GuildRank, client.Character.Guid));

        // DONE: Send notify message
        PacketClass response = new(Opcodes.SMSG_GUILD_EVENT);
        response.AddInt8((byte)GuildEvent.LEADER_CHANGED);
        response.AddInt8(2);
        response.AddString(client.Character.Name);
        response.AddString(playerName);
        var argnotTo = 0UL;
        _clusterServiceLocator.WcGuild.BroadcastToGuild(response, client.Character.Guild, notTo: argnotTo);
        response.Dispose();
    }

    public void On_MSG_SAVE_GUILD_EMBLEM(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length < 34)
        {
            return;
        }

        packet.GetInt16();
        var unk0 = packet.GetInt32();
        var unk1 = packet.GetInt32();
        var tEmblemStyle = packet.GetInt32();
        var tEmblemColor = packet.GetInt32();
        var tBorderStyle = packet.GetInt32();
        var tBorderColor = packet.GetInt32();
        var tBackgroundColor = packet.GetInt32();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_SAVE_GUILD_EMBLEM [{2},{3}] [{4}:{5}:{6}:{7}:{8}]", client.IP, client.Port, unk0, unk1, tEmblemStyle, tEmblemColor, tBorderStyle, tBorderColor, tBackgroundColor);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildLeader)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;

            // TODO: Check if you have enough money
            // ElseIf client.Character.Copper < 100000 Then
            // SendInventoryChangeFailure(Client.Character, InventoryChangeFailure.EQUIP_ERR_NOT_ENOUGH_MONEY, 0, 0)
            // Exit Sub
        }

        client.Character.Guild.EmblemStyle = (byte)tEmblemStyle;
        client.Character.Guild.EmblemColor = (byte)tEmblemColor;
        client.Character.Guild.BorderStyle = (byte)tBorderStyle;
        client.Character.Guild.BorderColor = (byte)tBorderColor;
        client.Character.Guild.BackgroundColor = (byte)tBackgroundColor;
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE guilds SET guild_tEmblemStyle = {1}, guild_tEmblemColor = {2}, guild_tBorderStyle = {3}, guild_tBorderColor = {4}, guild_tBackgroundColor = {5} WHERE guild_id = {0};", client.Character.Guild.Id, tEmblemStyle, tEmblemColor, tBorderStyle, tBorderColor, tBackgroundColor));
        _clusterServiceLocator.WcGuild.SendGuildQuery(client, client.Character.Guild.Id);
        PacketClass packetEvent = new(Opcodes.SMSG_GUILD_EVENT);
        packetEvent.AddInt8((byte)GuildEvent.TABARDCHANGE);
        packetEvent.AddInt32((int)client.Character.Guild.Id);
        var argnotTo = 0UL;
        _clusterServiceLocator.WcGuild.BroadcastToGuild(packetEvent, client.Character.Guild, notTo: argnotTo);
        packetEvent.Dispose();

        // TODO: This tabard design costs 10g!
        // Client.Character.Copper -= 100000
        // Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper)
        // Client.Character.SendCharacterUpdate(False)
    }

    public void On_CMSG_GUILD_DISBAND(PacketClass packet, ClientClass client)
    {
        // packet.GetInt16()

        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_DISBAND", client.IP, client.Port);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildLeader)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        // DONE: Clear all members
        PacketClass response = new(Opcodes.SMSG_GUILD_EVENT);
        response.AddInt8((byte)GuildEvent.DISBANDED);
        response.AddInt8(0);
        var guildId = (int)client.Character.Guild.Id;
        var tmpArray = client.Character.Guild.Members.ToArray();
        foreach (var member in tmpArray)
        {
            if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(member))
            {
                var tmp = _clusterServiceLocator.WorldCluster.CharacteRs;
                var argobjCharacter = tmp[member];
                _clusterServiceLocator.WcGuild.RemoveCharacterFromGuild(argobjCharacter);
                tmp[member] = argobjCharacter;
                _clusterServiceLocator.WorldCluster.CharacteRs[member].Client.SendMultiplyPackets(response);
            }
            else
            {
                _clusterServiceLocator.WcGuild.RemoveCharacterFromGuild(member);
            }
        }

        _clusterServiceLocator.WcGuild.GuilDs[(uint)guildId].Dispose();
        response.Dispose();

        // DONE: Delete guild information
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update("DELETE FROM guilds WHERE guild_id = " + guildId + ";");
    }

    public void On_CMSG_GUILD_MOTD(PacketClass packet, ClientClass client)
    {
        // Isn't the client even sending a null terminator for the motd if it's empty?
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var motd = "";
        if (packet.Length != 4)
        {
            motd = packet.GetString().Replace("\"", "_").Replace("'", "_");
        }

        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_MOTD", client.IP, client.Port);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildRightSet(GuildRankRights.GR_RIGHT_SETMOTD))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        client.Character.Guild.Motd = motd;
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE guilds SET guild_MOTD = '{1}' WHERE guild_id = '{0}';", client.Character.Guild.Id, motd));
        PacketClass response = new(Opcodes.SMSG_GUILD_EVENT);
        response.AddInt8((byte)GuildEvent.MOTD);
        response.AddInt8(1);
        response.AddString(motd);

        // DONE: Send message to everyone in the guild
        var argnotTo = 0UL;
        _clusterServiceLocator.WcGuild.BroadcastToGuild(response, client.Character.Guild, notTo: argnotTo);
        response.Dispose();
    }

    public void On_CMSG_GUILD_SET_OFFICER_NOTE(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var playerName = packet.GetString();
        if (packet.Data.Length - 1 < 6 + playerName.Length + 1)
        {
            return;
        }

        var note = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_SET_OFFICER_NOTE [{2}]", client.IP, client.Port, playerName);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildRightSet(GuildRankRights.GR_RIGHT_EOFFNOTE))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildOffNote = \"{1}\" WHERE char_name = \"{0}\";", playerName, note.Replace("\"", "_").Replace("'", "_")));
        _clusterServiceLocator.WcGuild.SendGuildRoster(client.Character);
    }

    public void On_CMSG_GUILD_SET_PUBLIC_NOTE(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var playerName = packet.GetString();
        if (packet.Data.Length - 1 < 6 + playerName.Length + 1)
        {
            return;
        }

        var note = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_SET_PUBLIC_NOTE [{2}]", client.IP, client.Port, playerName);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildRightSet(GuildRankRights.GR_RIGHT_EPNOTE))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildPNote = \"{1}\" WHERE char_name = \"{0}\";", playerName, note.Replace("\"", "_").Replace("'", "_")));
        _clusterServiceLocator.WcGuild.SendGuildRoster(client.Character);
    }

    public void On_CMSG_GUILD_REMOVE(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var playerName = packet.GetString().Replace("'", "_").Replace("\"", "_");
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_REMOVE [{2}]", client.IP, client.Port, playerName);
        if (playerName.Length < 2)
        {
            return;
        }

        playerName = _clusterServiceLocator.Functions.CapitalizeName(playerName);

        // DONE: Player1 checks
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildRightSet(GuildRankRights.GR_RIGHT_REMOVE))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        // DONE: Find player2's guid
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query("SELECT char_guid FROM characters WHERE char_name = '" + playerName + "';", ref q);

        // DONE: Removed checks
        if (q.Rows.Count == 0)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PLAYER_NOT_FOUND, playerName);
            return;
        }

        if (!_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(q.Rows[0].As<ulong>("char_guid")))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PLAYER_NOT_FOUND, playerName);
            return;
        }

        var objCharacter = _clusterServiceLocator.WorldCluster.CharacteRs[q.Rows[0].As<ulong>("char_guid")];
        if (objCharacter.IsGuildLeader)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_QUIT_S, GuildError.GUILD_LEADER_LEAVE);
            return;
        }

        // DONE: Send guild event
        PacketClass response = new(Opcodes.SMSG_GUILD_EVENT);
        response.AddInt8((byte)GuildEvent.REMOVED);
        response.AddInt8(2);
        response.AddString(playerName);
        response.AddString(objCharacter.Name);
        var argnotTo = 0UL;
        _clusterServiceLocator.WcGuild.BroadcastToGuild(response, client.Character.Guild, notTo: argnotTo);
        response.Dispose();
        _clusterServiceLocator.WcGuild.RemoveCharacterFromGuild(objCharacter);
    }

    public void On_CMSG_GUILD_PROMOTE(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var playerName = _clusterServiceLocator.Functions.CapitalizeName(packet.GetString());
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_PROMOTE [{2}]", client.IP, client.Port, playerName);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildRightSet(GuildRankRights.GR_RIGHT_PROMOTE))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        // DONE: Find promoted player's guid
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query("SELECT char_guid FROM characters WHERE char_name = '" + playerName.Replace("'", "_") + "';", ref q);

        // DONE: Promoted checks
        if (q.Rows.Count == 0)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_NAME_INVALID);
            return;
        }

        if (!_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(q.Rows[0].As<ulong>("char_guid")))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PLAYER_NOT_FOUND, playerName);
            return;
        }

        var objCharacter = _clusterServiceLocator.WorldCluster.CharacteRs[q.Rows[0].As<ulong>("char_guid")];
        if (objCharacter.Guild.Id != client.Character.Guild.Id)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD_S, playerName);
            return;
        }

        if (objCharacter.GuildRank <= client.Character.GuildRank)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        if (objCharacter.GuildRank == _clusterServiceLocator.GlobalConstants.GUILD_RANK_MIN)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_INTERNAL);
            return;
        }

        // DONE: Do the real update
        objCharacter.GuildRank = (byte)(objCharacter.GuildRank - 1);
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildRank = {0} WHERE char_guid = {1};", objCharacter.GuildRank, objCharacter.Guid));
        objCharacter.SendGuildUpdate();

        // DONE: Send event to guild
        PacketClass response = new(Opcodes.SMSG_GUILD_EVENT);
        response.AddInt8((byte)GuildEvent.PROMOTION);
        response.AddInt8(3);
        response.AddString(objCharacter.Name);
        response.AddString(playerName);
        response.AddString(client.Character.Guild.Ranks[objCharacter.GuildRank]);
        var argnotTo = 0UL;
        _clusterServiceLocator.WcGuild.BroadcastToGuild(response, client.Character.Guild, notTo: argnotTo);
        response.Dispose();
    }

    public void On_CMSG_GUILD_DEMOTE(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var playerName = _clusterServiceLocator.Functions.CapitalizeName(packet.GetString());
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_DEMOTE [{2}]", client.IP, client.Port, playerName);
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildRightSet(GuildRankRights.GR_RIGHT_PROMOTE))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        // DONE: Find demoted player's guid
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query("SELECT char_guid FROM characters WHERE char_name = '" + playerName.Replace("'", "_") + "';", ref q);

        // DONE: Demoted checks
        if (q.Rows.Count == 0)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_NAME_INVALID);
            return;
        }

        if (!_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(q.Rows[0].As<ulong>("char_guid")))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PLAYER_NOT_FOUND, playerName);
            return;
        }

        var objCharacter = _clusterServiceLocator.WorldCluster.CharacteRs[q.Rows[0].As<ulong>("char_guid")];
        if (objCharacter.Guild.Id != client.Character.Guild.Id)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD_S, playerName);
            return;
        }

        if (objCharacter.GuildRank <= client.Character.GuildRank)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        if (objCharacter.GuildRank == _clusterServiceLocator.GlobalConstants.GUILD_RANK_MAX)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_INTERNAL);
            return;
        }

        // DONE: Max defined rank check
        if (string.IsNullOrEmpty(Strings.Trim(client.Character.Guild.Ranks[objCharacter.GuildRank + 1])))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_INTERNAL);
            return;
        }

        // DONE: Do the real update
        objCharacter.GuildRank = (byte)(objCharacter.GuildRank + 1);
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildRank = {0} WHERE char_guid = {1};", objCharacter.GuildRank, objCharacter.Guid));
        objCharacter.SendGuildUpdate();

        // DONE: Send event to guild
        PacketClass response = new(Opcodes.SMSG_GUILD_EVENT);
        response.AddInt8((byte)GuildEvent.DEMOTION);
        response.AddInt8(3);
        response.AddString(objCharacter.Name);
        response.AddString(playerName);
        response.AddString(client.Character.Guild.Ranks[objCharacter.GuildRank]);
        var argnotTo = 0UL;
        _clusterServiceLocator.WcGuild.BroadcastToGuild(response, client.Character.Guild, notTo: argnotTo);
        response.Dispose();
    }

    // User Options
    public void On_CMSG_GUILD_INVITE(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        var playerName = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_INVITE [{2}]", client.IP, client.Port, playerName);

        // DONE: Inviter checks
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (!client.Character.IsGuildRightSet(GuildRankRights.GR_RIGHT_INVITE))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
            return;
        }

        // DONE: Find invited player's guid
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query("SELECT char_guid FROM characters WHERE char_name = '" + playerName.Replace("'", "_") + "';", ref q);

        // DONE: Invited checks
        if (q.Rows.Count == 0)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_NAME_INVALID);
            return;
        }

        if (!_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(q.Rows[0].As<ulong>("char_guid")))
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_PLAYER_NOT_FOUND, playerName);
            return;
        }

        var objCharacter = _clusterServiceLocator.WorldCluster.CharacteRs[q.Rows[0].As<ulong>("char_guid")];
        if (objCharacter.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.ALREADY_IN_GUILD, playerName);
            return;
        }

        if (objCharacter.Side != client.Character.Side)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.GUILD_NOT_ALLIED, playerName);
            return;
        }

        if (objCharacter.GuildInvited != 0L)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_INVITE_S, GuildError.ALREADY_INVITED_TO_GUILD, playerName);
            return;
        }

        PacketClass response = new(Opcodes.SMSG_GUILD_INVITE);
        response.AddString(client.Character.Name);
        response.AddString(client.Character.Guild.Name);
        objCharacter.Client.Send(response);
        response.Dispose();
        objCharacter.GuildInvited = client.Character.Guild.Id;
        objCharacter.GuildInvitedBy = client.Character.Guid;
    }

    public void On_CMSG_GUILD_ACCEPT(PacketClass packet, ClientClass client)
    {
        if (client.Character.GuildInvited == 0L)
        {
            throw new ApplicationException("Character accepting guild invitation whihtout being invited.");
        }

        _clusterServiceLocator.WcGuild.AddCharacterToGuild(client.Character, (int)client.Character.GuildInvited);
        client.Character.GuildInvited = 0U;
        PacketClass response = new(Opcodes.SMSG_GUILD_EVENT);
        response.AddInt8((byte)GuildEvent.JOINED);
        response.AddInt8(1);
        response.AddString(client.Character.Name);
        var argnotTo = 0UL;
        _clusterServiceLocator.WcGuild.BroadcastToGuild(response, client.Character.Guild, notTo: argnotTo);
        response.Dispose();
        _clusterServiceLocator.WcGuild.SendGuildRoster(client.Character);
        _clusterServiceLocator.WcGuild.SendGuildMotd(client.Character);
    }

    public void On_CMSG_GUILD_DECLINE(PacketClass packet, ClientClass client)
    {
        client.Character.GuildInvited = 0U;
        if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey((ulong)Conversions.ToLong(client.Character.GuildInvitedBy)))
        {
            PacketClass response = new(Opcodes.SMSG_GUILD_DECLINE);
            response.AddString(client.Character.Name);
            _clusterServiceLocator.WorldCluster.CharacteRs[(ulong)Conversions.ToLong(client.Character.GuildInvitedBy)].Client.Send(response);
            response.Dispose();
        }
    }

    public void On_CMSG_GUILD_LEAVE(PacketClass packet, ClientClass client)
    {
        // packet.GetInt16()

        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GUILD_LEAVE", client.IP, client.Port);

        // DONE: Checks
        if (!client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
            return;
        }

        if (client.Character.IsGuildLeader)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_QUIT_S, GuildError.GUILD_LEADER_LEAVE);
            return;
        }

        _clusterServiceLocator.WcGuild.RemoveCharacterFromGuild(client.Character);
        _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_QUIT_S, GuildError.GUILD_PLAYER_NO_MORE_IN_GUILD, client.Character.Name);
        PacketClass response = new(Opcodes.SMSG_GUILD_EVENT);
        response.AddInt8((byte)GuildEvent.LEFT);
        response.AddInt8(1);
        response.AddString(client.Character.Name);
        var argnotTo = 0UL;
        _clusterServiceLocator.WcGuild.BroadcastToGuild(response, client.Character.Guild, notTo: argnotTo);
        response.Dispose();
    }

    public void On_CMSG_TURN_IN_PETITION(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 13)
        {
            return;
        }

        packet.GetInt16();
        var itemGuid = packet.GetUInt64();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TURN_IN_PETITION [GUID={2:X}]", client.IP, client.Port, itemGuid);

        // DONE: Get info
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query("SELECT * FROM petitions WHERE petition_itemGuid = " + (itemGuid - _clusterServiceLocator.GlobalConstants.GUID_ITEM) + " LIMIT 1;", ref q);
        if (q.Rows.Count == 0)
        {
            return;
        }

        var type = q.Rows[0].As<byte>("petition_type");
        var name = q.Rows[0].As<string>("petition_name");

        // DONE: Check if already in guild
        if (type == 9 && client.Character.IsInGuild)
        {
            PacketClass response = new(Opcodes.SMSG_TURN_IN_PETITION_RESULTS);
            response.AddInt32((int)PetitionTurnInError.PETITIONTURNIN_ALREADY_IN_GUILD);
            client.Send(response);
            response.Dispose();
            return;
        }

        // DONE: Check required signs
        byte requiredSigns = 9;
        if (q.Rows[0].As<int>("petition_signedMembers") < requiredSigns)
        {
            PacketClass response = new(Opcodes.SMSG_TURN_IN_PETITION_RESULTS);
            response.AddInt32((int)PetitionTurnInError.PETITIONTURNIN_NEED_MORE_SIGNATURES);
            client.Send(response);
            response.Dispose();
            return;
        }

        DataTable q2 = new();

        // DONE: Create guild and add members
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("INSERT INTO guilds (guild_name, guild_leader, guild_cYear, guild_cMonth, guild_cDay) VALUES ('{0}', {1}, {2}, {3}, {4}); SELECT guild_id FROM guilds WHERE guild_name = '{0}';", name, client.Character.Guid, DateAndTime.Now.Year - 2006, DateAndTime.Now.Month, DateAndTime.Now.Day), ref q2);
        _clusterServiceLocator.WcGuild.AddCharacterToGuild(client.Character, q2.Rows[0].As<int>("guild_id"), 0);

        // DONE: Adding 9 more signed _WorldCluster.CHARACTERs
        for (byte i = 1; i <= 9; i++)
        {
            if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(q.Rows[0].As<ulong>("petition_signedMember" + i)))
            {
                var tmp = _clusterServiceLocator.WorldCluster.CharacteRs;
                var argobjCharacter = tmp[q.Rows[0].As<ulong>("petition_signedMember") + i];
                _clusterServiceLocator.WcGuild.AddCharacterToGuild(argobjCharacter, q2.Rows[0].As<int>("guild_id"));
                tmp[q.Rows[0].As<ulong>("petition_signedMember") + i] = argobjCharacter;
            }
            else
            {
                _clusterServiceLocator.WcGuild.AddCharacterToGuild(q.Rows[0].As<ulong>("petition_signedMember" + i), q2.Rows[0].As<int>("guild_id"));
            }
        }

        // DONE: Delete guild charter item, on the world server
        client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
        PacketClass success = new(Opcodes.SMSG_TURN_IN_PETITION_RESULTS);
        success.AddInt32(0); // Okay
        client.Send(success);
        success.Dispose();
    }
}
