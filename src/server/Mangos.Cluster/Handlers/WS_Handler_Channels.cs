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
using Mangos.Common.Enums.Channel;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.Cluster.Handlers;

public class WsHandlerChannels
{
    public Dictionary<string, ChatChannelClass> ChatChanneLs = new();
    private long _chatChanneLsCounter = 1L;

    private long GetNexyChatChannelId()
    {
        return Interlocked.Increment(ref _chatChanneLsCounter);
    }

    public class ChatChannelClass : IDisposable
    {
        private readonly ClusterServiceLocator _clusterServiceLocator;

        // This is server-side ID
        public long Id;

        // These are channel identificators
        public int ChannelIndex;

        public byte ChannelFlags;
        public string ChannelName;
        public string Password = "";
        public bool Announce = true;
        public bool Moderate = true;
        public List<ulong> Joined = new();
        public Dictionary<ulong, byte> JoinedMode = new();
        public List<ulong> Banned = new();
        public List<ulong> Moderators = new();
        public List<ulong> Muted = new();
        public ulong Owner;

        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                _clusterServiceLocator.WsHandlerChannels.ChatChanneLs.Remove(ChannelName.ToUpper());
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

        public ChatChannelClass(string name, ClusterServiceLocator clusterServiceLocator)
        {
            Id = clusterServiceLocator.WsHandlerChannels.GetNexyChatChannelId();
            ChannelIndex = 0;
            ChannelName = name;
            ChannelFlags = (byte)CHANNEL_FLAG.CHANNEL_FLAG_NONE;
            clusterServiceLocator.WsHandlerChannels.ChatChanneLs.Add(ChannelName, this);
            var sZone = name[(name.IndexOf(" - ", StringComparison.Ordinal) + 3)..];
            foreach (var chatChannel in clusterServiceLocator.WsDbcDatabase.ChatChannelsInfo)
            {
                if ((chatChannel.Value.Name.Replace("%s", sZone).ToUpper() ?? "") == (name.ToUpper() ?? ""))
                {
                    ChannelIndex = chatChannel.Key;
                    break;
                }
            }

            if (clusterServiceLocator.WsDbcDatabase.ChatChannelsInfo.ContainsKey(ChannelIndex))
            {
                // Default channel
                ChannelFlags = (byte)(ChannelFlags | (byte)CHANNEL_FLAG.CHANNEL_FLAG_GENERAL);
                Announce = false;
                Moderate = false;
                {
                    var withBlock = clusterServiceLocator.WsDbcDatabase.ChatChannelsInfo[ChannelIndex];
                    if (((ChatChannelsFlags)withBlock.Flags & ChatChannelsFlags.FLAG_TRADE) == ChatChannelsFlags.FLAG_TRADE)
                    {
                        ChannelFlags = (byte)(ChannelFlags | (byte)CHANNEL_FLAG.CHANNEL_FLAG_TRADE);
                    }

                    if (((ChatChannelsFlags)withBlock.Flags & ChatChannelsFlags.FLAG_CITY_ONLY2) == ChatChannelsFlags.FLAG_CITY_ONLY2)
                    {
                        ChannelFlags = (byte)(ChannelFlags | (byte)CHANNEL_FLAG.CHANNEL_FLAG_CITY);
                    }

                    ChannelFlags = ((ChatChannelsFlags)withBlock.Flags & ChatChannelsFlags.FLAG_LFG) == ChatChannelsFlags.FLAG_LFG
                        ? (byte)(ChannelFlags | (byte)CHANNEL_FLAG.CHANNEL_FLAG_LFG)
                        : (byte)((CHANNEL_FLAG)ChannelFlags | CHANNEL_FLAG.CHANNEL_FLAG_NOT_LFG);
                }
            }
            else
            {
                // Custom channel
                ChannelFlags = (byte)((CHANNEL_FLAG)ChannelFlags | CHANNEL_FLAG.CHANNEL_FLAG_CUSTOM);
            }

            _clusterServiceLocator = clusterServiceLocator;
        }

        public void Say(string message, int msgLang, WcHandlerCharacter.CharacterObject character)
        {
            if (Muted.Contains(character.Guid))
            {
                var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOUCANTSPEAK, character.Guid, default, default);
                character.Client.Send(p);
                p.Dispose();
            }
            else if (!Joined.Contains(character.Guid))
            {
                var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(p);
                p.Dispose();
            }
            else
            {
                var packet = _clusterServiceLocator.Functions.BuildChatMessage(character.Guid, message, ChatMsg.CHAT_MSG_CHANNEL, (LANGUAGES)msgLang, (byte)character.ChatFlag, ChannelName);
                Broadcast(packet);
                packet.Dispose();
                _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.USER, "[{0}:{1}] SMSG_MESSAGECHAT [{2}: <{3}> {4}]", character.Client.IP, character.Client.Port, ChannelName, character.Name, message);
            }
        }

        public virtual void Join(WcHandlerCharacter.CharacterObject character, string clientPassword)
        {
            // DONE: Check if Already joined
            if (Joined.Contains(character.Guid))
            {
                if (character.JoinedChannels.Contains(0.ToString()))
                {
                    var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_ALREADY_ON, character.Guid, default, default);
                    character.Client.Send(p);
                    p.Dispose();
                    return;
                }
            }

            // DONE: Check if banned
            if (Banned.Contains(character.Guid))
            {
                var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_ARE_BANNED, character.Guid, default, default);
                character.Client.Send(p);
                p.Dispose();
                return;
            }

            // DONE: Check for password
            if (!string.IsNullOrEmpty(Password))
            {
                if ((Password ?? "") != (clientPassword ?? ""))
                {
                    var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_WRONG_PASS, character.Guid, default, default);
                    character.Client.Send(p);
                    p.Dispose();
                    return;
                }
            }

            // DONE: {0} Joined channel
            if (Announce)
            {
                var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_JOINED, character.Guid, default, default);
                Broadcast(response);
                response.Dispose();
            }

            // DONE: You Joined channel
            var response2 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_JOINED, character.Guid, default, default);
            character.Client.Send(response2);
            response2.Dispose();
            if (Joined.Contains(character.Guid) == false)
            {
                Joined.Add(character.Guid);
            }

            if (JoinedMode.ContainsKey(character.Guid) == false)
            {
                JoinedMode.Add(character.Guid, (byte)CHANNEL_USER_FLAG.CHANNEL_FLAG_NONE);
            }

            if (character.JoinedChannels.Contains(ChannelName) == false)
            {
                character.JoinedChannels.Add(ChannelName);
            }

            // DONE: If new channel, set owner
            if (_clusterServiceLocator.Functions.HaveFlags(ChannelFlags, (byte)CHANNEL_FLAG.CHANNEL_FLAG_CUSTOM) && Owner == 0m)
            {
                SetOwner(character);
            }

            // DONE: Update flags
            var modes = CHANNEL_USER_FLAG.CHANNEL_FLAG_NONE;
            if (Muted.Contains(character.Guid))
            {
                modes |= CHANNEL_USER_FLAG.CHANNEL_FLAG_MUTED;
            }

            if (Moderators.Contains(character.Guid))
            {
                modes |= CHANNEL_USER_FLAG.CHANNEL_FLAG_MODERATOR;
            }

            if (Owner == character.Guid)
            {
                modes |= CHANNEL_USER_FLAG.CHANNEL_FLAG_OWNER;
            }

            JoinedMode[character.Guid] = (byte)modes;
        }

        public virtual void Part(WcHandlerCharacter.CharacterObject character)
        {
            // DONE: Check if not on this channel
            if (!Joined.Contains(character.Guid))
            {
                if (character.Client is not null)
                {
                    var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                    character.Client.Send(p);
                    p.Dispose();
                }

                return;
            }

            // DONE: You Left channel
            if (character.Client is not null)
            {
                var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_LEFT, character.Guid, default, default);
                character.Client.Send(p);
                p.Dispose();
            }

            Joined.Remove(character.Guid);
            JoinedMode.Remove(character.Guid);
            character.JoinedChannels.Remove(ChannelName);

            // DONE: {0} Left channel
            if (Announce)
            {
                var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_LEFT, character.Guid, default, default);
                Broadcast(response);
                response.Dispose();
            }

            // DONE: Set new owner
            if (_clusterServiceLocator.Functions.HaveFlags(ChannelFlags, (byte)CHANNEL_FLAG.CHANNEL_FLAG_CUSTOM) && Owner == character.Guid && Joined.Count > 0)
            {
                IEnumerator tmp = Joined.GetEnumerator();
                tmp.MoveNext();
                var tmp1 = _clusterServiceLocator.WorldCluster.CharacteRs;
                var argCharacter = tmp1[Conversions.ToULong(tmp.Current)];
                SetOwner(argCharacter);
                tmp1[Conversions.ToULong(tmp.Current)] = argCharacter;
            }

            // DONE: If free and not global - clear channel
            if (_clusterServiceLocator.Functions.HaveFlags(ChannelFlags, (byte)CHANNEL_FLAG.CHANNEL_FLAG_CUSTOM) && Joined.Count == 0)
            {
                _clusterServiceLocator.WsHandlerChannels.ChatChanneLs.Remove(ChannelName);
                Dispose();
            }
        }

        public virtual void Kick(WcHandlerCharacter.CharacterObject character, string name)
        {
            var victimGuid = _clusterServiceLocator.WcHandlerCharacter.GetCharacterGuidByName(name);
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Moderators.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(victimGuid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Joined.Contains(victimGuid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                // DONE: You Left channel
                var packet1 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_LEFT, character.Guid, default, default);
                _clusterServiceLocator.WorldCluster.CharacteRs[victimGuid].Client.Send(packet1);
                packet1.Dispose();
                Joined.Remove(victimGuid);
                JoinedMode.Remove(victimGuid);
                _clusterServiceLocator.WorldCluster.CharacteRs[victimGuid].JoinedChannels.Remove(ChannelName.ToUpper());

                // DONE: [%s] Player %s kicked by %s.
                var packet2 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_KICKED, victimGuid, character.Guid, default);
                Broadcast(packet2);
                packet2.Dispose();
            }
        }

        public virtual void Ban(WcHandlerCharacter.CharacterObject character, string name)
        {
            var victimGuid = _clusterServiceLocator.WcHandlerCharacter.GetCharacterGuidByName(name);
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Moderators.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(victimGuid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Joined.Contains(victimGuid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (Banned.Contains(victimGuid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITE_BANNED, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                Banned.Add(victimGuid);

                // DONE: [%s] Player %s banned by %s.
                var packet2 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_BANNED, victimGuid, character.Guid, default);
                Broadcast(packet2);
                packet2.Dispose();
                Joined.Remove(victimGuid);
                JoinedMode.Remove(victimGuid);
                _clusterServiceLocator.WorldCluster.CharacteRs[victimGuid].JoinedChannels.Remove(ChannelName.ToUpper());

                // DONE: You Left channel
                var packet1 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_LEFT, character.Guid, default, default);
                _clusterServiceLocator.WorldCluster.CharacteRs[victimGuid].Client.Send(packet1);
                packet1.Dispose();
            }
        }

        public virtual void UnBan(WcHandlerCharacter.CharacterObject character, string name)
        {
            var victimGuid = _clusterServiceLocator.WcHandlerCharacter.GetCharacterGuidByName(name);
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Moderators.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(victimGuid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Banned.Contains(victimGuid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_BANNED, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                Banned.Remove(victimGuid);

                // DONE: [%s] Player %s unbanned by %s.
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_UNBANNED, victimGuid, character.Guid, default);
                Broadcast(packet);
                packet.Dispose();
            }
        }

        public void List(WcHandlerCharacter.CharacterObject character)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                PacketClass packet = new(Opcodes.SMSG_CHANNEL_LIST);
                packet.AddInt8(0);                   // ChannelType
                packet.AddString(ChannelName);       // ChannelName
                packet.AddInt8(ChannelFlags);        // ChannelFlags
                packet.AddInt32(Joined.Count);
                foreach (var guid in Joined)
                {
                    packet.AddUInt64(guid);
                    packet.AddInt8(JoinedMode[guid]);
                }

                character.Client.Send(packet);
                packet.Dispose();
            }
        }

        public void Invite(WcHandlerCharacter.CharacterObject character, string name)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                var guid = _clusterServiceLocator.WcHandlerCharacter.GetCharacterGuidByName(name);
                if (!_clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                    character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (_clusterServiceLocator.Functions.GetCharacterSide((byte)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Race) != _clusterServiceLocator.Functions.GetCharacterSide((byte)character.Race))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_INVITED_WRONG_FACTION, character.Guid, default, default);
                    character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (Joined.Contains(guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_ALREADY_ON, guid, default, default);
                    character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (Banned.Contains(guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITE_BANNED, guid, default, name);
                    character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (_clusterServiceLocator.WorldCluster.CharacteRs[guid].IgnoreList.Contains(character.Guid))
                {
                }
                // ?
                else
                {
                    var packet1 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITED, character.Guid, default, _clusterServiceLocator.WorldCluster.CharacteRs[guid].Name);
                    character.Client.Send(packet1);
                    packet1.Dispose();
                    var packet2 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_INVITED, character.Guid, default, default);
                    _clusterServiceLocator.WorldCluster.CharacteRs[guid].Client.Send(packet2);
                    packet2.Dispose();
                }
            }
        }

        public bool CanSetOwner(WcHandlerCharacter.CharacterObject character, string name)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
                return false;
            }

            if (Owner != character.Guid)
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_OWNER, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
                return false;
            }

            foreach (var guid in Joined.ToArray())
            {
                if ((_clusterServiceLocator.WorldCluster.CharacteRs[guid].Name.ToUpper() ?? "") == (name.ToUpper() ?? ""))
                {
                    return true;
                }
            }

            var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, default);
            character.Client.Send(p);
            p.Dispose();
            return false;
        }

        public void GetOwner(WcHandlerCharacter.CharacterObject character)
        {
            PacketClass p;
            if (!Joined.Contains(character.Guid))
            {
                p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
            }
            else
            {
                p = Owner > 0m
                    ? BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_WHO_OWNER, character.Guid, default, _clusterServiceLocator.WorldCluster.CharacteRs[Owner].Name)
                    : BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_WHO_OWNER, character.Guid, default, "Nobody");
            }

            character.Client.Send(p);
            p.Dispose();
        }

        public void SetOwner(WcHandlerCharacter.CharacterObject character)
        {
            if (JoinedMode.ContainsKey(Owner))
            {
                JoinedMode[Owner] = (byte)((CHANNEL_USER_FLAG)JoinedMode[Owner] ^ CHANNEL_USER_FLAG.CHANNEL_FLAG_OWNER);
            }

            JoinedMode[character.Guid] = (byte)(JoinedMode[character.Guid] | (byte)CHANNEL_USER_FLAG.CHANNEL_FLAG_OWNER);
            Owner = character.Guid;
            if (!Moderators.Contains(Owner))
            {
                Moderators.Add(Owner);
            }

            PacketClass p;
            p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_CHANGE_OWNER, character.Guid, default, default);
            Broadcast(p);
            p.Dispose();
        }

        public void SetAnnouncements(WcHandlerCharacter.CharacterObject character)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Moderators.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                Announce = !Announce;
                var packet = Announce
                    ? BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_ENABLE_ANNOUNCE, character.Guid, default, default)
                    : BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_DISABLE_ANNOUNCE, character.Guid, default, default);
                Broadcast(packet);
                packet.Dispose();
            }
        }

        public void SetModeration(WcHandlerCharacter.CharacterObject character)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!(character.Guid == Owner))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_OWNER, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                Moderate = !Moderate;
                var packet = Announce
                    ? BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODERATED, character.Guid, default, default)
                    : BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_UNMODERATED, character.Guid, default, default);
                Broadcast(packet);
                packet.Dispose();
            }
        }

        public void SetPassword(WcHandlerCharacter.CharacterObject character, string newPassword)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Moderators.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                Password = newPassword;
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_SET_PASSWORD, character.Guid, default, default);
                Broadcast(packet);
                packet.Dispose();
            }
        }

        public void SetModerator(WcHandlerCharacter.CharacterObject character, string name)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Moderators.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                foreach (var guid in Joined.ToArray())
                {
                    if ((_clusterServiceLocator.WorldCluster.CharacteRs[guid].Name.ToUpper() ?? "") == (name.ToUpper() ?? ""))
                    {
                        var flags = JoinedMode[guid];
                        JoinedMode[guid] = (byte)((CHANNEL_USER_FLAG)JoinedMode[guid] | CHANNEL_USER_FLAG.CHANNEL_FLAG_MODERATOR);
                        if (!Moderators.Contains(guid))
                        {
                            Moderators.Add(guid);
                        }

                        var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODE_CHANGE, guid, flags, default);
                        Broadcast(response);
                        response.Dispose();
                        return;
                    }
                }

                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
        }

        public void SetUnModerator(WcHandlerCharacter.CharacterObject character, string name)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Moderators.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                foreach (var guid in Joined.ToArray())
                {
                    if ((_clusterServiceLocator.WorldCluster.CharacteRs[guid].Name.ToUpper() ?? "") == (name.ToUpper() ?? ""))
                    {
                        var flags = JoinedMode[guid];
                        JoinedMode[guid] = (byte)((CHANNEL_USER_FLAG)JoinedMode[guid] ^ CHANNEL_USER_FLAG.CHANNEL_FLAG_MODERATOR);
                        Moderators.Remove(guid);
                        var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODE_CHANGE, guid, flags, default);
                        Broadcast(response);
                        response.Dispose();
                        return;
                    }
                }

                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
        }

        public void SetMute(WcHandlerCharacter.CharacterObject character, string name)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Moderators.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                foreach (var guid in Joined.ToArray())
                {
                    if ((_clusterServiceLocator.WorldCluster.CharacteRs[guid].Name.ToUpper() ?? "") == (name.ToUpper() ?? ""))
                    {
                        var flags = JoinedMode[guid];
                        JoinedMode[guid] = (byte)((CHANNEL_USER_FLAG)JoinedMode[guid] | CHANNEL_USER_FLAG.CHANNEL_FLAG_MUTED);
                        if (!Muted.Contains(guid))
                        {
                            Muted.Add(guid);
                        }

                        var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODE_CHANGE, guid, flags, default);
                        Broadcast(response);
                        response.Dispose();
                        return;
                    }
                }

                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
        }

        public void SetUnMute(WcHandlerCharacter.CharacterObject character, string name)
        {
            if (!Joined.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else if (!Moderators.Contains(character.Guid))
            {
                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, character.Guid, default, default);
                character.Client.Send(packet);
                packet.Dispose();
            }
            else
            {
                foreach (var guid in Joined.ToArray())
                {
                    if ((_clusterServiceLocator.WorldCluster.CharacteRs[guid].Name.ToUpper() ?? "") == (name.ToUpper() ?? ""))
                    {
                        var flags = JoinedMode[guid];
                        JoinedMode[guid] = (byte)((CHANNEL_USER_FLAG)JoinedMode[guid] ^ CHANNEL_USER_FLAG.CHANNEL_FLAG_MUTED);
                        Muted.Remove(guid);
                        var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODE_CHANGE, guid, flags, default);
                        Broadcast(response);
                        response.Dispose();
                        return;
                    }
                }

                var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, character.Guid, default, name);
                character.Client.Send(packet);
                packet.Dispose();
            }
        }

        public void Broadcast(PacketClass p)
        {
            foreach (var guid in Joined.ToArray())
            {
                _clusterServiceLocator.WorldCluster.CharacteRs[guid].Client.SendMultiplyPackets(p);
            }
        }

        public void Save()
        {
            // TODO: Saving into database
        }

        public void Load()
        {
            // TODO: Loading from database
        }

        protected PacketClass BuildChannelNotify(CHANNEL_NOTIFY_FLAGS notify, ulong guid1, ulong guid2, string name)
        {
            PacketClass response = new(Opcodes.SMSG_CHANNEL_NOTIFY);
            response.AddInt8((byte)notify);
            response.AddString(ChannelName);
            switch (notify)
            {
                case var @case when @case == CHANNEL_NOTIFY_FLAGS.CHANNEL_WRONG_PASS:
                case var case1 when case1 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON:
                case var case2 when case2 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR:
                case var case3 when case3 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_OWNER:
                case var case4 when case4 == CHANNEL_NOTIFY_FLAGS.CHANNEL_YOUCANTSPEAK:
                case var case5 when case5 == CHANNEL_NOTIFY_FLAGS.CHANNEL_INVITED_WRONG_FACTION:
                case var case6 when case6 == CHANNEL_NOTIFY_FLAGS.CHANNEL_INVALID_NAME:
                case var case7 when case7 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATED:
                case var case8 when case8 == CHANNEL_NOTIFY_FLAGS.CHANNEL_THROTTLED:
                case var case9 when case9 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_IN_AREA:
                case var case10 when case10 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_IN_LFG:
                    {
                        break;
                    }
                // No extra fields

                case var case11 when case11 == CHANNEL_NOTIFY_FLAGS.CHANNEL_JOINED:
                case var case12 when case12 == CHANNEL_NOTIFY_FLAGS.CHANNEL_LEFT:
                case var case13 when case13 == CHANNEL_NOTIFY_FLAGS.CHANNEL_SET_PASSWORD:
                case var case14 when case14 == CHANNEL_NOTIFY_FLAGS.CHANNEL_CHANGE_OWNER:
                case var case15 when case15 == CHANNEL_NOTIFY_FLAGS.CHANNEL_ENABLE_ANNOUNCE:
                case var case16 when case16 == CHANNEL_NOTIFY_FLAGS.CHANNEL_DISABLE_ANNOUNCE:
                case var case17 when case17 == CHANNEL_NOTIFY_FLAGS.CHANNEL_MODERATED:
                case var case18 when case18 == CHANNEL_NOTIFY_FLAGS.CHANNEL_UNMODERATED:
                case var case19 when case19 == CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_ARE_BANNED:
                case var case20 when case20 == CHANNEL_NOTIFY_FLAGS.CHANNEL_ALREADY_ON:
                case var case21 when case21 == CHANNEL_NOTIFY_FLAGS.CHANNEL_INVITED:
                    {
                        response.AddUInt64(guid1);
                        break;
                    }

                case var case22 when case22 == CHANNEL_NOTIFY_FLAGS.CHANNEL_KICKED:
                case var case23 when case23 == CHANNEL_NOTIFY_FLAGS.CHANNEL_BANNED:
                case var case24 when case24 == CHANNEL_NOTIFY_FLAGS.CHANNEL_UNBANNED:
                    {
                        response.AddUInt64(guid1);           // Victim
                        response.AddUInt64(guid2);           // Moderator
                        break;
                    }

                case var case25 when case25 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME:
                case var case26 when case26 == CHANNEL_NOTIFY_FLAGS.CHANNEL_WHO_OWNER:
                case var case27 when case27 == CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITED:
                case var case28 when case28 == CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITE_BANNED:
                case var case29 when case29 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_BANNED:
                    {
                        response.AddString(name);
                        break;
                    }

                case var case30 when case30 == CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_JOINED:
                    {
                        response.AddInt8(ChannelFlags);
                        response.AddUInt64((ulong)ChannelIndex);
                        break;
                    }

                case var case31 when case31 == CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_LEFT:
                    {
                        response.AddUInt64((ulong)ChannelIndex);
                        break;
                    }

                case var case32 when case32 == CHANNEL_NOTIFY_FLAGS.CHANNEL_MODE_CHANGE:
                    {
                        response.AddUInt64(guid1);
                        response.AddInt8((byte)guid2);                     // Old Player Flags
                        response.AddInt8(JoinedMode[guid1]);        // New Player Flags
                        break;
                    }

                default:
                    {
                        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "Probably wrong channel function used for SendChannelNotify({0})", notify);
                        break;
                    }
            }

            return response;
        }
    }
}
