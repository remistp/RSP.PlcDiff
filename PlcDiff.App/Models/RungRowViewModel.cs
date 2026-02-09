using PlcDiff.Core.Diff;
using PlcDiff.Core.Models;

namespace PlcDiff.App.Models;

public sealed class RungRowViewModel
{
    public RungRowViewModel(int index, ChangeKind changeKind, string? rawText)
    {
        Index = index;
        ChangeKind = changeKind;
        RawText = rawText ?? string.Empty;
    }

    public int Index { get; }
    public ChangeKind ChangeKind { get; }
    public string RawText { get; }
}
