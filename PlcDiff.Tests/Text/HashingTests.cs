using PlcDiff.Core.Text;
using Xunit;

namespace PlcDiff.Tests.Text;

public sealed class HashingTests
{
    [Fact]
    public void ComputeKey_UsesRockwellIdWhenPresent()
    {
        var key = Hashing.ComputeKey("XIC(Input1)", "Rung-123");

        Assert.Equal("Rung-123", key);
    }

    [Fact]
    public void ComputeKey_ComputesSha256WhenRockwellIdMissing()
    {
        var key = Hashing.ComputeKey("XIC(Input1)", null);

        Assert.Equal("e100e49ddf8f326757a843423bb4dfa0b7f7334876e581aef2ae6ed072377596", key);
    }
}
