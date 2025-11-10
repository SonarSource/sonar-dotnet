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

namespace SonarAnalyzer.CSharp.Rules
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
            context.RegisterCodeBlockStartAction(
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
                                c.Model.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol)
                            {
                                c.ReportIssue(rule, thisExpression);
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
                                c.Model.GetSymbolInfo(assignment.Left).Symbol is IPropertySymbol)
                            {
                                c.ReportIssue(rule, right);
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
