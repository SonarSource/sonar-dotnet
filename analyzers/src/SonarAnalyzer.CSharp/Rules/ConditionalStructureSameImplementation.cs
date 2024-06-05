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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConditionalStructureSameImplementation : ConditionalStructureSameImplementationBase
{
    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ISet<SyntaxKind> IgnoredStatementsInSwitch = new HashSet<SyntaxKind>
    {
        SyntaxKind.BreakStatement,
        SyntaxKind.ReturnStatement,
        SyntaxKind.ThrowStatement,
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var ifStatement = (IfStatementSyntax)c.Node;

                var precedingStatements = ifStatement
                    .GetPrecedingStatementsInConditionChain()
                    .ToList();

                CheckStatement(c, ifStatement.Statement, precedingStatements);

                if (ifStatement.Else is null)
                {
                    return;
                }

                precedingStatements.Add(ifStatement.Statement);
                CheckStatement(c, ifStatement.Else.Statement, precedingStatements);
            },
            SyntaxKind.IfStatement);

        context.RegisterNodeAction(
            c =>
            {
                var switchSection = (SwitchSectionSyntax)c.Node;

                if (GetStatements(switchSection).Count(IsApprovedStatement) < 2)
                {
                    return;
                }

                var precedingSection = switchSection
                    .GetPrecedingSections()
                    .FirstOrDefault(preceding => CSharpEquivalenceChecker.AreEquivalent(switchSection.Statements, preceding.Statements)
                                                 && HaveTheSameInvocations(switchSection.Statements, preceding.Statements, c.SemanticModel));

                if (precedingSection is not null)
                {
                    ReportSyntaxNode(c, switchSection, precedingSection, "case");
                }
            },
            SyntaxKind.SwitchSection);
    }

    private static IEnumerable<StatementSyntax> GetStatements(SwitchSectionSyntax switchSection) =>
        Enumerable.Empty<StatementSyntax>()
            .Union(switchSection.Statements.OfType<BlockSyntax>().SelectMany(x => x.Statements))
            .Union(switchSection.Statements.Where(x => !x.IsKind(SyntaxKind.Block)));

    private static void CheckStatement(SonarSyntaxNodeReportingContext context, SyntaxNode statement, IEnumerable<StatementSyntax> precedingStatements)
    {
        if (statement.ChildNodes().Count() < 2)
        {
            return;
        }

        var precedingStatement = precedingStatements.FirstOrDefault(x => CSharpEquivalenceChecker.AreEquivalent(statement, x));
        if (precedingStatement is not null)
        {
            ReportSyntaxNode(context, statement, precedingStatement, "branch");
        }
    }

    private static void ReportSyntaxNode(SonarSyntaxNodeReportingContext context, SyntaxNode node, SyntaxNode precedingNode, string errorMessageDiscriminator) =>
        context.ReportIssue(Rule, node, [precedingNode.ToSecondaryLocation()], precedingNode.GetLineNumberToReport().ToString(), errorMessageDiscriminator);

    private static bool IsApprovedStatement(StatementSyntax statement) =>
        !statement.IsAnyKind(IgnoredStatementsInSwitch);

    private static bool HaveTheSameInvocations(SyntaxList<SyntaxNode> first, SyntaxList<SyntaxNode> second, SemanticModel model)
    {
        var referenceInvocations = first.SelectMany(x => x.DescendantNodes().OfType<InvocationExpressionSyntax>()).ToArray();
        var candidateInvocations = second.SelectMany(x => x.DescendantNodes().OfType<InvocationExpressionSyntax>()).ToArray();
        if (referenceInvocations.Length != candidateInvocations.Length)
        {
            return false;
        }

        for (var i = 0; i < referenceInvocations.Length; i++)
        {
            if (!referenceInvocations[i].IsEqualTo(candidateInvocations[i], model))
            {
                return false;
            }
        }
        return true;
    }
}
