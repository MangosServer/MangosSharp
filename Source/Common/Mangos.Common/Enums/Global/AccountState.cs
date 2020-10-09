
namespace Mangos.Common.Enums.Global
{
    public enum AccountState : byte
    {
        // RealmServ Error Codes
        LOGIN_OK = 0x0,
        LOGIN_FAILED = 0x1,          // Unable to connect
        LOGIN_BANNED = 0x3,          // This World of Warcraft account has been closed and is no longer in service -- Please check the registered email address of this account for further information.
        LOGIN_UNKNOWN_ACCOUNT = 0x4, // The information you have entered is not valid.  Please check the spelling of the account name and password.  If you need help in retrieving a lost or stolen password and account, see www.worldofwarcraft.com for more information.
        LOGIN_BAD_PASS = 0x5,        // The information you have entered is not valid.  Please check the spelling of the account name and password.  If you need help in retrieving a lost or stolen password and account, see www.worldofwarcraft.com for more information.
        LOGIN_ALREADYONLINE = 0x6,   // This account is already logged into World of Warcraft.  Please check the spelling and try again.
        LOGIN_NOTIME = 0x7,          // You have used up your prepaid time for this account. Please purchase more to continue playing.
        LOGIN_DBBUSY = 0x8,          // Could not log in to World of Warcraft at this time.  Please try again later.
        LOGIN_BADVERSION = 0x9,      // Unable to validate game version.  This may be caused by file corruption or the interference of another program.  Please visit www.blizzard.com/support/wow/ for more information and possible solutions to this issue.
        LOGIN_DOWNLOADFILE = 0xA,
        LOGIN_SUSPENDED = 0xC,       // This World Of Warcraft account has been temporarily suspended. Please go to http://www.wow-europe.com/en/misc/banned.html for further information.
        LOGIN_PARENTALCONTROL = 0xF // Access to this account has been blocked by parental controls.  Your settings may be changed in your account preferences at http://www.worldofwarcraft.com.
    }
}