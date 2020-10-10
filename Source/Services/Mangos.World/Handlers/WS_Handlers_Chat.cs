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

using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Server;

namespace Mangos.World.Handlers
{
    public class WS_Handlers_Chat
    {
        public byte GetChatFlag(WS_PlayerData.CharacterObject objCharacter)
        {
            if (objCharacter.GM)
            {
                return (byte)ChatFlag.FLAGS_GM;
            }
            else if (objCharacter.AFK)
            {
                return (byte)ChatFlag.FLAGS_AFK;
            }
            else if (objCharacter.DND)
            {
                return (byte)ChatFlag.FLAGS_DND;
            }
            else
            {
                return 0;
            }
        }

        public void On_CMSG_MESSAGECHAT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MESSAGECHAT", client.IP, client.Port);
            if (packet.Data.Length - 1 < 14)
                return;
            packet.GetInt16();
            ChatMsg msgType = (ChatMsg)packet.GetInt32();
            LANGUAGES msgLanguage = (LANGUAGES)packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MESSAGECHAT [{2}:{3}]", client.IP, client.Port, msgType, msgLanguage);

            // TODO: Check if we really are able to speak this language!

            // DONE: Changing language
            if (client.Character.Spell_Language != -1)
                msgLanguage = client.Character.Spell_Language;
            switch (msgType)
            {
                case var @case when @case == ChatMsg.CHAT_MSG_SAY:
                case var case1 when case1 == ChatMsg.CHAT_MSG_YELL:
                case var case2 when case2 == ChatMsg.CHAT_MSG_EMOTE:
                case var case3 when case3 == ChatMsg.CHAT_MSG_WHISPER:
                    {
                        string Message = packet.GetString();
                        // Handle admin/gm commands
                        if (Message.StartsWith(WorldServiceLocator._WorldServer.Config.CommandCharacter) && client.Character.Access > AccessLevel.Player)
                        {
                            Message = Message.Remove(0, 1); // Remove Command Start Character From Message
                            var toCommand = WorldServiceLocator._Functions.BuildChatMessage(WS_Commands.SystemGUID, Message, ChatMsg.CHAT_MSG_SYSTEM, LANGUAGES.LANG_UNIVERSAL);
                            try
                            {
                                client.Send(toCommand);
                            }
                            finally
                            {
                                toCommand.Dispose();
                            }

                            WorldServiceLocator._WS_Commands.OnCommand(ref client, Message);
                            return;
                        }
                        else
                        {
                            client.Character.SendChatMessage(ref client.Character, Message, msgType, (int)msgLanguage, "", true);
                        }

                        break;
                    }

                case var case4 when case4 == ChatMsg.CHAT_MSG_AFK:
                    {
                        string Message = packet.GetString();
                        if ((string.IsNullOrEmpty(Message) || client.Character.AFK == false) && client.Character.IsInCombat == false)
                        {
                            client.Character.AFK = !client.Character.AFK;
                            if (client.Character.AFK && client.Character.DND)
                            {
                                client.Character.DND = false;
                            }

                            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FLAGS, client.Character.cPlayerFlags);
                            client.Character.SendCharacterUpdate();
                        }

                        break;
                    }

                case var case5 when case5 == ChatMsg.CHAT_MSG_DND:
                    {
                        string Message = packet.GetString();
                        if (string.IsNullOrEmpty(Message) || client.Character.DND == false)
                        {
                            client.Character.DND = !client.Character.DND;
                            if (client.Character.DND && client.Character.AFK)
                            {
                                client.Character.AFK = false;
                            }

                            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FLAGS, client.Character.cPlayerFlags);
                            client.Character.SendCharacterUpdate();
                        }

                        break;
                    }

                case var case6 when case6 == ChatMsg.CHAT_MSG_CHANNEL:
                case var case7 when case7 == ChatMsg.CHAT_MSG_PARTY:
                case var case8 when case8 == ChatMsg.CHAT_MSG_RAID:
                case var case9 when case9 == ChatMsg.CHAT_MSG_RAID_LEADER:
                case var case10 when case10 == ChatMsg.CHAT_MSG_RAID_WARNING:
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "This chat message type should not be here!");
                        break;
                    }

                default:
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unknown chat message [msgType={2}, msgLanguage={3}]", client.IP, client.Port, msgType, msgLanguage);
                        WorldServiceLocator._Packets.DumpPacket(packet.Data, ref client);
                        break;
                    }
            }
        }
    }
}