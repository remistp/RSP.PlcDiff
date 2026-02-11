using PlcDiff.Core.Ladder.Parsing;

namespace PlcDiff.Core.Ladder.Layout;

public sealed class LadderLayoutPlan
{
    public List<LadderInstructionPlacement> Instructions { get; } = new();
    public List<LadderBranchRail> Rails { get; } = new();
    public int Rows { get; set; }
    public int Columns { get; set; }
}

public sealed class LadderInstructionPlacement
{
    public LadderInstructionPlacement(string opcode, string operands, int row, int column)
    {
        Opcode = opcode;
        Operands = operands;
        Row = row;
        Column = column;
    }

    public string Opcode { get; }
    public string Operands { get; }
    public int Row { get; }
    public int Column { get; }
}

public sealed class LadderBranchRail
{
    public LadderBranchRail(int rowStart, int rowEnd, int colStart, int colEnd)
    {
        RowStart = rowStart;
        RowEnd = rowEnd;
        ColStart = colStart;
        ColEnd = colEnd;
    }

    public int RowStart { get; }
    public int RowEnd { get; }
    public int ColStart { get; }
    public int ColEnd { get; }
}

public sealed class LadderLayoutPlanner
{
    public LadderLayoutPlan Plan(SeriesNode root)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        var plan = new LadderLayoutPlan();
        var size = Measure(root);
        plan.Rows = size.Rows;
        plan.Columns = size.Columns;

        PlaceNode(root, 0, 0, plan);

        return plan;
    }

    private static LayoutSize Measure(LadderNode node)
    {
        return node switch
        {
            SeriesNode series => MeasureSeries(series),
            ParallelNode parallel => MeasureParallel(parallel),
            InstructionNode => new LayoutSize(1, 1),
            _ => throw new InvalidOperationException($"Unknown node type {node.GetType().Name}.")
        };
    }

    private static LayoutSize MeasureSeries(SeriesNode series)
    {
        var width = 0;
        var height = 1;
        foreach (var child in series.Children)
        {
            var size = Measure(child);
            width += size.Columns;
            height = Math.Max(height, size.Rows);
        }

        return new LayoutSize(height, Math.Max(1, width));
    }

    private static LayoutSize MeasureParallel(ParallelNode parallel)
    {
        var width = 1;
        var height = 0;
        foreach (var branch in parallel.Branches)
        {
            var size = Measure(branch);
            width = Math.Max(width, size.Columns);
            height += size.Rows;
        }

        return new LayoutSize(Math.Max(1, height), width);
    }

    private static void PlaceNode(LadderNode node, int row, int col, LadderLayoutPlan plan)
    {
        switch (node)
        {
            case SeriesNode series:
                PlaceSeries(series, row, col, plan);
                break;
            case ParallelNode parallel:
                PlaceParallel(parallel, row, col, plan);
                break;
            case InstructionNode instruction:
                plan.Instructions.Add(new LadderInstructionPlacement(
                    instruction.Opcode,
                    instruction.Operands,
                    row,
                    col));
                break;
            default:
                throw new InvalidOperationException($"Unknown node type {node.GetType().Name}.");
        }
    }

    private static void PlaceSeries(SeriesNode series, int row, int col, LadderLayoutPlan plan)
    {
        var currentCol = col;
        var seriesHeight = Measure(series).Rows;
        foreach (var child in series.Children)
        {
            var size = Measure(child);
            var rowOffset = row;
            if (size.Rows < seriesHeight)
            {
                rowOffset = row + (seriesHeight - size.Rows) / 2;
            }

            PlaceNode(child, rowOffset, currentCol, plan);
            currentCol += size.Columns;
        }
    }

    private static void PlaceParallel(ParallelNode parallel, int row, int col, LadderLayoutPlan plan)
    {
        var startRow = row;
        var maxWidth = 1;
        foreach (var branch in parallel.Branches)
        {
            maxWidth = Math.Max(maxWidth, Measure(branch).Columns);
        }

        foreach (var branch in parallel.Branches)
        {
            var size = Measure(branch);
            PlaceNode(branch, startRow, col, plan);
            startRow += size.Rows;
        }

        var totalHeight = Measure(parallel).Rows;
        plan.Rails.Add(new LadderBranchRail(row, row + totalHeight - 1, col, col + maxWidth));
    }

    private readonly record struct LayoutSize(int Rows, int Columns);
}
