using PlcDiff.Core.Models;

namespace PlcDiff.Core.Diff;

public sealed class ProjectDiffer
{
    public DiffReport Compare(L5xProject projectA, L5xProject projectB)
    {
        if (projectA == null)
        {
            throw new ArgumentNullException(nameof(projectA));
        }

        if (projectB == null)
        {
            throw new ArgumentNullException(nameof(projectB));
        }

        var report = new DiffReport();
        var modifiedPrograms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var modifiedRoutines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        ComparePrograms(projectA, projectB, report, modifiedPrograms, modifiedRoutines);
        CompareControllerTags(projectA, projectB, report);

        foreach (var programName in modifiedPrograms)
        {
            report.Changes.Add(new ChangeItem
            {
                Type = ChangeItemType.Program,
                Path = programName,
                ChangeKind = ChangeKind.Modified,
                Summary = $"Program modified: {programName}"
            });
        }

        foreach (var routinePath in modifiedRoutines)
        {
            report.Changes.Add(new ChangeItem
            {
                Type = ChangeItemType.Routine,
                Path = routinePath,
                ChangeKind = ChangeKind.Modified,
                Summary = $"Routine modified: {routinePath}"
            });
        }

        return report;
    }

    private static void ComparePrograms(
        L5xProject projectA,
        L5xProject projectB,
        DiffReport report,
        HashSet<string> modifiedPrograms,
        HashSet<string> modifiedRoutines)
    {
        foreach (var (name, programA) in projectA.Programs)
        {
            if (!projectB.Programs.TryGetValue(name, out var programB))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Program,
                    Path = name,
                    ChangeKind = ChangeKind.Removed,
                    Summary = $"Program removed: {name}"
                });
                continue;
            }

            if (CompareProgramTags(programA, programB, report, name))
            {
                modifiedPrograms.Add(name);
            }

            if (CompareRoutines(programA, programB, report, name, modifiedPrograms, modifiedRoutines))
            {
                modifiedPrograms.Add(name);
            }
        }

        foreach (var (name, _) in projectB.Programs)
        {
            if (!projectA.Programs.ContainsKey(name))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Program,
                    Path = name,
                    ChangeKind = ChangeKind.Added,
                    Summary = $"Program added: {name}"
                });
            }
        }
    }

    private static bool CompareRoutines(
        ProgramModel programA,
        ProgramModel programB,
        DiffReport report,
        string programName,
        HashSet<string> modifiedPrograms,
        HashSet<string> modifiedRoutines)
    {
        var anyChanges = false;

        foreach (var (routineName, routineA) in programA.Routines)
        {
            if (!programB.Routines.TryGetValue(routineName, out var routineB))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Routine,
                    Path = $"{programName}/{routineName}",
                    ChangeKind = ChangeKind.Removed,
                    Summary = $"Routine removed: {programName}/{routineName}"
                });
                anyChanges = true;
                continue;
            }

            if (CompareRungs(routineA, routineB, report, programName, routineName))
            {
                modifiedRoutines.Add($"{programName}/{routineName}");
                anyChanges = true;
            }
        }

        foreach (var (routineName, _) in programB.Routines)
        {
            if (!programA.Routines.ContainsKey(routineName))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Routine,
                    Path = $"{programName}/{routineName}",
                    ChangeKind = ChangeKind.Added,
                    Summary = $"Routine added: {programName}/{routineName}"
                });
                anyChanges = true;
            }
        }

        return anyChanges;
    }

    private static bool CompareRungs(
        RoutineModel routineA,
        RoutineModel routineB,
        DiffReport report,
        string programName,
        string routineName)
    {
        var anyChanges = false;
        var rungsA = routineA.Rungs.ToDictionary(rung => rung.Key, StringComparer.OrdinalIgnoreCase);
        var rungsB = routineB.Rungs.ToDictionary(rung => rung.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var (key, rungA) in rungsA)
        {
            if (!rungsB.TryGetValue(key, out var rungB))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Rung,
                    Path = $"{programName}/{routineName}",
                    ChangeKind = ChangeKind.Removed,
                    Summary = $"Rung removed at index {rungA.Index}"
                });
                anyChanges = true;
                continue;
            }

            if (!string.Equals(rungA.NormalizedText, rungB.NormalizedText, StringComparison.Ordinal))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Rung,
                    Path = $"{programName}/{routineName}",
                    ChangeKind = ChangeKind.Modified,
                    Summary = $"Rung modified at index {rungA.Index}"
                });
                anyChanges = true;
                continue;
            }

            if (rungA.Index != rungB.Index)
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Rung,
                    Path = $"{programName}/{routineName}",
                    ChangeKind = ChangeKind.Moved,
                    Summary = $"Rung moved from {rungA.Index} to {rungB.Index}"
                });
                anyChanges = true;
            }
        }

        foreach (var (key, rungB) in rungsB)
        {
            if (!rungsA.ContainsKey(key))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Rung,
                    Path = $"{programName}/{routineName}",
                    ChangeKind = ChangeKind.Added,
                    Summary = $"Rung added at index {rungB.Index}"
                });
                anyChanges = true;
            }
        }

        return anyChanges;
    }

    private static bool CompareProgramTags(
        ProgramModel programA,
        ProgramModel programB,
        DiffReport report,
        string programName)
    {
        var anyChanges = false;
        var tagsA = programA.ProgramTags.ToDictionary(tag => tag.Name, StringComparer.OrdinalIgnoreCase);
        var tagsB = programB.ProgramTags.ToDictionary(tag => tag.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var (name, tagA) in tagsA)
        {
            if (!tagsB.TryGetValue(name, out var tagB))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Tag,
                    Path = programName,
                    ChangeKind = ChangeKind.Removed,
                    Summary = $"Tag removed: {name}"
                });
                anyChanges = true;
                continue;
            }

            if (!TagEquals(tagA, tagB))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Tag,
                    Path = programName,
                    ChangeKind = ChangeKind.Modified,
                    Summary = $"Tag modified: {name}"
                });
                anyChanges = true;
            }
        }

        foreach (var (name, tagB) in tagsB)
        {
            if (!tagsA.ContainsKey(name))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Tag,
                    Path = programName,
                    ChangeKind = ChangeKind.Added,
                    Summary = $"Tag added: {name}"
                });
                anyChanges = true;
            }
        }

        return anyChanges;
    }

    private static void CompareControllerTags(L5xProject projectA, L5xProject projectB, DiffReport report)
    {
        var tagsA = projectA.ControllerTags.ToDictionary(tag => tag.Name, StringComparer.OrdinalIgnoreCase);
        var tagsB = projectB.ControllerTags.ToDictionary(tag => tag.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var (name, tagA) in tagsA)
        {
            if (!tagsB.TryGetValue(name, out var tagB))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Tag,
                    Path = "Controller",
                    ChangeKind = ChangeKind.Removed,
                    Summary = $"Controller tag removed: {name}"
                });
                continue;
            }

            if (!TagEquals(tagA, tagB))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Tag,
                    Path = "Controller",
                    ChangeKind = ChangeKind.Modified,
                    Summary = $"Controller tag modified: {name}"
                });
            }
        }

        foreach (var (name, _) in tagsB)
        {
            if (!tagsA.ContainsKey(name))
            {
                report.Changes.Add(new ChangeItem
                {
                    Type = ChangeItemType.Tag,
                    Path = "Controller",
                    ChangeKind = ChangeKind.Added,
                    Summary = $"Controller tag added: {name}"
                });
            }
        }
    }

    private static bool TagEquals(TagModel tagA, TagModel tagB)
    {
        return string.Equals(tagA.DataType, tagB.DataType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(tagA.InitialValue ?? string.Empty, tagB.InitialValue ?? string.Empty, StringComparison.Ordinal)
            && tagA.Scope == tagB.Scope;
    }
}
