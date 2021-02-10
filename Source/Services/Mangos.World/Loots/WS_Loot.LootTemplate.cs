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
        public class LootTemplate
        {
            public List<LootStoreItem> Items;

            public Dictionary<byte, LootGroup> Groups;

            public LootTemplate()
            {
                Items = new List<LootStoreItem>();
                Groups = new Dictionary<byte, LootGroup>();
            }

            public void AddItem(ref LootStoreItem Item)
            {
                if (Item.Group > 0 && Item.MinCountOrRef > 0)
                {
                    if (!Groups.ContainsKey(Item.Group))
                    {
                        Groups.Add(Item.Group, new LootGroup());
                    }
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
                    if (Groups.ContainsKey(GroupID))
                    {
                        Groups[GroupID].Process(ref Loot);
                    }
                    return;
                }
                checked
                {
                    int num = Items.Count - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        if (!Items[i].Roll())
                        {
                            continue;
                        }
                        if (Items[i].MinCountOrRef < 0)
                        {
                            LootTemplate Referenced = WorldServiceLocator._WS_Loot.LootTemplates_Reference.GetLoot(-Items[i].MinCountOrRef);
                            if (Referenced != null)
                            {
                                int maxCount = Items[i].MaxCount;
                                for (int j = 1; j <= maxCount; j++)
                                {
                                    Referenced.Process(ref Loot, Items[i].Group);
                                }
                            }
                        }
                        else
                        {
                            List<LootItem> items = Loot.Items;
                            List<LootStoreItem> items2;
                            int index;
                            LootStoreItem Item = (items2 = Items)[index = i];
                            LootItem item = new LootItem(ref Item);
                            items2[index] = Item;
                            items.Add(item);
                        }
                    }
                    foreach (KeyValuePair<byte, LootGroup> group in Groups)
                    {
                        group.Value.Process(ref Loot);
                    }
                }
            }
        }
    }
}