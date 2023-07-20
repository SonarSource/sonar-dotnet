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
    public sealed class ValuesUselesslyIncremented : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2123";
        private const string MessageFormat = "Remove this {0} or correct the code not to waste it.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var increment = (PostfixUnaryExpressionSyntax)c.Node;
                    var symbol = c.SemanticModel.GetSymbolInfo(increment.Operand).Symbol;

                    if (symbol is ILocalSymbol || symbol is IParameterSymbol { RefKind: RefKind.None })
                    {
                        VisitParent(c, increment);
                    }
                },
                SyntaxKind.PostIncrementExpression,
                SyntaxKind.PostDecrementExpression);

        private static void VisitParent(SonarSyntaxNodeReportingContext context, PostfixUnaryExpressionSyntax increment)
        {
            switch (increment.Parent)
            {
                case ReturnStatementSyntax:
                case ArrowExpressionClauseSyntax:
                case CastExpressionSyntax castExpressionSyntax
                    when castExpressionSyntax.Parent.IsAnyKind(SyntaxKind.ReturnStatement, SyntaxKind.ArrowExpressionClause):
                case ArgumentSyntax argumentInAssignment
                    when argumentInAssignment.FindAssignmentComplement() is { } assignmentTarget
                         && CSharpEquivalenceChecker.AreEquivalent(assignmentTarget, increment.Operand):
                case ArgumentSyntax argumentInReturn
                    when argumentInReturn.OutermostTuple() is { SyntaxNode.Parent: ReturnStatementSyntax or ArrowExpressionClauseSyntax }:
                case AssignmentExpressionSyntax assignment
                    when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
                         && assignment.Right == increment
                         && CSharpEquivalenceChecker.AreEquivalent(assignment.Left, increment.Operand):

                    var operatorText = increment.OperatorToken.IsKind(SyntaxKind.PlusPlusToken)
                        ? "increment"
                        : "decrement";

                    context.ReportIssue(CreateDiagnostic(Rule, increment.GetLocation(), operatorText));
                    return;
                default:
                    return;
            }
        }
    }
}
