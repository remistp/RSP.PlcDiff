namespace PlcDiff.Core.Ladder.Layout;

public abstract class LadderLayoutNode
{
}

public sealed class SeriesLayout : LadderLayoutNode
{
    public List<LadderLayoutNode> Children { get; } = new();
}

public sealed class ParallelLayout : LadderLayoutNode
{
    public List<SeriesLayout> Branches { get; } = new();
}

public sealed class InstructionLayout : LadderLayoutNode
{
    public InstructionLayout(string opcode, string operands)
    {
        Opcode = opcode;
        Operands = operands;
    }

    public string Opcode { get; }
    public string Operands { get; }
}
