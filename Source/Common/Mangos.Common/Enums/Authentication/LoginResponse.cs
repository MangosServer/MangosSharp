
namespace Mangos.Common.Enums.Authentication
{
    public enum LoginResponse : byte
    {
        LOGIN_OK = 0xC,
        LOGIN_VERSION_MISMATCH = 0x14,
        LOGIN_UNKNOWN_ACCOUNT = 0x15,
        LOGIN_WAIT_QUEUE = 0x1B
    }
}