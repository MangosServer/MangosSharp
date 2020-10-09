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

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Gossip;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Quest;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Quests;
using Mangos.World.Server;
using Mangos.World.Spells;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
    public class WS_NPCs
    {
        private const int DbcBankBagSlotsMax = 12;
        private readonly int[] DbcBankBagSlotPrices = new int[13];

        /* TODO ERROR: Skipped RegionDirectiveTrivia */        /// <summary>
        /// Called when [CMSG_TRAINER_LIST] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_TRAINER_LIST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong guid;
            guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TRAINER_LIST [GUID={2}]", client.IP, client.Port, guid);
            SendTrainerList(ref client.Character, guid);
        }

        /// <summary>
        /// Called when [CMSG_TRAINER_BUY_SPELL] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_TRAINER_BUY_SPELL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong cGuid;
            cGuid = packet.GetUInt64();
            int spellID;
            spellID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TRAINER_BUY_SPELL [GUID={2} Spell={3}]", client.IP, client.Port, cGuid, spellID);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGuid) == false || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_TRAINER) == 0)
                return;
            var mySqlQuery = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM npc_trainer WHERE entry = {0} AND spell = {1};", (object)WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID, (object)spellID), mySqlQuery);
            if (mySqlQuery.Rows.Count == 0)
                return;
            WS_Spells.SpellInfo spellInfo;
            spellInfo = WorldServiceLocator._WS_Spells.SPELLs[spellID];
            if (spellInfo.SpellEffects[0] is object && spellInfo.SpellEffects[0].TriggerSpell > 0)
                spellInfo = WorldServiceLocator._WS_Spells.SPELLs[spellInfo.SpellEffects[0].TriggerSpell];

            // DONE: Check requirements
            byte reqLevel;
            reqLevel = Conversions.ToByte(mySqlQuery.Rows[0]["reqlevel"]);
            if (reqLevel == 0)
                reqLevel = (byte)spellInfo.spellLevel;
            uint spellCost;
            spellCost = Conversions.ToUInteger(mySqlQuery.Rows[0]["spellcost"]);
            int reqSpell;
            reqSpell = 0;
            if (WorldServiceLocator._WS_Spells.SpellChains.ContainsKey(spellInfo.ID))
            {
                reqSpell = WorldServiceLocator._WS_Spells.SpellChains[spellInfo.ID];
            }

            if (client.Character.HaveSpell(spellInfo.ID))
                return;
            if (client.Character.Copper < spellCost)
                return;
            if (client.Character.Level < reqLevel)
                return;
            if (reqSpell > 0 && client.Character.HaveSpell(reqSpell) == false)
                return;
            if (Conversions.ToInteger(mySqlQuery.Rows[0]["reqskill"]) > 0 && client.Character.HaveSkill(Conversions.ToInteger(mySqlQuery.Rows[0]["reqskill"]), Conversions.ToInteger(mySqlQuery.Rows[0]["reqskillvalue"])) == false)
                return;

            // TODO: Check proffessions - only alowed to learn 2!
            try
            {
                // DONE: Get the money
                client.Character.Copper -= spellCost;
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                client.Character.SendCharacterUpdate(false);

                // DONE: Cast the spell
                var spellTargets = new WS_Spells.SpellTargets();
                WS_Base.BaseUnit argobjCharacter = client.Character;
                spellTargets.SetTarget_UNIT(ref argobjCharacter);
                WS_Base.BaseUnit tmpCaster;
                if (spellInfo.SpellVisual == 222)
                {
                    tmpCaster = client.Character;
                }
                else
                {
                    tmpCaster = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid];
                }

                WS_Base.BaseObject argCaster = tmpCaster;
                var castParams = new WS_Spells.CastSpellParameters(ref spellTargets, ref argCaster, spellID, true);
                ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].MoveToInstant(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionX, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionY, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionZ, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].SpawnO);
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Training Spell Error: Unable to cast spell. [{0}:{1}]", Environment.NewLine, e.ToString());

                // TODO: Fix this opcode
                // Dim errorPacket As New PacketClass(OPCODES.SMSG_TRAINER_BUY_FAILED)
                // errorPacket.AddUInt64(cGUID)
                // errorPacket.AddInt32(SpellID)
                // Client.Send(errorPacket)
                // errorPacket.Dispose()
            }

            // DONE: Send response
            Packets.PacketClass response;
            response = new Packets.PacketClass(OPCODES.SMSG_TRAINER_BUY_SUCCEEDED);
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

        /// <summary>
        /// Sends the trainer list.
        /// </summary>
        /// <param name="objCharacter">The objCharacter.</param>
        /// <param name="cGuid">The objCharacter GUID.</param>
        /// <returns></returns>
        private void SendTrainerList(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
        {
            // DONE: Query the database and sort spells
            // Dim NeedToLearn As Boolean = False
            // Dim noTrainID As Integer = 0
            DataTable spellSqlQuery;
            spellSqlQuery = new DataTable();
            // Dim npcTextSQLQuery As New DataTable
            CreatureInfo creatureInfo;
            creatureInfo = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].CreatureInfo;
            List<DataRow> spellsList;
            spellsList = new List<DataRow>();
            if ((creatureInfo.Classe == 0 || creatureInfo.Classe == objCharacter.Classe) && (creatureInfo.Race == 0 || creatureInfo.Race == objCharacter.Race || objCharacter.GetReputation(creatureInfo.Faction) == ReputationRank.Exalted))
            {
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM npc_trainer WHERE entry = {0};", (object)WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID), spellSqlQuery);
                foreach (DataRow sellRow in spellSqlQuery.Rows)
                    spellsList.Add(sellRow);
            }

            // DONE: Build the packet
            var packet = new Packets.PacketClass(OPCODES.SMSG_TRAINER_LIST);
            packet.AddUInt64(cGuid);
            packet.AddInt32(creatureInfo.TrainerType);
            packet.AddInt32(spellsList.Count);              // Trains Length

            // DONE: Discount on reputation
            float discountMod = objCharacter.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].Faction);
            int spellID;
            foreach (DataRow sellRow in spellsList)
            {
                spellID = Conversions.ToInteger(sellRow["spell"]);
                if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(spellID) == false)
                    continue;
                var spellInfo = WorldServiceLocator._WS_Spells.SPELLs[spellID];
                if (spellInfo.SpellEffects[0] is object && spellInfo.SpellEffects[0].TriggerSpell > 0)
                    spellInfo = WorldServiceLocator._WS_Spells.SPELLs[spellInfo.SpellEffects[0].TriggerSpell];
                int reqSpell = 0;
                if (WorldServiceLocator._WS_Spells.SpellChains.ContainsKey(spellInfo.ID))
                {
                    reqSpell = WorldServiceLocator._WS_Spells.SpellChains[spellInfo.ID];
                }

                byte spellLevel = Conversions.ToByte(sellRow["reqlevel"]);
                if (spellLevel == 0)
                    spellLevel = (byte)spellInfo.spellLevel;

                // CanLearn (0):Green (1):Red (2):Gray
                byte canLearnFlag = 0;
                if (objCharacter.HaveSpell(spellInfo.ID))
                {
                    // NOTE: Already have that spell
                    canLearnFlag = 2;
                }
                else if (objCharacter.Level >= spellLevel)
                {
                    if (reqSpell > 0 && objCharacter.HaveSpell(reqSpell) == false)
                    {
                        canLearnFlag = 1;
                    }

                    if (canLearnFlag == 0 && Conversions.ToInteger(sellRow["reqskill"]) != 0)
                    {
                        if (Conversions.ToInteger(sellRow["reqskillvalue"]) != 0)
                        {
                            if (objCharacter.HaveSkill(Conversions.ToInteger(sellRow["reqskill"]), Conversions.ToInteger(sellRow["reqskillvalue"])) == false)
                            {
                                canLearnFlag = 1;
                            }
                        }
                    }
                }
                else
                {
                    // NOTE: Doesn't meet requirements, cannot learn that spell
                    canLearnFlag = 1;
                }

                // TODO: Check if the spell is a profession
                int isProf = 0;
                if (spellInfo.SpellEffects[1] is object && spellInfo.SpellEffects[1].ID == SpellEffects_Names.SPELL_EFFECT_SKILL_STEP)
                {
                    isProf = 1;
                }

                packet.AddInt32(spellID); // SpellID
                packet.AddInt8(canLearnFlag);
                packet.AddInt32((int)(Conversions.ToInteger(sellRow["spellcost"]) * discountMod));              // SpellCost
                packet.AddInt32(0);
                packet.AddInt32(isProf); // Profession
                packet.AddInt8(spellLevel);
                packet.AddInt32(Conversions.ToInteger(sellRow["reqskill"]));          // Required Skill
                packet.AddInt32(Conversions.ToInteger(sellRow["reqskillvalue"]));    // Required Skill Value
                packet.AddInt32(reqSpell);          // Required Spell
                packet.AddInt32(0);
                packet.AddInt32(0);
            }

            packet.AddString("Hello! Ready for some training?"); // Trainer UI message?
            objCharacter.client.Send(ref packet);
            packet.Dispose();
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */        /// <summary>
        /// Called when [CMSG_LIST_INVENTORY] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_LIST_INVENTORY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong guid;
            guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LIST_INVENTORY [GUID={2:X}]", client.IP, client.Port, guid);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) == false || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_VENDOR) == 0)
                return;
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Evade)
                return;
            WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].StopMoving();
            SendListInventory(ref client.Character, guid);
        }

        /// <summary>
        /// Called when [CMSG_SELL_ITEM] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_SELL_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 22)
                return;
            packet.GetInt16();
            ulong vendorGuid;
            vendorGuid = packet.GetUInt64();
            ulong itemGuid;
            itemGuid = packet.GetUInt64();
            byte count = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SELL_ITEM [vendorGuid={2:X} itemGuid={3:X} Count={4}]", client.IP, client.Port, vendorGuid, itemGuid, count);
            try
            {
                if (itemGuid == 0m || WorldServiceLocator._WorldServer.WORLD_ITEMs.ContainsKey(itemGuid) == false)
                {
                    var okPckt = new Packets.PacketClass(OPCODES.SMSG_SELL_ITEM);
                    try
                    {
                        okPckt.AddUInt64(vendorGuid);
                        okPckt.AddUInt64(itemGuid);
                        okPckt.AddInt8(SELL_ERROR.SELL_ERR_CANT_FIND_ITEM);
                        client.Send(ref okPckt);
                    }
                    finally
                    {
                        okPckt.Dispose();
                    }

                    return;
                }
                // DONE: You can't sell someone else's items
                if (WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].OwnerGUID != client.Character.GUID)
                {
                    var okPckt = new Packets.PacketClass(OPCODES.SMSG_SELL_ITEM);
                    try
                    {
                        okPckt.AddUInt64(vendorGuid);
                        okPckt.AddUInt64(itemGuid);
                        okPckt.AddInt8(SELL_ERROR.SELL_ERR_CANT_FIND_ITEM);
                        client.Send(ref okPckt);
                    }
                    finally
                    {
                        okPckt.Dispose();
                    }

                    return;
                }

                if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(vendorGuid))
                {
                    var okPckt = new Packets.PacketClass(OPCODES.SMSG_SELL_ITEM);
                    try
                    {
                        okPckt.AddUInt64(vendorGuid);
                        okPckt.AddUInt64(itemGuid);
                        okPckt.AddInt8(SELL_ERROR.SELL_ERR_CANT_FIND_VENDOR);
                        client.Send(ref okPckt);
                    }
                    finally
                    {
                        okPckt.Dispose();
                    }

                    return;
                }
                // DONE: Can't sell quest items
                if (WorldServiceLocator._WorldServer.ITEMDatabase[WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemEntry].SellPrice == 0 | WorldServiceLocator._WorldServer.ITEMDatabase[WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemEntry].ObjectClass == ITEM_CLASS.ITEM_CLASS_QUEST)
                {
                    var okPckt = new Packets.PacketClass(OPCODES.SMSG_SELL_ITEM);
                    try
                    {
                        okPckt.AddUInt64(vendorGuid);
                        okPckt.AddUInt64(itemGuid);
                        okPckt.AddInt8(SELL_ERROR.SELL_ERR_CANT_SELL_ITEM);
                        client.Send(ref okPckt);
                    }
                    finally
                    {
                        okPckt.Dispose();
                    }

                    return;
                }
                // DONE: Can't cheat and sell items that are located in the buyback
                for (byte i = BuyBackSlots.BUYBACK_SLOT_START, loopTo = BuyBackSlots.BUYBACK_SLOT_END - 1; i <= loopTo; i++)
                {
                    if (client.Character.Items.ContainsKey(i) && client.Character.Items[i].GUID == itemGuid)
                    {
                        var okPckt = new Packets.PacketClass(OPCODES.SMSG_SELL_ITEM);
                        okPckt.AddUInt64(vendorGuid);
                        okPckt.AddUInt64(itemGuid);
                        okPckt.AddInt8(SELL_ERROR.SELL_ERR_CANT_FIND_ITEM);
                        client.Send(ref okPckt);
                        okPckt.Dispose();
                        return;
                    }
                }

                if (count < 1)
                    count = (byte)WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].StackCount;
                if (WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].StackCount > count)
                {
                    WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].StackCount -= count;
                    var tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(itemGuid); // Lets create a new stack to place in the buyback
                    WorldServiceLocator._WorldServer.itemGuidCounter = (ulong)(WorldServiceLocator._WorldServer.itemGuidCounter + 1m); // Get a new GUID for our new stack
                    tmpItem.GUID = WorldServiceLocator._WorldServer.itemGuidCounter;
                    tmpItem.StackCount = count;
                    client.Character.ItemADD_BuyBack(ref tmpItem);
                    client.Character.Copper = (uint)(client.Character.Copper + WorldServiceLocator._WorldServer.ITEMDatabase[WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemEntry].SellPrice * count);
                    client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                    client.Character.SendItemUpdate(WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid]);
                    WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].Save(false);
                }
                else
                {
                    // DONE: Move item to buyback
                    // TODO: Remove items that expire in the buyback, in mangos it seems like they use 30 hours until it's removed.

                    foreach (KeyValuePair<byte, ItemObject> item in client.Character.Items)
                    {
                        if (item.Value.GUID == itemGuid)
                        {
                            client.Character.Copper = (uint)(client.Character.Copper + WorldServiceLocator._WorldServer.ITEMDatabase[item.Value.ItemEntry].SellPrice * item.Value.StackCount);
                            client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                            if (item.Key < InventorySlots.INVENTORY_SLOT_BAG_END)
                            {
                                var argItem = item.Value;
                                client.Character.UpdateRemoveItemStats(ref argItem, item.Key);
                                item.Value = argItem;
                            }

                            client.Character.ItemREMOVE(item.Value.GUID, false, true);
                            var argItem1 = item.Value;
                            client.Character.ItemADD_BuyBack(ref argItem1);
                            item.Value = argItem1;
                            var okPckt = new Packets.PacketClass(OPCODES.SMSG_SELL_ITEM);
                            okPckt.AddUInt64(vendorGuid);
                            okPckt.AddUInt64(itemGuid);
                            okPckt.AddInt8(0);
                            client.Send(ref okPckt);
                            okPckt.Dispose();
                            return;
                        }
                    }

                    for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_1, loopTo1 = InventorySlots.INVENTORY_SLOT_BAG_4; bag <= loopTo1; bag++)
                    {
                        if (client.Character.Items.ContainsKey(bag))
                        {
                            foreach (KeyValuePair<byte, ItemObject> item in client.Character.Items[bag].Items)
                            {
                                if (item.Value.GUID == itemGuid)
                                {
                                    client.Character.Copper = (uint)(client.Character.Copper + WorldServiceLocator._WorldServer.ITEMDatabase[item.Value.ItemEntry].SellPrice * item.Value.StackCount);
                                    client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                                    client.Character.ItemREMOVE(item.Value.GUID, false, true);
                                    var argItem2 = item.Value;
                                    client.Character.ItemADD_BuyBack(ref argItem2);
                                    item.Value = argItem2;
                                    var okPckt = new Packets.PacketClass(OPCODES.SMSG_SELL_ITEM);
                                    okPckt.AddUInt64(vendorGuid);
                                    okPckt.AddUInt64(itemGuid);
                                    okPckt.AddInt8(0);
                                    client.Send(ref okPckt);
                                    okPckt.Dispose();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error selling item: {0}{1}", Environment.NewLine, e.ToString());
            }
        }

        /// <summary>
        /// Called when [CMSG_BUY_ITEM] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_BUY_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 19)
                return;
            packet.GetInt16();
            ulong vendorGuid = packet.GetUInt64();
            int itemID = packet.GetInt32();
            byte count = packet.GetInt8();
            byte slot = packet.GetInt8();       // ??
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(vendorGuid) == false || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_ARMORER) == 0 && (WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_VENDOR) == 0)
                return;
            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID) == false)
                return;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUY_ITEM [vendorGuid={2:X} ItemID={3} Count={4} Slot={5}]", client.IP, client.Port, vendorGuid, itemID, count, slot);

            // TODO: Make sure that the vendor sells the item!

            // DONE: No count cheating
            if (count > WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable)
                count = (byte)WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable;
            if (count == 0)
                count = 1;

            // DONE: Can't buy quest items
            if (WorldServiceLocator._WorldServer.ITEMDatabase[itemID].ObjectClass == ITEM_CLASS.ITEM_CLASS_QUEST)
            {
                var errorPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                try
                {
                    errorPckt.AddUInt64(vendorGuid);
                    errorPckt.AddInt32(itemID);
                    errorPckt.AddInt8(BUY_ERROR.BUY_ERR_SELLER_DONT_LIKE_YOU);
                    client.Send(ref errorPckt);
                }
                finally
                {
                    errorPckt.Dispose();
                }

                return;
            }

            if (count * WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyCount > WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable)
                count = (byte)(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable / (double)WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyCount);

            // DONE: Reputation discount
            float discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].Faction);
            int itemPrice = (int)(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyPrice * discountMod);
            if (client.Character.Copper < itemPrice * count)
            {
                var errorPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                try
                {
                    errorPckt.AddUInt64(vendorGuid);
                    errorPckt.AddInt32(itemID);
                    errorPckt.AddInt8(BUY_ERROR.BUY_ERR_NOT_ENOUGHT_MONEY);
                    client.Send(ref errorPckt);
                }
                finally
                {
                    errorPckt.Dispose();
                }

                return;
            }

            client.Character.Copper = (uint)(client.Character.Copper - itemPrice * count);
            client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
            client.Character.SendCharacterUpdate(false);
            var tmpItem = new ItemObject(itemID, client.Character.GUID) { StackCount = count * WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyCount };

            // TODO: Remove one count of the item from the vendor if it's not unlimited

            if (!client.Character.ItemADD(ref tmpItem))
            {
                tmpItem.Delete();
                client.Character.Copper = (uint)(client.Character.Copper + itemPrice);
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
            }
            else
            {
                var okPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_ITEM);
                okPckt.AddUInt64(vendorGuid);
                okPckt.AddInt32(itemID);
                okPckt.AddInt32(count);
                client.Send(ref okPckt);
                okPckt.Dispose();
            }
        }

        /// <summary>
        /// Called when [CMSG_BUY_ITEM_IN_SLOT] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_BUY_ITEM_IN_SLOT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 27)
                return;
            packet.GetInt16();
            ulong vendorGuid = packet.GetUInt64();
            int itemID = packet.GetInt32();
            ulong clientGuid = packet.GetUInt64();
            byte slot = packet.GetInt8();
            byte count = packet.GetInt8();
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(vendorGuid) == false || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_ARMORER) == 0 && (WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_VENDOR) == 0)
                return;
            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID) == false)
                return;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUY_ITEM_IN_SLOT [vendorGuid={2:X} ItemID={3} Count={4} Slot={5}]", client.IP, client.Port, vendorGuid, itemID, count, slot);

            // DONE: No count cheating
            if (count > WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable)
                count = (byte)WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Stackable;

            // DONE: Can't buy quest items
            if (WorldServiceLocator._WorldServer.ITEMDatabase[itemID].ObjectClass == ITEM_CLASS.ITEM_CLASS_QUEST)
            {
                var errorPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                errorPckt.AddUInt64(vendorGuid);
                errorPckt.AddInt32(itemID);
                errorPckt.AddInt8(BUY_ERROR.BUY_ERR_SELLER_DONT_LIKE_YOU);
                client.Send(ref errorPckt);
                errorPckt.Dispose();
                return;
            }

            // DONE: Reputation discount
            float discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].Faction);
            int itemPrice = (int)(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyPrice * discountMod);
            if (client.Character.Copper < itemPrice * count)
            {
                var errorPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                errorPckt.AddUInt64(vendorGuid);
                errorPckt.AddInt32(itemID);
                errorPckt.AddInt8(BUY_ERROR.BUY_ERR_NOT_ENOUGHT_MONEY);
                client.Send(ref errorPckt);
                errorPckt.Dispose();
                return;
            }

            byte bag = 0;
            if (clientGuid == client.Character.GUID)
            {
                // Store in inventory
                bag = 0;
                if (client.Character.Items.ContainsKey(slot))
                {
                    var errorPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                    errorPckt.AddUInt64(vendorGuid);
                    errorPckt.AddInt32(itemID);
                    errorPckt.AddInt8(BUY_ERROR.BUY_ERR_CANT_CARRY_MORE);
                    client.Send(ref errorPckt);
                    errorPckt.Dispose();
                    return;
                }
            }
            else
            {
                // Store in bag
                byte i;
                var loopTo = InventorySlots.INVENTORY_SLOT_BAG_4;
                for (i = InventorySlots.INVENTORY_SLOT_BAG_1; i <= loopTo; i++)
                {
                    if (client.Character.Items[i].GUID == clientGuid)
                    {
                        bag = i;
                        break;
                    }
                }

                if (bag == 0)
                {
                    var okPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                    okPckt.AddUInt64(vendorGuid);
                    okPckt.AddInt32(itemID);
                    okPckt.AddInt8(BUY_ERROR.BUY_ERR_CANT_FIND_ITEM);
                    client.Send(ref okPckt);
                    okPckt.Dispose();
                    return;
                }

                if (client.Character.Items[bag].Items.ContainsKey(slot))
                {
                    var errorPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                    errorPckt.AddUInt64(vendorGuid);
                    errorPckt.AddInt32(itemID);
                    errorPckt.AddInt8(BUY_ERROR.BUY_ERR_CANT_CARRY_MORE);
                    client.Send(ref errorPckt);
                    errorPckt.Dispose();
                    return;
                }
            }

            var tmpItem = new ItemObject(itemID, client.Character.GUID) { StackCount = count };
            byte errCode = client.Character.ItemCANEQUIP(tmpItem, bag, slot);
            if (errCode != InventoryChangeFailure.EQUIP_ERR_OK)
            {
                if (errCode != InventoryChangeFailure.EQUIP_ERR_YOU_MUST_REACH_LEVEL_N)
                {
                    var errorPckt = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                    errorPckt.AddInt8(errCode);
                    errorPckt.AddUInt64(0UL);
                    errorPckt.AddUInt64(0UL);
                    errorPckt.AddInt8(0);
                    client.Send(ref errorPckt);
                    errorPckt.Dispose();
                }

                tmpItem.Delete();
                return;
            }
            else
            {
                client.Character.Copper = (uint)(client.Character.Copper - itemPrice * count);
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                if (!client.Character.ItemSETSLOT(ref tmpItem, slot, bag))
                {
                    tmpItem.Delete();
                    client.Character.Copper = (uint)(client.Character.Copper + itemPrice);
                    client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                }
                else
                {
                    var okPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_ITEM);
                    okPckt.AddUInt64(vendorGuid);
                    okPckt.AddInt32(itemID);
                    okPckt.AddInt32(count);
                    client.Send(ref okPckt);
                    okPckt.Dispose();
                }

                client.Character.SendCharacterUpdate(false);
            }
        }

        /// <summary>
        /// Called when [CMSG_BUYBACK_ITEM] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_BUYBACK_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong vendorGuid = packet.GetUInt64();
            int slot = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUYBACK_ITEM [vendorGuid={2:X} Slot={3}]", client.IP, client.Port, vendorGuid, slot);

            // TODO: If item is not located in your buyback you can't buy it back (this checking below doesn't work)
            if (slot < BuyBackSlots.BUYBACK_SLOT_START || slot >= BuyBackSlots.BUYBACK_SLOT_END || client.Character.Items.ContainsKey((byte)slot) == false)
            {
                var errorPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                try
                {
                    errorPckt.AddUInt64(vendorGuid);
                    errorPckt.AddInt32(0);
                    errorPckt.AddInt8(BUY_ERROR.BUY_ERR_CANT_FIND_ITEM);
                    client.Send(ref errorPckt);
                }
                finally
                {
                    errorPckt.Dispose();
                }

                return;
            }
            // DONE: Check if you can afford it
            var tmpItem = client.Character.Items[(byte)slot];
            if (client.Character.Copper < tmpItem.ItemInfo.SellPrice * tmpItem.StackCount)
            {
                var errorPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                try
                {
                    errorPckt.AddUInt64(vendorGuid);
                    errorPckt.AddInt32(tmpItem.ItemEntry);
                    errorPckt.AddInt8(BUY_ERROR.BUY_ERR_NOT_ENOUGHT_MONEY);
                    client.Send(ref errorPckt);
                }
                finally
                {
                    errorPckt.Dispose();
                }

                return;
            }

            // DONE: Move item to the inventory, if it's unable to do that tell the client that the bags are full
            client.Character.ItemREMOVE(tmpItem.GUID, false, true);
            if (client.Character.ItemADD_AutoSlot(ref tmpItem))
            {
                byte eSlot = slot - BuyBackSlots.BUYBACK_SLOT_START;
                client.Character.Copper = (uint)(client.Character.Copper - tmpItem.ItemInfo.SellPrice * tmpItem.StackCount);
                client.Character.BuyBackTimeStamp[eSlot] = 0;
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1 + eSlot, 0);
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BUYBACK_PRICE_1 + eSlot, 0);
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                client.Character.SendCharacterUpdate();
            }
            else
            {
                WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_INVENTORY_FULL, 0, 0);
                client.Character.ItemSETSLOT(ref tmpItem, 0, (byte)slot);
            }
        }

        /// <summary>
        /// Called when [CMSG_REPAIR_ITEM] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_REPAIR_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 21)
                return;
            packet.GetInt16();
            ulong vendorGuid = packet.GetUInt64();
            ulong itemGuid = packet.GetUInt64();
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(vendorGuid) == false || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_ARMORER) == 0)
                return;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_REPAIR_ITEM [vendorGuid={2:X} itemGuid={3:X}]", client.IP, client.Port, vendorGuid, itemGuid);

            // DONE: Reputation discount
            float discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[vendorGuid].Faction);
            uint price;
            if (itemGuid != 0m)
            {
                price = (uint)(WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].GetDurabulityCost * discountMod);
                if (client.Character.Copper >= price)
                {
                    client.Character.Copper -= price;
                    client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                    client.Character.SendCharacterUpdate(false);
                    WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ModifyToDurability(100.0f, ref client);
                }
            }
            else
            {
                for (byte i = 0, loopTo = EquipmentSlots.EQUIPMENT_SLOT_END - 1; i <= loopTo; i++)
                {
                    if (client.Character.Items.ContainsKey(i))
                    {
                        price = (uint)(client.Character.Items[i].GetDurabulityCost * discountMod);
                        if (client.Character.Copper >= price)
                        {
                            client.Character.Copper -= price;
                            client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                            client.Character.SendCharacterUpdate(false);
                            client.Character.Items[i].ModifyToDurability(100.0f, ref client);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends the list inventory.
        /// </summary>
        /// <param name="objCharacter">The obj char.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        private void SendListInventory(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
        {
            try
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_LIST_INVENTORY);
                packet.AddUInt64(guid);
                var mySqlQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM npc_vendor WHERE entry = {0};", (object)WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].ID), mySqlQuery);
                int dataPos = packet.Data.Length;
                packet.AddInt8(0); // Will be updated later
                byte i = 0;
                int itemID;
                foreach (DataRow sellRow in mySqlQuery.Rows)
                {
                    itemID = Conversions.ToInteger(sellRow["item"]);
                    // DONE: You will now only see items for your class
                    if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID) == false)
                    {
                        var tmpItem = new WS_Items.ItemInfo(itemID);
                        // The New does a an add to the .Containskey collection above
                    }

                    if (WorldServiceLocator._WorldServer.ITEMDatabase[itemID].AvailableClasses == 0L || Conversions.ToBoolean(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].AvailableClasses & objCharacter.ClassMask))
                    {
                        i = (byte)(i + 1);
                        packet.AddInt32(-1); // i-1
                        packet.AddInt32(itemID);
                        packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].Model);

                        // AviableCount
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectLessEqual(sellRow["maxcount"], 0, false)))
                        {
                            packet.AddInt32(-1);
                        }
                        else
                        {
                            packet.AddInt32(Conversions.ToInteger(sellRow["maxcount"]));
                        }

                        // DONE: Discount on reputation
                        float discountMod = objCharacter.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Faction);
                        packet.AddInt32((int)(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyPrice * discountMod));
                        packet.AddInt32(-1); // Durability
                        packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[itemID].BuyCount);
                    }
                }

                if (i > 0)
                    packet.AddInt8(i, dataPos);
                objCharacter.client.Send(ref packet);
                packet.Dispose();
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error while listing inventory.{0}", Environment.NewLine + e.ToString());
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */        /// <summary>
        /// Called when [CMSG_AUTOBANK_ITEM] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_AUTOBANK_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 7)
                return;
            packet.GetInt16();
            byte srcBag = packet.GetInt8();
            byte srcSlot = packet.GetInt8();
            if (srcBag == 255)
                srcBag = 0;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOBANK_ITEM [srcSlot={2}:{3}]", client.IP, client.Port, srcBag, srcSlot);
            for (byte dstSlot = BankItemSlots.BANK_SLOT_ITEM_START, loopTo = BankItemSlots.BANK_SLOT_ITEM_END; dstSlot <= loopTo; dstSlot++)
            {
                if (!client.Character.Items.ContainsKey(dstSlot))
                {
                    client.Character.ItemSWAP(srcBag, srcSlot, 0, dstSlot);
                    return;
                }
            }

            for (byte dstBag = BankBagSlots.BANK_SLOT_BAG_START, loopTo1 = BankBagSlots.BANK_SLOT_BAG_END - 1; dstBag <= loopTo1; dstBag++)
            {
                if (client.Character.Items.ContainsKey(dstBag))
                {
                    if (client.Character.Items[dstBag].ItemInfo.IsContainer)
                    {
                        for (byte dstSlot = 0, loopTo2 = (byte)(client.Character.Items[dstBag].ItemInfo.ContainerSlots - 1); dstSlot <= loopTo2; dstSlot++)
                        {
                            if (!client.Character.Items[dstBag].Items.ContainsKey(dstSlot))
                            {
                                client.Character.ItemSWAP(srcBag, srcSlot, dstBag, dstSlot);
                                // Not sure, but we probably have to send the "EQUIP_ERR_OK = 0," packet to play the "moving sound".
                                return;
                            }
                        }
                    }
                }
            }

            // If you ever get here, send error packet. I think it should be "EQUIP_ERR_INVENTORY_FULL = 50, // ERR_INV_FULL" here.
        }

        /// <summary>
        /// Called when [CMSG_AUTOSTORE_BANK_ITEM] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_AUTOSTORE_BANK_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 7)
                return;
            packet.GetInt16();
            byte srcBag = packet.GetInt8();
            byte srcSlot = packet.GetInt8();
            if (srcBag == 255)
                srcBag = 0;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOSTORE_BANK_ITEM [srcSlot={2}:{3}]", client.IP, client.Port, srcBag, srcSlot);
            for (byte dstSlot = InventoryPackSlots.INVENTORY_SLOT_ITEM_START, loopTo = InventoryPackSlots.INVENTORY_SLOT_ITEM_END; dstSlot <= loopTo; dstSlot++)
            {
                if (!client.Character.Items.ContainsKey(dstSlot))
                {
                    client.Character.ItemSWAP(srcBag, srcSlot, 0, dstSlot);
                    return;
                }
            }

            for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_START, loopTo1 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; bag <= loopTo1; bag++)
            {
                if (client.Character.Items.ContainsKey(bag))
                {
                    if (client.Character.Items[bag].ItemInfo.IsContainer)
                    {
                        for (byte dstSlot = 0, loopTo2 = (byte)(client.Character.Items[bag].ItemInfo.ContainerSlots - 1); dstSlot <= loopTo2; dstSlot++)
                        {
                            if (!client.Character.Items[bag].Items.ContainsKey(dstSlot))
                            {
                                client.Character.ItemSWAP(srcBag, srcSlot, bag, dstSlot);
                                // Not sure, but we probably have to send the "EQUIP_ERR_OK = 0," packet to play the "moving sound".
                                return;
                            }
                        }
                    }
                }
            }

            // If you ever get here, send error packet I think it should be "EQUIP_ERR_BAG_FULL3 = 53, // ERR_BAG_FULL" here.
        }

        /// <summary>
        /// Called when [CMSG_BUY_BANK_SLOT] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_BUY_BANK_SLOT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUY_BANK_SLOT", client.IP, client.Port);
            if (client.Character.Items_AvailableBankSlots < DbcBankBagSlotsMax && client.Character.Copper >= DbcBankBagSlotPrices[client.Character.Items_AvailableBankSlots])
            {
                client.Character.Copper = (uint)(client.Character.Copper - DbcBankBagSlotPrices[client.Character.Items_AvailableBankSlots]);
                client.Character.Items_AvailableBankSlots = (byte)(client.Character.Items_AvailableBankSlots + 1);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters SET char_bankSlots = {0}, char_copper = {1};", client.Character.Items_AvailableBankSlots, client.Character.Copper));
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_BYTES_2, client.Character.cPlayerBytes2);
                client.Character.SendCharacterUpdate(false);
            }
            else
            {
                var errorPckt = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                try
                {
                    errorPckt.AddUInt64(0UL);
                    errorPckt.AddInt32(0);
                    errorPckt.AddInt8(BUY_ERROR.BUY_ERR_NOT_ENOUGHT_MONEY);
                    client.Send(ref errorPckt);
                }
                finally
                {
                    errorPckt.Dispose();
                }
            }
        }

        /// <summary>
        /// Called when [CMSG_BANKER_ACTIVATE] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_BANKER_ACTIVATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BANKER_ACTIVATE [GUID={2:X}]", client.IP, client.Port, guid);
            SendShowBank(ref client.Character, guid);
        }

        /// <summary>
        /// Sends the opcode to show the bank.
        /// </summary>
        /// <param name="objCharacter">The objCharacter.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        private void SendShowBank(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_SHOW_BANK);
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

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */        /// <summary>
        /// Sends the bind point confirm.
        /// </summary>
        /// <param name="objCharacter">The obj char.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        private void SendBindPointConfirm(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
        {
            objCharacter.SendGossipComplete();
            objCharacter.ZoneCheck();
            var packet = new Packets.PacketClass(OPCODES.SMSG_BINDER_CONFIRM);
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

        /// <summary>
        /// Called when [CMSG_BINDER_ACTIVATE] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_BINDER_ACTIVATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BINDER_ACTIVATE [binderGUID={2:X}]", client.IP, client.Port, guid);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) == false)
                return;
            client.Character.SendGossipComplete();
            var spellTargets = new WS_Spells.SpellTargets();
            WS_Base.BaseUnit argobjCharacter = client.Character;
            spellTargets.SetTarget_UNIT(ref argobjCharacter);
            var tmp = WorldServiceLocator._WorldServer.WORLD_CREATUREs;
            WS_Base.BaseObject argCaster = tmp[guid];
            var castParams = new WS_Spells.CastSpellParameters(ref spellTargets, ref argCaster, 3286, true);
            tmp[guid] = (WS_Creatures.CreatureObject)argCaster;
            ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
        }

        /// <summary>
        /// Sends the talent wipe confirm.
        /// </summary>
        /// <param name="objCharacter">The obj char.</param>
        /// <param name="cost">The cost.</param>
        /// <returns></returns>
        private void SendTalentWipeConfirm(ref WS_PlayerData.CharacterObject objCharacter, int cost)
        {
            var packet = new Packets.PacketClass(OPCODES.MSG_TALENT_WIPE_CONFIRM);
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

        /// <summary>
        /// Called when [MSG_TALENT_WIPE_CONFIRM] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_MSG_TALENT_WIPE_CONFIRM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                packet.GetInt16();
                ulong guid = packet.GetPackGuid();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_TALENT_WIPE_CONFIRM [GUID={2:X}]", client.IP, client.Port, guid);
                if (client.Character.Level < 10)
                    return;

                // DONE: Removing all talents
                foreach (KeyValuePair<int, WS_DBCDatabase.TalentInfo> talentInfo in WorldServiceLocator._WS_DBCDatabase.Talents)
                {
                    for (int i = 0; i <= 4; i++)
                    {
                        if (talentInfo.Value.RankID[i] != 0)
                        {
                            if (client.Character.HaveSpell(talentInfo.Value.RankID[i]))
                            {
                                client.Character.UnLearnSpell(talentInfo.Value.RankID[i]);
                            }
                        }
                    }
                }

                // DONE: Reset Talentpoints to Level - 9
                client.Character.TalentPoints = (byte)(client.Character.Level - 9);
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_CHARACTER_POINTS1, client.Character.TalentPoints);
                client.Character.SendCharacterUpdate(true);

                // DONE: Use spell 14867
                var SMSG_SPELL_START = new Packets.PacketClass(OPCODES.SMSG_SPELL_START);
                try
                {
                    SMSG_SPELL_START.AddPackGUID(client.Character.GUID);
                    SMSG_SPELL_START.AddPackGUID(guid);
                    SMSG_SPELL_START.AddInt16(14867);
                    SMSG_SPELL_START.AddInt16(0);
                    SMSG_SPELL_START.AddInt16(0xF);
                    SMSG_SPELL_START.AddInt32(0);
                    SMSG_SPELL_START.AddInt16(0);
                    client.Send(ref SMSG_SPELL_START);
                }
                finally
                {
                    SMSG_SPELL_START.Dispose();
                }

                var SMSG_SPELL_GO = new Packets.PacketClass(OPCODES.SMSG_SPELL_GO);
                try
                {
                    SMSG_SPELL_GO.AddPackGUID(client.Character.GUID);
                    SMSG_SPELL_GO.AddPackGUID(guid);
                    SMSG_SPELL_GO.AddInt16(14867);
                    SMSG_SPELL_GO.AddInt16(0);
                    SMSG_SPELL_GO.AddInt8(0xD);
                    SMSG_SPELL_GO.AddInt8(0x1);
                    SMSG_SPELL_GO.AddInt8(0x1);
                    SMSG_SPELL_GO.AddUInt64(client.Character.GUID);
                    SMSG_SPELL_GO.AddInt32(0);
                    SMSG_SPELL_GO.AddInt16(0x200);
                    SMSG_SPELL_GO.AddInt16(0);
                    client.Send(ref SMSG_SPELL_GO);
                }
                finally
                {
                    SMSG_SPELL_GO.Dispose();
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error unlearning talents: {0}{1}", Environment.NewLine, e.ToString());
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class TDefaultTalk : TBaseTalk
        {

            /// <summary>
            /// Called when [gossip hello].
            /// </summary>
            /// <param name="objCharacter">The obj char.</param>
            /// <param name="cGuid">The objCharacter GUID.</param>
            /// <returns></returns>
            public override void OnGossipHello(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
            {
                int textID = 0;
                var npcMenu = new GossipMenu();
                objCharacter.TalkMenuTypes.Clear();
                var creatureInfo = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].CreatureInfo;
                try
                {
                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_VENDOR || creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_ARMORER)
                    {
                        npcMenu.AddMenu("Let me browse your goods.", MenuIcon.MENUICON_VENDOR);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_VENDOR);
                    }

                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_TAXIVENDOR)
                    {
                        npcMenu.AddMenu("I want to continue my journey.", MenuIcon.MENUICON_TAXI);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TAXIVENDOR);
                    }

                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_TRAINER)
                    {
                        if (creatureInfo.TrainerType == TrainerTypes.TRAINER_TYPE_CLASS)
                        {
                            if (creatureInfo.Classe != objCharacter.Classe)
                            {
                                switch (creatureInfo.Classe)
                                {
                                    case var @case when @case == Classes.CLASS_DRUID:
                                        {
                                            textID = 4913;
                                            break;
                                        }

                                    case var case1 when case1 == Classes.CLASS_HUNTER:
                                        {
                                            textID = 10090;
                                            break;
                                        }

                                    case var case2 when case2 == Classes.CLASS_MAGE:
                                        {
                                            textID = 328;
                                            break;
                                        }

                                    case var case3 when case3 == Classes.CLASS_PALADIN:
                                        {
                                            textID = 1635;
                                            break;
                                        }

                                    case var case4 when case4 == Classes.CLASS_PRIEST:
                                        {
                                            textID = 4436;
                                            break;
                                        }

                                    case var case5 when case5 == Classes.CLASS_ROGUE:
                                        {
                                            textID = 4797;
                                            break;
                                        }

                                    case var case6 when case6 == Classes.CLASS_SHAMAN:
                                        {
                                            textID = 5003;
                                            break;
                                        }

                                    case var case7 when case7 == Classes.CLASS_WARLOCK:
                                        {
                                            textID = 5836;
                                            break;
                                        }

                                    case var case8 when case8 == Classes.CLASS_WARRIOR:
                                        {
                                            textID = 4985;
                                            break;
                                        }
                                }

                                GossipMenu argMenu = null;
                                QuestMenu argqMenu = null;
                                objCharacter.SendGossip(cGuid, textID, Menu: ref argMenu, qMenu: ref argqMenu);
                                return;
                            }
                            else
                            {
                                string localGetClassName() { var argClasse = objCharacter.Classe; var ret = WorldServiceLocator._Functions.GetClassName(ref argClasse); objCharacter.Classe = argClasse; return ret; }

                                npcMenu.AddMenu("I am interested in " + localGetClassName() + " training.", MenuIcon.MENUICON_TRAINER);
                                objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TRAINER);
                                if (objCharacter.Level >= 10)
                                {
                                    npcMenu.AddMenu("I want to unlearn all my talents.", MenuIcon.MENUICON_GOSSIP);
                                    objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TALENTWIPE);
                                }
                            }
                        }
                        else if (creatureInfo.TrainerType == TrainerTypes.TRAINER_TYPE_MOUNTS)
                        {
                            if (creatureInfo.Race > 0 && creatureInfo.Race != objCharacter.Race && objCharacter.GetReputation(creatureInfo.Faction) < ReputationRank.Exalted)
                            {
                                switch (creatureInfo.Race)
                                {
                                    case var case9 when case9 == Races.RACE_DWARF:
                                        {
                                            textID = 5865;
                                            break;
                                        }

                                    case var case10 when case10 == Races.RACE_GNOME:
                                        {
                                            textID = 4881;
                                            break;
                                        }

                                    case var case11 when case11 == Races.RACE_HUMAN:
                                        {
                                            textID = 5861;
                                            break;
                                        }

                                    case var case12 when case12 == Races.RACE_NIGHT_ELF:
                                        {
                                            textID = 5862;
                                            break;
                                        }

                                    case var case13 when case13 == Races.RACE_ORC:
                                        {
                                            textID = 5863;
                                            break;
                                        }

                                    case var case14 when case14 == Races.RACE_TAUREN:
                                        {
                                            textID = 5864;
                                            break;
                                        }

                                    case var case15 when case15 == Races.RACE_TROLL:
                                        {
                                            textID = 5816;
                                            break;
                                        }

                                    case var case16 when case16 == Races.RACE_UNDEAD:
                                        {
                                            textID = 624;
                                            break;
                                        }
                                }

                                GossipMenu argMenu1 = null;
                                QuestMenu argqMenu1 = null;
                                objCharacter.SendGossip(cGuid, textID, Menu: ref argMenu1, qMenu: ref argqMenu1);
                                return;
                            }
                            else
                            {
                                npcMenu.AddMenu("I am interested in mount training.", MenuIcon.MENUICON_TRAINER);
                                objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TRAINER);
                            }
                        }
                        else if (creatureInfo.TrainerType == TrainerTypes.TRAINER_TYPE_TRADESKILLS)
                        {
                            if (creatureInfo.TrainerSpell > 0 && objCharacter.HaveSpell(creatureInfo.TrainerSpell) == false)
                            {
                                textID = 11031;
                                GossipMenu argMenu2 = null;
                                QuestMenu argqMenu2 = null;
                                objCharacter.SendGossip(cGuid, textID, Menu: ref argMenu2, qMenu: ref argqMenu2);
                                return;
                            }
                            else
                            {
                                npcMenu.AddMenu("I am interested in professions training.", MenuIcon.MENUICON_TRAINER);
                                objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TRAINER);
                            }
                        }
                        else if (creatureInfo.TrainerType == TrainerTypes.TRAINER_TYPE_PETS)
                        {
                            if (objCharacter.Classe != Classes.CLASS_HUNTER)
                            {
                                textID = 3620;
                                GossipMenu argMenu3 = null;
                                QuestMenu argqMenu3 = null;
                                objCharacter.SendGossip(cGuid, textID, Menu: ref argMenu3, qMenu: ref argqMenu3);
                                return;
                            }
                            else
                            {
                                npcMenu.AddMenu("I am interested in pet training.", MenuIcon.MENUICON_TRAINER);
                                objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TRAINER);
                            }
                        }
                    }

                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_SPIRITHEALER)
                    {
                        textID = 580;
                        npcMenu.AddMenu("Return me to life", MenuIcon.MENUICON_GOSSIP);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_SPIRITHEALER);
                    }
                    // UNIT_NPC_FLAG_GUARD
                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_INNKEEPER)
                    {
                        npcMenu.AddMenu("Make this inn your home.", MenuIcon.MENUICON_BINDER);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_INNKEEPER);
                    }

                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_BANKER)
                    {
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_BANKER);
                    }

                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_PETITIONER)
                    {
                        npcMenu.AddMenu("I am interested in guilds.", MenuIcon.MENUICON_PETITION);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_ARENACHARTER);
                    }

                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_TABARDVENDOR)
                    {
                        npcMenu.AddMenu("I want to purchase a tabard.", MenuIcon.MENUICON_TABARD);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_TABARDVENDOR);
                    }

                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_BATTLEFIELDPERSON)
                    {
                        npcMenu.AddMenu("My blood hungers for battle.", MenuIcon.MENUICON_BATTLEMASTER);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_BATTLEFIELD);
                    }

                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_AUCTIONEER)
                    {
                        npcMenu.AddMenu("Wanna auction something?", MenuIcon.MENUICON_AUCTIONER);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_AUCTIONEER);
                    }

                    if (creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_STABLE)
                    {
                        npcMenu.AddMenu("Let me check my pet.", MenuIcon.MENUICON_VENDOR);
                        objCharacter.TalkMenuTypes.Add(Gossip_Option.GOSSIP_OPTION_STABLEPET);
                    }

                    if (textID == 0)
                        textID = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].NPCTextID;
                    if ((creatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_QUESTGIVER) == NPCFlags.UNIT_NPC_FLAG_QUESTGIVER)
                    {
                        var qMenu = WorldServiceLocator._WorldServer.ALLQUESTS.GetQuestMenu(ref objCharacter, cGuid);
                        if (qMenu.IDs.Count == 0 && npcMenu.Menus.Count == 0)
                            return;
                        if (npcMenu.Menus.Count == 0) // If we only have quests to list
                        {
                            if (qMenu.IDs.Count == 1) // If we only have one quest to list, we direct the client directly to it
                            {
                                int questID = Conversions.ToInteger(qMenu.IDs[0]);
                                if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
                                {
                                    // TODO: Another chunk that doesn't do anything but should
                                    var tmpQuest = new WS_QuestInfo(questID);
                                }

                                QuestgiverStatusFlag status = qMenu.Icons[0];
                                if (status == QuestgiverStatusFlag.DIALOG_STATUS_INCOMPLETE)
                                {
                                    for (int i = 0, loopTo = QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
                                    {
                                        if (objCharacter.TalkQuests[i] is object && objCharacter.TalkQuests[i].ID == questID)
                                        {
                                            // Load quest data
                                            objCharacter.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                                            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, cGuid, ref objCharacter.TalkQuests[i]);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    objCharacter.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                                    WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestDetails(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, cGuid, true);
                                }
                            }
                            else // There were more than one quest to list
                            {
                                WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMenu(ref objCharacter, cGuid, "I have some tasks for you, $N.", qMenu);
                            }
                        }
                        else // We have to list both gossip options and quests
                        {
                            objCharacter.SendGossip(cGuid, textID, ref npcMenu, ref qMenu);
                        }
                    }
                    else
                    {
                        QuestMenu argqMenu4 = null;
                        objCharacter.SendGossip(cGuid, textID, ref npcMenu, qMenu: ref argqMenu4);
                    }
                }
                catch (Exception ex)
                {
                    // Stop
                }
            }

            /// <summary>
            /// Called when [gossip select].
            /// </summary>
            /// <param name="objCharacter">The objCharacter.</param>
            /// <param name="cGUID">The objCharacter GUID.</param>
            /// <param name="selected">The selected.</param>
            /// <returns></returns>
            public override void OnGossipSelect(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int selected)
            {
                switch (objCharacter.TalkMenuTypes[selected])
                {
                    case var @case when Operators.ConditionalCompareObjectEqual(@case, Gossip_Option.GOSSIP_OPTION_SPIRITHEALER, false):
                        {
                            if (objCharacter.DEAD == true)
                            {
                                var response = new Packets.PacketClass(OPCODES.SMSG_SPIRIT_HEALER_CONFIRM);
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

                            break;
                        }

                    case var case1 when Operators.ConditionalCompareObjectEqual(case1, Gossip_Option.GOSSIP_OPTION_VENDOR, false):
                    case var case2 when Operators.ConditionalCompareObjectEqual(case2, Gossip_Option.GOSSIP_OPTION_ARMORER, false):
                    case var case3 when Operators.ConditionalCompareObjectEqual(case3, Gossip_Option.GOSSIP_OPTION_STABLEPET, false):
                        {
                            WorldServiceLocator._WS_NPCs.SendListInventory(ref objCharacter, cGUID);
                            break;
                        }

                    case var case4 when Operators.ConditionalCompareObjectEqual(case4, Gossip_Option.GOSSIP_OPTION_TRAINER, false):
                        {
                            WorldServiceLocator._WS_NPCs.SendTrainerList(ref objCharacter, cGUID);
                            break;
                        }

                    case var case5 when Operators.ConditionalCompareObjectEqual(case5, Gossip_Option.GOSSIP_OPTION_TAXIVENDOR, false):
                        {
                            WorldServiceLocator._WS_Handlers_Taxi.SendTaxiMenu(ref objCharacter, cGUID);
                            break;
                        }

                    case var case6 when Operators.ConditionalCompareObjectEqual(case6, Gossip_Option.GOSSIP_OPTION_INNKEEPER, false):
                        {
                            WorldServiceLocator._WS_NPCs.SendBindPointConfirm(ref objCharacter, cGUID);
                            break;
                        }

                    case var case7 when Operators.ConditionalCompareObjectEqual(case7, Gossip_Option.GOSSIP_OPTION_BANKER, false):
                        {
                            WorldServiceLocator._WS_NPCs.SendShowBank(ref objCharacter, cGUID);
                            break;
                        }

                    case var case8 when Operators.ConditionalCompareObjectEqual(case8, Gossip_Option.GOSSIP_OPTION_ARENACHARTER, false):
                        {
                            WorldServiceLocator._WS_Guilds.SendPetitionActivate(ref objCharacter, cGUID);
                            break;
                        }

                    case var case9 when Operators.ConditionalCompareObjectEqual(case9, Gossip_Option.GOSSIP_OPTION_TABARDVENDOR, false):
                        {
                            WorldServiceLocator._WS_Guilds.SendTabardActivate(ref objCharacter, cGUID);
                            break;
                        }

                    case var case10 when Operators.ConditionalCompareObjectEqual(case10, Gossip_Option.GOSSIP_OPTION_AUCTIONEER, false):
                        {
                            WorldServiceLocator._WS_Auction.SendShowAuction(ref objCharacter, cGUID);
                            break;
                        }

                    case var case11 when Operators.ConditionalCompareObjectEqual(case11, Gossip_Option.GOSSIP_OPTION_TALENTWIPE, false):
                        {
                            WorldServiceLocator._WS_NPCs.SendTalentWipeConfirm(ref objCharacter, 0);
                            break;
                        }

                    case var case12 when Operators.ConditionalCompareObjectEqual(case12, Gossip_Option.GOSSIP_OPTION_GOSSIP, false):
                        {
                            objCharacter.SendTalking(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].NPCTextID);
                            break;
                        }

                    case var case13 when Operators.ConditionalCompareObjectEqual(case13, Gossip_Option.GOSSIP_OPTION_QUESTGIVER, false):
                        {
                            // NOTE: This may stay unused
                            var qMenu = WorldServiceLocator._WorldServer.ALLQUESTS.GetQuestMenu(ref objCharacter, cGUID);
                            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMenu(ref objCharacter, cGUID, "I have some tasks for you, $N.", qMenu);
                            break;
                        }
                }
                // 'c.SendGossipComplete()
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}