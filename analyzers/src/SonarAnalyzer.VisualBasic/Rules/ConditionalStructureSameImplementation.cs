/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ConditionalStructureSameImplementation : ConditionalStructureSameImplementationBase
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> ignoredStatementsInSwitch = new HashSet<SyntaxKind>
        {
            SyntaxKind.ReturnStatement,
            SyntaxKind.ThrowStatement,
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var ifStatement = (SingleLineIfStatementSyntax)c.Node;

                    if (ifStatement.ElseClause != null &&
                        VisualBasicEquivalenceChecker.AreEquivalent(ifStatement.ElseClause.Statements, ifStatement.Statements))
                    {
                        ReportIssue(c, ifStatement.ElseClause.Statements, ifStatement.Statements, "branch");
                    }
                },
                SyntaxKind.SingleLineIfStatement);

            context.RegisterNodeAction(
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
                        CheckStatementsAt(c, statements, i, "branch");
                    }
                },
                SyntaxKind.MultiLineIfBlock);

            context.RegisterNodeAction(
                c =>
                {
                    var select = (SelectBlockSyntax)c.Node;
                    var statements = select.CaseBlocks.Select(b => b.Statements).ToList();
                    for (var i = 1; i < statements.Count; i++)
                    {
                        CheckStatementsAt(c, statements, i, "case");
                    }
                },
                SyntaxKind.SelectBlock);
        }

        private static void CheckStatementsAt(SonarSyntaxNodeReportingContext context, List<SyntaxList<StatementSyntax>> statements, int currentIndex, string constructType)
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
                    ReportIssue(context, currentBlockStatements, statements[j], constructType);
                    return;
                }
            }
        }

        private static void ReportIssue(SonarSyntaxNodeReportingContext context, SyntaxList<StatementSyntax> statementsToReport, SyntaxList<StatementSyntax> locationProvider, string constructType)
        {
            var firstStatement = statementsToReport.FirstOrDefault();
            if (firstStatement == null)
            {
                return;
            }

            var lastStatement = statementsToReport.Last();

            context.ReportIssue(CreateDiagnostic(rule, firstStatement.CreateLocation(lastStatement),
                locationProvider.First().GetLineNumberToReport(), constructType));
        }

        private static bool IsApprovedStatement(StatementSyntax statement)
        {
            return !statement.IsAnyKind(ignoredStatementsInSwitch);
        }
    }
}
