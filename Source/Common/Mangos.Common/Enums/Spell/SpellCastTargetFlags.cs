
namespace Mangos.Common.Enums.Spell
{
    public enum SpellCastTargetFlags : int
    {
        TARGET_FLAG_SELF = 0x0,
        TARGET_FLAG_UNIT = 0x2,
        TARGET_FLAG_ITEM = 0x10,
        TARGET_FLAG_SOURCE_LOCATION = 0x20,
        TARGET_FLAG_DEST_LOCATION = 0x40,
        TARGET_FLAG_OBJECT_UNK = 0x80,
        TARGET_FLAG_PVP_CORPSE = 0x200,
        TARGET_FLAG_OBJECT = 0x800,
        TARGET_FLAG_TRADE_ITEM = 0x1000,
        TARGET_FLAG_STRING = 0x2000,
        TARGET_FLAG_UNK1 = 0x4000,
        TARGET_FLAG_CORPSE = 0x8000,
        TARGET_FLAG_UNK2 = 0x10000
    }
}