namespace PlcDiff.Core.Models;

public sealed class RoutineModel
{
    public required string Name { get; init; }
    public RoutineType Type { get; init; } = RoutineType.Lad;
    public List<RungModel> Rungs { get; } = new();
}
