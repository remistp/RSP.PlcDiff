using System.Text.RegularExpressions;

namespace PlcDiff.Core.Ladder;

public static class LadderTokenizer
{
    private static readonly Regex InstructionRegex = new(
        @"\b(XIC|XIO|OTE|TON|MOV)\s*\(\s*([^)]*)\s*\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static IReadOnlyList<LadderToken> Tokenize(string? rungText)
    {
        if (string.IsNullOrWhiteSpace(rungText))
        {
            return Array.Empty<LadderToken>();
        }

        var tokens = new List<LadderToken>();
        var matches = InstructionRegex.Matches(rungText).Cast<Match>().ToList();
        var branchDepths = GetBranchDepths(rungText, matches);

        foreach (var match in matches)
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
        return new Dictionary<int, int>();
    }

    private static Dictionary<int, int> GetBranchDepths(string rungText, IReadOnlyList<Match> matches)
    {
        var depths = new Dictionary<int, int>();
        var depthLevel = 0;
        var branchRows = new Stack<int>();
        var position = 0;

        foreach (var match in matches)
        {
            for (var i = position; i < match.Index && i < rungText.Length; i++)
            {
                var ch = rungText[i];
                switch (ch)
                {
                    case '[':
                        depthLevel++;
                        branchRows.Push(0);
                        break;
                    case ',':
                        if (branchRows.Count > 0)
                        {
                            var current = branchRows.Pop();
                            branchRows.Push(current + 1);
                        }
                        break;
                    case ']':
                        if (branchRows.Count > 0)
                        {
                            branchRows.Pop();
                        }
                        depthLevel = Math.Max(0, depthLevel - 1);
                        break;
                }
            }

            var rowOffset = branchRows.Count > 0 ? branchRows.Peek() : 0;
            var computedDepth = Math.Max(0, rowOffset);

            depths[match.Index] = computedDepth;
            position = match.Index;
        }

        return depths;
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
            case "MOV":
                instruction = LadderInstructionType.Mov;
                return true;
            default:
                instruction = default;
                return false;
        }
    }
}
