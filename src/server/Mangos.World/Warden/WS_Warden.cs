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

using Mangos.World.Globals;
using Mangos.World.Handlers;
using Mangos.World.Player;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mangos.World.Warden;

public partial class WS_Warden
{
    public WardenMaiev Maiev;

    public WS_Warden()
    {
        Maiev = new WardenMaiev();
    }

    private int VarPtr(ref object obj)
    {
        return GCHandle.Alloc(RuntimeHelpers.GetObjectValue(obj), GCHandleType.Pinned).AddrOfPinnedObject().ToInt32();
    }

    private int ByteArrPtr(ref byte[] arr)
    {
        var pData = Malloc(arr.Length);
        Marshal.Copy(arr, 0, new IntPtr(pData), arr.Length);
        return pData;
    }

    private int Malloc(int length)
    {
        checked
        {
            var tmpHandle = Marshal.AllocHGlobal(length + 4).ToInt32();
            var lockedHandle = NativeMethods.GlobalLock(tmpHandle, "") + 4;
            Marshal.WriteInt32(new IntPtr(lockedHandle - 4), tmpHandle);
            return lockedHandle;
        }
    }

    private void Free(int ptr)
    {
        var tmpHandle = Marshal.ReadInt32(new IntPtr(checked(ptr - 4)));
        NativeMethods.GlobalUnlock(tmpHandle, "");
        Marshal.FreeHGlobal(new IntPtr(tmpHandle));
    }

    public void SendWardenPacket(ref WS_PlayerData.CharacterObject objCharacter, ref Packets.PacketClass Packet)
    {
        var b = new byte[checked(Packet.Data.Length - 4 - 1 + 1)];
        Buffer.BlockCopy(Packet.Data, 4, b, 0, b.Length);
        WS_Handlers_Warden.RC4.Crypt(ref b, objCharacter.WardenData.KeyIn);
        Buffer.BlockCopy(b, 0, Packet.Data, 4, b.Length);
        objCharacter.client.Send(ref Packet);
    }
}
