// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System;
using System.Collections.Generic;
using Mangos.Common.Enums.Global;
using Mangos.World.Globals;
using Mangos.World.Player;

namespace Mangos.World.Battlegrounds
{
    public class WS_Battlegrounds
    {
        public Dictionary<int, Battlefield> BATTLEFIELDs = new Dictionary<int, Battlefield>();

        public class Battlefield : IDisposable
        {
            public List<WS_PlayerData.CharacterObject> MembersTeam1 = new List<WS_PlayerData.CharacterObject>();
            public List<WS_PlayerData.CharacterObject> MembersTeam2 = new List<WS_PlayerData.CharacterObject>();
            public int ID;
            public uint Map;
            public BattlefieldMapType MapType;

            public Battlefield(BattlefieldMapType rMapType, uint rMap)
            {
                WorldServiceLocator._WS_Battlegrounds.BATTLEFIELDs.Add(ID, this);
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    WorldServiceLocator._WS_Battlegrounds.BATTLEFIELDs.Remove(ID);
                }

                _disposedValue = true;
            }

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
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
                foreach (WS_PlayerData.CharacterObject objCharacter in MembersTeam1.ToArray())
                    objCharacter.client.SendMultiplyPackets(ref p);
            }

            public void BroadcastTeam2(Packets.PacketClass p)
            {
                foreach (WS_PlayerData.CharacterObject objCharacter in MembersTeam2.ToArray())
                    objCharacter.client.SendMultiplyPackets(ref p);
            }
        }
    }
}