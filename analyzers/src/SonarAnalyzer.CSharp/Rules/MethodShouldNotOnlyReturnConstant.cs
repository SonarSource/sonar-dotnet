/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
    public sealed class MethodShouldNotOnlyReturnConstant : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3400";
        private const string MessageFormat = "Remove this method and declare a constant for this value.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    if (methodDeclaration.ParameterList?.Parameters.Count > 0
                        || IsVirtual(methodDeclaration))
                    {
                        return;
                    }

                    var expressionSyntax = GetSingleExpressionOrDefault(methodDeclaration);
                    if (!IsConstantExpression(expressionSyntax, c.Model))
                    {
                        return;
                    }

                    if (c.Model.GetDeclaredSymbol(methodDeclaration) is { } methodSymbol
                        && !methodSymbol.ContainingType.IsInterface()
                        && methodSymbol.InterfaceMembers().IsEmpty()
                        && methodSymbol.GetOverriddenMember() == null)
                    {
                        c.ReportIssue(Rule, methodDeclaration.Identifier);
                    }
                },
                SyntaxKind.MethodDeclaration);

        private static bool IsVirtual(BaseMethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));

        private static ExpressionSyntax GetSingleExpressionOrDefault(MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.ExpressionBody != null)
            {
                return methodDeclaration.ExpressionBody.Expression;
            }

            if (methodDeclaration.Body is { Statements: { Count: 1 } })
            {
                return (methodDeclaration.Body.Statements[0] as ReturnStatementSyntax)?.Expression;
            }

            return null;
        }

        private static bool IsConstantExpression(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression.RemoveParentheses() is LiteralExpressionSyntax noParenthesesExpression
            && !noParenthesesExpression.IsNullLiteral()
            && semanticModel.GetConstantValue(noParenthesesExpression).HasValue;
    }
}
