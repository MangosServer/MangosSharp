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
using System.Collections;
using System.Collections.Generic;
using Mangos.Cluster.DataStores;
using Mangos.Cluster.Globals;
using Mangos.Common.Enums.Channel;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Cluster.Handlers
{
    public class WS_Handler_Channels
    {
        public Dictionary<string, ChatChannelClass> CHAT_CHANNELs = new Dictionary<string, ChatChannelClass>();
        private long CHAT_CHANNELs_Counter = 1L;

        private long GetNexyChatChannelID()
        {
            return System.Threading.Interlocked.Increment(ref CHAT_CHANNELs_Counter);
        }

        public class ChatChannelClass : IDisposable
        {

            // This is server-side ID
            public long ID = 0L;

            // These are channel identificators
            public int ChannelIndex;
            public byte ChannelFlags;
            public string ChannelName;
            public string Password = "";
            public bool Announce = true;
            public bool Moderate = true;
            public List<ulong> Joined = new List<ulong>();
            public Dictionary<ulong, byte> Joined_Mode = new Dictionary<ulong, byte>();
            public List<ulong> Banned = new List<ulong>();
            public List<ulong> Moderators = new List<ulong>();
            public List<ulong> Muted = new List<ulong>();
            public ulong Owner = 0UL;

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.Remove(ChannelName.ToUpper());
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
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            public ChatChannelClass(string name)
            {
                ID = ClusterServiceLocator._WS_Handler_Channels.GetNexyChatChannelID();
                ChannelIndex = 0;
                ChannelName = name;
                ChannelFlags = (byte)CHANNEL_FLAG.CHANNEL_FLAG_NONE;
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.Add(ChannelName, this);
                string sZone = name.Substring(name.IndexOf(" - ", StringComparison.Ordinal) + 3);
                foreach (KeyValuePair<int, WS_DBCDatabase.ChatChannelInfo> chatChannel in ClusterServiceLocator._WS_DBCDatabase.ChatChannelsInfo)
                {
                    if ((chatChannel.Value.Name.Replace("%s", sZone).ToUpper() ?? "") == (name.ToUpper() ?? ""))
                    {
                        ChannelIndex = chatChannel.Key;
                        break;
                    }
                }

                if (ClusterServiceLocator._WS_DBCDatabase.ChatChannelsInfo.ContainsKey(ChannelIndex))
                {
                    // Default channel
                    ChannelFlags = (byte) (ChannelFlags | (byte)CHANNEL_FLAG.CHANNEL_FLAG_GENERAL);
                    Announce = false;
                    Moderate = false;
                    {
                        var withBlock = ClusterServiceLocator._WS_DBCDatabase.ChatChannelsInfo[ChannelIndex];
                        if (((ChatChannelsFlags)withBlock.Flags & ChatChannelsFlags.FLAG_TRADE) == ChatChannelsFlags.FLAG_TRADE)
                        {
                            ChannelFlags = (byte)(ChannelFlags | (byte)CHANNEL_FLAG.CHANNEL_FLAG_TRADE);
                        }

                        if (((ChatChannelsFlags)withBlock.Flags & ChatChannelsFlags.FLAG_CITY_ONLY2) == ChatChannelsFlags.FLAG_CITY_ONLY2)
                        {
                            ChannelFlags = (byte)(ChannelFlags | (byte)CHANNEL_FLAG.CHANNEL_FLAG_CITY);
                        }

                        if (((ChatChannelsFlags)withBlock.Flags & ChatChannelsFlags.FLAG_LFG) == ChatChannelsFlags.FLAG_LFG)
                        {
                            ChannelFlags = (byte)(ChannelFlags | (byte)CHANNEL_FLAG.CHANNEL_FLAG_LFG);
                        }
                        else
                        {
                            ChannelFlags = (byte)((CHANNEL_FLAG)ChannelFlags | CHANNEL_FLAG.CHANNEL_FLAG_NOT_LFG);
                        }
                    }
                }
                else
                {
                    // Custom channel
                    ChannelFlags = (byte)((CHANNEL_FLAG)ChannelFlags | CHANNEL_FLAG.CHANNEL_FLAG_CUSTOM);
                }
            }

            public void Say(string message, int msgLang, WcHandlerCharacter.CharacterObject character)
            {
                if (Muted.Contains(character.Guid))
                {
                    var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOUCANTSPEAK, character.Guid, default, default);
                    character.Client.Send(p);
                    p.Dispose();
                    return;
                }
                else if (!Joined.Contains(character.Guid))
                {
                    var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, character.Guid, default, default);
                    character.Client.Send(p);
                    p.Dispose();
                    return;
                }
                else
                {
                    var packet = ClusterServiceLocator._Functions.BuildChatMessage(character.Guid, message, ChatMsg.CHAT_MSG_CHANNEL, (LANGUAGES)msgLang, (byte)character.ChatFlag, ChannelName);
                    Broadcast(packet);
                    packet.Dispose();
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.USER, "[{0}:{1}] SMSG_MESSAGECHAT [{2}: <{3}> {4}]", character.Client.IP, character.Client.Port, ChannelName, character.Name, message);
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

                if (Joined_Mode.ContainsKey(character.Guid) == false)
                {
                    Joined_Mode.Add(character.Guid, (byte)CHANNEL_USER_FLAG.CHANNEL_FLAG_NONE);
                }

                if (character.JoinedChannels.Contains(ChannelName) == false)
                {
                    character.JoinedChannels.Add(ChannelName);
                }

                // DONE: If new channel, set owner
                if (ClusterServiceLocator._Functions.HaveFlags(ChannelFlags, (byte)CHANNEL_FLAG.CHANNEL_FLAG_CUSTOM) && Owner == 0m)
                {
                    SetOwner(character);
                }

                // DONE: Update flags
                var modes = CHANNEL_USER_FLAG.CHANNEL_FLAG_NONE;
                if (Muted.Contains(character.Guid))
                {
                    modes = modes | CHANNEL_USER_FLAG.CHANNEL_FLAG_MUTED;
                }

                if (Moderators.Contains(character.Guid))
                {
                    modes = modes | CHANNEL_USER_FLAG.CHANNEL_FLAG_MODERATOR;
                }

                if (Owner == character.Guid)
                {
                    modes = modes | CHANNEL_USER_FLAG.CHANNEL_FLAG_OWNER;
                }

                Joined_Mode[character.Guid] = (byte)modes;
            }

            public virtual void Part(WcHandlerCharacter.CharacterObject Character)
            {
                // DONE: Check if not on this channel
                if (!Joined.Contains(Character.Guid))
                {
                    if (Character.Client is object)
                    {
                        var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                        Character.Client.Send(p);
                        p.Dispose();
                    }

                    return;
                }

                // DONE: You Left channel
                if (Character.Client is object)
                {
                    var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_LEFT, Character.Guid, default, default);
                    Character.Client.Send(p);
                    p.Dispose();
                }

                Joined.Remove(Character.Guid);
                Joined_Mode.Remove(Character.Guid);
                Character.JoinedChannels.Remove(ChannelName);

                // DONE: {0} Left channel
                if (Announce)
                {
                    var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_LEFT, Character.Guid, default, default);
                    Broadcast(response);
                    response.Dispose();
                }

                // DONE: Set new owner
                if (ClusterServiceLocator._Functions.HaveFlags(ChannelFlags, (byte)CHANNEL_FLAG.CHANNEL_FLAG_CUSTOM) && Owner == Character.Guid && Joined.Count > 0)
                {
                    IEnumerator tmp = Joined.GetEnumerator();
                    tmp.MoveNext();
                    var tmp1 = ClusterServiceLocator._WorldCluster.CHARACTERs;
                    var argCharacter = tmp1[Conversions.ToULong(tmp.Current)];
                    SetOwner(argCharacter);
                    tmp1[Conversions.ToULong(tmp.Current)] = argCharacter;
                }

                // DONE: If free and not global - clear channel
                if (ClusterServiceLocator._Functions.HaveFlags(ChannelFlags, (byte)CHANNEL_FLAG.CHANNEL_FLAG_CUSTOM) && Joined.Count == 0)
                {
                    ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.Remove(ChannelName);
                    Dispose();
                }
            }

            public virtual void Kick(WcHandlerCharacter.CharacterObject Character, string Name)
            {
                ulong VictimGUID = ClusterServiceLocator._WcHandlerCharacter.GetCharacterGUIDByName(Name);
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Moderators.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(VictimGUID))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Joined.Contains(VictimGUID))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    // DONE: You Left channel
                    var packet1 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_LEFT, Character.Guid, default, default);
                    ClusterServiceLocator._WorldCluster.CHARACTERs[VictimGUID].Client.Send(packet1);
                    packet1.Dispose();
                    Joined.Remove(VictimGUID);
                    Joined_Mode.Remove(VictimGUID);
                    ClusterServiceLocator._WorldCluster.CHARACTERs[VictimGUID].JoinedChannels.Remove(ChannelName.ToUpper());

                    // DONE: [%s] Player %s kicked by %s.
                    var packet2 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_KICKED, VictimGUID, Character.Guid, default);
                    Broadcast(packet2);
                    packet2.Dispose();
                }
            }

            public virtual void Ban(WcHandlerCharacter.CharacterObject Character, string Name)
            {
                ulong VictimGUID = ClusterServiceLocator._WcHandlerCharacter.GetCharacterGUIDByName(Name);
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Moderators.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(VictimGUID))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Joined.Contains(VictimGUID))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (Banned.Contains(VictimGUID))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITE_BANNED, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    Banned.Add(VictimGUID);

                    // DONE: [%s] Player %s banned by %s.
                    var packet2 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_BANNED, VictimGUID, Character.Guid, default);
                    Broadcast(packet2);
                    packet2.Dispose();
                    Joined.Remove(VictimGUID);
                    Joined_Mode.Remove(VictimGUID);
                    ClusterServiceLocator._WorldCluster.CHARACTERs[VictimGUID].JoinedChannels.Remove(ChannelName.ToUpper());

                    // DONE: You Left channel
                    var packet1 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_YOU_LEFT, Character.Guid, default, default);
                    ClusterServiceLocator._WorldCluster.CHARACTERs[VictimGUID].Client.Send(packet1);
                    packet1.Dispose();
                }
            }

            public virtual void UnBan(WcHandlerCharacter.CharacterObject Character, string Name)
            {
                ulong VictimGUID = ClusterServiceLocator._WcHandlerCharacter.GetCharacterGUIDByName(Name);
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Moderators.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(VictimGUID))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Banned.Contains(VictimGUID))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_BANNED, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    Banned.Remove(VictimGUID);

                    // DONE: [%s] Player %s unbanned by %s.
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_UNBANNED, VictimGUID, Character.Guid, default);
                    Broadcast(packet);
                    packet.Dispose();
                }
            }

            public void List(WcHandlerCharacter.CharacterObject Character)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    var packet = new Packets.PacketClass(OPCODES.SMSG_CHANNEL_LIST);
                    packet.AddInt8(0);                   // ChannelType
                    packet.AddString(ChannelName);       // ChannelName
                    packet.AddInt8(ChannelFlags);        // ChannelFlags
                    packet.AddInt32(Joined.Count);
                    foreach (ulong GUID in Joined)
                    {
                        packet.AddUInt64(GUID);
                        packet.AddInt8(Joined_Mode[GUID]);
                    }

                    Character.Client.Send(packet);
                    packet.Dispose();
                }
            }

            public void Invite(WcHandlerCharacter.CharacterObject Character, string Name)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    ulong GUID = ClusterServiceLocator._WcHandlerCharacter.GetCharacterGUIDByName(Name);
                    if (!ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(GUID))
                    {
                        var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                        Character.Client.Send(packet);
                        packet.Dispose();
                    }
                    else if (ClusterServiceLocator._Functions.GetCharacterSide((byte)ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Race) != ClusterServiceLocator._Functions.GetCharacterSide((byte)Character.Race))
                    {
                        var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_INVITED_WRONG_FACTION, Character.Guid, default, default);
                        Character.Client.Send(packet);
                        packet.Dispose();
                    }
                    else if (Joined.Contains(GUID))
                    {
                        var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_ALREADY_ON, GUID, default, default);
                        Character.Client.Send(packet);
                        packet.Dispose();
                    }
                    else if (Banned.Contains(GUID))
                    {
                        var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITE_BANNED, GUID, default, Name);
                        Character.Client.Send(packet);
                        packet.Dispose();
                    }
                    else if (ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].IgnoreList.Contains(Character.Guid))
                    {
                    }
                    // ?
                    else
                    {
                        var packet1 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITED, Character.Guid, default, ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Name);
                        Character.Client.Send(packet1);
                        packet1.Dispose();
                        var packet2 = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_INVITED, Character.Guid, default, default);
                        ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Client.Send(packet2);
                        packet2.Dispose();
                    }
                }
            }

            public bool CanSetOwner(WcHandlerCharacter.CharacterObject Character, string Name)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                    return false;
                }

                if (Owner != Character.Guid)
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_OWNER, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                    return false;
                }

                foreach (ulong GUID in Joined.ToArray())
                {
                    if ((ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Name.ToUpper() ?? "") == (Name.ToUpper() ?? ""))
                    {
                        return true;
                    }
                }

                var p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, default);
                Character.Client.Send(p);
                p.Dispose();
                return false;
            }

            public void GetOwner(WcHandlerCharacter.CharacterObject Character)
            {
                Packets.PacketClass p;
                if (!Joined.Contains(Character.Guid))
                {
                    p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                }
                else if (Owner > 0m)
                {
                    p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_WHO_OWNER, Character.Guid, default, ClusterServiceLocator._WorldCluster.CHARACTERs[Owner].Name);
                }
                else
                {
                    p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_WHO_OWNER, Character.Guid, default, "Nobody");
                }

                Character.Client.Send(p);
                p.Dispose();
            }

            public void SetOwner(WcHandlerCharacter.CharacterObject Character)
            {
                if (Joined_Mode.ContainsKey(Owner))
                {
                    Joined_Mode[Owner] = (byte)((CHANNEL_USER_FLAG)Joined_Mode[Owner] ^ CHANNEL_USER_FLAG.CHANNEL_FLAG_OWNER);
                }

                Joined_Mode[Character.Guid] = (byte)(Joined_Mode[Character.Guid] | (byte)CHANNEL_USER_FLAG.CHANNEL_FLAG_OWNER);
                Owner = Character.Guid;
                if (!Moderators.Contains(Owner))
                    Moderators.Add(Owner);
                Packets.PacketClass p;
                p = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_CHANGE_OWNER, Character.Guid, default, default);
                Broadcast(p);
                p.Dispose();
            }

            public void SetAnnouncements(WcHandlerCharacter.CharacterObject Character)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Moderators.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    Announce = !Announce;
                    Packets.PacketClass packet;
                    if (Announce)
                    {
                        packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_ENABLE_ANNOUNCE, Character.Guid, default, default);
                    }
                    else
                    {
                        packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_DISABLE_ANNOUNCE, Character.Guid, default, default);
                    }

                    Broadcast(packet);
                    packet.Dispose();
                }
            }

            public void SetModeration(WcHandlerCharacter.CharacterObject Character)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!(Character.Guid != Owner))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_OWNER, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    Moderate = !Moderate;
                    Packets.PacketClass packet;
                    if (Announce)
                    {
                        packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODERATED, Character.Guid, default, default);
                    }
                    else
                    {
                        packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_UNMODERATED, Character.Guid, default, default);
                    }

                    Broadcast(packet);
                    packet.Dispose();
                }
            }

            public void SetPassword(WcHandlerCharacter.CharacterObject Character, string NewPassword)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Moderators.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    Password = NewPassword;
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_SET_PASSWORD, Character.Guid, default, default);
                    Broadcast(packet);
                    packet.Dispose();
                }
            }

            public void SetModerator(WcHandlerCharacter.CharacterObject Character, string Name)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Moderators.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    foreach (ulong GUID in Joined.ToArray())
                    {
                        if ((ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Name.ToUpper() ?? "") == (Name.ToUpper() ?? ""))
                        {
                            byte flags = Joined_Mode[GUID];
                            Joined_Mode[GUID] = (byte)((CHANNEL_USER_FLAG)Joined_Mode[GUID] | CHANNEL_USER_FLAG.CHANNEL_FLAG_MODERATOR);
                            if (!Moderators.Contains(GUID))
                                Moderators.Add(GUID);
                            var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODE_CHANGE, GUID, flags, default);
                            Broadcast(response);
                            response.Dispose();
                            return;
                        }
                    }

                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
            }

            public void SetUnModerator(WcHandlerCharacter.CharacterObject Character, string Name)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Moderators.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    foreach (ulong GUID in Joined.ToArray())
                    {
                        if ((ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Name.ToUpper() ?? "") == (Name.ToUpper() ?? ""))
                        {
                            byte flags = Joined_Mode[GUID];
                            Joined_Mode[GUID] = (byte)((CHANNEL_USER_FLAG)Joined_Mode[GUID] ^ CHANNEL_USER_FLAG.CHANNEL_FLAG_MODERATOR);
                            Moderators.Remove(GUID);
                            var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODE_CHANGE, GUID, flags, default);
                            Broadcast(response);
                            response.Dispose();
                            return;
                        }
                    }

                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
            }

            public void SetMute(WcHandlerCharacter.CharacterObject Character, string Name)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Moderators.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    foreach (ulong GUID in Joined.ToArray())
                    {
                        if ((ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Name.ToUpper() ?? "") == (Name.ToUpper() ?? ""))
                        {
                            byte flags = Joined_Mode[GUID];
                            Joined_Mode[GUID] = (byte)((CHANNEL_USER_FLAG)Joined_Mode[GUID] | CHANNEL_USER_FLAG.CHANNEL_FLAG_MUTED);
                            if (!Muted.Contains(GUID))
                                Muted.Add(GUID);
                            var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODE_CHANGE, GUID, flags, default);
                            Broadcast(response);
                            response.Dispose();
                            return;
                        }
                    }

                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
            }

            public void SetUnMute(WcHandlerCharacter.CharacterObject Character, string Name)
            {
                if (!Joined.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else if (!Moderators.Contains(Character.Guid))
                {
                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_MODERATOR, Character.Guid, default, default);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
                else
                {
                    foreach (ulong GUID in Joined.ToArray())
                    {
                        if ((ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Name.ToUpper() ?? "") == (Name.ToUpper() ?? ""))
                        {
                            byte flags = Joined_Mode[GUID];
                            Joined_Mode[GUID] = (byte)((CHANNEL_USER_FLAG)Joined_Mode[GUID] ^ CHANNEL_USER_FLAG.CHANNEL_FLAG_MUTED);
                            Muted.Remove(GUID);
                            var response = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_MODE_CHANGE, GUID, flags, default);
                            Broadcast(response);
                            response.Dispose();
                            return;
                        }
                    }

                    var packet = BuildChannelNotify(CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME, Character.Guid, default, Name);
                    Character.Client.Send(packet);
                    packet.Dispose();
                }
            }

            public void Broadcast(Packets.PacketClass p)
            {
                foreach (ulong GUID in Joined.ToArray())
                    ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Client.SendMultiplyPackets(p);
            }

            public void Save()
            {
                // TODO: Saving into database
            }

            public void Load()
            {
                // TODO: Loading from database
            }

            protected Packets.PacketClass BuildChannelNotify(CHANNEL_NOTIFY_FLAGS Notify, ulong GUID1, ulong GUID2, string Name)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_CHANNEL_NOTIFY);
                response.AddInt8((byte)Notify);
                response.AddString(ChannelName);
                switch (Notify)
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
                            response.AddUInt64(GUID1);
                            break;
                        }

                    case var case22 when case22 == CHANNEL_NOTIFY_FLAGS.CHANNEL_KICKED:
                    case var case23 when case23 == CHANNEL_NOTIFY_FLAGS.CHANNEL_BANNED:
                    case var case24 when case24 == CHANNEL_NOTIFY_FLAGS.CHANNEL_UNBANNED:
                        {
                            response.AddUInt64(GUID1);           // Victim
                            response.AddUInt64(GUID2);           // Moderator
                            break;
                        }

                    case var case25 when case25 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_ON_FOR_NAME:
                    case var case26 when case26 == CHANNEL_NOTIFY_FLAGS.CHANNEL_WHO_OWNER:
                    case var case27 when case27 == CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITED:
                    case var case28 when case28 == CHANNEL_NOTIFY_FLAGS.CHANNEL_PLAYER_INVITE_BANNED:
                    case var case29 when case29 == CHANNEL_NOTIFY_FLAGS.CHANNEL_NOT_BANNED:
                        {
                            response.AddString(Name);
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
                            response.AddUInt64(GUID1);
                            response.AddInt8((byte)GUID2);                     // Old Player Flags
                            response.AddInt8(Joined_Mode[GUID1]);        // New Player Flags
                            break;
                        }

                    default:
                        {
                            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "Probably wrong channel function used for SendChannelNotify({0})", Notify);
                            break;
                        }
                }

                return response;
            }
        }
    }
}