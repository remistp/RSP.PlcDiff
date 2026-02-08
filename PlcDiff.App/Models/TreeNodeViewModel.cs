using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PlcDiff.App.Models;

public sealed class TreeNodeViewModel : ObservableObject
{
    public TreeNodeViewModel(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public ObservableCollection<TreeNodeViewModel> Children { get; } = new();
}
