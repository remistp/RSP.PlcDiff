using PlcDiff.Core.Ladder;
using Xunit;

namespace PlcDiff.Tests.Ladder;

public sealed class LadderTokenizerBranchTests
{
    [Fact]
    public void Tokenize_SetsBranchDepthWithinBrackets()
    {
        var rung = "[XIC(Input2) ,XIC(Input3) ]OTE(Output2);";

        var tokens = LadderTokenizer.Tokenize(rung);

        Assert.Collection(tokens,
            token =>
            {
                Assert.Equal(LadderInstructionType.Xic, token.Instruction);
                Assert.Equal(1, token.BranchDepth);
            },
            token =>
            {
                Assert.Equal(LadderInstructionType.Xic, token.Instruction);
                Assert.Equal(1, token.BranchDepth);
            },
            token =>
            {
                Assert.Equal(LadderInstructionType.Ote, token.Instruction);
                Assert.Equal(0, token.BranchDepth);
            });
    }
}
