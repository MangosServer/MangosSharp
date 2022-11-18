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
using Mangos.Cluster.Handlers.Guild;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Guild;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Social;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Mangos.Cluster.Handlers;

public class WcHandlerCharacter
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlerCharacter(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public class CharacterObject : IDisposable
    {
        private readonly ClusterServiceLocator _clusterServiceLocator;

        public CharacterObject(ulong g, ClientClass objCharacter, ClusterServiceLocator clusterServiceLocator)
        {
            _clusterServiceLocator = clusterServiceLocator;
            ChatFlag = ChatFlag.FLAGS_NONE;
            Guid = g;
            Client = objCharacter;
            ReLoad();
            Access = Client.Access;
            var argobjCharacter = this;
            _clusterServiceLocator.WcHandlersSocial.LoadIgnoreList(argobjCharacter);
            _clusterServiceLocator.WorldCluster.CharacteRsLock.AcquireWriterLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
            _clusterServiceLocator.WorldCluster.CharacteRs.Add(Guid, this);
            _clusterServiceLocator.WorldCluster.CharacteRsLock.ReleaseWriterLock();
        }

        public ulong Guid;
        public ClientClass Client;
        public bool IsInWorld;
        public uint Map;
        public uint Zone;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float PositionO;
        public AccessLevel Access;
        public string Name;
        public int Level;
        public Races Race;
        public Classes Classe;
        public byte Gender;
        public DateTime Time = DateAndTime.Now;
        public int Latency;
        public List<ulong> IgnoreList = new();
        public List<string> JoinedChannels = new();
        public bool Afk;
        public bool Dnd;
        public string AfkMessage;
        public uint GuildInvited;
        public ulong GuildInvitedBy;
        public WcGuild.Guild Guild;
        public byte GuildRank;
        public WcHandlersGroup.Group Group;
        public bool GroupAssistant;
        public bool GroupInvitedFlag;

        public bool IsInGroup => Group is not null && GroupInvitedFlag == false;

        public bool IsGroupLeader => Group is not null && ReferenceEquals(Group.Members[Group.Leader], this);

        public bool IsInRaid => Group is not null && Group.Type == GroupType.RAID;

        public bool IsInGuild => Guild is not null;

        public bool IsGuildLeader => Guild is not null && Guild.Leader == Guid;

        public bool IsGuildRightSet(GuildRankRights rights)
        {
            return Guild is not null && (Guild.RankRights[GuildRank] & (uint)rights) == (uint)rights;
        }

        public bool Side
        {
            get
            {
                switch (Race)
                {
                    case var @case when @case == Races.RACE_DWARF:
                    case var case1 when case1 == Races.RACE_GNOME:
                    case var case2 when case2 == Races.RACE_HUMAN:
                    case var case3 when case3 == Races.RACE_NIGHT_ELF:
                        {
                            return false;
                        }

                    default:
                        {
                            return true;
                        }
                }
            }
        }

        public IWorld GetWorld => _clusterServiceLocator.WcNetwork.WorldServer.Worlds[Map];

        public void ReLoad()
        {
            // DONE: Get character info from DB
            DataTable mySqlQuery = new();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT * FROM characters WHERE char_guid = {0};", Guid), ref mySqlQuery);
            if (mySqlQuery.Rows.Count > 0)
            {
                Race = (Races)mySqlQuery.Rows[0].As<byte>("char_race");
                Classe = (Classes)mySqlQuery.Rows[0].As<byte>("char_class");
                Gender = mySqlQuery.Rows[0].As<byte>("char_gender");
                Name = mySqlQuery.Rows[0].As<string>("char_name");
                Level = mySqlQuery.Rows[0].As<byte>("char_level");
                Zone = mySqlQuery.Rows[0].As<uint>("char_zone_id");
                Map = mySqlQuery.Rows[0].As<uint>("char_map_id");
                PositionX = mySqlQuery.Rows[0].As<float>("char_positionX");
                PositionY = mySqlQuery.Rows[0].As<float>("char_positionY");
                PositionZ = mySqlQuery.Rows[0].As<float>("char_positionZ");

                // DONE: Get guild info
                var guildId = mySqlQuery.Rows[0].As<uint>("char_guildId");
                if (guildId > 0L)
                {
                    if (_clusterServiceLocator.WcGuild.GuilDs.ContainsKey(guildId) == false)
                    {
                        WcGuild.Guild tmpGuild = new(guildId);
                        Guild = tmpGuild;
                    }
                    else
                    {
                        Guild = _clusterServiceLocator.WcGuild.GuilDs[guildId];
                    }

                    GuildRank = mySqlQuery.Rows[0].As<byte>("char_guildRank");
                }
            }
            else
            {
                _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DATABASE, "Failed to load expected results from:");
                _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DATABASE, string.Format("SELECT * FROM characters WHERE char_guid = {0};", Guid));
            }
        }

        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                Client = null;

                // DONE: Update character status in database
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_online = 0, char_logouttime = '{1}' WHERE char_guid = '{0}';", Guid, _clusterServiceLocator.Functions.GetTimestamp(DateAndTime.Now)));

                // NOTE: Don't leave group on normal disconnect, only on logout
                if (IsInGroup)
                {
                    // DONE: Tell the group the member is offline
                    var response = _clusterServiceLocator.Functions.BuildPartyMemberStatsOffline(Guid);
                    Group.Broadcast(response);
                    response.Dispose();

                    // DONE: Set new leader and loot master
                    Group.NewLeader(this);
                    Group.SendGroupList();
                }

                // DONE: Notify friends for logout
                var argobjCharacter = this;
                _clusterServiceLocator.WcHandlersSocial.NotifyFriendStatus(argobjCharacter, (FriendStatus)FriendResult.FRIEND_OFFLINE);

                // DONE: Notify guild for logout
                if (IsInGuild)
                {
                    var argobjCharacter1 = this;
                    _clusterServiceLocator.WcGuild.NotifyGuildStatus(argobjCharacter1, GuildEvent.SIGNED_OFF);
                }

                // DONE: Leave chat
                while (JoinedChannels.Count > 0)
                {
                    if (_clusterServiceLocator.WsHandlerChannels.ChatChanneLs.ContainsKey(JoinedChannels[0]))
                    {
                        var argCharacter = this;
                        _clusterServiceLocator.WsHandlerChannels.ChatChanneLs[JoinedChannels[0]].Part(argCharacter);
                    }
                    else
                    {
                        JoinedChannels.RemoveAt(0);
                    }
                }

                _clusterServiceLocator.WorldCluster.CharacteRsLock.AcquireWriterLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
                _clusterServiceLocator.WorldCluster.CharacteRs.Remove(Guid);
                _clusterServiceLocator.WorldCluster.CharacteRsLock.ReleaseWriterLock();
            }

            _disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Transfer(float posX, float posY, float posZ, float ori, int thisMap)
        {
            PacketClass p = new(Opcodes.SMSG_TRANSFER_PENDING);
            p.AddInt32(thisMap);
            Client.Send(p);
            p.Dispose();

            // Actions Here
            IsInWorld = false;
            GetWorld.ClientDisconnect(Client.Index);
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_positionX = {0}, char_positionY = {1}, char_positionZ = {2}, char_orientation = {3}, char_map_id = {4} WHERE char_guid = {5};", Strings.Trim(Conversion.Str(posX)), Strings.Trim(Conversion.Str(posY)), Strings.Trim(Conversion.Str(posZ)), Strings.Trim(Conversion.Str(ori)), thisMap, Guid));

            // Do global transfer
            _clusterServiceLocator.WcNetwork.WorldServer.ClientTransfer(Client.Index, posX, posY, posZ, ori, (uint)thisMap);
        }

        public void Transfer(float posX, float posY, float posZ, float ori)
        {
            PacketClass p = new(Opcodes.SMSG_TRANSFER_PENDING);
            p.AddInt32((int)Map);
            Client.Send(p);
            p.Dispose();

            // Actions Here
            IsInWorld = false;
            GetWorld.ClientDisconnect(Client.Index);
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_positionX = {0}, char_positionY = {1}, char_positionZ = {2}, char_orientation = {3}, char_map_id = {4} WHERE char_guid = {5};", Strings.Trim(Conversion.Str(posX)), Strings.Trim(Conversion.Str(posY)), Strings.Trim(Conversion.Str(posZ)), Strings.Trim(Conversion.Str(ori)), Map, Guid));

            // Do global transfer
            _clusterServiceLocator.WcNetwork.WorldServer.ClientTransfer(Client.Index, posX, posY, posZ, ori, Map);
        }

        // Login
        public void OnLogin()
        {
            // DONE: Update character status in database
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update("UPDATE characters SET char_online = 1 WHERE char_guid = " + Guid + ";");

            // DONE: SMSG_ACCOUNT_DATA_MD5
            var argcharacter = this;
            _clusterServiceLocator.Functions.SendAccountMd5(Client, argcharacter);

            // DONE: SMSG_TRIGGER_CINEMATIC
            DataTable q = new();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT char_moviePlayed FROM characters WHERE char_guid = {0} AND char_moviePlayed = 0;", Guid), ref q);
            if (q.Rows.Count > 0)
            {
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update("UPDATE characters SET char_moviePlayed = 1 WHERE char_guid = " + Guid + ";");
                var argcharacter1 = this;
                _clusterServiceLocator.Functions.SendTriggerCinematic(Client, argcharacter1);
            }

            // DONE: SMSG_LOGIN_SETTIMESPEED
            var argcharacter2 = this;
            _clusterServiceLocator.Functions.SendGameTime(Client, argcharacter2);

            // DONE: Server Message Of The Day
            _clusterServiceLocator.Functions.SendMessageMotd(Client, "Welcome to World of Warcraft.");
            _clusterServiceLocator.Functions.SendMessageMotd(Client, string.Format("This server is using {0} v.{1}", _clusterServiceLocator.Functions.SetColor("[MangosSharp, written in C# .NET 5.0]", 4, 147, 11), Assembly.GetExecutingAssembly().GetName().Version));

            // DONE: Guild Message Of The Day
            var argobjCharacter = this;
            _clusterServiceLocator.WcGuild.SendGuildMotd(argobjCharacter);

            // DONE: Social lists
            var argcharacter3 = this;
            _clusterServiceLocator.WcHandlersSocial.SendFriendList(Client, argcharacter3);
            var argcharacter4 = this;
            _clusterServiceLocator.WcHandlersSocial.SendIgnoreList(Client, argcharacter4);

            // DONE: Send "Friend online"
            var argobjCharacter1 = this;
            _clusterServiceLocator.WcHandlersSocial.NotifyFriendStatus(argobjCharacter1, (FriendStatus)FriendResult.FRIEND_ONLINE);

            // DONE: Send online notify for guild
            var argobjCharacter2 = this;
            _clusterServiceLocator.WcGuild.NotifyGuildStatus(argobjCharacter2, GuildEvent.SIGNED_ON);

            // DONE: Put back character in group if disconnected
            foreach (var tmpGroup in _clusterServiceLocator.WcHandlersGroup.GrouPs)
            {
                for (byte i = 0, loopTo = (byte)(tmpGroup.Value.Members.Length - 1); i <= loopTo; i++)
                {
                    if (tmpGroup.Value.Members[i] is not null && tmpGroup.Value.Members[i].Guid == Guid)
                    {
                        tmpGroup.Value.Members[i] = this;
                        tmpGroup.Value.SendGroupList();
                        PacketClass response = new(0) { Data = GetWorld.GroupMemberStats(Guid, 0) };
                        var argobjCharacter3 = this;
                        tmpGroup.Value.BroadcastToOther(response, argobjCharacter3);
                        response.Dispose();
                        return;
                    }
                }
            }
        }

        public void OnLogout()
        {
            // DONE: Leave group
            if (IsInGroup)
            {
                var argobjCharacter = this;
                Group.Leave(argobjCharacter);
            }

            // DONE: Leave chat
            while (JoinedChannels.Count > 0)
            {
                if (_clusterServiceLocator.WsHandlerChannels.ChatChanneLs.ContainsKey(JoinedChannels[0]))
                {
                    var argCharacter = this;
                    _clusterServiceLocator.WsHandlerChannels.ChatChanneLs[JoinedChannels[0]].Part(argCharacter);
                }
                else
                {
                    JoinedChannels.RemoveAt(0);
                }
            }
        }

        public void SendGuildUpdate()
        {
            var guildId = 0U;
            if (Guild is not null)
            {
                guildId = Guild.Id;
            }

            GetWorld.GuildUpdate(Guid, guildId, GuildRank);
        }

        public ChatFlag ChatFlag;

        public void SendChatMessage(ulong thisguid, string message, ChatMsg msgType, int msgLanguage, string channelName)
        {
            if (thisguid == 0m)
            {
                thisguid = Guid;
            }

            if (string.IsNullOrEmpty(channelName))
            {
                channelName = "Global";
            }

            var msgChatFlag = ChatFlag;
            if (msgType is ChatMsg.CHAT_MSG_WHISPER_INFORM or ChatMsg.CHAT_MSG_WHISPER)
            {
                msgChatFlag = _clusterServiceLocator.WorldCluster.CharacteRs[thisguid].ChatFlag;
            }

            var packet = _clusterServiceLocator.Functions.BuildChatMessage(thisguid, message, msgType, (LANGUAGES)msgLanguage, (byte)msgChatFlag, channelName);
            Client.Send(packet);
            packet.Dispose();
        }
    }

    public ulong GetCharacterGuidByName(string name)
    {
        var guid = 0UL;
        _clusterServiceLocator.WorldCluster.CharacteRsLock.AcquireReaderLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
        foreach (var objCharacter in _clusterServiceLocator.WorldCluster.CharacteRs)
        {
            if (_clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(objCharacter.Value.Name) == _clusterServiceLocator.CommonFunctions.UppercaseFirstLetter(name))
            {
                guid = objCharacter.Value.Guid;
                break;
            }
        }

        _clusterServiceLocator.WorldCluster.CharacteRsLock.ReleaseReaderLock();
        if (guid == 0m)
        {
            DataTable q = new();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT char_guid FROM characters WHERE char_name = \"{0}\";", _clusterServiceLocator.Functions.EscapeString(name)), ref q);
            return q.Rows.Count > 0 ? q.Rows[0].As<ulong>("char_guid") : 0UL;
        }

        return guid;
    }

    public string GetCharacterNameByGuid(string guid)
    {
        if (_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(Conversions.ToULong(guid)))
        {
            return _clusterServiceLocator.WorldCluster.CharacteRs[Conversions.ToULong(guid)].Name;
        }

        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT char_name FROM characters WHERE char_guid = \"{0}\";", guid), ref q);
        return q.Rows.Count > 0 ? q.Rows[0].As<string>("char_name") : "";
    }
}
