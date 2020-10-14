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

using Autofac;
using global;
using Mangos.Cluster.DataStores;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Server;
using Mangos.Common;
using Mangos.Common.Globals;

namespace Mangos.Cluster
{
    public static class ClusterServiceLocator
    {
        public static IContainer _Container { get; set; } = Program.CreateContainer();
        public static MangosGlobalConstants _Global_Constants { get; set; } = _Container.Resolve<MangosGlobalConstants>();
        public static Common.Globals.Functions _CommonGlobalFunctions { get; set; } = _Container.Resolve<Common.Globals.Functions>();
        public static Common.Functions _CommonFunctions { get; set; } = _Container.Resolve<Common.Functions>();
        public static GlobalZip _GlobalZip { get; set; } = _Container.Resolve<GlobalZip>();
        public static NativeMethods _NativeMethods { get; set; } = _Container.Resolve<NativeMethods>();
        public static WorldCluster _WorldCluster { get; set; } = _Container.Resolve<WorldCluster>();
        public static WS_DBCDatabase _WS_DBCDatabase { get; set; } = _Container.Resolve<WS_DBCDatabase>();
        public static WS_DBCLoad _WS_DBCLoad { get; set; } = _Container.Resolve<WS_DBCLoad>();
        public static Globals.Functions _Functions { get; set; } = _Container.Resolve<Globals.Functions>();
        public static Packets _Packets { get; set; } = _Container.Resolve<Packets>();
        public static WC_Guild _WC_Guild { get; set; } = _Container.Resolve<WC_Guild>();
        public static WC_Stats _WC_Stats { get; set; } = _Container.Resolve<WC_Stats>();
        public static WC_Network _WC_Network { get; set; } = _Container.Resolve<WC_Network>();
        public static WC_Handlers _WC_Handlers { get; set; } = _Container.Resolve<WC_Handlers>();
        public static WC_Handlers_Auth _WC_Handlers_Auth { get; set; } = _Container.Resolve<WC_Handlers_Auth>();
        public static WC_Handlers_Battleground _WC_Handlers_Battleground { get; set; } = _Container.Resolve<WC_Handlers_Battleground>();
        public static WC_Handlers_Chat _WC_Handlers_Chat { get; set; } = _Container.Resolve<WC_Handlers_Chat>();
        public static WC_Handlers_Group _WC_Handlers_Group { get; set; } = _Container.Resolve<WC_Handlers_Group>();
        public static WC_Handlers_Guild _WC_Handlers_Guild { get; set; } = _Container.Resolve<WC_Handlers_Guild>();
        public static WC_Handlers_Misc _WC_Handlers_Misc { get; set; } = _Container.Resolve<WC_Handlers_Misc>();
        public static WC_Handlers_Movement _WC_Handlers_Movement { get; set; } = _Container.Resolve<WC_Handlers_Movement>();
        public static WC_Handlers_Social _WC_Handlers_Social { get; set; } = _Container.Resolve<WC_Handlers_Social>();
        public static WC_Handlers_Tickets _WC_Handlers_Tickets { get; set; } = _Container.Resolve<WC_Handlers_Tickets>();
        public static WS_Handler_Channels _WS_Handler_Channels { get; set; } = _Container.Resolve<WS_Handler_Channels>();
        public static WcHandlerCharacter _WcHandlerCharacter { get; set; } = _Container.Resolve<WcHandlerCharacter>();
    }
}