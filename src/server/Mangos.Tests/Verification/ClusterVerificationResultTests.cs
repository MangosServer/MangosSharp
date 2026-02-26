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

using Mangos.Cluster.Verification;
using Xunit;

namespace Mangos.Tests.Verification;

public class ClusterVerificationResultTests
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
    public void When_VerificationResult_HasIssues_Then_IssuesCountIsSet()
    {
        var result = new VerificationResult
        {
            CheckName = "TestCheck",
            Status = VerificationStatus.Failed,
            Message = "Found problems",
            IssuesFound = 3
        };

        Assert.Equal("TestCheck", result.CheckName);
        Assert.Equal(VerificationStatus.Failed, result.Status);
        Assert.Equal("Found problems", result.Message);
        Assert.Equal(3, result.IssuesFound);
    }

    [Fact]
    public void When_VerificationResult_AddDetails_Then_DetailsAreStored()
    {
        var result = new VerificationResult
        {
            CheckName = "DetailCheck"
        };

        result.Details.Add("Detail 1");
        result.Details.Add("Detail 2");
        result.Details.Add("Detail 3");

        Assert.Equal(3, result.Details.Count);
        Assert.Equal("Detail 1", result.Details[0]);
    }

    [Theory]
    [InlineData(VerificationStatus.Passed)]
    [InlineData(VerificationStatus.Warning)]
    [InlineData(VerificationStatus.Failed)]
    [InlineData(VerificationStatus.Error)]
    public void When_VerificationStatus_Set_Then_ValueIsCorrect(VerificationStatus status)
    {
        var result = new VerificationResult
        {
            Status = status
        };

        Assert.Equal(status, result.Status);
    }
}
