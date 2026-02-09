using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PlcDiff.App.Models;
using PlcDiff.Core.Diff;
using PlcDiff.Core.Ladder;
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
    private TreeNodeViewModel? _selectedTreeNode;

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
    public ObservableCollection<LadderToken> BeforeTokens { get; } = new();
    public ObservableCollection<LadderToken> AfterTokens { get; } = new();
    public ObservableCollection<RungRowViewModel> RungRows { get; } = new();

    public string? HunkHeader { get; private set; }

    public ChangeItem? SelectedChange
    {
        get => _selectedChange;
        set
        {
            if (SetProperty(ref _selectedChange, value))
            {
                UpdateRoutineHunk();
            }
        }
    }

    public TreeNodeViewModel? SelectedTreeNode
    {
        get => _selectedTreeNode;
        set
        {
            if (SetProperty(ref _selectedTreeNode, value))
            {
                UpdateRoutineHunk();
            }
        }
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
        OnPropertyChanged(nameof(ChangeItems));
        UpdateRoutineHunk();
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
            var programNode = new TreeNodeViewModel(program.Name, program.Name, false);
            foreach (var routine in program.Routines.Values.OrderBy(routine => routine.Name))
            {
                programNode.Children.Add(new TreeNodeViewModel(routine.Name, $"{program.Name}/{routine.Name}", true));
            }

            ProgramTree.Add(programNode);
        }
    }

    private void ClearDiff()
    {
        ChangeItems.Clear();
        SelectedChange = null;
        CompareCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(ChangeItems));
    }

    private void UpdateTokens()
    {
        BeforeTokens.Clear();
        AfterTokens.Clear();
        RungRows.Clear();

        if (SelectedChange == null)
        {
            return;
        }

        foreach (var token in LadderTokenizer.Tokenize(SelectedChange.BeforeText))
        {
            BeforeTokens.Add(token);
        }

        foreach (var token in LadderTokenizer.Tokenize(SelectedChange.AfterText))
        {
            AfterTokens.Add(token);
        }
    }

    private void UpdateRoutineHunk()
    {
        RungRows.Clear();
        HunkHeader = null;
        OnPropertyChanged(nameof(HunkHeader));

        var path = SelectedTreeNode?.IsRoutine == true
            ? SelectedTreeNode.Path
            : SelectedChange?.Path;

        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var segments = path.Split('/');
        if (segments.Length != 2)
        {
            return;
        }

        var programName = segments[0];
        var routineName = segments[1];
        var routineA = GetRoutine(_projectA, programName, routineName);
        var routineB = GetRoutine(_projectB, programName, routineName);

        var rungsA = routineA?.Rungs.ToDictionary(rung => rung.Index) ?? new Dictionary<int, RungModel>();
        var rungsB = routineB?.Rungs.ToDictionary(rung => rung.Index) ?? new Dictionary<int, RungModel>();

        if (rungsA.Count == 0 && rungsB.Count == 0)
        {
            return;
        }

        var minIndex = rungsA.Keys.Concat(rungsB.Keys).DefaultIfEmpty(0).Min();
        var maxIndex = rungsA.Keys.Concat(rungsB.Keys).DefaultIfEmpty(0).Max();

        for (var index = minIndex; index <= maxIndex; index++)
        {
            rungsA.TryGetValue(index, out var rungA);
            rungsB.TryGetValue(index, out var rungB);

            if (rungA != null && rungB != null)
            {
                if (string.Equals(rungA.NormalizedText, rungB.NormalizedText, StringComparison.Ordinal))
                {
                    RungRows.Add(new RungRowViewModel(index, ChangeKind.None, rungA.RawText));
                }
                else
                {
                    RungRows.Add(new RungRowViewModel(index, ChangeKind.Removed, rungA.RawText));
                    RungRows.Add(new RungRowViewModel(index, ChangeKind.Added, rungB.RawText));
                }
            }
            else if (rungA != null)
            {
                RungRows.Add(new RungRowViewModel(index, ChangeKind.Removed, rungA.RawText));
            }
            else if (rungB != null)
            {
                RungRows.Add(new RungRowViewModel(index, ChangeKind.Added, rungB.RawText));
            }
        }

        HunkHeader = $"HUNK 1: RUNGS {minIndex + 1}-{maxIndex + 1}";
        OnPropertyChanged(nameof(HunkHeader));
    }

    private static RoutineModel? GetRoutine(L5xProject? project, string programName, string routineName)
    {
        if (project == null)
        {
            return null;
        }

        return project.Programs.TryGetValue(programName, out var program)
            && program.Routines.TryGetValue(routineName, out var routine)
            ? routine
            : null;
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
