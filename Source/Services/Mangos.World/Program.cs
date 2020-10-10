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
    public static class Program
    {
        public static void Main()
        {
            WorldServiceLocator._WorldServer.Main();
        }

        public static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Global_Constants>().As<Global_Constants>();
            builder.RegisterType<Common.Globals.Functions>().As<Common.Globals.Functions>();
            builder.RegisterType<Common.Functions>().As<Common.Functions>();
            builder.RegisterType<GlobalZip>().As<GlobalZip>();
            builder.RegisterType<Common.NativeMethods>().As<Common.NativeMethods>();
            builder.RegisterType<WorldServer>().As<WorldServer>();
            builder.RegisterType<Globals.Functions>().As<Globals.Functions>();
            builder.RegisterType<WS_Creatures_AI>().As<WS_Creatures_AI>();
            builder.RegisterType<WS_Auction>().As<WS_Auction>();
            builder.RegisterType<WS_Battlegrounds>().As<WS_Battlegrounds>();
            builder.RegisterType<WS_DBCDatabase>().As<WS_DBCDatabase>();
            builder.RegisterType<WS_DBCLoad>().As<WS_DBCLoad>();
            builder.RegisterType<Packets>().As<Packets>();
            builder.RegisterType<WS_GuardGossip>().As<WS_GuardGossip>();
            builder.RegisterType<WS_Loot>().As<WS_Loot>();
            builder.RegisterType<WS_Maps>().As<WS_Maps>();
            builder.RegisterType<WS_Corpses>().As<WS_Corpses>();
            builder.RegisterType<WS_Creatures>().As<WS_Creatures>();
            builder.RegisterType<WS_DynamicObjects>().As<WS_DynamicObjects>();
            builder.RegisterType<WS_GameObjects>().As<WS_GameObjects>();
            builder.RegisterType<WS_Items>().As<WS_Items>();
            builder.RegisterType<WS_NPCs>().As<WS_NPCs>();
            builder.RegisterType<WS_Pets>().As<WS_Pets>();
            builder.RegisterType<WS_Transports>().As<WS_Transports>();
            builder.RegisterType<CharManagementHandler>().As<CharManagementHandler>();
            builder.RegisterType<WS_CharMovement>().As<WS_CharMovement>();
            builder.RegisterType<WS_Combat>().As<WS_Combat>();
            builder.RegisterType<WS_Commands>().As<WS_Commands>();
            builder.RegisterType<WS_Handlers>().As<WS_Handlers>();
            builder.RegisterType<WS_Handlers_Battleground>().As<WS_Handlers_Battleground>();
            builder.RegisterType<WS_Handlers_Chat>().As<WS_Handlers_Chat>();
            builder.RegisterType<WS_Handlers_Gamemaster>().As<WS_Handlers_Gamemaster>();
            builder.RegisterType<WS_Handlers_Instance>().As<WS_Handlers_Instance>();
            builder.RegisterType<WS_Handlers_Misc>().As<WS_Handlers_Misc>();
            builder.RegisterType<WS_Handlers_Taxi>().As<WS_Handlers_Taxi>();
            builder.RegisterType<WS_Handlers_Trade>().As<WS_Handlers_Trade>();
            builder.RegisterType<WS_Handlers_Warden>().As<WS_Handlers_Warden>();
            builder.RegisterType<WS_Player_Creation>().As<WS_Player_Creation>();
            builder.RegisterType<WS_Player_Initializator>().As<WS_Player_Initializator>();
            builder.RegisterType<WS_PlayerData>().As<WS_PlayerData>();
            builder.RegisterType<WS_PlayerHelper>().As<WS_PlayerHelper>();
            builder.RegisterType<WS_Network>().As<WS_Network>();
            builder.RegisterType<WS_TimerBasedEvents>().As<WS_TimerBasedEvents>();
            builder.RegisterType<WS_Group>().As<WS_Group>();
            builder.RegisterType<WS_Guilds>().As<WS_Guilds>();
            builder.RegisterType<WS_Mail>().As<WS_Mail>();
            builder.RegisterType<WS_Spells>().As<WS_Spells>();
            builder.RegisterType<WS_Warden>().As<WS_Warden>();
            builder.RegisterType<WS_Weather>().As<WS_Weather>();
            return builder.Build();
        }
    }
}