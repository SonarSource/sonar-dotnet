/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
                var precedingStatements = ifStatement.GetPrecedingStatementsInConditionChain().ToList();
                var hasElse = HasLeafElseClause(ifStatement);

                CheckStatement(c, ifStatement.Statement, precedingStatements, c.SemanticModel, hasElse, "branch");

                if (ifStatement.Else is not null)
                {
                    CheckStatement(c, ifStatement.Else.Statement, [..precedingStatements, ifStatement.Statement], c.SemanticModel, hasElse, "branch");
                }
            },
            SyntaxKind.IfStatement);

        context.RegisterNodeAction(
            c =>
            {
                var switchSection = (SwitchSectionSyntax)c.Node;
                var precedingSections = switchSection.GetPrecedingSections().ToList();

                CheckStatement(c, switchSection, precedingSections, c.SemanticModel, HasDefaultClause((SwitchStatementSyntax)switchSection.Parent), "case");
            },
            SyntaxKind.SwitchSection);
    }

    private static bool HasDefaultClause(SwitchStatementSyntax switchStatement) =>
        switchStatement.Sections.SelectMany(x => x.Labels).Any(x => x.IsKind(SyntaxKind.DefaultSwitchLabel));

    private static bool HasLeafElseClause(IfStatementSyntax ifStatement)
    {
        while (ifStatement.Else?.Statement is IfStatementSyntax elseIfStatement)
        {
            ifStatement = elseIfStatement;
        }
        return ifStatement.Else is not null;
    }

    private static void CheckStatement(SonarSyntaxNodeReportingContext context, SyntaxNode node, IReadOnlyList<SyntaxNode> precedingBranches, SemanticModel model, bool hasElse, string discriminator)
    {
        var numberOfStatements = GetStatementsCount(node);
        if (!hasElse && numberOfStatements == 1)
        {
            if (precedingBranches.Any() && precedingBranches.All(x => AreEquivalentStatements(node, x, model)))
            {
                ReportSyntaxNode(context, node, precedingBranches[precedingBranches.Count - 1], discriminator);
            }
        }
        else if (numberOfStatements > 1 && precedingBranches.FirstOrDefault(x => AreEquivalentStatements(node, x, model)) is { } equivalentStatement)
        {
            ReportSyntaxNode(context, node, equivalentStatement, discriminator);
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
        // Get all child statements from the node, in case of a switch section, we need to handle the case where the statements are wrapped in a block
        var statements = node is SwitchSectionSyntax switchSection
            ? switchSection.Statements.OfType<BlockSyntax>().SelectMany(x => x.Statements)
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

    private readonly record struct EquivalentNodeCompare(SyntaxNode RefNode, SyntaxNode OtherNode);
}
