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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ConditionalStructureSameImplementation : ConditionalStructureSameImplementationBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> ignoredStatementsInSwitch = new HashSet<SyntaxKind>
        {
            SyntaxKind.ReturnStatement,
            SyntaxKind.ThrowStatement,
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var ifStatement = (SingleLineIfStatementSyntax)c.Node;

                    if (ifStatement.ElseClause != null &&
                        VisualBasicEquivalenceChecker.AreEquivalent(ifStatement.ElseClause.Statements, ifStatement.Statements))
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

                    for (var i = 1; i < statements.Count; i++)
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
                    for (var i = 1; i < statements.Count; i++)
                    {
                        CheckStatementsAt(i, statements, c, "case");
                    }
                },
                SyntaxKind.SelectBlock);
        }

        private static void CheckStatementsAt(int currentIndex, List<SyntaxList<StatementSyntax>> statements,
            SyntaxNodeAnalysisContext context, string constructType)
        {
            var currentBlockStatements = statements[currentIndex];
            if (currentBlockStatements.Count(IsApprovedStatement) < 2)
            {
                return;
            }

            for (var j = 0; j < currentIndex; j++)
            {
                if (VisualBasicEquivalenceChecker.AreEquivalent(currentBlockStatements, statements[j]))
                {
                    ReportIssue(currentBlockStatements, statements[j], context, constructType);
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

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, firstStatement.CreateLocation(lastStatement),
                locationProvider.First().GetLineNumberToReport(), constructType));
        }

        private static bool IsApprovedStatement(StatementSyntax statement)
        {
            return !statement.IsAnyKind(ignoredStatementsInSwitch);
        }
    }
}
