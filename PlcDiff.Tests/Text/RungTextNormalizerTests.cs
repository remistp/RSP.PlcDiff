using PlcDiff.Core.Text;
using Xunit;

namespace PlcDiff.Tests.Text;

public sealed class RungTextNormalizerTests
{
    [Fact]
    public void Normalize_TrimsAndNormalizesWhitespace()
    {
        var raw = "  XIC(Input1)  OTE(Output1);\r\n\r\n";

        var normalized = RungTextNormalizer.Normalize(raw);

        Assert.Equal("XIC(Input1) OTE(Output1);", normalized);
    }

    [Fact]
    public void Normalize_PreservesNewlinesButNormalizesLineEndings()
    {
        var raw = "XIC(Input1)\r\nOTE(Output1);\rXIC(Input2)";

        var normalized = RungTextNormalizer.Normalize(raw);

        Assert.Equal("XIC(Input1)\nOTE(Output1);\nXIC(Input2)", normalized);
    }
}
