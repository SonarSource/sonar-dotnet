/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MethodShouldNotOnlyReturnConstant : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3400";
        private const string MessageFormat = "Remove this method and declare a constant for this value.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (c.IsTest())
                    {
                        return;
                    }

                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;

                    if (methodDeclaration.ParameterList?.Parameters.Count > 0)
                    {
                        return;
                    }

                    var expressionSyntax = GetSingleExpressionOrDefault(methodDeclaration);
                    if (!IsConstantExpression(expressionSyntax, c.SemanticModel))
                    {
                        return;
                    }

                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);

                    if (methodSymbol != null &&
                        methodSymbol.GetInterfaceMember() == null &&
                        methodSymbol.GetOverriddenMember() == null)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodDeclaration.Identifier.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);
        }

        private ExpressionSyntax GetSingleExpressionOrDefault(MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.ExpressionBody != null)
            {
                return methodDeclaration.ExpressionBody.Expression;
            }

            if (methodDeclaration.Body != null &&
                methodDeclaration.Body.Statements.Count == 1)
            {
                return (methodDeclaration.Body.Statements[0] as ReturnStatementSyntax)?.Expression;
            }

            return null;
        }

        private bool IsConstantExpression(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            if (expression == null)
            {
                return false;
            }

            var noParenthesesExpression = expression.RemoveParentheses();
            return noParenthesesExpression is LiteralExpressionSyntax &&
                !noParenthesesExpression.IsKind(SyntaxKind.NullLiteralExpression) &&
                semanticModel.GetConstantValue(noParenthesesExpression).HasValue;
        }
    }
}
