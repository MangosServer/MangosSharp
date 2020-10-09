using System;

namespace Mangos.Common.Enums.Character
{
    [Flags]
    public enum CharacterFlagState : uint
    {
        CHARACTER_FLAG_NONE = 0x0,
        CHARACTER_FLAG_UNK1 = 0x1,
        CHARACTER_FLAG_UNK2 = 0x2,
        CHARACTER_FLAG_LOCKED_FOR_TRANSFER = 0x4,                    // Character Locked for Paid Character Transfer
        CHARACTER_FLAG_UNK4 = 0x8,
        CHARACTER_FLAG_UNK5 = 0x10,
        CHARACTER_FLAG_UNK6 = 0x20,
        CHARACTER_FLAG_UNK7 = 0x40,
        CHARACTER_FLAG_UNK8 = 0x80,
        CHARACTER_FLAG_UNK9 = 0x100,
        CHARACTER_FLAG_UNK10 = 0x200,
        CHARACTER_FLAG_HIDE_HELM = 0x400,
        CHARACTER_FLAG_HIDE_CLOAK = 0x800,
        CHARACTER_FLAG_UNK13 = 0x1000,
        CHARACTER_FLAG_GHOST = 0x2000,                               // Player is ghost in char selection screen
        CHARACTER_FLAG_RENAME = 0x4000,                              // On login player will be asked to change name
        CHARACTER_FLAG_UNK16 = 0x8000,
        CHARACTER_FLAG_UNK17 = 0x10000,
        CHARACTER_FLAG_UNK18 = 0x20000,
        CHARACTER_FLAG_UNK19 = 0x40000,
        CHARACTER_FLAG_UNK20 = 0x80000,
        CHARACTER_FLAG_UNK21 = 0x100000,
        CHARACTER_FLAG_UNK22 = 0x200000,
        CHARACTER_FLAG_UNK23 = 0x400000,
        CHARACTER_FLAG_UNK24 = 0x800000,
        CHARACTER_FLAG_LOCKED_BY_BILLING = 0x1000000,
        CHARACTER_FLAG_DECLINED = 0x2000000,
        CHARACTER_FLAG_UNK27 = 0x4000000,
        CHARACTER_FLAG_UNK28 = 0x8000000,
        CHARACTER_FLAG_UNK29 = 0x10000000,
        CHARACTER_FLAG_UNK30 = 0x20000000,
        CHARACTER_FLAG_UNK31 = 0x40000000,
        CHARACTER_FLAG_UNK32 = 0x80000000,
    }
}