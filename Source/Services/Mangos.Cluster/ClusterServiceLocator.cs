//
//  Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

namespace Mangos.Cluster
{
    public class ClusterServiceLocator
    {
        public MangosGlobalConstants _Global_Constants { get; set; }
        public Functions _CommonGlobalFunctions { get; set; }
        public Common.Legacy.Functions _CommonFunctions { get; set; }
        public ZipService _GlobalZip { get; set; }
        public NativeMethods _NativeMethods { get; set; }
        public WorldCluster _WorldCluster { get; set; }
        public WorldServerClass _WorldServerClass { get; set; }
        public WS_DBCDatabase _WS_DBCDatabase { get; set; }
        public WS_DBCLoad _WS_DBCLoad { get; set; }
        public Globals.Functions _Functions { get; set; }
        public Packets _Packets { get; set; }
        public WC_Guild _WC_Guild { get; set; }
        public WC_Stats _WC_Stats { get; set; }
        public WC_Network _WC_Network { get; set; }
        public WC_Handlers _WC_Handlers { get; set; }
        public WC_Handlers_Auth _WC_Handlers_Auth { get; set; }
        public WC_Handlers_Battleground _WC_Handlers_Battleground { get; set; }
        public WC_Handlers_Chat _WC_Handlers_Chat { get; set; }
        public WC_Handlers_Group _WC_Handlers_Group { get; set; }
        public WC_Handlers_Guild _WC_Handlers_Guild { get; set; }
        public WC_Handlers_Misc _WC_Handlers_Misc { get; set; }
        public WC_Handlers_Movement _WC_Handlers_Movement { get; set; }
        public WC_Handlers_Social _WC_Handlers_Social { get; set; }
        public WC_Handlers_Tickets _WC_Handlers_Tickets { get; set; }
        public WS_Handler_Channels _WS_Handler_Channels { get; set; }
        public WcHandlerCharacter _WcHandlerCharacter { get; set; }
    }
}