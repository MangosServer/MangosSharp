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

namespace Mangos.World.Loots;

public partial class WS_Loot
{
    public class LootStoreItem
    {
        public int ItemID;

        public float Chance;

        public byte Group;

        public int MinCountOrRef;

        public byte MaxCount;

        public ConditionType LootCondition;

        public int ConditionValue1;

        public int ConditionValue2;

        public bool NeedQuest;

        public LootStoreItem(int Item, float Chance, byte Group, int MinCountOrRef, byte MaxCount, ConditionType LootCondition, int ConditionValue1, int ConditionValue2, bool NeedQuest)
        {
            ItemID = 0;
            this.Chance = 0f;
            this.Group = 0;
            this.MinCountOrRef = 0;
            this.MaxCount = 0;
            this.LootCondition = ConditionType.CONDITION_NONE;
            this.ConditionValue1 = 0;
            this.ConditionValue2 = 0;
            this.NeedQuest = false;
            ItemID = Item;
            this.Chance = Chance;
            this.Group = Group;
            this.MinCountOrRef = MinCountOrRef;
            this.MaxCount = MaxCount;
            this.LootCondition = LootCondition;
            this.ConditionValue1 = ConditionValue1;
            this.ConditionValue2 = ConditionValue2;
            this.NeedQuest = NeedQuest;
        }

        public bool Roll()
        {
            return Chance >= 100f || WorldServiceLocator._Functions.RollChance(Chance);
        }
    }
}
