using PlcDiff.Core.Ladder.Parsing;
using Xunit;

namespace PlcDiff.Tests.Ladder;

public sealed class LadderParserTests
{
    [Fact]
    public void Parse_HandlesSimpleParallel()
    {
        var parser = new LadderParser();
        var ast = parser.Parse("[XIC(A),XIC(B)]OTE(C);");

        Assert.Single(ast.Children);
        var parallel = Assert.IsType<ParallelNode>(ast.Children[0]);
        Assert.Equal(2, parallel.Branches.Count);
        Assert.Single(parallel.Branches[0].Children);
        Assert.Single(parallel.Branches[1].Children);
    }

    [Fact]
    public void Parse_HandlesNestedParallel()
    {
        var parser = new LadderParser();
        var ast = parser.Parse("[XIC(A),[XIC(B),XIC(C)]XIO(D)]OTE(E);");

        Assert.Equal(2, ast.Children.Count);
        var parallel = Assert.IsType<ParallelNode>(ast.Children[0]);
        Assert.Equal(2, parallel.Branches.Count);
        Assert.IsType<SeriesNode>(parallel.Branches[1]);
    }
    [Fact]
    public void Parse_ParsesMovInstructionOperands()
    {
        var parser = new LadderParser();
        var ast = parser.Parse("XIC(A) MOV(Source, Destination) OTE(B);");

        Assert.Equal(3, ast.Children.Count);
        var mov = Assert.IsType<InstructionNode>(ast.Children[1]);
        Assert.Equal("MOV", mov.Opcode);
        Assert.Equal("Source, Destination", mov.Operands);
    }

}
