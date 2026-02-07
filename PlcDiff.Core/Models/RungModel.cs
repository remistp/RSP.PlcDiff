namespace PlcDiff.Core.Models;

public sealed class RungModel
{
    public string? RockwellId { get; init; }
    public required string RawText { get; init; }
    public required string NormalizedText { get; init; }
    public required string Key { get; init; }
    public int Index { get; init; }
}
