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

using Mangos.Common.Globals;
using System;

namespace Mangos.World.Globals;

public partial class Packets
{
    public class UpdatePacketClass : PacketClass
    {
        public int UpdatesCount
        {
            get => BitConverter.ToInt32(Data, 4);
            set
            {
                checked
                {
                    Data[4] = (byte)(value & 0xFF);
                    Data[5] = (byte)((value >> 8) & 0xFF);
                    Data[6] = (byte)((value >> 16) & 0xFF);
                    Data[7] = (byte)((value >> 24) & 0xFF);
                }
            }
        }

        public UpdatePacketClass()
            : base(Opcodes.SMSG_UPDATE_OBJECT)
        {
            AddInt32(0);
            AddInt8(0);
        }
    }
}
