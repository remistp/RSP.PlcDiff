using System.Text.RegularExpressions;

namespace PlcDiff.Core.Ladder;

public static class LadderTokenizer
{
    private static readonly Regex InstructionRegex = new(
        @"\b(XIC|XIO|OTE|TON)\s*\(\s*([^)]*)\s*\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static IReadOnlyList<LadderToken> Tokenize(string? rungText)
    {
        if (string.IsNullOrWhiteSpace(rungText))
        {
            return Array.Empty<LadderToken>();
        }

        var branchDepths = GetBranchDepths(rungText);
        var tokens = new List<LadderToken>();

        foreach (Match match in InstructionRegex.Matches(rungText))
        {
            if (!match.Success || match.Groups.Count < 3)
            {
                continue;
            }

            var instructionText = match.Groups[1].Value;
            if (!TryParseInstruction(instructionText, out var instruction))
            {
                continue;
            }

            var operand = match.Groups[2].Value.Trim();
            if (string.IsNullOrWhiteSpace(operand))
            {
                continue;
            }

            var depth = 0;
            if (branchDepths.TryGetValue(match.Index, out var matchDepth))
            {
                depth = matchDepth;
            }

            tokens.Add(new LadderToken
            {
                Instruction = instruction,
                Operand = operand,
                BranchDepth = depth
            });
        }

        return tokens;
    }

    private static Dictionary<int, int> GetBranchDepths(string rungText)
    {
        var depths = new Dictionary<int, int>();
        var depth = 0;

        for (var i = 0; i < rungText.Length; i++)
        {
            var ch = rungText[i];
            switch (ch)
            {
                case '[':
                    depth++;
                    break;
                case ']':
                    depth = Math.Max(0, depth - 1);
                    break;
            }

            if (IsInstructionStart(rungText, i))
            {
                depths[i] = depth;
            }
        }

        return depths;
    }

    private static bool IsInstructionStart(string text, int index)
    {
        if (index < 0 || index + 2 >= text.Length)
        {
            return false;
        }

        var remaining = text.Length - index;
        if (remaining < 3)
        {
            return false;
        }

        var slice = text.Substring(index, Math.Min(3, remaining)).ToUpperInvariant();
        return slice.StartsWith("XIC", StringComparison.Ordinal)
            || slice.StartsWith("XIO", StringComparison.Ordinal)
            || slice.StartsWith("OTE", StringComparison.Ordinal)
            || slice.StartsWith("TON", StringComparison.Ordinal);
    }

    private static bool TryParseInstruction(string text, out LadderInstructionType instruction)
    {
        switch (text.ToUpperInvariant())
        {
            case "XIC":
                instruction = LadderInstructionType.Xic;
                return true;
            case "XIO":
                instruction = LadderInstructionType.Xio;
                return true;
            case "OTE":
                instruction = LadderInstructionType.Ote;
                return true;
            case "TON":
                instruction = LadderInstructionType.Ton;
                return true;
            default:
                instruction = default;
                return false;
        }
    }
}
