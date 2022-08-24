/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

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
        context.RegisterSyntaxNodeActionInNonGenerated(
            c => CheckLoop(c, ((WhileStatementSyntax)c.Node).Statement),
            SyntaxKind.WhileStatement);

        context.RegisterSyntaxNodeActionInNonGenerated(
            c => CheckLoop(c, ((ForStatementSyntax)c.Node).Statement),
            SyntaxKind.ForStatement);

        context.RegisterSyntaxNodeActionInNonGenerated(
            c => CheckLoop(c, ((ForEachStatementSyntax)c.Node).Statement),
            SyntaxKind.ForEachStatement);

        context.RegisterSyntaxNodeActionInNonGenerated(
            c => CheckIf(c, (IfStatementSyntax)c.Node),
            SyntaxKind.IfStatement);
    }

    private static void CheckLoop(SyntaxNodeAnalysisContext context, StatementSyntax statement)
    {
        if (!IsNestedStatement(statement))
        {
            CheckStatement(context, statement, "in a loop", "only once");
        }
    }

    private static void CheckIf(SyntaxNodeAnalysisContext context, IfStatementSyntax ifStatement)
    {
        if (!ifStatement.GetPrecedingIfsInConditionChain().Any()
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

        while (ifStatement != null)
        {
            if (ifStatement.Else == null)
            {
                return ifStatement.Statement;
            }
            statement = ifStatement.Else.Statement;
            ifStatement = statement as IfStatementSyntax;
        }
        return statement;
    }

    private static void CheckStatement(SyntaxNodeAnalysisContext context, StatementSyntax first, string executed, string execute)
    {
        if (SecondStatement(context.Node, first) is { } second)
        {
            var firstPosition = first.GetLocation().GetLineSpan().StartLinePosition;
            var secondPosition = second.GetLocation().GetLineSpan().StartLinePosition;
            var rootPosition = context.Node.GetLocation().GetLineSpan().StartLinePosition;

            if (SameIndentation(firstPosition, secondPosition)
                || DifferentIndentation(rootPosition, secondPosition))
            {
                var lineSpan = context.Node.SyntaxTree.GetText().Lines[secondPosition.Line].Span;
                var location = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(second.SpanStart, lineSpan.End));
                var blockSize = secondPosition.Line - firstPosition.Line + 1;
                var additional = new[] { first.GetLocation() };
                context.ReportIssue(Rule.CreateDiagnostic(context.Compilation, location, additional, executed, execute, blockSize));
            }
        }
    }

    private static bool SameIndentation(LinePosition first, LinePosition second) =>
        first.Character == second.Character;

    private static bool DifferentIndentation(LinePosition first, LinePosition second) =>
        first.Character != second.Character;

    private static SyntaxNode SecondStatement(SyntaxNode root, SyntaxNode first) =>
        !first.IsKind(SyntaxKind.Block)
        // This algorithm to get the next statement can sometimes return a parent statement (for example a BlockSyntax)
        // so we need to filter this case by returning if the nextStatement happens to be one ancestor of statement.
        && root.GetLastToken().GetNextToken().Parent is { } second
        && !first.Ancestors().Contains(second)
        ? second
        : null;

    private static bool IsNestedStatement(StatementSyntax statement) =>
        statement.IsAnyKind(SyntaxKind.IfStatement, SyntaxKind.ForStatement, SyntaxKind.ForEachStatement, SyntaxKind.WhileStatement);

    private static bool IsStatementCandidateLoop(StatementSyntax statement) =>
        statement.IsAnyKind(SyntaxKind.ForEachStatement, SyntaxKind.ForStatement, SyntaxKind.WhileStatement);
}
