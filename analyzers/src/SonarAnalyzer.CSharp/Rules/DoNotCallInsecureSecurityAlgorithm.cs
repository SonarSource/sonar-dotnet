/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    public abstract class DoNotCallInsecureSecurityAlgorithm : DoNotCallInsecureSecurityAlgorithmBase<SyntaxKind, InvocationExpressionSyntax, ObjectCreationExpressionSyntax>
    {
        protected sealed override SyntaxKind ObjectCreation => SyntaxKind.ObjectCreationExpression;
        protected sealed override SyntaxKind Invocation => SyntaxKind.InvocationExpression;
        protected sealed override SyntaxKind StringLiteral => SyntaxKind.StringLiteralExpression;
        protected sealed override ILanguageFacade LanguageFacade => CSharpFacade.Instance;

        protected sealed override bool IsInsecureBaseAlgorithmCreationFactoryCall(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocationExpression)
        {
            var argumentList = invocationExpression.ArgumentList;

            if (argumentList == null || methodSymbol.ContainingType == null)
            {
                return false;
            }

            var methodFullName = $"{methodSymbol.ContainingType}.{methodSymbol.Name}";

            if (argumentList.Arguments.Count == 0)
            {
                return AlgorithmParameterlessFactoryMethods.Contains(methodFullName);
            }

            if (argumentList.Arguments.Count > 1 || !argumentList.Arguments.First().Expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }

            if (!AlgorithmParameterizedFactoryMethods.Contains(methodFullName))
            {
                return false;
            }

            var literalExpressionSyntax = (LiteralExpressionSyntax)argumentList.Arguments.First().Expression;
            return FactoryParameterNames.Any(alg => alg.Equals(literalExpressionSyntax.Token.ValueText, StringComparison.Ordinal));
        }

        protected sealed override SyntaxNode InvocationExpression(InvocationExpressionSyntax invocation) =>
            invocation.Expression;

        protected sealed override Location Location(ObjectCreationExpressionSyntax objectCreation) =>
            objectCreation.Type.GetLocation();
    }
}
