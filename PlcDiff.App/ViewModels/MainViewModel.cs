using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PlcDiff.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    public IRelayCommand OpenProjectACommand { get; }
    public IRelayCommand OpenProjectBCommand { get; }
    public IRelayCommand CompareCommand { get; }

    public MainViewModel()
    {
        OpenProjectACommand = new RelayCommand(OnOpenProjectA);
        OpenProjectBCommand = new RelayCommand(OnOpenProjectB);
        CompareCommand = new RelayCommand(OnCompare);
    }

    private void OnOpenProjectA()
    {
    }

    private void OnOpenProjectB()
    {
    }

    private void OnCompare()
    {
    }
}
