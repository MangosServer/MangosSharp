using System.Collections;

namespace Mangos.World.Objects
{
	public class GossipMenu
	{
		public ArrayList Icons;

		public ArrayList Menus;

		public ArrayList Coded;

		public ArrayList Costs;

		public ArrayList WarningMessages;

		public GossipMenu()
		{
			Icons = new ArrayList();
			Menus = new ArrayList();
			Coded = new ArrayList();
			Costs = new ArrayList();
			WarningMessages = new ArrayList();
		}

		public void AddMenu(string menu, byte icon = 0, byte isCoded = 0, int cost = 0, string WarningMessage = "")
		{
			Icons.Add(icon);
			Menus.Add(menu);
			Coded.Add(isCoded);
			Costs.Add(cost);
			WarningMessages.Add(WarningMessage);
		}
	}
}
