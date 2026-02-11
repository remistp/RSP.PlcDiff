namespace PlcDiff.Core.Ladder;

public sealed class LadderToken
{
    public required LadderInstructionType Instruction { get; init; }
    public required string Operand { get; init; }
    public int BranchDepth { get; init; }
}
