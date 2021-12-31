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

using System;
using System.Runtime.InteropServices;

namespace Mangos.World.Warden;

public sealed class NativeMethods
{
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "LoadLibraryA", ExactSpelling = true, SetLastError = true)]
    private static extern int LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] ref string lpLibFileName); //Is this the correct type?

    public static int LoadLibrary(string lpLibFileName, string dummy)
    {
        return LoadLibrary(ref lpLibFileName);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

    public static UIntPtr GetProcAddress(IntPtr hModule, string lpProcName, string dummy)
    {
        return GetProcAddress(hModule, lpProcName);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    public static int CloseHandle(IntPtr hObject, string dummy)
    {
        return 0 - (CloseHandle(hObject) ? 1 : 0);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern int VirtualProtect(int lpAddress, int dwSize, int flNewProtect, ref int lpflOldProtect);

    public static int VirtualProtect(int lpAddress, int dwSize, int flNewProtect, ref int lpflOldProtect, string dummy)
    {
        return VirtualProtect(lpAddress, dwSize, flNewProtect, ref lpflOldProtect);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern int FlushInstructionCache(int hProcess, int lpBaseAddress, int dwSize);

    public static int FlushInstructionCache(int hProcess, int lpBaseAddress, int dwSize, string dummy)
    {
        return FlushInstructionCache(hProcess, lpBaseAddress, dwSize);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern int GetCurrentProcess();

    public static int GetCurrentProcess(string dummy)
    {
        return GetCurrentProcess();
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern int VirtualFree(int lpAddress, int dwSize, int dwFreeType);

    public static int VirtualFree(int lpAddress, int dwSize, int dwFreeType, string dummy)
    {
        return VirtualFree(lpAddress, dwSize, dwFreeType);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern int GlobalLock(int hMem);

    public static int GlobalLock(int hMem, string dummy)
    {
        return GlobalLock(hMem);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern int GlobalUnlock(int hMem);

    public static int GlobalUnlock(int hMem, string dummy)
    {
        return GlobalUnlock(hMem);
    }
}
