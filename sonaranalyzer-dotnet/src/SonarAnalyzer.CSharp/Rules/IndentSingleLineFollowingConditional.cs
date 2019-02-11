/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

// Note: this rule only covers the indentation of the first line after a conditional.
// Rule 2681 covers the misleading indentation of other lines of multiline blocks (https://jira.sonarsource.com/browse/RSPEC-2681)

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class IndentSingleLineFollowingConditional : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3973";
        private const string MessageFormat = "Use curly braces or indentation to denote the code conditionally executed by this '{0}'";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckWhile, SyntaxKind.WhileStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckDo, SyntaxKind.DoStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckFor, SyntaxKind.ForStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckForEach, SyntaxKind.ForEachStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckIf, SyntaxKind.IfStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckElse, SyntaxKind.ElseClause);
        }

        private static void CheckWhile(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;
            if (!IsStatementIndentationOk(whileStatement, whileStatement.Statement))
            {
                // Squiggle - "while (condition1 && condition2)"
                var primaryLocation = whileStatement.WhileKeyword.CreateLocation(whileStatement.CloseParenToken);
                ReportIssue(context, primaryLocation, whileStatement.Statement, "while");
            }
        }

        private static void CheckDo(SyntaxNodeAnalysisContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;
            if (!IsStatementIndentationOk(doStatement, doStatement.Statement))
            {
                // Just highlight the "do" keyword
                ReportIssue(context, doStatement.DoKeyword.GetLocation(), doStatement.Statement, "do");
            }
        }

        private static void CheckFor(SyntaxNodeAnalysisContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;
            if (!IsStatementIndentationOk(forStatement, forStatement.Statement))
            {
                // Squiggle - "for (...)"
                var primaryLocation = forStatement.ForKeyword.CreateLocation(forStatement.CloseParenToken);
                ReportIssue(context, primaryLocation, forStatement.Statement, "for");
            }
        }

        private static void CheckForEach(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (ForEachStatementSyntax)context.Node;
            if (!IsStatementIndentationOk(forEachStatement, forEachStatement.Statement))
            {
                // Squiggle - "foreach (...)"
                var primaryLocation = forEachStatement.ForEachKeyword.CreateLocation(forEachStatement.CloseParenToken);
                ReportIssue(context, primaryLocation, forEachStatement.Statement, "foreach");
            }
        }

        private static void CheckIf(SyntaxNodeAnalysisContext context)
        {
            IfStatementSyntax ifStatement = (IfStatementSyntax)context.Node;

            // Special case for "else if" on the same line.
            // In that case, we'll check that the statement is more indented then the "else", not the "if".
            // Highlighting: "if (...)", or  "else if (...)" as appropriate

            SyntaxNode controlNode;
            SyntaxToken startToken;
            string conditionLabelText;
            if (ifStatement.Parent is ElseClauseSyntax elseClause
                && ifStatement.GetLineNumberToReport() == elseClause.GetLineNumberToReport())
            {
                controlNode = elseClause;
                startToken = elseClause.ElseKeyword;
                conditionLabelText = "else if";
            }
            else
            {
                controlNode = ifStatement;
                startToken = ifStatement.IfKeyword;
                conditionLabelText = "if";
            }

            if (!IsStatementIndentationOk(controlNode, ifStatement.Statement))
            {
                var primaryLocation = startToken.CreateLocation(ifStatement.CloseParenToken);
                ReportIssue(context, primaryLocation, ifStatement.Statement, conditionLabelText);
            }
        }

        private static void CheckElse(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;
            if (!IsStatementIndentationOk(elseClause, elseClause.Statement))
            {
                // Just highlight the "else" keyword
                ReportIssue(context, elseClause.ElseKeyword.GetLocation(), elseClause.Statement, "else");
            }
        }

        private static bool IsStatementIndentationOk(SyntaxNode controlNode, SyntaxNode conditionallyExecutedNode) =>
            conditionallyExecutedNode is BlockSyntax ||
            VisualIndentComparer.IsSecondIndentLonger(controlNode, conditionallyExecutedNode);

        private static void ReportIssue(SyntaxNodeAnalysisContext context, Location primaryLocation,
            SyntaxNode secondaryLocationNode, string conditionLabelText) =>
               context.ReportDiagnosticWhenActive(
                    Diagnostic.Create(
                        rule,
                        primaryLocation,
                        new Location[] { GetFirstLineOfNode(secondaryLocationNode) },
                        conditionLabelText));

        private static Location GetFirstLineOfNode(SyntaxNode node)
        {
            var lineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line;
            var wholeLineSpan = node.SyntaxTree.GetText().Lines[lineNumber].Span;
            var secondaryLocationSpan = wholeLineSpan.Intersection(node.GetLocation().SourceSpan);

            var location = Location.Create(node.SyntaxTree, secondaryLocationSpan ?? wholeLineSpan);
            return location;
        }
    }
}
