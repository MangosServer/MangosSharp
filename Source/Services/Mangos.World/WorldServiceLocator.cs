using System.Diagnostics;
using System.Runtime.CompilerServices;
using Autofac;
using global;
using Mangos.Common;
using Mangos.Common.Globals;
using Mangos.Configuration;
using Mangos.World.AI;
using Mangos.World.AntiCheat;
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
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World
{
    [StandardModule]
	public sealed class WorldServiceLocator
	{
		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static IContainer __Container;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static IConfigurationProvider<WorldServerConfiguration> __ConfigurationProvider;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Global_Constants __Global_Constants;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Mangos.Common.Globals.Functions __CommonGlobalFunctions;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Mangos.Common.Functions __CommonFunctions;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static GlobalZip __GlobalZip;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Mangos.Common.NativeMethods __NativeMethods;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WorldServer __WorldServer;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Mangos.World.Globals.Functions __Functions;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Creatures_AI __WS_Creatures_AI;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Auction __WS_Auction;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Battlegrounds __WS_Battlegrounds;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_DBCDatabase __WS_DBCDatabase;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_DBCLoad __WS_DBCLoad;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Packets __Packets;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_GuardGossip __WS_GuardGossip;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Loot __WS_Loot;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Maps __WS_Maps;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Corpses __WS_Corpses;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Creatures __WS_Creatures;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_DynamicObjects __WS_DynamicObjects;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_GameObjects __WS_GameObjects;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Items __WS_Items;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_NPCs __WS_NPCs;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Pets __WS_Pets;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Transports __WS_Transports;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static CharManagementHandler __CharManagementHandler;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_CharMovement __WS_CharMovement;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Combat __WS_Combat;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Commands __WS_Commands;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Handlers __WS_Handlers;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Handlers_Battleground __WS_Handlers_Battleground;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Handlers_Chat __WS_Handlers_Chat;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Handlers_Gamemaster __WS_Handlers_Gamemaster;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Handlers_Instance __WS_Handlers_Instance;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Handlers_Misc __WS_Handlers_Misc;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Handlers_Taxi __WS_Handlers_Taxi;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Handlers_Trade __WS_Handlers_Trade;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Handlers_Warden __WS_Handlers_Warden;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Player_Creation __WS_Player_Creation;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Player_Initializator __WS_Player_Initializator;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_PlayerData __WS_PlayerData;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_PlayerHelper __WS_PlayerHelper;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Network __WS_Network;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_TimerBasedEvents __WS_TimerBasedEvents;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Group __WS_Group;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Guilds __WS_Guilds;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Mail __WS_Mail;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Spells __WS_Spells;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Warden __WS_Warden;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static WS_Weather __WS_Weather;

		public static IContainer _Container
		{
			get;
			set;
		} = Program.CreateContainer();


		public static IConfigurationProvider<WorldServerConfiguration> _ConfigurationProvider
		{
			get;
			set;
		} = _Container.Resolve<IConfigurationProvider<WorldServerConfiguration>>();


		public static Global_Constants _Global_Constants
		{
			get;
			set;
		} = _Container.Resolve<Global_Constants>();


		public static Mangos.Common.Globals.Functions _CommonGlobalFunctions
		{
			get;
			set;
		} = _Container.Resolve<Mangos.Common.Globals.Functions>();


		public static Mangos.Common.Functions _CommonFunctions
		{
			get;
			set;
		} = _Container.Resolve<Mangos.Common.Functions>();


		public static GlobalZip _GlobalZip
		{
			get;
			set;
		} = _Container.Resolve<GlobalZip>();


		public static Mangos.Common.NativeMethods _NativeMethods
		{
			get;
			set;
		} = _Container.Resolve<Mangos.Common.NativeMethods>();


		public static WorldServer _WorldServer
		{
			get;
			set;
		} = _Container.Resolve<WorldServer>();


		public static Mangos.World.Globals.Functions _Functions
		{
			get;
			set;
		} = _Container.Resolve<Mangos.World.Globals.Functions>();


		public static WS_Creatures_AI _WS_Creatures_AI
		{
			get;
			set;
		} = _Container.Resolve<WS_Creatures_AI>();


		public static WS_Auction _WS_Auction
		{
			get;
			set;
		} = _Container.Resolve<WS_Auction>();


		public static WS_Battlegrounds _WS_Battlegrounds
		{
			get;
			set;
		} = _Container.Resolve<WS_Battlegrounds>();


		public static WS_DBCDatabase _WS_DBCDatabase
		{
			get;
			set;
		} = _Container.Resolve<WS_DBCDatabase>();


		public static WS_DBCLoad _WS_DBCLoad
		{
			get;
			set;
		} = _Container.Resolve<WS_DBCLoad>();


		public static Packets _Packets
		{
			get;
			set;
		} = _Container.Resolve<Packets>();


		public static WS_GuardGossip _WS_GuardGossip
		{
			get;
			set;
		} = _Container.Resolve<WS_GuardGossip>();


		public static WS_Loot _WS_Loot
		{
			get;
			set;
		} = _Container.Resolve<WS_Loot>();


		public static WS_Maps _WS_Maps
		{
			get;
			set;
		} = _Container.Resolve<WS_Maps>();


		public static WS_Corpses _WS_Corpses
		{
			get;
			set;
		} = _Container.Resolve<WS_Corpses>();


		public static WS_Creatures _WS_Creatures
		{
			get;
			set;
		} = _Container.Resolve<WS_Creatures>();


		public static WS_DynamicObjects _WS_DynamicObjects
		{
			get;
			set;
		} = _Container.Resolve<WS_DynamicObjects>();


		public static WS_GameObjects _WS_GameObjects
		{
			get;
			set;
		} = _Container.Resolve<WS_GameObjects>();


		public static WS_Items _WS_Items
		{
			get;
			set;
		} = _Container.Resolve<WS_Items>();


		public static WS_NPCs _WS_NPCs
		{
			get;
			set;
		} = _Container.Resolve<WS_NPCs>();


		public static WS_Pets _WS_Pets
		{
			get;
			set;
		} = _Container.Resolve<WS_Pets>();


		public static WS_Transports _WS_Transports
		{
			get;
			set;
		} = _Container.Resolve<WS_Transports>();


		public static CharManagementHandler _CharManagementHandler
		{
			get;
			set;
		} = _Container.Resolve<CharManagementHandler>();


		public static WS_CharMovement _WS_CharMovement
		{
			get;
			set;
		} = _Container.Resolve<WS_CharMovement>();


		public static WS_Combat _WS_Combat
		{
			get;
			set;
		} = _Container.Resolve<WS_Combat>();


		public static WS_Commands _WS_Commands
		{
			get;
			set;
		} = _Container.Resolve<WS_Commands>();


		public static WS_Handlers _WS_Handlers
		{
			get;
			set;
		} = _Container.Resolve<WS_Handlers>();


		public static WS_Handlers_Battleground _WS_Handlers_Battleground
		{
			get;
			set;
		} = _Container.Resolve<WS_Handlers_Battleground>();


		public static WS_Handlers_Chat _WS_Handlers_Chat
		{
			get;
			set;
		} = _Container.Resolve<WS_Handlers_Chat>();


		public static WS_Handlers_Gamemaster _WS_Handlers_Gamemaster
		{
			get;
			set;
		} = _Container.Resolve<WS_Handlers_Gamemaster>();


		public static WS_Handlers_Instance _WS_Handlers_Instance
		{
			get;
			set;
		} = _Container.Resolve<WS_Handlers_Instance>();


		public static WS_Handlers_Misc _WS_Handlers_Misc
		{
			get;
			set;
		} = _Container.Resolve<WS_Handlers_Misc>();


		public static WS_Handlers_Taxi _WS_Handlers_Taxi
		{
			get;
			set;
		} = _Container.Resolve<WS_Handlers_Taxi>();


		public static WS_Handlers_Trade _WS_Handlers_Trade
		{
			get;
			set;
		} = _Container.Resolve<WS_Handlers_Trade>();


		public static WS_Handlers_Warden _WS_Handlers_Warden
		{
			get;
			set;
		} = _Container.Resolve<WS_Handlers_Warden>();


		public static WS_Player_Creation _WS_Player_Creation
		{
			get;
			set;
		} = _Container.Resolve<WS_Player_Creation>();


		public static WS_Player_Initializator _WS_Player_Initializator
		{
			get;
			set;
		} = _Container.Resolve<WS_Player_Initializator>();


		public static WS_PlayerData _WS_PlayerData
		{
			get;
			set;
		} = _Container.Resolve<WS_PlayerData>();


		public static WS_PlayerHelper _WS_PlayerHelper
		{
			get;
			set;
		} = _Container.Resolve<WS_PlayerHelper>();


		public static WS_Network _WS_Network
		{
			get;
			set;
		} = _Container.Resolve<WS_Network>();


		public static WS_TimerBasedEvents _WS_TimerBasedEvents
		{
			get;
			set;
		} = _Container.Resolve<WS_TimerBasedEvents>();


		public static WS_Group _WS_Group
		{
			get;
			set;
		} = _Container.Resolve<WS_Group>();


		public static WS_Guilds _WS_Guilds
		{
			get;
			set;
		} = _Container.Resolve<WS_Guilds>();


		public static WS_Mail _WS_Mail
		{
			get;
			set;
		} = _Container.Resolve<WS_Mail>();


		public static WS_Spells _WS_Spells
		{
			get;
			set;
		} = _Container.Resolve<WS_Spells>();


		public static WS_Warden _WS_Warden
		{
			get;
			set;
		} = _Container.Resolve<WS_Warden>();


		public static WS_Weather _WS_Weather
		{
			get;
			set;
		} = _Container.Resolve<WS_Weather>();

	}
}
