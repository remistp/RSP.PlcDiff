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

            tokens.Add(new LadderToken
            {
                Instruction = instruction,
                Operand = operand
            });
        }

        return tokens;
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
