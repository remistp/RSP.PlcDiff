namespace PlcDiff.Core.Diff;

public sealed class DiffReport
{
    public List<ChangeItem> Changes { get; } = new();
}
