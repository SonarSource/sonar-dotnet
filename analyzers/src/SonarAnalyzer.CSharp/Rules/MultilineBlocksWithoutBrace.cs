/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MultilineBlocksWithoutBrace : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2681";
    private const string MessageFormat = "This line will not be executed {0}; only the first line of this {2}-line block will be. The rest will execute {1}.";
    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c => CheckLoop(c, ((WhileStatementSyntax)c.Node).Statement),
            SyntaxKind.WhileStatement);

        context.RegisterNodeAction(
            c => CheckLoop(c, ((ForStatementSyntax)c.Node).Statement),
            SyntaxKind.ForStatement);

        context.RegisterNodeAction(
            c => CheckLoop(c, ((ForEachStatementSyntax)c.Node).Statement),
            SyntaxKind.ForEachStatement);

        context.RegisterNodeAction(
            c => CheckIf(c, (IfStatementSyntax)c.Node),
            SyntaxKind.IfStatement);
    }

    private static void CheckLoop(SonarSyntaxNodeReportingContext context, StatementSyntax statement)
    {
        if (!IsNestedStatement(statement))
        {
            CheckStatement(context, statement, "in a loop", "only once");
        }
    }

    private static void CheckIf(SonarSyntaxNodeReportingContext context, IfStatementSyntax ifStatement)
    {
        if (!ifStatement.PrecedingIfsInConditionChain().Any()
            && !IsNestedStatement(ifStatement.Statement)
            && LastStatementInIfChain(ifStatement) is { } lastStatementInIfChain
            && !IsStatementCandidateLoop(lastStatementInIfChain))
        {
            CheckStatement(context, lastStatementInIfChain, "conditionally", "unconditionally");
        }
    }

    private static StatementSyntax LastStatementInIfChain(IfStatementSyntax ifStatement)
    {
        var statement = ifStatement.Statement;

        while (ifStatement is { })
        {
            if (ifStatement.Else is null)
            {
                return ifStatement.Statement;
            }
            statement = ifStatement.Else.Statement;
            ifStatement = statement as IfStatementSyntax;
        }
        return statement;
    }

    private static void CheckStatement(SonarSyntaxNodeReportingContext context, StatementSyntax first, string executed, string execute)
    {
        if (IsNotEmpty(first)
            && SecondStatement(context.Node, first) is { } second
            && IsNotEmpty(second)
            && MisleadingtIndenting(first, second))
        {
            var secondLine = StartPosition(second).Line;
            var lineSpan = context.Node.SyntaxTree.GetText().Lines[secondLine].Span;
            var location = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(second.SpanStart, lineSpan.End));
            var blockSize = secondLine - StartPosition(first).Line + 1;
            context.ReportIssue(Rule, location, [first.ToSecondaryLocation()], executed, execute, blockSize.ToString());
        }
    }

    private static bool IsNotEmpty(SyntaxNode node) =>
        node is not EmptyStatementSyntax;

    private static bool MisleadingtIndenting(SyntaxNode first, SyntaxNode second)
    {
        var firstPosition = StartPosition(first);
        var secondPosition = StartPosition(second);
        var ancestor = first.AncestorsAndSelf().Select(x => StartPosition(x)).Last(x => x.Line == firstPosition.Line);

        // If the first node is not at the same line as its parent
        return firstPosition.Character == ancestor.Character
            ? secondPosition.Character >= firstPosition.Character
            : secondPosition.Character > ancestor.Character;
    }

    private static LinePosition StartPosition(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition;

    private static SyntaxNode SecondStatement(SyntaxNode root, SyntaxNode first) =>
        !first.IsKind(SyntaxKind.Block)
        // This algorithm to get the next statement can sometimes return a parent statement (for example a BlockSyntax)
        // so we need to filter this case by returning if the nextStatement happens to be one ancestor of statement.
        && root.GetLastToken().GetNextToken().Parent is { } second
        && !first.Ancestors().Contains(second)
        && second is not ElseClauseSyntax
        ? second
        : null;

    private static bool IsNestedStatement(StatementSyntax statement) =>
        statement.IsAnyKind(SyntaxKind.IfStatement, SyntaxKind.ForStatement, SyntaxKind.ForEachStatement, SyntaxKind.WhileStatement);

    private static bool IsStatementCandidateLoop(StatementSyntax statement) =>
        statement.IsAnyKind(SyntaxKind.ForEachStatement, SyntaxKind.ForStatement, SyntaxKind.WhileStatement);
}
