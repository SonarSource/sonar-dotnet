/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public class ConditionalStructureSameImplementation : ConditionalStructureSameImplementationBase
    {
        protected static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var ifStatement = (SingleLineIfStatementSyntax)c.Node;

                    if (ifStatement.ElseClause != null &&
                        EquivalenceChecker.AreEquivalent(ifStatement.ElseClause.Statements, ifStatement.Statements))
                    {
                        ReportIssue(ifStatement.ElseClause.Statements, ifStatement.Statements, c, "branch");
                    }
                },
                SyntaxKind.SingleLineIfStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var ifBlock = (MultiLineIfBlockSyntax)c.Node;

                    var statements = new[] { ifBlock.Statements }
                        .Concat(ifBlock.ElseIfBlocks.Select(elseIf => elseIf.Statements))
                        .Concat(new[] { ifBlock.ElseBlock?.Statements ?? new SyntaxList<StatementSyntax>() })
                        .Where(l => l.Any())
                        .ToList();

                    for (int i = 1; i < statements.Count; i++)
                    {
                        CheckStatementsAt(i, statements, c, "branch");
                    }
                },
                SyntaxKind.MultiLineIfBlock);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var select = (SelectBlockSyntax)c.Node;
                    var statements = select.CaseBlocks.Select(b => b.Statements).ToList();
                    for (int i = 1; i < statements.Count; i++)
                    {
                        CheckStatementsAt(i, statements, c, "case");
                    }
                },
                SyntaxKind.SelectBlock);
        }

        private static void CheckStatementsAt(int currentIndex, List<SyntaxList<StatementSyntax>> statements,
            SyntaxNodeAnalysisContext context, string constructType)
        {
            for (int j = 0; j < currentIndex; j++)
            {
                if (EquivalenceChecker.AreEquivalent(statements[currentIndex], statements[j]))
                {
                    ReportIssue(statements[currentIndex], statements[j], context, constructType);
                    return;
                }
            }
        }

        private static void ReportIssue(SyntaxList<StatementSyntax> statementsToReport, SyntaxList<StatementSyntax> locationProvider,
            SyntaxNodeAnalysisContext context, string constructType)
        {
            var firstStatement = statementsToReport.FirstOrDefault();
            if (firstStatement == null)
            {
                return;
            }

            var lastStatement = statementsToReport.Last();

            var location = Location.Create(context.Node.SyntaxTree,
                TextSpan.FromBounds(firstStatement.SpanStart, lastStatement.Span.End));

            context.ReportDiagnostic(Diagnostic.Create(rule,
                location, locationProvider.First().GetLineNumberToReport(), constructType));
        }
    }
}
