
namespace Mangos.Common.Enums.Map
{
    public enum TransferAbortReason : short
    {
        TRANSFER_ABORT_MAX_PLAYERS = 0x1,                // Transfer Aborted: instance is full
        TRANSFER_ABORT_NOT_FOUND = 0x2,                  // Transfer Aborted: instance not found
        TRANSFER_ABORT_TOO_MANY_INSTANCES = 0x3,         // You have entered too many instances recently.
        TRANSFER_ABORT_ZONE_IN_COMBAT = 0x5,             // Unable to zone in while an encounter is in progress.
        TRANSFER_ABORT_INSUF_EXPAN_LVL1 = 0x106         // You must have TBC expansion installed to access this area.
    }
}