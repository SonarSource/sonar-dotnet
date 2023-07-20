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
    public sealed class DelegateSubtraction : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3172";
        private const string MessageFormat = "Review this subtraction of a chain of delegates: it may not work as you expect.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;
                    if (!IsDelegateSubtraction(assignment, c.SemanticModel) ||
                        ExpressionIsSimple(assignment.Right))
                    {
                        return;
                    }

                    c.ReportIssue(CreateDiagnostic(rule, assignment.GetLocation()));
                },
                SyntaxKind.SubtractAssignmentExpression);

            context.RegisterNodeAction(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;
                    if (!IsDelegateSubtraction(binary, c.SemanticModel) ||
                        !IsTopLevelSubtraction(binary))
                    {
                        return;
                    }

                    if (!BinaryIsValidSubstraction(binary))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, binary.GetLocation()));
                    }
                },
                SyntaxKind.SubtractExpression);
        }

        private static bool BinaryIsValidSubstraction(BinaryExpressionSyntax subtraction)
        {
            var currentSubtraction = subtraction;

            while (currentSubtraction != null &&
                   currentSubtraction.IsKind(SyntaxKind.SubtractExpression))
            {
                if (!ExpressionIsSimple(currentSubtraction.Right))
                {
                    return false;
                }

                currentSubtraction = currentSubtraction.Left as BinaryExpressionSyntax;
            }
            return true;
        }

        private static bool IsTopLevelSubtraction(BinaryExpressionSyntax subtraction)
        {
            return !(subtraction.Parent is BinaryExpressionSyntax parent) || !parent.IsKind(SyntaxKind.SubtractExpression);
        }

        private static bool IsDelegateSubtraction(SyntaxNode node, SemanticModel semanticModel)
        {
            return semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol subtractMethod &&
                subtractMethod.ReceiverType.Is(TypeKind.Delegate);
        }

        private static bool ExpressionIsSimple(ExpressionSyntax expression)
        {
            var expressionWithoutparentheses = expression.RemoveParentheses();

            return expressionWithoutparentheses is IdentifierNameSyntax ||
                expressionWithoutparentheses is MemberAccessExpressionSyntax;
        }
    }
}
