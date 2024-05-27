/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    // Note: this rule only covers the indentation of the first line after a conditional.
    // Rule 2681 covers the misleading indentation of other lines of multiline blocks (https://jira.sonarsource.com/browse/RSPEC-2681)
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IndentSingleLineFollowingConditional : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3973";
        private const string MessageFormat = "Use curly braces or indentation to denote the code conditionally executed by this '{0}'";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(CheckWhile, SyntaxKind.WhileStatement);
            context.RegisterNodeAction(CheckDo, SyntaxKind.DoStatement);
            context.RegisterNodeAction(CheckFor, SyntaxKind.ForStatement);
            context.RegisterNodeAction(CheckForEach, SyntaxKind.ForEachStatement);
            context.RegisterNodeAction(CheckIf, SyntaxKind.IfStatement);
            context.RegisterNodeAction(CheckElse, SyntaxKind.ElseClause);
        }

        private static void CheckWhile(SonarSyntaxNodeReportingContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;
            if (!IsStatementIndentationOk(whileStatement, whileStatement.Statement))
            {
                // Squiggle - "while (condition1 && condition2)"
                var primaryLocation = whileStatement.WhileKeyword.CreateLocation(whileStatement.CloseParenToken);
                ReportIssue(context, primaryLocation, whileStatement.Statement, "while");
            }
        }

        private static void CheckDo(SonarSyntaxNodeReportingContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;
            if (!IsStatementIndentationOk(doStatement, doStatement.Statement))
            {
                // Just highlight the "do" keyword
                ReportIssue(context, doStatement.DoKeyword.GetLocation(), doStatement.Statement, "do");
            }
        }

        private static void CheckFor(SonarSyntaxNodeReportingContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;
            if (!IsStatementIndentationOk(forStatement, forStatement.Statement))
            {
                // Squiggle - "for (...)"
                var primaryLocation = forStatement.ForKeyword.CreateLocation(forStatement.CloseParenToken);
                ReportIssue(context, primaryLocation, forStatement.Statement, "for");
            }
        }

        private static void CheckForEach(SonarSyntaxNodeReportingContext context)
        {
            var forEachStatement = (ForEachStatementSyntax)context.Node;
            if (!IsStatementIndentationOk(forEachStatement, forEachStatement.Statement))
            {
                // Squiggle - "foreach (...)"
                var primaryLocation = forEachStatement.ForEachKeyword.CreateLocation(forEachStatement.CloseParenToken);
                ReportIssue(context, primaryLocation, forEachStatement.Statement, "foreach");
            }
        }

        private static void CheckIf(SonarSyntaxNodeReportingContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

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

        private static void CheckElse(SonarSyntaxNodeReportingContext context)
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

        private static void ReportIssue(SonarSyntaxNodeReportingContext context, Location primaryLocation, SyntaxNode secondaryLocationNode, string conditionLabelText) =>
               context.ReportIssue(Rule, primaryLocation, [GetFirstLineOfNode(secondaryLocationNode).ToSecondary()], conditionLabelText);

        private static Location GetFirstLineOfNode(SyntaxNode node)
        {
            var lineNumber = node.GetLocation().StartLine();
            var wholeLineSpan = node.SyntaxTree.GetText().Lines[lineNumber].Span;
            var secondaryLocationSpan = wholeLineSpan.Intersection(node.GetLocation().SourceSpan);

            return Location.Create(node.SyntaxTree, secondaryLocationSpan ?? wholeLineSpan);
        }
    }
}
