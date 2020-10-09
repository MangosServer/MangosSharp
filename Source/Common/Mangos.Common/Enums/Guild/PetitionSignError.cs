
namespace Mangos.Common.Enums.Guild
{
    public enum PetitionSignError : int
    {
        PETITIONSIGN_OK = 0,                     // :Closes the window
        PETITIONSIGN_ALREADY_SIGNED = 1,         // You have already signed that guild charter
        PETITIONSIGN_ALREADY_IN_GUILD = 2,       // You are already in a guild
        PETITIONSIGN_CANT_SIGN_OWN = 3,          // You can's sign own guild charter
        PETITIONSIGN_NOT_SERVER = 4             // That player is not from your server
    }
}