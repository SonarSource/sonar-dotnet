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

                CheckStatement(c, ifStatement.Statement, precedingStatements, c.SemanticModel, ifStatement.Else is not null, "branch");

                if (ifStatement.Else is null)
                {
                    return;
                }

                precedingStatements.Add(ifStatement.Statement);
                CheckStatement(c, ifStatement.Else.Statement, precedingStatements, c.SemanticModel, true, "branch");
            },
            SyntaxKind.IfStatement);

        context.RegisterNodeAction(
            c =>
            {
                var switchSection = (SwitchSectionSyntax)c.Node;

                var precedingSections = switchSection
                    .GetPrecedingSections()
                    .ToList();

                CheckStatement(c, switchSection, precedingSections, c.SemanticModel, HasDefaultClause((SwitchStatementSyntax)switchSection.Parent), "case");
            },
            SyntaxKind.SwitchSection);
    }

    private static bool HasDefaultClause(SwitchStatementSyntax switchStatement) =>
        switchStatement.Sections.Any(x => x.Labels.Any(l => l.IsKind(SyntaxKind.DefaultSwitchLabel)));

    private static void CheckStatement(SonarSyntaxNodeReportingContext context, SyntaxNode node, IEnumerable<SyntaxNode> precedingStatements, SemanticModel model, bool hasElse, string discriminator)
    {
        var numberOfStatements = GetStatementsCount(node);
        if (!hasElse && numberOfStatements == 1)
        {
            var equivalentStatements = precedingStatements.Where(x => AreEquivalentStatements(node, x, model)).ToList();
            if (equivalentStatements.Count > 0 && equivalentStatements.Count == precedingStatements.Count())
            {
                ReportSyntaxNode(context, node, equivalentStatements[0], discriminator);
            }
        }
        else if (numberOfStatements > 1)
        {
            var equivalentStatement = precedingStatements.FirstOrDefault(x => AreEquivalentStatements(node, x, model));
            if (equivalentStatement is not null)
            {
                ReportSyntaxNode(context, node, equivalentStatement, discriminator);
            }
        }
    }

    private static bool AreEquivalentStatements(SyntaxNode node, SyntaxNode otherNode, SemanticModel model) =>
        new EquivalentNodeCompare(node, otherNode) switch
        {
            (SwitchSectionSyntax { Statements: var statements }, SwitchSectionSyntax { Statements: var otherStatements }) =>
                CSharpEquivalenceChecker.AreEquivalent(statements, otherStatements) && HaveTheSameInvocations(statements, otherStatements, model),
            (BlockSyntax refBlock, BlockSyntax otherBlock) =>
                CSharpEquivalenceChecker.AreEquivalent(refBlock, otherBlock) && HaveTheSameInvocations(refBlock, otherBlock, model),
            _ => false, // Should not happen
        };

    private static int GetStatementsCount(SyntaxNode node)
    {
        var statements = node is SwitchSectionSyntax switchSection
            ? Enumerable.Empty<SyntaxNode>()
                .Union(switchSection.Statements.OfType<BlockSyntax>().SelectMany(x => x.Statements))
                .Union(switchSection.Statements.Where(x => !x.IsKind(SyntaxKind.Block)))
            : node.ChildNodes();

        return statements.Count(IsApprovedStatement);
    }

    private static void ReportSyntaxNode(SonarSyntaxNodeReportingContext context, SyntaxNode node, SyntaxNode precedingNode, string errorMessageDiscriminator) =>
        context.ReportIssue(Rule, node, [precedingNode.ToSecondaryLocation()], precedingNode.GetLineNumberToReport().ToString(), errorMessageDiscriminator);

    private static bool IsApprovedStatement(SyntaxNode statement) =>
        !statement.IsAnyKind(IgnoredStatementsInSwitch);

    private static bool HaveTheSameInvocations(SyntaxList<SyntaxNode> first, SyntaxList<SyntaxNode> second, SemanticModel model)
    {
        var referenceInvocations = first.SelectMany(x => x.DescendantNodes().OfType<InvocationExpressionSyntax>()).ToArray();
        var candidateInvocations = second.SelectMany(x => x.DescendantNodes().OfType<InvocationExpressionSyntax>()).ToArray();
        return HaveTheSameInvocations(referenceInvocations, candidateInvocations, model);
    }

    private static bool HaveTheSameInvocations(SyntaxNode first, SyntaxNode second, SemanticModel model)
    {
        var referenceInvocations = first.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray();
        var candidateInvocations = second.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray();
        return HaveTheSameInvocations(referenceInvocations, candidateInvocations, model);
    }

    private static bool HaveTheSameInvocations(InvocationExpressionSyntax[] referenceInvocations, InvocationExpressionSyntax[] candidateInvocations, SemanticModel model)
    {
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

    private sealed record EquivalentNodeCompare(SyntaxNode RefNode, SyntaxNode OtherNode);
}
