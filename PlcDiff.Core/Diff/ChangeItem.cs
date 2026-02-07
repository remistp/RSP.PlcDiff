using PlcDiff.Core.Models;

namespace PlcDiff.Core.Diff;

public sealed class ChangeItem
{
    public required ChangeItemType Type { get; init; }
    public required string Path { get; init; }
    public required ChangeKind ChangeKind { get; init; }
    public required string Summary { get; init; }
}
