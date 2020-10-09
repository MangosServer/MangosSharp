
namespace Mangos.Common.Enums.Spell
{
    public enum SpellAuraInterruptFlags : int
    {
        AURA_INTERRUPT_FLAG_HOSTILE_SPELL_INFLICTED = 0x1, // removed when recieving a hostile spell?
        AURA_INTERRUPT_FLAG_DAMAGE = 0x2, // removed by any damage
        AURA_INTERRUPT_FLAG_CC = 0x4, // removed by crowd control
        AURA_INTERRUPT_FLAG_MOVE = 0x8, // removed by any movement
        AURA_INTERRUPT_FLAG_TURNING = 0x10, // removed by any turning
        AURA_INTERRUPT_FLAG_ENTER_COMBAT = 0x20, // removed by entering combat
        AURA_INTERRUPT_FLAG_NOT_MOUNTED = 0x40, // removed by unmounting
        AURA_INTERRUPT_FLAG_SLOWED = 0x80, // removed by being slowed
        AURA_INTERRUPT_FLAG_NOT_UNDERWATER = 0x100, // removed by leaving water
        AURA_INTERRUPT_FLAG_NOT_SHEATHED = 0x200, // removed by unsheathing
        AURA_INTERRUPT_FLAG_TALK = 0x400, // removed by action to NPC
        AURA_INTERRUPT_FLAG_USE = 0x800, // removed by action to GameObject
        AURA_INTERRUPT_FLAG_START_ATTACK = 0x1000, // removed by attacking
        AURA_INTERRUPT_FLAG_UNK4 = 0x2000,
        AURA_INTERRUPT_FLAG_UNK5 = 0x4000,
        AURA_INTERRUPT_FLAG_CAST_SPELL = 0x8000, // removed at spell cast
        AURA_INTERRUPT_FLAG_UNK6 = 0x10000,
        AURA_INTERRUPT_FLAG_MOUNTING = 0x20000, // removed by mounting
        AURA_INTERRUPT_FLAG_NOT_SEATED = 0x40000, // removed by standing up
        AURA_INTERRUPT_FLAG_CHANGE_MAP = 0x80000, // leaving map/getting teleported
        AURA_INTERRUPT_FLAG_INVINCIBLE = 0x100000, // removed when invicible
        AURA_INTERRUPT_FLAG_STEALTH = 0x200000, // removed by stealth
        AURA_INTERRUPT_FLAG_UNK7 = 0x400000
    }
}