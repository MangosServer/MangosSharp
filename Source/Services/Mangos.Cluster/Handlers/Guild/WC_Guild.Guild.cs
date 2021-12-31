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

using Mangos.Common.Legacy;
using System;
using System.Collections.Generic;
using System.Data;

namespace Mangos.Cluster.Handlers.Guild;

public partial class WcGuild
{
    public class Guild : IDisposable
    {
        private readonly ClusterServiceLocator _clusterServiceLocator;

        public Guild(ClusterServiceLocator clusterServiceLocator)
        {
            _clusterServiceLocator = clusterServiceLocator;
        }

        public uint Id;
        public string Name;
        public ulong Leader;
        public string Motd;
        public string Info;
        public List<ulong> Members = new();
        public string[] Ranks = new string[10];
        public uint[] RankRights = new uint[10];
        public byte EmblemStyle;
        public byte EmblemColor;
        public byte BorderStyle;
        public byte BorderColor;
        public byte BackgroundColor;
        public short CYear;
        public byte CMonth;
        public byte CDay;

        public Guild(uint guildId)
        {
            Id = guildId;
            DataTable mySqlQuery = new();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query("SELECT * FROM guilds WHERE guild_id = " + Id + ";", ref mySqlQuery);
            if (mySqlQuery.Rows.Count == 0)
            {
                throw new ApplicationException("GuildID " + Id + " not found in database.");
            }

            var guildInfo = mySqlQuery.Rows[0];
            Name = guildInfo.As<string>("guild_name");
            Leader = guildInfo.As<ulong>("guild_leader");
            Motd = guildInfo.As<string>("guild_MOTD");
            EmblemStyle = guildInfo.As<byte>("guild_tEmblemStyle");
            EmblemColor = guildInfo.As<byte>("guild_tEmblemColor");
            BorderStyle = guildInfo.As<byte>("guild_tBorderStyle");
            BorderColor = guildInfo.As<byte>("guild_tBorderColor");
            BackgroundColor = guildInfo.As<byte>("guild_tBackgroundColor");
            CYear = guildInfo.As<short>("guild_cYear");
            CMonth = guildInfo.As<byte>("guild_cMonth");
            CDay = guildInfo.As<byte>("guild_cDay");
            for (var i = 0; i <= 9; i++)
            {
                Ranks[i] = guildInfo.As<string>("guild_rank" + i);
                RankRights[i] = guildInfo.As<uint>("guild_rank" + i + "_Rights");
            }

            mySqlQuery.Clear();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query("SELECT char_guid FROM characters WHERE char_guildId = " + Id + ";", ref mySqlQuery);
            foreach (DataRow memberInfo in mySqlQuery.Rows)
            {
                Members.Add(guildInfo.As<ulong>("char_guid"));
            }

            _clusterServiceLocator.WcGuild.GuilDs.Add(Id, this);
        }

        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                _clusterServiceLocator.WcGuild.GuilDs.Remove(Id);
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
    }
}
