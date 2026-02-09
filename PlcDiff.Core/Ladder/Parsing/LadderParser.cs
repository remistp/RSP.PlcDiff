using System.Text.RegularExpressions;

namespace PlcDiff.Core.Ladder.Parsing;

public sealed class LadderParser
{
    private static readonly Regex InstructionRegex = new(
        @"\b([A-Za-z0-9_]+)\s*\(\s*([^)]*)\s*\)",
        RegexOptions.Compiled);

    public SeriesNode Parse(string rungText)
    {
        if (rungText == null)
        {
            throw new ArgumentNullException(nameof(rungText));
        }

        var trimmed = rungText.Trim();
        if (trimmed.EndsWith(';'))
        {
            trimmed = trimmed[..^1];
        }

        var tokens = Tokenize(trimmed);
        var root = new SeriesNode();
        var currentSeries = root;
        var stack = new Stack<(SeriesNode series, ParallelNode parallel)>();

        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case LadderTokenKind.Instruction:
                    currentSeries.Children.Add(new InstructionNode(
                        token.Opcode ?? string.Empty,
                        token.Operands ?? string.Empty,
                        token.Start,
                        token.Length));
                    break;
                case LadderTokenKind.BranchStart:
                    var parallel = new ParallelNode();
                    var firstBranch = new SeriesNode();
                    parallel.Branches.Add(firstBranch);
                    stack.Push((currentSeries, parallel));
                    currentSeries = firstBranch;
                    break;
                case LadderTokenKind.BranchNext:
                    if (stack.Count == 0)
                    {
                        throw new FormatException("Unexpected ',' without an open branch.");
                    }

                    var (_, existingParallel) = stack.Peek();
                    var nextBranch = new SeriesNode();
                    existingParallel.Branches.Add(nextBranch);
                    currentSeries = nextBranch;
                    break;
                case LadderTokenKind.BranchEnd:
                    if (stack.Count == 0)
                    {
                        throw new FormatException("Unexpected ']' without an open branch.");
                    }

                    var (parentSeries, closingParallel) = stack.Pop();
                    parentSeries.Children.Add(closingParallel);
                    currentSeries = parentSeries;
                    break;
                default:
                    throw new FormatException($"Unhandled token kind {token.Kind}.");
            }
        }

        if (stack.Count > 0)
        {
            throw new FormatException("Unclosed '[' in rung text.");
        }

        return root;
    }

    private static List<LadderToken> Tokenize(string text)
    {
        var tokens = new List<LadderToken>();
        var index = 0;

        while (index < text.Length)
        {
            var ch = text[index];
            if (char.IsWhiteSpace(ch))
            {
                index++;
                continue;
            }

            if (ch == '[')
            {
                tokens.Add(new LadderToken(LadderTokenKind.BranchStart, index, 1));
                index++;
                continue;
            }

            if (ch == ',')
            {
                tokens.Add(new LadderToken(LadderTokenKind.BranchNext, index, 1));
                index++;
                continue;
            }

            if (ch == ']')
            {
                tokens.Add(new LadderToken(LadderTokenKind.BranchEnd, index, 1));
                index++;
                continue;
            }

            var match = InstructionRegex.Match(text, index);
            if (match.Success && match.Index == index)
            {
                tokens.Add(new LadderToken(
                    LadderTokenKind.Instruction,
                    match.Index,
                    match.Length,
                    match.Groups[1].Value,
                    match.Groups[2].Value));
                index += match.Length;
                continue;
            }

            throw new FormatException($"Unexpected character '{ch}' at position {index}.");
        }

        return tokens;
    }

    private enum LadderTokenKind
    {
        Instruction,
        BranchStart,
        BranchNext,
        BranchEnd
    }

    private sealed class LadderToken
    {
        public LadderToken(LadderTokenKind kind, int start, int length, string? opcode = null, string? operands = null)
        {
            Kind = kind;
            Start = start;
            Length = length;
            Opcode = opcode;
            Operands = operands;
        }

        public LadderTokenKind Kind { get; }
        public int Start { get; }
        public int Length { get; }
        public string? Opcode { get; }
        public string? Operands { get; }
    }
}
