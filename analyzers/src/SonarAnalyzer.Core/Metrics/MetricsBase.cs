/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Metrics;

public abstract class MetricsBase
{
    protected const string InitializationErrorTextPattern = "The input tree is not of the expected language.";

    private readonly SyntaxTree tree;
    private readonly string filePath;

    public abstract ImmutableArray<int> ExecutableLines { get; }
    public abstract int ComputeCyclomaticComplexity(SyntaxNode node);
    protected abstract bool IsEndOfFile(SyntaxToken token);
    protected abstract int ComputeCognitiveComplexity(SyntaxNode node);
    protected abstract bool IsNoneToken(SyntaxToken token);
    protected abstract bool IsCommentTrivia(SyntaxTrivia trivia);
    protected abstract bool IsClass(SyntaxNode node);
    protected abstract bool IsStatement(SyntaxNode node);
    protected abstract bool IsFunction(SyntaxNode node);

    public ISet<int> CodeLines =>
        tree.GetRoot()
            .DescendantTokens()
            .Select(GetMappedLineSpan)
            .Where(x => x is not null)
            .SelectMany(
                x =>
                {
                    var start = x.Value.StartLinePosition.GetLineNumberToReport();
                    var end = x.Value.EndLinePosition.GetLineNumberToReport();
                    return Enumerable.Range(start, end - start + 1);
                })
            .ToHashSet();

    protected MetricsBase(SyntaxTree tree)
    {
        this.tree = tree;
        filePath = tree.GetRoot().GetMappedFilePathFromRoot();
    }

    public FileComments GetComments(bool ignoreHeaderComments)
    {
        var noSonar = new HashSet<int>();
        var nonBlank = new HashSet<int>();

        foreach (var trivia in tree.GetRoot().DescendantTrivia())
        {
            if (!IsCommentTrivia(trivia)
                || ignoreHeaderComments && IsNoneToken(trivia.Token.GetPreviousToken()))
            {
                continue;
            }

            var mappedSpan = tree.GetMappedLineSpan(trivia.FullSpan);
            if (!IsInSameFile(mappedSpan))
            {
                continue;
            }

            var lineNumber = mappedSpan.StartLinePosition.Line + 1;
            var triviaLines = trivia.ToFullString().Split(Constants.LineTerminators, StringSplitOptions.None);

            foreach (var line in triviaLines)
            {
                CategorizeLines(line, lineNumber, noSonar, nonBlank);

                lineNumber++;
            }
        }

        return new FileComments(noSonar.ToHashSet(), nonBlank.ToHashSet());
    }

    public int ClassCount =>
        ClassNodes.Count();

    public int StatementCount =>
        tree.GetRoot().DescendantNodes().Count(IsStatement);

    public int FunctionCount =>
        FunctionNodes.Count();

    public int Complexity =>
        ComputeCyclomaticComplexity(tree.GetRoot());

    public int CognitiveComplexity =>
        ComputeCognitiveComplexity(tree.GetRoot());

    protected bool IsInSameFile(FileLinePositionSpan span) =>
        // Syntax tree can contain elements from external files (e.g. razor imports files)
        // We need to make sure that we don't count these elements.
        string.Equals(filePath, span.Path, StringComparison.OrdinalIgnoreCase);

    private IEnumerable<SyntaxNode> ClassNodes =>
        tree.GetRoot().DescendantNodes().Where(IsClass);

    private IEnumerable<SyntaxNode> FunctionNodes =>
        tree.GetRoot().DescendantNodes().Where(IsFunction);

    private FileLinePositionSpan? GetMappedLineSpan(SyntaxToken token) =>
        !IsEndOfFile(token)
        && token.GetLocation().GetMappedLineSpan() is { IsValid: true } mappedLineSpan
        && (!GeneratedCodeRecognizer.IsRazor(token.SyntaxTree) || mappedLineSpan.HasMappedPath)
        && IsInSameFile(mappedLineSpan)
            ? mappedLineSpan
            : null;

    private static void CategorizeLines(string line, int lineNumber, ISet<int> noSonar, ISet<int> nonBlank)
    {
        if (line.Contains("NOSONAR"))
        {
            nonBlank.Remove(lineNumber);
            noSonar.Add(lineNumber);
        }
        else
        {
            if (HasValidCommentContent(line)
                && !noSonar.Contains(lineNumber))
            {
                nonBlank.Add(lineNumber);
            }
        }
    }

    private static bool HasValidCommentContent(string content) =>
        content.Any(char.IsLetter) || content.Any(char.IsDigit);
}
