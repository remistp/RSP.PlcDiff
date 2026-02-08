using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PlcDiff.App.Models;
using PlcDiff.Core.Diff;
using PlcDiff.Core.Models;
using PlcDiff.Core.Parsing;

namespace PlcDiff.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly L5xProjectParser _parser = new();
    private readonly ProjectDiffer _differ = new();

    private L5xProject? _projectA;
    private L5xProject? _projectB;
    private ChangeItem? _selectedChange;

    public MainViewModel()
    {
        OpenProjectACommand = new RelayCommand(OnOpenProjectA);
        OpenProjectBCommand = new RelayCommand(OnOpenProjectB);
        CompareCommand = new RelayCommand(OnCompare, CanCompare);
    }

    public IRelayCommand OpenProjectACommand { get; }
    public IRelayCommand OpenProjectBCommand { get; }
    public IRelayCommand CompareCommand { get; }

    public ObservableCollection<TreeNodeViewModel> ProgramTree { get; } = new();
    public ObservableCollection<ChangeItem> ChangeItems { get; } = new();

    public ChangeItem? SelectedChange
    {
        get => _selectedChange;
        set => SetProperty(ref _selectedChange, value);
    }

    private void OnOpenProjectA()
    {
        var path = ShowOpenFileDialog();
        if (path == null)
        {
            return;
        }

        _projectA = _parser.Parse(path);
        RefreshTree();
        ClearDiff();
        CompareCommand.NotifyCanExecuteChanged();
    }

    private void OnOpenProjectB()
    {
        var path = ShowOpenFileDialog();
        if (path == null)
        {
            return;
        }

        _projectB = _parser.Parse(path);
        RefreshTree();
        ClearDiff();
        CompareCommand.NotifyCanExecuteChanged();
    }

    private bool CanCompare()
    {
        return _projectA != null && _projectB != null;
    }

    private void OnCompare()
    {
        if (!CanCompare())
        {
            return;
        }

        var report = _differ.Compare(_projectA!, _projectB!);
        ChangeItems.Clear();
        foreach (var change in report.Changes.OrderBy(change => change.Path))
        {
            ChangeItems.Add(change);
        }

        SelectedChange = ChangeItems.FirstOrDefault();
    }

    private void RefreshTree()
    {
        ProgramTree.Clear();

        var project = _projectA ?? _projectB;
        if (project == null)
        {
            return;
        }

        foreach (var program in project.Programs.Values.OrderBy(program => program.Name))
        {
            var programNode = new TreeNodeViewModel(program.Name);
            foreach (var routine in program.Routines.Values.OrderBy(routine => routine.Name))
            {
                programNode.Children.Add(new TreeNodeViewModel(routine.Name));
            }

            ProgramTree.Add(programNode);
        }
    }

    private void ClearDiff()
    {
        ChangeItems.Clear();
        SelectedChange = null;
        CompareCommand.NotifyCanExecuteChanged();
    }

    private static string? ShowOpenFileDialog()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "L5X files (*.L5X;*.l5x)|*.L5X;*.l5x|All files (*.*)|*.*",
            CheckFileExists = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
