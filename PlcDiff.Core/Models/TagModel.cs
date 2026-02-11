namespace PlcDiff.Core.Models;

public sealed class TagModel
{
    public required string Name { get; init; }
    public required string DataType { get; init; }
    public string? InitialValue { get; init; }
    public TagScope Scope { get; init; }
}
