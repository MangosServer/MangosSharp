//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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

using Mangos.Common.Globals;
using Xunit;

namespace Mangos.Tests.Common;

public class OpcodeTests
{
    [Fact]
    public void When_PetitionOpcodes_Then_ValuesAreCorrect()
    {
        Assert.Equal((Opcodes)0x1BB, Opcodes.CMSG_PETITION_SHOWLIST);
        Assert.Equal((Opcodes)0x1BD, Opcodes.CMSG_PETITION_BUY);
        Assert.Equal((Opcodes)0x1BE, Opcodes.CMSG_PETITION_SHOW_SIGNATURES);
        Assert.Equal((Opcodes)0x1C0, Opcodes.CMSG_PETITION_SIGN);
        Assert.Equal((Opcodes)0x1C2, Opcodes.MSG_PETITION_DECLINE);
        Assert.Equal((Opcodes)0x1C3, Opcodes.CMSG_OFFER_PETITION);
        Assert.Equal((Opcodes)0x1C4, Opcodes.CMSG_TURN_IN_PETITION);
        Assert.Equal((Opcodes)0x1C6, Opcodes.CMSG_PETITION_QUERY);
        Assert.Equal((Opcodes)0x2C1, Opcodes.MSG_PETITION_RENAME);
    }

    [Fact]
    public void When_CoreAuthOpcodes_Then_ValuesAreCorrect()
    {
        Assert.Equal((Opcodes)0x1ED, Opcodes.CMSG_AUTH_SESSION);
        Assert.Equal((Opcodes)0x1DC, Opcodes.CMSG_PING);
        Assert.Equal((Opcodes)0x37, Opcodes.CMSG_CHAR_ENUM);
        Assert.Equal((Opcodes)0x36, Opcodes.CMSG_CHAR_CREATE);
        Assert.Equal((Opcodes)0x38, Opcodes.CMSG_CHAR_DELETE);
        Assert.Equal((Opcodes)0x3D, Opcodes.CMSG_PLAYER_LOGIN);
    }

    [Fact]
    public void When_GuildOpcodes_Then_ValuesAreCorrect()
    {
        Assert.Equal((Opcodes)0x81, Opcodes.CMSG_GUILD_CREATE);
        Assert.Equal((Opcodes)0x82, Opcodes.CMSG_GUILD_INVITE);
        Assert.Equal((Opcodes)0x89, Opcodes.CMSG_GUILD_QUERY);
        Assert.Equal((Opcodes)0x54, Opcodes.CMSG_GUILD_DISBAND);
    }

    [Fact]
    public void When_BattlegroundOpcodes_Then_ValuesAreCorrect()
    {
        Assert.Equal((Opcodes)0x2EE, Opcodes.CMSG_BATTLEMASTER_JOIN);
        Assert.Equal((Opcodes)0x2D5, Opcodes.CMSG_BATTLEFIELD_PORT);
        Assert.Equal((Opcodes)0x2E1, Opcodes.CMSG_LEAVE_BATTLEFIELD);
    }
}
