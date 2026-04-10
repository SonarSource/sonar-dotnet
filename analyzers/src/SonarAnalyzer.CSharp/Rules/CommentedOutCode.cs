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

    private static readonly string[] CodeEndings = ["{", ";}", "{}"];
    private static readonly string[] CodeParts = ["++", "catch(", "switch(", "try{", "else{"];
    private static readonly string[] CodePartsWithRelationalOperator = ["for(", "if(", "while("];
    private static readonly string[] RelationalOperators = ["<", ">", "<=", ">=", "==", "!="];

    // Groups 1 and 2 capture the first two words for keyword detection.
    private static readonly Regex SentencePattern =
        new(
            @"^\s*(?:[*\-]|->|=>)?\s*(\w+)[.,?:!']*\s+(\w+)[.,?:!']*\s+(?:\w+[.,?:!']*\s+)*\w+[.,?:!']*$",
            RegexOptions.None,
            Constants.DefaultRegexTimeout);

    private static readonly HashSet<string> CodeKeywords =
    [
        "abstract", "as", "async", "await", "base", "bool", "break", "byte", "catch", "char",
        "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double",
        "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
        "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "using", "virtual", "void",
        "volatile", "while", "yield",
    ];

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    internal static bool IsCode(string line)
    {
        var checkedLine = line.Replace(" ", string.Empty).Replace("\t", string.Empty);

        if (checkedLine.Contains("License") || checkedLine.Contains("c++") || checkedLine.Contains("C++"))
        {
            return false;
        }

        return EndsWithCode(checkedLine, line)
            || ContainsCodeParts(checkedLine)
            || ContainsMultipleLogicalOperators(checkedLine)
            || ContainsCodePartsWithRelationalOperator(checkedLine);
    }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterTreeAction(c =>
            {
                foreach (var token in c.Tree.GetRoot().DescendantTokens())
                {
                    CheckTrivia(c, token.LeadingTrivia);
                    CheckTrivia(c, token.TrailingTrivia);
                }
            });

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

        for (var triviaLineNumber = 0; triviaLineNumber < triviaLines.Length; triviaLineNumber++)
        {
            if (IsCode(triviaLines[triviaLineNumber]))
            {
                var triviaStartingLineNumber = trivia.GetLocation().StartLine();
                var lineNumber = triviaStartingLineNumber + triviaLineNumber;
                var lineSpan = context.Tree.GetText().Lines[lineNumber].Span;
                var commentLineSpan = lineSpan.Intersection(trivia.GetLocation().SourceSpan);
                var location = Location.Create(context.Tree, commentLineSpan ?? lineSpan);
                context.ReportIssue(Rule, location);
                return;
            }
        }

        string TriviaContent()
        {
            var content = trivia.ToString().Substring(CommentMarkLength);
            return content.EndsWith("*/", StringComparison.Ordinal) ? content.Substring(0, content.Length - CommentMarkLength) : content;
        }
    }

    private static bool ContainsMultipleLogicalOperators(string checkedLine)
    {
        const int lengthOfOperator = 2;
        const int operatorCountLimit = 3;
        var lineLengthWithoutLogicalOperators = checkedLine.Replace("&&", string.Empty).Replace("||", string.Empty).Length;

        return checkedLine.Length - lineLengthWithoutLogicalOperators >= operatorCountLimit * lengthOfOperator;
    }

    private static bool ContainsCodeParts(string checkedLine) =>
        CodeParts.Any(checkedLine.Contains);

    private static bool ContainsCodePartsWithRelationalOperator(string checkedLine)
    {
        return CodePartsWithRelationalOperator.Any(ContainsRelationalOperator);

        bool ContainsRelationalOperator(string codePart)
        {
            var index = checkedLine.IndexOf(codePart, StringComparison.Ordinal);
            return index >= 0 && RelationalOperators.Any(x => checkedLine.IndexOf(x, index, StringComparison.Ordinal) >= 0);
        }
    }

    private static bool EndsWithCode(string checkedLine, string originalLine) =>
        checkedLine == "}"
        || CodeEndings.Any(x => checkedLine.EndsWith(x, StringComparison.Ordinal))
        || (checkedLine.EndsWith(";", StringComparison.Ordinal) && !LooksLikeSentence(originalLine.Trim().TrimEnd(';')));

    private static bool LooksLikeSentence(string trimmedLine) =>
        SentencePattern.SafeMatch(trimmedLine) is { Success: true } match
        && !(CodeKeywords.Contains(match.Groups[1].Value) && CodeKeywords.Contains(match.Groups[2].Value));
}
