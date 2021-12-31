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
using Mangos.Common.Enums.Group;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;

namespace Mangos.World.Loots;

public partial class WS_Loot
{
    public class LootObject : IDisposable
    {
        public ulong GUID;

        public List<LootItem> Items;

        public int Money;

        public LootType LootType;

        public ulong LootOwner;

        public Dictionary<int, GroupLootInfo> GroupLootInfo;

        private bool _disposedValue;

        public bool IsEmpty
        {
            get
            {
                if (Money != 0)
                {
                    return false;
                }
                checked
                {
                    for (var i = 0; i <= Items.Count - 1; i++)
                    {
                        if (Items[i] != null)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        public LootObject(ulong GUID_, LootType LootType_)
        {
            GUID = 0uL;
            Items = new List<LootItem>();
            Money = 0;
            LootType = LootType.LOOTTYPE_CORPSE;
            LootOwner = 0uL;
            GroupLootInfo = new Dictionary<int, GroupLootInfo>(0);
            WorldServiceLocator._WS_Loot.LootTable[GUID_] = this;
            LootType = LootType_;
            GUID = GUID_;
        }

        public void SendLoot(ref WS_Network.ClientClass client)
        {
            if (Items.Count == 0)
            {
                WorldServiceLocator._WS_Loot.SendEmptyLoot(GUID, LootType, ref client);
                return;
            }
            if (decimal.Compare(new decimal(LootOwner), 0m) != 0 && client.Character.GUID != LootOwner)
            {
                Packets.PacketClass notMy = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                notMy.AddInt8(58);
                notMy.AddUInt64(0uL);
                notMy.AddUInt64(0uL);
                notMy.AddInt8(0);
                client.Send(ref notMy);
                notMy.Dispose();
                return;
            }
            Packets.PacketClass response = new(Opcodes.SMSG_LOOT_RESPONSE);
            response.AddUInt64(GUID);
            response.AddInt8((byte)LootType);
            response.AddInt32(Money);
            byte i;
            byte b2;
            checked
            {
                response.AddInt8((byte)Items.Count);
                var b = (byte)(Items.Count - 1);
                byte j = 0;
                while (j <= (uint)b)
                {
                    switch (Items[j])
                    {
                        case null:
                            response.AddInt8(j);
                            response.AddInt32(0);
                            response.AddInt32(0);
                            response.AddInt32(0);
                            response.AddUInt64(0uL);
                            response.AddInt8(0);
                            break;
                        default:
                            response.AddInt8(j);
                            response.AddInt32(Items[j].ItemID);
                            response.AddInt32(Items[j].ItemCount);
                            response.AddInt32(Items[j].ItemModel);
                            response.AddUInt64(0uL);
                            if (client.Character.IsInGroup && client.Character.Group.LootMethod == GroupLootMethod.LOOT_MASTER && client.Character.Group.LocalLootMaster != null && client.Character.Group.LocalLootMaster != client.Character)
                            {
                                response.AddInt8(2);
                            }
                            else
                            {
                                response.AddInt8(0);
                            }
                            break;
                    }
                    j = (byte)unchecked((uint)(j + 1));
                }
                client.Send(ref response);
                response.Dispose();
                client.Character.lootGUID = GUID;
                if (!client.Character.IsInGroup || !((client.Character.Group.LootMethod == GroupLootMethod.LOOT_NEED_BEFORE_GREED) || (client.Character.Group.LootMethod == GroupLootMethod.LOOT_GROUP)))
                {
                    return;
                }
                b2 = (byte)(Items.Count - 1);
                i = 0;
            }
            while (i <= (uint)b2)
            {
                if (Items[i] != null && WorldServiceLocator._WorldServer.ITEMDatabase[Items[i].ItemID].Quality >= (int)client.Character.Group.LootThreshold)
                {
                    GroupLootInfo[i] = new GroupLootInfo
                    {
                        LootObject = this,
                        LootSlot = i,
                        Item = Items[i]
                    };
                    WorldServiceLocator._WS_Loot.StartRoll(GUID, i, ref client.Character);
                    break;
                }
                checked
                {
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
        }

        public void GetLoot(ref WS_Network.ClientClass client, byte Slot)
        {
            try
            {
                if (Items[Slot] == null)
                {
                    Packets.PacketClass response4 = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    response4.AddInt8(49);
                    response4.AddUInt64(0uL);
                    response4.AddUInt64(0uL);
                    response4.AddInt8(0);
                    client.Send(ref response4);
                    response4.Dispose();
                    return;
                }
                if (GroupLootInfo.ContainsKey(Slot))
                {
                    Packets.PacketClass response3 = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    response3.AddInt8(58);
                    response3.AddUInt64(0uL);
                    response3.AddUInt64(0uL);
                    response3.AddInt8(0);
                    client.Send(ref response3);
                    response3.Dispose();
                    return;
                }
                ItemObject itemObject = new(Items[Slot].ItemID, client.Character.GUID)
                {
                    StackCount = Items[Slot].ItemCount
                };
                var tmpItem = itemObject;
                if (client.Character.ItemADD(ref tmpItem))
                {
                    if (tmpItem.ItemInfo.Bonding == 1)
                    {
                        var itemObject2 = tmpItem;
                        WS_Network.ClientClass client2 = null;
                        itemObject2.SoulbindItem(client2);
                    }
                    Packets.PacketClass response2 = new(Opcodes.SMSG_LOOT_REMOVED);
                    response2.AddInt8(Slot);
                    client.Send(ref response2);
                    response2.Dispose();
                    client.Character.LogLootItem(tmpItem, Items[Slot].ItemCount, Recieved: false, Created: false);
                    Items[Slot].Dispose();
                    Items[Slot] = null;
                    if (LootType == LootType.LOOTTYPE_FISHING && IsEmpty)
                    {
                        SendRelease(ref client);
                        Dispose();
                    }
                }
                else
                {
                    tmpItem.Delete();
                    Packets.PacketClass response = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    response.AddInt8(50);
                    response.AddUInt64(0uL);
                    response.AddUInt64(0uL);
                    response.AddInt8(0);
                    client.Send(ref response);
                    response.Dispose();
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Error getting loot.{0}", Environment.NewLine + e);
            }
        }

        public void SendRelease(ref WS_Network.ClientClass client)
        {
            Packets.PacketClass responseRelease = new(Opcodes.SMSG_LOOT_RELEASE_RESPONSE);
            responseRelease.AddUInt64(GUID);
            responseRelease.AddInt8(1);
            client.Send(ref responseRelease);
            responseRelease.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                WorldServiceLocator._WS_Loot.LootTable.Remove(GUID);
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Loot destroyed.");
            }
            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            //ILSpy generated this explicit interface implementation from .override directive in Dispose
            Dispose();
        }
    }
}
