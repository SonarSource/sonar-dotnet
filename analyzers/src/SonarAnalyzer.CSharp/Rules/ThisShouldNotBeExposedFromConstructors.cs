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
    public sealed class ThisShouldNotBeExposedFromConstructors : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3366";
        private const string MessageFormat = "Make sure the use of 'this' doesn't expose partially-constructed instances of this class in multi-threaded environments.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCodeBlockStartAction<SyntaxKind>(
                cbc =>
                {
                    if (!IsInstanceConstructor(cbc.CodeBlock))
                    {
                        return;
                    }

                    cbc.RegisterNodeAction(
                        c =>
                        {
                            var invocation = (InvocationExpressionSyntax)c.Node;
                            if (invocation.ArgumentList == null)
                            {
                                return;
                            }

                            var thisExpression = invocation.ArgumentList.Arguments
                                .Select(a => a.Expression)
                                .FirstOrDefault(IsThisExpression);

                            if (thisExpression != null &&
                                !IsClassMember(invocation.Expression) &&
                                c.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol)
                            {
                                c.ReportIssue(CreateDiagnostic(rule, thisExpression.GetLocation()));
                            }
                        },
                        SyntaxKind.InvocationExpression);
                    cbc.RegisterNodeAction(
                        c =>
                        {
                            var assignment = (AssignmentExpressionSyntax)c.Node;

                            var right = assignment.Right.RemoveParentheses();

                            if (IsThisExpression(right) &&
                                !IsClassMember(assignment.Left) &&
                                c.SemanticModel.GetSymbolInfo(assignment.Left).Symbol is IPropertySymbol)
                            {
                                c.ReportIssue(CreateDiagnostic(rule, right.GetLocation()));
                            }
                        },
                        SyntaxKind.SimpleAssignmentExpression);
                });
        }

        private bool IsClassMember(ExpressionSyntax expression) =>
            expression is IdentifierNameSyntax ||
            (expression is MemberAccessExpressionSyntax memberAccess && IsThisExpression(memberAccess.Expression));

        private static bool IsThisExpression(ExpressionSyntax expression) =>
            expression != null &&
            expression.RemoveParentheses().IsKind(SyntaxKind.ThisExpression);

        private static bool IsInstanceConstructor(SyntaxNode node)
        {
            bool IsStaticKeyword(SyntaxToken token) => token.IsKind(SyntaxKind.StaticKeyword);

            return node is ConstructorDeclarationSyntax ctor
                && !ctor.Modifiers.Any(IsStaticKeyword);
        }
    }
}
