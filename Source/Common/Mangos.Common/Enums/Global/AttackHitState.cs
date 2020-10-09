
namespace Mangos.Common.Enums.Global
{
    public enum AttackHitState : int
    {
        HIT_UNARMED = Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_NORMALSWING,
        HIT_NORMAL = Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_HITANIMATION,
        HIT_NORMAL_OFFHAND = (int)Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_HITANIMATION + (int)Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_LEFTSWING,
        HIT_MISS = Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_MISS,
        HIT_MISS_OFFHAND = (int)Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_MISS + (int)Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_LEFTSWING,
        HIT_CRIT = Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_CRITICALHIT,
        HIT_CRIT_OFFHAND = (int)Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_CRITICALHIT + (int)Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_LEFTSWING,
        HIT_RESIST = Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_RESIST,
        HIT_CRUSHING_BLOW = Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_CRUSHING,
        HIT_GLANCING_BLOW = Global::Mangos.Common.Enums.Global.AttackHitState.HITINFO_GLANCING,
        HITINFO_NORMALSWING = 0x0,
        HITINFO_UNK = 0x1,
        HITINFO_HITANIMATION = 0x2,
        HITINFO_LEFTSWING = 0x4,
        HITINFO_RANGED = 0x8,
        HITINFO_MISS = 0x10,
        HITINFO_ABSORB = 0x20,
        HITINFO_RESIST = 0x40,
        HITINFO_UNK2 = 0x100,
        HITINFO_CRITICALHIT = 0x200,
        HITINFO_BLOCK = 0x800,
        HITINFO_UNK3 = 0x2000,
        HITINFO_CRUSHING = 0x8000,
        HITINFO_GLANCING = 0x10000,
        HITINFO_NOACTION = 0x10000,
        HITINFO_SWINGNOHITSOUND = 0x80000
    }
}