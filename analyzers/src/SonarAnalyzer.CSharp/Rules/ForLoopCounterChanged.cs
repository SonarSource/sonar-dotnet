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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ForLoopCounterChanged : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S127";
        private const string MessageFormat = "Do not update the loop counter '{0}' within the loop body.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private sealed class SideEffectExpression
        {
            public ImmutableHashSet<SyntaxKind> Kinds { get; set; }
            public Func<SyntaxNode, ImmutableArray<SyntaxNode>> AffectedExpressions { get; set; }
        }

        private static readonly IImmutableList<SideEffectExpression> SideEffectExpressions = ImmutableArray.Create(
            new SideEffectExpression
            {
                Kinds = ImmutableHashSet.Create(SyntaxKind.PreIncrementExpression, SyntaxKind.PreDecrementExpression),
                AffectedExpressions = node => ImmutableArray.Create<SyntaxNode>(((PrefixUnaryExpressionSyntax)node).Operand)
            },
            new SideEffectExpression
            {
                Kinds = ImmutableHashSet.Create(SyntaxKind.PostIncrementExpression, SyntaxKind.PostDecrementExpression),
                AffectedExpressions = node => ImmutableArray.Create<SyntaxNode>(((PostfixUnaryExpressionSyntax)node).Operand)
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
                AffectedExpressions = node => ((AssignmentExpressionSyntax)node).AssignmentTargets()
            });

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var forNode = (ForStatementSyntax)c.Node;
                    var loopCounters = LoopCounters(forNode, c.SemanticModel).ToList();

                    foreach (var affectedExpression in ComputeAffectedExpressions(forNode.Statement))
                    {
                        var symbol = c.SemanticModel.GetSymbolInfo(affectedExpression).Symbol;

                        if (symbol != null && loopCounters.Contains(symbol))
                        {
                            c.ReportIssue(CreateDiagnostic(Rule, affectedExpression.GetLocation(), symbol.Name));
                        }
                    }
                },
                SyntaxKind.ForStatement);

        private static IEnumerable<ISymbol> LoopCounters(ForStatementSyntax node, SemanticModel semanticModel)
        {
            var declaredVariables = node.Declaration == null
                ? Enumerable.Empty<ISymbol>()
                : node.Declaration.Variables.Select(v => semanticModel.GetDeclaredSymbol(v));

            var initializedVariables = node.Initializers.OfType<AssignmentExpressionSyntax>()
                .SelectMany(x => x.AssignmentTargets())
                .Select(x => VariableDesignationSyntaxWrapper.IsInstance(x)
                    ? semanticModel.GetDeclaredSymbol(x)
                    : semanticModel.GetSymbolInfo(x).Symbol);

            return declaredVariables.Union(initializedVariables).Where(x => x is { Kind: not SymbolKindEx.Discard });
        }

        private static SyntaxNode[] ComputeAffectedExpressions(SyntaxNode node) =>
            (from descendantNode in node.DescendantNodesAndSelf()
             from sideEffect in SideEffectExpressions
             where descendantNode.IsAnyKind(sideEffect.Kinds)
             from expression in sideEffect.AffectedExpressions(descendantNode)
             select expression).ToArray();
    }
}
