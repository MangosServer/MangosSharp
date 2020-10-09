
namespace Mangos.Common.Enums.Guild
{
    public enum PetitionTurnInError : int
    {
        PETITIONTURNIN_OK = 0,                   // :Closes the window
        PETITIONTURNIN_ALREADY_IN_GUILD = 2,     // You are already in a guild
        PETITIONTURNIN_NEED_MORE_SIGNATURES = 4 // You need more signatures
    }
}