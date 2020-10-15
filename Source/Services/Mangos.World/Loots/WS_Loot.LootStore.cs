//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Mangos.Common;
using Mangos.Common.Enums.Global;
using Microsoft.VisualBasic;

namespace Mangos.World.Loots
{
    public partial class WS_Loot
	{
        public class LootStore
		{
			private readonly string Name;

			private readonly Dictionary<int, LootTemplate> Templates;

			public LootStore(string Name)
			{
				Templates = new Dictionary<int, LootTemplate>();
				this.Name = Name;
			}

			public LootTemplate GetLoot(int Entry)
			{
				if (Templates.ContainsKey(Entry))
				{
					return Templates[Entry];
				}
				return CreateTemplate(Entry);
			}

			private LootTemplate CreateTemplate(int Entry)
			{
				LootTemplate newTemplate = new LootTemplate();
				Templates.Add(Entry, newTemplate);
				DataTable MysqlQuery = new DataTable();
				WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT {0}.*,conditions.type,conditions.value1, conditions.value2 FROM {0} LEFT JOIN conditions ON {0}.`condition_id`=conditions.`condition_entry` WHERE entry = {1};", Name, Entry), ref MysqlQuery);
				if (MysqlQuery.Rows.Count == 0)
				{
					Templates[Entry] = null;
					return null;
				}
				IEnumerator enumerator = default;
				try
				{
					enumerator = MysqlQuery.Rows.GetEnumerator();
					while (enumerator.MoveNext())
					{
						DataRow row = (DataRow)enumerator.Current;
						int Item = row.As<int>("item");
						float ChanceOrQuestChance = row.As<float>("ChanceOrQuestChance");
						byte GroupID = row.As<byte>("groupid");
						int MinCountOrRef = row.As<int>("mincountOrRef");
						byte MaxCount = row.As<byte>("maxcount");
						ConditionType LootCondition = ConditionType.CONDITION_NONE;
						if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(row["type"])))
						{
							LootCondition = (ConditionType)row.As<int>("type");
						}
						int ConditionValue1 = 0;
						if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(row["value1"])))
						{
							ConditionValue1 = row.As<int>("value1");
						}
						int ConditionValue2 = 0;
						if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(row["value2"])))
						{
							ConditionValue2 = row.As<int>("value2");
						}
						LootStoreItem newItem = new LootStoreItem(Item, Math.Abs(ChanceOrQuestChance), GroupID, MinCountOrRef, MaxCount, LootCondition, ConditionValue1, ConditionValue2, ChanceOrQuestChance < 0f);
						newTemplate.AddItem(ref newItem);
					}
				}
				finally
				{
					if (enumerator is IDisposable)
					{
						(enumerator as IDisposable).Dispose();
					}
				}
				return newTemplate;
			}
		}
	}
}
