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

using System.Collections.Generic;

namespace Mangos.World.Loots;

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
            switch (Item.Group)
            {
                case > 0 when Item.MinCountOrRef > 0:
                    if (!Groups.ContainsKey(Item.Group))
                    {
                        Groups.Add(Item.Group, new LootGroup());
                    }
                    Groups[Item.Group].AddItem(ref Item);
                    break;
                default:
                    Items.Add(Item);
                    break;
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
                for (var i = 0; i <= Items.Count - 1; i++)
                {
                    if (!Items[i].Roll())
                    {
                        continue;
                    }
                    if (Items[i].MinCountOrRef < 0)
                    {
                        var Referenced = WorldServiceLocator._WS_Loot.LootTemplates_Reference.GetLoot(-Items[i].MinCountOrRef);
                        if (Referenced != null)
                        {
                            int maxCount = Items[i].MaxCount;
                            for (var j = 1; j <= maxCount; j++)
                            {
                                Referenced.Process(ref Loot, Items[i].Group);
                            }
                        }
                    }
                    else
                    {
                        int index;
                        List<LootStoreItem> items2;
                        var Item = (items2 = Items)[index = i];
                        items2[index] = Item;
                        var items = Loot.Items;
                        LootItem item = new(ref Item);
                        items.Add(item);
                    }
                }
                foreach (var group in Groups)
                {
                    group.Value.Process(ref Loot);
                }
            }
        }
    }
}
