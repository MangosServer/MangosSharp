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

using System;

namespace Mangos.WoWFakeClient;

public static class WS_Chat
{
    public enum LANGUAGES
    {
        LANG_GLOBAL = 0,
        LANG_UNIVERSAL = 0,
        LANG_ORCISH = 1,
        LANG_DARNASSIAN = 2,
        LANG_TAURAHE = 3,
        LANG_DWARVISH = 6,
        LANG_COMMON = 7,
        LANG_DEMONIC = 8,
        LANG_TITAN = 9,
        LANG_THALASSIAN = 10,
        LANG_DRACONIC = 11,
        LANG_KALIMAG = 12,
        LANG_GNOMISH = 13,
        LANG_TROLL = 14,
        LANG_GUTTERSPEAK = 33
    }

    public enum ChatMsg
    {
        CHAT_MSG_SAY = 0x0,
        CHAT_MSG_PARTY = 0x1,
        CHAT_MSG_RAID = 0x2,
        CHAT_MSG_GUILD = 0x3,
        CHAT_MSG_OFFICER = 0x4,
        CHAT_MSG_YELL = 0x5,
        CHAT_MSG_WHISPER = 0x6,
        CHAT_MSG_WHISPER_INFORM = 0x7,
        CHAT_MSG_EMOTE = 0x8,
        CHAT_MSG_TEXT_EMOTE = 0x9,
        CHAT_MSG_SYSTEM = 0xA,
        CHAT_MSG_MONSTER_SAY = 0xB,
        CHAT_MSG_MONSTER_YELL = 0xC,
        CHAT_MSG_MONSTER_EMOTE = 0xD,
        CHAT_MSG_CHANNEL = 0xE,
        CHAT_MSG_CHANNEL_JOIN = 0xF,
        CHAT_MSG_CHANNEL_LEAVE = 0x10,
        CHAT_MSG_CHANNEL_LIST = 0x11,
        CHAT_MSG_CHANNEL_NOTICE = 0x12,
        CHAT_MSG_CHANNEL_NOTICE_USER = 0x13,
        CHAT_MSG_AFK = 0x14,
        CHAT_MSG_DND = 0x15,
        CHAT_MSG_IGNORED = 0x16,
        CHAT_MSG_SKILL = 0x17,
        CHAT_MSG_LOOT = 0x18,
        CHAT_MSG_RAID_LEADER = 0x57,
        CHAT_MSG_RAID_WARNING = 0x58
    }

    public static void SendChatMessage(string Message)
    {
        Packets.PacketClass target = new(OPCODES.CMSG_SET_SELECTION);
        target.AddUInt64(Worldserver.CharacterGUID);
        Worldserver.Send(target);
        target.Dispose();
        Packets.PacketClass packet = new(OPCODES.CMSG_MESSAGECHAT);
        packet.AddInt32((int)ChatMsg.CHAT_MSG_WHISPER); // Whisper
        packet.AddInt32((int)LANGUAGES.LANG_GLOBAL); // Global
        packet.AddString("Warden");
        packet.AddString(Message);
        Worldserver.Send(packet);
        packet.Dispose();
    }

    public static void On_SMSG_MESSAGECHAT(ref Packets.PacketClass Packet)
    {
        ChatMsg msgType = (ChatMsg)Packet.GetInt8();
        LANGUAGES msgLanguage = (LANGUAGES)Packet.GetInt32();
        switch (msgType)
        {
            case ChatMsg.CHAT_MSG_WHISPER:
                {
                    var SenderGuid = (ulong)Packet.GetInt64();
                    var ByteCount = Packet.GetInt32();
                    var Message = Packet.GetString();
                    var ChatFlag = Packet.GetInt8();
                    Console.WriteLine("Answer: " + Message);
                    break;
                }
        }
    }
}
