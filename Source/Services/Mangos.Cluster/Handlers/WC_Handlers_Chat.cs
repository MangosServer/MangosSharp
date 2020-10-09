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

using System.Collections.Generic;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Server;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;

namespace Mangos.Cluster.Handlers
{
    public class WC_Handlers_Chat
    {
        public void On_CMSG_CHAT_IGNORED(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAT_IGNORED [0x{2}]", client.IP, client.Port, guid);
            if (ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(guid))
            {
                var response = ClusterServiceLocator._Functions.BuildChatMessage(client.Character.Guid, "", ChatMsg.CHAT_MSG_IGNORED, LANGUAGES.LANG_UNIVERSAL, 0, "");
                ClusterServiceLocator._WorldCluster.CHARACTERs[guid].Client.Send(ref response);
                response.Dispose();
            }
        }

        public void On_CMSG_MESSAGECHAT(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 14)
                return;
            packet.GetInt16();
            ChatMsg msgType = packet.GetInt32();
            LANGUAGES msgLanguage = packet.GetInt32();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MESSAGECHAT [{2}:{3}]", client.IP, client.Port, msgType, msgLanguage);
            switch (msgType)
            {
                case var @case when @case == ChatMsg.CHAT_MSG_CHANNEL:
                    {
                        string channel = packet.GetString();
                        if (packet.Data.Length - 1 < 14 + channel.Length)
                            return;
                        string message = packet.GetString();

                        // DONE: Broadcast to all
                        if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(channel))
                        {
                            ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[channel].Say(message, msgLanguage, ref client.Character);
                        }

                        return;
                    }

                case var case1 when case1 == ChatMsg.CHAT_MSG_WHISPER:
                    {
                        string argname = packet.GetString();
                        string toUser = ClusterServiceLocator._Functions.CapitalizeName(ref argname);
                        if (packet.Data.Length - 1 < 14 + toUser.Length)
                            return;
                        string message = packet.GetString();

                        // DONE: Handle admin/gm commands
                        // If ToUser = "Warden" AndAlso client.Character.Access > 0 Then
                        // client.Character.GetWorld.ClientPacket(Client.Index, packet.Data)
                        // Exit Sub
                        // End If

                        // DONE: Send whisper MSG to receiver
                        ulong guid = 0UL;
                        ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.AcquireReaderLock(ClusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> character in ClusterServiceLocator._WorldCluster.CHARACTERs)
                        {
                            if (ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(character.Value.Name) == ClusterServiceLocator._CommonFunctions.UppercaseFirstLetter(toUser))
                            {
                                guid = character.Value.Guid;
                                break;
                            }
                        }

                        ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.ReleaseReaderLock();
                        if (guid > 0m && ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(guid))
                        {
                            // DONE: Check if ignoring
                            if (ClusterServiceLocator._WorldCluster.CHARACTERs[guid].IgnoreList.Contains(client.Character.Guid) && client.Character.Access < AccessLevel.GameMaster)
                            {
                                // Client.Character.SystemMessage(String.Format("{0} is ignoring you.", ToUser))
                                client.Character.SendChatMessage(guid, "", ChatMsg.CHAT_MSG_IGNORED, LANGUAGES.LANG_UNIVERSAL, "");
                            }
                            else
                            {
                                // To message
                                client.Character.SendChatMessage(guid, message, ChatMsg.CHAT_MSG_WHISPER_INFORM, msgLanguage, "");
                                if (ClusterServiceLocator._WorldCluster.CHARACTERs[guid].DND == false || client.Character.Access >= AccessLevel.GameMaster)
                                {
                                    // From message
                                    ClusterServiceLocator._WorldCluster.CHARACTERs[guid].SendChatMessage(client.Character.Guid, message, ChatMsg.CHAT_MSG_WHISPER, msgLanguage, "");
                                }
                                else
                                {
                                    // DONE: Send the DND message
                                    client.Character.SendChatMessage(guid, ClusterServiceLocator._WorldCluster.CHARACTERs[guid].AfkMessage, ChatMsg.CHAT_MSG_DND, msgLanguage, "");
                                }

                                // DONE: Send the AFK message
                                if (ClusterServiceLocator._WorldCluster.CHARACTERs[guid].AFK)
                                    client.Character.SendChatMessage(guid, ClusterServiceLocator._WorldCluster.CHARACTERs[guid].AfkMessage, ChatMsg.CHAT_MSG_AFK, msgLanguage, "");
                            }
                        }
                        else
                        {
                            var smsgChatPlayerNotFound = new Packets.PacketClass(OPCODES.SMSG_CHAT_PLAYER_NOT_FOUND);
                            smsgChatPlayerNotFound.AddString(toUser);
                            client.Send(ref smsgChatPlayerNotFound);
                            smsgChatPlayerNotFound.Dispose();
                        }

                        break;
                    }

                case var case2 when case2 == ChatMsg.CHAT_MSG_PARTY:
                case var case3 when case3 == ChatMsg.CHAT_MSG_RAID:
                case var case4 when case4 == ChatMsg.CHAT_MSG_RAID_LEADER:
                case var case5 when case5 == ChatMsg.CHAT_MSG_RAID_WARNING:
                    {
                        string message = packet.GetString();

                        // DONE: Check in group
                        if (!client.Character.IsInGroup)
                        {
                            break;
                        }

                        // DONE: Broadcast to party
                        client.Character.Group.SendChatMessage(ref client.Character, message, msgLanguage, msgType);
                        break;
                    }

                case var case6 when case6 == ChatMsg.CHAT_MSG_AFK:
                    {
                        string message = packet.GetString();
                        // TODO: Can not be used while in combat!
                        if (string.IsNullOrEmpty(message) || client.Character.AFK == false)
                        {
                            if (client.Character.AFK == false)
                            {
                                if (string.IsNullOrEmpty(message))
                                    message = "Away From Keyboard";
                                client.Character.AfkMessage = message;
                            }

                            client.Character.AFK = !client.Character.AFK;
                            if (client.Character.AFK && client.Character.DND)
                            {
                                client.Character.DND = false;
                            }

                            if (client.Character.AFK)
                            {
                                client.Character.ChatFlag = ChatFlag.FLAGS_AFK;
                            }
                            else
                            {
                                client.Character.ChatFlag = ChatFlag.FLAGS_NONE;
                            }
                            // DONE: Pass the packet to the world server so it also knows about it
                            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
                        }

                        break;
                    }

                case var case7 when case7 == ChatMsg.CHAT_MSG_DND:
                    {
                        string message = packet.GetString();
                        if (string.IsNullOrEmpty(message) || client.Character.DND == false)
                        {
                            if (client.Character.DND == false)
                            {
                                if (string.IsNullOrEmpty(message))
                                    message = "Do Not Disturb";
                                client.Character.AfkMessage = message;
                            }

                            client.Character.DND = !client.Character.DND;
                            if (client.Character.DND && client.Character.AFK)
                            {
                                client.Character.AFK = false;
                            }

                            if (client.Character.DND)
                            {
                                client.Character.ChatFlag = ChatFlag.FLAGS_DND;
                            }
                            else
                            {
                                client.Character.ChatFlag = ChatFlag.FLAGS_NONE;
                            }
                            // DONE: Pass the packet to the world server so it also knows about it
                            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
                        }

                        break;
                    }

                case var case8 when case8 == ChatMsg.CHAT_MSG_SAY:
                case var case9 when case9 == ChatMsg.CHAT_MSG_YELL:
                case var case10 when case10 == ChatMsg.CHAT_MSG_EMOTE:
                    {
                        client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
                        break;
                    }

                case var case11 when case11 == ChatMsg.CHAT_MSG_GUILD:
                    {
                        string message = packet.GetString();

                        // DONE: Broadcast to guild
                        ClusterServiceLocator._WC_Guild.BroadcastChatMessageGuild(ref client.Character, message, msgLanguage, client.Character.Guild.ID);
                        break;
                    }

                case var case12 when case12 == ChatMsg.CHAT_MSG_OFFICER:
                    {
                        string message = packet.GetString();

                        // DONE: Broadcast to officer chat
                        ClusterServiceLocator._WC_Guild.BroadcastChatMessageOfficer(ref client.Character, message, msgLanguage, client.Character.Guild.ID);
                        break;
                    }

                default:
                    {
                        ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unknown chat message [msgType={2}, msgLanguage={3}]", client.IP, client.Port, msgType, msgLanguage);
                        ClusterServiceLocator._Packets.DumpPacket(packet.Data, ref client);
                        break;
                    }
            }
        }

        public void On_CMSG_JOIN_CHANNEL(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string channelName = packet.GetString();
            string password = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_JOIN_CHANNEL [{2}]", client.IP, client.Port, channelName);
            if (!ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(channelName))
            {
                // The New does a an add to the .Containskey collection above
                var newChannel = new WS_Handler_Channels.ChatChannelClass(channelName);
            }

            ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[channelName].Join(ref client.Character, password);
        }

        public void On_CMSG_LEAVE_CHANNEL(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LEAVE_CHANNEL [{2}]", client.IP, client.Port, ChannelName);
            ChannelName = ChannelName;
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Part(ref client.Character);
            }
        }

        public void On_CMSG_CHANNEL_LIST(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_LIST [{2}]", client.IP, client.Port, ChannelName);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].List(ref client.Character);
            }
        }

        public void On_CMSG_CHANNEL_PASSWORD(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelNewPassword = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_PASSWORD [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelNewPassword);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetPassword(ref client.Character, ChannelNewPassword);
            }
        }

        public void On_CMSG_CHANNEL_SET_OWNER(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelNewOwner = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_SET_OWNER [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelNewOwner);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].CanSetOwner(ref client.Character, ChannelNewOwner))
                {
                    foreach (ulong GUID in ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Joined.ToArray())
                    {
                        if ((ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Name.ToUpper() ?? "") == (ChannelNewOwner.ToUpper() ?? ""))
                        {
                            var tmp = ClusterServiceLocator._WorldCluster.CHARACTERs;
                            var argCharacter = tmp[GUID];
                            ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetOwner(ref argCharacter);
                            tmp[GUID] = argCharacter;
                            break;
                        }
                    }
                }
            }
        }

        public void On_CMSG_CHANNEL_OWNER(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_OWNER [{2}]", client.IP, client.Port, ChannelName);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].GetOwner(ref client.Character);
            }
        }

        public void On_CMSG_CHANNEL_MODERATOR(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelUser = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_MODERATOR [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelUser);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetModerator(ref client.Character, ChannelUser);
            }
        }

        public void On_CMSG_CHANNEL_UNMODERATOR(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelUser = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_UNMODERATOR [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelUser);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetUnModerator(ref client.Character, ChannelUser);
            }
        }

        public void On_CMSG_CHANNEL_MUTE(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelUser = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_MUTE [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelUser);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetMute(ref client.Character, ChannelUser);
            }
        }

        public void On_CMSG_CHANNEL_UNMUTE(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelUser = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_UNMUTE [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelUser);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetUnMute(ref client.Character, ChannelUser);
            }
        }

        public void On_CMSG_CHANNEL_INVITE(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            string ChannelName = packet.GetString();
            if (packet.Data.Length - 1 < 6 + ChannelName.Length + 1)
                return;
            string PlayerName = ClusterServiceLocator._Functions.CapitalizeName(ref packet.GetString());
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_INVITE [{2}, {3}]", client.IP, client.Port, ChannelName, PlayerName);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Invite(ref client.Character, PlayerName);
            }
        }

        public void On_CMSG_CHANNEL_KICK(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            string ChannelName = packet.GetString();
            if (packet.Data.Length - 1 < 6 + ChannelName.Length + 1)
                return;
            string PlayerName = ClusterServiceLocator._Functions.CapitalizeName(ref packet.GetString());
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_KICK [{2}, {3}]", client.IP, client.Port, ChannelName, PlayerName);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Kick(ref client.Character, PlayerName);
            }
        }

        public void On_CMSG_CHANNEL_ANNOUNCEMENTS(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_ANNOUNCEMENTS [{2}]", client.IP, client.Port, ChannelName);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetAnnouncements(ref client.Character);
            }
        }

        public void On_CMSG_CHANNEL_BAN(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            string ChannelName = packet.GetString();
            if (packet.Data.Length - 1 < 6 + ChannelName.Length + 1)
                return;
            string PlayerName = ClusterServiceLocator._Functions.CapitalizeName(ref packet.GetString());
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_BAN [{2}, {3}]", client.IP, client.Port, ChannelName, PlayerName);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Ban(ref client.Character, PlayerName);
            }
        }

        public void On_CMSG_CHANNEL_UNBAN(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            string ChannelName = packet.GetString();
            if (packet.Data.Length - 1 < 6 + ChannelName.Length + 1)
                return;
            string PlayerName = ClusterServiceLocator._Functions.CapitalizeName(ref packet.GetString());
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_UNBAN [{2}, {3}]", client.IP, client.Port, ChannelName, PlayerName);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].UnBan(ref client.Character, PlayerName);
            }
        }

        public void On_CMSG_CHANNEL_MODERATE(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_MODERATE [{2}]", client.IP, client.Port, ChannelName);

            // ChannelName = ChannelName.ToUpper
            if (ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                ClusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetModeration(ref client.Character);
            }
        }
    }
}