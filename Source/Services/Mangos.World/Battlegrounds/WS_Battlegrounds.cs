using System;
using System.Collections.Generic;
using Mangos.Common.Enums.Global;
using Mangos.World.Globals;
using Mangos.World.Player;

namespace Mangos.World.Battlegrounds
{
	public class WS_Battlegrounds
	{
		public class Battlefield : IDisposable
		{
			public List<WS_PlayerData.CharacterObject> MembersTeam1;

			public List<WS_PlayerData.CharacterObject> MembersTeam2;

			public int ID;

			public uint Map;

			public BattlefieldMapType MapType;

			private bool _disposedValue;

			public Battlefield(BattlefieldMapType rMapType, uint rMap)
			{
				MembersTeam1 = new List<WS_PlayerData.CharacterObject>();
				MembersTeam2 = new List<WS_PlayerData.CharacterObject>();
				WorldServiceLocator._WS_Battlegrounds.BATTLEFIELDs.Add(ID, this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					WorldServiceLocator._WS_Battlegrounds.BATTLEFIELDs.Remove(ID);
				}
				_disposedValue = true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				this.Dispose();
			}

			public void Update(object State)
			{
			}

			public void Broadcast(Packets.PacketClass p)
			{
				BroadcastTeam1(p);
				BroadcastTeam2(p);
			}

			public void BroadcastTeam1(Packets.PacketClass p)
			{
				WS_PlayerData.CharacterObject[] array = MembersTeam1.ToArray();
				foreach (WS_PlayerData.CharacterObject objCharacter in array)
				{
					objCharacter.client.SendMultiplyPackets(ref p);
				}
			}

			public void BroadcastTeam2(Packets.PacketClass p)
			{
				WS_PlayerData.CharacterObject[] array = MembersTeam2.ToArray();
				foreach (WS_PlayerData.CharacterObject objCharacter in array)
				{
					objCharacter.client.SendMultiplyPackets(ref p);
				}
			}
		}

		public Dictionary<int, Battlefield> BATTLEFIELDs;

		public WS_Battlegrounds()
		{
			BATTLEFIELDs = new Dictionary<int, Battlefield>();
		}
	}
}
