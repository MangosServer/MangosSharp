// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System;
using System.Collections.Generic;
using System.Data;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Server;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Social;
using Mangos.Common.Globals;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Cluster.Handlers
{
    public class WC_Handlers_Social
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void LoadIgnoreList(ref WcHandlerCharacter.CharacterObject objCharacter)
        {
            // DONE: Query DB
            var q = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT * FROM character_social WHERE guid = {0} AND flags = {1};", objCharacter.Guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_IGNORED)), q);

            // DONE: Add to list
            foreach (DataRow r in q.Rows)
                objCharacter.IgnoreList.Add(Conversions.ToULong(r["friend"]));
        }

        public void SendFriendList(ref WC_Network.ClientClass client, ref WcHandlerCharacter.CharacterObject character)
        {
            // DONE: Query DB
            var q = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT * FROM character_social WHERE guid = {0} AND (flags & {1}) > 0;", character.Guid, Conversions.ToInteger(SocialFlag.SOCIAL_FLAG_FRIEND)), q);

            // DONE: Make the packet
            var smsgFriendList = new Packets.PacketClass(OPCODES.SMSG_FRIEND_LIST);
            if (q.Rows.Count > 0)
            {
                smsgFriendList.AddInt8((byte)q.Rows.Count);
                foreach (DataRow r in q.Rows)
                {
                    ulong guid = Conversions.ToULong(r["friend"]);
                    smsgFriendList.AddUInt64(guid);                    // Player GUID
                    if (ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(guid) && ClusterServiceLocator._WorldCluster.CHARACTERs[guid].IsInWorld)
                    {
                        if (ClusterServiceLocator._WorldCluster.CHARACTERs[guid].DND)
                        {
                            smsgFriendList.AddInt8(FriendStatus.FRIEND_STATUS_DND);
                        }
                        else if (ClusterServiceLocator._WorldCluster.CHARACTERs[guid].AFK)
                        {
                            smsgFriendList.AddInt8(FriendStatus.FRIEND_STATUS_AFK);
                        }
                        else
                        {
                            smsgFriendList.AddInt8(FriendStatus.FRIEND_STATUS_ONLINE);
                        }

                        smsgFriendList.AddInt32((int)ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Zone);    // Area
                        smsgFriendList.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Level);   // Level
                        smsgFriendList.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Classe);  // Class
                    }
                    else
                    {
                        smsgFriendList.AddInt8(FriendStatus.FRIEND_STATUS_OFFLINE);
                    }
                }
            }
            else
            {
                smsgFriendList.AddInt8(0);
            }

            client.Send(ref smsgFriendList);
            smsgFriendList.Dispose();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_LIST", client.IP, client.Port);
        }

        public void SendIgnoreList(ref WC_Network.ClientClass client, ref WcHandlerCharacter.CharacterObject character)
        {
            // DONE: Query DB
            var q = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT * FROM character_social WHERE guid = {0} AND (flags & {1}) > 0;", character.Guid, Conversions.ToInteger(SocialFlag.SOCIAL_FLAG_IGNORED)), q);

            // DONE: Make the packet
            var smsgIgnoreList = new Packets.PacketClass(OPCODES.SMSG_IGNORE_LIST);
            if (q.Rows.Count > 0)
            {
                smsgIgnoreList.AddInt8((byte)q.Rows.Count);
                foreach (DataRow r in q.Rows)
                    smsgIgnoreList.AddUInt64(Conversions.ToULong(r["friend"]));                    // Player GUID
            }
            else
            {
                smsgIgnoreList.AddInt8(0);
            }

            client.Send(ref smsgIgnoreList);
            smsgIgnoreList.Dispose();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_IGNORE_LIST", client.IP, client.Port);
        }

        public void NotifyFriendStatus(ref WcHandlerCharacter.CharacterObject objCharacter, FriendStatus s)
        {
            var q = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT guid FROM character_social WHERE friend = {0} AND (flags & {1}) > 0;", objCharacter.Guid, Conversions.ToInteger(SocialFlag.SOCIAL_FLAG_FRIEND)), q);

            // DONE: Send "Friend offline/online"
            var friendpacket = new Packets.PacketClass(OPCODES.SMSG_FRIEND_STATUS);
            friendpacket.AddInt8(s);
            friendpacket.AddUInt64(objCharacter.Guid);
            foreach (DataRow r in q.Rows)
            {
                ulong guid = Conversions.ToULong(r["guid"]);
                if (ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(guid) && ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Client is object)
                {
                    ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Client.SendMultiplyPackets(ref friendpacket);
                }
            }

            friendpacket.Dispose();
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void On_CMSG_WHO(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            uint levelMinimum = packet.GetUInt32();       // 0
            uint levelMaximum = packet.GetUInt32();       // 100
            string namePlayer = ClusterServiceLocator._Functions.EscapeString(packet.GetString());
            string nameGuild = ClusterServiceLocator._Functions.EscapeString(packet.GetString());
            uint maskRace = packet.GetUInt32();
            uint maskClass = packet.GetUInt32();
            uint zonesCount = packet.GetUInt32();         // Limited to 10
            if (zonesCount > 10L)
                return;
            var zones = new List<uint>();
            for (int i = 1, loopTo = (int)zonesCount; i <= loopTo; i++)
                zones.Add(packet.GetUInt32());
            uint stringsCount = packet.GetUInt32();         // Limited to 4
            if (stringsCount > 4L)
                return;
            var strings = new List<string>();
            for (int i = 1, loopTo1 = (int)stringsCount; i <= loopTo1; i++)
                strings.Add(ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(ClusterServiceLocator._Functions.EscapeString(packet.GetString())));
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WHO [P:'{2}' G:'{3}' L:{4}-{5} C:{6:X} R:{7:X}]", client.IP, client.Port, namePlayer, nameGuild, levelMinimum, levelMaximum, maskClass, maskRace);

            // TODO: Don't show GMs?
            var results = new List<ulong>();
            ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.AcquireReaderLock(ClusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
            foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> objCharacter in ClusterServiceLocator._WorldCluster.CHARACTERs)
            {
                if (!objCharacter.Value.IsInWorld)
                    continue;
                if (ClusterServiceLocator._Functions.GetCharacterSide(objCharacter.Value.Race) != ClusterServiceLocator._Functions.GetCharacterSide(client.Character.Race) && client.Character.Access < AccessLevel.GameMaster)
                    continue;
                if (!string.IsNullOrEmpty(namePlayer) && ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(objCharacter.Value.Name).IndexOf(ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(namePlayer), StringComparison.Ordinal) == -1)
                    continue;
                if (!string.IsNullOrEmpty(nameGuild) && (objCharacter.Value.Guild is null || ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(objCharacter.Value.Guild.Name).IndexOf(ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(nameGuild), StringComparison.Ordinal) == -1))
                    continue;
                if (objCharacter.Value.Level < levelMinimum)
                    continue;
                if (objCharacter.Value.Level > levelMaximum)
                    continue;
                if (zonesCount > 0L && zones.Contains(objCharacter.Value.Zone) == false)
                    continue;
                if (stringsCount > 0L)
                {
                    bool passedStrings = true;
                    foreach (string stringValue in strings)
                    {
                        if (ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(objCharacter.Value.Name).IndexOf(stringValue, StringComparison.Ordinal) != -1)
                            continue;
                        if (ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(ClusterServiceLocator._Functions.GetRaceName(ref objCharacter.Value.Race)) == stringValue)
                            continue;
                        if (ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(ClusterServiceLocator._Functions.GetClassName(ref objCharacter.Value.Classe)) == stringValue)
                            continue;
                        if (objCharacter.Value.Guild is object && ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(objCharacter.Value.Guild.Name).IndexOf(stringValue, StringComparison.Ordinal) != -1)
                            continue;
                        // TODO: Look for zone name
                        passedStrings = false;
                        break;
                    }

                    if (passedStrings == false)
                        continue;
                }

                // DONE: List first 49 _WorldCluster.CHARACTERs (like original)
                if (results.Count > 49)
                    break;
                results.Add(objCharacter.Value.Guid);
            }

            var response = new Packets.PacketClass(OPCODES.SMSG_WHO);
            response.AddInt32(results.Count);
            response.AddInt32(results.Count);
            foreach (ulong guid in results)
            {
                response.AddString(ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Name);           // Name
                if (ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Guild is object)
                {
                    response.AddString(ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Guild.Name); // Guild Name
                }
                else
                {
                    response.AddString("");
                }                          // Guild Name

                response.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Level);           // Level
                response.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Classe);          // Class
                response.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Race);            // Race
                response.AddInt32((int)ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Zone);            // Zone ID
            }

            ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.ReleaseReaderLock();
            client.Send(ref response);
            response.Dispose();
        }

        public void On_CMSG_ADD_FRIEND(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            var response = new Packets.PacketClass(OPCODES.SMSG_FRIEND_STATUS);
            string name = packet.GetString();
            ulong guid = 0UL;
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ADD_FRIEND [{2}]", client.IP, client.Port, name);

            // DONE: Get GUID from DB
            var q = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT char_guid, char_race FROM characters WHERE char_name = \"{0}\";", name), q);
            if (q.Rows.Count > 0)
            {
                guid = (ulong)Conversions.ToLong(q.Rows[0]["char_guid"]);
                bool FriendSide = ClusterServiceLocator._Functions.GetCharacterSide(Conversions.ToByte(q.Rows[0]["char_race"]));
                q.Clear();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT flags FROM character_social WHERE flags = {0}", Conversions.ToByte(SocialFlag.SOCIAL_FLAG_FRIEND)), q);
                int NumberOfFriends = q.Rows.Count;
                q.Clear();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT flags FROM character_social WHERE guid = {0} AND friend = {1} AND flags = {2};", client.Character.Guid, guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_FRIEND)), q);
                if (guid == client.Character.Guid)
                {
                    response.AddInt8(FriendResult.FRIEND_SELF);
                    response.AddUInt64(guid);
                }
                else if (q.Rows.Count > 0)
                {
                    response.AddInt8(FriendResult.FRIEND_ALREADY);
                    response.AddUInt64(guid);
                }
                else if (NumberOfFriends >= SocialList.MAX_FRIENDS_ON_LIST)
                {
                    response.AddInt8(FriendResult.FRIEND_LIST_FULL);
                    response.AddUInt64(guid);
                }
                else if (ClusterServiceLocator._Functions.GetCharacterSide(client.Character.Race) != FriendSide)
                {
                    response.AddInt8(FriendResult.FRIEND_ENEMY);
                    response.AddUInt64(guid);
                }
                else if (ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(guid))
                {
                    response.AddInt8(FriendResult.FRIEND_ADDED_ONLINE);
                    response.AddUInt64(guid);
                    response.AddString(name);
                    if (ClusterServiceLocator._WorldCluster.CHARACTERs[guid].DND)
                    {
                        response.AddInt8(FriendStatus.FRIEND_STATUS_DND);
                    }
                    else if (ClusterServiceLocator._WorldCluster.CHARACTERs[guid].AFK)
                    {
                        response.AddInt8(FriendStatus.FRIEND_STATUS_AFK);
                    }
                    else
                    {
                        response.AddInt8(FriendStatus.FRIEND_STATUS_ONLINE);
                    }

                    response.AddInt32((int)ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Zone);
                    response.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Level);
                    response.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Classe);
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("INSERT INTO character_social (guid, friend, flags) VALUES ({0}, {1}, {2});", client.Character.Guid, guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_FRIEND)));
                }
                else
                {
                    response.AddInt8(FriendResult.FRIEND_ADDED_OFFLINE);
                    response.AddUInt64(guid);
                    response.AddString(name);
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("INSERT INTO character_social (guid, friend, flags) VALUES ({0}, {1}, {2});", client.Character.Guid, guid, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_FRIEND)));
                }
            }
            else
            {
                response.AddInt8(FriendResult.FRIEND_NOT_FOUND);
                response.AddUInt64(guid);
            }

            client.Send(ref response);
            response.Dispose();
            q.Dispose();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", client.IP, client.Port);
        }

        public void On_CMSG_ADD_IGNORE(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            var response = new Packets.PacketClass(OPCODES.SMSG_FRIEND_STATUS);
            string name = packet.GetString();
            ulong GUID = 0UL;
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ADD_IGNORE [{2}]", client.IP, client.Port, name);

            // DONE: Get GUID from DB
            var q = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT char_guid FROM characters WHERE char_name = \"{0}\";", name), q);
            if (q.Rows.Count > 0)
            {
                GUID = (ulong)Conversions.ToLong(q.Rows[0]["char_guid"]);
                q.Clear();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT flags FROM character_social WHERE flags = {0}", Conversions.ToByte(SocialFlag.SOCIAL_FLAG_IGNORED)), q);
                int NumberOfFriends = q.Rows.Count;
                q.Clear();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT * FROM character_social WHERE guid = {0} AND friend = {1} AND flags = {2};", client.Character.Guid, GUID, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_IGNORED)), q);
                if (GUID == client.Character.Guid)
                {
                    response.AddInt8(FriendResult.FRIEND_IGNORE_SELF);
                    response.AddUInt64(GUID);
                }
                else if (q.Rows.Count > 0)
                {
                    response.AddInt8(FriendResult.FRIEND_IGNORE_ALREADY);
                    response.AddUInt64(GUID);
                }
                else if (NumberOfFriends >= SocialList.MAX_IGNORES_ON_LIST)
                {
                    response.AddInt8(FriendResult.FRIEND_IGNORE_ALREADY);
                    response.AddUInt64(GUID);
                }
                else
                {
                    response.AddInt8(FriendResult.FRIEND_IGNORE_ADDED);
                    response.AddUInt64(GUID);
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("INSERT INTO character_social (guid, friend, flags) VALUES ({0}, {1}, {2});", client.Character.Guid, GUID, Conversions.ToByte(SocialFlag.SOCIAL_FLAG_IGNORED)));
                    client.Character.IgnoreList.Add(GUID);
                }
            }
            else
            {
                response.AddInt8(FriendResult.FRIEND_IGNORE_NOT_FOUND);
                response.AddUInt64(GUID);
            }

            client.Send(ref response);
            response.Dispose();
            q.Dispose();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", client.IP, client.Port);
        }

        public void On_CMSG_DEL_FRIEND(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DEL_FRIEND", client.IP, client.Port);
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            var response = new Packets.PacketClass(OPCODES.SMSG_FRIEND_STATUS);
            ulong GUID = packet.GetUInt64();
            try
            {
                var q = new DataTable();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT flags FROM character_social WHERE guid = {0} AND friend = {1};", (object)client.Character.Guid, (object)GUID), q);
                if (q.Rows.Count > 0)
                {
                    int flags = Conversions.ToInteger(q.Rows[0]["flags"]);
                    int newFlags = flags & !SocialFlag.SOCIAL_FLAG_FRIEND;
                    if ((newFlags & (SocialFlag.SOCIAL_FLAG_FRIEND | SocialFlag.SOCIAL_FLAG_IGNORED)) == 0)
                    {
                        ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM character_social WHERE friend = {1} AND guid = {0};", client.Character.Guid, GUID));
                    }
                    else
                    {
                        ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("UPDATE character_social SET flags = {2} WHERE friend = {1} AND guid = {0};", client.Character.Guid, GUID, newFlags));
                    }

                    response.AddInt8(FriendResult.FRIEND_REMOVED);
                }
                else
                {
                    response.AddInt8(FriendResult.FRIEND_NOT_FOUND);
                }
            }
            catch
            {
                response.AddInt8(FriendResult.FRIEND_DB_ERROR);
            }

            response.AddUInt64(GUID);
            client.Send(ref response);
            response.Dispose();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", client.IP, client.Port);
        }

        public void On_CMSG_DEL_IGNORE(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DEL_IGNORE", client.IP, client.Port);
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            var response = new Packets.PacketClass(OPCODES.SMSG_FRIEND_STATUS);
            ulong GUID = packet.GetUInt64();
            try
            {
                var q = new DataTable();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT flags FROM character_social WHERE guid = {0} AND friend = {1};", (object)client.Character.Guid, (object)GUID), q);
                if (q.Rows.Count > 0)
                {
                    int flags = Conversions.ToInteger(q.Rows[0]["flags"]);
                    int newFlags = flags & !SocialFlag.SOCIAL_FLAG_IGNORED;
                    if ((newFlags & (SocialFlag.SOCIAL_FLAG_FRIEND | SocialFlag.SOCIAL_FLAG_IGNORED)) == 0)
                    {
                        ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM character_social WHERE friend = {1} AND guid = {0};", client.Character.Guid, GUID));
                    }
                    else
                    {
                        ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("UPDATE character_social SET flags = {2} WHERE friend = {1} AND guid = {0};", client.Character.Guid, GUID, newFlags));
                    }

                    response.AddInt8(FriendResult.FRIEND_IGNORE_REMOVED);
                }
                else
                {
                    response.AddInt8(FriendResult.FRIEND_IGNORE_NOT_FOUND);
                }
            }
            catch
            {
                response.AddInt8(FriendResult.FRIEND_DB_ERROR);
            }

            response.AddUInt64(GUID);
            client.Send(ref response);
            response.Dispose();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", client.IP, client.Port);
        }

        public void On_CMSG_FRIEND_LIST(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_FRIEND_LIST", client.IP, client.Port);
            SendFriendList(ref client, ref client.Character);
            SendIgnoreList(ref client, ref client.Character);
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}