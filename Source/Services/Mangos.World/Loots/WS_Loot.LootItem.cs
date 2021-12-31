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
using Mangos.World.Objects;
using Microsoft.VisualBasic.CompilerServices;
using System;

namespace Mangos.World.Loots;

public partial class WS_Loot
{
    public class LootItem : IDisposable
    {
        public int ItemID;

        public byte ItemCount;

        private bool _disposedValue;

        public int ItemModel
        {
            get
            {
                if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(ItemID))
                {
                    try
                    {
                        WorldServiceLocator._WorldServer.ITEMDatabase.Remove(ItemID);
                        WS_Items.ItemInfo tmpItem = new(ItemID);
                        WorldServiceLocator._WorldServer.ITEMDatabase.Add(ItemID, tmpItem);
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error on ItemModel [Item ID {0} : Exception {1}]", ItemID, ex);
                    }
                }
                return WorldServiceLocator._WorldServer.ITEMDatabase[ItemID].Model;
            }
        }

        public LootItem(ref LootStoreItem Item)
        {
            ItemID = 0;
            ItemCount = 0;
            ItemID = Item.ItemID;
            checked
            {
                ItemCount = (byte)WorldServiceLocator._WorldServer.Rnd.Next(Item.MinCountOrRef, Item.MaxCount + 1);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
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
