using System;

namespace Mangos.Common.Enums.Player
{
    [Flags]
    public enum DamageMasks : int
    {
        DMG_NORMAL = 0x0,
        DMG_PHYSICAL = 0x1,
        DMG_HOLY = 0x2,
        DMG_FIRE = 0x4,
        DMG_NATURE = 0x8,
        DMG_FROST = 0x10,
        DMG_SHADOW = 0x20,
        DMG_ARCANE = 0x40
    }
}