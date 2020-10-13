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

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Server;
using Mangos.Common;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Guild;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Cluster
{
    public class WC_Guild
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<uint, Guild> GUILDs = new Dictionary<uint, Guild>();

        public class Guild : IDisposable
        {
            public uint ID;
            public string Name;
            public ulong Leader;
            public string Motd;
            public string Info;
            public List<ulong> Members = new List<ulong>();
            public string[] Ranks = new string[10];
            public uint[] RankRights = new uint[10];
            public byte EmblemStyle;
            public byte EmblemColor;
            public byte BorderStyle;
            public byte BorderColor;
            public byte BackgroundColor;
            public short cYear;
            public byte cMonth;
            public byte cDay;

            public Guild(uint guildId)
            {
                ID = guildId;
                var mySqlQuery = new DataTable();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query("SELECT * FROM guilds WHERE guild_id = " + ID + ";", ref mySqlQuery);
                if (mySqlQuery.Rows.Count == 0)
                    throw new ApplicationException("GuildID " + ID + " not found in database.");
                var guildInfo = mySqlQuery.Rows[0];
                Name = guildInfo.As<string>("guild_name");
                Leader = guildInfo.As<ulong>("guild_leader");
                Motd = guildInfo.As<string>("guild_MOTD");
                EmblemStyle = guildInfo.As<byte>("guild_tEmblemStyle");
                EmblemColor = guildInfo.As<byte>("guild_tEmblemColor");
                BorderStyle = guildInfo.As<byte>("guild_tBorderStyle");
                BorderColor = guildInfo.As<byte>("guild_tBorderColor");
                BackgroundColor = guildInfo.As<byte>("guild_tBackgroundColor");
                cYear = Conversions.ToShort(guildInfo["guild_cYear"]);
                cMonth = guildInfo.As<byte>("guild_cMonth");
                cDay = guildInfo.As<byte>("guild_cDay");
                for (int i = 0; i <= 9; i++)
                {
                    Ranks[i] = guildInfo.As<string>("guild_rank" + i);
                    RankRights[i] = guildInfo.As<uint>("guild_rank" + i + "_Rights");
                }

                mySqlQuery.Clear();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query("SELECT char_guid FROM characters WHERE char_guildId = " + ID + ";", ref mySqlQuery);
                foreach (DataRow row in mySqlQuery.Rows)
                    Members.Add(row.As<ulong>("char_guid"));
                ClusterServiceLocator._WC_Guild.GUILDs.Add(ID, this);
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    ClusterServiceLocator._WC_Guild.GUILDs.Remove(ID);
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
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        // Basic Guild Framework
        public void AddCharacterToGuild(WcHandlerCharacter.CharacterObject objCharacter, int guildId, int guildRank = 4)
        {
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = {2}, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", guildId, objCharacter.Guid, guildRank));
            if (GUILDs.ContainsKey((uint)guildId) == false)
            {
                var tmpGuild = new Guild((uint)guildId);
                GUILDs.Add((uint)guildId, tmpGuild);
            }

            objCharacter.Guild = GUILDs[(uint)guildId];
            objCharacter.Guild.Members.Add(objCharacter.Guid);
            objCharacter.GuildRank = (byte)guildRank;
            objCharacter.SendGuildUpdate();
        }

        public void AddCharacterToGuild(ulong guid, int guildId, int guildRank = 4)
        {
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = {2}, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", guildId, guid, guildRank));
        }

        public void RemoveCharacterFromGuild(WcHandlerCharacter.CharacterObject objCharacter)
        {
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = 0, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", 0, objCharacter.Guid));
            objCharacter.Guild.Members.Remove(objCharacter.Guid);
            objCharacter.Guild = null;
            objCharacter.GuildRank = 0;
            objCharacter.SendGuildUpdate();
        }

        public void RemoveCharacterFromGuild(ulong guid)
        {
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("UPDATE characters SET char_guildId = {0}, char_guildRank = 0, char_guildOffNote = '', char_guildPNote = '' WHERE char_guid = {1};", 0, guid));
        }

        public void BroadcastChatMessageGuild(WcHandlerCharacter.CharacterObject sender, string message, LANGUAGES language, int guildId)
        {
            // DONE: Check for guild member
            if (!sender.IsInGuild)
            {
                SendGuildResult(sender.Client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
                return;
            }

            // DONE: Check for rights to speak
            if (!sender.IsGuildRightSet(GuildRankRights.GR_RIGHT_GCHATSPEAK))
            {
                SendGuildResult(sender.Client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
                return;
            }

            // DONE: Build packet
            var packet = ClusterServiceLocator._Functions.BuildChatMessage(sender.Guid, message, ChatMsg.CHAT_MSG_GUILD, language, (byte)sender.ChatFlag);

            // DONE: Send message to everyone
            var tmpArray = sender.Guild.Members.ToArray();
            foreach (ulong member in tmpArray)
            {
                if (ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(member))
                {
                    if (ClusterServiceLocator._WorldCluster.CHARACTERs[member].IsGuildRightSet(GuildRankRights.GR_RIGHT_GCHATLISTEN))
                    {
                        ClusterServiceLocator._WorldCluster.CHARACTERs[member].Client.SendMultiplyPackets(packet);
                    }
                }
            }

            packet.Dispose();
        }

        public void BroadcastChatMessageOfficer(WcHandlerCharacter.CharacterObject sender, string message, LANGUAGES language, int guildId)
        {
            // DONE: Check for guild member
            if (!sender.IsInGuild)
            {
                SendGuildResult(sender.Client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PLAYER_NOT_IN_GUILD);
                return;
            }

            // DONE: Check for rights to speak
            if (!sender.IsGuildRightSet(GuildRankRights.GR_RIGHT_OFFCHATSPEAK))
            {
                SendGuildResult(sender.Client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_PERMISSIONS);
                return;
            }

            // DONE: Build packet
            var packet = ClusterServiceLocator._Functions.BuildChatMessage(sender.Guid, message, ChatMsg.CHAT_MSG_OFFICER, language, (byte)sender.ChatFlag);

            // DONE: Send message to everyone
            var tmpArray = sender.Guild.Members.ToArray();
            foreach (ulong member in tmpArray)
            {
                if (ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(member))
                {
                    if (ClusterServiceLocator._WorldCluster.CHARACTERs[member].IsGuildRightSet(GuildRankRights.GR_RIGHT_OFFCHATLISTEN))
                    {
                        ClusterServiceLocator._WorldCluster.CHARACTERs[member].Client.SendMultiplyPackets(packet);
                    }
                }
            }

            packet.Dispose();
        }

        public void SendGuildQuery(WC_Network.ClientClass client, uint guildId)
        {
            if (guildId == 0L)
                return;
            // WARNING: This opcode is used also in character enum, so there must not be used any references to CharacterObject, only ClientClass

            // DONE: Load the guild if it doesn't exist in the memory
            if (GUILDs.ContainsKey(guildId) == false)
            {
                var tmpGuild = new Guild(guildId);
                GUILDs.Add(guildId, tmpGuild);
            }

            var response = new Packets.PacketClass(OPCODES.SMSG_GUILD_QUERY_RESPONSE);
            response.AddUInt32(guildId);
            response.AddString(GUILDs[guildId].Name);
            for (int i = 0; i <= 9; i++)
                response.AddString(GUILDs[guildId].Ranks[i]);
            response.AddInt32(GUILDs[guildId].EmblemStyle);
            response.AddInt32(GUILDs[guildId].EmblemColor);
            response.AddInt32(GUILDs[guildId].BorderStyle);
            response.AddInt32(GUILDs[guildId].BorderColor);
            response.AddInt32(GUILDs[guildId].BackgroundColor);
            response.AddInt32(0);
            client.Send(response);
            response.Dispose();
        }

        public void SendGuildRoster(WcHandlerCharacter.CharacterObject objCharacter)
        {
            if (!objCharacter.IsInGuild)
                return;

            // DONE: Count the ranks
            byte guildRanksCount = 0;
            for (int i = 0; i <= 9; i++)
            {
                if (!string.IsNullOrEmpty(objCharacter.Guild.Ranks[i]))
                    guildRanksCount = (byte)(guildRanksCount + 1);
            }

            // DONE: Count the members
            var Members = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query("SELECT char_online, char_guid, char_name, char_class, char_level, char_zone_id, char_logouttime, char_guildRank, char_guildPNote, char_guildOffNote FROM characters WHERE char_guildId = " + objCharacter.Guild.ID + ";", ref Members);
            var response = new Packets.PacketClass(OPCODES.SMSG_GUILD_ROSTER);
            response.AddInt32(Members.Rows.Count);
            response.AddString(objCharacter.Guild.Motd);
            response.AddString(objCharacter.Guild.Info);
            response.AddInt32(guildRanksCount);
            for (int i = 0; i <= 9; i++)
            {
                if (!string.IsNullOrEmpty(objCharacter.Guild.Ranks[i]))
                {
                    response.AddUInt32(objCharacter.Guild.RankRights[i]);
                }
            }

            bool Officer = objCharacter.IsGuildRightSet(GuildRankRights.GR_RIGHT_VIEWOFFNOTE);
            for (int i = 0, loopTo = Members.Rows.Count - 1; i <= loopTo; i++)
            {
                if (Conversions.ToByte(Members.Rows[i]["char_online"]) == 1)
                {
                    response.AddUInt64(Conversions.ToULong(Members.Rows[i]["char_guid"]));
                    response.AddInt8(1);                         // OnlineFlag
                    response.AddString(Conversions.ToString(Members.Rows[i]["char_name"]));
                    response.AddInt32(Conversions.ToInteger(Members.Rows[i]["char_guildRank"]));
                    response.AddInt8(Conversions.ToByte(Members.Rows[i]["char_level"]));
                    response.AddInt8(Conversions.ToByte(Members.Rows[i]["char_class"]));
                    response.AddInt32(Conversions.ToInteger(Members.Rows[i]["char_zone_id"]));
                    response.AddString(Conversions.ToString(Members.Rows[i]["char_guildPNote"]));
                    if (Officer)
                    {
                        response.AddString(Conversions.ToString(Members.Rows[i]["char_guildOffNote"]));
                    }
                    else
                    {
                        response.AddInt8(0);
                    }
                }
                else
                {
                    response.AddUInt64(Conversions.ToULong(Members.Rows[i]["char_guid"]));
                    response.AddInt8(0);                         // OfflineFlag
                    response.AddString(Conversions.ToString(Members.Rows[i]["char_name"]));
                    response.AddInt32(Conversions.ToInteger(Members.Rows[i]["char_guildRank"]));
                    response.AddInt8(Conversions.ToByte(Members.Rows[i]["char_level"]));
                    response.AddInt8(Conversions.ToByte(Members.Rows[i]["char_class"]));
                    response.AddInt32(Conversions.ToInteger(Members.Rows[i]["char_zone_id"]));
                    // 0 = < 1 hour / 0.1 = 2.4 hours / 1 = 24 hours (1 day)
                    // (Time logged out / 86400) = Days offline
                    float DaysOffline = (float)((ClusterServiceLocator._Functions.GetTimestamp(DateAndTime.Now) - Conversions.ToUInteger(Members.Rows[i]["char_logouttime"])) / (double)DateInterval.Day);
                    response.AddSingle(DaysOffline); // Days offline
                    response.AddString(Conversions.ToString(Members.Rows[i]["char_guildPNote"]));
                    if (Officer)
                    {
                        response.AddString(Conversions.ToString(Members.Rows[i]["char_guildOffNote"]));
                    }
                    else
                    {
                        response.AddInt8(0);
                    }
                }
            }

            objCharacter.Client.Send(response);
            response.Dispose();
        }

        public void SendGuildResult(WC_Network.ClientClass client, GuildCommand command, GuildError result, string text = "")
        {
            var response = new Packets.PacketClass(OPCODES.SMSG_GUILD_COMMAND_RESULT);
            response.AddInt32((int)command);
            response.AddString(text);
            response.AddInt32((int)result);
            client.Send(response);
            response.Dispose();
        }

        public void NotifyGuildStatus(WcHandlerCharacter.CharacterObject objCharacter, GuildEvent status)
        {
            if (objCharacter.Guild is null)
                return;
            var statuspacket = new Packets.PacketClass(OPCODES.SMSG_GUILD_EVENT);
            statuspacket.AddInt8((byte)status);
            statuspacket.AddInt8(1);
            statuspacket.AddString(objCharacter.Name);
            statuspacket.AddInt8(0);
            statuspacket.AddInt8(0);
            statuspacket.AddInt8(0);
            BroadcastToGuild(statuspacket, objCharacter.Guild, objCharacter.Guid);
            statuspacket.Dispose();
        }

        public void BroadcastToGuild(Packets.PacketClass packet, Guild guild, [Optional, DefaultParameterValue(0UL)] ulong notTo)
        {
            var tmpArray = guild.Members.ToArray();
            foreach (ulong member in tmpArray)
            {
                if (member == notTo)
                    continue;
                if (ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(member))
                {
                    ClusterServiceLocator._WorldCluster.CHARACTERs[member].Client.SendMultiplyPackets(packet);
                }
            }
        }

        // Members Options
        public void SendGuildMOTD(WcHandlerCharacter.CharacterObject objCharacter)
        {
            if (objCharacter.IsInGuild)
            {
                if (!string.IsNullOrEmpty(objCharacter.Guild.Motd))
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_GUILD_EVENT);
                    response.AddInt8((byte)GuildEvent.MOTD);
                    response.AddInt8(1);
                    response.AddString(objCharacter.Guild.Motd);
                    objCharacter.Client.Send(response);
                    response.Dispose();
                }
            }
        }
    }
}