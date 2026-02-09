using System.Collections.ObjectModel;
using PlcDiff.Core.Diff;
using PlcDiff.Core.Ladder;
using PlcDiff.Core.Models;

namespace PlcDiff.App.Models;

public sealed class RungRowViewModel
{
    public RungRowViewModel(int index, ChangeKind changeKind, string? rawText)
    {
        Index = index;
        ChangeKind = changeKind;
        RawText = rawText ?? string.Empty;

        foreach (var token in LadderTokenizer.Tokenize(rawText))
        {
            Tokens.Add(token);
        }
    }

    public int Index { get; }
    public ChangeKind ChangeKind { get; }
    public string RawText { get; }
    public ObservableCollection<LadderToken> Tokens { get; } = new();
}
