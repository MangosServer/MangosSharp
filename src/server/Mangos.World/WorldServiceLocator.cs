//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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

using Autofac;
using Mangos.Common.Globals;
using Mangos.Configuration;
using Mangos.DataStores;
using Mangos.World.AI;
using Mangos.World.Auction;
using Mangos.World.Battlegrounds;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Gossip;
using Mangos.World.Handlers;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Social;
using Mangos.World.Spells;
using Mangos.World.Warden;
using Mangos.World.Weather;
using Mangos.Zip;
using Functions = Mangos.Common.Legacy.Globals.Functions;
using NativeMethods = Mangos.Common.Legacy.NativeMethods;

namespace Mangos.World;

public sealed class WorldServiceLocator
{
    public static IContainer Container { get; set; }

    public static DataStoreProvider DataStoreProvider => Container.Resolve<DataStoreProvider>();

    public static MangosConfiguration MangosConfiguration => Container.Resolve<MangosConfiguration>();

    public static MangosGlobalConstants GlobalConstants => Container.Resolve<MangosGlobalConstants>();

    public static Functions CommonGlobalFunctions => Container.Resolve<Functions>();

    public static Common.Legacy.Functions CommonFunctions => Container.Resolve<Common.Legacy.Functions>();

    public static ZipService GlobalZip => Container.Resolve<ZipService>();

    public static NativeMethods NativeMethods => Container.Resolve<NativeMethods>();

    public static WorldServer WorldServer => Container.Resolve<WorldServer>();

    public static Globals.Functions Functions => Container.Resolve<Globals.Functions>();

    public static WS_Creatures_AI WSCreaturesAI => Container.Resolve<WS_Creatures_AI>();

    public static WS_Auction WSAuction => Container.Resolve<WS_Auction>();

    public static WS_Battlegrounds WSBattlegrounds => Container.Resolve<WS_Battlegrounds>();

    public static WS_DBCDatabase WSDBCDatabase => Container.Resolve<WS_DBCDatabase>();

    public static WS_DBCLoad WSDBCLoad => Container.Resolve<WS_DBCLoad>();

    public static Packets Packets => Container.Resolve<Packets>();

    public static WS_GuardGossip WSGuardGossip => Container.Resolve<WS_GuardGossip>();

    public static WS_Loot WSLoot => Container.Resolve<WS_Loot>();

    public static WS_Maps WSMaps => Container.Resolve<WS_Maps>();

    public static WS_Corpses WSCorpses => Container.Resolve<WS_Corpses>();

    public static WS_Creatures WSCreatures => Container.Resolve<WS_Creatures>();

    public static WS_DynamicObjects WSDynamicObjects => Container.Resolve<WS_DynamicObjects>();

    public static WS_GameObjects WSGameObjects => Container.Resolve<WS_GameObjects>();

    public static WS_Items WSItems => Container.Resolve<WS_Items>();

    public static WS_NPCs WSNPCs => Container.Resolve<WS_NPCs>();

    public static WS_Pets WSPets => Container.Resolve<WS_Pets>();

    public static WS_Transports WSTransports => Container.Resolve<WS_Transports>();

    public static CharManagementHandler CharManagementHandler => Container.Resolve<CharManagementHandler>();

    public static WS_CharMovement WSCharMovement => Container.Resolve<WS_CharMovement>();

    public static WS_Combat WSCombat => Container.Resolve<WS_Combat>();

    public static WS_Commands WSCommands => Container.Resolve<WS_Commands>();

    public static WS_Handlers WSHandlers => Container.Resolve<WS_Handlers>();

    public static WS_Handlers_Battleground WSHandlersBattleground => Container.Resolve<WS_Handlers_Battleground>();

    public static WS_Handlers_Chat WSHandlersChat => Container.Resolve<WS_Handlers_Chat>();

    public static WS_Handlers_Gamemaster WSHandlersGamemaster => Container.Resolve<WS_Handlers_Gamemaster>();

    public static WS_Handlers_Instance WSHandlersInstance => Container.Resolve<WS_Handlers_Instance>();

    public static WS_Handlers_Misc WSHandlersMisc => Container.Resolve<WS_Handlers_Misc>();

    public static WS_Handlers_Taxi WSHandlersTaxi => Container.Resolve<WS_Handlers_Taxi>();

    public static WS_Handlers_Trade WSHandlersTrade => Container.Resolve<WS_Handlers_Trade>();

    public static WS_Handlers_Warden WSHandlersWarden => Container.Resolve<WS_Handlers_Warden>();

    public static WS_Player_Creation WSPlayerCreation => Container.Resolve<WS_Player_Creation>();

    public static WS_Player_Initializator WSPlayerInitializator => Container.Resolve<WS_Player_Initializator>();

    public static WS_PlayerData WSPlayerData => Container.Resolve<WS_PlayerData>();

    public static WS_PlayerHelper WSPlayerHelper => Container.Resolve<WS_PlayerHelper>();

    public static WS_Network WSNetwork => Container.Resolve<WS_Network>();

    public static WS_TimerBasedEvents WSTimerBasedEvents => Container.Resolve<WS_TimerBasedEvents>();

    public static WS_Group WSGroup => Container.Resolve<WS_Group>();

    public static WS_Guilds WSGuilds => Container.Resolve<WS_Guilds>();

    public static WS_Mail WSMail => Container.Resolve<WS_Mail>();

    public static WS_Spells WSSpells => Container.Resolve<WS_Spells>();

    public static WS_Warden WSWarden => Container.Resolve<WS_Warden>();

    public static WS_Weather WSWeather => Container.Resolve<WS_Weather>();
}
