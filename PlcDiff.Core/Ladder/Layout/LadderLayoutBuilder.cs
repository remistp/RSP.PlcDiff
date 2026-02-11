using PlcDiff.Core.Ladder.Parsing;

namespace PlcDiff.Core.Ladder.Layout;

public static class LadderLayoutBuilder
{
    public static SeriesLayout Build(SeriesNode series)
    {
        if (series == null)
        {
            throw new ArgumentNullException(nameof(series));
        }

        var layout = new SeriesLayout();
        foreach (var child in series.Children)
        {
            layout.Children.Add(BuildNode(child));
        }

        return layout;
    }

    private static LadderLayoutNode BuildNode(LadderNode node)
    {
        return node switch
        {
            SeriesNode series => Build(series),
            ParallelNode parallel => BuildParallel(parallel),
            InstructionNode instruction => new InstructionLayout(instruction.Opcode, instruction.Operands),
            _ => throw new InvalidOperationException($"Unknown node type {node.GetType().Name}.")
        };
    }

    private static ParallelLayout BuildParallel(ParallelNode parallel)
    {
        var layout = new ParallelLayout();
        foreach (var branch in parallel.Branches)
        {
            layout.Branches.Add(Build(branch));
        }

        return layout;
    }
}
