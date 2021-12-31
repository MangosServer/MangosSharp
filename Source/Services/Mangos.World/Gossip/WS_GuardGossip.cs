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

using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Gossip;
using Mangos.World.Objects;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Gossip;

public class WS_GuardGossip
{
    public class TGuardTalk : TBaseTalk
    {
        public override void OnGossipHello(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            var Gossip = GetGossip(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].ID);
            switch (Gossip)
            {
                case Gossips.Darnassus:
                    OnGossipHello_Darnassus(ref objCharacter, cGUID);
                    break;

                case Gossips.DunMorogh:
                    OnGossipHello_DunMorogh(ref objCharacter, cGUID);
                    break;

                case Gossips.Durotar:
                    OnGossipHello_Durotar(ref objCharacter, cGUID);
                    break;

                case Gossips.ElwynnForest:
                    OnGossipHello_ElwynnForest(ref objCharacter, cGUID);
                    break;

                case Gossips.Ironforge:
                    OnGossipHello_Ironforge(ref objCharacter, cGUID);
                    break;

                case Gossips.Mulgore:
                    OnGossipHello_Mulgore(ref objCharacter, cGUID);
                    break;

                case Gossips.Orgrimmar:
                    OnGossipHello_Orgrimmar(ref objCharacter, cGUID);
                    break;

                case Gossips.Stormwind:
                    OnGossipHello_Stormwind(ref objCharacter, cGUID);
                    break;

                case Gossips.Teldrassil:
                    OnGossipHello_Teldrassil(ref objCharacter, cGUID);
                    break;

                case Gossips.Thunderbluff:
                    OnGossipHello_Thunderbluff(ref objCharacter, cGUID);
                    break;

                case Gossips.Tirisfall:
                    OnGossipHello_Tirisfall(ref objCharacter, cGUID);
                    break;

                case Gossips.Undercity:
                    OnGossipHello_Undercity(ref objCharacter, cGUID);
                    break;

                default:
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Unknown gossip [{0}].", Gossip);
                    break;
            }
        }

        public override void OnGossipSelect(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int selected)
        {
            var Gossip = GetGossip(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].ID);
            switch (Gossip)
            {
                case Gossips.Darnassus:
                    OnGossipSelect_Darnassus(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.DunMorogh:
                    OnGossipSelect_DunMorogh(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.Durotar:
                    OnGossipSelect_Durotar(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.ElwynnForest:
                    OnGossipSelect_ElwynnForest(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.Ironforge:
                    OnGossipSelect_Ironforge(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.Mulgore:
                    OnGossipSelect_Mulgore(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.Orgrimmar:
                    OnGossipSelect_Orgrimmar(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.Stormwind:
                    OnGossipSelect_Stormwind(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.Teldrassil:
                    OnGossipSelect_Teldrassil(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.Thunderbluff:
                    OnGossipSelect_Thunderbluff(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.Tirisfall:
                    OnGossipSelect_Tirisfall(ref objCharacter, cGUID, selected);
                    break;

                case Gossips.Undercity:
                    OnGossipSelect_Undercity(ref objCharacter, cGUID, selected);
                    break;

                default:
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Unknown gossip [{0}].", Gossip);
                    break;
            }
        }

        public Gossips GetGossip(int Entry)
        {
            switch (Entry)
            {
                case 3084:
                    return Gossips.Thunderbluff;

                case 4262:
                    return Gossips.Darnassus;

                case 3296:
                    return Gossips.Orgrimmar;

                case 68:
                case 1976:
                    return Gossips.Stormwind;

                case 1423:
                    return Gossips.ElwynnForest;

                case 5595:
                    return Gossips.Ironforge;

                case 727:
                    return Gossips.DunMorogh;

                case 5953:
                    return Gossips.Durotar;

                case 3571:
                    return Gossips.Teldrassil;

                case 5624:
                    return Gossips.Undercity;

                case 1496:
                case 1652:
                case 1738:
                case 1742:
                case 1743:
                case 1744:
                case 1745:
                case 1746:
                case 5725:
                    return Gossips.Tirisfall;

                default:
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Creature Entry [{0}] was not found in guard table.", Entry);
                    return Gossips.Thunderbluff;
            }
        }

        private void OnGossipHello_Stormwind(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            objCharacter.TalkMenuTypes.Clear();
            GossipMenu npcMenu = new();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STORMWIND_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DEEPRUNTRAM);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GRYPHON);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_OFFICERS);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 13);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 933, npcMenu, qMenu);
        }

        private void OnGossipSelect_Stormwind(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8811.46f, 667.46f, 6, 6, 0, "Stormwind Auction House");
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 3834, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8916.87f, 622.87f, 6, 6, 0, "Stormwind Bank");
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 764, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8378.88f, 554.23f, 6, 6, 0, "The Deeprun Tram");
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 3813, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8869f, 675.4f, 6, 6, 0, "The Gilded Rose");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 3860, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8837f, 493.5f, 6, 6, 0, "Stormwind Gryphon Master");
                    var obj5 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 879, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8894f, 611.2f, 6, 6, 0, "Stormwind Vistor`s Center");
                    var obj6 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 882, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8876.48f, 649.18f, 6, 6, 0, "Stormwind Mailbox");
                    var obj7 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 3861, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8433f, 554.7f, 6, 6, 0, "Jenova Stoneshield");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 5984, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8797f, 612.8f, 6, 6, 0, "Woo Ping");
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 4516, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8759.92f, 399.69f, 6, 6, 0, "Champions` Hall");
                    var obj10 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 7047, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    GossipMenu npcMenu3 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH);
                    objCharacter.TalkMenuTypes.Add(14);
                    objCharacter.TalkMenuTypes.Add(15);
                    objCharacter.TalkMenuTypes.Add(16);
                    var obj11 = objCharacter;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 7499, npcMenu3, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PALADIN);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK);
                    var j = 17;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 24);
                    var obj12 = objCharacter;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 898, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 25;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 36);
                    var obj13 = objCharacter;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 918, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8443.88f, 335.99f, 6, 6, 0, "Thelman Slatefist");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 7500, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8443.88f, 335.99f, 6, 6, 0, "Lady Hoteshem");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 7650, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8443.88f, 335.99f, 6, 6, 0, "Elfarran");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 7501, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9012f, 867.6f, 6, 6, 0, "Wizard`s Sanctum");
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 899, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8753f, 367.8f, 6, 6, 0, "Stormwind - Rogue House");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 900, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8690.11f, 324.85f, 6, 6, 0, "Command Center");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 901, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8751f, 1124.5f, 6, 6, 0, "The Park");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 902, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8512f, 862.4f, 6, 6, 0, "Catedral Of Light");
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 903, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8577f, 881.7f, 6, 6, 0, "Catedral Of Light");
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 904, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8413f, 541.5f, 6, 6, 0, "Hunter Lodge");
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 905, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 24, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8948.91f, 998.35f, 6, 6, 0, "The Slaughtered Lamb");
                    var obj24 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj24.SendGossip(cGUID, 906, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 25, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8988f, 759.6f, 6, 6, 0, "Alchemy Needs");
                    var obj25 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj25.SendGossip(cGUID, 919, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 26, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8424f, 616.9f, 6, 6, 0, "Therum Deepforge");
                    var obj26 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj26.SendGossip(cGUID, 920, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 27, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8611f, 364.6f, 6, 6, 0, "Pig and Whistle Tavern");
                    var obj27 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj27.SendGossip(cGUID, 921, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 28, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8858f, 803.7f, 6, 6, 0, "Lucan Cordell");
                    var obj28 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj28.SendGossip(cGUID, 941, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 29, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8347f, 644.1f, 6, 6, 0, "Lilliam Sparkspindle");
                    var obj29 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj29.SendGossip(cGUID, 922, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 30, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8513f, 801.8f, 6, 6, 0, "Shaina Fuller");
                    var obj30 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj30.SendGossip(cGUID, 923, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 31, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8803f, 767.5f, 6, 6, 0, "Arnold Leland");
                    var obj31 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj31.SendGossip(cGUID, 940, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 32, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8967f, 779.5f, 6, 6, 0, "Alchemy Needs");
                    var obj32 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj32.SendGossip(cGUID, 924, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 33, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8726f, 477.4f, 6, 6, 0, "The Protective Hide");
                    var obj33 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj33.SendGossip(cGUID, 925, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 34, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8434f, 692.8f, 6, 6, 0, "Gelman Stonehand");
                    var obj34 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj34.SendGossip(cGUID, 927, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 35, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8716f, 469.4f, 6, 6, 0, "The Protective Hide");
                    var obj35 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj35.SendGossip(cGUID, 928, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 36, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-8938f, 800.7f, 6, 6, 0, "Duncan`s Textiles");
                    var obj36 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj36.SendGossip(cGUID, 929, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_Orgrimmar(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WINDRIDER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ZEPPLINMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_OFFICERS);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 13);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 2593, npcMenu, qMenu);
        }

        private void OnGossipSelect_Orgrimmar(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1631.51f, -4375.33f, 6, 6, 0, "Bank of Orgrimmar");
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 2554, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1676.6f, -4332.72f, 6, 6, 0, "The Sky Tower");
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 2555, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1576.93f, -4294.75f, 6, 6, 0, "Horde Embassy");
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 2556, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1644.51f, -4447.27f, 6, 6, 0, "Orgrimmar Inn");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 2557, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1622.53f, -4388.79f, 6, 6, 0, "Orgrimmar Mailbox");
                    var obj5 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 2558, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1679.21f, -4450.1f, 6, 6, 0, "Orgrimmar Auction House");
                    var obj6 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 3075, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1337.36f, -4632.7f, 6, 6, 0, "Orgrimmar Zeppelin Tower");
                    var obj7 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 3173, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2092.56f, -4823.95f, 6, 6, 0, "Sayoc & Hanashi");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 4519, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2133.12f, -4663.93f, 6, 6, 0, "Xon'cha");
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 5974, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1633.56f, -4249.37f, 6, 6, 0, "Hall of Legends");
                    var obj10 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 7046, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    GossipMenu npcMenu3 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH);
                    objCharacter.TalkMenuTypes.Add(14);
                    objCharacter.TalkMenuTypes.Add(15);
                    objCharacter.TalkMenuTypes.Add(16);
                    var obj11 = objCharacter;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 7521, npcMenu3, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SHAMAN);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 17;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 23);
                    var obj12 = objCharacter;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 2599, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 24;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 35);
                    var obj13 = objCharacter;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 2594, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1983.92f, -4794.2f, 6, 6, 0, "Hall of the Brave");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 7484, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1983.92f, -4794.2f, 6, 6, 0, "Hall of the Brave");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 7644, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1983.92f, -4794.2f, 6, 6, 0, "Hall of the Brave");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 7520, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2114.84f, -4625.31f, 6, 6, 0, "Orgrimmar Hunter's Hall");
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 2559, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1451.26f, -4223.33f, 6, 6, 0, "Darkbriar Lodge");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 2560, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1442.21f, -4183.24f, 6, 6, 0, "Spirit Lodge");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 2561, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1925.34f, -4181.89f, 6, 6, 0, "Thrall's Fortress");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 2562, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1773.39f, -4278.97f, 6, 6, 0, "Shadowswift Brotherhood");
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 2563, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1849.57f, -4359.68f, 6, 6, 0, "Darkfire Enclave");
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 2564, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1983.92f, -4794.2f, 6, 6, 0, "Hall of the Brave");
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 2565, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 24, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1955.17f, -4475.79f, 6, 6, 0, "Yelmak's Alchemy and Potions");
                    var obj24 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj24.SendGossip(cGUID, 2497, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 25, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2054.34f, -4831.85f, 6, 6, 0, "The Burning Anvil");
                    var obj25 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj25.SendGossip(cGUID, 2499, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 26, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1780.96f, -4481.31f, 6, 6, 0, "Borstan's Firepit");
                    var obj26 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj26.SendGossip(cGUID, 2500, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 27, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1917.5f, -4434.95f, 6, 6, 0, "Godan's Runeworks");
                    var obj27 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj27.SendGossip(cGUID, 2501, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 28, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2038.45f, -4744.75f, 6, 6, 0, "Nogg's Machine Shop");
                    var obj28 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj28.SendGossip(cGUID, 2653, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 29, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1485.21f, -4160.91f, 6, 6, 0, "Survival of the Fittest");
                    var obj29 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj29.SendGossip(cGUID, 2502, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 30, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1994.15f, -4655.7f, 6, 6, 0, "Lumak's Fishing");
                    var obj30 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj30.SendGossip(cGUID, 2503, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 31, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1898.61f, -4454.93f, 6, 6, 0, "Jandi's Arboretum");
                    var obj31 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj31.SendGossip(cGUID, 2504, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 32, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1852.82f, -4562.31f, 6, 6, 0, "Kodohide Leatherworkers");
                    var obj32 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj32.SendGossip(cGUID, 2513, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 33, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2029.79f, -4704f, 6, 6, 0, "Red Canyon Mining");
                    var obj33 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj33.SendGossip(cGUID, 2515, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 34, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1852.82f, -4562.31f, 6, 6, 0, "Kodohide Leatherworkers");
                    var obj34 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj34.SendGossip(cGUID, 2516, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 35, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1802.66f, -4560.66f, 6, 6, 0, "Magar's Cloth Goods");
                    var obj35 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj35.SendGossip(cGUID, 2518, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_Thunderbluff(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WINDRIDER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 11);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 3543, npcMenu, qMenu);
        }

        private void OnGossipSelect_Thunderbluff(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1257.8f, 24.14f, 6, 6, 0, "Thunder Bluff Bank");
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 1292, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1196.43f, 28.26f, 6, 6, 0, "Wind Rider Roost");
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 1293, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1296.5f, 127.57f, 6, 6, 0, "Thunder Bluff Civic Information");
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 1291, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1296f, 39.7f, 6, 6, 0, "Thunder Bluff Inn");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 3153, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1263.59f, 44.36f, 6, 6, 0, "Thunder Bluff Mailbox");
                    var obj5 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 3154, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1381.77f, -4371.16f, 6, 6, 0, WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE);
                    var obj6 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 3155, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1282.31f, 89.56f, 6, 6, 0, "Ansekhwa");
                    var obj7 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 4520, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1270.19f, 48.84f, 6, 6, 0, "Bulrug");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 5977, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    GossipMenu npcMenu3 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH);
                    objCharacter.TalkMenuTypes.Add(12);
                    objCharacter.TalkMenuTypes.Add(13);
                    objCharacter.TalkMenuTypes.Add(14);
                    var obj9 = objCharacter;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 7527, npcMenu3, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SHAMAN);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 15;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 20);
                    var obj10 = objCharacter;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 3542, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 21;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 31);
                    var obj11 = objCharacter;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 3541, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1387.82f, -97.55f, 6, 6, 0, "Taim Ragetotem");
                    var obj12 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 7522, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-997f, 214.12f, 6, 6, 0, "Martin Lindsey");
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 7648, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1384.94f, -75.91f, 6, 6, 0, "Kergul Bloodaxe");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 7523, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1054.47f, -285f, 6, 6, 0, "Hall of Elders");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 1294, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1416.32f, -114.28f, 6, 6, 0, "Hunter's Hall");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 1295, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1061.2f, 195.5f, 6, 6, 0, "Pools of Vision");
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 1296, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1061.2f, 195.5f, 6, 6, 0, "Pools of Vision");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 1297, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-989.54f, 278.25f, 6, 6, 0, "Hall of Spirits");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 1298, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1416.32f, -114.28f, 6, 6, 0, "Hunter's Hall");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 1299, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1085.56f, 27.29f, 6, 6, 0, "Bena's Alchemy");
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 1332, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1239.75f, 104.88f, 6, 6, 0, "Karn's Smithy");
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 1333, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1214.5f, -21.23f, 6, 6, 0, "Aska's Kitchen");
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 1334, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 24, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1112.65f, 48.26f, 6, 6, 0, "Dawnstrider Enchanters");
                    var obj24 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj24.SendGossip(cGUID, 1335, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 25, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-996.58f, 200.5f, 6, 6, 0, "Spiritual Healing");
                    var obj25 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj25.SendGossip(cGUID, 1336, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 26, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1169.35f, -68.87f, 6, 6, 0, "Mountaintop Bait & Tackle");
                    var obj26 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj26.SendGossip(cGUID, 1337, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 27, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1137.7f, -1.51f, 6, 6, 0, "Holistic Herbalism");
                    var obj27 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj27.SendGossip(cGUID, 1338, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 28, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1156.22f, 66.86f, 6, 6, 0, "Thunder Bluff Armorers");
                    var obj28 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj28.SendGossip(cGUID, 1339, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 29, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1249.17f, 155f, 6, 6, 0, "Stonehoof Geology");
                    var obj29 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj29.SendGossip(cGUID, 1340, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 30, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1148.56f, 51.18f, 6, 6, 0, "Mooranta");
                    var obj30 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj30.SendGossip(cGUID, 1343, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 31, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1156.22f, 66.86f, 6, 6, 0, "Thunder Bluff Armorers");
                    var obj31 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj31.SendGossip(cGUID, 1341, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_Darnassus(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HIPPOGRYPH);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 11);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 3543, npcMenu, qMenu);
        }

        private void OnGossipSelect_Darnassus(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9861.23f, 2334.55f, 6, 6, 0, "Darnassus Auction House");
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 3833, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9938.45f, 2512.35f, 6, 6, 0, "Darnassus Bank");
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 3017, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9945.65f, 2618.94f, 6, 6, 0, "Rut'theran Village");
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 3018, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10076.4f, 2199.59f, 6, 6, 0, "Darnassus Guild Master");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 3019, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10133.29f, 2222.52f, 6, 6, 0, "Darnassus Inn");
                    var obj5 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 3020, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9942.17f, 2495.48f, 6, 6, 0, "Darnassus Mailbox");
                    var obj6 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 3021, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10167.2f, 2522.66f, 6, 6, 0, "Alassin");
                    var obj7 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 5980, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9907.11f, 2329.7f, 6, 6, 0, "Ilyenia Moonfire");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 4517, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    GossipMenu npcMenu3 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH);
                    objCharacter.TalkMenuTypes.Add(12);
                    objCharacter.TalkMenuTypes.Add(13);
                    objCharacter.TalkMenuTypes.Add(14);
                    var obj9 = objCharacter;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 7519, npcMenu3, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 15;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 19);
                    var obj10 = objCharacter;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 4264, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 20;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 28);
                    var obj11 = objCharacter;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 4273, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9923.61f, 2327.43f, 6, 6, 0, "Brogun Stoneshield");
                    var obj12 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 7518, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9977.37f, 2324.39f, 6, 6, 0, "Keras Wolfheart");
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 7651, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9979.84f, 2315.79f, 6, 6, 0, "Aethalas");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 7482, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10186f, 2570.46f, 6, 6, 0, "Darnassus Druid Trainer");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 3024, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10177.29f, 2511.1f, 6, 6, 0, "Darnassus Hunter Trainer");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 3023, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9659.12f, 2524.88f, 6, 6, 0, "Temple of the Moon");
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 3025, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10122f, 2599.12f, 6, 6, 0, "Darnassus Rogue Trainer");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 3026, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9951.91f, 2280.38f, 6, 6, 0, "Warrior's Terrace");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 3033, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10075.9f, 2356.76f, 6, 6, 0, "Darnassus Alchemy Trainer");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 3035, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10088.59f, 2419.21f, 6, 6, 0, "Darnassus Cooking Trainer");
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 3036, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10146.09f, 2313.42f, 6, 6, 0, "Darnassus Enchanting Trainer");
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 3337, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10150.09f, 2390.43f, 6, 6, 0, "Darnassus First Aid Trainer");
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 3037, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 24, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9836.2f, 2432.17f, 6, 6, 0, "Darnassus Fishing Trainer");
                    var obj24 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj24.SendGossip(cGUID, 3038, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 25, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9757.17f, 2430.16f, 6, 6, 0, "Darnassus Herbalism Trainer");
                    var obj25 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj25.SendGossip(cGUID, 3039, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 26, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10086.59f, 2255.77f, 6, 6, 0, "Darnassus Leatherworking Trainer");
                    var obj26 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj26.SendGossip(cGUID, 3040, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 27, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10081.4f, 2257.18f, 6, 6, 0, "Darnassus Skinning Trainer");
                    var obj27 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj27.SendGossip(cGUID, 3042, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 28, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10079.7f, 2268.19f, 6, 6, 0, "Darnassus Tailor");
                    var obj28 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj28.SendGossip(cGUID, 3044, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_Ironforge(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_IRONFORGE_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DEEPRUNTRAM);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GRYPHON);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 12);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 933, npcMenu, qMenu);
        }

        private void OnGossipSelect_Ironforge(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4957.39f, -911.6f, 6, 6, 0, "Ironforge Auction House");
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 3014, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4891.91f, -991.47f, 6, 6, 0, "The Vault");
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 2761, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4835.27f, -1294.69f, 6, 6, 0, "Deeprun Tram");
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 3814, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4821.52f, -1152.3f, 6, 6, 0, "Ironforge Gryphon Master");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 2762, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5021f, -996.45f, 6, 6, 0, "Ironforge Visitor's Center");
                    var obj5 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 2764, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4850.47f, -872.57f, 6, 6, 0, "Stonefire Tavern");
                    var obj6 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 2768, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4845.7f, -880.55f, 6, 6, 0, "Ironforge Mailbox");
                    var obj7 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 2769, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5010.2f, -1262f, 6, 6, 0, "Ulbrek Firehand");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 5986, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5040f, -1201.88f, 6, 6, 0, "Bixi and Buliwyf");
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 4518, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    GossipMenu npcMenu3 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH);
                    objCharacter.TalkMenuTypes.Add(13);
                    objCharacter.TalkMenuTypes.Add(14);
                    objCharacter.TalkMenuTypes.Add(15);
                    var obj10 = objCharacter;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 7529, npcMenu3, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PALADIN);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 16;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 22);
                    var obj11 = objCharacter;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 2766, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 23;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 34);
                    var obj12 = objCharacter;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 2793, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5047.87f, -1263.77f, 6, 6, 0, "Glordrum Steelbeard");
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 7483, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5038.37f, -1266.39f, 6, 6, 0, "Donal Osgood");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 7649, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5037.24f, -1274.82f, 6, 6, 0, "Lylandris");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 7528, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5023f, -1253.68f, 6, 6, 0, "Hall of Arms");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 2770, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4627f, -926.45f, 6, 6, 0, "Hall of Mysteries");
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 2771, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4627.02f, -926.45f, 6, 6, 0, "Hall of Mysteries");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 2773, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4627f, -926.45f, 6, 6, 0, "Hall of Mysteries");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 2772, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4647.83f, -1124f, 6, 6, 0, "Ironforge Rogue Trainer");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 2774, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4605f, -1110.45f, 6, 6, 0, "Ironforge Warlock Trainer");
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 2775, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5023.08f, -1253.68f, 6, 6, 0, "Hall of Arms");
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 2776, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4858.5f, -1241.83f, 6, 6, 0, "Berryfizz's Potions and Mixed Drinks");
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 2794, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 24, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4796.97f, -1110.17f, 6, 6, 0, "The Great Forge");
                    var obj24 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj24.SendGossip(cGUID, 2795, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 25, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4767.83f, -1184.59f, 6, 6, 0, "The Bronze Kettle");
                    var obj25 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj25.SendGossip(cGUID, 2796, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 26, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4803.72f, -1196.53f, 6, 6, 0, "Thistlefuzz Arcanery");
                    var obj26 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj26.SendGossip(cGUID, 2797, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 27, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4799.56f, -1250.23f, 6, 6, 0, "Springspindle's Gadgets");
                    var obj27 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj27.SendGossip(cGUID, 2798, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 28, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4881.6f, -1153.13f, 6, 6, 0, "Ironforge Physician");
                    var obj28 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj28.SendGossip(cGUID, 2799, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 29, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4597.91f, -1091.93f, 6, 6, 0, "Traveling Fisherman");
                    var obj29 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj29.SendGossip(cGUID, 2800, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 30, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4876.9f, -1151.92f, 6, 6, 0, "Ironforge Physician");
                    var obj30 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj30.SendGossip(cGUID, 2801, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 31, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4745f, -1027.57f, 6, 6, 0, "Finespindle's Leather Goods");
                    var obj31 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj31.SendGossip(cGUID, 2802, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 32, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4705.06f, -1116.43f, 6, 6, 0, "Deepmountain Mining Guild");
                    var obj32 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj32.SendGossip(cGUID, 2804, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 33, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4745f, -1027.57f, 6, 6, 0, "Finespindle's Leather Goods");
                    var obj33 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj33.SendGossip(cGUID, 2805, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 34, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-4719.6f, -1056.96f, 6, 6, 0, "Stonebrow's Clothier");
                    var obj34 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj34.SendGossip(cGUID, 2807, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_Undercity(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATHANDLER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAILBOX);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_AUCTIONHOUSE);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ZEPPLINMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WEAPONMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATTLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 12);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 3543, npcMenu, qMenu);
        }

        private void OnGossipSelect_Undercity(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1595.64f, 232.45f, 6, 6, 0, "Undercity Bank");
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 3514, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1565.9f, 271.43f, 6, 6, 0, "Undercity Bat Handler");
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 3515, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1594.17f, 205.57f, 6, 6, 0, "Undercity Guild Master");
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 3516, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1639.43f, 220.99f, 6, 6, 0, "Undercity Inn");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 3517, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1632.68f, 219.4f, 6, 6, 0, "Undercity Mailbox");
                    var obj5 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 3518, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1647.9f, 258.49f, 6, 6, 0, "Undercity Auction House");
                    var obj6 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 3519, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2059f, 274.86f, 6, 6, 0, "Undercity Zeppelin");
                    var obj7 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 3520, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1670.31f, 324.66f, 6, 6, 0, "Archibald");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 4521, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1634.18f, 226.76f, 6, 6, 0, "Anya Maulray");
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 5979, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    GossipMenu npcMenu3 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALTERACVALLEY);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ARATHIBASIN);
                    npcMenu3.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARSONGULCH);
                    objCharacter.TalkMenuTypes.Add(13);
                    objCharacter.TalkMenuTypes.Add(14);
                    objCharacter.TalkMenuTypes.Add(15);
                    var obj10 = objCharacter;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 7527, npcMenu3, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 16;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 20);
                    var obj11 = objCharacter;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 3542, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 21;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 32);
                    var obj12 = objCharacter;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 3541, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1329f, 333.92f, 6, 6, 0, "Grizzle Halfmane");
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 7525, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1283.3f, 287.16f, 6, 6, 0, "Sir Malory Wheeler");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 7646, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1265f, 351.18f, 6, 6, 0, "Kurden Bloodclaw");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 7526, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1781f, 53f, 6, 6, 0, "Undercity Mage Trainers");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 3513, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1758.33f, 401.5f, 6, 6, 0, "Undercity Priest Trainers");
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 3521, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1418.56f, 65f, 6, 6, 0, "Undercity Rogue Trainers");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 3524, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1780.92f, 53.16f, 6, 6, 0, "Undercity Warlock Trainers");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 3526, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1775.59f, 418.19f, 6, 6, 0, "Undercity Warrior Trainers");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 3527, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1419.82f, 417.19f, 6, 6, 0, "The Apothecarium");
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 3528, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1696f, 285f, 6, 6, 0, "Undercity Blacksmithing Trainer");
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 3529, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1596.34f, 274.68f, 6, 6, 0, "Undercity Cooking Trainer");
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 3530, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 24, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1488.54f, 280.19f, 6, 6, 0, "Undercity Enchanting Trainer");
                    var obj24 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj24.SendGossip(cGUID, 3531, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 25, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1408.58f, 143.43f, 6, 6, 0, "Undercity Engineering Trainer");
                    var obj25 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj25.SendGossip(cGUID, 3532, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 26, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1519.65f, 167.19f, 6, 6, 0, "Undercity First Aid Trainer");
                    var obj26 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj26.SendGossip(cGUID, 3533, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 27, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1679.9f, 89f, 6, 6, 0, "Undercity Fishing Trainer");
                    var obj27 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj27.SendGossip(cGUID, 3534, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 28, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1558f, 349.36f, 6, 6, 0, "Undercity Herbalism Trainer");
                    var obj28 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj28.SendGossip(cGUID, 3535, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 29, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1498.76f, 196.43f, 6, 6, 0, "Undercity Leatherworking Trainer");
                    var obj29 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj29.SendGossip(cGUID, 3536, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 30, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1642.88f, 335.58f, 6, 6, 0, "Undercity Mining Trainer");
                    var obj30 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj30.SendGossip(cGUID, 3537, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 31, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1498.6f, 196.46f, 6, 6, 0, "Undercity Skinning Trainer");
                    var obj31 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj31.SendGossip(cGUID, 3538, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 32, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(1689.55f, 193f, 6, 6, 0, "Undercity Tailoring Trainer");
                    var obj32 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj32.SendGossip(cGUID, 3539, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_Mulgore(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WINDRIDER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 6);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 3543, npcMenu, qMenu);
        }

        private void OnGossipSelect_Mulgore(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 4051, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 4052, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2361.38f, -349.19f, 6, 6, 0, "Bloodhoof Village Inn");
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 4053, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2338.86f, -357.56f, 6, 6, 0, "Seikwa");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 5976, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SHAMAN);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 7;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 10);
                    var obj5 = objCharacter;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 4069, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 11;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 21);
                    var obj6 = objCharacter;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 4070, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2312.15f, -443.69f, 6, 6, 0, "Gennia Runetotem");
                    var obj7 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 4054, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2178.14f, -406.14f, 6, 6, 0, "Yaw Sharpmane");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 4055, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2301.5f, -439.87f, 6, 6, 0, "Narm Skychaser");
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 4056, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2345.43f, -494.11f, 6, 6, 0, "Krang Stonehoof");
                    var obj10 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 4057, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    var obj11 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 4058, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    var obj12 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 4059, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2263.34f, -287.91f, 6, 6, 0, "Pyall Silentstride");
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 4060, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 4061, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2353.52f, -355.82f, 6, 6, 0, "Vira Younghoof");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 4062, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2349.21f, -241.37f, 6, 6, 0, "Uthan Stillwater");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 4063, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 4064, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2257.12f, -288.63f, 6, 6, 0, "Chaw Stronghide");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 4065, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 4066, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-2252.94f, -291.32f, 6, 6, 0, "Yonn Deepcut");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 4067, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 4068, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_Durotar(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WINDRIDER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 6);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 4037, npcMenu, qMenu);
        }

        private void OnGossipSelect_Durotar(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 4032, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 4033, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(338.7f, -4688.87f, 6, 6, 0, "Razor Hill Inn");
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 4034, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(330.31f, -4710.66f, 6, 6, 0, "Shoja'my");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 5973, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SHAMAN);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 7;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 13);
                    var obj5 = objCharacter;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 4035, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 14;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 25);
                    var obj6 = objCharacter;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 4036, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(276f, -4706.72f, 6, 6, 0, "Thotar");
                    var obj7 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 4013, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-839.33f, -4935.6f, 6, 6, 0, "Un'Thuwa");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 4014, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(296.22f, -4828.1f, 6, 6, 0, "Tai'jin");
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 4015, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(265.76f, -4709f, 6, 6, 0, "Kaplak");
                    var obj10 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 4016, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(307.79f, -4836.97f, 6, 6, 0, "Swart");
                    var obj11 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 4017, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(355.88f, -4836.45f, 6, 6, 0, "Dhugru Gorelust");
                    var obj12 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 4018, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(312.3f, -4824.66f, 6, 6, 0, "Tarshaw Jaggedscar");
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 4019, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-800.25f, -4894.33f, 6, 6, 0, "Miao'zan");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 4020, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(373.24f, -4716.45f, 6, 6, 0, "Dwukk");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 4021, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 4022, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 4023, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(368.95f, -4723.95f, 6, 6, 0, "Mukdrak");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 4024, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(327.17f, -4825.62f, 6, 6, 0, "Rawrk");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 4025, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-1065.48f, -4777.43f, 6, 6, 0, "Lau'Tiki");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 4026, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-836.25f, -4896.89f, 6, 6, 0, "Mishiki");
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 4027, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 4028, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(366.94f, -4705f, 6, 6, 0, "Krunn");
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 4029, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 24, TextCompare: false))
                {
                    var obj24 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj24.SendGossip(cGUID, 4030, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 25, TextCompare: false))
                {
                    var obj25 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj25.SendGossip(cGUID, 4031, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_ElwynnForest(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GRYPHON);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 7);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 933, npcMenu, qMenu);
        }

        private void OnGossipSelect_ElwynnForest(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 4260, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 4261, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 4262, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9459.34f, 42.08f, 6, 6, 0, "Lion's Pride Inn");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 4263, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9466.62f, 45.87f, 6, 6, 0, "Erma");
                    var obj5 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 5983, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PALADIN);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 8;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 15);
                    var obj6 = objCharacter;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 4264, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 16;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 27);
                    var obj7 = objCharacter;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 4036, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 4265, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 4266, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9471.12f, 33.44f, 6, 6, 0, "Zaldimar Wefhellt");
                    var obj10 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 4015, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9469f, 108.05f, 6, 6, 0, "Brother Wilhelm");
                    var obj11 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 4269, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9461.07f, 32.6f, 6, 6, 0, "Priestess Josetta");
                    var obj12 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 4267, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9465.13f, 13.29f, 6, 6, 0, "Keryn Sylvius");
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 4270, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9473.21f, -4.08f, 6, 6, 0, "Maximillian Crowe");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 4272, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9461.82f, 109.5f, 6, 6, 0, "Lyria Du Lac");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 4271, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9057.04f, 153.63f, 6, 6, 0, "Alchemist Mallory");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 4274, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9456.58f, 87.9f, 6, 6, 0, "Smith Argus");
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 4275, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9467.54f, -3.16f, 6, 6, 0, "Tomas");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 4276, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 4277, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 4278, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9456.82f, 30.49f, 6, 6, 0, "Michelle Belle");
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 4279, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9386.54f, -118.73f, 6, 6, 0, "Lee Brown");
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 4280, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9060.7f, 149.23f, 6, 6, 0, "Herbalist Pomeroy");
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 4281, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 24, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9376.12f, -75.23f, 6, 6, 0, "Adele Fielder");
                    var obj24 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj24.SendGossip(cGUID, 4282, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 25, TextCompare: false))
                {
                    var obj25 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj25.SendGossip(cGUID, 4283, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 26, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9536.91f, -1212.76f, 6, 6, 0, "Helene Peltskinner");
                    var obj26 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj26.SendGossip(cGUID, 4284, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 27, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-9376.12f, -75.23f, 6, 6, 0, "Eldrin");
                    var obj27 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj27.SendGossip(cGUID, 4285, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_DunMorogh(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HIPPOGRYPH);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 7);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 4287, npcMenu, qMenu);
        }

        private void OnGossipSelect_DunMorogh(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 4288, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 4289, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 4290, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5582.66f, -525.89f, 6, 6, 0, "Thunderbrew Distillery");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 4291, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5604f, -509.58f, 6, 6, 0, "Shelby Stoneflint");
                    var obj5 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 5985, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PALADIN);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 8;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 14);
                    var obj6 = objCharacter;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 4292, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 15;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 26);
                    var obj7 = objCharacter;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 4300, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5618.29f, -454.25f, 6, 6, 0, "Grif Wildheart");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 4293, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5585.6f, -539.99f, 6, 6, 0, "Magis Sparkmantle");
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 4266, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5585.6f, -539.99f, 6, 6, 0, "Azar Stronghammer");
                    var obj10 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 4295, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5591.74f, -525.61f, 6, 6, 0, "Maxan Anvol");
                    var obj11 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 4296, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5602.75f, -542.4f, 6, 6, 0, "Hogral Bakkan");
                    var obj12 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 4267, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5641.97f, -523.76f, 6, 6, 0, "Gimrizz Shadowcog");
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 4298, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5604.79f, -529.38f, 6, 6, 0, "Granis Swiftaxe");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 4299, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 4301, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5584.72f, -428.41f, 6, 6, 0, "Tognus Flintfire");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 4302, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5596.85f, -541.43f, 6, 6, 0, "Gremlock Pilsnor");
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 4303, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 4304, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5531f, -666.53f, 6, 6, 0, "Bronk Guzzlegear");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 4305, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5603.67f, -523.57f, 6, 6, 0, "Thamner Pol");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 4306, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5199.9f, 58.58f, 6, 6, 0, "Paxton Ganter");
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 4307, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 4308, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 4310, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 24, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(-5531f, -666.53f, 6, 6, 0, "Yarr Hamerstone");
                    var obj24 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj24.SendGossip(cGUID, 4311, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 25, TextCompare: false))
                {
                    var obj25 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj25.SendGossip(cGUID, 4312, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 26, TextCompare: false))
                {
                    var obj26 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj26.SendGossip(cGUID, 4313, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_Tirisfall(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BATHANDLER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 6);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 4097, npcMenu, qMenu);
        }

        private void OnGossipSelect_Tirisfall(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 4074, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 4075, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2246.68f, 241.89f, 6, 6, 0, "Gallows` End Tavern");
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 4076, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2267.66f, 319.32f, 6, 6, 0, "Morganus");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 5978, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MAGE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARLOCK);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 7;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 11);
                    var obj5 = objCharacter;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 4292, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BLACKSMITHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENGINEERING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_MINING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 12;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 23);
                    var obj6 = objCharacter;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 4096, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2259.18f, 240.93f, 6, 6, 0, "Cain Firesong");
                    var obj7 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 4077, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2259.18f, 240.93f, 6, 6, 0, "Dark Cleric Beryl");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 4078, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2259.18f, 240.93f, 6, 6, 0, "Marion Call");
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 4079, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2259.18f, 240.93f, 6, 6, 0, "Rupert Boch");
                    var obj10 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 4080, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2256.48f, 240.32f, 6, 6, 0, "Austil de Mon");
                    var obj11 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 4081, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2263.25f, 344.23f, 6, 6, 0, "Carolai Anise");
                    var obj12 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 4082, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 4083, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 4084, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2250.35f, 249.12f, 6, 6, 0, "Vance Undergloom");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 4085, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 4086, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2246.68f, 241.89f, 6, 6, 0, "Nurse Neela");
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 4087, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2292.37f, -10.72f, 6, 6, 0, "Clyde Kellen");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 4088, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2268.21f, 331.69f, 6, 6, 0, "Faruza");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 4089, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2027f, 78.72f, 6, 6, 0, "Shelene Rhobart");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 4090, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 4091, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 22, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2027f, 78.72f, 6, 6, 0, "Rand Rhobart");
                    var obj22 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj22.SendGossip(cGUID, 4092, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 23, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(2160.45f, 659.93f, 6, 6, 0, "Bowen Brisboise");
                    var obj23 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj23.SendGossip(cGUID, 4093, Menu, qMenu);
                }
            }
        }

        private void OnGossipHello_Teldrassil(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_BANK);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FERRY);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_GUILDMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_INN);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_STABLEMASTER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_CLASSTRAINER);
            npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PROFTRAINER);
            var i = 1;
            do
            {
                objCharacter.TalkMenuTypes.Add(i);
                i = checked(i + 1);
            }
            while (i <= 7);
            var obj = objCharacter;
            QuestMenu qMenu = null;
            obj.SendGossip(cGUID, 4316, npcMenu, qMenu);
        }

        private void OnGossipSelect_Teldrassil(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int Selected)
        {
            var left = objCharacter.TalkMenuTypes[Selected];
            checked
            {
                if (Operators.ConditionalCompareObjectEqual(left, 1, TextCompare: false))
                {
                    var obj = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj.SendGossip(cGUID, 4317, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 2, TextCompare: false))
                {
                    var obj2 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj2.SendGossip(cGUID, 4318, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 3, TextCompare: false))
                {
                    var obj3 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj3.SendGossip(cGUID, 4319, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 4, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9821.49f, 960.13f, 6, 6, 0, "Dolanaar Inn");
                    var obj4 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj4.SendGossip(cGUID, 4320, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 5, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9808.37f, 931.1f, 6, 6, 0, "Seriadne");
                    var obj5 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj5.SendGossip(cGUID, 5982, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 6, TextCompare: false))
                {
                    GossipMenu npcMenu2 = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_DRUID);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HUNTER);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_PRIEST);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ROGUE);
                    npcMenu2.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_WARRIOR);
                    var j = 8;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(j);
                        j++;
                    }
                    while (j <= 12);
                    var obj6 = objCharacter;
                    QuestMenu qMenu = null;
                    obj6.SendGossip(cGUID, 4264, npcMenu2, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 7, TextCompare: false))
                {
                    GossipMenu npcMenu = new();
                    objCharacter.TalkMenuTypes.Clear();
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ALCHEMY);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_COOKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_ENCHANTING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FIRSTAID);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_FISHING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_HERBALISM);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_LEATHERWORKING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_SKINNING);
                    npcMenu.AddMenu(WorldServiceLocator._Global_Constants.GOSSIP_TEXT_TAILORING);
                    var i = 13;
                    do
                    {
                        objCharacter.TalkMenuTypes.Add(i);
                        i++;
                    }
                    while (i <= 21);
                    var obj7 = objCharacter;
                    QuestMenu qMenu = null;
                    obj7.SendGossip(cGUID, 4273, npcMenu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 8, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9741.58f, 963.7f, 6, 6, 0, "Kal");
                    var obj8 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj8.SendGossip(cGUID, 4323, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 9, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9815.12f, 926.28f, 6, 6, 0, "Dazalar");
                    var obj9 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj9.SendGossip(cGUID, 4324, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 10, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9906.16f, 986.63f, 6, 6, 0, "Laurna Morninglight");
                    var obj10 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj10.SendGossip(cGUID, 4325, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 11, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9789f, 942.86f, 6, 6, 0, "Jannok Breezesong");
                    var obj11 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj11.SendGossip(cGUID, 4326, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 12, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9821.96f, 950.61f, 6, 6, 0, "Kyra Windblade");
                    var obj12 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj12.SendGossip(cGUID, 4327, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 13, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9767.59f, 878.81f, 6, 6, 0, "Cyndra Kindwhisper");
                    var obj13 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj13.SendGossip(cGUID, 4329, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 14, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9751.19f, 906.13f, 6, 6, 0, "Zarrin");
                    var obj14 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj14.SendGossip(cGUID, 4330, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 15, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10677.59f, 1946.56f, 6, 6, 0, "Alanna Raveneye");
                    var obj15 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj15.SendGossip(cGUID, 4331, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 16, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9903.12f, 999f, 6, 6, 0, "Byancie");
                    var obj16 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj16.SendGossip(cGUID, 4332, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 17, TextCompare: false))
                {
                    var obj17 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj17.SendGossip(cGUID, 4333, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 18, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(9773.78f, 875.88f, 6, 6, 0, "Malorne Bladeleaf");
                    var obj18 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj18.SendGossip(cGUID, 4334, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 19, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10152.59f, 1681.46f, 6, 6, 0, "Nadyia Maneweaver");
                    var obj19 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj19.SendGossip(cGUID, 4335, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 20, TextCompare: false))
                {
                    objCharacter.SendPointOfInterest(10135.59f, 1673.18f, 6, 6, 0, "Radnaal Maneweaver");
                    var obj20 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj20.SendGossip(cGUID, 4336, Menu, qMenu);
                }
                else if (Operators.ConditionalCompareObjectEqual(left, 21, TextCompare: false))
                {
                    var obj21 = objCharacter;
                    GossipMenu Menu = null;
                    QuestMenu qMenu = null;
                    obj21.SendGossip(cGUID, 4337, Menu, qMenu);
                }
            }
        }
    }
}
