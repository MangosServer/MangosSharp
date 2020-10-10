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
using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Loots
{
    public class WS_Loot
    {
        public LootStore LootTemplates_Creature; // DONE!
        public LootStore LootTemplates_Disenchant;
        public LootStore LootTemplates_Fishing; // DONE!
        public LootStore LootTemplates_Gameobject; // DONE!
        public LootStore LootTemplates_Item; // DONE!
        public LootStore LootTemplates_Pickpocketing; // DONE!
        public LootStore LootTemplates_QuestMail;
        public LootStore LootTemplates_Reference; // DONE!
        public LootStore LootTemplates_Skinning; // DONE!
        public Dictionary<ulong, LootObject> LootTable = new Dictionary<ulong, LootObject>();
        public Dictionary<int, TLock> Locks = new Dictionary<int, TLock>();

        public class TLock
        {
            public byte[] KeyType = new byte[5];
            public int[] Keys = new int[5];
            public short RequiredMiningSkill;
            public short RequiredLockingSkill;

            public TLock(byte[] KeyType_, int[] Keys_, short ReqMining, short ReqLock)
            {
                for (byte i = 0; i <= 4; i++)
                {
                    KeyType[i] = KeyType_[i];
                    Keys[i] = Keys_[i];
                }

                RequiredMiningSkill = ReqMining;
                RequiredLockingSkill = ReqLock;
            }
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class LootItem : IDisposable
        {
            public int ItemID = 0;
            public byte ItemCount = 0;

            public int ItemModel
            {
                get
                {
                    if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(ItemID))
                    {
                        // TODO: Another one of these useless bits of code, needs to be implemented correctly
                        var tmpItem = new WS_Items.ItemInfo(ItemID);
                        try
                        {
                            WorldServiceLocator._WorldServer.ITEMDatabase.Remove(ItemID);
                        }
                        catch (Exception)
                        {
                        }

                        WorldServiceLocator._WorldServer.ITEMDatabase.Add(ItemID, tmpItem);
                    }

                    return WorldServiceLocator._WorldServer.ITEMDatabase[ItemID].Model;
                }
            }

            public LootItem(ref LootStoreItem Item)
            {
                ItemID = Item.ItemID;
                ItemCount = (byte)WorldServiceLocator._WorldServer.Rnd.Next(Item.MinCountOrRef, Item.MaxCount + 1);
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                }

                _disposedValue = true;
            }

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class LootStoreItem
        {
            public int ItemID = 0;
            public float Chance = 0.0f;
            public byte Group = 0;
            public int MinCountOrRef = 0;
            public byte MaxCount = 0;
            public ConditionType LootCondition = ConditionType.CONDITION_NONE;
            // Public ConditionValue1 As Integer = 0
            // Public ConditionValue2 As Integer = 0
            public bool NeedQuest = false;

            public LootStoreItem(int Item, float Chance, byte Group, int MinCountOrRef, byte MaxCount, ConditionType LootCondition, bool NeedQuest)
            {
                ItemID = Item;
                this.Chance = Chance;
                this.Group = Group;
                this.MinCountOrRef = MinCountOrRef;
                this.MaxCount = MaxCount;
                this.LootCondition = LootCondition;
                // Me.ConditionValue1 = ConditionValue1
                // Me.ConditionValue2 = ConditionValue2
                this.NeedQuest = NeedQuest;
            }

            public bool Roll()
            {
                if (Chance >= 100.0f)
                    return true;
                return WorldServiceLocator._Functions.RollChance(Chance);
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class LootGroup
        {
            public List<LootStoreItem> ExplicitlyChanced = new List<LootStoreItem>();
            public List<LootStoreItem> EqualChanced = new List<LootStoreItem>();

            public LootGroup()
            {
            }

            public void AddItem(ref LootStoreItem Item)
            {
                if (Item.Chance != 0.0f)
                {
                    ExplicitlyChanced.Add(Item);
                }
                else
                {
                    EqualChanced.Add(Item);
                }
            }

            public LootStoreItem Roll()
            {
                if (ExplicitlyChanced.Count > 0)
                {
                    float rollChance = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 100.0d);
                    for (int i = 0, loopTo = ExplicitlyChanced.Count - 1; i <= loopTo; i++)
                    {
                        if (ExplicitlyChanced[i].Chance >= 100.0f)
                            return ExplicitlyChanced[i];
                        rollChance -= ExplicitlyChanced[i].Chance;
                        if (rollChance <= 0.0f)
                            return ExplicitlyChanced[i];
                    }
                }

                if (EqualChanced.Count > 0)
                {
                    return EqualChanced[WorldServiceLocator._WorldServer.Rnd.Next(0, EqualChanced.Count)];
                }

                return null;
            }

            public void Process(ref LootObject Loot)
            {
                var Item = Roll();
                if (Item is object)
                    Loot.Items.Add(new LootItem(ref Item));
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class LootObject : IDisposable
        {
            public LootObject()
            {
                LootType = this.LootType.LOOTTYPE_CORPSE;
            }

            public LootObject(ulong GUID_, LootType LootType_)
            {
                LootType = this.LootType.LOOTTYPE_CORPSE;
                WorldServiceLocator._WS_Loot.LootTable[GUID_] = this;
                LootType = LootType_;
                GUID = GUID_;
            }

            public ulong GUID = 0UL;
            public List<LootItem> Items = new List<LootItem>();
            public int Money = 0;
            public LootType LootType;
            public ulong LootOwner = 0UL;
            public Dictionary<int, GroupLootInfo> GroupLootInfo = new Dictionary<int, GroupLootInfo>(0);

            public void SendLoot(ref WS_Network.ClientClass client)
            {
                if (Items.Count == 0)
                {
                    WorldServiceLocator._WS_Loot.SendEmptyLoot(GUID, LootType, ref client);
                    return;
                }

                if (LootOwner != 0m && client.Character.GUID != LootOwner)
                {
                    // DONE: Loot owning!
                    var notMy = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                    notMy.AddInt8((byte)InventoryChangeFailure.EQUIP_ERR_OBJECT_IS_BUSY);
                    notMy.AddUInt64(0UL);
                    notMy.AddUInt64(0UL);
                    notMy.AddInt8(0);
                    client.Send(notMy);
                    notMy.Dispose();
                    return;
                }

                var response = new Packets.PacketClass(OPCODES.SMSG_LOOT_RESPONSE);
                response.AddUInt64(GUID);
                response.AddInt8((byte)LootType);
                response.AddInt32(Money);
                response.AddInt8((byte)Items.Count);
                for (byte i = 0, loopTo = (byte)(Items.Count - 1); i <= loopTo; i++)
                {
                    if (Items[i] is null)
                    {
                        response.AddInt8(i);
                        response.AddInt32(0);
                        response.AddInt32(0);
                        response.AddInt32(0);
                        response.AddUInt64(0UL);
                        response.AddInt8(0);
                    }
                    else
                    {
                        response.AddInt8(i);
                        response.AddInt32(Items[i].ItemID);
                        response.AddInt32(Items[i].ItemCount);
                        response.AddInt32(Items[i].ItemModel);
                        response.AddUInt64(0UL);
                        if (client.Character.IsInGroup && client.Character.Group.LootMethod == GroupLootMethod.LOOT_MASTER && client.Character.Group.LocalLootMaster is object && !ReferenceEquals(client.Character.Group.LocalLootMaster, client.Character))
                        {
                            response.AddInt8(2); // Unlootable?
                        }
                        else
                        {
                            response.AddInt8(0);
                        } // 1: Message "Still rolled for."
                    }
                }

                client.Send(response);
                response.Dispose();
                client.Character.lootGUID = GUID;
                if (client.Character.IsInGroup)
                {
                    if (client.Character.Group.LootMethod == GroupLootMethod.LOOT_NEED_BEFORE_GREED | client.Character.Group.LootMethod == GroupLootMethod.LOOT_GROUP)
                    {

                        // DONE: Check threshold if in group
                        for (byte i = 0, loopTo1 = (byte)(Items.Count - 1); i <= loopTo1; i++)
                        {
                            if (Items[i] is object)
                            {
                                if (WorldServiceLocator._WorldServer.ITEMDatabase[Items[i].ItemID].Quality >= client.Character.Group.LootThreshold)
                                {
                                    GroupLootInfo[i] = new GroupLootInfo();
                                    GroupLootInfo[i].LootObject = this;
                                    GroupLootInfo[i].LootSlot = i;
                                    GroupLootInfo[i].Item = Items[i];
                                    WorldServiceLocator._WS_Loot.StartRoll(GUID, i, ref client.Character);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            public void GetLoot(ref WS_Network.ClientClass client, byte Slot)
            {
                try
                {
                    if (Items[Slot] is null)
                    {
                        var response = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                        response.AddInt8((byte)InventoryChangeFailure.EQUIP_ERR_ALREADY_LOOTED);
                        response.AddUInt64(0UL);
                        response.AddUInt64(0UL);
                        response.AddInt8(0);
                        client.Send(response);
                        response.Dispose();
                        return;
                    }

                    if (GroupLootInfo.ContainsKey(Slot))
                    {
                        var response = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                        response.AddInt8((byte)InventoryChangeFailure.EQUIP_ERR_OBJECT_IS_BUSY);
                        response.AddUInt64(0UL);
                        response.AddUInt64(0UL);
                        response.AddInt8(0);
                        client.Send(response);
                        response.Dispose();
                        return;
                    }

                    var tmpItem = new ItemObject(Items[Slot].ItemID, client.Character.GUID) { StackCount = Items[Slot].ItemCount };
                    if (client.Character.ItemADD(ref tmpItem))
                    {
                        // DONE: Bind item to player
                        if (tmpItem.ItemInfo.Bonding == ITEM_BONDING_TYPE.BIND_WHEN_PICKED_UP)
                        {
                            WS_Network.ClientClass argclient = null;
                            tmpItem.SoulbindItem(client: ref argclient);
                        }

                        // TODO: If other players is looting the same object remove it for them as well.

                        var response = new Packets.PacketClass(OPCODES.SMSG_LOOT_REMOVED);
                        response.AddInt8(Slot);
                        client.Send(response);
                        response.Dispose();
                        client.Character.LogLootItem(tmpItem, Items[Slot].ItemCount, false, false);
                        Items[Slot].Dispose();
                        Items[Slot] = null;
                        if (LootType == this.LootType.LOOTTYPE_FISHING && IsEmpty)
                        {
                            SendRelease(ref client);
                            Dispose();
                        }
                    }
                    else
                    {
                        tmpItem.Delete();
                        var response = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                        response.AddInt8((byte)InventoryChangeFailure.EQUIP_ERR_INVENTORY_FULL);
                        response.AddUInt64(0UL);
                        response.AddUInt64(0UL);
                        response.AddInt8(0);
                        client.Send(response);
                        response.Dispose();
                    }
                }
                catch (Exception e)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error getting loot.{0}", Environment.NewLine + e.ToString());
                }
            }

            public void SendRelease(ref WS_Network.ClientClass client)
            {
                var responseRelease = new Packets.PacketClass(OPCODES.SMSG_LOOT_RELEASE_RESPONSE);
                responseRelease.AddUInt64(GUID);
                responseRelease.AddInt8(1);
                client.Send(responseRelease);
                responseRelease.Dispose();
            }

            public bool IsEmpty
            {
                get
                {
                    if (Money != 0)
                        return false;
                    for (int i = 0, loopTo = Items.Count - 1; i <= loopTo; i++)
                    {
                        if (Items[i] is object)
                            return false;
                    }

                    return true;
                    // Return ((Items.Count = 0) And (Money = 0))
                }
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    WorldServiceLocator._WS_Loot.LootTable.Remove(GUID);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Loot destroyed.");
                }

                _disposedValue = true;
            }

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class LootTemplate
        {
            public List<LootStoreItem> Items = new List<LootStoreItem>();
            public Dictionary<byte, LootGroup> Groups = new Dictionary<byte, LootGroup>();

            public LootTemplate()
            {
            }

            public void AddItem(ref LootStoreItem Item)
            {
                if (Item.Group > 0 && Item.MinCountOrRef > 0)
                {
                    if (Groups.ContainsKey(Item.Group) == false)
                        Groups.Add(Item.Group, new LootGroup());
                    Groups[Item.Group].AddItem(ref Item);
                }
                else
                {
                    Items.Add(Item);
                }
            }

            public void Process(ref LootObject Loot, byte GroupID)
            {
                if (GroupID > 0)
                {
                    if (Groups.ContainsKey(GroupID) == false)
                        return;
                    Groups[GroupID].Process(ref Loot);
                    return;
                }

                // Go through all items
                for (int i = 0, loopTo = Items.Count - 1; i <= loopTo; i++)
                {
                    if (Items[i].Roll() == false)
                        continue; // Bad luck
                    if (Items[i].MinCountOrRef < 0) // Loot Template ID
                    {
                        var Referenced = WorldServiceLocator._WS_Loot.LootTemplates_Reference.GetLoot(-Items[i].MinCountOrRef);
                        if (Referenced is null)
                            continue;
                        for (int j = 1, loopTo1 = Items[i].MaxCount; j <= loopTo1; j++)
                            Referenced.Process(ref Loot, Items[i].Group);
                    }
                    else // Normal Item
                    {
                        var tmp = Items;
                        var argItem = tmp[i];
                        Loot.Items.Add(new LootItem(ref argItem));
                        tmp[i] = argItem;
                    }
                }

                // Go through all loot groups
                foreach (KeyValuePair<byte, LootGroup> Group in Groups)
                    Group.Value.Process(ref Loot);
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class LootStore
        {
            private readonly string Name;
            private readonly Dictionary<int, LootTemplate> Templates = new Dictionary<int, LootTemplate>();

            public LootStore(string Name)
            {
                this.Name = Name;
            }

            public LootTemplate GetLoot(int Entry)
            {
                if (Templates.ContainsKey(Entry))
                {
                    return Templates[Entry];
                }
                else
                {
                    return CreateTemplate(Entry);
                }
            }

            private LootTemplate CreateTemplate(int Entry)
            {
                var newTemplate = new LootTemplate();
                Templates.Add(Entry, newTemplate);
                var MysqlQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM {0} WHERE entry = {1};", Name, (object)Entry), MysqlQuery);
                if (MysqlQuery.Rows.Count == 0)
                {
                    Templates[Entry] = null;
                    return null; // No results found
                }

                foreach (DataRow LootRow in MysqlQuery.Rows)
                {
                    int Item = Conversions.ToInteger(LootRow["item"]);
                    float ChanceOrQuestChance = Conversions.ToSingle(LootRow["ChanceOrQuestChance"]);
                    byte GroupID = Conversions.ToByte(LootRow["groupid"]);
                    int MinCountOrRef = Conversions.ToInteger(LootRow["mincountOrRef"]);
                    byte MaxCount = Conversions.ToByte(LootRow["maxcount"]);
                    ConditionType LootCondition = (ConditionType)LootRow["condition_id"];
                    // Dim ConditionValue1 As Integer = LootRow.Item("condition_value1")
                    // Dim ConditionValue2 As Integer = LootRow.Item("condition_value2")

                    var newItem = new LootStoreItem(Item, Math.Abs(ChanceOrQuestChance), GroupID, MinCountOrRef, MaxCount, LootCondition, ChanceOrQuestChance < 0.0f);
                    newTemplate.AddItem(ref newItem);
                }

                return newTemplate;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class GroupLootInfo
        {
            public LootObject LootObject;
            public byte LootSlot;
            public LootItem Item;
            public List<WS_PlayerData.CharacterObject> Rolls = new List<WS_PlayerData.CharacterObject>();
            public Dictionary<WS_PlayerData.CharacterObject, int> Looters = new Dictionary<WS_PlayerData.CharacterObject, int>(5);
            public Timer RollTimeoutTimer = null;

            public void Check()
            {
                if (Looters.Count == Rolls.Count)
                {
                    // DONE: End loot
                    byte maxRollType = 0;
                    foreach (KeyValuePair<WS_PlayerData.CharacterObject, int> looter in Looters)
                    {
                        if (looter.Value == 1)
                            maxRollType = 1;
                        if (looter.Value == 2 && maxRollType != 1)
                            maxRollType = 2;
                    }

                    if (maxRollType == 0)
                    {
                        LootObject.GroupLootInfo.Remove(LootSlot);
                        var response = new Packets.PacketClass(OPCODES.SMSG_LOOT_ALL_PASSED);
                        response.AddUInt64(LootObject.GUID);
                        response.AddInt32(LootSlot);
                        response.AddInt32(Item.ItemID);
                        response.AddInt32(0);
                        response.AddInt32(0);
                        Broadcast(ref response);
                        response.Dispose();
                        return;
                    }

                    int maxRoll = -1;
                    WS_PlayerData.CharacterObject looterCharacter = null;
                    foreach (KeyValuePair<WS_PlayerData.CharacterObject, int> looter in Looters)
                    {
                        if (looter.Value == maxRollType)
                        {
                            byte rollValue = (byte)WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
                            if (rollValue > maxRoll)
                            {
                                maxRoll = rollValue;
                                looterCharacter = looter.Key;
                            }

                            var response = new Packets.PacketClass(OPCODES.SMSG_LOOT_ROLL);
                            response.AddUInt64(LootObject.GUID);
                            response.AddInt32(LootSlot);
                            response.AddUInt64(looter.Key.GUID);
                            response.AddInt32(Item.ItemID);
                            response.AddInt32(0);
                            response.AddInt32(0);
                            response.AddInt8(rollValue);
                            response.AddInt8((byte)looter.Value);
                            Broadcast(ref response);
                            response.Dispose();
                        }
                    }

                    var tmpItem = new ItemObject(Item.ItemID, looterCharacter.GUID) { StackCount = Item.ItemCount };
                    var wonItem = new Packets.PacketClass(OPCODES.SMSG_LOOT_ROLL_WON);
                    wonItem.AddUInt64(LootObject.GUID);
                    wonItem.AddInt32(LootSlot);
                    wonItem.AddInt32(Item.ItemID);
                    wonItem.AddInt32(0);
                    wonItem.AddInt32(0);
                    wonItem.AddUInt64(looterCharacter.GUID);
                    wonItem.AddInt8((byte)maxRoll);
                    wonItem.AddInt8(maxRollType);
                    Broadcast(ref wonItem);
                    if (looterCharacter.ItemADD(ref tmpItem))
                    {
                        looterCharacter.LogLootItem(tmpItem, Item.ItemCount, false, false);
                        LootObject.GroupLootInfo.Remove(LootSlot);
                        LootObject.Items[LootSlot] = null;
                    }
                    else
                    {
                        tmpItem.Delete();
                        LootObject.GroupLootInfo.Remove(LootSlot);
                    }
                }
            }

            public void Broadcast(ref Packets.PacketClass packet)
            {
                foreach (WS_PlayerData.CharacterObject objCharacter in Rolls)
                    objCharacter.client.SendMultiplyPackets(ref packet);
            }

            public void EndRoll(object state)
            {
                foreach (WS_PlayerData.CharacterObject objCharacter in Rolls)
                {
                    if (!Looters.ContainsKey(objCharacter))
                    {
                        Looters[objCharacter] = 0;

                        // DONE: Send roll info
                        var response = new Packets.PacketClass(OPCODES.SMSG_LOOT_ROLL);
                        response.AddUInt64(LootObject.GUID);
                        response.AddInt32(LootSlot);
                        response.AddUInt64(objCharacter.GUID);
                        response.AddInt32(Item.ItemID);
                        response.AddInt32(0);
                        response.AddInt32(0);
                        response.AddInt8(249);
                        response.AddInt8(0);
                        Broadcast(ref response);
                    }
                }

                RollTimeoutTimer.Dispose();
                RollTimeoutTimer = null;
                Check();
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void On_CMSG_AUTOSTORE_LOOT_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            try
            {
                packet.GetInt16();
                byte slot = packet.GetInt8();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOSTORE_LOOT_ITEM [slot={2}]", client.IP, client.Port, slot);
                if (LootTable.ContainsKey(client.Character.lootGUID))
                {
                    LootTable[client.Character.lootGUID].GetLoot(ref client, slot);
                }
                else
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                    response.AddInt8((byte)InventoryChangeFailure.EQUIP_ERR_ALREADY_LOOTED);
                    response.AddUInt64(0UL);
                    response.AddUInt64(0UL);
                    response.AddInt8(0);
                    client.Send(response);
                    response.Dispose();
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error looting item.{0}", Environment.NewLine + e.ToString());
            }
        }

        public void On_CMSG_LOOT_MONEY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_MONEY", client.IP, client.Port);
            if (!LootTable.ContainsKey(client.Character.lootGUID))
                return;
            if (client.Character.IsInGroup)
            {
                // DONE: Party share
                var members = WorldServiceLocator._WS_Spells.GetPartyMembersAroundMe(ref client.Character, 100f);
                int copper = LootTable[client.Character.lootGUID].Money / members.Count + 1;
                LootTable[client.Character.lootGUID].Money = 0;
                var sharePcket = new Packets.PacketClass(OPCODES.SMSG_LOOT_MONEY_NOTIFY);
                sharePcket.AddInt32(copper);
                foreach (WS_PlayerData.CharacterObject character in members)
                {
                    character.client.SendMultiplyPackets(ref sharePcket);
                    character.Copper = (uint)(character.Copper + copper);
                    character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, character.Copper);
                    character.SaveCharacter();
                }

                client.SendMultiplyPackets(ref sharePcket);
                client.Character.Copper = (uint)(client.Character.Copper + copper);
                sharePcket.Dispose();
            }
            else
            {
                // DONE: Not in party
                int copper = LootTable[client.Character.lootGUID].Money;
                client.Character.Copper = (uint)(client.Character.Copper + copper);
                LootTable[client.Character.lootGUID].Money = 0;
                var lootPacket = new Packets.PacketClass(OPCODES.SMSG_LOOT_MONEY_NOTIFY);
                lootPacket.AddInt32(copper);
                client.Send(lootPacket);
                lootPacket.Dispose();
            }

            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
            client.Character.SendCharacterUpdate(false);
            client.Character.SaveCharacter();

            // TODO: Send to party loooters
            var response2 = new Packets.PacketClass(OPCODES.SMSG_LOOT_CLEAR_MONEY);
            client.SendMultiplyPackets(ref response2);
            // Client.Character.SendToNearPlayers(response2)
            response2.Dispose();
        }

        public void On_CMSG_LOOT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT [GUID={2:X}]", client.IP, client.Port, GUID);

            // DONE: Make sure other players sees that you're looting
            client.Character.cUnitFlags = client.Character.cUnitFlags | UnitFlags.UNIT_FLAG_LOOTING;
            client.Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, client.Character.cUnitFlags);
            client.Character.SendCharacterUpdate();
            if (LootTable.ContainsKey(GUID))
            {
                LootTable[GUID].SendLoot(ref client);
            }
            else
            {
                SendEmptyLoot(GUID, LootType.LOOTTYPE_CORPSE, ref client);
            }
        }

        public void On_CMSG_LOOT_RELEASE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_RELEASE [lootGUID={2:X}]", client.IP, client.Port, GUID);
            if (client.Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL] is object)
            {
                client.Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL].State = SpellCastState.SPELL_STATE_IDLE;
            }

            // DONE: Remove looting for other players
            client.Character.cUnitFlags = client.Character.cUnitFlags & !UnitFlags.UNIT_FLAG_LOOTING;
            client.Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, client.Character.cUnitFlags);
            client.Character.SendCharacterUpdate();
            if (LootTable.ContainsKey(GUID))
            {
                LootTable[GUID].SendRelease(ref client);

                // DONE: Remove loot owner
                LootTable[GUID].LootOwner = 0UL;
                var LootType = LootTable[GUID].LootType;
                if (LootTable[GUID].IsEmpty)
                {
                    // DONE: Delete loot
                    LootTable[GUID].Dispose();

                    // DONE: Remove loot sing for player
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
                    {
                        if (LootType == LootType.LOOTTYPE_CORPSE)
                        {
                            // DONE: Set skinnable
                            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.SkinLootID > 0)
                            {
                                WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags = WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags | UnitFlags.UNIT_FLAG_SKINNABLE;
                            }

                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags = 0;
                            var response = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                            response.AddInt32(1);
                            response.AddInt8(0);
                            var UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                            UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_DYNAMIC_FLAGS, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags);
                            UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags);
                            UpdateData.AddToPacket(response, ObjectUpdateType.UPDATETYPE_VALUES, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID]);
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SendToNearPlayers(ref response);
                            response.Dispose();
                            UpdateData.Dispose();
                        }
                        else if (LootType == LootType.LOOTTYPE_SKINNING)
                        {
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Despawn();
                        }
                    }
                    else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID))
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].IsConsumeable)
                        {
                            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State = GameObjectLootState.LOOT_LOOTED;
                            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].Despawn();
                        }
                        else
                        {
                            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State = GameObjectLootState.LOOT_UNLOOTED;
                        }
                    }
                    else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsItem(GUID))
                    {
                        client.Character.ItemREMOVE(GUID, true, true);
                    }
                }

                // DONE: Send loot for other players
                else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
                {
                    if (LootType == LootType.LOOTTYPE_CORPSE)
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID) == false)
                        {
                            LootTable[GUID].Dispose();
                        }
                        else
                        {
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags = (int)DynamicFlags.UNIT_DYNFLAG_LOOTABLE;
                            var response = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                            response.AddInt32(1);
                            response.AddInt8(0);
                            var UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                            UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_DYNAMIC_FLAGS, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags);
                            UpdateData.AddToPacket(response, ObjectUpdateType.UPDATETYPE_VALUES, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID]);
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SendToNearPlayers(ref response);
                            response.Dispose();
                            UpdateData.Dispose();
                        }
                    }
                    else if (LootType == LootType.LOOTTYPE_SKINNING)
                    {
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Despawn();
                    }
                }
                else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID))
                {
                    if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID) == false || LootTable[GUID].LootType == LootType.LOOTTYPE_FISHING)
                    {
                        LootTable[GUID].Dispose();
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State = GameObjectLootState.LOOT_UNLOOTED;
                        var response = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                        response.AddInt32(1);
                        response.AddInt8(0);
                        var UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                        UpdateData.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_STATE, 0, WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State);
                        UpdateData.AddToPacket(response, ObjectUpdateType.UPDATETYPE_VALUES, WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID]);
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].SendToNearPlayers(ref response);
                        response.Dispose();
                        UpdateData.Dispose();
                    }
                }
                else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsItem(GUID))
                {
                    LootTable[GUID].Dispose();
                    client.Character.ItemREMOVE(GUID, true, true);
                }
                else
                {
                    // DONE: In all other cases - delete the loot
                    LootTable[GUID].Dispose();
                }
            }
            else
            {
                var responseRelease = new Packets.PacketClass(OPCODES.SMSG_LOOT_RELEASE_RESPONSE);
                responseRelease.AddUInt64(GUID);
                responseRelease.AddInt8(1);
                client.Send(responseRelease);
                responseRelease.Dispose();
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
                {
                    // DONE: Set skinnable
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.SkinLootID > 0)
                    {
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags = WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags | UnitFlags.UNIT_FLAG_SKINNABLE;
                    }

                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags = 0;
                    var response = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                    response.AddInt32(1);
                    response.AddInt8(0);
                    var UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                    UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_DYNAMIC_FLAGS, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags);
                    UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags);
                    UpdateData.AddToPacket(response, ObjectUpdateType.UPDATETYPE_VALUES, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID]);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SendToNearPlayers(ref response);
                    response.Dispose();
                    UpdateData.Dispose();
                }
            }

            client.Character.lootGUID = 0UL;
        }

        public void SendEmptyLoot(ulong GUID, LootType LootType, ref WS_Network.ClientClass client)
        {
            var response = new Packets.PacketClass(OPCODES.SMSG_LOOT_RESPONSE);
            response.AddUInt64(GUID);
            response.AddInt8((byte)LootType);
            response.AddInt32(0);
            response.AddInt8(0);
            client.Send(response);
            response.Dispose();
            /* TODO ERROR: Skipped IfDirectiveTrivia */
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Empty loot for GUID [{2:X}].", client.IP, client.Port, GUID);
            /* TODO ERROR: Skipped EndIfDirectiveTrivia */
        }

        public void StartRoll(ulong LootGUID, byte Slot, ref WS_PlayerData.CharacterObject Character)
        {
            var rollCharacters = new List<WS_PlayerData.CharacterObject>() { Character };
            foreach (ulong GUID in Character.Group.LocalMembers)
            {
                if (Character.playersNear.Contains(GUID))
                    rollCharacters.Add(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
            }

            var startRoll = new Packets.PacketClass(OPCODES.SMSG_LOOT_START_ROLL);
            startRoll.AddUInt64(LootGUID);
            startRoll.AddInt32(Slot);
            startRoll.AddInt32(LootTable[LootGUID].GroupLootInfo[Slot].Item.ItemID);
            startRoll.AddInt32(0);
            startRoll.AddInt32(0);
            startRoll.AddInt32(60000);
            foreach (WS_PlayerData.CharacterObject objCharacter in rollCharacters)
                objCharacter.client.SendMultiplyPackets(ref startRoll);
            startRoll.Dispose();
            LootTable[LootGUID].GroupLootInfo[Slot].Rolls = rollCharacters;
            LootTable[LootGUID].GroupLootInfo[Slot].RollTimeoutTimer = new Timer(LootTable[LootGUID].GroupLootInfo[Slot].EndRoll, 0, 60000, Timeout.Infinite);
        }

        public void On_CMSG_LOOT_ROLL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 18)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            byte Slot = (byte)packet.GetInt32();
            byte rollType = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_ROLL [loot={2} roll={3}]", client.IP, client.Port, GUID, rollType);

            // 0 - Pass
            // 1 - Need
            // 2 - Greed

            // DONE: Send roll info
            var response = new Packets.PacketClass(OPCODES.SMSG_LOOT_ROLL);
            response.AddUInt64(GUID);
            response.AddInt32(Slot);
            response.AddUInt64(client.Character.GUID);
            response.AddInt32(LootTable[GUID].GroupLootInfo[Slot].Item.ItemID);
            response.AddInt32(0);
            response.AddInt32(0);

            // FIRST:  0: "Need for: [item name]" > 127: "you passed on: [item name]"
            // SECOND: 0: "Need for: [item name]" 0: "You have selected need for [item name] 1: need roll 2: greed roll
            switch (rollType)
            {
                case 0:
                    {
                        response.AddInt8(249);
                        response.AddInt8(0);
                        break;
                    }

                case 1:
                    {
                        response.AddInt8(0);
                        response.AddInt8(0);
                        break;
                    }

                case 2:
                    {
                        response.AddInt8(249);
                        response.AddInt8(2);
                        break;
                    }
            }

            LootTable[GUID].GroupLootInfo[Slot].Broadcast(ref response);
            response.Dispose();
            LootTable[GUID].GroupLootInfo[Slot].Looters[client.Character] = rollType;
            LootTable[GUID].GroupLootInfo[Slot].Check();
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}