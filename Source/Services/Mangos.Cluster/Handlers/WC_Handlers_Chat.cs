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
        private readonly ClusterServiceLocator clusterServiceLocator;

        public WC_Handlers_Chat(ClusterServiceLocator clusterServiceLocator)
        {
            this.clusterServiceLocator = clusterServiceLocator;
        }

        public void On_CMSG_CHAT_IGNORED(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAT_IGNORED [0x{2}]", client.IP, client.Port, guid);
            if (clusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(guid))
            {
                var response = clusterServiceLocator._Functions.BuildChatMessage(client.Character.Guid, "", ChatMsg.CHAT_MSG_IGNORED, LANGUAGES.LANG_UNIVERSAL, 0, "");
                clusterServiceLocator._WorldCluster.CHARACTERs[guid].Client.Send(response);
                response.Dispose();
            }
        }

        public void On_CMSG_MESSAGECHAT(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 14)
                return;
            packet.GetInt16();
            ChatMsg msgType = (ChatMsg)packet.GetInt32();
            LANGUAGES msgLanguage = (LANGUAGES)packet.GetInt32();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MESSAGECHAT [{2}:{3}]", client.IP, client.Port, msgType, msgLanguage);
            switch (msgType)
            {
                case var @case when @case == ChatMsg.CHAT_MSG_CHANNEL:
                    {
                        string channel = packet.GetString();
                        if (packet.Data.Length - 1 < 14 + channel.Length)
                            return;
                        string message = packet.GetString();

                        // DONE: Broadcast to all
                        if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(channel))
                        {
                            clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[channel].Say(message, (int)msgLanguage, client.Character);
                        }

                        return;
                    }

                case var case1 when case1 == ChatMsg.CHAT_MSG_WHISPER:
                    {
                        string argname = packet.GetString();
                        string toUser = clusterServiceLocator._Functions.CapitalizeName(argname);
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
                        clusterServiceLocator._WorldCluster.CHARACTERs_Lock.AcquireReaderLock(clusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> character in clusterServiceLocator._WorldCluster.CHARACTERs)
                        {
                            if (clusterServiceLocator._CommonFunctions.UppercaseFirstLetter(character.Value.Name) == clusterServiceLocator._CommonFunctions.UppercaseFirstLetter(toUser))
                            {
                                guid = character.Value.Guid;
                                break;
                            }
                        }

                        clusterServiceLocator._WorldCluster.CHARACTERs_Lock.ReleaseReaderLock();
                        if (guid > 0m && clusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(guid))
                        {
                            // DONE: Check if ignoring
                            if (clusterServiceLocator._WorldCluster.CHARACTERs[guid].IgnoreList.Contains(client.Character.Guid) && client.Character.Access < AccessLevel.GameMaster)
                            {
                                // Client.Character.SystemMessage(String.Format("{0} is ignoring you.", ToUser))
                                client.Character.SendChatMessage(guid, "", ChatMsg.CHAT_MSG_IGNORED, (int)LANGUAGES.LANG_UNIVERSAL, "");
                            }
                            else
                            {
                                // To message
                                client.Character.SendChatMessage(guid, message, ChatMsg.CHAT_MSG_WHISPER_INFORM, (int)msgLanguage, "");
                                if (clusterServiceLocator._WorldCluster.CHARACTERs[guid].DND == false || client.Character.Access >= AccessLevel.GameMaster)
                                {
                                    // From message
                                    clusterServiceLocator._WorldCluster.CHARACTERs[guid].SendChatMessage(client.Character.Guid, message, ChatMsg.CHAT_MSG_WHISPER, (int)msgLanguage, "");
                                }
                                else
                                {
                                    // DONE: Send the DND message
                                    client.Character.SendChatMessage(guid, clusterServiceLocator._WorldCluster.CHARACTERs[guid].AfkMessage, ChatMsg.CHAT_MSG_DND, (int)msgLanguage, "");
                                }

                                // DONE: Send the AFK message
                                if (clusterServiceLocator._WorldCluster.CHARACTERs[guid].AFK)
                                    client.Character.SendChatMessage(guid, clusterServiceLocator._WorldCluster.CHARACTERs[guid].AfkMessage, ChatMsg.CHAT_MSG_AFK, (int)msgLanguage, "");
                            }
                        }
                        else
                        {
                            var smsgChatPlayerNotFound = new Packets.PacketClass(Opcodes.SMSG_CHAT_PLAYER_NOT_FOUND);
                            smsgChatPlayerNotFound.AddString(toUser);
                            client.Send(smsgChatPlayerNotFound);
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
                        client.Character.Group.SendChatMessage(client.Character, message, msgLanguage, msgType);
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
                        clusterServiceLocator._WC_Guild.BroadcastChatMessageGuild(client.Character, message, msgLanguage, (int)client.Character.Guild.ID);
                        break;
                    }

                case var case12 when case12 == ChatMsg.CHAT_MSG_OFFICER:
                    {
                        string message = packet.GetString();

                        // DONE: Broadcast to officer chat
                        clusterServiceLocator._WC_Guild.BroadcastChatMessageOfficer(client.Character, message, msgLanguage, (int)client.Character.Guild.ID);
                        break;
                    }

                default:
                    {
                        clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unknown chat message [msgType={2}, msgLanguage={3}]", client.IP, client.Port, msgType, msgLanguage);
                        clusterServiceLocator._Packets.DumpPacket(packet.Data, client);
                        break;
                    }
            }
        }

        public void On_CMSG_JOIN_CHANNEL(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string channelName = packet.GetString();
            string password = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_JOIN_CHANNEL [{2}]", client.IP, client.Port, channelName);
            if (!clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(channelName))
            {
                // The New does a an add to the .Containskey collection above
                var newChannel = new WS_Handler_Channels.ChatChannelClass(channelName);
            }

            clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[channelName].Join(client.Character, password);
        }

        public void On_CMSG_LEAVE_CHANNEL(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LEAVE_CHANNEL [{2}]", client.IP, client.Port, ChannelName);
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Part(client.Character);
            }
        }

        public void On_CMSG_CHANNEL_LIST(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_LIST [{2}]", client.IP, client.Port, ChannelName);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].List(client.Character);
            }
        }

        public void On_CMSG_CHANNEL_PASSWORD(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelNewPassword = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_PASSWORD [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelNewPassword);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetPassword(client.Character, ChannelNewPassword);
            }
        }

        public void On_CMSG_CHANNEL_SET_OWNER(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelNewOwner = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_SET_OWNER [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelNewOwner);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].CanSetOwner(client.Character, ChannelNewOwner))
                {
                    foreach (ulong GUID in clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Joined.ToArray())
                    {
                        if ((clusterServiceLocator._WorldCluster.CHARACTERs[GUID].Name.ToUpper() ?? "") == (ChannelNewOwner.ToUpper() ?? ""))
                        {
                            var tmp = clusterServiceLocator._WorldCluster.CHARACTERs;
                            var argCharacter = tmp[GUID];
                            clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetOwner(argCharacter);
                            tmp[GUID] = argCharacter;
                            break;
                        }
                    }
                }
            }
        }

        public void On_CMSG_CHANNEL_OWNER(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_OWNER [{2}]", client.IP, client.Port, ChannelName);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].GetOwner(client.Character);
            }
        }

        public void On_CMSG_CHANNEL_MODERATOR(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelUser = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_MODERATOR [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelUser);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetModerator(client.Character, ChannelUser);
            }
        }

        public void On_CMSG_CHANNEL_UNMODERATOR(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelUser = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_UNMODERATOR [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelUser);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetUnModerator(client.Character, ChannelUser);
            }
        }

        public void On_CMSG_CHANNEL_MUTE(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelUser = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_MUTE [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelUser);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetMute(client.Character, ChannelUser);
            }
        }

        public void On_CMSG_CHANNEL_UNMUTE(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            string ChannelUser = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_UNMUTE [{2}, {3}]", client.IP, client.Port, ChannelName, ChannelUser);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetUnMute(client.Character, ChannelUser);
            }
        }

        public void On_CMSG_CHANNEL_INVITE(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            string ChannelName = packet.GetString();
            if (packet.Data.Length - 1 < 6 + ChannelName.Length + 1)
                return;
            string PlayerName = clusterServiceLocator._Functions.CapitalizeName(packet.GetString());
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_INVITE [{2}, {3}]", client.IP, client.Port, ChannelName, PlayerName);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Invite(client.Character, PlayerName);
            }
        }

        public void On_CMSG_CHANNEL_KICK(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            string ChannelName = packet.GetString();
            if (packet.Data.Length - 1 < 6 + ChannelName.Length + 1)
                return;
            string PlayerName = clusterServiceLocator._Functions.CapitalizeName(packet.GetString());
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_KICK [{2}, {3}]", client.IP, client.Port, ChannelName, PlayerName);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Kick(client.Character, PlayerName);
            }
        }

        public void On_CMSG_CHANNEL_ANNOUNCEMENTS(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_ANNOUNCEMENTS [{2}]", client.IP, client.Port, ChannelName);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetAnnouncements(client.Character);
            }
        }

        public void On_CMSG_CHANNEL_BAN(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            string ChannelName = packet.GetString();
            if (packet.Data.Length - 1 < 6 + ChannelName.Length + 1)
                return;
            string PlayerName = clusterServiceLocator._Functions.CapitalizeName(packet.GetString());
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_BAN [{2}, {3}]", client.IP, client.Port, ChannelName, PlayerName);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].Ban(client.Character, PlayerName);
            }
        }

        public void On_CMSG_CHANNEL_UNBAN(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            string ChannelName = packet.GetString();
            if (packet.Data.Length - 1 < 6 + ChannelName.Length + 1)
                return;
            string PlayerName = clusterServiceLocator._Functions.CapitalizeName(packet.GetString());
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_UNBAN [{2}, {3}]", client.IP, client.Port, ChannelName, PlayerName);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].UnBan(client.Character, PlayerName);
            }
        }

        public void On_CMSG_CHANNEL_MODERATE(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string ChannelName = packet.GetString();
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_MODERATE [{2}]", client.IP, client.Port, ChannelName);

            // ChannelName = ChannelName.ToUpper
            if (clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs.ContainsKey(ChannelName))
            {
                clusterServiceLocator._WS_Handler_Channels.CHAT_CHANNELs[ChannelName].SetModeration(client.Character);
            }
        }
    }
}