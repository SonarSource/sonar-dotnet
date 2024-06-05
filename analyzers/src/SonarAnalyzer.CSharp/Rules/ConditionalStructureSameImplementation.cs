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

                CheckStatement(c, ifStatement.Statement, precedingStatements, ifStatement.Else is not null);

                if (ifStatement.Else is null)
                {
                    return;
                }

                precedingStatements.Add(ifStatement.Statement);
                CheckStatement(c, ifStatement.Else.Statement, precedingStatements, true);
            },
            SyntaxKind.IfStatement);

        context.RegisterNodeAction(
            c =>
            {
                var switchSection = (SwitchSectionSyntax)c.Node;

                var precedingSections = switchSection
                    .GetPrecedingSections()
                    .ToList();
                var numberOfStatements = GetStatements(switchSection).Count(IsApprovedStatement);

                if (!HasDefaultClause((SwitchStatementSyntax)switchSection.Parent) && numberOfStatements == 1)
                {
                    var equivalentStatements = precedingSections.Where(x => EquivalentStatements(x.Statements)).ToList();

                    if (equivalentStatements.Count > 0 && equivalentStatements.Count == precedingSections.Count)
                    {
                        ReportSyntaxNode(c, switchSection, equivalentStatements[0], "case");
                    }
                }
                else if (numberOfStatements > 1)
                {
                    var precedingSection = precedingSections.Find(x => EquivalentStatements(x.Statements));
                    if (precedingSection is not null)
                    {
                        ReportSyntaxNode(c, switchSection, precedingSection, "case");
                    }
                }

                bool EquivalentStatements(SyntaxList<SyntaxNode> statements) =>
                    CSharpEquivalenceChecker.AreEquivalent(switchSection.Statements, statements) && HaveTheSameInvocations(switchSection.Statements, statements, c.SemanticModel);
            },
            SyntaxKind.SwitchSection);
    }

    private static IEnumerable<StatementSyntax> GetStatements(SwitchSectionSyntax switchSection) =>
        Enumerable.Empty<StatementSyntax>()
            .Union(switchSection.Statements.OfType<BlockSyntax>().SelectMany(x => x.Statements))
            .Union(switchSection.Statements.Where(x => !x.IsKind(SyntaxKind.Block)));

    private static bool HasDefaultClause(SwitchStatementSyntax switchStatement) =>
        switchStatement.Sections.Any(x => x.Labels.Any(l => l.IsKind(SyntaxKind.DefaultSwitchLabel)));

    private static void CheckStatement(SonarSyntaxNodeReportingContext context, SyntaxNode statement, IList<StatementSyntax> precedingStatements, bool hasElse)
    {
        var numberOfStatements = statement.ChildNodes().Count();
        if (!hasElse && numberOfStatements == 1)
        {
            var precedingStatement = precedingStatements.Where(x => CSharpEquivalenceChecker.AreEquivalent(statement, x)).ToList();
            if (precedingStatement.Count > 0 && precedingStatement.Count == precedingStatements.Count)
            {
                ReportSyntaxNode(context, statement, precedingStatement[0], "branch");
            }
        }
        else if (numberOfStatements > 1)
        {
            var precedingStatement = precedingStatements.FirstOrDefault(x => CSharpEquivalenceChecker.AreEquivalent(statement, x));
            if (precedingStatement is not null)
            {
                ReportSyntaxNode(context, statement, precedingStatement, "branch");
            }
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
