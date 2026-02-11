namespace PlcDiff.Core.Models;

public enum RoutineType
{
    Lad
}

public enum TagScope
{
    Controller,
    Program
}

public enum ChangeKind
{
    None,
    Added,
    Removed,
    Modified,
    Moved
}

public enum ChangeItemType
{
    Program,
    Routine,
    Rung,
    Tag
}
