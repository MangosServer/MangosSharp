
namespace Mangos.Common.Enums.Global
{
    public enum AuraFlags
    {
        AFLAG_NONE = 0x0,
        AFLAG_VISIBLE = 0x1,
        AFLAG_EFF_INDEX_1 = 0x2,
        AFLAG_EFF_INDEX_2 = 0x4,
        AFLAG_NOT_GUID = 0x8,
        AFLAG_CANCELLABLE = 0x10,
        AFLAG_HAS_DURATION = 0x20,
        AFLAG_UNK2 = 0x40,
        AFLAG_NEGATIVE = 0x80,
        AFLAG_POSITIVE = 0x1F,
        AFLAG_MASK = 0xFF
    }
}