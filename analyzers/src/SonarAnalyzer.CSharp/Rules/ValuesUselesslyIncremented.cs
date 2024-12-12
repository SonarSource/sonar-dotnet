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
                    when castExpressionSyntax.Parent?.Kind() is SyntaxKind.ReturnStatement or SyntaxKind.ArrowExpressionClause:
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

                    context.ReportIssue(Rule, increment, operatorText);
                    return;
                default:
                    return;
            }
        }
    }
}
