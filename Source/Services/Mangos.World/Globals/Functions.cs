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

using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Mangos.World.Network;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Mangos.World.Globals;

public class Functions
{
    public enum PartyMemberStatsStatus : byte
    {
        STATUS_OFFLINE = 0,
        STATUS_ONLINE = 1,
        STATUS_PVP = 2,
        STATUS_CORPSE = 8,
        STATUS_DEAD = 0x10
    }

    public enum PartyMemberStatsBits : byte
    {
        FIELD_STATUS,
        FIELD_LIFE_CURRENT,
        FIElD_LIFE_MAX,
        FIELD_MANA_TYPE,
        FIELD_MANA_CURRENT,
        FIELD_MANA_MAX,
        FIELD_LEVEL,
        FIELD_ZONEID,
        FIELD_POSXPOSY
    }

    public enum PartyMemberStatsFlag : uint
    {
        GROUP_UPDATE_FLAG_NONE = 0u,
        GROUP_UPDATE_FLAG_STATUS = 1u,
        GROUP_UPDATE_FLAG_CUR_HP = 2u,
        GROUP_UPDATE_FLAG_MAX_HP = 4u,
        GROUP_UPDATE_FLAG_POWER_TYPE = 8u,
        GROUP_UPDATE_FLAG_CUR_POWER = 0x10u,
        GROUP_UPDATE_FLAG_MAX_POWER = 0x20u,
        GROUP_UPDATE_FLAG_LEVEL = 0x40u,
        GROUP_UPDATE_FLAG_ZONE = 0x80u,
        GROUP_UPDATE_FLAG_POSITION = 0x100u,
        GROUP_UPDATE_FLAG_AURAS = 0x200u,
        GROUP_UPDATE_FLAG_PET_GUID = 0x400u,
        GROUP_UPDATE_FLAG_PET_NAME = 0x800u,
        GROUP_UPDATE_FLAG_PET_MODEL_ID = 0x1000u,
        GROUP_UPDATE_FLAG_PET_CUR_HP = 0x2000u,
        GROUP_UPDATE_FLAG_PET_MAX_HP = 0x4000u,
        GROUP_UPDATE_FLAG_PET_POWER_TYPE = 0x8000u,
        GROUP_UPDATE_FLAG_PET_CUR_POWER = 0x10000u,
        GROUP_UPDATE_FLAG_PET_MAX_POWER = 0x20000u,
        GROUP_UPDATE_FLAG_PET_AURAS = 0x40000u,
        GROUP_UPDATE_PET = 523264u,
        GROUP_UPDATE_FULL = 1015u,
        GROUP_UPDATE_FULL_PET = 524279u,
        GROUP_UPDATE_FULL_REQUEST_REPLY = 2147224575u
    }

    private readonly Regex Regex_AZ;

    private readonly Regex Regex_Guild;

    public Functions()
    {
        Regex_AZ = new Regex("^[a-zA-Z]+$");
        Regex_Guild = new Regex("^[a-z A-Z]+$");
    }

    public int ToInteger(bool Value)
    {
        return Value ? 1 : 0;
    }

    public string ToHex(byte[] bBytes, int start = 0)
    {
        if (bBytes.Length == 0)
        {
            return "''";
        }
        var tmpStr = "0x";
        checked
        {
            var num = bBytes.Length - 1;
            for (var i = start; i <= num; i++)
            {
                tmpStr = (bBytes[i] >= 16) ? (tmpStr + Conversion.Hex(bBytes[i])) : (tmpStr + "0" + Conversion.Hex(bBytes[i]));
            }
            return tmpStr;
        }
    }

    public char[] ByteToCharArray(byte[] bBytes)
    {
        if (bBytes.Length == 0)
        {
            return Array.Empty<char>();
        }
        checked
        {
            var bChar = new char[bBytes.Length - 1 + 1];
            var num = bBytes.Length - 1;
            for (var i = 0; i <= num; i++)
            {
                bChar[i] = Strings.Chr(bBytes[i]);
            }
            return bChar;
        }
    }

    public int[] ByteToIntArray(byte[] bBytes)
    {
        if (bBytes.Length == 0)
        {
            return Array.Empty<int>();
        }
        checked
        {
            var bInt = new int[(checked(bBytes.Length - 1) / 4) + 1];
            var num = bBytes.Length - 1;
            for (var i = 0; i <= num; i += 4)
            {
                bInt[i / 4] = BitConverter.ToInt32(bBytes, i);
            }
            return bInt;
        }
    }

    public byte[] IntToByteArray(int[] bInt)
    {
        if (bInt.Length == 0)
        {
            return Array.Empty<byte>();
        }
        checked
        {
            var bBytes = new byte[(bInt.Length * 4) - 1 + 1];
            var num = bInt.Length - 1;
            for (var i = 0; i <= num; i++)
            {
                var tmpBytes = BitConverter.GetBytes(bInt[i]);
                Array.Copy(tmpBytes, 0, bBytes, i * 4, 4);
            }
            return bBytes;
        }
    }

    public byte[] Concat(byte[] a, byte[] b)
    {
        checked
        {
            var buffer1 = new byte[a.Length + b.Length - 1 + 1];
            var num3 = a.Length - 1;
            for (var num1 = 0; num1 <= num3; num1++)
            {
                buffer1[num1] = a[num1];
            }
            var num4 = b.Length - 1;
            for (var num2 = 0; num2 <= num4; num2++)
            {
                buffer1[num2 + a.Length] = b[num2];
            }
            return buffer1;
        }
    }

    public bool HaveFlag(uint value, byte flagPos)
    {
        checked
        {
            value >>= flagPos;
            value = (uint)(value % 2L);
        }
        return (ulong)value == 1;
    }

    public bool HaveFlags(int value, int flags)
    {
        return (value & flags) == flags;
    }

    public void SetFlag(ref uint value, byte flagPos, bool flagValue)
    {
        if (flagValue)
        {
            value |= (uint)(1 << flagPos);
        }
        else
        {
            value &= (uint)((0 << flagPos) & -1);
        }
    }

    public DateTime GetNextDay(DayOfWeek iDay, int Hour = 0)
    {
        checked
        {
            var iDiff = iDay - DateAndTime.Today.DayOfWeek;
            if (iDiff <= 0)
            {
                iDiff += 7;
            }
            return DateAndTime.Today.AddDays(iDiff).AddHours(Hour);
        }
    }

    public DateTime GetNextDate(int Days, int Hours = 0)
    {
        return DateAndTime.Today.AddDays(Days).AddHours(Hours);
    }

    public uint GetTimestamp(DateTime fromDateTime)
    {
        DateTime startDate = new(621355968000000000L);
        return checked((uint)Math.Round(Math.Abs(fromDateTime.Subtract(startDate).TotalSeconds)));
    }

    public DateTime GetDateFromTimestamp(uint unixTimestamp)
    {
        DateTime startDate = new(621355968000000000L);
        if ((ulong)unixTimestamp == 0)
        {
            return startDate;
        }
        TimeSpan timeSpan = new(0, 0, checked((int)unixTimestamp));
        return startDate.Add(timeSpan);
    }

    public string GetTimeLeftString(uint seconds)
    {
        return seconds switch
        {
            < 60 => Conversions.ToString(seconds) + "s",
            < 3600 => Conversions.ToString(seconds / 60L) + "m " + Conversions.ToString(seconds % 60L) + "s",
            _ => seconds < 86400L
            ? Conversions.ToString(seconds / 3600L) + "h " + Conversions.ToString(seconds / 60L % 60) + "m " + Conversions.ToString(seconds % 60L) + "s"
            : Conversions.ToString(seconds / 86400L) + "d " + Conversions.ToString(seconds / 3600L % 24) + "h " + Conversions.ToString(seconds / 60L % 60) + "m " + Conversions.ToString(seconds % 60L) + "s"
        };
    }

    public string EscapeString(string s)
    {
        return s.Replace("\"", "").Replace("'", "");
    }

    public string CapitalizeName(ref string Name)
    {
        return Name.Length > 1
            ? WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Strings.Left(Name, 1)) + WorldServiceLocator._CommonFunctions.LowercaseFirstLetter(Strings.Right(Name, checked(Name.Length - 1)))
            : WorldServiceLocator._CommonFunctions.UppercaseFirstLetter(Name);
    }

    public bool ValidateName(string strName)
    {
        return strName.Length is not < 2 and not > 16 && Regex_AZ.IsMatch(strName);
    }

    public bool ValidateGuildName(string strName)
    {
        return strName.Length is not < 2 and not > 16 && Regex_Guild.IsMatch(strName);
    }

    public string FixName(string strName)
    {
        return strName.Replace("\"", "'").Replace("<", "").Replace(">", "")
            .Replace("*", "")
            .Replace("/", "")
            .Replace("\\", "")
            .Replace(":", "")
            .Replace("|", "")
            .Replace("?", "");
    }

    public void RAND_bytes(ref byte[] bBytes, int length)
    {
        checked
        {
            if (length != 0)
            {
                bBytes = new byte[length - 1 + 1];
                Random rnd = new();
                var num = length - 1;
                for (var i = 0; i <= num && i != bBytes.Length; i++)
                {
                    bBytes[i] = (byte)rnd.Next(0, 256);
                }
            }
        }
    }

    public float MathLerp(float value1, float value2, float amount)
    {
        return value1 + ((value2 - value1) * amount);
    }

    public void Ban_Account(string Name, string Reason)
    {
        DataTable account = new();
        DataTable bannedAccount = new();
        WorldServiceLocator._WorldServer.AccountDatabase.Query($"SELECT id, username FROM account WHERE username = {Name};", ref account);
        switch (account.Rows.Count)
        {
            case > 0:
                {
                    var accID = Conversions.ToInteger(account.Rows[0]["id"]);
                    WorldServiceLocator._WorldServer.AccountDatabase.Query($"SELECT id, active FROM account_banned WHERE id = {accID};", ref bannedAccount);
                    switch (bannedAccount.Rows.Count)
                    {
                        case > 0:
                            WorldServiceLocator._WorldServer.AccountDatabase.Update("UPDATE account_banned SET active = 1 WHERE id = '" + Conversions.ToString(accID) + "';");
                            break;
                        default:
                            {
                                var tempBanDate = Strings.FormatDateTime(Conversions.ToDate(DateTime.Now.ToFileTimeUtc().ToString()), DateFormat.LongDate) + " " + Strings.FormatDateTime(Conversions.ToDate(DateTime.Now.ToFileTimeUtc().ToString()), DateFormat.LongTime);
                                WorldServiceLocator._WorldServer.AccountDatabase.Update(string.Format("INSERT INTO `account_banned` VALUES ('{0}', UNIX_TIMESTAMP('{1}'), UNIX_TIMESTAMP('{2}'), '{3}', '{4}', active = 1);", accID, tempBanDate, "0000-00-00 00:00:00", Name, Reason));
                                break;
                            }
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Account [{0}] banned by server. Reason: [{1}].", Name, Reason);
                    break;
                }

            default:
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Account [{0}] NOT Found in Database.", Name);
                break;
        }
    }

    public string GetClassName(ref int Classe)
    {
        return Classe switch
        {
            11 => "Druid",
            3 => "Hunter",
            8 => "Mage",
            2 => "Paladin",
            5 => "Priest",
            4 => "Rogue",
            7 => "Shaman",
            9 => "Warlock",
            1 => "Warrior",
            _ => "Unknown Class",
        };
    }

    public string GetRaceName(ref int Race)
    {
        return Race switch
        {
            3 => "Dwarf",
            7 => "Gnome",
            1 => "Human",
            4 => "Night Elf",
            2 => "Orc",
            6 => "Tauren",
            8 => "Troll",
            5 => "Undead",
            _ => "Unknown Race",
        };
    }

    public int GetRaceModel(Races Race, int Gender)
    {
        return checked(Race switch
        {
            Races.RACE_HUMAN => 49 + Gender,
            Races.RACE_ORC => 51 + Gender,
            Races.RACE_DWARF => 53 + Gender,
            Races.RACE_NIGHT_ELF => 55 + Gender,
            Races.RACE_UNDEAD => 57 + Gender,
            Races.RACE_TAUREN => 59 + Gender,
            Races.RACE_GNOME => 1563 + Gender,
            Races.RACE_TROLL => 1478 + Gender,
            _ => 16358,
        });
    }

    public bool GetCharacterSide(byte Race)
    {
        return Race switch
        {
            1 or 3 or 4 or 7 => false,
            _ => true,
        };
    }

    public bool IsContinentMap(int Map)
    {
        return (uint)Map <= 1u;
    }

    public string SetColor(string Message, byte Red, byte Green, byte Blue)
    {
        var SetColor = "|cFF";
        SetColor = (Red >= 16) ? (SetColor + Conversion.Hex(Red)) : (SetColor + "0" + Conversion.Hex(Red));
        SetColor = (Green >= 16) ? (SetColor + Conversion.Hex(Green)) : (SetColor + "0" + Conversion.Hex(Green));
        SetColor = (Blue >= 16) ? (SetColor + Conversion.Hex(Blue)) : (SetColor + "0" + Conversion.Hex(Blue));
        return SetColor + Message + "|r";
    }

    public bool RollChance(float Chance)
    {
        var nChance = checked((int)Math.Round(Chance * 100f));
        return WorldServiceLocator._WorldServer.Rnd.Next(1, 10001) <= nChance;
    }

    public void SendMessageMOTD(ref WS_Network.ClientClass client, string Message)
    {
        var packet = BuildChatMessage(0uL, Message, ChatMsg.CHAT_MSG_SYSTEM, LANGUAGES.LANG_GLOBAL);
        client.Send(ref packet);
    }

    public void SendMessageNotification(ref WS_Network.ClientClass client, string Message)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_NOTIFICATION);
        try
        {
            packet.AddString(Message);
            client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
    }

    public void SendMessageSystem(WS_Network.ClientClass objCharacter, string Message)
    {
        var packet = BuildChatMessage(0uL, Message, ChatMsg.CHAT_MSG_SYSTEM, LANGUAGES.LANG_GLOBAL, 0, "");
        try
        {
            objCharacter.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
    }

    public void Broadcast(string Message)
    {
        WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
        foreach (var Character in WorldServiceLocator._WorldServer.CHARACTERs)
        {
            if (Character.Value.client != null)
            {
                SendMessageSystem(Character.Value.client, "System Message: " + SetColor(Message, byte.MaxValue, 0, 0));
            }
        }
        WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
    }

    public void SendAccountMD5(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        var FoundData = false;
        Packets.PacketClass SMSG_ACCOUNT_DATA_TIMES = new(Opcodes.SMSG_ACCOUNT_DATA_MD5);
        try
        {
            var i = 0;
            do
            {
                if (!FoundData)
                {
                    SMSG_ACCOUNT_DATA_TIMES.AddInt64(0L);
                    SMSG_ACCOUNT_DATA_TIMES.AddInt64(0L);
                }
                i = checked(i + 1);
            }
            while (i <= 7);
            client.Send(ref SMSG_ACCOUNT_DATA_TIMES);
        }
        finally
        {
            SMSG_ACCOUNT_DATA_TIMES.Dispose();
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_ACCOUNT_DATA_MD5", client.IP, client.Port);
    }

    public void SendTriggerCinematic(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_TRIGGER_CINEMATIC);
        try
        {
            if (!WorldServiceLocator._WS_DBCDatabase.CharRaces.ContainsKey((int)Character.Race))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] SMSG_TRIGGER_CINEMATIC [Error: RACE={2} CLASS={3}]", client.IP, client.Port, Character.Race, Character.Classe);
                return;
            }
            packet.AddInt32(WorldServiceLocator._WS_DBCDatabase.CharRaces[(int)Character.Race].CinematicID);
            client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_TRIGGER_CINEMATIC", client.IP, client.Port);
    }

    public void SendTimeSyncReq(ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SendTimeSyncReq", client.IP, client.Port);
    }

    public void SendGameTime(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        Packets.PacketClass SMSG_LOGIN_SETTIMESPEED = new(Opcodes.SMSG_LOGIN_SETTIMESPEED);
        checked
        {
            try
            {
                var time = DateTime.Now;
                var Year = time.Year - 2000;
                var Month = time.Month - 1;
                var Day = time.Day - 1;
                var DayOfWeek = (int)time.DayOfWeek;
                var Hour = time.Hour;
                var Minute = time.Minute;
                SMSG_LOGIN_SETTIMESPEED.AddInt32(Minute + (Hour << 6) + (DayOfWeek << 11) + (Day << 14) + (Month << 20) + (Year << 24));
                SMSG_LOGIN_SETTIMESPEED.AddSingle(0.01666667f);
                client.Send(ref SMSG_LOGIN_SETTIMESPEED);
            }
            finally
            {
                SMSG_LOGIN_SETTIMESPEED.Dispose();
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_LOGIN_SETTIMESPEED", client.IP, client.Port);
        }
    }

    public void SendProficiency(ref WS_Network.ClientClass client, byte ProficiencyType, int ProficiencyFlags)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_SET_PROFICIENCY);
        try
        {
            packet.AddInt8(ProficiencyType);
            packet.AddInt32(ProficiencyFlags);
            client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_SET_PROFICIENCY", client.IP, client.Port);
    }

    public void SendCorpseReclaimDelay(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character, int Seconds = 30)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_CORPSE_RECLAIM_DELAY);
        try
        {
            packet.AddInt32(checked(Seconds * 1000));
            client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_CORPSE_RECLAIM_DELAY [{2}s]", client.IP, client.Port, Seconds);
    }

    public Packets.PacketClass BuildChatMessage(ulong SenderGUID, string Message, ChatMsg msgType, LANGUAGES msgLanguage, byte Flag = 0, string msgChannel = "Global")
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_MESSAGECHAT);
        try
        {
            packet.AddInt8(checked((byte)msgType));
            packet.AddInt32((int)msgLanguage);
            switch (msgType)
            {
                case ChatMsg.CHAT_MSG_CHANNEL:
                    packet.AddString(msgChannel);
                    packet.AddUInt32(0u);
                    packet.AddUInt64(SenderGUID);
                    break;

                case ChatMsg.CHAT_MSG_SAY:
                case ChatMsg.CHAT_MSG_PARTY:
                case ChatMsg.CHAT_MSG_YELL:
                    packet.AddUInt64(SenderGUID);
                    packet.AddUInt64(SenderGUID);
                    break;

                case ChatMsg.CHAT_MSG_RAID:
                case ChatMsg.CHAT_MSG_GUILD:
                case ChatMsg.CHAT_MSG_OFFICER:
                case ChatMsg.CHAT_MSG_WHISPER:
                case ChatMsg.CHAT_MSG_WHISPER_INFORM:
                case ChatMsg.CHAT_MSG_EMOTE:
                case ChatMsg.CHAT_MSG_SYSTEM:
                case ChatMsg.CHAT_MSG_AFK:
                case ChatMsg.CHAT_MSG_DND:
                case ChatMsg.CHAT_MSG_IGNORED:
                case ChatMsg.CHAT_MSG_SKILL:
                case ChatMsg.CHAT_MSG_RAID_LEADER:
                case ChatMsg.CHAT_MSG_RAID_WARNING:
                    packet.AddUInt64(SenderGUID);
                    break;

                case ChatMsg.CHAT_MSG_MONSTER_SAY:
                case ChatMsg.CHAT_MSG_MONSTER_YELL:
                case ChatMsg.CHAT_MSG_MONSTER_EMOTE:
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Use Creature.SendChatMessage() for this message type - {0}!", msgType);
                    break;

                default:
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Unknown chat message type - {0}!", msgType);
                    break;
            }
            packet.AddUInt32(checked((uint)(Encoding.UTF8.GetByteCount(Message) + 1)));
            packet.AddString(Message);
            packet.AddInt8(Flag);
        }
        catch (Exception ex)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "failed chat message type - {0}!", msgType, ex);
        }
        return packet;
    }

    public Packets.PacketClass BuildPartyMemberStatsOffline(ulong GUID)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_PARTY_MEMBER_STATS_FULL);
        packet.AddPackGUID(GUID);
        packet.AddUInt32(1u);
        packet.AddInt8(0);
        return packet;
    }
}
