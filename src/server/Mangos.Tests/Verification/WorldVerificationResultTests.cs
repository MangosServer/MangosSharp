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

using Mangos.World.Verification;
using Xunit;

namespace Mangos.Tests.Verification;

public class WorldVerificationResultTests
{
    [Fact]
    public void When_VerificationResult_Created_Then_DefaultsAreCorrect()
    {
        var result = new VerificationResult();

        Assert.NotNull(result.Details);
        Assert.Empty(result.Details);
        Assert.Equal(0, result.IssuesFound);
        Assert.True(result.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void When_VerificationResult_HasIssues_Then_PropertiesAreSet()
    {
        var result = new VerificationResult
        {
            CheckName = "WorldCheck",
            Status = VerificationStatus.Warning,
            Message = "Minor issues found",
            IssuesFound = 2
        };

        Assert.Equal("WorldCheck", result.CheckName);
        Assert.Equal(VerificationStatus.Warning, result.Status);
        Assert.Equal(2, result.IssuesFound);
    }

    [Fact]
    public void When_VerificationResult_Timestamp_Then_IsRecentUtc()
    {
        var before = DateTime.UtcNow;
        var result = new VerificationResult();
        var after = DateTime.UtcNow;

        Assert.InRange(result.Timestamp, before, after);
    }
}
