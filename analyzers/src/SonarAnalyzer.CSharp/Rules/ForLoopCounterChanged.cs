/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ForLoopCounterChanged : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S127";
    private const string MessageFormat = "Do not update the stop condition variable '{0}' in the body of the for loop.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly IImmutableList<SideEffectExpression> SideEffectExpressions = ImmutableArray.Create(
    new SideEffectExpression
    {
        Kinds = ImmutableHashSet.Create(SyntaxKind.PreIncrementExpression, SyntaxKind.PreDecrementExpression),
        AffectedExpressions = x => ImmutableArray.Create<SyntaxNode>(((PrefixUnaryExpressionSyntax)x).Operand)
    },
    new SideEffectExpression
    {
        Kinds = ImmutableHashSet.Create(SyntaxKind.PostIncrementExpression, SyntaxKind.PostDecrementExpression),
        AffectedExpressions = x => ImmutableArray.Create<SyntaxNode>(((PostfixUnaryExpressionSyntax)x).Operand)
    },
    new SideEffectExpression
    {
        Kinds = ImmutableHashSet.Create(
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.RightShiftAssignmentExpression),
        AffectedExpressions = x => ((AssignmentExpressionSyntax)x).AssignmentTargets()
    });

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var forNode = (ForStatementSyntax)c.Node;
                var loopCounters = LoopCounters(forNode, c.Model).ToList();
                foreach (var affectedExpression in ComputeAffectedExpressions(forNode.Statement))
                {
                    if (c.Model.GetSymbolInfo(affectedExpression).Symbol is { } symbol
                        && loopCounters.Contains(symbol))
                    {
                        c.ReportIssue(Rule, affectedExpression, symbol.Name);
                    }
                }
            },
            SyntaxKind.ForStatement);

    private static IEnumerable<ISymbol> LoopCounters(ForStatementSyntax node, SemanticModel model)
    {
        if (node.Condition is null || node.Incrementors.Count == 0)
        {
            return [];
        }

        var conditionSymbols = node.Condition.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
            .Select(x => model.GetSymbolInfo(x).Symbol)
            .Where(x => x is ILocalSymbol or IParameterSymbol)
            .WhereNotNull()
            .ToHashSet();

        return node.Incrementors
            .SelectMany(x => x.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
            .Select(x => model.GetSymbolInfo(x).Symbol)
            .Where(x => x is ILocalSymbol or IParameterSymbol)
            .WhereNotNull()
            .Where(conditionSymbols.Contains)
            .Distinct();
    }

    private static SyntaxNode[] ComputeAffectedExpressions(SyntaxNode node) =>
        (from descendantNode in node.DescendantNodesAndSelf()
         from sideEffect in SideEffectExpressions
         where descendantNode.IsAnyKind(sideEffect.Kinds)
         from expression in sideEffect.AffectedExpressions(descendantNode)
         select expression).ToArray();

    private readonly struct SideEffectExpression
    {
        public ImmutableHashSet<SyntaxKind> Kinds { get; init; }
        public Func<SyntaxNode, ImmutableArray<SyntaxNode>> AffectedExpressions { get; init; }
    }
}
