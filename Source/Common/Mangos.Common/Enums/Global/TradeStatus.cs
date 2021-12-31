//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

namespace Mangos.Common.Enums.Global;

public enum TradeStatus : byte
{
    TRADE_TARGET_UNAVIABLE = 0,              // "[NAME] is busy"
    TRADE_STATUS_OK = 1,                     // BEGIN TRADE
    TRADE_TRADE_WINDOW_OPEN = 2,             // OPEN TRADE WINDOW
    TRADE_STATUS_CANCELED = 3,               // "Trade canceled"
    TRADE_STATUS_COMPLETE = 4,               // TRADE COMPLETE
    TRADE_TARGET_UNAVIABLE2 = 5,             // "[NAME] is busy"
    TRADE_TARGET_MISSING = 6,                // SOUND: I dont have a target
    TRADE_STATUS_UNACCEPT = 7,               // BACK TRADE
    TRADE_COMPLETE = 8,                      // "Trade Complete"
    TRADE_UNK2 = 9,
    TRADE_TARGET_TOO_FAR = 10,               // "Trade target is too far away"
    TRADE_TARGET_DIFF_FACTION = 11,          // "Trade is not party of your alliance"
    TRADE_TRADE_WINDOW_CLOSE = 12,           // CLOSE TRADE WINDOW
    TRADE_UNK3 = 13,
    TRADE_TARGET_IGNORING = 14,              // "[NAME] is ignoring you"
    TRADE_STUNNED = 15,                      // "You are stunned"
    TRADE_TARGET_STUNNED = 16,               // "Target is stunned"
    TRADE_DEAD = 17,                         // "You cannot do that when you are dead"
    TRADE_TARGET_DEAD = 18,                  // "You cannot trade with dead players"
    TRADE_LOGOUT = 19,                       // "You are loging out"
    TRADE_TARGET_LOGOUT = 20,                // "The player is loging out"
    TRADE_TRIAL_ACCOUNT = 21,                // "Trial accounts cannot perform that action"
    TRADE_STATUS_ONLY_CONJURED = 22         // "You can only trade conjured items... (cross realm BG related)."
}
