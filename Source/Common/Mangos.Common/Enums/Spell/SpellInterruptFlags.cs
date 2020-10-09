
namespace Mangos.Common.Enums.Spell
{
    public enum SpellInterruptFlags : int
    {
        SPELL_INTERRUPT_FLAG_MOVEMENT = 0x1, // why need this for instant?
        SPELL_INTERRUPT_FLAG_PUSH_BACK = 0x2, // push back
        SPELL_INTERRUPT_FLAG_INTERRUPT = 0x4, // interrupt
        SPELL_INTERRUPT_FLAG_AUTOATTACK = 0x8, // no
        SPELL_INTERRUPT_FLAG_DAMAGE = 0x10  // _complete_ interrupt on direct damage?
    }
}