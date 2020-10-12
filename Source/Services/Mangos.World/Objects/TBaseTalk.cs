using Mangos.World.Player;

namespace Mangos.World.Objects
{
	public class TBaseTalk
	{
		public virtual void OnGossipHello(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
		{
		}

		public virtual void OnGossipSelect(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID, int selected)
		{
		}

		public virtual int OnQuestStatus(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
		{
			return 0;
		}

		public virtual bool OnQuestHello(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
		{
			return true;
		}
	}
}
