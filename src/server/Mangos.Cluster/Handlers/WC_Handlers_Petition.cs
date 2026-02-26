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

using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Guild;
using Mangos.Common.Globals;
using Mangos.MySql;
using System.Data;

namespace Mangos.Cluster.Handlers;

public class WcHandlersPetition
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlersPetition(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void On_CMSG_PETITION_SHOWLIST(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 13)
        {
            return;
        }

        packet.GetInt16();
        var npcGuid = packet.GetUInt64();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(
            LogType.DEBUG,
            "[{0}:{1}] CMSG_PETITION_SHOWLIST [GUID={2:X}]",
            client.IP, client.Port, npcGuid);

        PacketClass response = new(Opcodes.SMSG_PETITION_SHOWLIST);
        response.AddUInt64(npcGuid);
        response.AddInt8(1);
        response.AddInt32(1);
        response.AddInt32(5863);
        response.AddInt32(16161);
        response.AddInt32(1000);
        response.AddInt32(0);
        response.AddInt32(9);
        client.Send(response);
        response.Dispose();
    }

    public void On_CMSG_PETITION_BUY(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 26)
        {
            return;
        }

        packet.GetInt16();
        var npcGuid = packet.GetUInt64();
        packet.GetInt32();
        packet.GetInt32();
        var guildName = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(
            LogType.DEBUG,
            "[{0}:{1}] CMSG_PETITION_BUY [NPC={2:X}, Name={3}]",
            client.IP, client.Port, npcGuid, guildName);

        if (client.Character.IsInGuild)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_ALREADY_IN_GUILD);
            return;
        }

        DataTable existingPetition = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(
            "SELECT petition_id FROM petitions WHERE petition_player = " + client.Character.Guid + ";",
            ref existingPetition);
        if (existingPetition.Rows.Count > 0)
        {
            client.Send(new PacketClass(Opcodes.SMSG_TURN_IN_PETITION_RESULTS));
            return;
        }

        DataTable nameCheck = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(
            "SELECT guild_id FROM guilds WHERE guild_name = '" + guildName.Replace("'", "''") + "';",
            ref nameCheck);
        if (nameCheck.Rows.Count > 0)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_NAME_EXISTS);
            return;
        }

        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(
            string.Format(
                "INSERT INTO petitions (petition_player, petition_name, petition_type) VALUES ({0}, '{1}', 9);",
                client.Character.Guid, guildName.Replace("'", "''")));
    }

    public void On_CMSG_PETITION_SHOW_SIGNATURES(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 13)
        {
            return;
        }

        packet.GetInt16();
        var itemGuid = packet.GetUInt64();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(
            LogType.DEBUG,
            "[{0}:{1}] CMSG_PETITION_SHOW_SIGNATURES [GUID={2:X}]",
            client.IP, client.Port, itemGuid);

        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(
            "SELECT * FROM petitions WHERE petition_itemGuid = " +
            (itemGuid - _clusterServiceLocator.GlobalConstants.GUID_ITEM) + " LIMIT 1;",
            ref q);
        if (q.Rows.Count == 0)
        {
            return;
        }

        var petitionId = q.Rows[0].As<int>("petition_id");
        var ownerGuid = q.Rows[0].As<ulong>("petition_player");

        DataTable signatures = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(
            "SELECT petition_player FROM petition_signs WHERE petition_id = " + petitionId + ";",
            ref signatures);

        PacketClass response = new(Opcodes.SMSG_PETITION_SHOW_SIGNATURES);
        response.AddUInt64(itemGuid);
        response.AddUInt64(ownerGuid);
        response.AddInt32(petitionId);
        response.AddInt8((byte)signatures.Rows.Count);

        for (var i = 0; i < signatures.Rows.Count; i++)
        {
            response.AddUInt64(signatures.Rows[i].As<ulong>("petition_player"));
            response.AddInt32(0);
        }

        client.Send(response);
        response.Dispose();
    }

    public void On_CMSG_PETITION_SIGN(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 14)
        {
            return;
        }

        packet.GetInt16();
        var itemGuid = packet.GetUInt64();
        var unk1 = packet.GetInt8();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(
            LogType.DEBUG,
            "[{0}:{1}] CMSG_PETITION_SIGN [GUID={2:X}]",
            client.IP, client.Port, itemGuid);

        if (client.Character.IsInGuild)
        {
            PacketClass signResult = new(Opcodes.SMSG_PETITION_SIGN_RESULTS);
            signResult.AddUInt64(itemGuid);
            signResult.AddUInt64(client.Character.Guid);
            signResult.AddInt32((int)PetitionSignError.PETITIONSIGN_ALREADY_IN_GUILD);
            client.Send(signResult);
            signResult.Dispose();
            return;
        }

        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(
            "SELECT * FROM petitions WHERE petition_itemGuid = " +
            (itemGuid - _clusterServiceLocator.GlobalConstants.GUID_ITEM) + " LIMIT 1;",
            ref q);
        if (q.Rows.Count == 0)
        {
            return;
        }

        var petitionId = q.Rows[0].As<int>("petition_id");
        var ownerGuid = q.Rows[0].As<ulong>("petition_player");

        if (ownerGuid == client.Character.Guid)
        {
            PacketClass signResult = new(Opcodes.SMSG_PETITION_SIGN_RESULTS);
            signResult.AddUInt64(itemGuid);
            signResult.AddUInt64(client.Character.Guid);
            signResult.AddInt32((int)PetitionSignError.PETITIONSIGN_CANT_SIGN_OWN);
            client.Send(signResult);
            signResult.Dispose();
            return;
        }

        DataTable alreadySigned = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(
            string.Format("SELECT petition_id FROM petition_signs WHERE petition_id = {0} AND petition_player = {1};",
                petitionId, client.Character.Guid),
            ref alreadySigned);
        if (alreadySigned.Rows.Count > 0)
        {
            PacketClass signResult = new(Opcodes.SMSG_PETITION_SIGN_RESULTS);
            signResult.AddUInt64(itemGuid);
            signResult.AddUInt64(client.Character.Guid);
            signResult.AddInt32((int)PetitionSignError.PETITIONSIGN_ALREADY_SIGNED);
            client.Send(signResult);
            signResult.Dispose();
            return;
        }

        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(
            string.Format("INSERT INTO petition_signs (petition_id, petition_player) VALUES ({0}, {1});",
                petitionId, client.Character.Guid));

        PacketClass result = new(Opcodes.SMSG_PETITION_SIGN_RESULTS);
        result.AddUInt64(itemGuid);
        result.AddUInt64(client.Character.Guid);
        result.AddInt32((int)PetitionSignError.PETITIONSIGN_OK);
        client.Send(result);
        result.Dispose();

        if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(ownerGuid))
        {
            PacketClass ownerNotify = new(Opcodes.SMSG_PETITION_SIGN_RESULTS);
            ownerNotify.AddUInt64(itemGuid);
            ownerNotify.AddUInt64(client.Character.Guid);
            ownerNotify.AddInt32((int)PetitionSignError.PETITIONSIGN_OK);
            _clusterServiceLocator.WorldCluster.CharacteRs[ownerGuid].Client.Send(ownerNotify);
            ownerNotify.Dispose();
        }
    }

    public void On_CMSG_PETITION_QUERY(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 17)
        {
            return;
        }

        packet.GetInt16();
        var petitionId = packet.GetInt32();
        var itemGuid = packet.GetUInt64();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(
            LogType.DEBUG,
            "[{0}:{1}] CMSG_PETITION_QUERY [ID={2}, GUID={3:X}]",
            client.IP, client.Port, petitionId, itemGuid);

        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(
            "SELECT * FROM petitions WHERE petition_id = " + petitionId + " LIMIT 1;",
            ref q);
        if (q.Rows.Count == 0)
        {
            return;
        }

        var petitionName = q.Rows[0].As<string>("petition_name");
        var ownerGuid = q.Rows[0].As<ulong>("petition_player");

        PacketClass response = new(Opcodes.SMSG_PETITION_QUERY_RESPONSE);
        response.AddInt32(petitionId);
        response.AddUInt64(ownerGuid);
        response.AddString(petitionName);
        response.AddString("");
        response.AddInt32(9);
        response.AddInt32(9);
        response.AddInt32(0);
        response.AddInt32(0);
        response.AddInt32(0);
        response.AddInt32(0);
        response.AddInt32(0);
        response.AddInt16(0);
        response.AddInt32(0);
        response.AddInt32(0);
        response.AddInt32(0);
        for (var i = 0; i < 10; i++)
        {
            response.AddString("");
        }
        response.AddInt32(0);
        response.AddInt32(0);

        client.Send(response);
        response.Dispose();
    }

    public void On_CMSG_OFFER_PETITION(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 21)
        {
            return;
        }

        packet.GetInt16();
        var petitionType = packet.GetInt32();
        var itemGuid = packet.GetUInt64();
        var targetGuid = packet.GetUInt64();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(
            LogType.DEBUG,
            "[{0}:{1}] CMSG_OFFER_PETITION [GUID={2:X}, Target={3:X}]",
            client.IP, client.Port, itemGuid, targetGuid);

        _clusterServiceLocator.WorldCluster.CharacteRsLock.EnterReadLock();
        try
        {
            if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(targetGuid))
            {
                var target = _clusterServiceLocator.WorldCluster.CharacteRs[targetGuid];
                if (target.IsInGuild)
                {
                    _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_ALREADY_IN_GUILD, target.Name);
                    return;
                }

                On_CMSG_PETITION_SHOW_SIGNATURES(packet, target.Client);
            }
        }
        finally
        {
            _clusterServiceLocator.WorldCluster.CharacteRsLock.ExitReadLock();
        }
    }

    public void On_MSG_PETITION_DECLINE(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 13)
        {
            return;
        }

        packet.GetInt16();
        var itemGuid = packet.GetUInt64();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(
            LogType.DEBUG,
            "[{0}:{1}] MSG_PETITION_DECLINE [GUID={2:X}]",
            client.IP, client.Port, itemGuid);

        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(
            "SELECT petition_player FROM petitions WHERE petition_itemGuid = " +
            (itemGuid - _clusterServiceLocator.GlobalConstants.GUID_ITEM) + " LIMIT 1;",
            ref q);
        if (q.Rows.Count == 0)
        {
            return;
        }

        var ownerGuid = q.Rows[0].As<ulong>("petition_player");
        if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(ownerGuid))
        {
            PacketClass decline = new(Opcodes.MSG_PETITION_DECLINE);
            decline.AddUInt64(client.Character.Guid);
            _clusterServiceLocator.WorldCluster.CharacteRs[ownerGuid].Client.Send(decline);
            decline.Dispose();
        }
    }

    public void On_MSG_PETITION_RENAME(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 13)
        {
            return;
        }

        packet.GetInt16();
        var itemGuid = packet.GetUInt64();
        var newName = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(
            LogType.DEBUG,
            "[{0}:{1}] MSG_PETITION_RENAME [GUID={2:X}, Name={3}]",
            client.IP, client.Port, itemGuid, newName);

        DataTable nameCheck = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(
            "SELECT guild_id FROM guilds WHERE guild_name = '" + newName.Replace("'", "''") + "';",
            ref nameCheck);
        if (nameCheck.Rows.Count > 0)
        {
            _clusterServiceLocator.WcGuild.SendGuildResult(client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_NAME_EXISTS);
            return;
        }

        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(
            string.Format("UPDATE petitions SET petition_name = '{0}' WHERE petition_itemGuid = {1};",
                newName.Replace("'", "''"),
                itemGuid - _clusterServiceLocator.GlobalConstants.GUID_ITEM));

        PacketClass response = new(Opcodes.MSG_PETITION_RENAME);
        response.AddUInt64(itemGuid);
        response.AddString(newName);
        client.Send(response);
        response.Dispose();
    }
}
