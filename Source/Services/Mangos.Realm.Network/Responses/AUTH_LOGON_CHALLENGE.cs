﻿//
// Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
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

namespace Mangos.Realm.Network.Responses
{
    public class AUTH_LOGON_CHALLENGE
    {
        public byte[] PublicB { get; }
        public byte[] G { get; }
        public byte[] N { get; }
        public byte[] Salt { get; }
        public byte[] CrcSalt { get; }

        public AUTH_LOGON_CHALLENGE(byte[] publicB, byte[] g, byte[] n, byte[] salt, byte[] crcSalt)
        {
            PublicB = publicB;
            G = g;
            N = n;
            Salt = salt;
            CrcSalt = crcSalt;
        }
    }
}