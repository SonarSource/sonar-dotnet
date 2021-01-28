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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class InsecureEncryptionAlgorithm : InsecureEncryptionAlgorithmBase<SyntaxKind, InvocationExpressionSyntax, ObjectCreationExpressionSyntax>
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override SyntaxKind ObjectCreation => SyntaxKind.ObjectCreationExpression;
        protected override SyntaxKind Invocation => SyntaxKind.InvocationExpression;
        protected override SyntaxKind StringLiteral => SyntaxKind.StringLiteralExpression;
        protected override ILanguageFacade LanguageFacade => VisualBasicFacade.Instance;

        public InsecureEncryptionAlgorithm() : base(RspecStrings.ResourceManager) { }

        protected override SyntaxNode InvocationExpression(InvocationExpressionSyntax invocation) =>
            invocation.Expression;

        protected override Location Location(ObjectCreationExpressionSyntax objectCreation) =>
            objectCreation.Type.GetLocation();

        protected override bool IsInsecureBaseAlgorithmCreationFactoryCall(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocationExpression)
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

            if (argumentList.Arguments.Count > 1 || !argumentList.Arguments.First().GetExpression().IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }

            if (!AlgorithmParameterizedFactoryMethods.Contains(methodFullName))
            {
                return false;
            }

            var literalExpressionSyntax = (LiteralExpressionSyntax)argumentList.Arguments.First().GetExpression();
            return FactoryParameterNames.Any(alg => alg.Equals(literalExpressionSyntax.Token.ValueText, StringComparison.Ordinal));
        }
    }
}
