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

using Mangos.Cluster.DataStores;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Handlers.Guild;
using Mangos.Cluster.Network;
using Mangos.Cluster.Stats;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.Zip;
using Functions = Mangos.Common.Legacy.Globals.Functions;

namespace Mangos.Cluster;

public class ClusterServiceLocator
{
    public MangosGlobalConstants GlobalConstants { get; set; }
    public Functions CommonGlobalFunctions { get; set; }
    public Common.Legacy.Functions CommonFunctions { get; set; }
    public ZipService GlobalZip { get; set; }
    public NativeMethods NativeMethods { get; set; }
    public WorldCluster WorldCluster { get; set; }
    public WorldServerClass WorldServerClass { get; set; }
    public WsDbcDatabase WsDbcDatabase { get; set; }
    public WsDbcLoad WsDbcLoad { get; set; }
    public Globals.Functions Functions { get; set; }
    public Packets Packets { get; set; }
    public WcGuild WcGuild { get; set; }
    public WcStats WcStats { get; set; }
    public WcNetwork WcNetwork { get; set; }
    public WcHandlers WcHandlers { get; set; }
    public WcHandlersAuth WcHandlersAuth { get; set; }
    public WcHandlersBattleground WcHandlersBattleground { get; set; }
    public WcHandlersChat WcHandlersChat { get; set; }
    public WcHandlersGroup WcHandlersGroup { get; set; }
    public WcHandlersGuild WcHandlersGuild { get; set; }
    public WcHandlersMisc WcHandlersMisc { get; set; }
    public WcHandlersMovement WcHandlersMovement { get; set; }
    public WcHandlersSocial WcHandlersSocial { get; set; }
    public WcHandlersTickets WcHandlersTickets { get; set; }
    public WsHandlerChannels WsHandlerChannels { get; set; }
    public WcHandlerCharacter WcHandlerCharacter { get; set; }
}
