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
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Quest;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Player;
using Mangos.World.Quests;
using Mangos.World.Spells;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Functions = Mangos.World.Globals.Functions;

namespace Mangos.World.Objects;

public class WS_NPCs
{
    public class TDefaultTalk : TBaseTalk
    {
        public override void OnGossipHello(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
        {
            var textID = 0;
            GossipMenu npcMenu = new();
            objCharacter.TalkMenuTypes.Clear();
            var creatureInfo = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].CreatureInfo;
            try
            {
                if (((uint)creatureInfo.cNpcFlags & 4u) != 0 || ((uint)creatureInfo.cNpcFlags & 0x4000u) != 0)
                {
                    npcMenu.AddMenu("Let me browse your goods.", 1);
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_VENDOR);
                }
                if (((uint)creatureInfo.cNpcFlags & 8u) != 0)
                {
                    npcMenu.AddMenu("I want to continue my journey.", 2);
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TAXIVENDOR);
                }
                if ((creatureInfo.cNpcFlags & 0x10) == 0)
                {
                    goto IL_0431;
                }
                QuestMenu qMenu2;
                if (creatureInfo.TrainerType == 0)
                {
                    if (creatureInfo.Classe == (uint)objCharacter.Classe)
                    {
                        var gossipMenu = npcMenu;
                        var functions = WorldServiceLocator._Functions;
                        WS_PlayerData.CharacterObject characterObject;
                        var Classe = (int)(characterObject = objCharacter).Classe;
                        var className = functions.GetClassName(ref Classe);
                        characterObject.Classe = (Classes)checked((byte)Classe);
                        gossipMenu.AddMenu("I am interested in " + className + " training.", 3);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TRAINER);
                        if (objCharacter.Level >= 10)
                        {
                            npcMenu.AddMenu("I want to unlearn all my talents.");
                            objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TALENTWIPE);
                        }
                        goto IL_0431;
                    }
                    switch (creatureInfo.Classe)
                    {
                        case 11:
                            textID = 4913;
                            break;

                        case 3:
                            textID = 10090;
                            break;

                        case 8:
                            textID = 328;
                            break;

                        case 2:
                            textID = 1635;
                            break;

                        case 5:
                            textID = 4436;
                            break;

                        case 4:
                            textID = 4797;
                            break;

                        case 7:
                            textID = 5003;
                            break;

                        case 9:
                            textID = 5836;
                            break;

                        case 1:
                            textID = 4985;
                            break;
                    }
                    var obj = objCharacter;
                    var cTextID = textID;
                    GossipMenu Menu = null;
                    qMenu2 = null;
                    obj.SendGossip(cGuid, cTextID, Menu, qMenu2);
                }
                else if (creatureInfo.TrainerType == 1)
                {
                    if (creatureInfo.Race <= 0 || creatureInfo.Race == (uint)objCharacter.Race || objCharacter.GetReputation(creatureInfo.Faction) >= ReputationRank.Exalted)
                    {
                        npcMenu.AddMenu("I am interested in mount training.", 3);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TRAINER);
                        goto IL_0431;
                    }
                    switch (creatureInfo.Race)
                    {
                        case 3:
                            textID = 5865;
                            break;

                        case 7:
                            textID = 4881;
                            break;

                        case 1:
                            textID = 5861;
                            break;

                        case 4:
                            textID = 5862;
                            break;

                        case 2:
                            textID = 5863;
                            break;

                        case 6:
                            textID = 5864;
                            break;

                        case 8:
                            textID = 5816;
                            break;

                        case 5:
                            textID = 624;
                            break;
                    }
                    var obj2 = objCharacter;
                    var cTextID2 = textID;
                    GossipMenu Menu = null;
                    qMenu2 = null;
                    obj2.SendGossip(cGuid, cTextID2, Menu, qMenu2);
                }
                else if (creatureInfo.TrainerType == 2)
                {
                    if (creatureInfo.TrainerSpell <= 0 || objCharacter.HaveSpell(creatureInfo.TrainerSpell))
                    {
                        npcMenu.AddMenu("I am interested in professions training.", 3);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TRAINER);
                        goto IL_0431;
                    }
                    textID = 11031;
                    var obj3 = objCharacter;
                    var cTextID3 = textID;
                    GossipMenu Menu = null;
                    qMenu2 = null;
                    obj3.SendGossip(cGuid, cTextID3, Menu, qMenu2);
                }
                else
                {
                    if (creatureInfo.TrainerType != 3)
                    {
                        goto IL_0431;
                    }
                    if (objCharacter.Classe == Classes.CLASS_HUNTER)
                    {
                        npcMenu.AddMenu("I am interested in pet training.", 3);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TRAINER);
                        goto IL_0431;
                    }
                    textID = 3620;
                    var obj4 = objCharacter;
                    var cTextID4 = textID;
                    GossipMenu Menu = null;
                    qMenu2 = null;
                    obj4.SendGossip(cGuid, cTextID4, Menu, qMenu2);
                }
                goto end_IL_002c;
            IL_0431:
                if (((uint)creatureInfo.cNpcFlags & 0x20u) != 0)
                {
                    textID = 580;
                    npcMenu.AddMenu("Return me to life");
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_SPIRITHEALER);
                }
                if (((uint)creatureInfo.cNpcFlags & 0x80u) != 0)
                {
                    npcMenu.AddMenu("Make this inn your home.", 5);
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_INNKEEPER);
                }
                if (((uint)creatureInfo.cNpcFlags & 0x100u) != 0)
                {
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_BANKER);
                }
                if (((uint)creatureInfo.cNpcFlags & 0x200u) != 0)
                {
                    npcMenu.AddMenu("I am interested in guilds.", 7);
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_ARENACHARTER);
                }
                if (((uint)creatureInfo.cNpcFlags & 0x400u) != 0)
                {
                    npcMenu.AddMenu("I want to purchase a tabard.", 8);
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TABARDVENDOR);
                }
                if (((uint)creatureInfo.cNpcFlags & 0x800u) != 0)
                {
                    npcMenu.AddMenu("My blood hungers for battle.", 9);
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_BATTLEFIELD);
                }
                if (((uint)creatureInfo.cNpcFlags & 0x1000u) != 0)
                {
                    npcMenu.AddMenu("Wanna auction something?", 10);
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_AUCTIONEER);
                }
                if (((uint)creatureInfo.cNpcFlags & 0x2000u) != 0)
                {
                    npcMenu.AddMenu("Let me check my pet.", 1);
                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_STABLEPET);
                }
                if (textID == 0)
                {
                    textID = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].NPCTextID;
                }
                if ((creatureInfo.cNpcFlags & 2) == 2)
                {
                    var qMenu = WorldServiceLocator._WorldServer.ALLQUESTS.GetQuestMenu(ref objCharacter, cGuid);
                    if (qMenu.IDs.Count == 0 && npcMenu.Menus.Count == 0)
                    {
                        return;
                    }
                    if (npcMenu.Menus.Count == 0)
                    {
                        if (qMenu.IDs.Count == 1)
                        {
                            var questID = Conversions.ToInteger(qMenu.IDs[0]);
                            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
                            {
                                WS_QuestInfo tmpQuest = new(questID);
                            }
                            QuestgiverStatusFlag status = (QuestgiverStatusFlag)Conversions.ToInteger(qMenu.Icons[0]);
                            if (status == QuestgiverStatusFlag.DIALOG_STATUS_INCOMPLETE)
                            {
                                var i = 0;
                                do
                                {
                                    if (objCharacter.TalkQuests[i] != null && objCharacter.TalkQuests[i].ID == questID)
                                    {
                                        objCharacter.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                                        WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, cGuid, ref objCharacter.TalkQuests[i]);
                                        break;
                                    }
                                    i = checked(i + 1);
                                }
                                while (i <= 24);
                            }
                            else
                            {
                                objCharacter.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                                WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestDetails(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, cGuid, acceptActive: true);
                            }
                        }
                        else
                        {
                            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMenu(ref objCharacter, cGuid, "I have some tasks for you, $N.", qMenu);
                        }
                    }
                    else
                    {
                        objCharacter.SendGossip(cGuid, textID, npcMenu, qMenu);
                    }
                    return;
                }
                var obj5 = objCharacter;
                var cTextID5 = textID;
                qMenu2 = null;
                obj5.SendGossip(cGuid, cTextID5, npcMenu, qMenu2);
            end_IL_002c:
                ;
            }
            catch (Exception ex2)
            {
                ProjectData.SetProjectError(ex2);
                ProjectData.ClearProjectError();
            }
        }

        public override void OnGossipSelect(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int selected)
        {
            var left = objCharacter.TalkMenuTypes[selected];
            if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_SPIRITHEALER, TextCompare: false))
            {
                if (objCharacter.DEAD)
                {
                    Packets.PacketClass response = new(Opcodes.SMSG_SPIRIT_HEALER_CONFIRM);
                    try
                    {
                        response.AddUInt64(cGUID);
                        objCharacter.client.Send(ref response);
                    }
                    finally
                    {
                        response.Dispose();
                    }
                    objCharacter.SendGossipComplete();
                }
            }
            else if (Conversions.ToBoolean(Conversions.ToBoolean(Operators.CompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_VENDOR, TextCompare: false)) || Conversions.ToBoolean(Operators.CompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_ARMORER, TextCompare: false)) || Conversions.ToBoolean(Operators.CompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_STABLEPET, TextCompare: false))))
            {
                WorldServiceLocator._WS_NPCs.SendListInventory(ref objCharacter, cGUID);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_TRAINER, TextCompare: false))
            {
                WorldServiceLocator._WS_NPCs.SendTrainerList(ref objCharacter, cGUID);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_TAXIVENDOR, TextCompare: false))
            {
                WorldServiceLocator._WS_Handlers_Taxi.SendTaxiMenu(ref objCharacter, cGUID);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_INNKEEPER, TextCompare: false))
            {
                WorldServiceLocator._WS_NPCs.SendBindPointConfirm(ref objCharacter, cGUID);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_BANKER, TextCompare: false))
            {
                WorldServiceLocator._WS_NPCs.SendShowBank(ref objCharacter, cGUID);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_ARENACHARTER, TextCompare: false))
            {
                WorldServiceLocator._WS_Guilds.SendPetitionActivate(ref objCharacter, cGUID);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_TABARDVENDOR, TextCompare: false))
            {
                WorldServiceLocator._WS_Guilds.SendTabardActivate(ref objCharacter, cGUID);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_AUCTIONEER, TextCompare: false))
            {
                WorldServiceLocator._WS_Auction.SendShowAuction(ref objCharacter, cGUID);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_TALENTWIPE, TextCompare: false))
            {
                WorldServiceLocator._WS_NPCs.SendTalentWipeConfirm(ref objCharacter, 0);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_GOSSIP, TextCompare: false))
            {
                objCharacter.SendTalking(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].NPCTextID);
            }
            else if (Operators.ConditionalCompareObjectEqual(left, Gossip_Option.GOSSIP_OPTION_QUESTGIVER, TextCompare: false))
            {
                var qMenu = WorldServiceLocator._WorldServer.ALLQUESTS.GetQuestMenu(ref objCharacter, cGUID);
                WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMenu(ref objCharacter, cGUID, "I have some tasks for you, $N.", qMenu);
            }
        }
    }

    private const int DbcBankBagSlotsMax = 12;

    private readonly int[] DbcBankBagSlotPrices;

    public WS_NPCs()
    {
        DbcBankBagSlotPrices = new int[13];
    }

    public void On_CMSG_TRAINER_LIST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TRAINER_LIST [GUID={2}]", client.IP, client.Port, guid);
            SendTrainerList(ref client.Character, guid);
        }
    }

    public void On_CMSG_TRAINER_BUY_SPELL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 17)
            {
                return;
            }
            packet.GetInt16();
            var cGuid = packet.GetUInt64();
            var spellID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TRAINER_BUY_SPELL [GUID={2} Spell={3}]", client.IP, client.Port, cGuid, spellID);
            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGuid) || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].CreatureInfo.cNpcFlags & 0x10) == 0)
            {
                return;
            }
            DataTable mySqlQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM npc_trainer WHERE entry = {WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID} AND spell = {spellID};", ref mySqlQuery);
            if (mySqlQuery.Rows.Count == 0)
            {
                return;
            }
            var spellInfo = WorldServiceLocator._WS_Spells.SPELLs[spellID];
            if (spellInfo.SpellEffects[0] != null && spellInfo.SpellEffects[0].TriggerSpell > 0)
            {
                spellInfo = WorldServiceLocator._WS_Spells.SPELLs[spellInfo.SpellEffects[0].TriggerSpell];
            }
            var reqLevel = mySqlQuery.Rows[0].As<byte>("reqlevel");
            if (reqLevel == 0)
            {
                reqLevel = (byte)spellInfo.spellLevel;
            }
            var spellCost = mySqlQuery.Rows[0].As<uint>("spellcost");
            var reqSpell = 0;
            if (WorldServiceLocator._WS_Spells.SpellChains.ContainsKey(spellInfo.ID))
            {
                reqSpell = WorldServiceLocator._WS_Spells.SpellChains[spellInfo.ID];
            }
            if (!client.Character.HaveSpell(spellInfo.ID) && client.Character.Copper >= spellCost && client.Character.Level >= (uint)reqLevel && (reqSpell <= 0 || client.Character.HaveSpell(reqSpell)) && (mySqlQuery.Rows[0].As<int>("reqskill") <= 0 || client.Character.HaveSkill(mySqlQuery.Rows[0].As<int>("reqskill"), mySqlQuery.Rows[0].As<int>("reqskillvalue"))))
            {
                try
                {
                    client.Character.Copper -= spellCost;
                    client.Character.SetUpdateFlag(1176, client.Character.Copper);
                    client.Character.SendCharacterUpdate(toNear: false);
                    WS_Spells.SpellTargets spellTargets = new();
                    var spellTargets2 = spellTargets;
                    ref var character = ref client.Character;
                    WS_Base.BaseUnit objCharacter = character;
                    spellTargets2.SetTarget_UNIT(ref objCharacter);
                    character = (WS_PlayerData.CharacterObject)objCharacter;
                    WS_Base.BaseUnit tmpCaster = (WS_Base.BaseUnit)((spellInfo.SpellVisual != 222) ? WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid] : ((object)client.Character));
                    WS_Base.BaseObject Caster = tmpCaster;
                    WS_Spells.CastSpellParameters castSpellParameters = new(ref spellTargets, ref Caster, spellID, Instant: true);
                    tmpCaster = (WS_Base.BaseUnit)Caster;
                    var castParams = castSpellParameters;
                    ThreadPool.QueueUserWorkItem(castParams.Cast);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].MoveToInstant(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionX, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionY, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionZ, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].SpawnO);
                }
                catch (Exception ex)
                {
                    ProjectData.SetProjectError(ex);
                    var e = ex;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Training Spell Error: Unable to cast spell. [{0}:{1}]", Environment.NewLine, e.ToString());
                    ProjectData.ClearProjectError();
                }
                Packets.PacketClass response = new(Opcodes.SMSG_TRAINER_BUY_SUCCEEDED);
                try
                {
                    response.AddUInt64(cGuid);
                    response.AddInt32(spellID);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }
            }
        }
    }

    private void SendTrainerList(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
    {
        DataTable spellSqlQuery = new();
        var creatureInfo = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].CreatureInfo;
        List<DataRow> spellsList = new();
        if ((creatureInfo.Classe == 0 || creatureInfo.Classe == (uint)objCharacter.Classe) && (creatureInfo.Race == 0 || creatureInfo.Race == (uint)objCharacter.Race || objCharacter.GetReputation(creatureInfo.Faction) == ReputationRank.Exalted))
        {
            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM npc_trainer WHERE entry = {WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID};", ref spellSqlQuery);
            IEnumerator enumerator = default;
            try
            {
                enumerator = spellSqlQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    spellsList.Add(row);
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
        }
        Packets.PacketClass packet = new(Opcodes.SMSG_TRAINER_LIST);
        packet.AddUInt64(cGuid);
        packet.AddInt32(creatureInfo.TrainerType);
        packet.AddInt32(spellsList.Count);
        var discountMod = objCharacter.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].Faction);
        foreach (var row in spellsList)
        {
            var spellID = row.As<int>("spell");
            if (!WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(spellID))
            {
                continue;
            }
            var spellInfo = WorldServiceLocator._WS_Spells.SPELLs[spellID];
            if (spellInfo.SpellEffects[0] != null && spellInfo.SpellEffects[0].TriggerSpell > 0)
            {
                spellInfo = WorldServiceLocator._WS_Spells.SPELLs[spellInfo.SpellEffects[0].TriggerSpell];
            }
            var reqSpell = 0;
            if (WorldServiceLocator._WS_Spells.SpellChains.ContainsKey(spellInfo.ID))
            {
                reqSpell = WorldServiceLocator._WS_Spells.SpellChains[spellInfo.ID];
            }
            var spellLevel = row.As<byte>("reqlevel");
            if (spellLevel == 0)
            {
                spellLevel = checked((byte)spellInfo.spellLevel);
            }
            byte canLearnFlag = 0;
            if (objCharacter.HaveSpell(spellInfo.ID))
            {
                canLearnFlag = 2;
            }
            else if (objCharacter.Level >= (uint)spellLevel)
            {
                if (reqSpell > 0 && !objCharacter.HaveSpell(reqSpell))
                {
                    canLearnFlag = 1;
                }
                if (canLearnFlag == 0 && row.As<int>("reqskill") != 0 && row.As<int>("reqskillvalue") != 0 && !objCharacter.HaveSkill(row.As<int>("reqskill"), row.As<int>("reqskillvalue")))
                {
                    canLearnFlag = 1;
                }
            }
            else
            {
                canLearnFlag = 1;
            }
            var isProf = 0;
            if (spellInfo.SpellEffects[1] != null && spellInfo.SpellEffects[1].ID == SpellEffects_Names.SPELL_EFFECT_SKILL_STEP)
            {
                isProf = 1;
            }
            packet.AddInt32(spellID);
            packet.AddInt8(canLearnFlag);
            packet.AddInt32(checked((int)Math.Round(row.As<int>("spellcost") * discountMod)));
            packet.AddInt32(0);
            packet.AddInt32(isProf);
            packet.AddInt8(spellLevel);
            packet.AddInt32(row.As<int>("reqskill"));
            packet.AddInt32(row.As<int>("reqskillvalue"));
            packet.AddInt32(reqSpell);
            packet.AddInt32(0);
            packet.AddInt32(0);
        }
        packet.AddString("Hello! Ready for some training?");
        objCharacter.client.Send(ref packet);
        packet.Dispose();
    }

    public void On_CMSG_LIST_INVENTORY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LIST_INVENTORY [GUID={2:X}]", client.IP, client.Port, guid);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) && ((uint)WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].CreatureInfo.cNpcFlags & 4u) != 0 && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Evade)
            {
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].StopMoving();
                SendListInventory(ref client.Character, guid);
            }
        }
    }

    public void On_CMSG_SELL_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 22)
            {
                return;
            }
            packet.GetInt16();
            var vendorGuid = packet.GetUInt64();
            var itemGuid = packet.GetUInt64();
            var count = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SELL_ITEM [vendorGuid={2:X} itemGuid={3:X} Count={4}]", client.IP, client.Port, vendorGuid, itemGuid, count);
            try
            {
                if (decimal.Compare(new decimal(itemGuid), 0m) == 0 || !WorldServiceLocator._WorldServer.WORLD_ITEMs.ContainsKey(itemGuid))
                {
                    Packets.PacketClass okPckt7 = new(Opcodes.SMSG_SELL_ITEM);
                    try
                    {
                        okPckt7.AddUInt64(vendorGuid);
                        okPckt7.AddUInt64(itemGuid);
                        okPckt7.AddInt8(1);
                        client.Send(ref okPckt7);
                    }
                    finally
                    {
                        okPckt7.Dispose();
                    }
                    return;
                }
                if (WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].OwnerGUID != client.Character.GUID)
                {
                    Packets.PacketClass okPckt6 = new(Opcodes.SMSG_SELL_ITEM);
                    try
                    {
                        okPckt6.AddUInt64(vendorGuid);
                        okPckt6.AddUInt64(itemGuid);
                        okPckt6.AddInt8(1);
                        client.Send(ref okPckt6);
                    }
                    finally
                    {
                        okPckt6.Dispose();
                    }
                    return;
                }
                if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(vendorGuid))
                {
                    Packets.PacketClass okPckt5 = new(Opcodes.SMSG_SELL_ITEM);
                    try
                    {
                        okPckt5.AddUInt64(vendorGuid);
                        okPckt5.AddUInt64(itemGuid);
                        okPckt5.AddInt8(3);
                        client.Send(ref okPckt5);
                    }
                    finally
                    {
                        okPckt5.Dispose();
                    }
                    return;
                }
                if ((WorldServiceLocator._WorldServer.ITEMDatabase[WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemEntry].SellPrice == 0) || (WorldServiceLocator._WorldServer.ITEMDatabase[WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemEntry].ObjectClass == ITEM_CLASS.ITEM_CLASS_QUEST))
                {
                    Packets.PacketClass okPckt4 = new(Opcodes.SMSG_SELL_ITEM);
                    try
                    {
                        okPckt4.AddUInt64(vendorGuid);
                        okPckt4.AddUInt64(itemGuid);
                        okPckt4.AddInt8(2);
                        client.Send(ref okPckt4);
                    }
                    finally
                    {
                        okPckt4.Dispose();
                    }
                    return;
                }
                byte i = 69;
                do
                {
                    if (client.Character.Items.ContainsKey(i) && client.Character.Items[i].GUID == itemGuid)
                    {
                        Packets.PacketClass okPckt3 = new(Opcodes.SMSG_SELL_ITEM);
                        okPckt3.AddUInt64(vendorGuid);
                        okPckt3.AddUInt64(itemGuid);
                        okPckt3.AddInt8(1);
                        client.Send(ref okPckt3);
                        okPckt3.Dispose();
                        return;
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                while (i <= 80u);
                if (count < 1)
                {
                    count = (byte)WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].StackCount;
                }
                if (WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].StackCount > count)
                {
                    WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].StackCount -= count;
                    var tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(itemGuid);
                    ref var itemGuidCounter = ref WorldServiceLocator._WorldServer.itemGuidCounter;
                    itemGuidCounter = Convert.ToUInt64(decimal.Add(new decimal(itemGuidCounter), 1m));
                    tmpItem.GUID = WorldServiceLocator._WorldServer.itemGuidCounter;
                    tmpItem.StackCount = count;
                    client.Character.ItemADD_BuyBack(ref tmpItem);
                    ref var copper = ref client.Character.Copper;
                    copper = (uint)(copper + checked(WorldServiceLocator._WorldServer.ITEMDatabase[WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemEntry].SellPrice * count));
                    client.Character.SetUpdateFlag(1176, client.Character.Copper);
                    client.Character.SendItemUpdate(WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid]);
                    WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].Save(saveAll: false);
                    return;
                }
                foreach (var item2 in client.Character.Items)
                {
                    if (item2.Value.GUID == itemGuid)
                    {
                        ref var copper2 = ref client.Character.Copper;
                        copper2 = (uint)(copper2 + checked(WorldServiceLocator._WorldServer.ITEMDatabase[item2.Value.ItemEntry].SellPrice * item2.Value.StackCount));
                        client.Character.SetUpdateFlag(1176, client.Character.Copper);
                        ItemObject Item;
                        if (item2.Key < 23u)
                        {
                            var character = client.Character;
                            Item = item2.Value;
                            character.UpdateRemoveItemStats(ref Item, item2.Key);
                        }
                        client.Character.ItemREMOVE(item2.Value.GUID, Destroy: false, Update: true);
                        var character2 = client.Character;
                        Item = item2.Value;
                        character2.ItemADD_BuyBack(ref Item);
                        Packets.PacketClass okPckt2 = new(Opcodes.SMSG_SELL_ITEM);
                        okPckt2.AddUInt64(vendorGuid);
                        okPckt2.AddUInt64(itemGuid);
                        okPckt2.AddInt8(0);
                        client.Send(ref okPckt2);
                        okPckt2.Dispose();
                        return;
                    }
                }
                byte bag = 19;
                do
                {
                    if (client.Character.Items.ContainsKey(bag))
                    {
                        foreach (var item in client.Character.Items[bag].Items)
                        {
                            if (item.Value.GUID == itemGuid)
                            {
                                ref var copper3 = ref client.Character.Copper;
                                copper3 = (uint)(copper3 + checked(WorldServiceLocator._WorldServer.ITEMDatabase[item.Value.ItemEntry].SellPrice * item.Value.StackCount));
                                client.Character.SetUpdateFlag(1176, client.Character.Copper);
                                client.Character.ItemREMOVE(item.Value.GUID, Destroy: false, Update: true);
                                var character3 = client.Character;
                                var Item = item.Value;
                                character3.ItemADD_BuyBack(ref Item);
                                Packets.PacketClass okPckt = new(Opcodes.SMSG_SELL_ITEM);
                                okPckt.AddUInt64(vendorGuid);
                                okPckt.AddUInt64(itemGuid);
                                okPckt.AddInt8(0);
                                client.Send(ref okPckt);
                                okPckt.Dispose();
                                return;
                            }
                        }
                    }
                    bag = (byte)unchecked((uint)(bag + 1));
                }
                while (bag <= 22u);
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                var e = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error selling item: {0}{1}", Environment.NewLine, e.ToString());
                ProjectData.ClearProjectError();
            }
        }
    }

    public void On_CMSG_BUY_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 19)
            {
                return;
            }
            packet.GetInt16();
            var vendorGuid = packet.GetUInt64();
            var itemID = packet.GetInt32();
            var count = packet.GetInt8();
            var slot = packet.GetInt8();
            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(vendorGuid) || ((WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & 0x4000) == 0 && (WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & 4) == 0) || !WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID))
            {
                return;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUY_ITEM [vendorGuid={2:X} ItemID={3} Count={4} Slot={5}]", client.IP, client.Port, vendorGuid, itemID, count, slot);
            if (count > WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable)
            {
                count = (byte)WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable;
            }
            if (count == 0)
            {
                count = 1;
            }
            if (WorldServiceLocator._WorldServer.ITEMDatabase[itemID].ObjectClass == ITEM_CLASS.ITEM_CLASS_QUEST)
            {
                Packets.PacketClass errorPckt2 = new(Opcodes.SMSG_BUY_FAILED);
                try
                {
                    errorPckt2.AddUInt64(vendorGuid);
                    errorPckt2.AddInt32(itemID);
                    errorPckt2.AddInt8(4);
                    client.Send(ref errorPckt2);
                }
                finally
                {
                    errorPckt2.Dispose();
                }
                return;
            }
            if (count * WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyCount > WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable)
            {
                count = (byte)Math.Round(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable / (double)WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyCount);
            }
            var discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].Faction);
            var itemPrice = (int)Math.Round(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyPrice * discountMod);
            if (client.Character.Copper < itemPrice * count)
            {
                Packets.PacketClass errorPckt = new(Opcodes.SMSG_BUY_FAILED);
                try
                {
                    errorPckt.AddUInt64(vendorGuid);
                    errorPckt.AddInt32(itemID);
                    errorPckt.AddInt8(2);
                    client.Send(ref errorPckt);
                }
                finally
                {
                    errorPckt.Dispose();
                }
                return;
            }
            ref var copper = ref client.Character.Copper;
            copper = (uint)(copper - checked(itemPrice * count));
            client.Character.SetUpdateFlag(1176, client.Character.Copper);
            client.Character.SendCharacterUpdate(toNear: false);
            ItemObject itemObject = new(itemID, client.Character.GUID)
            {
                StackCount = count * WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyCount
            };
            var tmpItem = itemObject;
            if (!client.Character.ItemADD(ref tmpItem))
            {
                tmpItem.Delete();
                ref var copper2 = ref client.Character.Copper;
                copper2 = (uint)(copper2 + itemPrice);
                client.Character.SetUpdateFlag(1176, client.Character.Copper);
            }
            else
            {
                Packets.PacketClass okPckt = new(Opcodes.SMSG_BUY_ITEM);
                okPckt.AddUInt64(vendorGuid);
                okPckt.AddInt32(itemID);
                okPckt.AddInt32(count);
                client.Send(ref okPckt);
                okPckt.Dispose();
            }
        }
    }

    public void On_CMSG_BUY_ITEM_IN_SLOT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 27)
            {
                return;
            }
            packet.GetInt16();
            var vendorGuid = packet.GetUInt64();
            var itemID = packet.GetInt32();
            var clientGuid = packet.GetUInt64();
            var slot = packet.GetInt8();
            var count = packet.GetInt8();
            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(vendorGuid) || ((WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & 0x4000) == 0 && (WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & 4) == 0) || !WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID))
            {
                return;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUY_ITEM_IN_SLOT [vendorGuid={2:X} ItemID={3} Count={4} Slot={5}]", client.IP, client.Port, vendorGuid, itemID, count, slot);
            if (count > WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable)
            {
                count = (byte)WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable;
            }
            if (WorldServiceLocator._WorldServer.ITEMDatabase[itemID].ObjectClass == ITEM_CLASS.ITEM_CLASS_QUEST)
            {
                Packets.PacketClass errorPckt5 = new(Opcodes.SMSG_BUY_FAILED);
                errorPckt5.AddUInt64(vendorGuid);
                errorPckt5.AddInt32(itemID);
                errorPckt5.AddInt8(4);
                client.Send(ref errorPckt5);
                errorPckt5.Dispose();
                return;
            }
            var discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].Faction);
            var itemPrice = (int)Math.Round(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyPrice * discountMod);
            if (client.Character.Copper < itemPrice * count)
            {
                Packets.PacketClass errorPckt4 = new(Opcodes.SMSG_BUY_FAILED);
                errorPckt4.AddUInt64(vendorGuid);
                errorPckt4.AddInt32(itemID);
                errorPckt4.AddInt8(2);
                client.Send(ref errorPckt4);
                errorPckt4.Dispose();
                return;
            }
            byte bag = 0;
            if (clientGuid == client.Character.GUID)
            {
                bag = 0;
                if (client.Character.Items.ContainsKey(slot))
                {
                    Packets.PacketClass errorPckt3 = new(Opcodes.SMSG_BUY_FAILED);
                    errorPckt3.AddUInt64(vendorGuid);
                    errorPckt3.AddInt32(itemID);
                    errorPckt3.AddInt8(8);
                    client.Send(ref errorPckt3);
                    errorPckt3.Dispose();
                    return;
                }
            }
            else
            {
                byte i = 19;
                do
                {
                    if (client.Character.Items[i].GUID == clientGuid)
                    {
                        bag = i;
                        break;
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                while (i <= 22u);
                if (bag == 0)
                {
                    Packets.PacketClass okPckt2 = new(Opcodes.SMSG_BUY_FAILED);
                    okPckt2.AddUInt64(vendorGuid);
                    okPckt2.AddInt32(itemID);
                    okPckt2.AddInt8(0);
                    client.Send(ref okPckt2);
                    okPckt2.Dispose();
                    return;
                }
                if (client.Character.Items[bag].Items.ContainsKey(slot))
                {
                    Packets.PacketClass errorPckt2 = new(Opcodes.SMSG_BUY_FAILED);
                    errorPckt2.AddUInt64(vendorGuid);
                    errorPckt2.AddInt32(itemID);
                    errorPckt2.AddInt8(8);
                    client.Send(ref errorPckt2);
                    errorPckt2.Dispose();
                    return;
                }
            }
            ItemObject itemObject = new(itemID, client.Character.GUID)
            {
                StackCount = count
            };
            var tmpItem = itemObject;
            var errCode = (byte)client.Character.ItemCANEQUIP(tmpItem, bag, slot);
            if (errCode != 0)
            {
                if (errCode != 1)
                {
                    Packets.PacketClass errorPckt = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    errorPckt.AddInt8(errCode);
                    errorPckt.AddUInt64(0uL);
                    errorPckt.AddUInt64(0uL);
                    errorPckt.AddInt8(0);
                    client.Send(ref errorPckt);
                    errorPckt.Dispose();
                }
                tmpItem.Delete();
                return;
            }
            ref var copper = ref client.Character.Copper;
            copper = (uint)(copper - checked(itemPrice * count));
            client.Character.SetUpdateFlag(1176, client.Character.Copper);
            if (!client.Character.ItemSETSLOT(ref tmpItem, slot, bag))
            {
                tmpItem.Delete();
                ref var copper2 = ref client.Character.Copper;
                copper2 = (uint)(copper2 + itemPrice);
                client.Character.SetUpdateFlag(1176, client.Character.Copper);
            }
            else
            {
                Packets.PacketClass okPckt = new(Opcodes.SMSG_BUY_ITEM);
                okPckt.AddUInt64(vendorGuid);
                okPckt.AddInt32(itemID);
                okPckt.AddInt32(count);
                client.Send(ref okPckt);
                okPckt.Dispose();
            }
            client.Character.SendCharacterUpdate(toNear: false);
        }
    }

    public void On_CMSG_BUYBACK_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 17)
            {
                return;
            }
            packet.GetInt16();
            var vendorGuid = packet.GetUInt64();
            var slot = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUYBACK_ITEM [vendorGuid={2:X} Slot={3}]", client.IP, client.Port, vendorGuid, slot);
            if (slot < 69 || slot >= 81 || !client.Character.Items.ContainsKey((byte)slot))
            {
                Packets.PacketClass errorPckt2 = new(Opcodes.SMSG_BUY_FAILED);
                try
                {
                    errorPckt2.AddUInt64(vendorGuid);
                    errorPckt2.AddInt32(0);
                    errorPckt2.AddInt8(0);
                    client.Send(ref errorPckt2);
                }
                finally
                {
                    errorPckt2.Dispose();
                }
                return;
            }
            var tmpItem = client.Character.Items[(byte)slot];
            if (client.Character.Copper < tmpItem.ItemInfo.SellPrice * tmpItem.StackCount)
            {
                Packets.PacketClass errorPckt = new(Opcodes.SMSG_BUY_FAILED);
                try
                {
                    errorPckt.AddUInt64(vendorGuid);
                    errorPckt.AddInt32(tmpItem.ItemEntry);
                    errorPckt.AddInt8(2);
                    client.Send(ref errorPckt);
                }
                finally
                {
                    errorPckt.Dispose();
                }
                return;
            }
            client.Character.ItemREMOVE(tmpItem.GUID, Destroy: false, Update: true);
            if (client.Character.ItemADD_AutoSlot(ref tmpItem))
            {
                var eSlot = (byte)(slot - 69);
                ref var copper = ref client.Character.Copper;
                copper = (uint)(copper - checked(tmpItem.ItemInfo.SellPrice * tmpItem.StackCount));
                client.Character.BuyBackTimeStamp[eSlot] = 0;
                client.Character.SetUpdateFlag(1238 + eSlot, 0);
                client.Character.SetUpdateFlag(1226 + eSlot, 0);
                client.Character.SetUpdateFlag(1176, client.Character.Copper);
                client.Character.SendCharacterUpdate();
            }
            else
            {
                WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_INVENTORY_FULL, 0uL, 0uL);
                client.Character.ItemSETSLOT(ref tmpItem, 0, (byte)slot);
            }
        }
    }

    public void On_CMSG_REPAIR_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 21)
            {
                return;
            }
            packet.GetInt16();
            var vendorGuid = packet.GetUInt64();
            var itemGuid = packet.GetUInt64();
            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(vendorGuid) || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & 0x4000) == 0)
            {
                return;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_REPAIR_ITEM [vendorGuid={2:X} itemGuid={3:X}]", client.IP, client.Port, vendorGuid, itemGuid);
            var discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].Faction);
            if (decimal.Compare(new decimal(itemGuid), 0m) != 0)
            {
                var price = (uint)Math.Round(WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].GetDurabulityCost * discountMod);
                if (client.Character.Copper >= price)
                {
                    client.Character.Copper -= price;
                    client.Character.SetUpdateFlag(1176, client.Character.Copper);
                    client.Character.SendCharacterUpdate(toNear: false);
                    WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ModifyToDurability(100f, ref client);
                }
                return;
            }
            byte i = 0;
            do
            {
                if (client.Character.Items.ContainsKey(i))
                {
                    var price = (uint)Math.Round(client.Character.Items[i].GetDurabulityCost * discountMod);
                    if (client.Character.Copper >= price)
                    {
                        client.Character.Copper -= price;
                        client.Character.SetUpdateFlag(1176, client.Character.Copper);
                        client.Character.SendCharacterUpdate(toNear: false);
                        client.Character.Items[i].ModifyToDurability(100f, ref client);
                    }
                }
                i = (byte)unchecked((uint)(i + 1));
            }
            while (i <= 18u);
        }
    }

    private void SendListInventory(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
    {
        try
        {
            Packets.PacketClass packet = new(Opcodes.SMSG_LIST_INVENTORY);
            packet.AddUInt64(guid);
            DataTable mySqlQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM npc_vendor WHERE entry = {WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].ID};", ref mySqlQuery);
            var dataPos = packet.Data.Length;
            packet.AddInt8(0);
            byte i = 0;
            IEnumerator enumerator = default;
            try
            {
                enumerator = mySqlQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    var itemID = row.As<int>("item");
                    if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID))
                    {
                        WS_Items.ItemInfo tmpItem = new(itemID);
                    }
                    if (WorldServiceLocator._WorldServer.ITEMDatabase[itemID].AvailableClasses == 0L || (WorldServiceLocator._WorldServer.ITEMDatabase[itemID].AvailableClasses & objCharacter.ClassMask) != 0)
                    {
                        checked
                        {
                            i = (byte)(i + 1);
                            packet.AddInt32(-1);
                            packet.AddInt32(itemID);
                            packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Model);
                            if (Operators.ConditionalCompareObjectLessEqual(row["maxcount"], 0, TextCompare: false))
                            {
                                packet.AddInt32(-1);
                            }
                            else
                            {
                                packet.AddInt32(row.As<int>("maxcount"));
                            }
                            var discountMod = objCharacter.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Faction);
                            packet.AddInt32((int)Math.Round(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyPrice * discountMod));
                            packet.AddInt32(-1);
                            packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyCount);
                        }
                    }
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            if (i > 0)
            {
                packet.AddInt8(i, dataPos);
            }
            objCharacter.client.Send(ref packet);
            packet.Dispose();
        }
        catch (Exception ex)
        {
            ProjectData.SetProjectError(ex);
            var e = ex;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error while listing inventory.{0}", Environment.NewLine + e);
            ProjectData.ClearProjectError();
        }
    }

    public void On_CMSG_AUTOBANK_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 7)
            {
                return;
            }
            packet.GetInt16();
            var srcBag = packet.GetInt8();
            var srcSlot = packet.GetInt8();
            if (srcBag == byte.MaxValue)
            {
                srcBag = 0;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOBANK_ITEM [srcSlot={2}:{3}]", client.IP, client.Port, srcBag, srcSlot);
            byte dstSlot2 = 39;
            do
            {
                if (!client.Character.Items.ContainsKey(dstSlot2))
                {
                    client.Character.ItemSWAP(srcBag, srcSlot, 0, dstSlot2);
                    return;
                }
                dstSlot2 = (byte)unchecked((uint)(dstSlot2 + 1));
            }
            while (dstSlot2 <= 63u);
            byte dstBag = 63;
            do
            {
                if (client.Character.Items.ContainsKey(dstBag) && client.Character.Items[dstBag].ItemInfo.IsContainer)
                {
                    var b = (byte)(client.Character.Items[dstBag].ItemInfo.ContainerSlots - 1);
                    byte dstSlot = 0;
                    while (dstSlot <= (uint)b)
                    {
                        if (!client.Character.Items[dstBag].Items.ContainsKey(dstSlot))
                        {
                            client.Character.ItemSWAP(srcBag, srcSlot, dstBag, dstSlot);
                            return;
                        }
                        dstSlot = (byte)unchecked((uint)(dstSlot + 1));
                    }
                }
                dstBag = (byte)unchecked((uint)(dstBag + 1));
            }
            while (dstBag <= 68u);
        }
    }

    public void On_CMSG_AUTOSTORE_BANK_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 7)
            {
                return;
            }
            packet.GetInt16();
            var srcBag = packet.GetInt8();
            var srcSlot = packet.GetInt8();
            if (srcBag == byte.MaxValue)
            {
                srcBag = 0;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOSTORE_BANK_ITEM [srcSlot={2}:{3}]", client.IP, client.Port, srcBag, srcSlot);
            byte dstSlot2 = 23;
            do
            {
                if (!client.Character.Items.ContainsKey(dstSlot2))
                {
                    client.Character.ItemSWAP(srcBag, srcSlot, 0, dstSlot2);
                    return;
                }
                dstSlot2 = (byte)unchecked((uint)(dstSlot2 + 1));
            }
            while (dstSlot2 <= 39u);
            byte bag = 19;
            do
            {
                if (client.Character.Items.ContainsKey(bag) && client.Character.Items[bag].ItemInfo.IsContainer)
                {
                    var b = (byte)(client.Character.Items[bag].ItemInfo.ContainerSlots - 1);
                    byte dstSlot = 0;
                    while (dstSlot <= (uint)b)
                    {
                        if (!client.Character.Items[bag].Items.ContainsKey(dstSlot))
                        {
                            client.Character.ItemSWAP(srcBag, srcSlot, bag, dstSlot);
                            return;
                        }
                        dstSlot = (byte)unchecked((uint)(dstSlot + 1));
                    }
                }
                bag = (byte)unchecked((uint)(bag + 1));
            }
            while (bag <= 22u);
        }
    }

    public void On_CMSG_BUY_BANK_SLOT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUY_BANK_SLOT", client.IP, client.Port);
        checked
        {
            if (client.Character.Items_AvailableBankSlots < 12 && client.Character.Copper >= DbcBankBagSlotPrices[client.Character.Items_AvailableBankSlots])
            {
                ref var copper = ref client.Character.Copper;
                copper = (uint)(copper - DbcBankBagSlotPrices[client.Character.Items_AvailableBankSlots]);
                client.Character.Items_AvailableBankSlots++;
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters SET char_bankSlots = {client.Character.Items_AvailableBankSlots}, char_copper = {client.Character.Copper};");
                client.Character.SetUpdateFlag(1176, client.Character.Copper);
                client.Character.SetUpdateFlag(194, client.Character.cPlayerBytes2);
                client.Character.SendCharacterUpdate(toNear: false);
                return;
            }
            Packets.PacketClass errorPckt = new(Opcodes.SMSG_BUY_FAILED);
            try
            {
                errorPckt.AddUInt64(0uL);
                errorPckt.AddInt32(0);
                errorPckt.AddInt8(2);
                client.Send(ref errorPckt);
            }
            finally
            {
                errorPckt.Dispose();
            }
        }
    }

    public void On_CMSG_BANKER_ACTIVATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BANKER_ACTIVATE [GUID={2:X}]", client.IP, client.Port, guid);
            SendShowBank(ref client.Character, guid);
        }
    }

    private void SendShowBank(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_SHOW_BANK);
        try
        {
            packet.AddUInt64(guid);
            objCharacter.client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
    }

    private void SendBindPointConfirm(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
    {
        objCharacter.SendGossipComplete();
        objCharacter.ZoneCheck();
        Packets.PacketClass packet = new(Opcodes.SMSG_BINDER_CONFIRM);
        try
        {
            packet.AddUInt64(guid);
            packet.AddInt32(objCharacter.ZoneID);
            objCharacter.client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
    }

    public void On_CMSG_BINDER_ACTIVATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BINDER_ACTIVATE [binderGUID={2:X}]", client.IP, client.Port, guid);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid))
            {
                client.Character.SendGossipComplete();
                WS_Spells.SpellTargets spellTargets = new();
                var spellTargets2 = spellTargets;
                ref var character = ref client.Character;
                WS_Base.BaseUnit objCharacter = character;
                spellTargets2.SetTarget_UNIT(ref objCharacter);
                character = (WS_PlayerData.CharacterObject)objCharacter;
                Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                ulong key;
                WS_Base.BaseObject Caster = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = guid];
                WS_Spells.CastSpellParameters castSpellParameters = new(ref spellTargets, ref Caster, 3286, Instant: true);
                wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)Caster;
                var castParams = castSpellParameters;
                ThreadPool.QueueUserWorkItem(castParams.Cast);
            }
        }
    }

    private void SendTalentWipeConfirm(ref WS_PlayerData.CharacterObject objCharacter, int cost)
    {
        Packets.PacketClass packet = new(Opcodes.MSG_TALENT_WIPE_CONFIRM);
        try
        {
            packet.AddUInt64(objCharacter.GUID);
            packet.AddInt32(cost);
            objCharacter.client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
    }

    public void On_MSG_TALENT_WIPE_CONFIRM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            try
            {
                packet.GetInt16();
                var guid = packet.GetPackGuid();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_TALENT_WIPE_CONFIRM [GUID={2:X}]", client.IP, client.Port, guid);
                if (client.Character.Level < 10)
                {
                    return;
                }
                foreach (var talentInfo in WorldServiceLocator._WS_DBCDatabase.Talents)
                {
                    var i = 0;
                    do
                    {
                        if (talentInfo.Value.RankID[i] != 0 && client.Character.HaveSpell(talentInfo.Value.RankID[i]))
                        {
                            client.Character.UnLearnSpell(talentInfo.Value.RankID[i]);
                        }
                        i++;
                    }
                    while (i <= 4);
                }
                client.Character.TalentPoints = (byte)(client.Character.Level - 9);
                client.Character.SetUpdateFlag(1102, client.Character.TalentPoints);
                client.Character.SendCharacterUpdate();
                Packets.PacketClass SMSG_SPELL_START = new(Opcodes.SMSG_SPELL_START);
                try
                {
                    SMSG_SPELL_START.AddPackGUID(client.Character.GUID);
                    SMSG_SPELL_START.AddPackGUID(guid);
                    SMSG_SPELL_START.AddInt16(14867);
                    SMSG_SPELL_START.AddInt16(0);
                    SMSG_SPELL_START.AddInt16(15);
                    SMSG_SPELL_START.AddInt32(0);
                    SMSG_SPELL_START.AddInt16(0);
                    client.Send(ref SMSG_SPELL_START);
                }
                finally
                {
                    SMSG_SPELL_START.Dispose();
                }
                Packets.PacketClass SMSG_SPELL_GO = new(Opcodes.SMSG_SPELL_GO);
                try
                {
                    SMSG_SPELL_GO.AddPackGUID(client.Character.GUID);
                    SMSG_SPELL_GO.AddPackGUID(guid);
                    SMSG_SPELL_GO.AddInt16(14867);
                    SMSG_SPELL_GO.AddInt16(0);
                    SMSG_SPELL_GO.AddInt8(13);
                    SMSG_SPELL_GO.AddInt8(1);
                    SMSG_SPELL_GO.AddInt8(1);
                    SMSG_SPELL_GO.AddUInt64(client.Character.GUID);
                    SMSG_SPELL_GO.AddInt32(0);
                    SMSG_SPELL_GO.AddInt16(512);
                    SMSG_SPELL_GO.AddInt16(0);
                    client.Send(ref SMSG_SPELL_GO);
                }
                finally
                {
                    SMSG_SPELL_GO.Dispose();
                }
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                var e = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error unlearning talents: {0}{1}", Environment.NewLine, e.ToString());
                ProjectData.ClearProjectError();
            }
        }
    }
}
