using Mangos.World.Objects;
using Mangos.World.Player;

namespace Mangos.World.Quests
{
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

		public virtual void OnQuestCastSpell(ref WS_PlayerData.CharacterObject objCharacter, ref WS_GameObjects.GameObjectObject GameObject, int SpellID)
		{
		}

		public virtual void OnQuestExplore(ref WS_PlayerData.CharacterObject objCharacter, int AreaID)
		{
		}

		public virtual void OnQuestEmote(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject Creature, int EmoteID)
		{
		}
	}
}
