using Autofac;
using global;
using Mangos.Common.Globals;
using Mangos.World.AI;
using Mangos.World.Auction;
using Mangos.World.Battlegrounds;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Gossip;
using Mangos.World.Handlers;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Social;
using Mangos.World.Spells;
using Mangos.World.Warden;
using Mangos.World.Weather;

namespace Mangos.World
{
    public static class WorldServiceLocator
    {
        public static IContainer _Container { get; set; } = Program.CreateContainer();
        public static Global_Constants _Global_Constants { get; set; } = _Container.Resolve<Global_Constants>();
        public static Common.Globals.Functions _CommonGlobalFunctions { get; set; } = _Container.Resolve<Common.Globals.Functions>();
        public static Common.Functions _CommonFunctions { get; set; } = _Container.Resolve<Common.Functions>();
        public static GlobalZip _GlobalZip { get; set; } = _Container.Resolve<GlobalZip>();
        public static Common.NativeMethods _NativeMethods { get; set; } = _Container.Resolve<Common.NativeMethods>();
        public static WorldServer _WorldServer { get; set; } = _Container.Resolve<WorldServer>();
        public static Globals.Functions _Functions { get; set; } = _Container.Resolve<Globals.Functions>();
        public static WS_Creatures_AI _WS_Creatures_AI { get; set; } = _Container.Resolve<WS_Creatures_AI>();
        public static WS_Auction _WS_Auction { get; set; } = _Container.Resolve<WS_Auction>();
        public static WS_Battlegrounds _WS_Battlegrounds { get; set; } = _Container.Resolve<WS_Battlegrounds>();
        public static WS_DBCDatabase _WS_DBCDatabase { get; set; } = _Container.Resolve<WS_DBCDatabase>();
        public static WS_DBCLoad _WS_DBCLoad { get; set; } = _Container.Resolve<WS_DBCLoad>();
        public static Packets _Packets { get; set; } = _Container.Resolve<Packets>();
        public static WS_GuardGossip _WS_GuardGossip { get; set; } = _Container.Resolve<WS_GuardGossip>();
        public static WS_Loot _WS_Loot { get; set; } = _Container.Resolve<WS_Loot>();
        public static WS_Maps _WS_Maps { get; set; } = _Container.Resolve<WS_Maps>();
        public static WS_Corpses _WS_Corpses { get; set; } = _Container.Resolve<WS_Corpses>();
        public static WS_Creatures _WS_Creatures { get; set; } = _Container.Resolve<WS_Creatures>();
        public static WS_DynamicObjects _WS_DynamicObjects { get; set; } = _Container.Resolve<WS_DynamicObjects>();
        public static WS_GameObjects _WS_GameObjects { get; set; } = _Container.Resolve<WS_GameObjects>();
        public static WS_Items _WS_Items { get; set; } = _Container.Resolve<WS_Items>();
        public static WS_NPCs _WS_NPCs { get; set; } = _Container.Resolve<WS_NPCs>();
        public static WS_Pets _WS_Pets { get; set; } = _Container.Resolve<WS_Pets>();
        public static WS_Transports _WS_Transports { get; set; } = _Container.Resolve<WS_Transports>();
        public static CharManagementHandler _CharManagementHandler { get; set; } = _Container.Resolve<CharManagementHandler>();
        public static WS_CharMovement _WS_CharMovement { get; set; } = _Container.Resolve<WS_CharMovement>();
        public static WS_Combat _WS_Combat { get; set; } = _Container.Resolve<WS_Combat>();
        public static WS_Commands _WS_Commands { get; set; } = _Container.Resolve<WS_Commands>();
        public static WS_Handlers _WS_Handlers { get; set; } = _Container.Resolve<WS_Handlers>();
        public static WS_Handlers_Battleground _WS_Handlers_Battleground { get; set; } = _Container.Resolve<WS_Handlers_Battleground>();
        public static WS_Handlers_Chat _WS_Handlers_Chat { get; set; } = _Container.Resolve<WS_Handlers_Chat>();
        public static WS_Handlers_Gamemaster _WS_Handlers_Gamemaster { get; set; } = _Container.Resolve<WS_Handlers_Gamemaster>();
        public static WS_Handlers_Instance _WS_Handlers_Instance { get; set; } = _Container.Resolve<WS_Handlers_Instance>();
        public static WS_Handlers_Misc _WS_Handlers_Misc { get; set; } = _Container.Resolve<WS_Handlers_Misc>();
        public static WS_Handlers_Taxi _WS_Handlers_Taxi { get; set; } = _Container.Resolve<WS_Handlers_Taxi>();
        public static WS_Handlers_Trade _WS_Handlers_Trade { get; set; } = _Container.Resolve<WS_Handlers_Trade>();
        public static WS_Handlers_Warden _WS_Handlers_Warden { get; set; } = _Container.Resolve<WS_Handlers_Warden>();
        public static WS_Player_Creation _WS_Player_Creation { get; set; } = _Container.Resolve<WS_Player_Creation>();
        public static WS_Player_Initializator _WS_Player_Initializator { get; set; } = _Container.Resolve<WS_Player_Initializator>();
        public static WS_PlayerData _WS_PlayerData { get; set; } = _Container.Resolve<WS_PlayerData>();
        public static WS_PlayerHelper _WS_PlayerHelper { get; set; } = _Container.Resolve<WS_PlayerHelper>();
        public static WS_Network _WS_Network { get; set; } = _Container.Resolve<WS_Network>();
        public static WS_TimerBasedEvents _WS_TimerBasedEvents { get; set; } = _Container.Resolve<WS_TimerBasedEvents>();
        public static WS_Group _WS_Group { get; set; } = _Container.Resolve<WS_Group>();
        public static WS_Guilds _WS_Guilds { get; set; } = _Container.Resolve<WS_Guilds>();
        public static WS_Mail _WS_Mail { get; set; } = _Container.Resolve<WS_Mail>();
        public static WS_Spells _WS_Spells { get; set; } = _Container.Resolve<WS_Spells>();
        public static WS_Warden _WS_Warden { get; set; } = _Container.Resolve<WS_Warden>();
        public static WS_Weather _WS_Weather { get; set; } = _Container.Resolve<WS_Weather>();
    }
}