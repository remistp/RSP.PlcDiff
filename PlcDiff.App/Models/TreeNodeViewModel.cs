using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PlcDiff.App.Models;

public sealed class TreeNodeViewModel : ObservableObject
{
    public TreeNodeViewModel(string name, string path, bool isRoutine)
    {
        Name = name;
        Path = path;
        IsRoutine = isRoutine;
    }

    public string Name { get; }
    public string Path { get; }
    public bool IsRoutine { get; }

    public ObservableCollection<TreeNodeViewModel> Children { get; } = new();
}
