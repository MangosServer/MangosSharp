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
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Handlers
{
    public class WS_Handlers_Trade
    {
        public class TTradeInfo : IDisposable
        {
            public int ID = 0;
            public WS_PlayerData.CharacterObject Trader = null;
            public WS_PlayerData.CharacterObject Target = null;
            public int[] TraderSlots = new int[] { -1, -1, -1, -1, -1, -1, -1 };
            public uint TraderGold = 0U;
            public bool TraderAccept = false;
            public int[] TargetSlots = new int[] { -1, -1, -1, -1, -1, -1, -1 };
            public uint TargetGold = 0U;
            public bool TargetAccept = false;

            public TTradeInfo(ref WS_PlayerData.CharacterObject Trader_, ref WS_PlayerData.CharacterObject Target_)
            {
                Trader = Trader_;
                Target = Target_;
                Trader.tradeInfo = this;
                Target.tradeInfo = this;
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
                    Trader.tradeInfo = null;
                    Target.tradeInfo = null;
                    Trader = null;
                    Target = null;
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
            public void SendTradeUpdateToTrader()
            {
                if (Trader is null)
                    return;
                var packet = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS_EXTENDED);
                try
                {
                    packet.AddInt8(1);               // giving(0x00) or receiving(0x01)
                    packet.AddInt32(7);              // Slots for Character 1
                    packet.AddInt32(7);              // Slots for Character 2
                    packet.AddUInt32(TargetGold);     // Gold
                    packet.AddInt32(0);
                    for (int i = 0; i <= 6; i++)
                    {
                        packet.AddInt8((byte)i);
                        if (TargetSlots[i] > 0)
                        {
                            byte mySlot = (byte)(TargetSlots[i] & 0xFF);
                            byte myBag = (byte)(TargetSlots[i] >> 8);
                            ItemObject myItem = null;
                            if (myBag == 0)
                                myItem = Target.Items[mySlot];
                            else
                                myItem = Target.Items[myBag].Items[mySlot];
                            packet.AddInt32(myItem.ItemEntry);
                            packet.AddInt32(myItem.ItemInfo.Model);
                            packet.AddInt32(myItem.StackCount);              // ITEM_FIELD_STACK_COUNT
                            packet.AddInt32(0);                              // Unk.. probably gift=1, created_by=0?
                            packet.AddUInt64(myItem.GiftCreatorGUID);        // ITEM_FIELD_GIFTCREATOR
                            if (myItem.Enchantments.ContainsKey((byte)EnchantSlots.ENCHANTMENT_PERM))
                            {
                                packet.AddInt32(myItem.Enchantments(EnchantSlots.ENCHANTMENT_PERM).ID);
                            }
                            else
                            {
                                packet.AddInt32(0);
                            }                          // ITEM_FIELD_ENCHANTMENT

                            packet.AddUInt64(myItem.CreatorGUID);            // ITEM_FIELD_CREATOR
                            packet.AddInt32(myItem.ChargesLeft);             // ITEM_FIELD_SPELL_CHARGES
                            packet.AddInt32(0);                              // ITEM_FIELD_PROPERTY_SEED
                            packet.AddInt32(myItem.RandomProperties);        // ITEM_FIELD_RANDOM_PROPERTIES_ID
                            packet.AddInt32(myItem.ItemInfo.Flags);          // ITEM_FIELD_FLAGS
                            packet.AddInt32(myItem.ItemInfo.Durability);     // ITEM_FIELD_MAXDURABILITY
                            packet.AddInt32(myItem.Durability);              // ITEM_FIELD_DURABILITY
                        }
                        else
                        {
                            int j;
                            for (j = 0; j <= 14; j++)
                                packet.AddInt32(0);
                        }
                    }

                    Trader.client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }

            public void SendTradeUpdateToTarget()
            {
                if (Target is null)
                    return;
                var packet = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS_EXTENDED);
                try
                {
                    packet.AddInt8(1);               // giving(0x00) or receiving(0x01)
                    packet.AddInt32(7);              // Slots for Character 1
                    packet.AddInt32(7);              // Slots for Character 2
                    packet.AddUInt32(TraderGold);     // Gold
                    packet.AddInt32(0);
                    for (int i = 0; i <= 6; i++)
                    {
                        packet.AddInt8((byte)i);
                        if (TraderSlots[i] > 0)
                        {
                            byte mySlot = (byte)(TraderSlots[i] & 0xFF);
                            byte myBag = (byte)(TraderSlots[i] >> 8);
                            ItemObject myItem = null;
                            if (myBag == 0)
                                myItem = Trader.Items[mySlot];
                            else
                                myItem = Trader.Items[myBag].Items[mySlot];
                            packet.AddInt32(myItem.ItemEntry);
                            packet.AddInt32(myItem.ItemInfo.Model);
                            packet.AddInt32(myItem.StackCount);              // ITEM_FIELD_STACK_COUNT
                            packet.AddInt32(0);                              // Unk.. probably gift=1, created_by=0?
                            packet.AddUInt64(myItem.GiftCreatorGUID);        // ITEM_FIELD_GIFTCREATOR
                            if (myItem.Enchantments.ContainsKey((byte)EnchantSlots.ENCHANTMENT_PERM))
                            {
                                packet.AddInt32(myItem.Enchantments(EnchantSlots.ENCHANTMENT_PERM).ID);
                            }
                            else
                            {
                                packet.AddInt32(0);
                            }                          // ITEM_FIELD_ENCHANTMENT

                            packet.AddUInt64(myItem.CreatorGUID);            // ITEM_FIELD_CREATOR
                            packet.AddInt32(myItem.ChargesLeft);             // ITEM_FIELD_SPELL_CHARGES
                            packet.AddInt32(0);                              // ITEM_FIELD_PROPERTY_SEED
                            packet.AddInt32(myItem.RandomProperties);        // ITEM_FIELD_RANDOM_PROPERTIES_ID
                            packet.AddInt32(myItem.ItemInfo.Flags);          // ITEM_FIELD_FLAGS
                            packet.AddInt32(myItem.ItemInfo.Durability);     // ITEM_FIELD_MAXDURABILITY
                            packet.AddInt32(myItem.Durability);              // ITEM_FIELD_DURABILITY
                        }
                        else
                        {
                            int j;
                            for (j = 0; j <= 14; j++)
                                packet.AddInt32(0);
                        }
                    }

                    Target.client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }

            public void DoTrade(ref WS_PlayerData.CharacterObject Who)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_STATUS_COMPLETE);
                    if (ReferenceEquals(Trader, Who))
                    {
                        Target.client.SendMultiplyPackets(ref response);
                        TraderAccept = true;
                    }
                    else
                    {
                        Trader.client.SendMultiplyPackets(ref response);
                        TargetAccept = true;
                    }
                }
                finally
                {
                    response.Dispose();
                }

                if (TargetAccept && TraderAccept)
                    DoTrade();
            }

            private void DoTrade()
            {
                byte TargetReqItems = 0;
                byte TraderReqItems = 0;
                for (byte i = 0; i <= 5; i++)
                {
                    if (TraderSlots[i] > 0)
                        TargetReqItems = (byte)(TargetReqItems + 1);
                    if (TargetSlots[i] > 0)
                        TraderReqItems = (byte)(TraderReqItems + 1);
                }

                try
                {
                    // DONE: Check free slots
                    if (Target.ItemFREESLOTS() < TargetReqItems)
                    {
                        var responseUnAccept = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                        try
                        {
                            responseUnAccept.AddInt32((int)TradeStatus.TRADE_STATUS_UNACCEPT);
                            Target.client.SendMultiplyPackets(ref responseUnAccept);
                            TraderAccept = false;
                            Trader.client.SendMultiplyPackets(ref responseUnAccept);
                            TraderAccept = false;
                        }
                        finally
                        {
                            responseUnAccept.Dispose();
                        }

                        var responseNoSlot = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                        try
                        {
                            responseNoSlot.AddInt8((byte)InventoryChangeFailure.EQUIP_ERR_INVENTORY_FULL);
                            responseNoSlot.AddUInt64(0UL);
                            responseNoSlot.AddUInt64(0UL);
                            responseNoSlot.AddInt8(0);
                            Target.client.Send(ref responseNoSlot);
                        }
                        finally
                        {
                            responseNoSlot.Dispose();
                        }

                        return;
                    }

                    if (Trader.ItemFREESLOTS() < TraderReqItems)
                    {
                        var responseUnAccept = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                        try
                        {
                            responseUnAccept.AddInt32((int)TradeStatus.TRADE_STATUS_UNACCEPT);
                            Target.client.SendMultiplyPackets(ref responseUnAccept);
                            TraderAccept = false;
                            Trader.client.SendMultiplyPackets(ref responseUnAccept);
                            TargetAccept = false;
                        }
                        finally
                        {
                            responseUnAccept.Dispose();
                        }

                        var responseNoSlot = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                        try
                        {
                            responseNoSlot.AddInt8((byte)InventoryChangeFailure.EQUIP_ERR_INVENTORY_FULL);
                            responseNoSlot.AddUInt64(0UL);
                            responseNoSlot.AddUInt64(0UL);
                            responseNoSlot.AddInt8(0);
                            Trader.client.Send(ref responseNoSlot);
                        }
                        finally
                        {
                            responseNoSlot.Dispose();
                        }

                        return;
                    }

                    // DONE: Trade gold
                    if (TargetGold > 0L | TraderGold > 0L)
                    {
                        Trader.Copper = Trader.Copper - TraderGold + TargetGold;
                        Target.Copper = Target.Copper + TraderGold - TargetGold;
                        Trader.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, Trader.Copper);
                        Target.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, Target.Copper);
                    }

                    // TODO: For item set ITEM_FIELD_GIFTCREATOR
                    // DONE: Item trading
                    if (TargetReqItems > 0 | TraderReqItems > 0)
                    {
                        for (byte i = 0; i <= 5; i++)
                        {
                            if (TraderSlots[i] > 0)
                            {
                                byte mySlot = (byte)(TraderSlots[i] & 0xFF);
                                byte myBag = (byte)(TraderSlots[i] >> 8);
                                ItemObject myItem = null;
                                if (myBag == 0)
                                    myItem = Trader.Items[mySlot];
                                else
                                    myItem = Trader.Items[myBag].Items[mySlot];

                                // DONE: Disable trading of quest items
                                if (myItem.ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_QUEST)
                                {
                                    // DONE: Swap items
                                    myItem.OwnerGUID = Target.GUID;
                                    if (Target.ItemADD(ref myItem))
                                        Trader.ItemREMOVE(myBag, mySlot, false, false);
                                }
                            }

                            if (TargetSlots[i] > 0)
                            {
                                byte mySlot = (byte)(TargetSlots[i] & 0xFF);
                                byte myBag = (byte)(TargetSlots[i] >> 8);
                                ItemObject myItem = null;
                                if (myBag == 0)
                                    myItem = Target.Items[mySlot];
                                else
                                    myItem = Target.Items[myBag].Items[mySlot];

                                // DONE: Disable trading of quest items
                                if (myItem.ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_QUEST)
                                {
                                    // DONE: Swap items
                                    myItem.OwnerGUID = Trader.GUID;
                                    if (Trader.ItemADD(ref myItem))
                                        Target.ItemREMOVE(myBag, mySlot, false, false);
                                }
                            }
                        }
                    }

                    Trader.SendCharacterUpdate(true);
                    Target.SendCharacterUpdate(true);
                    var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                    try
                    {
                        response.AddInt32((int)TradeStatus.TRADE_COMPLETE);
                        Target.client.SendMultiplyPackets(ref response);
                        Trader.client.SendMultiplyPackets(ref response);
                    }
                    finally
                    {
                        response.Dispose();
                        Dispose();
                    }
                }
                catch (Exception e)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error doing trade: {0}{1}", Environment.NewLine, e.ToString());
                }
            }
        }

        public void On_CMSG_CANCEL_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (client is null)
                return;
            if (client.Character is null)
                return;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_TRADE", client.IP, client.Port);
            if (client.Character.tradeInfo is object)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_STATUS_CANCELED);
                    if (client.Character.tradeInfo.Target is object)
                        client.Character.tradeInfo.Target.client.SendMultiplyPackets(ref response);
                    if (client.Character.tradeInfo.Trader is object)
                        client.Character.tradeInfo.Trader.client.SendMultiplyPackets(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                client.Character.tradeInfo.Dispose();
            }
        }

        public void On_CMSG_SET_TRADE_GOLD(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            uint gold = packet.GetUInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_TRADE_GOLD [gold={2}]", client.IP, client.Port, gold);
            if (client.Character.tradeInfo is null)
                return;
            if (ReferenceEquals(client.Character.tradeInfo.Trader, client.Character))
            {
                client.Character.tradeInfo.TraderGold = gold;
                client.Character.tradeInfo.SendTradeUpdateToTarget();
            }
            else
            {
                client.Character.tradeInfo.TargetGold = gold;
                client.Character.tradeInfo.SendTradeUpdateToTrader();
            }
        }

        public void On_CMSG_SET_TRADE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            byte slot = packet.GetInt8();
            byte myBag = packet.GetInt8();
            byte mySlot = packet.GetInt8();
            if (myBag == 255)
                myBag = 0;
            if (slot > 6)
                return;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_TRADE_ITEM [slot={2} myBag={3} mySlot={4}]", client.IP, client.Port, slot, myBag, mySlot);
            if (client.Character.tradeInfo is null)
                return;
            if (ReferenceEquals(client.Character.tradeInfo.Trader, client.Character))
            {
                client.Character.tradeInfo.TraderSlots[slot] = (Conversions.ToInteger(myBag) << 8) + mySlot;
                client.Character.tradeInfo.SendTradeUpdateToTarget();
            }
            else
            {
                client.Character.tradeInfo.TargetSlots[slot] = (Conversions.ToInteger(myBag) << 8) + mySlot;
                client.Character.tradeInfo.SendTradeUpdateToTrader();
            }
        }

        public void On_CMSG_CLEAR_TRADE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            byte slot = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CLEAR_TRADE_ITEM [slot={2}]", client.IP, client.Port, slot);
            if (ReferenceEquals(client.Character.tradeInfo.Trader, client.Character))
            {
                client.Character.tradeInfo.TraderSlots[slot] = -1;
                client.Character.tradeInfo.SendTradeUpdateToTarget();
            }
            else
            {
                client.Character.tradeInfo.TargetSlots[slot] = -1;
                client.Character.tradeInfo.SendTradeUpdateToTrader();
            }
        }

        public void On_CMSG_INITIATE_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong targetGUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_INITIATE_TRADE [Trader={2} Target={3}]", client.IP, client.Port, client.Character.GUID, targetGUID);
            if (client.Character.DEAD == true)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_DEAD);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }
            else if (client.Character.LogoutTimer is object)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_LOGOUT);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }
            else if (client.Character.cUnitFlags & UnitFlags.UNIT_FLAG_STUNTED)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_STUNNED);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(targetGUID) == false)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_TARGET_MISSING);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }
            else if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].DEAD == true)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_TARGET_DEAD);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }
            else if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].LogoutTimer is object)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_TARGET_LOGOUT);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }
            else if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].cUnitFlags & UnitFlags.UNIT_FLAG_STUNTED)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_STUNNED);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }

            if (client.Character.tradeInfo is object)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_TARGET_UNAVIABLE);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].tradeInfo is object)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_TARGET_UNAVIABLE2);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].IsHorde != client.Character.IsHorde)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_TARGET_DIFF_FACTION);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }

            if (WorldServiceLocator._WS_Combat.GetDistance(client.Character, WorldServiceLocator._WorldServer.CHARACTERs[targetGUID]) > 30.0f)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_TARGET_TOO_FAR);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }

            if (client.Character.Access == AccessLevel.Trial)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_TRIAL_ACCOUNT);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }

            if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].Access == AccessLevel.Trial)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32((int)TradeStatus.TRADE_TRIAL_ACCOUNT);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }

                return;
            }

            // TODO: Another of these currently 'DO NOTHING' lines, needs to be implemented correctly
            var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
            var argTarget_ = tmp[targetGUID];
            var tmpTradeInfo = new TTradeInfo(ref client.Character, ref argTarget_);
            tmp[targetGUID] = argTarget_;
            var response_ok = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
            try
            {
                response_ok.AddInt32((int)TradeStatus.TRADE_STATUS_OK);
                response_ok.AddUInt64(client.Character.GUID);
                client.Character.tradeInfo.Target.client.Send(ref response_ok);
            }
            finally
            {
                response_ok.Dispose();
            }
        }

        public void On_CMSG_BEGIN_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BEGIN_TRADE", client.IP, client.Port);
            client.Character.tradeInfo.ID += 1;
            var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
            try
            {
                response.AddInt32((int)TradeStatus.TRADE_TRADE_WINDOW_OPEN);
                response.AddInt32(client.Character.tradeInfo.ID);
                client.Character.tradeInfo.Trader.client.SendMultiplyPackets(ref response);
                client.Character.tradeInfo.Target.client.SendMultiplyPackets(ref response);
            }
            finally
            {
                response.Dispose();
            }
        }

        public void On_CMSG_UNACCEPT_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_UNACCEPT_TRADE", client.IP, client.Port);
            var response = new Packets.PacketClass(OPCODES.SMSG_TRADE_STATUS);
            try
            {
                response.AddInt32((int)TradeStatus.TRADE_STATUS_UNACCEPT);
                if (ReferenceEquals(client.Character.tradeInfo.Trader, client.Character))
                {
                    client.Character.tradeInfo.Target.client.SendMultiplyPackets(ref response);
                    client.Character.tradeInfo.TraderAccept = false;
                }
                else
                {
                    client.Character.tradeInfo.Trader.client.SendMultiplyPackets(ref response);
                    client.Character.tradeInfo.TargetAccept = false;
                }
            }
            finally
            {
                response.Dispose();
            }
        }

        public void On_CMSG_ACCEPT_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ACCEPT_TRADE", client.IP, client.Port);
            client.Character.tradeInfo.DoTrade(ref client.Character);
        }

        public void On_CMSG_IGNORE_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_IGNORE_TRADE", client.IP, client.Port);
        }

        public void On_CMSG_BUSY_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUSY_TRADE", client.IP, client.Port);
        }
    }
}