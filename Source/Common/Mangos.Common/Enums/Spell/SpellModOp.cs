
namespace Mangos.Common.Enums.Spell
{
    public enum SpellModOp
    {
        SPELLMOD_DAMAGE = 0,
        SPELLMOD_DURATION = 1,
        SPELLMOD_THREAT = 2,
        SPELLMOD_EFFECT1 = 3,
        SPELLMOD_CHARGES = 4,
        SPELLMOD_RANGE = 5,
        SPELLMOD_RADIUS = 6,
        SPELLMOD_CRITICAL_CHANCE = 7,
        SPELLMOD_ALL_EFFECTS = 8,
        SPELLMOD_NOT_LOSE_CASTING_TIME = 9,
        SPELLMOD_CASTING_TIME = 10,
        SPELLMOD_COOLDOWN = 11,
        SPELLMOD_EFFECT2 = 12,
        // spellmod 13 unused
        SPELLMOD_COST = 14,
        SPELLMOD_CRIT_DAMAGE_BONUS = 15,
        SPELLMOD_RESIST_MISS_CHANCE = 16,
        SPELLMOD_JUMP_TARGETS = 17,
        SPELLMOD_CHANCE_OF_SUCCESS = 18,                   // Only used with SPELL_AURA_ADD_FLAT_MODIFIER and affects proc spells
        SPELLMOD_ACTIVATION_TIME = 19,
        SPELLMOD_EFFECT_PAST_FIRST = 20,
        SPELLMOD_CASTING_TIME_OLD = 21,
        SPELLMOD_DOT = 22,
        SPELLMOD_EFFECT3 = 23,
        SPELLMOD_SPELL_BONUS_DAMAGE = 24,
        // spellmod 25 unused
        SPELLMOD_FREQUENCY_OF_SUCCESS = 26,                   // Only used with SPELL_AURA_ADD_PCT_MODIFIER and affects used on proc spells
        SPELLMOD_MULTIPLE_VALUE = 27,
        SPELLMOD_RESIST_DISPEL_CHANCE = 28
    }
}