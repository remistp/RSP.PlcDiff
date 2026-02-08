using PlcDiff.Core.Ladder;
using Xunit;

namespace PlcDiff.Tests.Ladder;

public sealed class LadderTokenizerTests
{
    [Fact]
    public void Tokenize_ExtractsInstructionsWithOperands()
    {
        var rung = "XIC(Input1) OTE(Output1);";

        var tokens = LadderTokenizer.Tokenize(rung);

        Assert.Collection(tokens,
            token =>
            {
                Assert.Equal(LadderInstructionType.Xic, token.Instruction);
                Assert.Equal("Input1", token.Operand);
            },
            token =>
            {
                Assert.Equal(LadderInstructionType.Ote, token.Instruction);
                Assert.Equal("Output1", token.Operand);
            });
    }

    [Fact]
    public void Tokenize_HandlesParallelAndTimers()
    {
        var rung = "[XIC(Input2) ,XIC(Input3), XIC(Input4) ]TON(Timer1, 1000);";

        var tokens = LadderTokenizer.Tokenize(rung);

        Assert.Equal(4, tokens.Count);
        Assert.Equal(LadderInstructionType.Ton, tokens[^1].Instruction);
        Assert.Equal("Timer1, 1000", tokens[^1].Operand);
    }

    [Fact]
    public void Tokenize_ReturnsEmptyForMalformed()
    {
        var rung = "XIC( ) OTE";

        var tokens = LadderTokenizer.Tokenize(rung);

        Assert.Empty(tokens);
    }
}
