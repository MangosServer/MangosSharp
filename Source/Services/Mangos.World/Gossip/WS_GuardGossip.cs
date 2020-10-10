// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Gossip;
using Mangos.Common.Enums.Unit;
using Mangos.World.Objects;
using Mangos.World.Player;

namespace Mangos.World.Gossip
{
    public class WS_GuardGossip
    {
        public class TGuardTalk : TBaseTalk
        {

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            public override void OnGossipHello(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var Gossip = GetGossip(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].ID);
                switch (Gossip)
                {
                    case var @case when @case == Gossips.Darnassus:
                        {
                            OnGossipHello_Darnassus(ref objCharacter, cGUID);
                            break;
                        }

                    case var case1 when case1 == Gossips.DunMorogh:
                        {
                            OnGossipHello_DunMorogh(ref objCharacter, cGUID);
                            break;
                        }

                    case var case2 when case2 == Gossips.Durotar:
                        {
                            OnGossipHello_Durotar(ref objCharacter, cGUID);
                            break;
                        }

                    case var case3 when case3 == Gossips.ElwynnForest:
                        {
                            OnGossipHello_ElwynnForest(ref objCharacter, cGUID);
                            break;
                        }

                    case var case4 when case4 == Gossips.Ironforge:
                        {
                            OnGossipHello_Ironforge(ref objCharacter, cGUID);
                            break;
                        }

                    case var case5 when case5 == Gossips.Mulgore:
                        {
                            OnGossipHello_Mulgore(ref objCharacter, cGUID);
                            break;
                        }

                    case var case6 when case6 == Gossips.Orgrimmar:
                        {
                            OnGossipHello_Orgrimmar(ref objCharacter, cGUID);
                            break;
                        }

                    case var case7 when case7 == Gossips.Stormwind:
                        {
                            OnGossipHello_Stormwind(ref objCharacter, cGUID);
                            break;
                        }

                    case var case8 when case8 == Gossips.Teldrassil:
                        {
                            OnGossipHello_Teldrassil(ref objCharacter, cGUID);
                            break;
                        }

                    case var case9 when case9 == Gossips.Thunderbluff:
                        {
                            OnGossipHello_Thunderbluff(ref objCharacter, cGUID);
                            break;
                        }

                    case var case10 when case10 == Gossips.Tirisfall:
                        {
                            OnGossipHello_Tirisfall(ref objCharacter, cGUID);
                            break;
                        }

                    case var case11 when case11 == Gossips.Undercity:
                        {
                            OnGossipHello_Undercity(ref objCharacter, cGUID);
                            break;
                        }

                    default:
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Unknown gossip [{0}].", Gossip);
                            break;
                        }
                }
            }

            public override void OnGossipSelect(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int selected)
            {
                var Gossip = GetGossip(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].ID);
                switch (Gossip)
                {
                    case var @case when @case == Gossips.Darnassus:
                        {
                            OnGossipSelect_Darnassus(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case1 when case1 == Gossips.DunMorogh:
                        {
                            OnGossipSelect_DunMorogh(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case2 when case2 == Gossips.Durotar:
                        {
                            OnGossipSelect_Durotar(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case3 when case3 == Gossips.ElwynnForest:
                        {
                            OnGossipSelect_ElwynnForest(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case4 when case4 == Gossips.Ironforge:
                        {
                            OnGossipSelect_Ironforge(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case5 when case5 == Gossips.Mulgore:
                        {
                            OnGossipSelect_Mulgore(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case6 when case6 == Gossips.Orgrimmar:
                        {
                            OnGossipSelect_Orgrimmar(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case7 when case7 == Gossips.Stormwind:
                        {
                            OnGossipSelect_Stormwind(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case8 when case8 == Gossips.Teldrassil:
                        {
                            OnGossipSelect_Teldrassil(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case9 when case9 == Gossips.Thunderbluff:
                        {
                            OnGossipSelect_Thunderbluff(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case10 when case10 == Gossips.Tirisfall:
                        {
                            OnGossipSelect_Tirisfall(ref objCharacter, cGUID, selected);
                            break;
                        }

                    case var case11 when case11 == Gossips.Undercity:
                        {
                            OnGossipSelect_Undercity(ref objCharacter, cGUID, selected);
                            break;
                        }

                    default:
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Unknown gossip [{0}].", Gossip);
                            break;
                        }
                }
            }

            public Gossips GetGossip(int Entry)
            {
                switch ((Guards)Entry)
                {
                    case var @case when @case == Guards.Bluffwatcher:
                        {
                            return Gossips.Thunderbluff;
                        }

                    case var case1 when case1 == Guards.Darnassus_Sentinel:
                        {
                            return Gossips.Darnassus;
                        }

                    case var case2 when case2 == Guards.Orgrimmar_Grunt:
                        {
                            return Gossips.Orgrimmar;
                        }

                    case var case3 when case3 == Guards.Stormwind_City_Guard:
                    case var case4 when case4 == Guards.Stormwind_City_Patroller:
                        {
                            return Gossips.Stormwind;
                        }

                    case var case5 when case5 == Guards.Stormwind_Guard:
                        {
                            return Gossips.ElwynnForest;
                        }

                    case var case6 when case6 == Guards.Ironforge_Guard:
                        {
                            return Gossips.Ironforge;
                        }

                    case var case7 when case7 == Guards.Ironforge_Mountaineer:
                        {
                            return Gossips.DunMorogh;
                        }

                    case var case8 when case8 == Guards.Razor_Hill_Grunt:
                        {
                            return Gossips.Durotar;
                        }

                    case var case9 when case9 == Guards.Teldrassil_Sentinel:
                        {
                            return Gossips.Teldrassil;
                        }

                    case var case10 when case10 == Guards.Undercity_Guardian:
                        {
                            return Gossips.Undercity;
                        }

                    case var case11 when case11 == Guards.Deathguard_Bartholomew:
                    case var case12 when case12 == Guards.Deathguard_Burgess:
                    case var case13 when case13 == Guards.Deathguard_Cyrus:
                    case var case14 when case14 == Guards.Deathguard_Dillinger:
                    case var case15 when case15 == Guards.Deathguard_Lawrence:
                    case var case16 when case16 == Guards.Deathguard_Lundmark:
                    case var case17 when case17 == Guards.Deathguard_Morris:
                    case var case18 when case18 == Guards.Deathguard_Mort:
                    case var case19 when case19 == Guards.Deathguard_Terrence:
                        {
                            return Gossips.Tirisfall;
                        }

                    default:
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Creature Entry [{0}] was not found in guard table.", Entry);
                            return 0;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Stormwind(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STORMWIND_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DEEPRUNTRAM, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GRYPHON, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_OFFICERS, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 13; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 933, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Stormwind(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Auctionhouse
                        {
                            objCharacter.SendPointOfInterest(-8811.46f, 667.46f, 6, 6, 0, "Stormwind Auction House");
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 3834, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Bank
                        {
                            objCharacter.SendPointOfInterest(-8916.87f, 622.87f, 6, 6, 0, "Stormwind Bank");
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 764, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Deeprun Tram
                        {
                            objCharacter.SendPointOfInterest(-8378.88f, 554.23f, 6, 6, 0, "The Deeprun Tram");
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 3813, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Inn
                        {
                            objCharacter.SendPointOfInterest(-8869.0f, 675.4f, 6, 6, 0, "The Gilded Rose");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 3860, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Gryphon Master
                        {
                            objCharacter.SendPointOfInterest(-8837.0f, 493.5f, 6, 6, 0, "Stormwind Gryphon Master");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 879, Menu: ref argMenu4, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Guild Master
                        {
                            objCharacter.SendPointOfInterest(-8894.0f, 611.2f, 6, 6, 0, "Stormwind Vistor`s Center");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 882, Menu: ref argMenu5, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Mailbox
                        {
                            objCharacter.SendPointOfInterest(-8876.48f, 649.18f, 6, 6, 0, "Stormwind Mailbox");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 3861, Menu: ref argMenu6, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(-8433.0f, 554.7f, 6, 6, 0, "Jenova Stoneshield");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 5984, Menu: ref argMenu7, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Weapon Trainer
                        {
                            objCharacter.SendPointOfInterest(-8797.0f, 612.8f, 6, 6, 0, "Woo Ping");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 4516, Menu: ref argMenu8, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Officers Lounge
                        {
                            objCharacter.SendPointOfInterest(-8759.92f, 399.69f, 6, 6, 0, "Champions` Hall");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 7047, Menu: ref argMenu9, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Battlemasters
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH, (int)MenuIcon.MENUICON_GOSSIP);
                            objCharacter.TalkMenuTypes.Add(14);
                            objCharacter.TalkMenuTypes.Add(15);
                            objCharacter.TalkMenuTypes.Add(16);
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 7499, ref npcMenu, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PALADIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 17; i <= 24; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 898, ref npcMenu, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 25; i <= 36; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 918, ref npcMenu, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // AV
                        {
                            objCharacter.SendPointOfInterest(-8443.88f, 335.99f, 6, 6, 0, "Thelman Slatefist");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 7500, Menu: ref argMenu10, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // AB
                        {
                            objCharacter.SendPointOfInterest(-8443.88f, 335.99f, 6, 6, 0, "Lady Hoteshem");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 7650, Menu: ref argMenu11, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // WSG
                        {
                            objCharacter.SendPointOfInterest(-8443.88f, 335.99f, 6, 6, 0, "Elfarran");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 7501, Menu: ref argMenu12, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Mage
                        {
                            objCharacter.SendPointOfInterest(-9012.0f, 867.6f, 6, 6, 0, "Wizard`s Sanctum");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 899, Menu: ref argMenu13, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Rogue
                        {
                            objCharacter.SendPointOfInterest(-8753.0f, 367.8f, 6, 6, 0, "Stormwind - Rogue House");
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 900, Menu: ref argMenu14, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Warrior
                        {
                            objCharacter.SendPointOfInterest(-8690.11f, 324.85f, 6, 6, 0, "Command Center");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 901, Menu: ref argMenu15, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Druid
                        {
                            objCharacter.SendPointOfInterest(-8751.0f, 1124.5f, 6, 6, 0, "The Park");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 902, Menu: ref argMenu16, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Priest
                        {
                            objCharacter.SendPointOfInterest(-8512.0f, 862.4f, 6, 6, 0, "Catedral Of Light");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 903, Menu: ref argMenu17, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Paladin
                        {
                            objCharacter.SendPointOfInterest(-8577.0f, 881.7f, 6, 6, 0, "Catedral Of Light");
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 904, Menu: ref argMenu18, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // Hunter
                        {
                            objCharacter.SendPointOfInterest(-8413.0f, 541.5f, 6, 6, 0, "Hunter Lodge");
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 905, Menu: ref argMenu19, qMenu: ref argqMenu22);
                            break;
                        }

                    case 24: // Hunter
                        {
                            objCharacter.SendPointOfInterest(-8948.91f, 998.35f, 6, 6, 0, "The Slaughtered Lamb");
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu23 = null;
                            objCharacter.SendGossip(cGUID, 906, Menu: ref argMenu20, qMenu: ref argqMenu23);
                            break;
                        }

                    case 25: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(-8988.0f, 759.6f, 6, 6, 0, "Alchemy Needs");
                            GossipMenu argMenu21 = null;
                            QuestMenu argqMenu24 = null;
                            objCharacter.SendGossip(cGUID, 919, Menu: ref argMenu21, qMenu: ref argqMenu24);
                            break;
                        }

                    case 26: // Blacksmithing
                        {
                            objCharacter.SendPointOfInterest(-8424.0f, 616.9f, 6, 6, 0, "Therum Deepforge");
                            GossipMenu argMenu22 = null;
                            QuestMenu argqMenu25 = null;
                            objCharacter.SendGossip(cGUID, 920, Menu: ref argMenu22, qMenu: ref argqMenu25);
                            break;
                        }

                    case 27: // Cooking
                        {
                            objCharacter.SendPointOfInterest(-8611.0f, 364.6f, 6, 6, 0, "Pig and Whistle Tavern");
                            GossipMenu argMenu23 = null;
                            QuestMenu argqMenu26 = null;
                            objCharacter.SendGossip(cGUID, 921, Menu: ref argMenu23, qMenu: ref argqMenu26);
                            break;
                        }

                    case 28: // Enchanting
                        {
                            objCharacter.SendPointOfInterest(-8858.0f, 803.7f, 6, 6, 0, "Lucan Cordell");
                            GossipMenu argMenu24 = null;
                            QuestMenu argqMenu27 = null;
                            objCharacter.SendGossip(cGUID, 941, Menu: ref argMenu24, qMenu: ref argqMenu27);
                            break;
                        }

                    case 29: // Engineering
                        {
                            objCharacter.SendPointOfInterest(-8347.0f, 644.1f, 6, 6, 0, "Lilliam Sparkspindle");
                            GossipMenu argMenu25 = null;
                            QuestMenu argqMenu28 = null;
                            objCharacter.SendGossip(cGUID, 922, Menu: ref argMenu25, qMenu: ref argqMenu28);
                            break;
                        }

                    case 30: // First Aid
                        {
                            objCharacter.SendPointOfInterest(-8513.0f, 801.8f, 6, 6, 0, "Shaina Fuller");
                            GossipMenu argMenu26 = null;
                            QuestMenu argqMenu29 = null;
                            objCharacter.SendGossip(cGUID, 923, Menu: ref argMenu26, qMenu: ref argqMenu29);
                            break;
                        }

                    case 31: // Fishing
                        {
                            objCharacter.SendPointOfInterest(-8803.0f, 767.5f, 6, 6, 0, "Arnold Leland");
                            GossipMenu argMenu27 = null;
                            QuestMenu argqMenu30 = null;
                            objCharacter.SendGossip(cGUID, 940, Menu: ref argMenu27, qMenu: ref argqMenu30);
                            break;
                        }

                    case 32: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(-8967.0f, 779.5f, 6, 6, 0, "Alchemy Needs");
                            GossipMenu argMenu28 = null;
                            QuestMenu argqMenu31 = null;
                            objCharacter.SendGossip(cGUID, 924, Menu: ref argMenu28, qMenu: ref argqMenu31);
                            break;
                        }

                    case 33: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(-8726.0f, 477.4f, 6, 6, 0, "The Protective Hide");
                            GossipMenu argMenu29 = null;
                            QuestMenu argqMenu32 = null;
                            objCharacter.SendGossip(cGUID, 925, Menu: ref argMenu29, qMenu: ref argqMenu32);
                            break;
                        }

                    case 34: // Mining
                        {
                            objCharacter.SendPointOfInterest(-8434.0f, 692.8f, 6, 6, 0, "Gelman Stonehand");
                            GossipMenu argMenu30 = null;
                            QuestMenu argqMenu33 = null;
                            objCharacter.SendGossip(cGUID, 927, Menu: ref argMenu30, qMenu: ref argqMenu33);
                            break;
                        }

                    case 35: // Skinning
                        {
                            objCharacter.SendPointOfInterest(-8716.0f, 469.4f, 6, 6, 0, "The Protective Hide");
                            GossipMenu argMenu31 = null;
                            QuestMenu argqMenu34 = null;
                            objCharacter.SendGossip(cGUID, 928, Menu: ref argMenu31, qMenu: ref argqMenu34);
                            break;
                        }

                    case 36: // Tailoring
                        {
                            objCharacter.SendPointOfInterest(-8938.0f, 800.7f, 6, 6, 0, "Duncan`s Textiles");
                            GossipMenu argMenu32 = null;
                            QuestMenu argqMenu35 = null;
                            objCharacter.SendGossip(cGUID, 929, Menu: ref argMenu32, qMenu: ref argqMenu35);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Orgrimmar(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WINDRIDER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ZEPPLINMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_OFFICERS, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 13; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 2593, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Orgrimmar(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Bank
                        {
                            objCharacter.SendPointOfInterest(1631.51f, -4375.33f, 6, 6, 0, "Bank of Orgrimmar");
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 2554, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Wind Rider
                        {
                            objCharacter.SendPointOfInterest(1676.6f, -4332.72f, 6, 6, 0, "The Sky Tower");
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 2555, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Guild Master
                        {
                            objCharacter.SendPointOfInterest(1576.93f, -4294.75f, 6, 6, 0, "Horde Embassy");
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 2556, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Inn
                        {
                            objCharacter.SendPointOfInterest(1644.51f, -4447.27f, 6, 6, 0, "Orgrimmar Inn");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 2557, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Mailbox
                        {
                            objCharacter.SendPointOfInterest(1622.53f, -4388.79f, 6, 6, 0, "Orgrimmar Mailbox");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 2558, Menu: ref argMenu4, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Auction House
                        {
                            objCharacter.SendPointOfInterest(1679.21f, -4450.1f, 6, 6, 0, "Orgrimmar Auction House");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 3075, Menu: ref argMenu5, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Zeppelin
                        {
                            objCharacter.SendPointOfInterest(1337.36f, -4632.7f, 6, 6, 0, "Orgrimmar Zeppelin Tower");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 3173, Menu: ref argMenu6, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Weapon Trainer
                        {
                            objCharacter.SendPointOfInterest(2092.56f, -4823.95f, 6, 6, 0, "Sayoc & Hanashi");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 4519, Menu: ref argMenu7, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(2133.12f, -4663.93f, 6, 6, 0, "Xon'cha");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 5974, Menu: ref argMenu8, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Officers Lounge
                        {
                            objCharacter.SendPointOfInterest(1633.56f, -4249.37f, 6, 6, 0, "Hall of Legends");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 7046, Menu: ref argMenu9, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Battlemasters
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH, (int)MenuIcon.MENUICON_GOSSIP);
                            objCharacter.TalkMenuTypes.Add(14);
                            objCharacter.TalkMenuTypes.Add(15);
                            objCharacter.TalkMenuTypes.Add(16);
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 7521, ref npcMenu, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SHAMAN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 17; i <= 23; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 2599, ref npcMenu, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 24; i <= 35; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 2594, ref npcMenu, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // AV
                        {
                            objCharacter.SendPointOfInterest(1983.92f, -4794.2f, 6, 6, 0, "Hall of the Brave");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 7484, Menu: ref argMenu10, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // AB
                        {
                            objCharacter.SendPointOfInterest(1983.92f, -4794.2f, 6, 6, 0, "Hall of the Brave");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 7644, Menu: ref argMenu11, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // WSG
                        {
                            objCharacter.SendPointOfInterest(1983.92f, -4794.2f, 6, 6, 0, "Hall of the Brave");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 7520, Menu: ref argMenu12, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Hunter
                        {
                            objCharacter.SendPointOfInterest(2114.84f, -4625.31f, 6, 6, 0, "Orgrimmar Hunter's Hall");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 2559, Menu: ref argMenu13, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Mage
                        {
                            objCharacter.SendPointOfInterest(1451.26f, -4223.33f, 6, 6, 0, "Darkbriar Lodge");
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 2560, Menu: ref argMenu14, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Priest
                        {
                            objCharacter.SendPointOfInterest(1442.21f, -4183.24f, 6, 6, 0, "Spirit Lodge");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 2561, Menu: ref argMenu15, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Shaman
                        {
                            objCharacter.SendPointOfInterest(1925.34f, -4181.89f, 6, 6, 0, "Thrall's Fortress");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 2562, Menu: ref argMenu16, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Rogue
                        {
                            objCharacter.SendPointOfInterest(1773.39f, -4278.97f, 6, 6, 0, "Shadowswift Brotherhood");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 2563, Menu: ref argMenu17, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Warlock
                        {
                            objCharacter.SendPointOfInterest(1849.57f, -4359.68f, 6, 6, 0, "Darkfire Enclave");
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 2564, Menu: ref argMenu18, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // Warrior
                        {
                            objCharacter.SendPointOfInterest(1983.92f, -4794.2f, 6, 6, 0, "Hall of the Brave");
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 2565, Menu: ref argMenu19, qMenu: ref argqMenu22);
                            break;
                        }

                    case 24: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(1955.17f, -4475.79f, 6, 6, 0, "Yelmak's Alchemy and Potions");
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu23 = null;
                            objCharacter.SendGossip(cGUID, 2497, Menu: ref argMenu20, qMenu: ref argqMenu23);
                            break;
                        }

                    case 25: // Blacksmithing
                        {
                            objCharacter.SendPointOfInterest(2054.34f, -4831.85f, 6, 6, 0, "The Burning Anvil");
                            GossipMenu argMenu21 = null;
                            QuestMenu argqMenu24 = null;
                            objCharacter.SendGossip(cGUID, 2499, Menu: ref argMenu21, qMenu: ref argqMenu24);
                            break;
                        }

                    case 26: // Cooking
                        {
                            objCharacter.SendPointOfInterest(1780.96f, -4481.31f, 6, 6, 0, "Borstan's Firepit");
                            GossipMenu argMenu22 = null;
                            QuestMenu argqMenu25 = null;
                            objCharacter.SendGossip(cGUID, 2500, Menu: ref argMenu22, qMenu: ref argqMenu25);
                            break;
                        }

                    case 27: // Enchanting
                        {
                            objCharacter.SendPointOfInterest(1917.5f, -4434.95f, 6, 6, 0, "Godan's Runeworks");
                            GossipMenu argMenu23 = null;
                            QuestMenu argqMenu26 = null;
                            objCharacter.SendGossip(cGUID, 2501, Menu: ref argMenu23, qMenu: ref argqMenu26);
                            break;
                        }

                    case 28: // Engineering
                        {
                            objCharacter.SendPointOfInterest(2038.45f, -4744.75f, 6, 6, 0, "Nogg's Machine Shop");
                            GossipMenu argMenu24 = null;
                            QuestMenu argqMenu27 = null;
                            objCharacter.SendGossip(cGUID, 2653, Menu: ref argMenu24, qMenu: ref argqMenu27);
                            break;
                        }

                    case 29: // First Aid
                        {
                            objCharacter.SendPointOfInterest(1485.21f, -4160.91f, 6, 6, 0, "Survival of the Fittest");
                            GossipMenu argMenu25 = null;
                            QuestMenu argqMenu28 = null;
                            objCharacter.SendGossip(cGUID, 2502, Menu: ref argMenu25, qMenu: ref argqMenu28);
                            break;
                        }

                    case 30: // Fishing
                        {
                            objCharacter.SendPointOfInterest(1994.15f, -4655.7f, 6, 6, 0, "Lumak's Fishing");
                            GossipMenu argMenu26 = null;
                            QuestMenu argqMenu29 = null;
                            objCharacter.SendGossip(cGUID, 2503, Menu: ref argMenu26, qMenu: ref argqMenu29);
                            break;
                        }

                    case 31: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(1898.61f, -4454.93f, 6, 6, 0, "Jandi's Arboretum");
                            GossipMenu argMenu27 = null;
                            QuestMenu argqMenu30 = null;
                            objCharacter.SendGossip(cGUID, 2504, Menu: ref argMenu27, qMenu: ref argqMenu30);
                            break;
                        }

                    case 32: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(1852.82f, -4562.31f, 6, 6, 0, "Kodohide Leatherworkers");
                            GossipMenu argMenu28 = null;
                            QuestMenu argqMenu31 = null;
                            objCharacter.SendGossip(cGUID, 2513, Menu: ref argMenu28, qMenu: ref argqMenu31);
                            break;
                        }

                    case 33: // Mining
                        {
                            objCharacter.SendPointOfInterest(2029.79f, -4704.0f, 6, 6, 0, "Red Canyon Mining");
                            GossipMenu argMenu29 = null;
                            QuestMenu argqMenu32 = null;
                            objCharacter.SendGossip(cGUID, 2515, Menu: ref argMenu29, qMenu: ref argqMenu32);
                            break;
                        }

                    case 34: // Skinning
                        {
                            objCharacter.SendPointOfInterest(1852.82f, -4562.31f, 6, 6, 0, "Kodohide Leatherworkers");
                            GossipMenu argMenu30 = null;
                            QuestMenu argqMenu33 = null;
                            objCharacter.SendGossip(cGUID, 2516, Menu: ref argMenu30, qMenu: ref argqMenu33);
                            break;
                        }

                    case 35: // Tailoring
                        {
                            objCharacter.SendPointOfInterest(1802.66f, -4560.66f, 6, 6, 0, "Magar's Cloth Goods");
                            GossipMenu argMenu31 = null;
                            QuestMenu argqMenu34 = null;
                            objCharacter.SendGossip(cGUID, 2518, Menu: ref argMenu31, qMenu: ref argqMenu34);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Thunderbluff(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WINDRIDER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 11; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 3543, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Thunderbluff(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Bank
                        {
                            objCharacter.SendPointOfInterest(-1257.8f, 24.14f, 6, 6, 0, "Thunder Bluff Bank");
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 1292, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Wind Rider
                        {
                            objCharacter.SendPointOfInterest(-1196.43f, 28.26f, 6, 6, 0, "Wind Rider Roost");
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 1293, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Guild Master
                        {
                            objCharacter.SendPointOfInterest(-1296.5f, 127.57f, 6, 6, 0, "Thunder Bluff Civic Information");
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 1291, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Inn
                        {
                            objCharacter.SendPointOfInterest(-1296.0f, 39.7f, 6, 6, 0, "Thunder Bluff Inn");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 3153, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Mailbox
                        {
                            objCharacter.SendPointOfInterest(-1263.59f, 44.36f, 6, 6, 0, "Thunder Bluff Mailbox");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 3154, Menu: ref argMenu4, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Auction House
                        {
                            objCharacter.SendPointOfInterest(1381.77f, -4371.16f, 6, 6, 0, WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE);
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 3155, Menu: ref argMenu5, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Weapon Trainer
                        {
                            objCharacter.SendPointOfInterest(-1282.31f, 89.56f, 6, 6, 0, "Ansekhwa");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 4520, Menu: ref argMenu6, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(-1270.19f, 48.84f, 6, 6, 0, "Bulrug");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 5977, Menu: ref argMenu7, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Battlemasters
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH, (int)MenuIcon.MENUICON_GOSSIP);
                            objCharacter.TalkMenuTypes.Add(12);
                            objCharacter.TalkMenuTypes.Add(13);
                            objCharacter.TalkMenuTypes.Add(14);
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 7527, ref npcMenu, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SHAMAN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 15; i <= 20; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 3542, ref npcMenu, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 21; i <= 31; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 3541, ref npcMenu, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // AV
                        {
                            objCharacter.SendPointOfInterest(-1387.82f, -97.55f, 6, 6, 0, "Taim Ragetotem");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 7522, Menu: ref argMenu8, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // AB
                        {
                            objCharacter.SendPointOfInterest(-997.0f, 214.12f, 6, 6, 0, "Martin Lindsey");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 7648, Menu: ref argMenu9, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // WSG
                        {
                            objCharacter.SendPointOfInterest(-1384.94f, -75.91f, 6, 6, 0, "Kergul Bloodaxe");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 7523, Menu: ref argMenu10, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // Druid
                        {
                            objCharacter.SendPointOfInterest(-1054.47f, -285.0f, 6, 6, 0, "Hall of Elders");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 1294, Menu: ref argMenu11, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // Hunter
                        {
                            objCharacter.SendPointOfInterest(-1416.32f, -114.28f, 6, 6, 0, "Hunter's Hall");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 1295, Menu: ref argMenu12, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Mage
                        {
                            objCharacter.SendPointOfInterest(-1061.2f, 195.5f, 6, 6, 0, "Pools of Vision");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 1296, Menu: ref argMenu13, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Priest
                        {
                            objCharacter.SendPointOfInterest(-1061.2f, 195.5f, 6, 6, 0, "Pools of Vision");
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 1297, Menu: ref argMenu14, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Shaman
                        {
                            objCharacter.SendPointOfInterest(-989.54f, 278.25f, 6, 6, 0, "Hall of Spirits");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 1298, Menu: ref argMenu15, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Warrior
                        {
                            objCharacter.SendPointOfInterest(-1416.32f, -114.28f, 6, 6, 0, "Hunter's Hall");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 1299, Menu: ref argMenu16, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(-1085.56f, 27.29f, 6, 6, 0, "Bena's Alchemy");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 1332, Menu: ref argMenu17, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Blacksmithing
                        {
                            objCharacter.SendPointOfInterest(-1239.75f, 104.88f, 6, 6, 0, "Karn's Smithy");
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 1333, Menu: ref argMenu18, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // Cooking
                        {
                            objCharacter.SendPointOfInterest(-1214.5f, -21.23f, 6, 6, 0, "Aska's Kitchen");
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 1334, Menu: ref argMenu19, qMenu: ref argqMenu22);
                            break;
                        }

                    case 24: // Enchanting
                        {
                            objCharacter.SendPointOfInterest(-1112.65f, 48.26f, 6, 6, 0, "Dawnstrider Enchanters");
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu23 = null;
                            objCharacter.SendGossip(cGUID, 1335, Menu: ref argMenu20, qMenu: ref argqMenu23);
                            break;
                        }

                    case 25: // First Aid
                        {
                            objCharacter.SendPointOfInterest(-996.58f, 200.5f, 6, 6, 0, "Spiritual Healing");
                            GossipMenu argMenu21 = null;
                            QuestMenu argqMenu24 = null;
                            objCharacter.SendGossip(cGUID, 1336, Menu: ref argMenu21, qMenu: ref argqMenu24);
                            break;
                        }

                    case 26: // Fishing
                        {
                            objCharacter.SendPointOfInterest(-1169.35f, -68.87f, 6, 6, 0, "Mountaintop Bait & Tackle");
                            GossipMenu argMenu22 = null;
                            QuestMenu argqMenu25 = null;
                            objCharacter.SendGossip(cGUID, 1337, Menu: ref argMenu22, qMenu: ref argqMenu25);
                            break;
                        }

                    case 27: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(-1137.7f, -1.51f, 6, 6, 0, "Holistic Herbalism");
                            GossipMenu argMenu23 = null;
                            QuestMenu argqMenu26 = null;
                            objCharacter.SendGossip(cGUID, 1338, Menu: ref argMenu23, qMenu: ref argqMenu26);
                            break;
                        }

                    case 28: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(-1156.22f, 66.86f, 6, 6, 0, "Thunder Bluff Armorers");
                            GossipMenu argMenu24 = null;
                            QuestMenu argqMenu27 = null;
                            objCharacter.SendGossip(cGUID, 1339, Menu: ref argMenu24, qMenu: ref argqMenu27);
                            break;
                        }

                    case 29: // Mining
                        {
                            objCharacter.SendPointOfInterest(-1249.17f, 155.0f, 6, 6, 0, "Stonehoof Geology");
                            GossipMenu argMenu25 = null;
                            QuestMenu argqMenu28 = null;
                            objCharacter.SendGossip(cGUID, 1340, Menu: ref argMenu25, qMenu: ref argqMenu28);
                            break;
                        }

                    case 30: // Skinning
                        {
                            objCharacter.SendPointOfInterest(-1148.56f, 51.18f, 6, 6, 0, "Mooranta");
                            GossipMenu argMenu26 = null;
                            QuestMenu argqMenu29 = null;
                            objCharacter.SendGossip(cGUID, 1343, Menu: ref argMenu26, qMenu: ref argqMenu29);
                            break;
                        }

                    case 31: // Tailoring
                        {
                            objCharacter.SendPointOfInterest(-1156.22f, 66.86f, 6, 6, 0, "Thunder Bluff Armorers");
                            GossipMenu argMenu27 = null;
                            QuestMenu argqMenu30 = null;
                            objCharacter.SendGossip(cGUID, 1341, Menu: ref argMenu27, qMenu: ref argqMenu30);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Darnassus(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HIPPOGRYPH, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 11; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 3543, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Darnassus(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Auction House
                        {
                            objCharacter.SendPointOfInterest(9861.23f, 2334.55f, 6, 6, 0, "Darnassus Auction House");
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 3833, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Bank
                        {
                            objCharacter.SendPointOfInterest(9938.45f, 2512.35f, 6, 6, 0, "Darnassus Bank");
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 3017, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Hippogryph
                        {
                            objCharacter.SendPointOfInterest(9945.65f, 2618.94f, 6, 6, 0, "Rut'theran Village");
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 3018, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Guild Master
                        {
                            objCharacter.SendPointOfInterest(10076.4f, 2199.59f, 6, 6, 0, "Darnassus Guild Master");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 3019, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Inn
                        {
                            objCharacter.SendPointOfInterest(10133.29f, 2222.52f, 6, 6, 0, "Darnassus Inn");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 3020, Menu: ref argMenu4, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Mailbox
                        {
                            objCharacter.SendPointOfInterest(9942.17f, 2495.48f, 6, 6, 0, "Darnassus Mailbox");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 3021, Menu: ref argMenu5, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(10167.2f, 2522.66f, 6, 6, 0, "Alassin");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 5980, Menu: ref argMenu6, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Weapon Trainer
                        {
                            objCharacter.SendPointOfInterest(9907.11f, 2329.7f, 6, 6, 0, "Ilyenia Moonfire");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 4517, Menu: ref argMenu7, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Battlemasters
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH, (int)MenuIcon.MENUICON_GOSSIP);
                            objCharacter.TalkMenuTypes.Add(12);
                            objCharacter.TalkMenuTypes.Add(13);
                            objCharacter.TalkMenuTypes.Add(14);
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 7519, ref npcMenu, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 15; i <= 19; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 4264, ref npcMenu, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 20; i <= 28; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 4273, ref npcMenu, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // AV
                        {
                            objCharacter.SendPointOfInterest(9923.61f, 2327.43f, 6, 6, 0, "Brogun Stoneshield");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 7518, Menu: ref argMenu8, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // AB
                        {
                            objCharacter.SendPointOfInterest(9977.37f, 2324.39f, 6, 6, 0, "Keras Wolfheart");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 7651, Menu: ref argMenu9, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // WSG
                        {
                            objCharacter.SendPointOfInterest(9979.84f, 2315.79f, 6, 6, 0, "Aethalas");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 7482, Menu: ref argMenu10, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // Druid
                        {
                            objCharacter.SendPointOfInterest(10186.0f, 2570.46f, 6, 6, 0, "Darnassus Druid Trainer");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 3024, Menu: ref argMenu11, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // Hunter
                        {
                            objCharacter.SendPointOfInterest(10177.29f, 2511.1f, 6, 6, 0, "Darnassus Hunter Trainer");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 3023, Menu: ref argMenu12, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Priest
                        {
                            objCharacter.SendPointOfInterest(9659.12f, 2524.88f, 6, 6, 0, "Temple of the Moon");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 3025, Menu: ref argMenu13, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Rogue
                        {
                            objCharacter.SendPointOfInterest(10122.0f, 2599.12f, 6, 6, 0, "Darnassus Rogue Trainer");
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 3026, Menu: ref argMenu14, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Warrior
                        {
                            objCharacter.SendPointOfInterest(9951.91f, 2280.38f, 6, 6, 0, "Warrior's Terrace");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 3033, Menu: ref argMenu15, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(10075.9f, 2356.76f, 6, 6, 0, "Darnassus Alchemy Trainer");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 3035, Menu: ref argMenu16, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Cooking
                        {
                            objCharacter.SendPointOfInterest(10088.59f, 2419.21f, 6, 6, 0, "Darnassus Cooking Trainer");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 3036, Menu: ref argMenu17, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Enchanting
                        {
                            objCharacter.SendPointOfInterest(10146.09f, 2313.42f, 6, 6, 0, "Darnassus Enchanting Trainer");
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 3337, Menu: ref argMenu18, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // First Aid
                        {
                            objCharacter.SendPointOfInterest(10150.09f, 2390.43f, 6, 6, 0, "Darnassus First Aid Trainer");
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 3037, Menu: ref argMenu19, qMenu: ref argqMenu22);
                            break;
                        }

                    case 24: // Fishing
                        {
                            objCharacter.SendPointOfInterest(9836.2f, 2432.17f, 6, 6, 0, "Darnassus Fishing Trainer");
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu23 = null;
                            objCharacter.SendGossip(cGUID, 3038, Menu: ref argMenu20, qMenu: ref argqMenu23);
                            break;
                        }

                    case 25: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(9757.17f, 2430.16f, 6, 6, 0, "Darnassus Herbalism Trainer");
                            GossipMenu argMenu21 = null;
                            QuestMenu argqMenu24 = null;
                            objCharacter.SendGossip(cGUID, 3039, Menu: ref argMenu21, qMenu: ref argqMenu24);
                            break;
                        }

                    case 26: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(10086.59f, 2255.77f, 6, 6, 0, "Darnassus Leatherworking Trainer");
                            GossipMenu argMenu22 = null;
                            QuestMenu argqMenu25 = null;
                            objCharacter.SendGossip(cGUID, 3040, Menu: ref argMenu22, qMenu: ref argqMenu25);
                            break;
                        }

                    case 27: // Skinning
                        {
                            objCharacter.SendPointOfInterest(10081.4f, 2257.18f, 6, 6, 0, "Darnassus Skinning Trainer");
                            GossipMenu argMenu23 = null;
                            QuestMenu argqMenu26 = null;
                            objCharacter.SendGossip(cGUID, 3042, Menu: ref argMenu23, qMenu: ref argqMenu26);
                            break;
                        }

                    case 28: // Tailoring
                        {
                            objCharacter.SendPointOfInterest(10079.7f, 2268.19f, 6, 6, 0, "Darnassus Tailor");
                            GossipMenu argMenu24 = null;
                            QuestMenu argqMenu27 = null;
                            objCharacter.SendGossip(cGUID, 3044, Menu: ref argMenu24, qMenu: ref argqMenu27);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Ironforge(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_IRONFORGE_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DEEPRUNTRAM, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GRYPHON, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 12; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 933, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Ironforge(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Auctionhouse
                        {
                            objCharacter.SendPointOfInterest(-4957.39f, -911.6f, 6, 6, 0, "Ironforge Auction House");
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 3014, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Bank
                        {
                            objCharacter.SendPointOfInterest(-4891.91f, -991.47f, 6, 6, 0, "The Vault");
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 2761, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Deeprun Tram
                        {
                            objCharacter.SendPointOfInterest(-4835.27f, -1294.69f, 6, 6, 0, "Deeprun Tram");
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 3814, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Gryphon Master
                        {
                            objCharacter.SendPointOfInterest((float)-4821.52d, (float)-1152.3d, 6, 6, 0, "Ironforge Gryphon Master");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 2762, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Guild Master
                        {
                            objCharacter.SendPointOfInterest(-5021.0f, -996.45f, 6, 6, 0, "Ironforge Visitor's Center");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 2764, Menu: ref argMenu4, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Inn
                        {
                            objCharacter.SendPointOfInterest(-4850.47f, -872.57f, 6, 6, 0, "Stonefire Tavern");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 2768, Menu: ref argMenu5, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Mailbox
                        {
                            objCharacter.SendPointOfInterest(-4845.7f, -880.55f, 6, 6, 0, "Ironforge Mailbox");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 2769, Menu: ref argMenu6, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(-5010.2f, -1262.0f, 6, 6, 0, "Ulbrek Firehand");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 5986, Menu: ref argMenu7, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Weapon Trainer
                        {
                            objCharacter.SendPointOfInterest(-5040.0f, -1201.88f, 6, 6, 0, "Bixi and Buliwyf");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 4518, Menu: ref argMenu8, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Battlemasters
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH, (int)MenuIcon.MENUICON_GOSSIP);
                            objCharacter.TalkMenuTypes.Add(13);
                            objCharacter.TalkMenuTypes.Add(14);
                            objCharacter.TalkMenuTypes.Add(15);
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 7529, ref npcMenu, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PALADIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 16; i <= 22; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 2766, ref npcMenu, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 23; i <= 34; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 2793, ref npcMenu, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // AV
                        {
                            objCharacter.SendPointOfInterest(-5047.87f, -1263.77f, 6, 6, 0, "Glordrum Steelbeard");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 7483, Menu: ref argMenu9, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // AB
                        {
                            objCharacter.SendPointOfInterest(-5038.37f, -1266.39f, 6, 6, 0, "Donal Osgood");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 7649, Menu: ref argMenu10, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // WSG
                        {
                            objCharacter.SendPointOfInterest(-5037.24f, -1274.82f, 6, 6, 0, "Lylandris");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 7528, Menu: ref argMenu11, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // Hunter
                        {
                            objCharacter.SendPointOfInterest(-5023.0f, -1253.68f, 6, 6, 0, "Hall of Arms");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 2770, Menu: ref argMenu12, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Mage
                        {
                            objCharacter.SendPointOfInterest(-4627.0f, -926.45f, 6, 6, 0, "Hall of Mysteries");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 2771, Menu: ref argMenu13, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Paladin
                        {
                            objCharacter.SendPointOfInterest(-4627.02f, -926.45f, 6, 6, 0, "Hall of Mysteries");
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 2773, Menu: ref argMenu14, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Priest
                        {
                            objCharacter.SendPointOfInterest(-4627.0f, -926.45f, 6, 6, 0, "Hall of Mysteries");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 2772, Menu: ref argMenu15, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Rogue
                        {
                            objCharacter.SendPointOfInterest(-4647.83f, -1124.0f, 6, 6, 0, "Ironforge Rogue Trainer");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 2774, Menu: ref argMenu16, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Warlock
                        {
                            objCharacter.SendPointOfInterest(-4605.0f, -1110.45f, 6, 6, 0, "Ironforge Warlock Trainer");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 2775, Menu: ref argMenu17, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Warrior
                        {
                            objCharacter.SendPointOfInterest(-5023.08f, -1253.68f, 6, 6, 0, "Hall of Arms");
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 2776, Menu: ref argMenu18, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(-4858.5f, -1241.83f, 6, 6, 0, "Berryfizz's Potions and Mixed Drinks");
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 2794, Menu: ref argMenu19, qMenu: ref argqMenu22);
                            break;
                        }

                    case 24: // Blacksmithing
                        {
                            objCharacter.SendPointOfInterest(-4796.97f, -1110.17f, 6, 6, 0, "The Great Forge");
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu23 = null;
                            objCharacter.SendGossip(cGUID, 2795, Menu: ref argMenu20, qMenu: ref argqMenu23);
                            break;
                        }

                    case 25: // Cooking
                        {
                            objCharacter.SendPointOfInterest(-4767.83f, -1184.59f, 6, 6, 0, "The Bronze Kettle");
                            GossipMenu argMenu21 = null;
                            QuestMenu argqMenu24 = null;
                            objCharacter.SendGossip(cGUID, 2796, Menu: ref argMenu21, qMenu: ref argqMenu24);
                            break;
                        }

                    case 26: // Enchanting
                        {
                            objCharacter.SendPointOfInterest(-4803.72f, -1196.53f, 6, 6, 0, "Thistlefuzz Arcanery");
                            GossipMenu argMenu22 = null;
                            QuestMenu argqMenu25 = null;
                            objCharacter.SendGossip(cGUID, 2797, Menu: ref argMenu22, qMenu: ref argqMenu25);
                            break;
                        }

                    case 27: // Engineering
                        {
                            objCharacter.SendPointOfInterest(-4799.56f, -1250.23f, 6, 6, 0, "Springspindle's Gadgets");
                            GossipMenu argMenu23 = null;
                            QuestMenu argqMenu26 = null;
                            objCharacter.SendGossip(cGUID, 2798, Menu: ref argMenu23, qMenu: ref argqMenu26);
                            break;
                        }

                    case 28: // First Aid
                        {
                            objCharacter.SendPointOfInterest(-4881.6f, -1153.13f, 6, 6, 0, "Ironforge Physician");
                            GossipMenu argMenu24 = null;
                            QuestMenu argqMenu27 = null;
                            objCharacter.SendGossip(cGUID, 2799, Menu: ref argMenu24, qMenu: ref argqMenu27);
                            break;
                        }

                    case 29: // Fishing
                        {
                            objCharacter.SendPointOfInterest(-4597.91f, -1091.93f, 6, 6, 0, "Traveling Fisherman");
                            GossipMenu argMenu25 = null;
                            QuestMenu argqMenu28 = null;
                            objCharacter.SendGossip(cGUID, 2800, Menu: ref argMenu25, qMenu: ref argqMenu28);
                            break;
                        }

                    case 30: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(-4876.9f, -1151.92f, 6, 6, 0, "Ironforge Physician");
                            GossipMenu argMenu26 = null;
                            QuestMenu argqMenu29 = null;
                            objCharacter.SendGossip(cGUID, 2801, Menu: ref argMenu26, qMenu: ref argqMenu29);
                            break;
                        }

                    case 31: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(-4745.0f, -1027.57f, 6, 6, 0, "Finespindle's Leather Goods");
                            GossipMenu argMenu27 = null;
                            QuestMenu argqMenu30 = null;
                            objCharacter.SendGossip(cGUID, 2802, Menu: ref argMenu27, qMenu: ref argqMenu30);
                            break;
                        }

                    case 32: // Mining
                        {
                            objCharacter.SendPointOfInterest(-4705.06f, -1116.43f, 6, 6, 0, "Deepmountain Mining Guild");
                            GossipMenu argMenu28 = null;
                            QuestMenu argqMenu31 = null;
                            objCharacter.SendGossip(cGUID, 2804, Menu: ref argMenu28, qMenu: ref argqMenu31);
                            break;
                        }

                    case 33: // Skinning
                        {
                            objCharacter.SendPointOfInterest(-4745.0f, -1027.57f, 6, 6, 0, "Finespindle's Leather Goods");
                            GossipMenu argMenu29 = null;
                            QuestMenu argqMenu32 = null;
                            objCharacter.SendGossip(cGUID, 2805, Menu: ref argMenu29, qMenu: ref argqMenu32);
                            break;
                        }

                    case 34: // Tailoring
                        {
                            objCharacter.SendPointOfInterest(-4719.6f, -1056.96f, 6, 6, 0, "Stonebrow's Clothier");
                            GossipMenu argMenu30 = null;
                            QuestMenu argqMenu33 = null;
                            objCharacter.SendGossip(cGUID, 2807, Menu: ref argMenu30, qMenu: ref argqMenu33);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Undercity(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATHANDLER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ZEPPLINMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 12; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 3543, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Undercity(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Bank
                        {
                            objCharacter.SendPointOfInterest(1595.64f, 232.45f, 6, 6, 0, "Undercity Bank");
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 3514, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Bat Handler
                        {
                            objCharacter.SendPointOfInterest(1565.9f, 271.43f, 6, 6, 0, "Undercity Bat Handler");
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 3515, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Guild Master
                        {
                            objCharacter.SendPointOfInterest(1594.17f, 205.57f, 6, 6, 0, "Undercity Guild Master");
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 3516, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Inn
                        {
                            objCharacter.SendPointOfInterest(1639.43f, 220.99f, 6, 6, 0, "Undercity Inn");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 3517, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Mailbox
                        {
                            objCharacter.SendPointOfInterest(1632.68f, 219.4f, 6, 6, 0, "Undercity Mailbox");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 3518, Menu: ref argMenu4, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Auction House
                        {
                            objCharacter.SendPointOfInterest(1647.9f, 258.49f, 6, 6, 0, "Undercity Auction House");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 3519, Menu: ref argMenu5, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Zeppelin
                        {
                            objCharacter.SendPointOfInterest(2059.0f, 274.86f, 6, 6, 0, "Undercity Zeppelin");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 3520, Menu: ref argMenu6, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Weapon Trainer
                        {
                            objCharacter.SendPointOfInterest(1670.31f, 324.66f, 6, 6, 0, "Archibald");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 4521, Menu: ref argMenu7, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(1634.18f, 226.76f, 6, 6, 0, "Anya Maulray");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 5979, Menu: ref argMenu8, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Battlemasters
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH, (int)MenuIcon.MENUICON_GOSSIP);
                            objCharacter.TalkMenuTypes.Add(13);
                            objCharacter.TalkMenuTypes.Add(14);
                            objCharacter.TalkMenuTypes.Add(15);
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 7527, ref npcMenu, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 16; i <= 20; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 3542, ref npcMenu, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 21; i <= 32; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 3541, ref npcMenu, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // AV
                        {
                            objCharacter.SendPointOfInterest(1329.0f, 333.92f, 6, 6, 0, "Grizzle Halfmane");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 7525, Menu: ref argMenu9, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // AB
                        {
                            objCharacter.SendPointOfInterest(1283.3f, 287.16f, 6, 6, 0, "Sir Malory Wheeler");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 7646, Menu: ref argMenu10, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // WSG
                        {
                            objCharacter.SendPointOfInterest(1265.0f, 351.18f, 6, 6, 0, "Kurden Bloodclaw");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 7526, Menu: ref argMenu11, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // Mage
                        {
                            objCharacter.SendPointOfInterest(1781.0f, 53.0f, 6, 6, 0, "Undercity Mage Trainers");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 3513, Menu: ref argMenu12, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Priest
                        {
                            objCharacter.SendPointOfInterest(1758.33f, 401.5f, 6, 6, 0, "Undercity Priest Trainers");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 3521, Menu: ref argMenu13, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Rogue
                        {
                            objCharacter.SendPointOfInterest(1418.56f, 65.0f, 6, 6, 0, "Undercity Rogue Trainers");
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 3524, Menu: ref argMenu14, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Warlock
                        {
                            objCharacter.SendPointOfInterest(1780.92f, 53.16f, 6, 6, 0, "Undercity Warlock Trainers");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 3526, Menu: ref argMenu15, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Warrior
                        {
                            objCharacter.SendPointOfInterest(1775.59f, 418.19f, 6, 6, 0, "Undercity Warrior Trainers");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 3527, Menu: ref argMenu16, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(1419.82f, 417.19f, 6, 6, 0, "The Apothecarium");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 3528, Menu: ref argMenu17, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Blacksmithing
                        {
                            objCharacter.SendPointOfInterest(1696.0f, 285.0f, 6, 6, 0, "Undercity Blacksmithing Trainer");
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 3529, Menu: ref argMenu18, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // Cooking
                        {
                            objCharacter.SendPointOfInterest(1596.34f, 274.68f, 6, 6, 0, "Undercity Cooking Trainer");
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 3530, Menu: ref argMenu19, qMenu: ref argqMenu22);
                            break;
                        }

                    case 24: // Enchanting
                        {
                            objCharacter.SendPointOfInterest(1488.54f, 280.19f, 6, 6, 0, "Undercity Enchanting Trainer");
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu23 = null;
                            objCharacter.SendGossip(cGUID, 3531, Menu: ref argMenu20, qMenu: ref argqMenu23);
                            break;
                        }

                    case 25: // Engineering
                        {
                            objCharacter.SendPointOfInterest(1408.58f, 143.43f, 6, 6, 0, "Undercity Engineering Trainer");
                            GossipMenu argMenu21 = null;
                            QuestMenu argqMenu24 = null;
                            objCharacter.SendGossip(cGUID, 3532, Menu: ref argMenu21, qMenu: ref argqMenu24);
                            break;
                        }

                    case 26: // First Aid
                        {
                            objCharacter.SendPointOfInterest(1519.65f, 167.19f, 6, 6, 0, "Undercity First Aid Trainer");
                            GossipMenu argMenu22 = null;
                            QuestMenu argqMenu25 = null;
                            objCharacter.SendGossip(cGUID, 3533, Menu: ref argMenu22, qMenu: ref argqMenu25);
                            break;
                        }

                    case 27: // Fishing
                        {
                            objCharacter.SendPointOfInterest(1679.9f, 89.0f, 6, 6, 0, "Undercity Fishing Trainer");
                            GossipMenu argMenu23 = null;
                            QuestMenu argqMenu26 = null;
                            objCharacter.SendGossip(cGUID, 3534, Menu: ref argMenu23, qMenu: ref argqMenu26);
                            break;
                        }

                    case 28: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(1558.0f, 349.36f, 6, 6, 0, "Undercity Herbalism Trainer");
                            GossipMenu argMenu24 = null;
                            QuestMenu argqMenu27 = null;
                            objCharacter.SendGossip(cGUID, 3535, Menu: ref argMenu24, qMenu: ref argqMenu27);
                            break;
                        }

                    case 29: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(1498.76f, 196.43f, 6, 6, 0, "Undercity Leatherworking Trainer");
                            GossipMenu argMenu25 = null;
                            QuestMenu argqMenu28 = null;
                            objCharacter.SendGossip(cGUID, 3536, Menu: ref argMenu25, qMenu: ref argqMenu28);
                            break;
                        }

                    case 30: // Mining
                        {
                            objCharacter.SendPointOfInterest(1642.88f, 335.58f, 6, 6, 0, "Undercity Mining Trainer");
                            GossipMenu argMenu26 = null;
                            QuestMenu argqMenu29 = null;
                            objCharacter.SendGossip(cGUID, 3537, Menu: ref argMenu26, qMenu: ref argqMenu29);
                            break;
                        }

                    case 31: // Skinning
                        {
                            objCharacter.SendPointOfInterest(1498.6f, 196.46f, 6, 6, 0, "Undercity Skinning Trainer");
                            GossipMenu argMenu27 = null;
                            QuestMenu argqMenu30 = null;
                            objCharacter.SendGossip(cGUID, 3538, Menu: ref argMenu27, qMenu: ref argqMenu30);
                            break;
                        }

                    case 32: // Tailoring
                        {
                            objCharacter.SendPointOfInterest(1689.55f, 193.0f, 6, 6, 0, "Undercity Tailoring Trainer");
                            GossipMenu argMenu28 = null;
                            QuestMenu argqMenu31 = null;
                            objCharacter.SendGossip(cGUID, 3539, Menu: ref argMenu28, qMenu: ref argqMenu31);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Mulgore(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WINDRIDER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 6; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 3543, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Mulgore(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Bank
                        {
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 4051, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Wind Rider
                        {
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 4052, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Inn
                        {
                            objCharacter.SendPointOfInterest(-2361.38f, -349.19f, 6, 6, 0, "Bloodhoof Village Inn");
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 4053, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(-2338.86f, -357.56f, 6, 6, 0, "Seikwa");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 5976, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SHAMAN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 7; i <= 10; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 4069, ref npcMenu, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 11; i <= 21; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 4070, ref npcMenu, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Druid
                        {
                            objCharacter.SendPointOfInterest(-2312.15f, -443.69f, 6, 6, 0, "Gennia Runetotem");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 4054, Menu: ref argMenu4, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Hunter
                        {
                            objCharacter.SendPointOfInterest(-2178.14f, -406.14f, 6, 6, 0, "Yaw Sharpmane");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 4055, Menu: ref argMenu5, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Shaman
                        {
                            objCharacter.SendPointOfInterest(-2301.5f, -439.87f, 6, 6, 0, "Narm Skychaser");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 4056, Menu: ref argMenu6, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Warrior
                        {
                            objCharacter.SendPointOfInterest(-2345.43f, -494.11f, 6, 6, 0, "Krang Stonehoof");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 4057, Menu: ref argMenu7, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Alchemy
                        {
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 4058, Menu: ref argMenu8, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Blacksmithing
                        {
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 4059, Menu: ref argMenu9, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // Cooking
                        {
                            objCharacter.SendPointOfInterest(-2263.34f, -287.91f, 6, 6, 0, "Pyall Silentstride");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 4060, Menu: ref argMenu10, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // Enchanting
                        {
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 4061, Menu: ref argMenu11, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // First Aid
                        {
                            objCharacter.SendPointOfInterest(-2353.52f, -355.82f, 6, 6, 0, "Vira Younghoof");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 4062, Menu: ref argMenu12, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // Fishing
                        {
                            objCharacter.SendPointOfInterest(-2349.21f, -241.37f, 6, 6, 0, "Uthan Stillwater");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 4063, Menu: ref argMenu13, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Herbalism
                        {
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 4064, Menu: ref argMenu14, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(-2257.12f, -288.63f, 6, 6, 0, "Chaw Stronghide");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 4065, Menu: ref argMenu15, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Mining
                        {
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 4066, Menu: ref argMenu16, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Skinning
                        {
                            objCharacter.SendPointOfInterest(-2252.94f, -291.32f, 6, 6, 0, "Yonn Deepcut");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 4067, Menu: ref argMenu17, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Tailoring
                        {
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 4068, Menu: ref argMenu18, qMenu: ref argqMenu20);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Durotar(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WINDRIDER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 6; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 4037, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Durotar(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Bank
                        {
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 4032, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Wind Rider
                        {
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 4033, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Inn
                        {
                            objCharacter.SendPointOfInterest(338.7f, -4688.87f, 6, 6, 0, "Razor Hill Inn");
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 4034, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(330.31f, -4710.66f, 6, 6, 0, "Shoja'my");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 5973, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SHAMAN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 7; i <= 13; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 4035, ref npcMenu, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 14; i <= 25; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 4036, ref npcMenu, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Hunter
                        {
                            objCharacter.SendPointOfInterest(276.0f, -4706.72f, 6, 6, 0, "Thotar");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 4013, Menu: ref argMenu4, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Mage
                        {
                            objCharacter.SendPointOfInterest(-839.33f, -4935.6f, 6, 6, 0, "Un'Thuwa");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 4014, Menu: ref argMenu5, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Priest
                        {
                            objCharacter.SendPointOfInterest(296.22f, -4828.1f, 6, 6, 0, "Tai'jin");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 4015, Menu: ref argMenu6, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Rogue
                        {
                            objCharacter.SendPointOfInterest(265.76f, -4709.0f, 6, 6, 0, "Kaplak");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 4016, Menu: ref argMenu7, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Shaman
                        {
                            objCharacter.SendPointOfInterest(307.79f, -4836.97f, 6, 6, 0, "Swart");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 4017, Menu: ref argMenu8, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Warlock
                        {
                            objCharacter.SendPointOfInterest(355.88f, -4836.45f, 6, 6, 0, "Dhugru Gorelust");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 4018, Menu: ref argMenu9, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // Warrior
                        {
                            objCharacter.SendPointOfInterest(312.3f, -4824.66f, 6, 6, 0, "Tarshaw Jaggedscar");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 4019, Menu: ref argMenu10, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(-800.25f, -4894.33f, 6, 6, 0, "Miao'zan");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 4020, Menu: ref argMenu11, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // Blacksmithing
                        {
                            objCharacter.SendPointOfInterest(373.24f, -4716.45f, 6, 6, 0, "Dwukk");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 4021, Menu: ref argMenu12, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // Cooking
                        {
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 4022, Menu: ref argMenu13, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Enchanting
                        {
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 4023, Menu: ref argMenu14, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Engineering
                        {
                            objCharacter.SendPointOfInterest(368.95f, -4723.95f, 6, 6, 0, "Mukdrak");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 4024, Menu: ref argMenu15, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // First Aid
                        {
                            objCharacter.SendPointOfInterest(327.17f, -4825.62f, 6, 6, 0, "Rawrk");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 4025, Menu: ref argMenu16, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Fishing
                        {
                            objCharacter.SendPointOfInterest(-1065.48f, -4777.43f, 6, 6, 0, "Lau'Tiki");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 4026, Menu: ref argMenu17, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(-836.25f, -4896.89f, 6, 6, 0, "Mishiki");
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 4027, Menu: ref argMenu18, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Leatherworking
                        {
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 4028, Menu: ref argMenu19, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // Mining
                        {
                            objCharacter.SendPointOfInterest(366.94f, -4705.0f, 6, 6, 0, "Krunn");
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 4029, Menu: ref argMenu20, qMenu: ref argqMenu22);
                            break;
                        }

                    case 24: // Skinning
                        {
                            GossipMenu argMenu21 = null;
                            QuestMenu argqMenu23 = null;
                            objCharacter.SendGossip(cGUID, 4030, Menu: ref argMenu21, qMenu: ref argqMenu23);
                            break;
                        }

                    case 25: // Tailoring
                        {
                            GossipMenu argMenu22 = null;
                            QuestMenu argqMenu24 = null;
                            objCharacter.SendGossip(cGUID, 4031, Menu: ref argMenu22, qMenu: ref argqMenu24);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_ElwynnForest(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GRYPHON, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 7; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 933, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_ElwynnForest(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Bank
                        {
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 4260, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Gryphon Master
                        {
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 4261, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Guild Master
                        {
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 4262, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Inn
                        {
                            objCharacter.SendPointOfInterest(-9459.34f, 42.08f, 6, 6, 0, "Lion's Pride Inn");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 4263, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(-9466.62f, 45.87f, 6, 6, 0, "Erma");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 5983, Menu: ref argMenu4, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PALADIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 8; i <= 15; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 4264, ref npcMenu, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 16; i <= 27; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 4036, ref npcMenu, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Druid
                        {
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 4265, Menu: ref argMenu5, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Hunter
                        {
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 4266, Menu: ref argMenu6, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Mage
                        {
                            objCharacter.SendPointOfInterest(-9471.12f, 33.44f, 6, 6, 0, "Zaldimar Wefhellt");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 4015, Menu: ref argMenu7, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Paladin
                        {
                            objCharacter.SendPointOfInterest(-9469.0f, 108.05f, 6, 6, 0, "Brother Wilhelm");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 4269, Menu: ref argMenu8, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Priest
                        {
                            objCharacter.SendPointOfInterest(-9461.07f, 32.6f, 6, 6, 0, "Priestess Josetta");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 4267, Menu: ref argMenu9, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // Rogue
                        {
                            objCharacter.SendPointOfInterest(-9465.13f, 13.29f, 6, 6, 0, "Keryn Sylvius");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 4270, Menu: ref argMenu10, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // Warlock
                        {
                            objCharacter.SendPointOfInterest(-9473.21f, -4.08f, 6, 6, 0, "Maximillian Crowe");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 4272, Menu: ref argMenu11, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // Warrior
                        {
                            objCharacter.SendPointOfInterest(-9461.82f, 109.5f, 6, 6, 0, "Lyria Du Lac");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 4271, Menu: ref argMenu12, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(-9057.04f, 153.63f, 6, 6, 0, "Alchemist Mallory");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 4274, Menu: ref argMenu13, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Blacksmithing
                        {
                            objCharacter.SendPointOfInterest(-9456.58f, 87.9f, 6, 6, 0, "Smith Argus");
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 4275, Menu: ref argMenu14, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Cooking
                        {
                            objCharacter.SendPointOfInterest(-9467.54f, -3.16f, 6, 6, 0, "Tomas");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 4276, Menu: ref argMenu15, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Enchanting
                        {
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 4277, Menu: ref argMenu16, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Engineering
                        {
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 4278, Menu: ref argMenu17, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // First Aid
                        {
                            objCharacter.SendPointOfInterest(-9456.82f, 30.49f, 6, 6, 0, "Michelle Belle");
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 4279, Menu: ref argMenu18, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Fishing
                        {
                            objCharacter.SendPointOfInterest(-9386.54f, -118.73f, 6, 6, 0, "Lee Brown");
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 4280, Menu: ref argMenu19, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(-9060.7f, 149.23f, 6, 6, 0, "Herbalist Pomeroy");
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 4281, Menu: ref argMenu20, qMenu: ref argqMenu22);
                            break;
                        }

                    case 24: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(-9376.12f, -75.23f, 6, 6, 0, "Adele Fielder");
                            GossipMenu argMenu21 = null;
                            QuestMenu argqMenu23 = null;
                            objCharacter.SendGossip(cGUID, 4282, Menu: ref argMenu21, qMenu: ref argqMenu23);
                            break;
                        }

                    case 25: // Mining
                        {
                            GossipMenu argMenu22 = null;
                            QuestMenu argqMenu24 = null;
                            objCharacter.SendGossip(cGUID, 4283, Menu: ref argMenu22, qMenu: ref argqMenu24);
                            break;
                        }

                    case 26: // Skinning
                        {
                            objCharacter.SendPointOfInterest(-9536.91f, -1212.76f, 6, 6, 0, "Helene Peltskinner");
                            GossipMenu argMenu23 = null;
                            QuestMenu argqMenu25 = null;
                            objCharacter.SendGossip(cGUID, 4284, Menu: ref argMenu23, qMenu: ref argqMenu25);
                            break;
                        }

                    case 27: // Tailoring
                        {
                            objCharacter.SendPointOfInterest(-9376.12f, -75.23f, 6, 6, 0, "Eldrin");
                            GossipMenu argMenu24 = null;
                            QuestMenu argqMenu26 = null;
                            objCharacter.SendGossip(cGUID, 4285, Menu: ref argMenu24, qMenu: ref argqMenu26);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_DunMorogh(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HIPPOGRYPH, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 7; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 4287, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_DunMorogh(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Bank
                        {
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 4288, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Gryphon Master
                        {
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 4289, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Guild Master
                        {
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 4290, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Inn
                        {
                            objCharacter.SendPointOfInterest(-5582.66f, -525.89f, 6, 6, 0, "Thunderbrew Distillery");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 4291, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(-5604.0f, -509.58f, 6, 6, 0, "Shelby Stoneflint");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 5985, Menu: ref argMenu4, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PALADIN, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 8; i <= 14; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 4292, ref npcMenu, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 15; i <= 26; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 4300, ref npcMenu, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Hunter
                        {
                            objCharacter.SendPointOfInterest(-5618.29f, -454.25f, 6, 6, 0, "Grif Wildheart");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 4293, Menu: ref argMenu5, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Mage
                        {
                            objCharacter.SendPointOfInterest(-5585.6f, -539.99f, 6, 6, 0, "Magis Sparkmantle");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 4266, Menu: ref argMenu6, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Paladin
                        {
                            objCharacter.SendPointOfInterest(-5585.6f, -539.99f, 6, 6, 0, "Azar Stronghammer");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 4295, Menu: ref argMenu7, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Priest
                        {
                            objCharacter.SendPointOfInterest(-5591.74f, -525.61f, 6, 6, 0, "Maxan Anvol");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 4296, Menu: ref argMenu8, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Rogue
                        {
                            objCharacter.SendPointOfInterest(-5602.75f, -542.4f, 6, 6, 0, "Hogral Bakkan");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 4267, Menu: ref argMenu9, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // Warlock
                        {
                            objCharacter.SendPointOfInterest(-5641.97f, -523.76f, 6, 6, 0, "Gimrizz Shadowcog");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 4298, Menu: ref argMenu10, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // Warrior
                        {
                            objCharacter.SendPointOfInterest(-5604.79f, -529.38f, 6, 6, 0, "Granis Swiftaxe");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 4299, Menu: ref argMenu11, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // Alchemy
                        {
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 4301, Menu: ref argMenu12, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // Blacksmithing
                        {
                            objCharacter.SendPointOfInterest(-5584.72f, -428.41f, 6, 6, 0, "Tognus Flintfire");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 4302, Menu: ref argMenu13, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Cooking
                        {
                            objCharacter.SendPointOfInterest(-5596.85f, -541.43f, 6, 6, 0, "Gremlock Pilsnor");
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 4303, Menu: ref argMenu14, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Enchanting
                        {
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 4304, Menu: ref argMenu15, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Engineering
                        {
                            objCharacter.SendPointOfInterest(-5531.0f, -666.53f, 6, 6, 0, "Bronk Guzzlegear");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 4305, Menu: ref argMenu16, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // First Aid
                        {
                            objCharacter.SendPointOfInterest(-5603.67f, -523.57f, 6, 6, 0, "Thamner Pol");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 4306, Menu: ref argMenu17, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Fishing
                        {
                            objCharacter.SendPointOfInterest(-5199.9f, 58.58f, 6, 6, 0, "Paxton Ganter");
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 4307, Menu: ref argMenu18, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Herbalism
                        {
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 4308, Menu: ref argMenu19, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // Leatherworking
                        {
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 4310, Menu: ref argMenu20, qMenu: ref argqMenu22);
                            break;
                        }

                    case 24: // Mining
                        {
                            objCharacter.SendPointOfInterest(-5531, (float)-666.53d, 6, 6, 0, "Yarr Hamerstone");
                            GossipMenu argMenu21 = null;
                            QuestMenu argqMenu23 = null;
                            objCharacter.SendGossip(cGUID, 4311, Menu: ref argMenu21, qMenu: ref argqMenu23);
                            break;
                        }

                    case 25: // Skinning
                        {
                            GossipMenu argMenu22 = null;
                            QuestMenu argqMenu24 = null;
                            objCharacter.SendGossip(cGUID, 4312, Menu: ref argMenu22, qMenu: ref argqMenu24);
                            break;
                        }

                    case 26: // Tailoring
                        {
                            GossipMenu argMenu23 = null;
                            QuestMenu argqMenu25 = null;
                            objCharacter.SendGossip(cGUID, 4313, Menu: ref argMenu23, qMenu: ref argqMenu25);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Tirisfall(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATHANDLER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 6; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 4097, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Tirisfall(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Bank
                        {
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 4074, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Bat Handler
                        {
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 4075, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Inn
                        {
                            objCharacter.SendPointOfInterest(2246.68f, 241.89f, 6, 6, 0, "Gallows` End Tavern");
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 4076, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(2267.66f, 319.32f, 6, 6, 0, "Morganus");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 5978, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 7; i <= 11; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 4292, ref npcMenu, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 12; i <= 23; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 4096, ref npcMenu, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Mage
                        {
                            objCharacter.SendPointOfInterest(2259.18f, 240.93f, 6, 6, 0, "Cain Firesong");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 4077, Menu: ref argMenu4, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Priest
                        {
                            objCharacter.SendPointOfInterest(2259.18f, 240.93f, 6, 6, 0, "Dark Cleric Beryl");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 4078, Menu: ref argMenu5, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Rogue
                        {
                            objCharacter.SendPointOfInterest(2259.18f, 240.93f, 6, 6, 0, "Marion Call");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 4079, Menu: ref argMenu6, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Warlock
                        {
                            objCharacter.SendPointOfInterest(2259.18f, 240.93f, 6, 6, 0, "Rupert Boch");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 4080, Menu: ref argMenu7, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Warrior
                        {
                            objCharacter.SendPointOfInterest(2256.48f, 240.32f, 6, 6, 0, "Austil de Mon");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 4081, Menu: ref argMenu8, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(2263.25f, 344.23f, 6, 6, 0, "Carolai Anise");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 4082, Menu: ref argMenu9, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // Blacksmithing
                        {
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 4083, Menu: ref argMenu10, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // Cooking
                        {
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 4084, Menu: ref argMenu11, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // Enchanting
                        {
                            objCharacter.SendPointOfInterest(2250.35f, 249.12f, 6, 6, 0, "Vance Undergloom");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 4085, Menu: ref argMenu12, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // Engineering
                        {
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 4086, Menu: ref argMenu13, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // First Aid
                        {
                            objCharacter.SendPointOfInterest(2246.68f, 241.89f, 6, 6, 0, "Nurse Neela");
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 4087, Menu: ref argMenu14, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Fishing
                        {
                            objCharacter.SendPointOfInterest(2292.37f, -10.72f, 6, 6, 0, "Clyde Kellen");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 4088, Menu: ref argMenu15, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(2268.21f, 331.69f, 6, 6, 0, "Faruza");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 4089, Menu: ref argMenu16, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(2027.0f, 78.72f, 6, 6, 0, "Shelene Rhobart");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 4090, Menu: ref argMenu17, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Mining
                        {
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 4091, Menu: ref argMenu18, qMenu: ref argqMenu20);
                            break;
                        }

                    case 22: // Skinning
                        {
                            objCharacter.SendPointOfInterest(2027.0f, 78.72f, 6, 6, 0, "Rand Rhobart");
                            GossipMenu argMenu19 = null;
                            QuestMenu argqMenu21 = null;
                            objCharacter.SendGossip(cGUID, 4092, Menu: ref argMenu19, qMenu: ref argqMenu21);
                            break;
                        }

                    case 23: // Tailoring
                        {
                            objCharacter.SendPointOfInterest(2160.45f, 659.93f, 6, 6, 0, "Bowen Brisboise");
                            GossipMenu argMenu20 = null;
                            QuestMenu argqMenu22 = null;
                            objCharacter.SendGossip(cGUID, 4093, Menu: ref argMenu20, qMenu: ref argqMenu22);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void OnGossipHello_Teldrassil(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
            {
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FERRY, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER, (int)MenuIcon.MENUICON_GOSSIP);
                for (int i = 1; i <= 7; i++)
                    objCharacter.TalkMenuTypes.Add(i);
                QuestMenu argqMenu = null;
                objCharacter.SendGossip(cGUID, 4316, ref npcMenu, qMenu: ref argqMenu);
            }

            private void OnGossipSelect_Teldrassil(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
            {
                // TODO: These hardcoded values need to be replaced by values from either the DB or DBC's
                switch (objCharacter.TalkMenuTypes[Selected])
                {
                    case 1: // Bank
                        {
                            GossipMenu argMenu = null;
                            QuestMenu argqMenu = null;
                            objCharacter.SendGossip(cGUID, 4317, Menu: ref argMenu, qMenu: ref argqMenu);
                            break;
                        }

                    case 2: // Ferry
                        {
                            GossipMenu argMenu1 = null;
                            QuestMenu argqMenu1 = null;
                            objCharacter.SendGossip(cGUID, 4318, Menu: ref argMenu1, qMenu: ref argqMenu1);
                            break;
                        }

                    case 3: // Guild Master
                        {
                            GossipMenu argMenu2 = null;
                            QuestMenu argqMenu2 = null;
                            objCharacter.SendGossip(cGUID, 4319, Menu: ref argMenu2, qMenu: ref argqMenu2);
                            break;
                        }

                    case 4: // Inn
                        {
                            objCharacter.SendPointOfInterest(9821.49f, 960.13f, 6, 6, 0, "Dolanaar Inn");
                            GossipMenu argMenu3 = null;
                            QuestMenu argqMenu3 = null;
                            objCharacter.SendGossip(cGUID, 4320, Menu: ref argMenu3, qMenu: ref argqMenu3);
                            break;
                        }

                    case 5: // Stable Master
                        {
                            objCharacter.SendPointOfInterest(9808.37f, 931.1f, 6, 6, 0, "Seriadne");
                            GossipMenu argMenu4 = null;
                            QuestMenu argqMenu4 = null;
                            objCharacter.SendGossip(cGUID, 5982, Menu: ref argMenu4, qMenu: ref argqMenu4);
                            break;
                        }

                    case 6: // Class trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 8; i <= 12; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu5 = null;
                            objCharacter.SendGossip(cGUID, 4264, ref npcMenu, qMenu: ref argqMenu5);
                            break;
                        }

                    case 7: // Profession trainers
                        {
                            var npcMenu = new GossipMenu();
                            objCharacter.TalkMenuTypes.Clear();
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING, (int)MenuIcon.MENUICON_GOSSIP);
                            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING, (int)MenuIcon.MENUICON_GOSSIP);
                            for (int i = 13; i <= 21; i++)
                                objCharacter.TalkMenuTypes.Add(i);
                            QuestMenu argqMenu6 = null;
                            objCharacter.SendGossip(cGUID, 4273, ref npcMenu, qMenu: ref argqMenu6);
                            break;
                        }

                    case 8: // Druid
                        {
                            objCharacter.SendPointOfInterest(9741.58f, 963.7f, 6, 6, 0, "Kal");
                            GossipMenu argMenu5 = null;
                            QuestMenu argqMenu7 = null;
                            objCharacter.SendGossip(cGUID, 4323, Menu: ref argMenu5, qMenu: ref argqMenu7);
                            break;
                        }

                    case 9: // Hunter
                        {
                            objCharacter.SendPointOfInterest(9815.12f, 926.28f, 6, 6, 0, "Dazalar");
                            GossipMenu argMenu6 = null;
                            QuestMenu argqMenu8 = null;
                            objCharacter.SendGossip(cGUID, 4324, Menu: ref argMenu6, qMenu: ref argqMenu8);
                            break;
                        }

                    case 10: // Priest
                        {
                            objCharacter.SendPointOfInterest(9906.16f, 986.63f, 6, 6, 0, "Laurna Morninglight");
                            GossipMenu argMenu7 = null;
                            QuestMenu argqMenu9 = null;
                            objCharacter.SendGossip(cGUID, 4325, Menu: ref argMenu7, qMenu: ref argqMenu9);
                            break;
                        }

                    case 11: // Rogue
                        {
                            objCharacter.SendPointOfInterest(9789.0f, 942.86f, 6, 6, 0, "Jannok Breezesong");
                            GossipMenu argMenu8 = null;
                            QuestMenu argqMenu10 = null;
                            objCharacter.SendGossip(cGUID, 4326, Menu: ref argMenu8, qMenu: ref argqMenu10);
                            break;
                        }

                    case 12: // Warrior
                        {
                            objCharacter.SendPointOfInterest(9821.96f, 950.61f, 6, 6, 0, "Kyra Windblade");
                            GossipMenu argMenu9 = null;
                            QuestMenu argqMenu11 = null;
                            objCharacter.SendGossip(cGUID, 4327, Menu: ref argMenu9, qMenu: ref argqMenu11);
                            break;
                        }

                    case 13: // Alchemy
                        {
                            objCharacter.SendPointOfInterest(9767.59f, 878.81f, 6, 6, 0, "Cyndra Kindwhisper");
                            GossipMenu argMenu10 = null;
                            QuestMenu argqMenu12 = null;
                            objCharacter.SendGossip(cGUID, 4329, Menu: ref argMenu10, qMenu: ref argqMenu12);
                            break;
                        }

                    case 14: // Cooking
                        {
                            objCharacter.SendPointOfInterest(9751.19f, 906.13f, 6, 6, 0, "Zarrin");
                            GossipMenu argMenu11 = null;
                            QuestMenu argqMenu13 = null;
                            objCharacter.SendGossip(cGUID, 4330, Menu: ref argMenu11, qMenu: ref argqMenu13);
                            break;
                        }

                    case 15: // Enchanting
                        {
                            objCharacter.SendPointOfInterest(10677.59f, 1946.56f, 6, 6, 0, "Alanna Raveneye");
                            GossipMenu argMenu12 = null;
                            QuestMenu argqMenu14 = null;
                            objCharacter.SendGossip(cGUID, 4331, Menu: ref argMenu12, qMenu: ref argqMenu14);
                            break;
                        }

                    case 16: // First Aid
                        {
                            objCharacter.SendPointOfInterest(9903.12f, 999.0f, 6, 6, 0, "Byancie");
                            GossipMenu argMenu13 = null;
                            QuestMenu argqMenu15 = null;
                            objCharacter.SendGossip(cGUID, 4332, Menu: ref argMenu13, qMenu: ref argqMenu15);
                            break;
                        }

                    case 17: // Fishing
                        {
                            GossipMenu argMenu14 = null;
                            QuestMenu argqMenu16 = null;
                            objCharacter.SendGossip(cGUID, 4333, Menu: ref argMenu14, qMenu: ref argqMenu16);
                            break;
                        }

                    case 18: // Herbalism
                        {
                            objCharacter.SendPointOfInterest(9773.78f, 875.88f, 6, 6, 0, "Malorne Bladeleaf");
                            GossipMenu argMenu15 = null;
                            QuestMenu argqMenu17 = null;
                            objCharacter.SendGossip(cGUID, 4334, Menu: ref argMenu15, qMenu: ref argqMenu17);
                            break;
                        }

                    case 19: // Leatherworking
                        {
                            objCharacter.SendPointOfInterest(10152.59f, 1681.46f, 6, 6, 0, "Nadyia Maneweaver");
                            GossipMenu argMenu16 = null;
                            QuestMenu argqMenu18 = null;
                            objCharacter.SendGossip(cGUID, 4335, Menu: ref argMenu16, qMenu: ref argqMenu18);
                            break;
                        }

                    case 20: // Skinning
                        {
                            objCharacter.SendPointOfInterest(10135.59f, 1673.18f, 6, 6, 0, "Radnaal Maneweaver");
                            GossipMenu argMenu17 = null;
                            QuestMenu argqMenu19 = null;
                            objCharacter.SendGossip(cGUID, 4336, Menu: ref argMenu17, qMenu: ref argqMenu19);
                            break;
                        }

                    case 21: // Tailoring
                        {
                            GossipMenu argMenu18 = null;
                            QuestMenu argqMenu20 = null;
                            objCharacter.SendGossip(cGUID, 4337, Menu: ref argMenu18, qMenu: ref argqMenu20);
                            break;
                        }
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        }
    }
}