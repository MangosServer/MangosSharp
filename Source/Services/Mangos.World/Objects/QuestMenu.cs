using System.Collections;

namespace Mangos.World.Objects
{
	public class QuestMenu
	{
		public ArrayList IDs;

		public ArrayList Names;

		public ArrayList Icons;

		public ArrayList Levels;

		public QuestMenu()
		{
			IDs = new ArrayList();
			Names = new ArrayList();
			Icons = new ArrayList();
			Levels = new ArrayList();
		}

		public void AddMenu(string QuestName, short ID, short Level, byte Icon = 0)
		{
			Names.Add(QuestName);
			IDs.Add(ID);
			Icons.Add(Icon);
			Levels.Add(Level);
		}
	}
}
