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
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Social;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;

namespace Mangos.Cluster.Handlers;

public class WcHandlersSocial
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlersSocial(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void LoadIgnoreList(WcHandlerCharacter.CharacterObject objCharacter)
    {
        // DONE: Query DB
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT * FROM character_social WHERE guid = {0} AND flags = {1};", objCharacter.Guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_IGNORED)), ref q);

        // DONE: Add to list
        foreach (DataRow row in q.Rows)
        {
            objCharacter.IgnoreList.Add(row.As<ulong>("friend"));
        }
    }

    public void SendFriendList(ClientClass client, WcHandlerCharacter.CharacterObject character)
    {
        // DONE: Query DB
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT * FROM character_social WHERE guid = {0} AND (flags & {1}) > 0;", character.Guid, Conversions.ToInteger(SocialFlag.SOCIAL_FLAG_FRIEND)), ref q);

        // DONE: Make the packet
        PacketClass smsgFriendList = new(Opcodes.SMSG_FRIEND_LIST);
        if (q.Rows.Count > 0)
        {
            smsgFriendList.AddInt8((byte)q.Rows.Count);
            foreach (DataRow row in q.Rows)
            {
                var guid = row.As<ulong>("friend");
                smsgFriendList.AddUInt64(guid);                    // Player GUID
                if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(guid) && _clusterServiceLocator.WorldCluster.CharacteRs[guid].IsInWorld)
                {
                    if (_clusterServiceLocator.WorldCluster.CharacteRs[guid].Dnd)
                    {
                        smsgFriendList.AddInt8((byte)FriendStatus.FRIEND_STATUS_DND);
                    }
                    else if (_clusterServiceLocator.WorldCluster.CharacteRs[guid].Afk)
                    {
                        smsgFriendList.AddInt8((byte)FriendStatus.FRIEND_STATUS_AFK);
                    }
                    else
                    {
                        smsgFriendList.AddInt8((byte)FriendStatus.FRIEND_STATUS_ONLINE);
                    }

                    smsgFriendList.AddInt32((int)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Zone);    // Area
                    smsgFriendList.AddInt32(_clusterServiceLocator.WorldCluster.CharacteRs[guid].Level);   // Level
                    smsgFriendList.AddInt32((int)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Classe);  // Class
                }
                else
                {
                    smsgFriendList.AddInt8((byte)FriendStatus.FRIEND_STATUS_OFFLINE);
                }
            }
        }
        else
        {
            smsgFriendList.AddInt8(0);
        }

        client.Send(smsgFriendList);
        smsgFriendList.Dispose();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_LIST", client.IP, client.Port);
    }

    public void SendIgnoreList(ClientClass client, WcHandlerCharacter.CharacterObject character)
    {
        // DONE: Query DB
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT * FROM character_social WHERE guid = {0} AND (flags & {1}) > 0;", character.Guid, Conversions.ToInteger(SocialFlag.SOCIAL_FLAG_IGNORED)), ref q);

        // DONE: Make the packet
        PacketClass smsgIgnoreList = new(Opcodes.SMSG_IGNORE_LIST);
        if (q.Rows.Count > 0)
        {
            smsgIgnoreList.AddInt8((byte)q.Rows.Count);
            foreach (DataRow row in q.Rows)
            {
                smsgIgnoreList.AddUInt64(row.As<ulong>("friend"));                    // Player GUID
            }
        }
        else
        {
            smsgIgnoreList.AddInt8(0);
        }

        client.Send(smsgIgnoreList);
        smsgIgnoreList.Dispose();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_IGNORE_LIST", client.IP, client.Port);
    }

    public void NotifyFriendStatus(WcHandlerCharacter.CharacterObject objCharacter, FriendStatus s)
    {
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT guid FROM character_social WHERE friend = {0} AND (flags & {1}) > 0;", objCharacter.Guid, Conversions.ToInteger(SocialFlag.SOCIAL_FLAG_FRIEND)), ref q);

        // DONE: Send "Friend offline/online"
        PacketClass friendpacket = new(Opcodes.SMSG_FRIEND_STATUS);
        friendpacket.AddInt8((byte)s);
        friendpacket.AddUInt64(objCharacter.Guid);
        foreach (DataRow row in q.Rows)
        {
            var guid = row.As<ulong>("guid");
            if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(guid) && _clusterServiceLocator.WorldCluster.CharacteRs[guid].Client is not null)
            {
                _clusterServiceLocator.WorldCluster.CharacteRs[guid].Client.SendMultiplyPackets(friendpacket);
            }
        }

        friendpacket.Dispose();
    }

    public void On_CMSG_WHO(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();
        var levelMinimum = packet.GetUInt32();       // 0
        var levelMaximum = packet.GetUInt32();       // 100
        var namePlayer = _clusterServiceLocator.Functions.EscapeString(packet.GetString());
        var nameGuild = _clusterServiceLocator.Functions.EscapeString(packet.GetString());
        var maskRace = packet.GetUInt32();
        var maskClass = packet.GetUInt32();
        var zonesCount = packet.GetUInt32();         // Limited to 10
        if (zonesCount > 10L)
        {
            return;
        }

        List<uint> zones = new();
        for (int i = 1, loopTo = (int)zonesCount; i <= loopTo; i++)
        {
            zones.Add(packet.GetUInt32());
        }

        var stringsCount = packet.GetUInt32();         // Limited to 4
        if (stringsCount > 4L)
        {
            return;
        }

        List<string> strings = new();
        for (int i = 1, loopTo1 = (int)stringsCount; i <= loopTo1; i++)
        {
            strings.Add(_clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(_clusterServiceLocator.Functions.EscapeString(packet.GetString())));
        }

        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WHO [P:'{2}' G:'{3}' L:{4}-{5} C:{6:X} R:{7:X}]", client.IP, client.Port, namePlayer, nameGuild, levelMinimum, levelMaximum, maskClass, maskRace);

        // TODO: Don't show GMs?
        List<ulong> results = new();
        _clusterServiceLocator.WorldCluster.CharacteRsLock.AcquireReaderLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
        foreach (var objCharacter in _clusterServiceLocator.WorldCluster.CharacteRs)
        {
            if (!objCharacter.Value.IsInWorld)
            {
                continue;
            }

            if (_clusterServiceLocator.Functions.GetCharacterSide((byte)objCharacter.Value.Race) != _clusterServiceLocator.Functions.GetCharacterSide((byte)client.Character.Race) && client.Character.Access < AccessLevel.GameMaster)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(namePlayer) && !_clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(objCharacter.Value.Name).Contains(_clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(namePlayer)))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(nameGuild) && (objCharacter.Value.Guild is null || !_clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(objCharacter.Value.Guild.Name).Contains(_clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(nameGuild))))
            {
                continue;
            }

            if (objCharacter.Value.Level < levelMinimum)
            {
                continue;
            }

            if (objCharacter.Value.Level > levelMaximum)
            {
                continue;
            }

            if (zonesCount > 0L && zones.Contains(objCharacter.Value.Zone) == false)
            {
                continue;
            }

            if (stringsCount > 0L)
            {
                var passedStrings = true;
                foreach (var stringValue in strings)
                {
                    if (_clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(objCharacter.Value.Name).IndexOf(stringValue, StringComparison.Ordinal) != -1)
                    {
                        continue;
                    }

                    if (_clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(_clusterServiceLocator.Functions.GetRaceName((int)objCharacter.Value.Race)) == stringValue)
                    {
                        continue;
                    }

                    if (_clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(_clusterServiceLocator.Functions.GetClassName((int)objCharacter.Value.Classe)) == stringValue)
                    {
                        continue;
                    }

                    if (objCharacter.Value.Guild is not null && _clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(objCharacter.Value.Guild.Name).IndexOf(stringValue, StringComparison.Ordinal) != -1)
                    {
                        continue;
                    }
                    // TODO: Look for zone name
                    passedStrings = false;
                    break;
                }

                if (passedStrings == false)
                {
                    continue;
                }
            }

            // DONE: List first 49 _WorldCluster.CHARACTERs (like original)
            if (results.Count > 49)
            {
                break;
            }

            results.Add(objCharacter.Value.Guid);
        }

        PacketClass response = new(Opcodes.SMSG_WHO);
        response.AddInt32(results.Count);
        response.AddInt32(results.Count);
        foreach (var guid in results)
        {
            response.AddString(_clusterServiceLocator.WorldCluster.CharacteRs[guid].Name);           // Name
            if (_clusterServiceLocator.WorldCluster.CharacteRs[guid].Guild is not null)
            {
                response.AddString(_clusterServiceLocator.WorldCluster.CharacteRs[guid].Guild.Name); // Guild Name
            }
            else
            {
                response.AddString("");
            }                          // Guild Name

            response.AddInt32(_clusterServiceLocator.WorldCluster.CharacteRs[guid].Level);           // Level
            response.AddInt32((int)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Classe);          // Class
            response.AddInt32((int)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Race);            // Race
            response.AddInt32((int)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Zone);            // Zone ID
        }

        _clusterServiceLocator.WorldCluster.CharacteRsLock.ReleaseReaderLock();
        client.Send(response);
        response.Dispose();
    }

    public void On_CMSG_ADD_FRIEND(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        PacketClass response = new(Opcodes.SMSG_FRIEND_STATUS);
        var name = packet.GetString();
        var guid = 0UL;
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ADD_FRIEND [{2}]", client.IP, client.Port, name);

        // DONE: Get GUID from DB
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT char_guid, char_race FROM characters WHERE char_name = \"{0}\";", name), ref q);
        if (q.Rows.Count > 0)
        {
            guid = (ulong)q.Rows[0].As<long>("char_guid");
            var friendSide = _clusterServiceLocator.Functions.GetCharacterSide(q.Rows[0].As<byte>("char_race"));
            q.Clear();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT flags FROM character_social WHERE flags = {0}", Conversions.ToByte(SocialFlag.SOCIAL_FLAG_FRIEND)), ref q);
            var numberOfFriends = q.Rows.Count;
            q.Clear();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT flags FROM character_social WHERE guid = {0} AND friend = {1} AND flags = {2};", client.Character.Guid, guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_FRIEND)), ref q);
            if (guid == client.Character.Guid)
            {
                response.AddInt8((byte)FriendResult.FRIEND_SELF);
                response.AddUInt64(guid);
            }
            else if (q.Rows.Count > 0)
            {
                response.AddInt8((byte)FriendResult.FRIEND_ALREADY);
                response.AddUInt64(guid);
            }
            else if (numberOfFriends >= (int)SocialList.MAX_FRIENDS_ON_LIST)
            {
                response.AddInt8((byte)FriendResult.FRIEND_LIST_FULL);
                response.AddUInt64(guid);
            }
            else if (_clusterServiceLocator.Functions.GetCharacterSide((byte)client.Character.Race) != friendSide)
            {
                response.AddInt8((byte)FriendResult.FRIEND_ENEMY);
                response.AddUInt64(guid);
            }
            else if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(guid))
            {
                response.AddInt8((byte)FriendResult.FRIEND_ADDED_ONLINE);
                response.AddUInt64(guid);
                response.AddString(name);
                if (_clusterServiceLocator.WorldCluster.CharacteRs[guid].Dnd)
                {
                    response.AddInt8((byte)FriendStatus.FRIEND_STATUS_DND);
                }
                else if (_clusterServiceLocator.WorldCluster.CharacteRs[guid].Afk)
                {
                    response.AddInt8((byte)FriendStatus.FRIEND_STATUS_AFK);
                }
                else
                {
                    response.AddInt8((byte)FriendStatus.FRIEND_STATUS_ONLINE);
                }

                response.AddInt32((int)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Zone);
                response.AddInt32(_clusterServiceLocator.WorldCluster.CharacteRs[guid].Level);
                response.AddInt32((int)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Classe);
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("INSERT INTO character_social (guid, friend, flags) VALUES ({0}, {1}, {2});", client.Character.Guid, guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_FRIEND)));
            }
            else
            {
                response.AddInt8((byte)FriendResult.FRIEND_ADDED_OFFLINE);
                response.AddUInt64(guid);
                response.AddString(name);
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("INSERT INTO character_social (guid, friend, flags) VALUES ({0}, {1}, {2});", client.Character.Guid, guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_FRIEND)));
            }
        }
        else
        {
            response.AddInt8((byte)FriendResult.FRIEND_NOT_FOUND);
            response.AddUInt64(guid);
        }

        client.Send(response);
        response.Dispose();
        q.Dispose();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", client.IP, client.Port);
    }

    public void On_CMSG_ADD_IGNORE(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 6)
        {
            return;
        }

        packet.GetInt16();
        PacketClass response = new(Opcodes.SMSG_FRIEND_STATUS);
        var name = packet.GetString();
        var guid = 0UL;
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ADD_IGNORE [{2}]", client.IP, client.Port, name);

        // DONE: Get GUID from DB
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT char_guid FROM characters WHERE char_name = \"{0}\";", name), ref q);
        if (q.Rows.Count > 0)
        {
            guid = (ulong)q.Rows[0].As<long>("char_guid");
            q.Clear();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT flags FROM character_social WHERE flags = {0}", Conversions.ToByte(SocialFlag.SOCIAL_FLAG_IGNORED)), ref q);
            var numberOfFriends = q.Rows.Count;
            q.Clear();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT * FROM character_social WHERE guid = {0} AND friend = {1} AND flags = {2};", client.Character.Guid, guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_IGNORED)), ref q);
            if (guid == client.Character.Guid)
            {
                response.AddInt8((byte)FriendResult.FRIEND_IGNORE_SELF);
                response.AddUInt64(guid);
            }
            else if (q.Rows.Count > 0)
            {
                response.AddInt8((byte)FriendResult.FRIEND_IGNORE_ALREADY);
                response.AddUInt64(guid);
            }
            else if (numberOfFriends >= (int)SocialList.MAX_IGNORES_ON_LIST)
            {
                response.AddInt8((byte)FriendResult.FRIEND_IGNORE_ALREADY);
                response.AddUInt64(guid);
            }
            else
            {
                response.AddInt8((byte)FriendResult.FRIEND_IGNORE_ADDED);
                response.AddUInt64(guid);
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("INSERT INTO character_social (guid, friend, flags) VALUES ({0}, {1}, {2});", client.Character.Guid, guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_IGNORED)));
                client.Character.IgnoreList.Add(guid);
            }
        }
        else
        {
            response.AddInt8((byte)FriendResult.FRIEND_IGNORE_NOT_FOUND);
            response.AddUInt64(guid);
        }

        client.Send(response);
        response.Dispose();
        q.Dispose();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", client.IP, client.Port);
    }

    public void On_CMSG_DEL_FRIEND(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DEL_FRIEND", client.IP, client.Port);
        if (packet.Data.Length - 1 < 13)
        {
            return;
        }

        packet.GetInt16();
        PacketClass response = new(Opcodes.SMSG_FRIEND_STATUS);
        var guid = packet.GetUInt64();
        try
        {
            DataTable q = new();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT flags FROM character_social WHERE guid = {0} AND friend = {1};", client.Character.Guid, guid), ref q);
            if (q.Rows.Count > 0)
            {
                var flags = q.Rows[0].As<int>("flags");
                var newFlags = (SocialFlag)flags ^ SocialFlag.SOCIAL_FLAG_FRIEND;
                if ((newFlags & (SocialFlag.SOCIAL_FLAG_FRIEND | SocialFlag.SOCIAL_FLAG_IGNORED)) == 0)
                {
                    _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM character_social WHERE friend = {1} AND guid = {0};", client.Character.Guid, guid));
                }
                else
                {
                    _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE character_social SET flags = {2} WHERE friend = {1} AND guid = {0};", client.Character.Guid, guid, newFlags));
                }

                response.AddInt8((byte)FriendResult.FRIEND_REMOVED);
            }
            else
            {
                response.AddInt8((byte)FriendResult.FRIEND_NOT_FOUND);
            }
        }
        catch
        {
            response.AddInt8((byte)FriendResult.FRIEND_DB_ERROR);
        }

        response.AddUInt64(guid);
        client.Send(response);
        response.Dispose();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", client.IP, client.Port);
    }

    public void On_CMSG_DEL_IGNORE(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DEL_IGNORE", client.IP, client.Port);
        if (packet.Data.Length - 1 < 13)
        {
            return;
        }

        packet.GetInt16();
        PacketClass response = new(Opcodes.SMSG_FRIEND_STATUS);
        var guid = packet.GetUInt64();
        try
        {
            DataTable q = new();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT flags FROM character_social WHERE guid = {0} AND friend = {1};", client.Character.Guid, guid), ref q);
            if (q.Rows.Count > 0)
            {
                var flags = q.Rows[0].As<int>("flags");
                var newFlags = (SocialFlag)flags ^ SocialFlag.SOCIAL_FLAG_IGNORED;
                if ((newFlags & (SocialFlag.SOCIAL_FLAG_FRIEND | SocialFlag.SOCIAL_FLAG_IGNORED)) == 0)
                {
                    _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM character_social WHERE friend = {1} AND guid = {0};", client.Character.Guid, guid));
                }
                else
                {
                    _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE character_social SET flags = {2} WHERE friend = {1} AND guid = {0};", client.Character.Guid, guid, newFlags));
                }

                response.AddInt8((byte)FriendResult.FRIEND_IGNORE_REMOVED);
            }
            else
            {
                response.AddInt8((byte)FriendResult.FRIEND_IGNORE_NOT_FOUND);
            }
        }
        catch
        {
            response.AddInt8((byte)FriendResult.FRIEND_DB_ERROR);
        }

        response.AddUInt64(guid);
        client.Send(response);
        response.Dispose();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", client.IP, client.Port);
    }

    public void On_CMSG_FRIEND_LIST(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_FRIEND_LIST", client.IP, client.Port);
        SendFriendList(client, client.Character);
        SendIgnoreList(client, client.Character);
    }
}
