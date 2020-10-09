using System;

namespace Mangos.Common.Enums.GameObject
{
    [Flags]
    public enum GameObjectFlags : byte
    {
        GO_FLAG_IN_USE = 0x1,                        // disables interaction while animated
        GO_FLAG_LOCKED = 0x2,                        // require key, spell, event, etc to be opened. Makes "Locked" appear in tooltip
        GO_FLAG_INTERACT_COND = 0x4,                 // cannot interact (condition to interact)
        GO_FLAG_TRANSPORT = 0x8,                     // any kind of transport? Object can transport (elevator, boat, car)
        GO_FLAG_UNK1 = 0x10,                         // 
        GO_FLAG_NODESPAWN = 0x20,                    // never despawn, typically for doors, they just change state
        GO_FLAG_TRIGGERED = 0x40                    // typically, summoned objects. Triggered by spell or other events
    }
}