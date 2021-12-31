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

namespace Mangos.World.Quests;

public class WS_QuestsBaseScripted : WS_QuestsBase
{
    public virtual void OnQuestStart(ref WS_PlayerData.CharacterObject objCharacter)
    {
    }

    public virtual void OnQuestComplete(ref WS_PlayerData.CharacterObject objCharacter)
    {
    }

    public virtual void OnQuestCancel(ref WS_PlayerData.CharacterObject objCharacter)
    {
    }

    public virtual void OnQuestItem(ref WS_PlayerData.CharacterObject objCharacter, int ItemID, int ItemCount)
    {
    }

    public virtual void OnQuestKill(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject Creature)
    {
    }

    public virtual void OnQuestCastSpell(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject Creature, int SpellID)
    {
    }

    public virtual void OnQuestCastSpell(ref WS_PlayerData.CharacterObject objCharacter, ref WS_GameObjects.GameObject GameObject, int SpellID)
    {
    }

    public virtual void OnQuestExplore(ref WS_PlayerData.CharacterObject objCharacter, int AreaID)
    {
    }

    public virtual void OnQuestEmote(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject Creature, int EmoteID)
    {
    }
}
