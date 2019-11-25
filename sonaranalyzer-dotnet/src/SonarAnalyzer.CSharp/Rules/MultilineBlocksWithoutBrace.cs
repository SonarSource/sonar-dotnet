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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MultilineBlocksWithoutBrace : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2681";
        private const string MessageFormat =
            "This line will not be executed {0}; only the first line of this {2}-line block will be. The rest will execute {1}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

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
            if (IsNestedStatement(statement))
            {
                return;
            }

            CheckStatement(context, statement, "in a loop", "only once");
        }

        private static void CheckIf(SyntaxNodeAnalysisContext context, IfStatementSyntax ifStatement)
        {
            if (ifStatement.GetPrecedingIfsInConditionChain().Any())
            {
                return;
            }

            if (IsNestedStatement(ifStatement.Statement))
            {
                return;
            }

            var lastStatementInIfChain = GetLastStatementInIfChain(ifStatement);
            if (IsStatementCandidateLoop(lastStatementInIfChain))
            {
                return;
            }

            CheckStatement(context, lastStatementInIfChain, "conditionally", "unconditionally");
        }

        private static bool IsNestedStatement(StatementSyntax nested) =>
            nested.IsKind(SyntaxKind.IfStatement) ||
            nested.IsKind(SyntaxKind.ForStatement) ||
            nested.IsKind(SyntaxKind.ForEachStatement) ||
            nested.IsKind(SyntaxKind.WhileStatement);

        private static StatementSyntax GetLastStatementInIfChain(IfStatementSyntax ifStatement)
        {
            var currentIfStatement = ifStatement;
            var statement = currentIfStatement.Statement;
            while (currentIfStatement != null)
            {
                if (currentIfStatement.Else == null)
                {
                    return currentIfStatement.Statement;
                }

                statement = currentIfStatement.Else.Statement;
                currentIfStatement = statement as IfStatementSyntax;
            }

            return statement;
            //var currentIfStatement = ifStatement;
            //var statement = currentIfStatement.Statement;
            //while (currentIfStatement != null)
            //{
            //    if (currentIfStatement.Else == null)
            //    {
            //        return currentIfStatement.Statement;
            //    }

            //    statement = currentIfStatement.Else.Statement;
            //    currentIfStatement = statement as IfStatementSyntax;
            //}

            //return statement;
        }

        private static void CheckStatement(SyntaxNodeAnalysisContext context, StatementSyntax statement,
            string executed, string execute)
        {
            if (statement.IsKind(SyntaxKind.Block))
            {
                return;
            }

            var nextStatement = context.Node.GetLastToken().GetNextToken().Parent;
            // This algorithm to get the next statement can sometimes return a parent statement (for example a BlockSyntax)
            // so we need to filter this case by returning if the nextStatement happens to be one ancestor of statement.
            if (nextStatement == null ||
                statement.Ancestors().Contains(nextStatement))
            {
                return;
            }

            var statementPosition = statement.GetLocation().GetLineSpan().StartLinePosition;
            var nextStatementPosition = nextStatement.GetLocation().GetLineSpan().StartLinePosition;

            if (statementPosition.Character == nextStatementPosition.Character)
            {
                var lineSpan = context.Node.SyntaxTree.GetText().Lines[nextStatementPosition.Line].Span;
                var location = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(nextStatement.SpanStart, lineSpan.End));

                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location,
                    additionalLocations: new[] { statement.GetLocation() },
                    messageArgs: new object[] { executed, execute, nextStatementPosition.Line - statementPosition.Line + 1 }));
            }
        }

        private static bool IsStatementCandidateLoop(StatementSyntax statement) =>
            statement.IsKind(SyntaxKind.ForEachStatement) ||
            statement.IsKind(SyntaxKind.ForStatement) ||
            statement.IsKind(SyntaxKind.WhileStatement);
    }
}
