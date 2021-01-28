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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(S2278DiagnosticId)]
    [Rule(DiagnosticId)]
    public sealed class InsecureEncryptionAlgorithm : InsecureEncryptionAlgorithmBase<SyntaxKind, InvocationExpressionSyntax, ObjectCreationExpressionSyntax>
    {
        // S2278 was deprecated in favor of S5547. Technically, there is no difference in the C# analyzer between
        // the 2 rules, but to be coherent with all the other languages, we still replace it with the new one
        private const string S2278DiagnosticId = "S2278";
        private const string S2278MessageFormat = "Use the recommended AES (Advanced Encryption Standard) instead.";

        private static readonly DiagnosticDescriptor S2278 = DiagnosticDescriptorBuilder.GetDescriptor(S2278DiagnosticId, S2278MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(S2278, Rule);

        protected override SyntaxKind ObjectCreation => SyntaxKind.ObjectCreationExpression;
        protected override SyntaxKind Invocation => SyntaxKind.InvocationExpression;
        protected override SyntaxKind StringLiteral => SyntaxKind.StringLiteralExpression;
        protected override ILanguageFacade LanguageFacade => CSharpFacade.Instance;

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
    }
}
