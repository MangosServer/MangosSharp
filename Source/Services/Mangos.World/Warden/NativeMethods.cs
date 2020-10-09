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
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Warden
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryA")]
        private static extern int LoadLibrary(string lpLibFileName);

        public static int LoadLibrary(string lpLibFileName, string dummy)
        {
            return NativeMethods.LoadLibrary(ref lpLibFileName);
        }

        // Private Declare Function GetProcAddress Lib "kernel32.dll" (ByVal hModule As Integer, ByVal lpProcName As String) As Integer
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

        public static UIntPtr GetProcAddress(IntPtr hModule, string lpProcName, string dummy)
        {
            return GetProcAddress(hModule, lpProcName);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        // Declare Function CloseHandle Lib "kernel32" Alias "CloseHandle" (ByVal hObject As Integer) As Integer

        public static int CloseHandle(IntPtr hObject, string dummy)
        {
            return Conversions.ToInteger(CloseHandle(hObject));
        }

        [DllImport("kernel32.dll")]
        private static extern int VirtualProtect(int lpAddress, int dwSize, int flNewProtect, ref int lpflOldProtect);

        public static int VirtualProtect(int lpAddress, int dwSize, int flNewProtect, ref int lpflOldProtect, string dummy)
        {
            return VirtualProtect(lpAddress, dwSize, flNewProtect, ref lpflOldProtect);
        }

        [DllImport("kernel32.dll")]
        private static extern int FlushInstructionCache(int hProcess, int lpBaseAddress, int dwSize);

        public static int FlushInstructionCache(int hProcess, int lpBaseAddress, int dwSize, string dummy)
        {
            return FlushInstructionCache(hProcess, lpBaseAddress, dwSize);
        }

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentProcess();

        public static int GetCurrentProcess(string dummy)
        {
            return GetCurrentProcess();
        }

        [DllImport("kernel32.dll")]
        private static extern int VirtualFree(int lpAddress, int dwSize, int dwFreeType);

        public static int VirtualFree(int lpAddress, int dwSize, int dwFreeType, string dummy)
        {
            return VirtualFree(lpAddress, dwSize, dwFreeType);
        }

        [DllImport("kernel32.dll")]
        private static extern int GlobalLock(int hMem);

        public static int GlobalLock(int hMem, string dummy)
        {
            return GlobalLock(hMem);
        }

        [DllImport("kernel32.dll")]
        private static extern int GlobalUnlock(int hMem);

        public static int GlobalUnlock(int hMem, string dummy)
        {
            return GlobalUnlock(hMem);
        }
    }
}