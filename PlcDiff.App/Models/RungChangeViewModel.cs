using System.Collections.ObjectModel;
using PlcDiff.Core.Diff;
using PlcDiff.Core.Ladder;

namespace PlcDiff.App.Models;

public sealed class RungChangeViewModel
{
    public RungChangeViewModel(ChangeItem change)
    {
        Change = change;

        foreach (var token in LadderTokenizer.Tokenize(change.BeforeText))
        {
            BeforeTokens.Add(token);
        }

        foreach (var token in LadderTokenizer.Tokenize(change.AfterText))
        {
            AfterTokens.Add(token);
        }
    }

    public ChangeItem Change { get; }
    public ObservableCollection<LadderToken> BeforeTokens { get; } = new();
    public ObservableCollection<LadderToken> AfterTokens { get; } = new();
}
