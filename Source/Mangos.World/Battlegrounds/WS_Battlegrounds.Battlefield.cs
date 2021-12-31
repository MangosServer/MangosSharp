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

using Mangos.Common.Enums.Global;
using Mangos.World.Globals;
using Mangos.World.Player;
using System;
using System.Collections.Generic;

namespace Mangos.World.Battlegrounds;

public partial class WS_Battlegrounds
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
            Dispose();
        }

        public void Update(object State)
        {
        }

        public void Broadcast(Packets.PacketClass p)
        {
            if (p is null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            BroadcastTeam1(p);
            BroadcastTeam2(p);
        }

        public void BroadcastTeam1(Packets.PacketClass p)
        {
            if (p is null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            foreach (var objCharacter in MembersTeam1.ToArray())
            {
                objCharacter.client.SendMultiplyPackets(ref p);
            }
        }

        public void BroadcastTeam2(Packets.PacketClass p)
        {
            if (p is null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            var array = MembersTeam2.ToArray();
            foreach (var objCharacter in array)
            {
                objCharacter.client.SendMultiplyPackets(ref p);
            }
        }
    }
}
