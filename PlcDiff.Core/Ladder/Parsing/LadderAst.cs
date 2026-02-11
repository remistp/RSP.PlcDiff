namespace PlcDiff.Core.Ladder.Parsing;

public abstract class LadderNode
{
}

public sealed class SeriesNode : LadderNode
{
    public List<LadderNode> Children { get; } = new();
}

public sealed class ParallelNode : LadderNode
{
    public List<SeriesNode> Branches { get; } = new();
}

public sealed class InstructionNode : LadderNode
{
    public InstructionNode(string opcode, string operands, int start, int length)
    {
        Opcode = opcode;
        Operands = operands;
        Start = start;
        Length = length;
    }

    public string Opcode { get; }
    public string Operands { get; }
    public int Start { get; }
    public int Length { get; }
}
