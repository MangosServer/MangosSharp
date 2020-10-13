using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.World.Globals;
using Mangos.World.Server;

namespace Mangos.World.Handlers
{
	public class WS_Handlers_Gamemaster
	{
		public void On_CMSG_WORLD_TELEPORT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WORLD_TELEPORT", client.IP, client.Port);
			if (client.Access >= AccessLevel.GameMaster)
			{
				packet.GetInt16();
				int Time = packet.GetInt32();
				uint Map = packet.GetUInt32();
				float X = packet.GetFloat();
				float Y = packet.GetFloat();
				float Z = packet.GetFloat();
				float O = packet.GetFloat();
				client.Character.Teleport(X, Y, Z, O, checked((int)Map));
			}
		}
	}
}
