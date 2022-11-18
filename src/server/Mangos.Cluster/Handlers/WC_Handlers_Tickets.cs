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

using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.Data;

namespace Mangos.Cluster.Handlers;

public class WcHandlersTickets
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlersTickets(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void On_CMSG_BUG(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 14)
        {
            return;
        }

        packet.GetInt16();
        SuggestionType suggestion = (SuggestionType)packet.GetInt32();
        var cLength = packet.GetInt32();
        var cString = _clusterServiceLocator.Functions.EscapeString(packet.GetString());
        if (packet.Data.Length - 1 < 14 + cString.Length + 5)
        {
            return;
        }

        var tLength = packet.GetInt32();
        var tString = _clusterServiceLocator.Functions.EscapeString(packet.GetString());
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUG [2]", client.IP, client.Port, suggestion);
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "Bug report [{0}:{1} Lengths:{2}, {3}] " + cString + Constants.vbCrLf + tString, cLength.ToString(), tLength.ToString());
    }

    // ERR_TICKET_ALREADY_EXISTS
    // ERR_TICKET_CREATE_ERROR
    // ERR_TICKET_UPDATE_ERROR
    // ERR_TICKET_DB_ERROR
    // ERR_TICKET_NO_TEXT

    private enum GmTicketGetResult
    {
        GMTICKET_AVAILABLE = 6,
        GMTICKET_NOTICKET = 10
    }

    public void On_CMSG_GMTICKET_GETTICKET(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_GETTICKET", client.IP, client.Port);
        PacketClass smsgGmticketGetticket = new(Opcodes.SMSG_GMTICKET_GETTICKET);
        DataTable mySqlResult = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT * FROM characters_tickets WHERE char_guid = {0};", client.Character.Guid), ref mySqlResult);
        if (mySqlResult.Rows.Count > 0)
        {
            smsgGmticketGetticket.AddInt32((int)GmTicketGetResult.GMTICKET_AVAILABLE);
            smsgGmticketGetticket.AddString(Conversions.ToString(mySqlResult.Rows[0]["ticket_text"]));
        }
        else
        {
            smsgGmticketGetticket.AddInt32((int)GmTicketGetResult.GMTICKET_NOTICKET);
        }

        client.Send(smsgGmticketGetticket);
        smsgGmticketGetticket.Dispose();
        PacketClass smsgQueryTimeResponse = new(Opcodes.SMSG_QUERY_TIME_RESPONSE);
        smsgQueryTimeResponse.AddInt32(_clusterServiceLocator.NativeMethods.timeGetTime("")); // GetTimestamp(Now))
        client.Send(smsgQueryTimeResponse);
        smsgQueryTimeResponse.Dispose();
    }

    private enum GmTicketCreateResult
    {
        GMTICKET_ALREADY_HAVE = 1,
        GMTICKET_CREATE_OK = 2
    }

    public void On_CMSG_GMTICKET_CREATE(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();
        var ticketMap = packet.GetUInt32();
        var ticketX = packet.GetFloat();
        var ticketY = packet.GetFloat();
        var ticketZ = packet.GetFloat();
        var ticketText = _clusterServiceLocator.Functions.EscapeString(packet.GetString());
        DataTable mySqlResult = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT * FROM characters_tickets WHERE char_guid = {0};", client.Character.Guid), ref mySqlResult);
        PacketClass smsgGmticketCreate = new(Opcodes.SMSG_GMTICKET_CREATE);
        if (mySqlResult.Rows.Count > 0)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_CREATE", client.IP, client.Port);
            smsgGmticketCreate.AddInt32((int)GmTicketCreateResult.GMTICKET_ALREADY_HAVE);
        }
        else
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_CREATE [{2}]", client.IP, client.Port, ticketText);
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("INSERT INTO characters_tickets (char_guid, ticket_text, ticket_x, ticket_y, ticket_z, ticket_map) VALUES ({0} , \"{1}\", {2}, {3}, {4}, {5});", client.Character.Guid, ticketText, Strings.Trim(Conversion.Str(ticketX)), Strings.Trim(Conversion.Str(ticketY)), Strings.Trim(Conversion.Str(ticketZ)), ticketMap));
            smsgGmticketCreate.AddInt32((int)GmTicketCreateResult.GMTICKET_CREATE_OK);
        }

        client.Send(smsgGmticketCreate);
        smsgGmticketCreate.Dispose();
    }

    private enum GmTicketSystemStatus
    {
        GMTICKET_SYSTEMSTATUS_ENABLED = 1,
        GMTICKET_SYSTEMSTATUS_DISABLED = 2,
        GMTICKET_SYSTEMSTATUS_SURVEY = 3
    }

    public void On_CMSG_GMTICKET_SYSTEMSTATUS(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_SYSTEMSTATUS", client.IP, client.Port);
        PacketClass smsgGmticketSystemstatus = new(Opcodes.SMSG_GMTICKET_SYSTEMSTATUS);
        smsgGmticketSystemstatus.AddInt32((int)GmTicketSystemStatus.GMTICKET_SYSTEMSTATUS_SURVEY);
        client.Send(smsgGmticketSystemstatus);
        smsgGmticketSystemstatus.Dispose();
    }

    private enum GmTicketDeleteResult
    {
        GMTICKET_DELETE_SUCCESS = 9
    }

    public void On_CMSG_GMTICKET_DELETETICKET(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_DELETETICKET", client.IP, client.Port);
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM characters_tickets WHERE char_guid = {0};", client.Character.Guid));
        PacketClass smsgGmticketDeleteticket = new(Opcodes.SMSG_GMTICKET_DELETETICKET);
        smsgGmticketDeleteticket.AddInt32((int)GmTicketDeleteResult.GMTICKET_DELETE_SUCCESS);
        client.Send(smsgGmticketDeleteticket);
        smsgGmticketDeleteticket.Dispose();
    }

    public void On_CMSG_GMTICKET_UPDATETEXT(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 7)
        {
            return;
        }

        packet.GetInt16();
        var ticketText = _clusterServiceLocator.Functions.EscapeString(packet.GetString());
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_UPDATETEXT [{2}]", client.IP, client.Port, ticketText);
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters_tickets SET char_guid={0}, ticket_text=\"{1}\";", client.Character.Guid, ticketText));
    }

    public void On_CMSG_WHOIS(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();
        var name = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WHOIS [{2}]", client.IP, client.Port, name);
        PacketClass response = new(Opcodes.SMSG_WHOIS);
        response.AddString("This feature is not available yet.");
        client.Send(response);
        response.Dispose();
    }
}
