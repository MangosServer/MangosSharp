//
//  Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System.Collections.Generic;

namespace Mangos.World.Loots
{
    public partial class WS_Loot
    {
        public class LootGroup
        {
            public List<LootStoreItem> ExplicitlyChanced;

            public List<LootStoreItem> EqualChanced;

            public LootGroup()
            {
                ExplicitlyChanced = new List<LootStoreItem>();
                EqualChanced = new List<LootStoreItem>();
            }

            public void AddItem(ref LootStoreItem Item)
            {
                if (Item.Chance != 0f)
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
                checked
                {
                    if (ExplicitlyChanced.Count > 0)
                    {
                        float rollChance = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 100.0);
                        int num = ExplicitlyChanced.Count - 1;
                        for (int i = 0; i <= num; i++)
                        {
                            if (ExplicitlyChanced[i].Chance >= 100f)
                            {
                                return ExplicitlyChanced[i];
                            }
                            rollChance -= ExplicitlyChanced[i].Chance;
                            if (rollChance <= 0f)
                            {
                                return ExplicitlyChanced[i];
                            }
                        }
                    }
                    if (EqualChanced.Count > 0)
                    {
                        return EqualChanced[WorldServiceLocator._WorldServer.Rnd.Next(0, EqualChanced.Count)];
                    }
                    return null;
                }
            }

            public void Process(ref LootObject Loot)
            {
                LootStoreItem Item = Roll();
                if (Item != null)
                {
                    Loot.Items.Add(new LootItem(ref Item));
                }
            }
        }
    }
}
