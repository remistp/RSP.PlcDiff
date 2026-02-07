namespace PlcDiff.Core.Models;

public sealed class ProgramModel
{
    public required string Name { get; init; }
    public Dictionary<string, RoutineModel> Routines { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<TagModel> ProgramTags { get; } = new();
}
