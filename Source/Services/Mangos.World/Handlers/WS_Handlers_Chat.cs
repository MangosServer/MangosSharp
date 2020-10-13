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

using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Handlers
{
	public class WS_Handlers_Chat
	{
		public byte GetChatFlag(WS_PlayerData.CharacterObject objCharacter)
		{
			if (objCharacter.GM)
			{
				return 3;
			}
			if (objCharacter.AFK)
			{
				return 1;
			}
			if (objCharacter.DND)
			{
				return 2;
			}
			return 0;
		}

		public void On_CMSG_MESSAGECHAT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MESSAGECHAT", client.IP, client.Port);
			if (checked(packet.Data.Length - 1) < 14)
			{
				return;
			}
			packet.GetInt16();
			ChatMsg msgType = (ChatMsg)packet.GetInt32();
			LANGUAGES msgLanguage = (LANGUAGES)packet.GetInt32();
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MESSAGECHAT [{2}:{3}]", client.IP, client.Port, msgType, msgLanguage);
			if (client.Character.Spell_Language != (LANGUAGES)(-1))
			{
				msgLanguage = client.Character.Spell_Language;
			}
			switch (msgType)
			{
			case ChatMsg.CHAT_MSG_SAY:
			case ChatMsg.CHAT_MSG_YELL:
			case ChatMsg.CHAT_MSG_WHISPER:
			case ChatMsg.CHAT_MSG_EMOTE:
			{
				string Message3 = packet.GetString();
				if (Message3.StartsWith(WorldServiceLocator._ConfigurationProvider.GetConfiguration().CommandCharacter) && client.Character.Access > AccessLevel.Player)
				{
					Message3 = Message3.Remove(0, 1);
					Packets.PacketClass toCommand = WorldServiceLocator._Functions.BuildChatMessage(2147483647uL, Message3, ChatMsg.CHAT_MSG_SYSTEM, LANGUAGES.LANG_GLOBAL, 0);
					try
					{
						client.Send(ref toCommand);
					}
					finally
					{
						toCommand.Dispose();
					}
					WorldServiceLocator._WS_Commands.OnCommand(ref client, Message3);
				}
				else
				{
					client.Character.SendChatMessage(ref client.Character, Message3, msgType, (int)msgLanguage, "", SendToMe: true);
				}
				break;
			}
			case ChatMsg.CHAT_MSG_AFK:
			{
				string Message = packet.GetString();
				if ((Operators.CompareString(Message, "", TextCompare: false) == 0 || !client.Character.AFK) && !client.Character.IsInCombat)
				{
					client.Character.AFK = !client.Character.AFK;
					if (client.Character.AFK && client.Character.DND)
					{
						client.Character.DND = false;
					}
					client.Character.SetUpdateFlag(190, (int)client.Character.cPlayerFlags);
					client.Character.SendCharacterUpdate();
				}
				break;
			}
			case ChatMsg.CHAT_MSG_DND:
			{
				string Message2 = packet.GetString();
				if (Operators.CompareString(Message2, "", TextCompare: false) == 0 || !client.Character.DND)
				{
					client.Character.DND = !client.Character.DND;
					if (client.Character.DND && client.Character.AFK)
					{
						client.Character.AFK = false;
					}
					client.Character.SetUpdateFlag(190, (int)client.Character.cPlayerFlags);
					client.Character.SendCharacterUpdate();
				}
				break;
			}
			case ChatMsg.CHAT_MSG_PARTY:
			case ChatMsg.CHAT_MSG_RAID:
			case ChatMsg.CHAT_MSG_CHANNEL:
			case ChatMsg.CHAT_MSG_RAID_LEADER:
			case ChatMsg.CHAT_MSG_RAID_WARNING:
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "This chat message type should not be here!");
				break;
			default:
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unknown chat message [msgType={2}, msgLanguage={3}]", client.IP, client.Port, msgType, msgLanguage);
				WorldServiceLocator._Packets.DumpPacket(packet.Data, client);
				break;
			}
		}
	}
}
