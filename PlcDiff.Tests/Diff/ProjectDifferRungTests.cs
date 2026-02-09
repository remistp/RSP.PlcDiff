using PlcDiff.Core.Diff;
using PlcDiff.Core.Models;
using Xunit;

namespace PlcDiff.Tests.Diff;

public sealed class ProjectDifferRungTests
{
    [Fact]
    public void Compare_ReportsRungModificationByIndexWhenKeysDiffer()
    {
        var projectA = BuildProject(new[]
        {
            (0, "XIC(Input1_2)OTE(Output1_2);"),
            (1, "[XIC(Input2_2) ,XIC(Input3_2) ]OTE(Output2_2);")
        });

        var projectB = BuildProject(new[]
        {
            (0, "XIC(Input1_2)OTE(Output1_2);"),
            (1, "[XIC(Input2) XIC(Input1_2) ,XIC(Input3_2) ]OTE(Output2_2);"),
            (2, "[XIC(Input2) ,XIC(Input3) ]OTE(Output2);")
        });

        var report = new ProjectDiffer().Compare(projectA, projectB);

        Assert.Contains(report.Changes, change =>
            change.Type == ChangeItemType.Rung
            && change.ChangeKind == ChangeKind.Modified
            && change.Summary.Contains("index 1", StringComparison.Ordinal));
    }

    private static L5xProject BuildProject(IEnumerable<(int index, string text)> rungs)
    {
        var project = new L5xProject();
        var program = new ProgramModel { Name = "MainProgram" };
        var routine = new RoutineModel { Name = "MainRoutine2" };

        foreach (var (index, text) in rungs)
        {
            routine.Rungs.Add(new RungModel
            {
                Index = index,
                RawText = text,
                NormalizedText = text,
                Key = text,
                RockwellId = null
            });
        }

        program.Routines[routine.Name] = routine;
        project.Programs[program.Name] = program;

        return project;
    }
}
