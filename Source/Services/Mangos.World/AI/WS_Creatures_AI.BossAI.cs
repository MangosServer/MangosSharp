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

using Mangos.World.Objects;
using Mangos.World.Player;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.World.AI;

public partial class WS_Creatures_AI
{
    public class BossAI : DefaultAI
    {
        public BossAI(ref WS_Creatures.CreatureObject Creature)
            : base(ref Creature)
        {
        }

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
            foreach (var Unit in aiHateTable)
            {
                if (Unit.Key is not WS_PlayerData.CharacterObject)
                {
                    continue;
                }
                WS_PlayerData.CharacterObject characterObject = (WS_PlayerData.CharacterObject)Unit.Key;
                if (characterObject.IsInGroup)
                {
                    var array = characterObject.Group.LocalMembers.ToArray();
                    foreach (var member in array)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(member) && WorldServiceLocator._WorldServer.CHARACTERs[member].MapID == characterObject.MapID && WorldServiceLocator._WorldServer.CHARACTERs[member].instance == characterObject.instance)
                        {
                            aiHateTable.Add(WorldServiceLocator._WorldServer.CHARACTERs[member], 0);
                        }
                    }
                    break;
                }
            }
        }

        public override void DoThink()
        {
            base.DoThink();
            new Thread(OnThink)
            {
                Name = "Boss Thinking"
            }.Start();
        }

        public virtual void OnThink()
        {
        }
    }
}
