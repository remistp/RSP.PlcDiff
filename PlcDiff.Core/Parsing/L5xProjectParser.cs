using System.Xml;
using PlcDiff.Core.Models;
using PlcDiff.Core.Text;

namespace PlcDiff.Core.Parsing;

public sealed class L5xProjectParser
{
    public L5xProject Parse(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is required.", nameof(path));
        }

        var project = new L5xProject();
        ProgramModel? currentProgram = null;
        RoutineModel? currentRoutine = null;
        var rungIndex = 0;
        var inController = false;
        var inControllerTags = false;
        var inProgramTags = false;

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        using var stream = File.OpenRead(path);
        using var reader = XmlReader.Create(stream, settings);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var elementName = reader.Name;

                if (elementName.Equals("Controller", StringComparison.OrdinalIgnoreCase))
                {
                    inController = true;
                    continue;
                }

                if (elementName.Equals("Program", StringComparison.OrdinalIgnoreCase))
                {
                    var programName = reader.GetAttribute("Name") ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(programName))
                    {
                        currentProgram = new ProgramModel { Name = programName };
                        project.Programs[programName] = currentProgram;
                    }

                    continue;
                }

                if (elementName.Equals("Routine", StringComparison.OrdinalIgnoreCase))
                {
                    var routineName = reader.GetAttribute("Name") ?? string.Empty;
                    var routineType = reader.GetAttribute("Type");
                    var isLad = string.Equals(routineType, "RLL", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(routineType, "LAD", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(routineType, "Ladder", StringComparison.OrdinalIgnoreCase);

                    if (!string.IsNullOrWhiteSpace(routineName) && isLad && currentProgram != null)
                    {
                        currentRoutine = new RoutineModel
                        {
                            Name = routineName,
                            Type = RoutineType.Lad
                        };
                        currentProgram.Routines[routineName] = currentRoutine;
                        rungIndex = 0;
                    }
                    else
                    {
                        currentRoutine = null;
                    }

                    continue;
                }

                if (elementName.Equals("Tags", StringComparison.OrdinalIgnoreCase))
                {
                    if (currentProgram != null)
                    {
                        inProgramTags = true;
                    }
                    else if (inController)
                    {
                        inControllerTags = true;
                    }

                    continue;
                }

                if (elementName.Equals("Tag", StringComparison.OrdinalIgnoreCase)
                    && (inControllerTags || inProgramTags))
                {
                    var tag = ReadTag(reader, inProgramTags ? TagScope.Program : TagScope.Controller);
                    if (tag != null)
                    {
                        if (inProgramTags && currentProgram != null)
                        {
                            currentProgram.ProgramTags.Add(tag);
                        }
                        else
                        {
                            project.ControllerTags.Add(tag);
                        }
                    }

                    continue;
                }

                if (elementName.Equals("Rung", StringComparison.OrdinalIgnoreCase) && currentRoutine != null)
                {
                    var rung = ReadRung(reader, rungIndex);
                    if (rung != null)
                    {
                        currentRoutine.Rungs.Add(rung);
                        rungIndex++;
                    }

                    continue;
                }
            }

            if (reader.NodeType == XmlNodeType.EndElement)
            {
                var elementName = reader.Name;

                if (elementName.Equals("Program", StringComparison.OrdinalIgnoreCase))
                {
                    currentProgram = null;
                }

                if (elementName.Equals("Routine", StringComparison.OrdinalIgnoreCase))
                {
                    currentRoutine = null;
                }

                if (elementName.Equals("Tags", StringComparison.OrdinalIgnoreCase))
                {
                    inControllerTags = false;
                    inProgramTags = false;
                }

                if (elementName.Equals("Controller", StringComparison.OrdinalIgnoreCase))
                {
                    inController = false;
                }
            }
        }

        return project;
    }

    private static RungModel? ReadRung(XmlReader reader, int index)
    {
        if (reader.IsEmptyElement)
        {
            return null;
        }

        var rockwellId = reader.GetAttribute("ID")
            ?? reader.GetAttribute("Id")
            ?? reader.GetAttribute("Number");
        string? rawText = null;

        using var subtree = reader.ReadSubtree();
        subtree.Read();
        while (subtree.Read())
        {
            if (subtree.NodeType == XmlNodeType.Element
                && subtree.Name.Equals("Text", StringComparison.OrdinalIgnoreCase))
            {
                rawText = subtree.ReadElementContentAsString();
                break;
            }
        }

        reader.Read();

        rawText ??= string.Empty;
        var normalized = RungTextNormalizer.Normalize(rawText);
        var key = Hashing.ComputeKey(normalized, rockwellId);

        return new RungModel
        {
            RockwellId = rockwellId,
            RawText = rawText,
            NormalizedText = normalized,
            Key = key,
            Index = index
        };
    }

    private static TagModel? ReadTag(XmlReader reader, TagScope scope)
    {
        var name = reader.GetAttribute("Name");
        var dataType = reader.GetAttribute("DataType");

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(dataType))
        {
            if (!reader.IsEmptyElement)
            {
                reader.Skip();
            }

            return null;
        }

        string? initialValue = null;

        if (!reader.IsEmptyElement)
        {
            using var subtree = reader.ReadSubtree();
            subtree.Read();
            while (subtree.Read())
            {
                if (subtree.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (subtree.Name.Equals("Data", StringComparison.OrdinalIgnoreCase)
                    || subtree.Name.Equals("DataValue", StringComparison.OrdinalIgnoreCase)
                    || subtree.Name.Equals("Value", StringComparison.OrdinalIgnoreCase))
                {
                    initialValue = subtree.ReadElementContentAsString();
                    break;
                }
            }

            reader.Read();
        }

        return new TagModel
        {
            Name = name,
            DataType = dataType,
            InitialValue = string.IsNullOrWhiteSpace(initialValue) ? null : initialValue,
            Scope = scope
        };
    }
}
