/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Text.RegularExpressions;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommentedOutCode : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S125";
    internal const string MessageFormat = "Remove this commented out code.";
    private const int CommentMarkLength = 2;

    // These values were tuned against unit tests and Peach, resulting in 1.13% FP and 2.01% FN among reports.
    private const double MinimumThreshold = 0.87;
    private const double NaturalLanguageWeight = 0.005;
    private const double WordWeight = 0.010;
    private const int WordCountCap = 9;
    private const double MaximumThreshold = 0.99;

    internal static readonly string[] WeakKeywordNames =
    [
        "is", "as", "in", "case", "if", "for", "while", "else", "true", "false", "null", "default", "do", "this", "base", "event", "out",
    ];

    // Subset of contextual keywords that are uncommon in prose.
    internal static readonly string[] ContextualKeywordNames = ["var", "async", "await", "yield", "nameof", "nint", "nuint", "notnull"];

    internal static readonly Detector BlockBoundaryAtLineEnd = CreateDetector(0.95, @"(?:\A\}|[;{]\}|\{)\z", ignoreWhitespace: true);

    internal static readonly Detector ControlFlowStart = CreateDetector(
        0.95,
        @"(?<=\A(?:[{}();,*\-/=<>!&|?:]|else|do|try|return|yield|await|case|default|break)*)(?:if\(|for\(|while\(|catch\(|switch\(|try\{|else\{)",
        ignoreWhitespace: true);

    internal static readonly Detector Increment = CreateDetector(0.95, CreateDelimitedTokenPattern(["++"]));
    internal static readonly Detector LogicalOperators = CreateDetector(0.55, CreateDelimitedTokenPattern(["&&", "||", "??"]));
    internal static readonly Detector TrailingSemicolon = CreateDetector(0.90, @";\s*\z");

    internal static readonly Detector Assignment = CreateDetector(0.60, @"(?:(?<![=!<>])(?<!\[\^)|(?<=<<|>>))=(?![=>]).*\z");

    internal static readonly Detector Comparison = CreateDetector(0.45, @"(?<!=)[!<>=]=(?!=)", ignoreWhitespace: true);

    internal static readonly Detector Call = CreateDetector(0.35, @"(?<![\p{L}\p{Nd}_])(?!(?:if|for|while|catch|switch)\()[\p{L}\p{Nd}_]+\(|>\(");

    internal static readonly Detector StrongKeywords = CreateDetector(
        0.35,
        CreateDelimitedTokenPattern(SyntaxFacts.GetKeywordKinds().Select(SyntaxFacts.GetText)
            .Where(x => SyntaxFacts.GetKeywordKind(x) != SyntaxKind.None)
            .Concat(ContextualKeywordNames)
            .Except(WeakKeywordNames, StringComparer.Ordinal)));
    internal static readonly Detector WeakKeywords = CreateDetector(0.10, CreateDelimitedTokenPattern(WeakKeywordNames));

    private static readonly Detector[] Detectors =
    [
        BlockBoundaryAtLineEnd, ControlFlowStart, Increment, LogicalOperators, TrailingSemicolon, Assignment, Comparison, Call, StrongKeywords, WeakKeywords,
    ];

    private static readonly Regex TaskTagPattern = new(
        @"\A[\w \t*/\-\[\]:.]*?\b(?:TODO|FIXME|HACK|XXX)(?=:|\s+\p{L}|\s*\z)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        Constants.DefaultRegexTimeout);

    private static readonly Regex StringLiteralPattern = new("\".*?\"", RegexOptions.None, Constants.DefaultRegexTimeout);

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    internal static bool IsCode(string line)
    {
        var lineWithoutWhitespace = RemoveWhitespace(line);
        if (IsExcluded(line, lineWithoutWhitespace))
        {
            return false;
        }
        var codeScore = CalculateCodeScore(line, lineWithoutWhitespace);
        return codeScore > MinimumThreshold && codeScore > CalculateThreshold(line);
    }

    internal static double CalculateCodeScore(string line, string lineWithoutWhitespace)
    {
        var probability = 0d;
        foreach (var detector in Detectors)
        {
            var matchCount = CountMatches(detector, line, lineWithoutWhitespace);
            var detectorProbability = 1d - Math.Pow(1d - detector.Weight, matchCount);
            probability = 1d - ((1d - probability) * (1d - detectorProbability));
        }
        return probability;
    }

    internal static int CountMatches(Detector detector, string line, string lineWithoutWhitespace)
    {
        var input = detector.IgnoreWhitespace ? lineWithoutWhitespace : line;
        // In testing this reduces allocations from this rule by ~90%
        if (!detector.Regex.SafeIsMatch(input))
        {
            return 0;
        }
        return detector.Regex.SafeMatches(input).Count;
    }

    internal static string RemoveWhitespace(string line) =>
        new(line.Where(x => !char.IsWhiteSpace(x)).ToArray());

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterTreeAction(c =>
            {
                foreach (var token in c.Tree.GetRoot().DescendantTokens())
                {
                    CheckTrivia(c, token.LeadingTrivia);
                    CheckTrivia(c, token.TrailingTrivia);
                }
            });

    private static double CalculateThreshold(string line)
    {
        var lineWithoutStringLiterals = WithoutStringLiterals(line);
        return Math.Min(
            MinimumThreshold
            + (NaturalLanguageWeight * NaturalLanguageDetector.HumanLanguageScore(lineWithoutStringLiterals))
            + (WordWeight * Math.Min(WordCount(lineWithoutStringLiterals), WordCountCap)),
            MaximumThreshold);
    }

    // Leave lines with unpaired quotes unchanged.
    private static string WithoutStringLiterals(string line) =>
        line.Count(x => x == '"') % 2 == 0
            ? StringLiteralPattern.SafeReplace(line, " ")
            : line;

    private static void CheckTrivia(SonarSyntaxTreeReportingContext context, SyntaxTriviaList trivia)
    {
        var shouldReport = true;
        foreach (var trivium in trivia)
        {
            // comment start is checked because of  https://github.com/dotnet/roslyn/issues/10003
            if (trivium.IsKind(SyntaxKind.MultiLineCommentTrivia) && !trivium.ToFullString().TrimStart().StartsWith("/**", StringComparison.Ordinal))
            {
                CheckMultilineComment(context, trivium);
                shouldReport = true;
            }
            else if (shouldReport
                && trivium.IsKind(SyntaxKind.SingleLineCommentTrivia)
                && !trivium.ToFullString().TrimStart().StartsWith("///", StringComparison.Ordinal)
                && IsCode(trivium.ToString().Substring(CommentMarkLength)))
            {
                context.ReportIssue(Rule, trivium.GetLocation());
                shouldReport = false;
            }
        }
    }

    private static void CheckMultilineComment(SonarSyntaxTreeReportingContext context, SyntaxTrivia trivia)
    {
        var triviaLines = TriviaContent().Split(Constants.LineTerminators, StringSplitOptions.None);
        var triviaLineNumber = Array.FindIndex(triviaLines, IsCode);
        if (triviaLineNumber >= 0)
        {
            var lineNumber = trivia.GetLocation().StartLine + triviaLineNumber;
            var lineSpan = context.Tree.GetText(context.Cancel).Lines[lineNumber].Span;
            var commentLineSpan = lineSpan.Intersection(trivia.GetLocation().SourceSpan);
            var location = Location.Create(context.Tree, commentLineSpan ?? lineSpan);
            context.ReportIssue(Rule, location);
        }

        string TriviaContent()
        {
            var content = trivia.ToString().Substring(CommentMarkLength);
            return content.EndsWith("*/", StringComparison.Ordinal) ? content.Substring(0, content.Length - CommentMarkLength) : content;
        }
    }

    private static bool IsExcluded(string line, string lineWithoutWhitespace) =>
        lineWithoutWhitespace.Contains("License")
        || lineWithoutWhitespace.Contains("c++")
        || lineWithoutWhitespace.Contains("C++")
        || TaskTagPattern.SafeIsMatch(line);

    private static int WordCount(string line) =>
        line.Split().Count(x => x.Any(char.IsLetter));

    private static Detector CreateDetector(double weight, string pattern, bool ignoreWhitespace = false) =>
        new(weight, new Regex(pattern, RegexOptions.None, Constants.DefaultRegexTimeout), ignoreWhitespace);

    private static string CreateDelimitedTokenPattern(IEnumerable<string> tokens) =>
        @"(?<=\A|[ \t(),{}])(?:" + string.Join("|", tokens.Select(Regex.Escape)) + @")(?=\z|[ \t(),{}])";

    internal sealed record Detector(double Weight, Regex Regex, bool IgnoreWhitespace);
}
