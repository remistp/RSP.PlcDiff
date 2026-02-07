namespace PlcDiff.Core.Models;

public sealed class L5xProject
{
    public Dictionary<string, ProgramModel> Programs { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<TagModel> ControllerTags { get; } = new();
}
