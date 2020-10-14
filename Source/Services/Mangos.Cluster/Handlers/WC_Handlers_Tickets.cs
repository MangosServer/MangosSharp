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

using System.Data;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Server;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Cluster.Handlers
{
    public class WC_Handlers_Tickets
    {
        public void On_CMSG_BUG(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 14)
                return;
            packet.GetInt16();
            SuggestionType suggestion = (SuggestionType)packet.GetInt32();
            int cLength = packet.GetInt32();
            string cString = ClusterServiceLocator._Functions.EscapeString(packet.GetString());
            if (packet.Data.Length - 1 < 14 + cString.Length + 5)
                return;
            int tLength = packet.GetInt32();
            string tString = ClusterServiceLocator._Functions.EscapeString(packet.GetString());
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUG [2]", client.IP, client.Port, suggestion);
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "Bug report [{0}:{1} Lengths:{2}, {3}] " + cString + Constants.vbCrLf + tString, cLength.ToString(), tLength.ToString());
        }

        // ERR_TICKET_ALREADY_EXISTS
        // ERR_TICKET_CREATE_ERROR
        // ERR_TICKET_UPDATE_ERROR
        // ERR_TICKET_DB_ERROR
        // ERR_TICKET_NO_TEXT

        private enum GMTicketGetResult
        {
            GMTICKET_AVAILABLE = 6,
            GMTICKET_NOTICKET = 10
        }

        public void On_CMSG_GMTICKET_GETTICKET(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_GETTICKET", client.IP, client.Port);
            var SMSG_GMTICKET_GETTICKET = new Packets.PacketClass(Opcodes.SMSG_GMTICKET_GETTICKET);
            var MySQLResult = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT * FROM characters_tickets WHERE char_guid = {0};", client.Character.Guid), ref MySQLResult);
            if (MySQLResult.Rows.Count > 0)
            {
                SMSG_GMTICKET_GETTICKET.AddInt32((int)GMTicketGetResult.GMTICKET_AVAILABLE);
                SMSG_GMTICKET_GETTICKET.AddString(Conversions.ToString(MySQLResult.Rows[0]["ticket_text"]));
            }
            else
            {
                SMSG_GMTICKET_GETTICKET.AddInt32((int)GMTicketGetResult.GMTICKET_NOTICKET);
            }

            client.Send(SMSG_GMTICKET_GETTICKET);
            SMSG_GMTICKET_GETTICKET.Dispose();
            var SMSG_QUERY_TIME_RESPONSE = new Packets.PacketClass(Opcodes.SMSG_QUERY_TIME_RESPONSE);
            SMSG_QUERY_TIME_RESPONSE.AddInt32(ClusterServiceLocator._NativeMethods.timeGetTime("")); // GetTimestamp(Now))
            client.Send(SMSG_QUERY_TIME_RESPONSE);
            SMSG_QUERY_TIME_RESPONSE.Dispose();
        }

        private enum GMTicketCreateResult
        {
            GMTICKET_ALREADY_HAVE = 1,
            GMTICKET_CREATE_OK = 2
        }

        public void On_CMSG_GMTICKET_CREATE(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            uint ticket_map = packet.GetUInt32();
            float ticket_x = packet.GetFloat();
            float ticket_y = packet.GetFloat();
            float ticket_z = packet.GetFloat();
            string ticket_text = ClusterServiceLocator._Functions.EscapeString(packet.GetString());
            var MySQLResult = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT * FROM characters_tickets WHERE char_guid = {0};", client.Character.Guid), ref MySQLResult);
            var SMSG_GMTICKET_CREATE = new Packets.PacketClass(Opcodes.SMSG_GMTICKET_CREATE);
            if (MySQLResult.Rows.Count > 0)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_CREATE", client.IP, client.Port);
                SMSG_GMTICKET_CREATE.AddInt32((int)GMTicketCreateResult.GMTICKET_ALREADY_HAVE);
            }
            else
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_CREATE [{2}]", client.IP, client.Port, ticket_text);
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("INSERT INTO characters_tickets (char_guid, ticket_text, ticket_x, ticket_y, ticket_z, ticket_map) VALUES ({0} , \"{1}\", {2}, {3}, {4}, {5});", client.Character.Guid, ticket_text, Strings.Trim(Conversion.Str(ticket_x)), Strings.Trim(Conversion.Str(ticket_y)), Strings.Trim(Conversion.Str(ticket_z)), ticket_map));
                SMSG_GMTICKET_CREATE.AddInt32((int)GMTicketCreateResult.GMTICKET_CREATE_OK);
            }

            client.Send(SMSG_GMTICKET_CREATE);
            SMSG_GMTICKET_CREATE.Dispose();
        }

        private enum GMTicketSystemStatus
        {
            GMTICKET_SYSTEMSTATUS_ENABLED = 1,
            GMTICKET_SYSTEMSTATUS_DISABLED = 2,
            GMTICKET_SYSTEMSTATUS_SURVEY = 3
        }

        public void On_CMSG_GMTICKET_SYSTEMSTATUS(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_SYSTEMSTATUS", client.IP, client.Port);
            var SMSG_GMTICKET_SYSTEMSTATUS = new Packets.PacketClass(Opcodes.SMSG_GMTICKET_SYSTEMSTATUS);
            SMSG_GMTICKET_SYSTEMSTATUS.AddInt32((int)GMTicketSystemStatus.GMTICKET_SYSTEMSTATUS_SURVEY);
            client.Send(SMSG_GMTICKET_SYSTEMSTATUS);
            SMSG_GMTICKET_SYSTEMSTATUS.Dispose();
        }

        private enum GMTicketDeleteResult
        {
            GMTICKET_DELETE_SUCCESS = 9
        }

        public void On_CMSG_GMTICKET_DELETETICKET(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_DELETETICKET", client.IP, client.Port);
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM characters_tickets WHERE char_guid = {0};", client.Character.Guid));
            var SMSG_GMTICKET_DELETETICKET = new Packets.PacketClass(Opcodes.SMSG_GMTICKET_DELETETICKET);
            SMSG_GMTICKET_DELETETICKET.AddInt32((int)GMTicketDeleteResult.GMTICKET_DELETE_SUCCESS);
            client.Send(SMSG_GMTICKET_DELETETICKET);
            SMSG_GMTICKET_DELETETICKET.Dispose();
        }

        public void On_CMSG_GMTICKET_UPDATETEXT(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 7)
                return;
            packet.GetInt16();
            string ticket_text = ClusterServiceLocator._Functions.EscapeString(packet.GetString());
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_UPDATETEXT [{2}]", client.IP, client.Port, ticket_text);
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("UPDATE characters_tickets SET char_guid={0}, ticket_text=\"{1}\";", client.Character.Guid, ticket_text));
        }

        public void On_CMSG_WHOIS(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string Name = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WHOIS [{2}]", client.IP, client.Port, Name);
            var response = new Packets.PacketClass(Opcodes.SMSG_WHOIS);
            response.AddString("This feature is not available yet.");
            client.Send(response);
            response.Dispose();
        }
    }
}