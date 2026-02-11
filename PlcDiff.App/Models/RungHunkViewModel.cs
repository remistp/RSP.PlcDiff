using System.Collections.ObjectModel;

namespace PlcDiff.App.Models;

public sealed class RungHunkViewModel
{
    public RungHunkViewModel(string path, IEnumerable<RungChangeViewModel> changes)
    {
        Path = path;

        foreach (var change in changes)
        {
            Changes.Add(change);
        }
    }

    public string Path { get; }
    public ObservableCollection<RungChangeViewModel> Changes { get; } = new();
}
