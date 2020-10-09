
namespace Mangos.Common.Enums.Warden
{
    public enum MaievResponse : byte
    {
        MAIEV_RESPONSE_FAILED_OR_MISSING = 0x0,          // The module was either currupt or not in the cache request transfer
        MAIEV_RESPONSE_SUCCESS = 0x1,                    // The module was in the cache and loaded successfully
        MAIEV_RESPONSE_RESULT = 0x2,
        MAIEV_RESPONSE_HASH = 0x4
    }
}